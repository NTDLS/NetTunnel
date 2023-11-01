using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.UI.Helpers;
using System.DirectoryServices;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private int _allPairIdHashs = -1;
        private bool _needToRepopulateTunnels = false;
        private NtClient? _client;
        private bool _inTimerTick = false;
        private volatile int _gridPopulationScope = 0;
        private System.Windows.Forms.Timer? _timer;
        private ListViewItemComparer? _endpointsListViewItemComparer;
        private ListViewItemComparer? _tunnelsListViewItemComparer;

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
                Interval = 250
            };

            _timer.Tick += _timer_Tick;
            _timer.Start();

            listViewTunnels.MouseUp += ListViewTunnels_MouseUp;
            listViewTunnels.SelectedIndexChanged += ListViewTunnels_SelectedIndexChanged;

            _endpointsListViewItemComparer = new ListViewItemComparer();
            listViewEndpoints.ListViewItemSorter = _endpointsListViewItemComparer;
            listViewEndpoints.ColumnClick += ListViewEndpoints_ColumnClick;

            _tunnelsListViewItemComparer = new ListViewItemComparer();
            listViewTunnels.ListViewItemSorter = _tunnelsListViewItemComparer;
            listViewTunnels.ColumnClick += ListViewTunnels_ColumnClick;
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

        private void _timer_Tick(object? sender, EventArgs e)
        {
            Utility.EnsureNotNull(_timer);

            if (_needToRepopulateTunnels)
            {
                _needToRepopulateTunnels = false;
                RepopulateTunnelsGrid();
                return;
            }

            lock (_timer)
            {
                if (_gridPopulationScope != 0 || _inTimerTick)
                {
                    return;
                }
                _inTimerTick = true;
            }

            try
            {
                if (_client != null && _client.IsConnected)
                {
                    _client.GetStatistics().ContinueWith(o =>
                    {
                        if (o.Result.Success)
                        {
                            int allPairIdHashs = o.Result.AllPairIdHashs();

                            if (allPairIdHashs != _allPairIdHashs && _allPairIdHashs != -1)
                            {
                                _needToRepopulateTunnels = true;
                            }
                            _allPairIdHashs = allPairIdHashs;

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

                            var tunnelStats = statistics.Where(o => o.TunnelPairId == endpoint.TunnelPairId).ToList();
                            if (tunnelStats != null)
                            {
                                var endpointStats = tunnelStats.SelectMany(o => o.EndpointStatistics)
                                    .Where(o => o.EndpointPairId == endpoint.PairId && o.Direction == direction).SingleOrDefault();
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
                                item.SubItems[columnHeaderTunnelStatus.Index].Text = tunnelStats.Status.ToString();

                                switch (tunnelStats.Status)
                                {
                                    case NtTunnelStatus.Connecting:
                                    case NtTunnelStatus.Disconnected:
                                        item.BackColor = Color.FromArgb(255, 200, 200);
                                        break;
                                    case NtTunnelStatus.Established:
                                        item.BackColor = Color.FromArgb(200, 255, 200);
                                        break;
                                }
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
                    menu.Items.Add("Add Inbound Endpoint to Tunnel");
                    menu.Items.Add("Add Outbound Endpoint to Tunnel");
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Delete Tunnel");
                }

                menu.Show(listViewTunnels, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

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
                    else if (e.ClickedItem?.Text == "Add Inbound Endpoint to Tunnel")
                    {
                        Utility.EnsureNotNull(_client);
                        Utility.EnsureNotNull(itemUnderMouse);

                        using (var formAddEndpoint = new FormAddEndpoint(_client, (INtTunnelConfiguration)itemUnderMouse.Tag, NtDirection.Inbound))
                        {
                            if (formAddEndpoint.ShowDialog() == DialogResult.OK)
                            {
                                RepopulateTunnelsGrid();
                            }
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Outbound Endpoint to Tunnel")
                    {
                        Utility.EnsureNotNull(_client);
                        Utility.EnsureNotNull(itemUnderMouse);

                        using (var formAddEndpoint = new FormAddEndpoint(_client, (INtTunnelConfiguration)itemUnderMouse.Tag, NtDirection.Outbound))
                        {
                            if (formAddEndpoint.ShowDialog() == DialogResult.OK)
                            {
                                RepopulateTunnelsGrid();
                            }
                        }
                    }
                    else if (e.ClickedItem?.Text == "Delete Tunnel")
                    {
                        Utility.EnsureNotNull(_client);
                        Utility.EnsureNotNull(itemUnderMouse);

                        var tunnel = (INtTunnelConfiguration)itemUnderMouse.Tag;

                        if (MessageBox.Show($"Delete the tunnel '{tunnel.Name}'?",
                            Constants.FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (itemUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            _client.TunnelInbound.DeletePair(tunnelInbound.PairId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote tunnel, would you like to delete the local one anyway?",
                                        Constants.FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local tunnel.
                                            _client.TunnelInbound.Delete(tunnelInbound.PairId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });

                        }
                        else if (itemUnderMouse.Tag is NtTunnelOutboundConfiguration tunneloutbound)
                        {
                            _client.TunnelOutbound.DeletePair(tunneloutbound.PairId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote tunnel, would you like to delete the local one anyway?",
                                            Constants.FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local tunnel.
                                            _client.TunnelOutbound.Delete(tunneloutbound.PairId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });
                        }

                        listViewEndpoints.InvokeClearRows();
                    }
                };
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

        #region Grid Sorting.

        private void ListViewTunnels_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            Utility.EnsureNotNull(sender);
            Utility.EnsureNotNull(_tunnelsListViewItemComparer);

            var listView = (ListView)sender;

            if (e.Column == _tunnelsListViewItemComparer.SortColumn)
            {
                if (_tunnelsListViewItemComparer.SortOrder == SortOrder.Ascending)
                    _tunnelsListViewItemComparer.SortOrder = SortOrder.Descending;
                else
                    _tunnelsListViewItemComparer.SortOrder = SortOrder.Ascending;
            }
            else
            {
                _tunnelsListViewItemComparer.SortColumn = e.Column;
                _tunnelsListViewItemComparer.SortOrder = SortOrder.Ascending;
            }

            listView.Sort();
        }

        private void ListViewEndpoints_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            Utility.EnsureNotNull(sender);
            Utility.EnsureNotNull(_endpointsListViewItemComparer);

            var listView = (ListView)sender;

            if (e.Column == _endpointsListViewItemComparer.SortColumn)
            {
                if (_endpointsListViewItemComparer.SortOrder == SortOrder.Ascending)
                    _endpointsListViewItemComparer.SortOrder = SortOrder.Descending;
                else
                    _endpointsListViewItemComparer.SortOrder = SortOrder.Ascending;
            }
            else
            {
                _endpointsListViewItemComparer.SortColumn = e.Column;
                _endpointsListViewItemComparer.SortOrder = SortOrder.Ascending;
            }

            listView.Sort();
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
