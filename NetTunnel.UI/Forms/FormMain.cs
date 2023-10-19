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

            listViewEndpoints.MouseUp += ListViewEndpoints_MouseUp;

        }

        private void ListViewEndpoints_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var itemUnderMouse = listViewEndpoints.GetItemAt(e.X, e.Y);
                if (itemUnderMouse != null)
                {
                    itemUnderMouse.Selected = true;
                }

                var menu = new ContextMenuStrip();

                menu.Items.Add("Add Endpoint");

                if (itemUnderMouse != null)
                {
                    menu.Items.Add(new ToolStripSeparator());
                    menu.Items.Add("Delete Endpoint");
                }

                menu.Show(listViewEndpoints, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    if (e.ClickedItem?.Text == "Add Endpoint")
                    {
                        Utility.EnsureNotNull(_client);

                        using (var formAddEndpoint = new FormAddEndpoint(_client))
                        {
                            if (formAddEndpoint.ShowDialog() == DialogResult.OK)
                            {
                                RepopulateEndpointsGrid();
                            }
                        }
                    }
                    else if (e.ClickedItem?.Text == "Delete Endpoint")
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
                        RepopulateEndpointsGrid();
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

        private void RepopulateEndpointsGrid()
        {
            Utility.EnsureNotNull(_client);

            listViewEndpoints.Items.Clear();

            _client.IncommingEndpoint.List().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddIncommingEndpointToGrid(t));
            });

            _client.OutgoingEndpoint.List().ContinueWith(t =>
            {
                t.Result.Collection.ForEach(t => AddOutgoingEndpointToGrid(t));
            });

            void AddIncommingEndpointToGrid(NtIncommingEndpoint endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddIncommingEndpointToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.SubItems.Add("Incomming");
                    item.SubItems.Add($"<dynamic>");

                    listViewEndpoints.Items.Add(item);
                }
            }

            void AddOutgoingEndpointToGrid(NtOutgoingEndpoint endpoint)
            {
                if (listViewEndpoints.InvokeRequired)
                {
                    listViewEndpoints.Invoke(AddOutgoingEndpointToGrid, endpoint);
                }
                else
                {
                    var item = new ListViewItem(endpoint.Name);
                    item.SubItems.Add("Outgoing");
                    item.SubItems.Add($"{endpoint.Address}{endpoint.Port}");

                    listViewEndpoints.Items.Add(item);

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
