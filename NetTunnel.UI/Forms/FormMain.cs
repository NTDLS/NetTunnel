using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private NtClient? _client;

        volatile int _gridPopulationScope = 0;

        private System.Windows.Forms.Timer? _timer;

        #region Constructor / Deconstructor.

        public FormMain()
        {
            InitializeComponent();
        }

        public new void Dispose()
        {
            _client?.Dispose();
            base.Dispose();
        }

        #endregion

        private void FormMain_Load(object sender, EventArgs e)
        {
            Shown += (object? sender, EventArgs e) =>
            {
                if (!ChangeConnection()) Close();
            };

            _timer = new System.Windows.Forms.Timer()
            {
                Enabled = true,
                Interval = 1000
            };

            _timer.Tick += _timer_Tick;
            _timer.Start();

            listViewTunnels.MouseUp += ListViewTunnels_MouseUp;
            listViewTunnels.SelectedIndexChanged += ListViewTunnels_SelectedIndexChanged;
        }

        bool _inTimerTick = false;

        private void _timer_Tick(object? sender, EventArgs e)
        {
            if (_gridPopulationScope != 0 || _inTimerTick)
            {
                return;
            }

            try
            {
                _inTimerTick = true;

                if (_client != null && _client.IsConnected)
                {
                    _client.GetStatistics().ContinueWith(o =>
                    {
                        if (o.Result.Success)
                        {
                            PopulateEndpointStatistics(o.Result.Statistics);
                            PopulateTunnelStatistics(o.Result.Statistics);
                        }
                    });
                }

                void PopulateEndpointStatistics(List<NtTunnelStatistics> statistics)
                {
                    if (listViewEndpoints.InvokeRequired)
                    {
                        listViewEndpoints.Invoke(PopulateEndpointStatistics, statistics);
                    }
                    else
                    {
                        foreach (ListViewItem item in listViewEndpoints.Items)
                        {
                            var endpoint = (INtEndpointConfiguration)item.Tag;
                            var direction = (endpoint is NtEndpointInboundConfiguration) ? NtDirection.Inbound : NtDirection.Outbound;

                            var tunnelStats = statistics.Where(o => o.TunnelPairId == endpoint.TunnelPairId && o.Direction == direction).SingleOrDefault();
                            if (tunnelStats != null)
                            {
                                var endpointStats = tunnelStats.EndpointStatistics.Where(o => o.EndpointPairId == endpoint.PairId && o.Direction == direction).SingleOrDefault();
                                if (endpointStats != null)
                                {
                                    item.SubItems[columnHeaderEndpointBytesSent.Index].Text = $"{endpointStats.BytesSentKb:n0}";
                                    item.SubItems[columnHeaderEndpointBytesReceived.Index].Text = $"{endpointStats.BytesReceivedKb:n0}";
                                    item.SubItems[columnHeaderEndpointTotalConnections.Index].Text = $"{endpointStats.TotalConnections:n0}";
                                    item.SubItems[columnHeaderEndpointCurrentConenctions.Index].Text = $"{endpointStats.CurrentConnections:n0}";
                                }
                            }
                        }
                    }
                }

                void PopulateTunnelStatistics(List<NtTunnelStatistics> statistics)
                {
                    if (listViewTunnels.InvokeRequired)
                    {
                        listViewTunnels.Invoke(PopulateTunnelStatistics, statistics);
                    }
                    else
                    {
                        foreach (ListViewItem item in listViewTunnels.Items)
                        {
                            var tunnel = (INtTunnelConfiguration)item.Tag;
                            var direction = (tunnel is NtTunnelInboundConfiguration) ? NtDirection.Inbound : NtDirection.Outbound;

                            var tunnelStats = statistics.Where(o => o.TunnelPairId == tunnel.PairId && o.Direction == direction).SingleOrDefault();
                            if (tunnelStats != null)
                            {
                                item.SubItems[columnHeaderTunnelBytesSent.Index].Text = $"{tunnelStats.BytesSentKb:n0}";
                                item.SubItems[columnHeaderTunnelBytesReceived.Index].Text = $"{tunnelStats.BytesReceivedKb:n0}";
                                item.SubItems[columnHeaderTunnelTotalConnections.Index].Text = $"{tunnelStats.TotalConnections:n0}";
                                item.SubItems[columnHeaderTunnelCurrentConenctions.Index].Text = $"{tunnelStats.CurrentConnections:n0}";
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _inTimerTick = false;
            }
        }

        private void ListViewTunnels_SelectedIndexChanged(object? sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

            if (listViewTunnels.SelectedItems.Count == 1)
            {
                var selectedRow = listViewTunnels.SelectedItems[0];

                if (selectedRow.Tag is INtTunnelConfiguration tunnel)
                {
                    lock (listViewEndpoints)
                    {
                        RepopulateEndpointsGrid(tunnel);
                    }
                }
            }
        }

        private void ListViewTunnels_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var itemUnderMouse = listViewTunnels.GetItemAt(e.X, e.Y);
                if (itemUnderMouse != null)
                {
                    itemUnderMouse.Selected = true;
                }

                var menu = new ContextMenuStrip();

                menu.Items.Add("Create Outbound Tunnel");

                if (itemUnderMouse != null)
                {
                    menu.Items.Add("Add Endpoint to Tunnel");
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Delete Tunnel");
                }

                menu.Show(listViewTunnels, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    if (e.ClickedItem?.Text == "Create Outbound Tunnel")
                    {
                        Utility.EnsureNotNull(_client);

                        using (var formCreateOutboundTunnel = new FormCreateOutboundTunnel(_client))
                        {
                            if (formCreateOutboundTunnel.ShowDialog() == DialogResult.OK)
                            {
                                RepopulateTunnelsGrid();
                            }
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Endpoint to Tunnel")
                    {
                        Utility.EnsureNotNull(_client);
                        Utility.EnsureNotNull(itemUnderMouse);

                        if (itemUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            using (var formAddEndpoint = new FormAddEndpoint(_client, tunnelInbound))
                            {
                                if (formAddEndpoint.ShowDialog() == DialogResult.OK)
                                {
                                    RepopulateTunnelsGrid();
                                }
                            }
                        }
                        else if (itemUnderMouse.Tag is NtTunnelOutboundConfiguration tunnelOutbound)
                        {
                            using (var formAddEndpoint = new FormAddEndpoint(_client, tunnelOutbound))
                            {
                                if (formAddEndpoint.ShowDialog() == DialogResult.OK)
                                {
                                    RepopulateTunnelsGrid();
                                }
                            }
                        }
                    }
                    else if (e.ClickedItem?.Text == "Delete Tunnel")
                    {
                        Utility.EnsureNotNull(_client);
                        Utility.EnsureNotNull(itemUnderMouse);

                        if (itemUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            _client.TunnelInbound.Delete(tunnelInbound.PairId).ContinueWith((o) =>
                            {
                                DeleteItemFromGrid(listViewTunnels, itemUnderMouse);
                                ClearGridItems(listViewEndpoints);
                            });
                        }
                        else if (itemUnderMouse.Tag is NtTunnelOutboundConfiguration tunneloutbound)
                        {
                            _client.TunnelInbound.Delete(tunneloutbound.PairId).ContinueWith((o) =>
                            {
                                DeleteItemFromGrid(listViewTunnels, itemUnderMouse);
                                ClearGridItems(listViewEndpoints);
                            });
                        }
                    }
                };
            }
        }

        private bool ChangeConnection()
        {
            try
            {
                using (var formLogin = new FormLogin())
                {
                    if (formLogin.ShowDialog() == DialogResult.OK)
                    {
                        _client = new NtClient(formLogin.Address, formLogin.Username, formLogin.Password);
                        RepopulateTunnelsGrid();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
            return false;
        }

        void DeleteItemFromGrid(ListView grid, ListViewItem item)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke(DeleteItemFromGrid, new object[] { grid, item });
            }
            else
            {
                grid.Items.Remove(item);
            }
        }

        void ClearGridItems(ListView grid)
        {
            if (grid.InvokeRequired)
            {
                grid.Invoke(ClearGridItems, grid);
            }
            else
            {
                grid.Items.Clear();
            }
        }

        #region Populate Grids.

        private void RepopulateTunnelsGrid()
        {
            try
            {
                _timer?.Stop();
                _gridPopulationScope++;
                RepopulateTunnelsGrid_LockRequired();
            }
            finally
            {
                _gridPopulationScope--;

                if (_gridPopulationScope == 0)
                {
                    _timer?.Start();
                }
            }
        }

        private void RepopulateTunnelsGrid_LockRequired()
        {
            Utility.EnsureNotNull(_client);

            listViewTunnels.Items.Clear();

            _client.TunnelInbound.List().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddInboundTunnelToGrid(t));
            });

            _client.TunnelOutbound.List().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddOutboundTunnelToGrid(t));
            });

            void AddInboundTunnelToGrid(NtTunnelInboundConfiguration tunnel)
            {
                if (listViewTunnels.InvokeRequired)
                {
                    listViewTunnels.Invoke(AddInboundTunnelToGrid, tunnel);
                }
                else
                {
                    int endpointCount = tunnel.EndpointOutboundConfigurations.Count
                        + tunnel.EndpointInboundConfigurations.Count;

                    var item = new ListViewItem(tunnel.Name);
                    item.Tag = tunnel;
                    item.SubItems.Add("Inbound");
                    item.SubItems.Add($"*:{tunnel.DataPort}");
                    item.SubItems.Add($"{endpointCount:n0}");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    listViewTunnels.Items.Add(item);
                }
            }

            void AddOutboundTunnelToGrid(NtTunnelOutboundConfiguration tunnel)
            {
                if (listViewTunnels.InvokeRequired)
                {
                    listViewTunnels.Invoke(AddOutboundTunnelToGrid, tunnel);
                }
                else
                {
                    int endpointCount = tunnel.EndpointOutboundConfigurations.Count
                        + tunnel.EndpointInboundConfigurations.Count;

                    var item = new ListViewItem(tunnel.Name);
                    item.Tag = tunnel;
                    item.SubItems.Add("Outbound");
                    item.SubItems.Add($"{tunnel.Address}{tunnel.DataPort}");
                    item.SubItems.Add($"{endpointCount:n0}");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    listViewTunnels.Items.Add(item);
                }
            }
        }

        private void RepopulateEndpointsGrid(INtTunnelConfiguration tunnel)
        {
            try
            {
                _timer?.Stop();
                _gridPopulationScope++;
                RepopulateEndpointsGrid_LockRequired(tunnel);
            }
            finally
            {
                _gridPopulationScope--;

                if (_gridPopulationScope == 0)
                {
                    _timer?.Start();
                }
            }
        }

        private void RepopulateEndpointsGrid_LockRequired(INtTunnelConfiguration tunnelInbound)
        {
            Utility.EnsureNotNull(_client);
            listViewEndpoints.Items.Clear();

            tunnelInbound.EndpointInboundConfigurations.ForEach(x => AddEndpointInboundToGrid(x));
            tunnelInbound.EndpointOutboundConfigurations.ForEach(x => AddEndpointOutboundToGrid(x));

            void AddEndpointInboundToGrid(NtEndpointInboundConfiguration endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddEndpointInboundToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.Tag = endpoint;
                    item.SubItems.Add("Inbound");
                    item.SubItems.Add($"*:{endpoint.TransmissionPort}");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    listViewEndpoints.Items.Add(item);
                }
            }

            void AddEndpointOutboundToGrid(NtEndpointOutboundConfiguration endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddEndpointOutboundToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.Tag = endpoint;
                    item.SubItems.Add("Outbound");
                    item.SubItems.Add($"{endpoint.Address}{endpoint.TransmissionPort}");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    item.SubItems.Add("~");
                    listViewEndpoints.Items.Add(item);
                }
            }
        }

        #endregion

        #region Body menu click.

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ChangeConnection()) Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);
            using (var formUsers = new FormUsers(_client))
            {
                formUsers.ShowDialog();
            }
        }

        #endregion
    }
}
