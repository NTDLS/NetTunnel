using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NetTunnel.UI.Types;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormEndpointEdgeConnections : Form
    {
        private readonly ServiceClient? _client;
        private readonly DirectionalKey _tunnelKey;
        private readonly DirectionalKey _endpointKey;
        private bool _firstShown = true;
        private bool _inTimerTick = false;
        private System.Windows.Forms.Timer? _timer;
        private ListViewColumnMap? _connectionsGridColumnMap;

        public FormEndpointEdgeConnections()
        {
            InitializeComponent();
            _tunnelKey = new DirectionalKey();
            _endpointKey = new DirectionalKey();
        }

        public FormEndpointEdgeConnections(ServiceClient client, DirectionalKey tunnelKey, DirectionalKey endpointKey)
        {
            InitializeComponent();

            _client = client;
            _endpointKey = endpointKey;
            _tunnelKey = tunnelKey;

            Text = $"{FriendlyName} : Edge Connections";

            Shown += FormEndpointEdgeConnections_Shown;
            FormClosing += FormEndpointEdgeConnections_FormClosing;

            #region Setup listViewConnections.

            listViewConnections.MouseUp += ListViewConnections_MouseUp;

            listViewConnections.Columns.Clear();
            AddListViewColumn(listViewConnections, "AddressFamily", "Family", 100);
            AddListViewColumn(listViewConnections, "Address", "Address", 100);
            AddListViewColumn(listViewConnections, "Port", "Port", 100);

            AddListViewColumn(listViewConnections, "BytesReceived", "Received", 100);
            AddListViewColumn(listViewConnections, "BytesSent", "Sent", 100);

            AddListViewColumn(listViewConnections, "StartDateTime", "Started", 100);
            AddListViewColumn(listViewConnections, "LastActivityDateTime", "Activity", 100);
            AddListViewColumn(listViewConnections, "EdgeId", "Id", 100);
            AddListViewColumn(listViewConnections, "ThreadId", "Thread Id", 100);
            AddListViewColumn(listViewConnections, "IsConnected", "Connected", 100);
            _connectionsGridColumnMap = new ListViewColumnMap(listViewConnections);

            #endregion

            _timer = new System.Windows.Forms.Timer()
            {
                Enabled = true,
                Interval = 1000,
            };

            _timer.Tick += Timer_Tick;
            _timer.Start();

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

        private void FormEndpointEdgeConnections_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            if (_inTimerTick)
            {
                e.Cancel = true;

                new Thread(() => //Delayed form close.
                {
                    for (int i = 0; _inTimerTick == true; i++)
                    {
                        if (i == 1000)
                        {
                            this.InvokeMessageBox("A timeout has occurred while waiting on the timer to terminate.",
                                FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            this.InvokeClose(DialogResult.OK);
                            return;
                        }

                        Thread.Sleep(100);
                    }

                    this.InvokeClose(DialogResult.OK);
                }).Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_inTimerTick) return;
            _inTimerTick = true;

            new Thread(() =>
            {
                try
                {
                    try
                    {
                        var result = _client.EnsureNotNull().UIQueryGetEndpointEdgeConnections(_tunnelKey, _endpointKey);
                        listViewConnections.Invoke(PopulateListView, result.Collection);
                    }
                    catch
                    {
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

        private void ListViewConnections_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var menu = new ContextMenuStrip();

                var selectedItem = listViewConnections.SelectedItems.Count == 1 ? listViewConnections.SelectedItems[0] : null;

                if (selectedItem != null)
                {
                    menu.Items.Add("Disconnect");
                }

                menu.Show(listViewConnections, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (selectedItem != null && e.ClickedItem?.Text == "Disconnect")
                    {
                        if (MessageBox.Show($"Disconnect the endpoint edge connection?",
                            FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }

                        var progressForm = new ProgressForm(FriendlyName, "Terminating edge connection...");

                        progressForm.Execute(() =>
                        {
                            try
                            {
                                if (selectedItem.Tag is EdgeState tag)
                                {
                                    _client.EnsureNotNull().NotifyTerminateEndpointEdgeConnection(tag.TunnelKey, tag.EndpointKey.Id, tag.EdgeId);
                                }
                            }
                            catch (Exception ex)
                            {
                                progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        });

                        Clipboard.SetText(selectedItem.SubItems[1].Text);
                    }
                };
            }
        }

        private void FormEndpointEdgeConnections_Shown(object? sender, EventArgs e)
        {
            if (!_firstShown)
            {
                return;
            }
            _firstShown = false;

            var progressForm = new ProgressForm(FriendlyName, "Getting connections...");

            progressForm.Execute(() =>
            {
                try
                {
                    var result = _client.EnsureNotNull().UIQueryGetEndpointEdgeConnections(_tunnelKey, _endpointKey);
                    listViewConnections.Invoke(PopulateListView, result.Collection);

                    AutoResizeColumns();
                }
                catch (Exception ex)
                {
                    progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            });
        }

        public void AutoResizeColumns()
        {
            if (listViewConnections.InvokeRequired)
            {
                listViewConnections.Invoke(AutoResizeColumns);
                return;
            }

            if (listViewConnections.Items.Count == 0)
            {
                listViewConnections.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.HeaderSize);
                listViewConnections.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            else
            {
                listViewConnections.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewConnections.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }


        enum EdgeStatus
        {
            Normal, //This is a persistent connection.
            New, //This is a new connection.
            Expire //The connection is gone.
        }

        class EdgeState
        {
            public EdgeStatus Status { get; set; }
            public DirectionalKey TunnelKey { get; set; } = new();
            public DirectionalKey EndpointKey { get; set; } = new();
            public Guid EdgeId { get; set; }
        }

        public void PopulateListView(List<EndpointEdgeConnectionDisplay> connections)
        {
            _connectionsGridColumnMap.EnsureNotNull();

            listViewConnections.BeginUpdate();

            var idLookup = new Dictionary<Guid, int>();

            var expiredItems = new List<ListViewItem>();

            foreach (ListViewItem item in listViewConnections.Items)
            {
                var tag = (EdgeState)item.Tag.EnsureNotNull();

                var edgeId = Guid.Parse(_connectionsGridColumnMap.SubItem(item, "EdgeId").Text);
                idLookup.Add(edgeId, item.Index);

                if (tag.Status == EdgeStatus.Expire)
                {
                    expiredItems.Add(item);
                }

                if (connections.Any(o => o.EdgeId == edgeId) == false)
                {
                    item.BackColor = Color.FromArgb(255, 200, 200);
                    tag.Status = EdgeStatus.Expire;
                }
            }

            foreach (var connection in connections)
            {
                if (idLookup.TryGetValue(connection.EdgeId, out var index))
                {
                    var item = listViewConnections.Items[index];

                    var tag = (EdgeState)item.Tag.EnsureNotNull();

                    _connectionsGridColumnMap.SubItem(item, "AddressFamily").Text = connection.AddressFamily;
                    _connectionsGridColumnMap.SubItem(item, "Address").Text = connection.Address;
                    _connectionsGridColumnMap.SubItem(item, "Port").Text = $"{connection.Port}";
                    _connectionsGridColumnMap.SubItem(item, "BytesReceived").Text = $"{Formatters.FileSize((long)connection.BytesReceived)}";
                    _connectionsGridColumnMap.SubItem(item, "BytesSent").Text = $"{Formatters.FileSize((long)connection.BytesSent)}";
                    _connectionsGridColumnMap.SubItem(item, "StartDateTime").Text = $"{connection.StartDateTime}";
                    _connectionsGridColumnMap.SubItem(item, "LastActivityDateTime").Text = $"{connection.LastActivityDateTime}";
                    //_connectionsGridColumnMap.SubItem(item, "EdgeId").Text = $"{connection.EdgeId}";
                    _connectionsGridColumnMap.SubItem(item, "ThreadId").Text = $"{connection.ThreadId}";
                    _connectionsGridColumnMap.SubItem(item, "IsConnected").Text = $"{connection.IsConnected}";

                    item.BackColor = Color.Transparent;

                    tag.Status = EdgeStatus.Normal;
                }
                else
                {
                    var item = new ListViewItem(connection.AddressFamily);
                    item.SubItems.Add(connection.Address);
                    item.SubItems.Add($"{connection.Port}");
                    item.SubItems.Add($"{Formatters.FileSize((long)connection.BytesReceived)}");
                    item.SubItems.Add($"{Formatters.FileSize((long)connection.BytesSent)}");
                    item.SubItems.Add($"{connection.StartDateTime}");
                    item.SubItems.Add($"{connection.LastActivityDateTime}");
                    item.SubItems.Add($"{connection.EdgeId}");
                    item.SubItems.Add($"{connection.ThreadId}");
                    item.SubItems.Add($"{connection.IsConnected}");

                    item.BackColor = Color.FromArgb(200, 255, 200);

                    item.Tag = new EdgeState
                    {
                        Status = EdgeStatus.New,
                        EdgeId = connection.EdgeId,
                        TunnelKey = connection.TunnelKey,
                        EndpointKey = connection.EndpointKey
                    };

                    listViewConnections.Items.Add(item);
                }
            }

            foreach (var expiredItem in expiredItems)
            {
                listViewConnections.Items.Remove(expiredItem);
            }

            listViewConnections.EndUpdate();
        }
    }
}
