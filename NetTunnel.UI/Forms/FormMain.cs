using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.UI.Helpers;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private int _allTunnelAndEndpointHashes = -1;
        private bool _needToRepopulateTunnels = false;
        private NtClient? OLDDELETEMECLIENT_client;

        private ClientWrapper? _client;

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
            OLDDELETEMECLIENT_client?.Dispose();
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

            _tunnelsListViewItemComparer = new ListViewItemComparer();
            listViewTunnels.ListViewItemSorter = _tunnelsListViewItemComparer;
            listViewTunnels.ColumnClick += ListViewTunnels_ColumnClick;
            listViewTunnels.MouseUp += ListViewTunnels_MouseUp;
            listViewTunnels.SelectedIndexChanged += ListViewTunnels_SelectedIndexChanged;

            _endpointsListViewItemComparer = new ListViewItemComparer();
            listViewEndpoints.ListViewItemSorter = _endpointsListViewItemComparer;
            listViewEndpoints.ColumnClick += ListViewEndpoints_ColumnClick;
            listViewEndpoints.MouseUp += listViewEndpoints_MouseUp;
            listViewEndpoints.MouseDoubleClick += ListViewEndpoints_MouseDoubleClick;
        }

        private void ListViewEndpoints_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            OLDDELETEMECLIENT_client.EnsureNotNull();

            var selectedTunnelRow = listViewTunnels.SelectedItems?.Count > 0 ? listViewTunnels.SelectedItems[0] : null;
            if (selectedTunnelRow == null)
            {
                return;
            }

            var selectedTunnel = (selectedTunnelRow.Tag as INtTunnelConfiguration).EnsureNotNull();

            var selectedEndpointRow = listViewEndpoints.GetItemAt(e.X, e.Y);
            if (selectedEndpointRow != null)
            {
                var selectedEndpoint = (selectedEndpointRow.Tag as INtEndpointConfiguration).EnsureNotNull();

                using var form = new FormAddEditEndpoint(OLDDELETEMECLIENT_client, selectedTunnel, selectedEndpoint);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RepopulateTunnelsGrid();
                }
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
                        //Text = $"{Constants.FriendlyName} : {formLogin.Address} {(formLogin.UseSSL ? "" : " : [INSECURE]")}";
                        _client = formLogin.ResultingClient;
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
            _timer.EnsureNotNull();

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
                if (OLDDELETEMECLIENT_client != null && OLDDELETEMECLIENT_client.IsConnected)
                {
                    OLDDELETEMECLIENT_client.Service.GetStatistics().ContinueWith(o =>
                    {
                        if (o.Result.Success)
                        {
                            int allTunnelAndEndpointHashes = o.Result.AllTunnelIdAndEndpointIdHashes();

                            if (allTunnelAndEndpointHashes != _allTunnelAndEndpointHashes && _allTunnelAndEndpointHashes != -1)
                            {
                                _needToRepopulateTunnels = true;
                            }
                            _allTunnelAndEndpointHashes = allTunnelAndEndpointHashes;

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
                        listViewEndpoints.BeginUpdate();

                        foreach (ListViewItem item in listViewEndpoints.Items)
                        {
                            var endpoint = ((INtEndpointConfiguration?)item.Tag).EnsureNotNull();

                            var direction = (endpoint is NtEndpointInboundConfiguration) ? NtDirection.Inbound : NtDirection.Outbound;

                            var tunnelStats = statistics.Where(o => o.TunnelId == endpoint.TunnelId).ToList();
                            if (tunnelStats != null)
                            {
                                var endpointStats = tunnelStats.SelectMany(o => o.EndpointStatistics)
                                    .Where(o => o.EndpointId == endpoint.EndpointId && o.Direction == direction).SingleOrDefault();
                                if (endpointStats != null)
                                {
                                    double compressionRatio = 0;
                                    if (endpointStats.BytesSentKb > 0 && endpointStats.BytesReceivedKb > 0)
                                    {
                                        if (endpointStats.BytesSentKb > endpointStats.BytesReceivedKb)
                                        {
                                            compressionRatio = 100 - (endpointStats.BytesReceivedKb / endpointStats.BytesSentKb) * 100.0;
                                        }
                                        else
                                        {
                                            compressionRatio = 100 - (endpointStats.BytesSentKb / endpointStats.BytesReceivedKb) * 100.0;
                                        }
                                    }

                                    item.SubItems[columnHeaderEndpointBytesSent.Index].Text = $"{endpointStats.BytesSentKb:n0}";
                                    item.SubItems[columnHeaderEndpointBytesReceived.Index].Text = $"{endpointStats.BytesReceivedKb:n0}";
                                    item.SubItems[columnHeaderEndpointTotalConnections.Index].Text = $"{endpointStats.TotalConnections:n0}";
                                    item.SubItems[columnHeaderEndpointCurrentConenctions.Index].Text = $"{endpointStats.CurrentConnections:n0}";
                                    item.SubItems[columnHeaderCompressionRatio.Index].Text = $"{compressionRatio:n2}";
                                }
                            }
                        }

                        listViewEndpoints.EndUpdate();
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
                        listViewTunnels.BeginUpdate();

                        foreach (ListViewItem item in listViewTunnels.Items)
                        {
                            var tunnel = ((INtTunnelConfiguration?)item.Tag).EnsureNotNull();

                            var direction = (tunnel is NtTunnelInboundConfiguration) ? NtDirection.Inbound : NtDirection.Outbound;

                            var tunnelStats = statistics.Where(o => o.TunnelId == tunnel.TunnelId && o.Direction == direction).SingleOrDefault();
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
                                    case NtTunnelStatus.Stopped:
                                        item.BackColor = Color.FromArgb(255, 200, 0);
                                        break;
                                    case NtTunnelStatus.Established:
                                        item.BackColor = Color.FromArgb(200, 255, 200);
                                        break;
                                    default:
                                        item.BackColor = Color.FromArgb(255, 255, 255);
                                        break;
                                }
                            }
                        }

                        listViewTunnels.EndUpdate();
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
            OLDDELETEMECLIENT_client.EnsureNotNull();

            labelEndpoints.Text = "Endpoints of ";

            if (listViewTunnels.SelectedItems.Count == 1)
            {
                var selectedRow = listViewTunnels.SelectedItems[0];

                if (selectedRow.Tag is INtTunnelConfiguration tunnel)
                {
                    if (tunnel is NtTunnelInboundConfiguration)
                    {
                        labelEndpoints.Text += "inbound tunnel ";
                    }
                    else if (tunnel is NtTunnelOutboundConfiguration)
                    {
                        labelEndpoints.Text += "outbound tunnel ";
                    }

                    labelEndpoints.Text += $"'{tunnel.Name}'";

                    lock (listViewEndpoints)
                    {
                        RepopulateEndpointsGrid(tunnel);
                    }
                }
            }
            else
            {
                labelEndpoints.Text += " (select a tunnel above)";
            }
        }

        private void listViewEndpoints_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedTunnelRow = listViewTunnels.SelectedItems?.Count > 0 ? listViewTunnels.SelectedItems[0] : null;
                if (selectedTunnelRow == null)
                {
                    return;
                }

                var selectedTunnel = (selectedTunnelRow.Tag as INtTunnelConfiguration).EnsureNotNull();

                var selectedEndpointRow = listViewEndpoints.GetItemAt(e.X, e.Y);
                if (selectedEndpointRow != null)
                {
                    selectedEndpointRow.Selected = true;
                }

                var menu = new ContextMenuStrip();

                menu.Items.Add("Add Inbound Endpoint to Tunnel");
                menu.Items.Add("Add Outbound Endpoint to Tunnel");

                if (selectedEndpointRow != null)
                {
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Delete Endpoint");
                }

                menu.Show(listViewEndpoints, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (e.ClickedItem?.Text == "Add Inbound Endpoint to Tunnel")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();

                        using var form = new FormAddEditEndpoint(OLDDELETEMECLIENT_client, selectedTunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Outbound Endpoint to Tunnel")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();

                        using var form = new FormAddEditEndpoint(OLDDELETEMECLIENT_client, selectedTunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Delete Endpoint")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();
                        var selectedEndpoint = (selectedEndpointRow?.Tag as INtEndpointConfiguration).EnsureNotNull();

                        if (MessageBox.Show($"Delete the endpoint '{selectedEndpoint.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (selectedTunnel is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelInbound.DeleteEndpointPair(tunnelInbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote endpoint, would you like to delete the local one anyway?",
                                        FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local endpoint.
                                            OLDDELETEMECLIENT_client.TunnelInbound.DeleteEndpoint(tunnelInbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });

                        }
                        else if (selectedTunnel is NtTunnelOutboundConfiguration tunnelOutbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelOutbound.DeleteEndpointPair(tunnelOutbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote endpoint, would you like to delete the local one anyway?",
                                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local endpoint.
                                            OLDDELETEMECLIENT_client.TunnelOutbound.DeleteEndpoint(tunnelOutbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });
                        }

                        listViewEndpoints.InvokeClearListViewRows();
                    }
                };
            }
        }

        private void ListViewTunnels_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var rowUnderMouse = listViewTunnels.GetItemAt(e.X, e.Y);
                if (rowUnderMouse != null)
                {
                    rowUnderMouse.Selected = true;
                }

                var selectedTunnel = ((INtTunnelConfiguration?)rowUnderMouse?.Tag);

                var menu = new ContextMenuStrip();

                menu.Items.Add("Create Inbound Tunnel");
                menu.Items.Add("Create Outbound Tunnel");

                if (rowUnderMouse != null && selectedTunnel != null)
                {
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Add Inbound Endpoint to Tunnel");
                    menu.Items.Add("Add Outbound Endpoint to Tunnel");

                    menu.Items.Add(new ToolStripSeparator());

                    if (rowUnderMouse.SubItems[columnHeaderTunnelStatus.Index].Text == "Stopped")
                    {
                        menu.Items.Add("Start");
                    }
                    else
                    {
                        menu.Items.Add("Stop");
                    }

                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Delete Tunnel");
                }

                menu.Show(listViewTunnels, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (e.ClickedItem?.Text == "Create Outbound Tunnel")
                    {
                        using var form = new FormCreateOutboundTunnel(_client.EnsureNotNull());
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Create Inbound Tunnel")
                    {
                        using var form = new FormCreateInboundTunnel(_client.EnsureNotNull());
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Inbound Endpoint to Tunnel")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((INtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        using var form = new FormAddEditEndpoint(OLDDELETEMECLIENT_client, tunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Outbound Endpoint to Tunnel")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((INtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        using var form = new FormAddEditEndpoint(OLDDELETEMECLIENT_client, tunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Stop")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((INtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        if (MessageBox.Show($"Stop the tunnel '{tunnel.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (rowUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelInbound.Stop(tunnelInbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.InvokeMessageBox($"Failed to stop the tunnel.",
                                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                    _needToRepopulateTunnels = true;
                                }
                            });
                        }
                        else if (rowUnderMouse.Tag is NtTunnelOutboundConfiguration tunnelOutbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelOutbound.Stop(tunnelOutbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.InvokeMessageBox($"Failed to stop the tunnel.",
                                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                    _needToRepopulateTunnels = true;
                                }
                            });
                        }

                        listViewEndpoints.InvokeClearListViewRows();
                    }
                    // Stop ↑
                    else if (e.ClickedItem?.Text == "Start")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((INtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        if (rowUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelInbound.Start(tunnelInbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.InvokeMessageBox($"Failed to start the tunnel.",
                                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                    _needToRepopulateTunnels = true;
                                }
                            });
                        }
                        else if (rowUnderMouse.Tag is NtTunnelOutboundConfiguration tunnelOutbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelOutbound.Start(tunnelOutbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.InvokeMessageBox($"Failed to start the tunnel.",
                                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                    _needToRepopulateTunnels = true;
                                }
                            });
                        }

                        listViewEndpoints.InvokeClearListViewRows();
                    }
                    //Start ↑
                    else if (e.ClickedItem?.Text == "Delete Tunnel")
                    {
                        OLDDELETEMECLIENT_client.EnsureNotNull();
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((INtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        if (MessageBox.Show($"Delete the tunnel '{tunnel.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (rowUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelInbound.DeletePair(tunnelInbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote tunnel, would you like to delete the local one anyway?",
                                        FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local tunnel.
                                            OLDDELETEMECLIENT_client.TunnelInbound.Delete(tunnelInbound.TunnelId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });

                        }
                        else if (rowUnderMouse.Tag is NtTunnelOutboundConfiguration tunnelOutbound)
                        {
                            OLDDELETEMECLIENT_client.TunnelOutbound.DeletePair(tunnelOutbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote tunnel, would you like to delete the local one anyway?",
                                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local tunnel.
                                            OLDDELETEMECLIENT_client.TunnelOutbound.Delete(tunnelOutbound.TunnelId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });
                        }

                        listViewEndpoints.InvokeClearListViewRows();
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
            _client.EnsureNotNull();

            listViewTunnels.Items.Clear();
            listViewEndpoints.Items.Clear();

            _client.GetInboundTunnels().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddInboundTunnelToGrid(t));
            });

            _client.GetOutboundTunnels().ContinueWith(t =>
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
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
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
                    item.SubItems.Add($"{tunnel.Address}:{tunnel.DataPort}");
                    item.SubItems.Add($"{endpointCount:n0}");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
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
            OLDDELETEMECLIENT_client.EnsureNotNull();
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
                    item.SubItems.Add($"*:{endpoint.InboundPort}");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
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
                    item.SubItems.Add($"{endpoint.OutboundAddress}:{endpoint.OutboundPort}");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    listViewEndpoints.Items.Add(item);
                }
            }
        }

        #endregion

        #region Grid Sorting.

        private void ListViewTunnels_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            sender.EnsureNotNull();
            _tunnelsListViewItemComparer.EnsureNotNull();

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
            sender.EnsureNotNull();
            _endpointsListViewItemComparer.EnsureNotNull();

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
            using var form = new FormUsers(OLDDELETEMECLIENT_client.EnsureNotNull());
            form.ShowDialog();
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new FormServiceConfiguration(OLDDELETEMECLIENT_client.EnsureNotNull());
            form.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new FormAbout();
            form.ShowDialog();
        }

        #endregion
    }
}
