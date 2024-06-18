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

        private Library.NtServiceClient? _client;

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
            var selectedTunnelRow = listViewTunnels.SelectedItems?.Count > 0 ? listViewTunnels.SelectedItems[0] : null;
            if (selectedTunnelRow == null)
            {
                return;
            }

            var selectedTunnel = (selectedTunnelRow.Tag as NtTunnelConfiguration).EnsureNotNull();

            var selectedEndpointRow = listViewEndpoints.GetItemAt(e.X, e.Y);
            if (selectedEndpointRow != null)
            {
                var selectedEndpoint = (selectedEndpointRow.Tag as NtEndpointConfiguration).EnsureNotNull();

                using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), selectedTunnel, selectedEndpoint);
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
                /*
                if (_client != null && _client.IsConnected)
                {
                    _client.GetStatistics().ContinueWith(o =>
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
                */




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
                            var epTag = ((EndpointTag?)item.Tag).EnsureNotNull();

                            var tunnelStats = statistics.Where(o => o.TunnelId == epTag.Tunnel.TunnelId).ToList();
                            if (tunnelStats != null)
                            {
                                var endpointStats = tunnelStats.SelectMany(o => o.EndpointStatistics)
                                    .Where(o => o.EndpointId == epTag.Endpoint.EndpointId && o.Direction == epTag.Endpoint.Direction).SingleOrDefault();
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
                            var tTag = ((TunnelTag?)item.Tag).EnsureNotNull();

                            var tunnelStats = statistics.Where(o => o.TunnelId == tTag.Tunnel.TunnelId).SingleOrDefault();
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
            _client.EnsureNotNull();

            labelEndpoints.Text = "Endpoints of ";

            if (listViewTunnels.SelectedItems.Count == 1)
            {
                var selectedRow = listViewTunnels.SelectedItems[0];

                if (selectedRow.Tag is NtTunnelConfiguration tunnel)
                {
                    if (tunnel is NtTunnelConfiguration)
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

                var selectedTunnel = (selectedTunnelRow.Tag as NtTunnelConfiguration).EnsureNotNull();

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
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), selectedTunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Outbound Endpoint to Tunnel")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), selectedTunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Delete Endpoint")
                    {
                        /*
                        var selectedEndpoint = (selectedEndpointRow?.Tag as INtEndpointConfiguration).EnsureNotNull();

                        if (MessageBox.Show($"Delete the endpoint '{selectedEndpoint.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (selectedTunnel is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            _client.TunnelInbound.DeleteEndpointPair(tunnelInbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote endpoint, would you like to delete the local one anyway?",
                                        FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local endpoint.
                                            _client.TunnelInbound.DeleteEndpoint(tunnelInbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
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
                            _client.TunnelOutbound.DeleteEndpointPair(tunnelOutbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote endpoint, would you like to delete the local one anyway?",
                                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local endpoint.
                                            _client.TunnelOutbound.DeleteEndpoint(tunnelOutbound.TunnelId, selectedEndpoint.EndpointId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });

                        }

                    */
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

                var selectedTunnel = ((NtTunnelConfiguration?)rowUnderMouse?.Tag);

                var menu = new ContextMenuStrip();

                menu.Items.Add("Connect Tunnel");

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

                    if (e.ClickedItem?.Text == "Connect Tunnel")
                    {
                        using var form = new FormConnectTunnel(_client.EnsureNotNull());
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Inbound Endpoint to Tunnel")
                    {
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((NtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Outbound Endpoint to Tunnel")
                    {
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((NtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Stop")
                    {
                        /*
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((NtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        if (MessageBox.Show($"Stop the tunnel '{tunnel.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (rowUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            _client.TunnelInbound.Stop(tunnelInbound.TunnelId).ContinueWith((o) =>
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
                            _client.TunnelOutbound.Stop(tunnelOutbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.InvokeMessageBox($"Failed to stop the tunnel.",
                                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                    _needToRepopulateTunnels = true;
                                }
                            });
                        }
                        */
                        listViewEndpoints.InvokeClearListViewRows();
                    }
                    // Stop ↑
                    else if (e.ClickedItem?.Text == "Start")
                    {
                        /*
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((NtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        if (rowUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            _client.TunnelInbound.Start(tunnelInbound.TunnelId).ContinueWith((o) =>
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
                            _client.TunnelOutbound.Start(tunnelOutbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    this.InvokeMessageBox($"Failed to start the tunnel.",
                                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                    _needToRepopulateTunnels = true;
                                }
                            });
                        }
                        */
                        listViewEndpoints.InvokeClearListViewRows();
                    }
                    //Start ↑
                    else if (e.ClickedItem?.Text == "Delete Tunnel")
                    {
                        /*
                        _client.EnsureNotNull();
                        rowUnderMouse.EnsureNotNull();

                        var tunnel = ((NtTunnelConfiguration?)rowUnderMouse.Tag).EnsureNotNull();

                        if (MessageBox.Show($"Delete the tunnel '{tunnel.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        if (rowUnderMouse.Tag is NtTunnelInboundConfiguration tunnelInbound)
                        {
                            _client.TunnelInbound.DeletePair(tunnelInbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote tunnel, would you like to delete the local one anyway?",
                                        FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local tunnel.
                                            _client.TunnelInbound.Delete(tunnelInbound.TunnelId).ContinueWith((o) =>
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
                            _client.TunnelOutbound.DeletePair(tunnelOutbound.TunnelId).ContinueWith((o) =>
                            {
                                if (o.IsCompletedSuccessfully == false)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        if (MessageBox.Show(this, $"Failed to delete the remote tunnel, would you like to delete the local one anyway?",
                                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            //If the pair deletion failed, just delete the local tunnel.
                                            _client.TunnelOutbound.Delete(tunnelOutbound.TunnelId).ContinueWith((o) =>
                                            {
                                                _needToRepopulateTunnels = true;
                                            });
                                        }
                                    }));
                                }
                            });
                        }
                        */

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


            _client.QueryGetTunnels().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddTunnelToGrid(t));
            });


            void AddTunnelToGrid(NtTunnelConfiguration tunnel)
            {
                if (listViewTunnels.InvokeRequired)
                {
                    listViewTunnels.Invoke(AddTunnelToGrid, tunnel);
                }
                else
                {
                    var item = new ListViewItem(tunnel.Name);
                    item.Tag = new TunnelTag(tunnel);
                    item.SubItems.Add("????");
                    item.SubItems.Add($"{tunnel.Address}");
                    item.SubItems.Add($"{tunnel.Endpoints.Count:n0}");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    listViewTunnels.Items.Add(item);
                }
            }
        }

        private void RepopulateEndpointsGrid(NtTunnelConfiguration tunnel)
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

        private void RepopulateEndpointsGrid_LockRequired(NtTunnelConfiguration tunnel)
        {
            listViewEndpoints.Items.Clear();

            tunnel.Endpoints.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(x => AddEndpointInboundToGrid(tunnel, x));

            tunnel.Endpoints.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(x => AddEndpointOutboundToGrid(tunnel, x));

            void AddEndpointInboundToGrid(NtTunnelConfiguration tunnel, NtEndpointConfiguration endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddEndpointInboundToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.Tag = new EndpointTag(tunnel, endpoint);
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

            void AddEndpointOutboundToGrid(NtTunnelConfiguration tunnel, NtEndpointConfiguration endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddEndpointOutboundToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.Tag = new EndpointTag(tunnel, endpoint);
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
            using var form = new FormUsers(_client.EnsureNotNull());
            form.ShowDialog();
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new FormServiceConfiguration(_client.EnsureNotNull());
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
