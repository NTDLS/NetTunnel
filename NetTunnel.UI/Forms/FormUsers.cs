using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Service;

namespace NetTunnel.UI.Forms
{
    public partial class FormUsers : Form
    {
        private readonly NtClient? _client;

        public FormUsers()
        {
            InitializeComponent();
        }

        public FormUsers(NtClient client)
        {
            InitializeComponent();
            _client = client;
        }

        private void FormUsers_Load(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);
            _client.Security.ListUsers().ContinueWith(t =>
            {
                foreach (var user in t.Result.Collection)
                {
                    AddUserToGrid(user);
                }
            });
        }

        void AddUserToGrid(NtUser? user)
        {
            if (user == null) return;

            if (listViewUsers.InvokeRequired)
            {
                listViewUsers.Invoke(AddUserToGrid, user);
            }
            else
            {
                var item = new ListViewItem(user.Username);
                listViewUsers.Items.Add(item);
            }
        }

        private void buttonAddUser_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

            using (var formAddUser = new FormAddUser(_client))
            {
                if (formAddUser.ShowDialog() == DialogResult.OK)
                {
                    AddUserToGrid(formAddUser.CreatedUser);
                }
            }
        }
    }
}
