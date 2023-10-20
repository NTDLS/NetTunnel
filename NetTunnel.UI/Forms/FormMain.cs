using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;

namespace NetTunnel.UI.Forms
{
    public partial class FormMain : Form
    {
        private NtClient? _client;

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

            listViewTunnels.MouseUp += ListViewTunnels_MouseUp;

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

                        using (var formAddEndpoint = new FormAddEndpoint(_client))
                        {
                            if (formAddEndpoint.ShowDialog() == DialogResult.OK)
                            {
                                RepopulateTunnelsGrid();
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

            _client.IncomingTunnel.List().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddIncomingTunnelToGrid(t));
            });

            _client.OutgoingTunnel.List().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddOutgoingTunnelToGrid(t));
            });

            void AddIncomingTunnelToGrid(NtTunnelInboundConfig tunnel)
            {
                if (listViewTunnels.InvokeRequired)
                {
                    listViewTunnels.Invoke(AddIncomingTunnelToGrid, tunnel);
                }
                else
                {
                    var item = new ListViewItem(tunnel.Name);
                    item.Tag = tunnel;
                    item.SubItems.Add("Inbound");
                    item.SubItems.Add($"*:{tunnel.DataPort}");

                    listViewTunnels.Items.Add(item);
                }
            }

            void AddOutgoingTunnelToGrid(NtTunnelOutboundConfig tunnel)
            {
                if (listViewTunnels.InvokeRequired)
                {
                    listViewTunnels.Invoke(AddOutgoingTunnelToGrid, tunnel);
                }
                else
                {
                    var item = new ListViewItem(tunnel.Name);
                    item.Tag = tunnel;
                    item.SubItems.Add("Outbound");
                    item.SubItems.Add($"{tunnel.Address}{tunnel.DataPort}");

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
