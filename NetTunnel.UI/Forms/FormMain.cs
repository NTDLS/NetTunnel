using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private NtClient? _client;

        private System.Windows.Forms.Timer? _timer;
        private readonly List<NtTunnelStatistics> _latestStats = new();

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

        private void _timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_client != null && _client.IsConnected)
                {
                    _client.GetStatistics().ContinueWith(inboundStats =>
                    {
                        lock (_latestStats)
                        {
                            _latestStats.Clear();
                            _latestStats.AddRange(inboundStats.Result.TunnelStatistics);

                            throw new Exception("Fill in the stats of the grids??");
                        }

                    });
                }
            }
            catch { }
        }

        private void ListViewTunnels_SelectedIndexChanged(object? sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

            if (listViewTunnels.SelectedItems.Count == 1)
            {
                var selectedRow = listViewTunnels.SelectedItems[0];

                if (selectedRow.Tag is INtTunnelConfiguration tunnel)
                {
                    RepopulateEndpointsGrid(tunnel);
                }
            }
        }

        private void RepopulateEndpointsGrid(INtTunnelConfiguration tunnelInbound)
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
                    item.SubItems.Add($"*:{endpoint.Port}");
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
                    item.SubItems.Add($"{endpoint.Address}{endpoint.Port}");
                    listViewEndpoints.Items.Add(item);
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

                menu.Items.Add("Add Tunnel");

                if (itemUnderMouse != null)
                {
                    menu.Items.Add("Add Endpoint to Tunnel");
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Delete Tunnel");
                }

                menu.Show(listViewTunnels, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    if (e.ClickedItem?.Text == "Add Tunnel")
                    {
                        Utility.EnsureNotNull(_client);

                        using (var formAddTunnel = new FormAddTunnel(_client))
                        {
                            if (formAddTunnel.ShowDialog() == DialogResult.OK)
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
                        MessageBox.Show("Not implemented");
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

        private void RepopulateTunnelsGrid()
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

                    listViewTunnels.Items.Add(item);

                }
            }
        }

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
