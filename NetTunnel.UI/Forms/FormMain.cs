using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NetTunnel.UI.Helpers;
using NetTunnel.UI.Properties;
using NetTunnel.UI.Types;
using NTDLS.Helpers;
using NTDLS.Localization;
using NTDLS.ReliableMessaging;
using NTDLS.WinFormsHelpers;
using NTDLS.WinFormsHelpers.Forms;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private bool _formClosing = false;
        private bool _needToRepopulateTunnels = false;
        private int _changeDetectionHash = -1;
        private ListViewColumnMap? _endpointsGridColumnMap;
        private ListViewColumnMap? _tunnelsGridColumnMap;

        private bool _inTimerTick = false;
        private Library.ServiceClient? _client;
        private ListViewItemComparer? _endpointsListViewItemComparer;
        private ListViewItemComparer? _tunnelsListViewItemComparer;
        private readonly Stack<VisualLogEntry> _visualLogEntries = new();
        private System.Windows.Forms.Timer? _timer;
        private volatile int _gridPopulationScope = 0;
        private readonly UISettings _settings = UISettings.Load();

        public FormMain()
        {
            InitializeComponent();
            Theming.InitializeForm(this);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Shown += (object? sender, EventArgs e) =>
            {
                ChangeConnection();
            };

            FormClosed += (object? sender, FormClosedEventArgs e) =>
            {
                _formClosing = true;

                _timer?.Stop();

                var client = _client;
                _client = null;

                Exceptions.Ignore(() =>
                    new Thread(() => client?.Disconnect()
                    ).Start());
            };

            _timer = new System.Windows.Forms.Timer()
            {
                Enabled = true,
                Interval = 1000,
            };

            _timer.Tick += Timer_Tick;
            _timer.Start();

            listViewLogs.MouseUp += ListViewLogs_MouseUp;

            #region Setup listViewTunnels.

            _tunnelsListViewItemComparer = new ListViewItemComparer();
            listViewTunnels.ListViewItemSorter = _tunnelsListViewItemComparer;
            listViewTunnels.ColumnClick += ListViewTunnels_ColumnClick;
            listViewTunnels.MouseUp += ListViewTunnels_MouseUp;
            listViewTunnels.SelectedIndexChanged += ListViewTunnels_SelectedIndexChanged;

            listViewTunnels.Columns.Clear();
            AddListViewColumn(listViewTunnels, "Name", () => Localize("Name"), 250);
            AddListViewColumn(listViewTunnels, "Direction", () => Localize("Direction"), 75);
            AddListViewColumn(listViewTunnels, "Address", () => Localize("Address"), 120);
            AddListViewColumn(listViewTunnels, "Endpoints", () => Localize("Endpoints"), 70);
            AddListViewColumn(listViewTunnels, "BytesSent", () => Localize("Sent"), 80);
            AddListViewColumn(listViewTunnels, "BytesReceived", () => Localize("Received"), 80);
            AddListViewColumn(listViewTunnels, "Status", () => Localize("Status"), 140);
            AddListViewColumn(listViewTunnels, "Ping", () => Localize("Ping"), 80);
            _tunnelsGridColumnMap = new ListViewColumnMap(listViewTunnels);

            #endregion

            #region Setup listViewEndpoints.

            _endpointsListViewItemComparer = new ListViewItemComparer();
            listViewEndpoints.ListViewItemSorter = _endpointsListViewItemComparer;
            listViewEndpoints.ColumnClick += ListViewEndpoints_ColumnClick;
            listViewEndpoints.MouseUp += ListViewEndpoints_MouseUp;
            listViewEndpoints.MouseDoubleClick += ListViewEndpoints_MouseDoubleClick;

            listViewEndpoints.Columns.Clear();
            AddListViewColumn(listViewEndpoints, "Name", () => Localize("Name"), 250);
            AddListViewColumn(listViewEndpoints, "Direction", () => Localize("Direction"), 75);
            AddListViewColumn(listViewEndpoints, "Address", () => Localize("Address"), 120);
            AddListViewColumn(listViewEndpoints, "BytesSent", () => Localize("Sent"), 80);
            AddListViewColumn(listViewEndpoints, "BytesReceived", () => Localize("Received"), 80);
            //AddListViewColumn(listViewEndpoints, "CompressionRatio", () => Localize("Comp. Ratio"), 80);
            AddListViewColumn(listViewEndpoints, "CurrentConnections", () => Localize("Current Conn."), 100);
            AddListViewColumn(listViewEndpoints, "TotalConnections", () => Localize("Total Conn."), 100);
            _endpointsGridColumnMap = new ListViewColumnMap(listViewEndpoints);

            #endregion

            ApplyLanguage();

            static void AddListViewColumn(ListView listView, string name, Func<string> text, int width)
            {
                //The tag holds the function which produces the text in the current language.
                listView.Columns.Add(new ColumnHeader
                {
                    Name = name,
                    Text = text(),
                    Tag = text,
                    Width = width
                });
            }
        }

        #region Language.

        private void LanguageToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using var form = new FormLanguage(_settings.Language) { Icon = Icon };
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _settings.Language = form.SelectedLanguage;
            Localizer.Default.SetLanguage(_settings.Language);
            _settings.Save();
            ApplyLanguage();
        }

        /// <summary>
        /// Puts all of the text on the window in the current language. Called at the start and whenever the language changes.
        /// The text the designer put on the controls is translated by <see cref="FormLocalizer"/>; the rest is set here.
        /// </summary>
        private void ApplyLanguage()
        {
            FormLocalizer.Apply(this);

            foreach (var listView in new[] { listViewTunnels, listViewEndpoints })
            {
                foreach (ColumnHeader column in listView.Columns)
                {
                    if (column.Tag is Func<string> text)
                    {
                        column.Text = text();
                    }
                }
            }

            Text = _client != null ? $"{FriendlyName} : {_client.Address}" : FriendlyName;

            if (_client != null)
            {
                //Repopulates the grids, which have text that is built in the current language.
                RepopulateTunnelsGrid();
                ListViewTunnels_SelectedIndexChanged(null, EventArgs.Empty);
            }
        }

        #endregion

        private void ListViewLogs_MouseUp(object? sender, MouseEventArgs e)
        {

            var menu = new ContextMenuStrip();

            UIUtility.AddMenuItem(menu, "Clear", Localize("Clear"));

            menu.Show(listViewLogs, new Point(e.X, e.Y));

            menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
            {
                menu.Hide();

                if (e.ClickedItem?.Tag as string == "Clear")
                {
                    listViewLogs.Items.Clear();
                }
            };
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
                var eTag = TunnelEndpointTag.FromItem(selectedEndpointRow);

                using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, eTag.Endpoint);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RepopulateTunnelsGrid();
                }
            }
        }

        private void WriteVisualLog(DateTime dateTime, NtLogSeverity severity, string message)
        {
            Monitor.Enter(_visualLogEntries);
            try
            {
                _visualLogEntries.Push(new VisualLogEntry
                {
                    DateTime = dateTime,
                    Severity = severity,
                    Message = message,
                });
            }
            catch
            {
            }
            Monitor.Exit(_visualLogEntries);
        }

        private void ChangeConnection()
        {
            if (_formClosing)
            {
                return;
            }

            try
            {
                _visualLogEntries.Clear();

                using var form = new FormLogin(WriteVisualLog);
                form.Owner = this;
                form.LanguageChanged += (_, _) =>
                {
                    _settings.Language = UISettings.Load().Language;
                    ApplyLanguage();
                };
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _client = form.ResultingClient.EnsureNotNull();

                    _client.OnDisconnected += (RmContext context) =>
                    {
                        if (_formClosing)
                        {
                            return;
                        }
                        _client = null;
                        Invoke(new Action(ChangeConnection));
                    };

                    Text = $"{FriendlyName} : {_client.Address}";

                    configurationToolStripMenuItem.Enabled = IsAdministrator();
                    usersToolStripMenuItem.Enabled = IsAdministrator();

                    RepopulateTunnelsGrid();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

            Close();
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
                if (_gridPopulationScope != 0 || _inTimerTick || _client == null)
                {
                    return;
                }
                _inTimerTick = true;
            }

            if (Monitor.TryEnter(_visualLogEntries))
            {
                try
                {
                    int? lastItemIndex = null;

                    while (_visualLogEntries.TryPop(out var logEntry))
                    {
                        var item = new ListViewItem($"{logEntry.DateTime}");
                        item.SubItems.Add($"{logEntry.Severity}");
                        item.SubItems.Add(logEntry.Message);
                        lastItemIndex = listViewLogs.Items.Add(item).Index;
                    }

                    if (lastItemIndex != null)
                    {
                        listViewLogs.Items[(int)lastItemIndex].EnsureVisible();
                    }
                }
                catch
                {
                }
                Monitor.Exit(_visualLogEntries);
            }

            listViewLogs.EndUpdate();

            new Thread(() =>
            {
                try
                {
                    Thread.Sleep(100); //Hack to prevent meaningless race-condition by giving _client a chance to be NULL.
                    if (_client == null)
                    {
                        return;
                    }

                    if (_client.IsConnected)
                    {
                        var result = _client.UIQueryGetTunnelStatistics();

                        int changeDetectionHash = result.AllTunnelIdAndEndpointIdHashes();

                        if (changeDetectionHash != _changeDetectionHash && _changeDetectionHash != -1)
                        {
                            _needToRepopulateTunnels = true;
                        }
                        _changeDetectionHash = changeDetectionHash;

                        PopulateEndpointStatistics(result.Statistics);
                        PopulateTunnelStatistics(result.Statistics);
                    }

                    void PopulateEndpointStatistics(List<TunnelStatisticsDisplay> statistics)
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
                            var eTag = TunnelEndpointTag.FromItem(item);

                            var tunnelStats = statistics.Where(o => o.TunnelKey == eTag.Tunnel.TunnelKey).ToList();
                            if (tunnelStats != null)
                            {
                                var endpointStats = tunnelStats.SelectMany(o => o.EndpointStatistics)
                                    .SingleOrDefault(o => o.EndpointKey == eTag.Endpoint.EndpointKey);

                                if (endpointStats != null)
                                {
                                    /*
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
                                    */

                                    _endpointsGridColumnMap.EnsureNotNull();
                                    _endpointsGridColumnMap.SubItem(item, "BytesSent").Text = $"{Formatters.FileSize((long)endpointStats.BytesSent)}";
                                    _endpointsGridColumnMap.SubItem(item, "BytesReceived").Text = $"{Formatters.FileSize((long)endpointStats.BytesReceived)}";
                                    _endpointsGridColumnMap.SubItem(item, "TotalConnections").Text = $"{endpointStats.TotalConnections:n0}";
                                    _endpointsGridColumnMap.SubItem(item, "CurrentConnections").Text = $"{endpointStats.CurrentConnections:n0}";
                                    //_endpointsGridColumnMap.SubItem(item, "CompressionRatio").Text = $"{compressionRatio:n2}";
                                }
                            }
                        }

                        listViewEndpoints.EndUpdate();

                        #endregion
                    }

                    void PopulateTunnelStatistics(List<TunnelStatisticsDisplay> statistics)
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
                                _tunnelsGridColumnMap.SubItem(item, "Status").Text = UIUtility.StatusText(tunnelStats.Status);
                                _tunnelsGridColumnMap.SubItem(item, "Ping").Text = $"{(tunnelStats.PingTime?.ToString("n2") ?? "∞")}ms";

                                var isDark = Theming.IsDarkMode;

                                item.BackColor = tunnelStats.Status switch
                                {
                                    NtTunnelStatus.Connecting or NtTunnelStatus.Disconnected
                                        => isDark ? Color.FromArgb(110, 40, 40) : Color.FromArgb(255, 200, 200),
                                    NtTunnelStatus.Stopped
                                        => isDark ? Color.FromArgb(125, 90, 0) : Color.FromArgb(255, 200, 0),
                                    NtTunnelStatus.Established
                                        => isDark ? Color.FromArgb(35, 95, 45) : Color.FromArgb(200, 255, 200),
                                    _ => listViewTunnels.BackColor,
                                };
                                item.ForeColor = isDark ? Color.White : Color.Black;
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

            labelEndpoints.Text = Localize("Endpoints of (select a tunnel above)");

            if (listViewTunnels.SelectedItems.Count == 1)
            {
                var selectedRow = listViewTunnels.SelectedItems[0];

                if (selectedRow.Tag is TunnelTag tTag)
                {
                    labelEndpoints.Text = tTag.Tunnel.Direction == NtDirection.Inbound
                        ? Localize("Endpoints of inbound tunnel '{0}'", tTag.Tunnel.Name)
                        : Localize("Endpoints of outbound tunnel '{0}'", tTag.Tunnel.Name);

                    lock (listViewEndpoints)
                    {
                        RepopulateEndpointsGrid(tTag.Tunnel);
                    }
                }
            }
            else
            {
                labelEndpoints.Text = Localize("Endpoints of (select a tunnel above)");
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

                var eTag = TunnelEndpointTag.FromItemOrDefault(selectedEndpointRow);

                var menu = new ContextMenuStrip();

                if (_client != null)
                {
                    if (tTag.Tunnel.Direction == NtDirection.Outbound
                        || (tTag.Tunnel.Direction == NtDirection.Inbound && tTag.Tunnel.IsLoggedIn))
                    {
                        UIUtility.AddMenuItem(menu, "Add Inbound Endpoint", Localize("Add Inbound Endpoint")).Enabled = IsAdministrator();
                        UIUtility.AddMenuItem(menu, "Add Outbound Endpoint", Localize("Add Outbound Endpoint")).Enabled = IsAdministrator();

                        if (eTag != null)
                        {
                            menu.Items.Add(new ToolStripSeparator());
                            UIUtility.AddMenuItem(menu, "Delete Endpoint", Localize("Delete Endpoint")).Enabled = IsAdministrator();
                            menu.Items.Add(new ToolStripSeparator());
                            UIUtility.AddMenuItem(menu, "Edge Connections", Localize("Edge Connections"));
                            UIUtility.AddMenuItem(menu, "Properties", Localize("Properties"));
                        }
                    }
                }

                menu.Show(listViewEndpoints, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (e.ClickedItem?.Tag as string == "Add Inbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (e.ClickedItem?.Tag as string == "Add Outbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (eTag != null && e.ClickedItem?.Tag as string == "Edge Connections")
                    {
                        try
                        {
                            using var form = new FormEndpointEdgeConnections(_client.EnsureNotNull(), tTag.Tunnel.TunnelKey, eTag.Endpoint.EndpointKey);
                            form.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            this.InvokeMessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }

                        listViewEndpoints.InvokeClearRows();
                    }

                    else if (eTag != null && e.ClickedItem?.Tag as string == "Properties")
                    {
                        try
                        {
                            using var form = new FormEndpointProperties(_client.EnsureNotNull(), tTag.Tunnel.TunnelKey, eTag.Endpoint.EndpointKey);
                            form.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            this.InvokeMessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }

                        listViewEndpoints.InvokeClearRows();
                    }
                    else if (eTag != null && e.ClickedItem?.Tag as string == "Delete Endpoint")
                    {
                        if (MessageBox.Show(Localize("Delete the endpoint '{0}'?", eTag.Endpoint.Name),
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        var progressForm = new ProgressForm(FriendlyName, Localize("Deleting endpoint..."));

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                _client.EnsureNotNull().UIQueryDeleteEndpoint(eTag.Tunnel.TunnelKey, eTag.Endpoint.EndpointId);

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

                UIUtility.AddMenuItem(menu, "Create New Tunnel", Localize("Create New Tunnel"));

                if (selectedRow != null && tTag != null)
                {
                    if (_client != null && tTag.Tunnel.IsLoggedIn)
                    {
                        menu.Items.Add(new ToolStripSeparator());
                        UIUtility.AddMenuItem(menu, "Add Inbound Endpoint", Localize("Add Inbound Endpoint")).Enabled = IsAdministrator();
                        UIUtility.AddMenuItem(menu, "Add Outbound Endpoint", Localize("Add Outbound Endpoint")).Enabled = IsAdministrator();
                    }

                    if (IsAdministrator() && tTag.Tunnel.Direction == NtDirection.Outbound)
                    {
                        menu.Items.Add(new ToolStripSeparator());

                        if (tTag.TunnelStatus == NtTunnelStatus.Stopped)
                        {
                            UIUtility.AddMenuItem(menu, "Start", Localize("Start"));
                        }
                        else
                        {
                            UIUtility.AddMenuItem(menu, "Stop", Localize("Stop"));
                        }

                        menu.Items.Add(new ToolStripSeparator());
                        UIUtility.AddMenuItem(menu, "Delete Tunnel", Localize("Delete Tunnel"));
                    }

                    menu.Items.Add(new ToolStripSeparator());
                    UIUtility.AddMenuItem(menu, "Properties", Localize("Properties"));
                }

                menu.Show(listViewTunnels, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (e.ClickedItem?.Tag as string == "Create New Tunnel")
                    {
                        using var form = new FormConnectTunnel(_client.EnsureNotNull(), WriteVisualLog);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (tTag != null && e.ClickedItem?.Tag as string == "Add Inbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Inbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (tTag != null && e.ClickedItem?.Tag as string == "Add Outbound Endpoint")
                    {
                        using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), tTag.Tunnel, NtDirection.Outbound);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            RepopulateTunnelsGrid();
                        }
                    }
                    else if (tTag != null && e.ClickedItem?.Tag as string == "Stop")
                    {
                        if (MessageBox.Show(Localize("Stop the tunnel '{0}'?", tTag.Tunnel.Name),
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        var progressForm = new ProgressForm(FriendlyName, Localize("Stopping tunnel..."));

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                _client.EnsureNotNull().UIQueryStopTunnel(tTag.Tunnel.TunnelKey);

                                _needToRepopulateTunnels = true;
                                listViewEndpoints.InvokeClearRows();
                            }
                            catch (Exception ex)
                            {
                                progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        });
                    }
                    else if (tTag != null && e.ClickedItem?.Tag as string == "Start")
                    {
                        var progressForm = new ProgressForm(FriendlyName, Localize("Starting tunnel..."));

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                _client.EnsureNotNull().UIQueryStartTunnel(tTag.Tunnel.TunnelKey);

                                _needToRepopulateTunnels = true;
                                listViewEndpoints.InvokeClearRows();
                            }
                            catch (Exception ex)
                            {
                                progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        });
                    }
                    else if (tTag != null && e.ClickedItem?.Tag as string == "Properties")
                    {
                        try
                        {
                            using var form = new FormTunnelProperties(_client.EnsureNotNull(), tTag.Tunnel.TunnelKey);
                            form.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            this.InvokeMessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }

                        listViewEndpoints.InvokeClearRows();
                    }
                    else if (tTag != null && e.ClickedItem?.Tag as string == "Delete Tunnel")
                    {
                        if (MessageBox.Show(Localize("Delete the tunnel '{0}'?", tTag.Tunnel.Name),
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        try
                        {
                            _client.EnsureNotNull().UIQueryDeleteTunnel(tTag.Tunnel.TunnelKey);

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

        public bool IsAdministrator()
        {
            if (_client == null)
            {
                return false;
            }

            return _client.Role == NtUserRole.Administrator;
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
                var result = _client.UIQueryGetTunnels();
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
                    item.SubItems.Add(UIUtility.DirectionText(tunnel.Direction));
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

            void AddEndpointInboundToGrid(TunnelDisplay tunnel, EndpointConfiguration endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddEndpointInboundToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.Tag = new TunnelEndpointTag(tunnel, endpoint);
                    item.SubItems.Add(Localize("Inbound"));
                    item.SubItems.Add($"*:{endpoint.InboundPort}");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    item.SubItems.Add("∞");
                    listViewEndpoints.Items.Add(item);
                }
            }

            void AddEndpointOutboundToGrid(TunnelDisplay tunnel, EndpointConfiguration endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddEndpointOutboundToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.Tag = new TunnelEndpointTag(tunnel, endpoint);
                    item.SubItems.Add(Localize("Outbound"));
                    item.SubItems.Add(Utility.FormatHostPort(endpoint.OutboundAddress, endpoint.OutboundPort));
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
                {
                    _tunnelsListViewItemComparer.SortOrder = SortOrder.Descending;
                }
                else
                {
                    _tunnelsListViewItemComparer.SortOrder = SortOrder.Ascending;
                }
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
                {
                    _endpointsListViewItemComparer.SortOrder = SortOrder.Descending;
                }
                else
                {
                    _endpointsListViewItemComparer.SortOrder = SortOrder.Ascending;
                }
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
            ChangeConnection();
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
            using var form = new FormAbout(Constants.FriendlyName, this.Icon, Resources.Logo128);
            form.ShowDialog();
        }

        #endregion
    }
}
