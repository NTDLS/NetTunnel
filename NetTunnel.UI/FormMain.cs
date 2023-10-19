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

                        _client.Endpoint.List().ContinueWith(t =>
                        {
                            foreach (var endpoint in t.Result.Collection)
                            {
                                AddEndpointToGrid(endpoint);
                            }
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


        void AddEndpointToGrid(NtEndpoint endpoint)
        {
            if (listViewEndpoints.InvokeRequired)
            {
                listViewEndpoints.Invoke(AddEndpointToGrid, endpoint);
            }
            else
            {
                // This code will execute on the UI thread.
                ListViewItem item = new ListViewItem(endpoint.Name);
                item.SubItems.Add(endpoint.Direction.ToString());
                item.SubItems.Add($"{endpoint.Port}");

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
