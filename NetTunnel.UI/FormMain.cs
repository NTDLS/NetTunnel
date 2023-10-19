using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;

namespace NetTunnel.UI
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

                        _client.IncommingEndpoint.List().ContinueWith(t =>
                        {
                            t.Result.Collection.ForEach(t => AddIncommingEndpointToGrid(t));
                        });

                        _client.OutgoingEndpoint.List().ContinueWith(t =>
                        {
                            t.Result.Collection.ForEach(t => AddOutgoingEndpointToGrid(t));
                        });

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
            using (var FormUsers = new FormUsers(_client))
            {
                FormUsers.ShowDialog();
            }
        }

        #endregion
    }
}
