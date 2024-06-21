﻿using NetTunnel.Library.Payloads;
using NetTunnel.UI.Helpers;
using NetTunnel.UI.Types;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private int _changeDetectionHash = -1;
        private bool _needToRepopulateTunnels = false;
        private ListViewColumnMap? _tunnelsGridColumnMap;
        private ListViewColumnMap? _endpointsGridColumnMap;

        private Library.ServiceClient? _client;

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

            _timer.Tick += Timer_Tick;
            _timer.Start();

            #region Setup listViewTunnels.

            _tunnelsListViewItemComparer = new ListViewItemComparer();
            listViewTunnels.ListViewItemSorter = _tunnelsListViewItemComparer;
            listViewTunnels.ColumnClick += ListViewTunnels_ColumnClick;
            listViewTunnels.MouseUp += ListViewTunnels_MouseUp;
            listViewTunnels.SelectedIndexChanged += ListViewTunnels_SelectedIndexChanged;

            listViewTunnels.Columns.Clear();
            AddListViewColumn(listViewTunnels, "Name", "Name", 250);
            AddListViewColumn(listViewTunnels, "Direction", "Direction", 75);
            AddListViewColumn(listViewTunnels, "Address", "Address", 120);
            AddListViewColumn(listViewTunnels, "Endpoints", "Endpoints", 70);
            AddListViewColumn(listViewTunnels, "BytesSent", "Sent", 80);
            AddListViewColumn(listViewTunnels, "BytesReceived", "Received", 80);
            AddListViewColumn(listViewTunnels, "Status", "Status", 140);
            AddListViewColumn(listViewTunnels, "Ping", "Ping", 80);
            _tunnelsGridColumnMap = new ListViewColumnMap(listViewTunnels);

            #endregion

            #region Setup listViewEndpoints.

            _endpointsListViewItemComparer = new ListViewItemComparer();
            listViewEndpoints.ListViewItemSorter = _endpointsListViewItemComparer;
            listViewEndpoints.ColumnClick += ListViewEndpoints_ColumnClick;
            listViewEndpoints.MouseUp += ListViewEndpoints_MouseUp;
            listViewEndpoints.MouseDoubleClick += ListViewEndpoints_MouseDoubleClick;

            listViewEndpoints.Columns.Clear();
            AddListViewColumn(listViewEndpoints, "Name", "Name", 250);
            AddListViewColumn(listViewEndpoints, "Direction", "Direction", 75);
            AddListViewColumn(listViewEndpoints, "Address", "Address", 120);
            AddListViewColumn(listViewEndpoints, "BytesSent", "Sent", 80);
            AddListViewColumn(listViewEndpoints, "BytesReceived", "Received", 80);
            AddListViewColumn(listViewEndpoints, "CompressionRatio", "Comp. Ratio", 80);
            AddListViewColumn(listViewEndpoints, "CurrentConnections", "Current Conn.", 100);
            AddListViewColumn(listViewEndpoints, "TotalConnections", "Total Conn.", 100);
            _endpointsGridColumnMap = new ListViewColumnMap(listViewEndpoints);

            #endregion

            static void AddListViewColumn(ListView listView, string name, string text, int width)
            {
                listView.Columns.Add(new ColumnHeader
                {
                    Name = name,
                    Text = text,
                    Width = width
                });
            }
        }

        private void ListViewEndpoints_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            var selectedTunnelRow = listViewTunnels.SelectedItems?.Count > 0 ? listViewTunnels.SelectedItems[0] : null;
            if (selectedTunnelRow == null)
            {
                return;
            }

            var tTag = TunnelTag.FromItem(selectedTunnelRow);

            var selectedEndpointRow = listViewEndpoints.GetItemAt(e.X, e.Y);
            if (selectedEndpointRow != null)
            {
                var eTag = EndpointTag.FromItem(selectedEndpointRow);

                using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, eTag.Endpoint);
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
                using var form = new FormLogin();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _client = form.ResultingClient.EnsureNotNull();
                    Text = $"{FriendlyName} : {_client.Address}";
                    RepopulateTunnelsGrid();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            return false;
        }

        private void Timer_Tick(object? sender, EventArgs e)
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

            new Thread(() =>
            {
                try
                {
                    if (_client != null && _client.IsConnected)
                    {
                        var result = _client.QueryGetTunnelStatistics();

                        int changeDetectionHash = result.AllTunnelIdAndEndpointIdHashes();

                        if (changeDetectionHash != _changeDetectionHash && _changeDetectionHash != -1)
                        {
                            _needToRepopulateTunnels = true;
                        }
                        _changeDetectionHash = changeDetectionHash;

                        PopulateEndpointStatistics(result.Statistics);
                        PopulateTunnelStatistics(result.Statistics);

                    }

                    void PopulateEndpointStatistics(List<TunnelStatistics> statistics)
                    {
                        #region Populate Endpoint Statistics.

                        if (listViewEndpoints.InvokeRequired)
                        {
                            listViewEndpoints.Invoke(PopulateEndpointStatistics, statistics);
                            return;
                        }
                        listViewEndpoints.BeginUpdate();

                        foreach (ListViewItem item in listViewEndpoints.Items)
                        {
                            var eTag = EndpointTag.FromItem(item);

                            var tunnelStats = statistics.Where(o => o.TunnelKey == eTag.Tunnel.TunnelKey).ToList();
                            if (tunnelStats != null)
                            {
                                var endpointStats = tunnelStats.SelectMany(o => o.EndpointStatistics)
                                    .SingleOrDefault(o => o.EndpointKey == eTag.Endpoint.EndpointKey);

                                if (endpointStats != null)
                                {
                                    double compressionRatio = 0;
                                    if (endpointStats.BytesSent > 0 && endpointStats.BytesReceived > 0)
                                    {
                                        if (endpointStats.BytesSent > endpointStats.BytesReceived)
                                        {
                                            compressionRatio = 100 - (endpointStats.BytesReceived / endpointStats.BytesSent) * 100.0;
                                        }
                                        else
                                        {
                                            compressionRatio = 100 - (endpointStats.BytesSent / endpointStats.BytesReceived) * 100.0;
                                        }
                                    }

                                    _endpointsGridColumnMap.EnsureNotNull();
                                    _endpointsGridColumnMap.SubItem(item, "BytesSent").Text = $"{Formatters.FileSize((long)endpointStats.BytesSent)}";
                                    _endpointsGridColumnMap.SubItem(item, "BytesReceived").Text = $"{Formatters.FileSize((long)endpointStats.BytesReceived)}";
                                    _endpointsGridColumnMap.SubItem(item, "TotalConnections").Text = $"{endpointStats.TotalConnections:n0}";
                                    _endpointsGridColumnMap.SubItem(item, "CurrentConnections").Text = $"{endpointStats.CurrentConnections:n0}";
                                    _endpointsGridColumnMap.SubItem(item, "CompressionRatio").Text = $"{compressionRatio:n2}";
                                }
                            }
                        }

                        listViewEndpoints.EndUpdate();

                        #endregion
                    }

                    void PopulateTunnelStatistics(List<TunnelStatistics> statistics)
                    {
                        #region Populate Tunnel Statistics.

                        if (listViewTunnels.InvokeRequired)
                        {
                            listViewTunnels.Invoke(PopulateTunnelStatistics, statistics);
                            return;
                        }
                        listViewTunnels.BeginUpdate();

                        foreach (ListViewItem item in listViewTunnels.Items)
                        {
                            var tTag = TunnelTag.FromItem(item);

                            var tunnelStats = statistics.SingleOrDefault(o => o.TunnelKey == tTag.Tunnel.TunnelKey);
                            if (tunnelStats != null)
                            {
                                _tunnelsGridColumnMap.EnsureNotNull();
                                _tunnelsGridColumnMap.SubItem(item, "BytesSent").Text = $"{Formatters.FileSize((long)tunnelStats.BytesSent)}";
                                _tunnelsGridColumnMap.SubItem(item, "BytesReceived").Text = $"{Formatters.FileSize((long)tunnelStats.BytesReceived)}";
                                _tunnelsGridColumnMap.SubItem(item, "Status").Text = tunnelStats.Status.ToString();
                                _tunnelsGridColumnMap.SubItem(item, "Ping").Text = $"{(tunnelStats.PingMs?.ToString("n2") ?? "∞")}ms";

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

                        #endregion
                    }
                }
                catch
                {
                }
                finally
                {
                    _inTimerTick = false;
                }
            }).Start();
        }
        private void ListViewTunnels_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _client.EnsureNotNull();

            labelEndpoints.Text = "Endpoints of ";

            if (listViewTunnels.SelectedItems.Count == 1)
            {
                var selectedRow = listViewTunnels.SelectedItems[0];

                if (selectedRow.Tag is TunnelTag tTag)
                {
                    if (tTag.Tunnel.ServiceId == _client.ServiceId)
                    {
                        labelEndpoints.Text += "outbound tunnel ";
                    }

                    labelEndpoints.Text += $"'{tTag.Tunnel.Name}'";

                    lock (listViewEndpoints)
                    {
                        RepopulateEndpointsGrid(tTag.Tunnel);
                    }
                }
            }
            else
            {
                labelEndpoints.Text += " (select a tunnel above)";
            }
        }

        private void ListViewEndpoints_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedTunnelRow = listViewTunnels.SelectedItems?.Count > 0 ? listViewTunnels.SelectedItems[0] : null;
                if (selectedTunnelRow == null)
                {
                    return;
                }

                var tTag = TunnelTag.FromItem(selectedTunnelRow);

                var selectedEndpointRow = listViewEndpoints.GetItemAt(e.X, e.Y);
                if (selectedEndpointRow != null)
                {
                    selectedEndpointRow.Selected = true;
                }

                var eTag = EndpointTag.FromItemOrDefault(selectedEndpointRow);

                var menu = new ContextMenuStrip();

                if (tTag.Tunnel.Direction == NtDirection.Outbound
                    || (tTag.Tunnel.Direction == NtDirection.Inbound && tTag.Tunnel.IsLoggedIn))
                {
                    menu.Items.Add("Add Inbound Endpoint");
                    menu.Items.Add("Add Outbound Endpoint");

                    if (eTag != null)
                    {
                        menu.Items.Add(new ToolStripSeparator());
                        menu.Items.Add("Delete Endpoint");
                    }
                }

                menu.Show(listViewEndpoints, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (e.ClickedItem?.Text == "Add Inbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Text == "Add Outbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (eTag != null && e.ClickedItem?.Text == "Delete Endpoint")
                    {
                        if (MessageBox.Show($"Delete the endpoint '{eTag.Endpoint.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        var progressForm = new ProgressForm(FriendlyName, "Deleting endpoint...");

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                _client.EnsureNotNull().QueryDeleteEndpoint(eTag.Tunnel.TunnelKey, eTag.Endpoint.EndpointId);

                                _needToRepopulateTunnels = true;
                                listViewEndpoints.InvokeClearRows();
                            }
                            catch (Exception ex)
                            {
                                progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        });
                    }
                };
            }
        }

        private void ListViewTunnels_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedRow = listViewTunnels.GetItemAt(e.X, e.Y);
                if (selectedRow != null)
                {
                    selectedRow.Selected = true;
                }

                var tTag = TunnelTag.FromItemOrDefault(selectedRow);

                var menu = new ContextMenuStrip();

                menu.Items.Add("Connect Tunnel");

                if (selectedRow != null && tTag != null)
                {
                    if (tTag.Tunnel.Direction == NtDirection.Outbound
                        || (tTag.Tunnel.Direction == NtDirection.Inbound && tTag.Tunnel.IsLoggedIn))
                    {
                        menu.Items.Add(new ToolStripSeparator());
                        menu.Items.Add("Add Inbound Endpoint");
                        menu.Items.Add("Add Outbound Endpoint");
                    }

                    if (tTag.Tunnel.Direction == NtDirection.Outbound)
                    {
                        menu.Items.Add(new ToolStripSeparator());

                        if (tTag.TunnelStatus == NtTunnelStatus.Stopped)
                        {
                            menu.Items.Add("Start");
                        }
                        else
                        {
                            menu.Items.Add("Stop");
                        }
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
                    else if (tTag != null && e.ClickedItem?.Text == "Add Inbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (tTag != null && e.ClickedItem?.Text == "Add Outbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (tTag != null && e.ClickedItem?.Text == "Stop")
                    {
                        if (MessageBox.Show($"Stop the tunnel '{tTag.Tunnel.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        var progressForm = new ProgressForm(FriendlyName, "Stopping tunnel...");

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                _client.EnsureNotNull().QueryStopTunnel(tTag.Tunnel.TunnelKey);

                                _needToRepopulateTunnels = true;
                                listViewEndpoints.InvokeClearRows();
                            }
                            catch (Exception ex)
                            {
                                progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        });
                    }
                    else if (tTag != null && e.ClickedItem?.Text == "Start")
                    {
                        var progressForm = new ProgressForm(FriendlyName, "Starting tunnel...");

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                _client.EnsureNotNull().QueryStartTunnel(tTag.Tunnel.TunnelKey);

                                _needToRepopulateTunnels = true;
                                listViewEndpoints.InvokeClearRows();
                            }
                            catch (Exception ex)
                            {
                                progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        });
                    }
                    //Start ↑
                    else if (tTag != null && e.ClickedItem?.Text == "Delete Tunnel")
                    {
                        if (MessageBox.Show($"Delete the tunnel '{tTag.Tunnel.Name}'?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        try
                        {
                            _client.EnsureNotNull().QueryDeleteTunnel(tTag.Tunnel.TunnelKey);

                            Invoke(new Action(() =>
                            {
                                _needToRepopulateTunnels = true;
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.InvokeMessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
            _client.EnsureNotNull();

            listViewTunnels.Items.Clear();
            listViewEndpoints.Items.Clear();

            try
            {
                var result = _client.QueryGetTunnels();
                result.Collection.ForEach(t => AddTunnelToGrid(t));
            }
            catch (Exception ex)
            {
                this.InvokeMessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

            void AddTunnelToGrid(TunnelDisplay tunnel)
            {
                if (listViewTunnels.InvokeRequired)
                {
                    listViewTunnels.Invoke(AddTunnelToGrid, tunnel);
                }
                else
                {
                    var item = new ListViewItem(tunnel.Name);
                    item.Tag = new TunnelTag(tunnel);
                    item.SubItems.Add($"{tunnel.Direction}");
                    item.SubItems.Add($"{tunnel.Address}");
                    item.SubItems.Add($"{tunnel.Endpoints.Count:n0}");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    listViewTunnels.Items.Add(item);
                }
            }
        }

        private void RepopulateEndpointsGrid(TunnelDisplay tunnel)
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

        private void RepopulateEndpointsGrid_LockRequired(TunnelDisplay tunnel)
        {
            listViewEndpoints.Items.Clear();

            tunnel.Endpoints.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(x => AddEndpointInboundToGrid(tunnel, x));

            tunnel.Endpoints.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(x => AddEndpointOutboundToGrid(tunnel, x));

            void AddEndpointInboundToGrid(TunnelDisplay tunnel, EndpointDisplay endpoint)
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

            void AddEndpointOutboundToGrid(TunnelDisplay tunnel, EndpointDisplay endpoint)
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

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ChangeConnection()) Close();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new FormUsers(_client.EnsureNotNull());
            form.ShowDialog();
        }

        private void ConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new FormServiceConfiguration(_client.EnsureNotNull());
            form.ShowDialog();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new FormAbout();
            form.ShowDialog();
        }

        #endregion
    }
}
