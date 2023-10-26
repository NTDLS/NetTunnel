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

            listViewUsers.MouseUp += ListViewUsers_MouseUp;
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
                item.Tag = user;
                listViewUsers.Items.Add(item);
            }
        }

        private void ListViewUsers_MouseUp(object? sender, MouseEventArgs e)
        {
            Utility.EnsureNotNull(_client);

            if (e.Button == MouseButtons.Right)
            {
                var itemUnderMouse = listViewUsers.GetItemAt(e.X, e.Y);
                if (itemUnderMouse != null)
                {
                    itemUnderMouse.Selected = true;
                }

                if (itemUnderMouse != null)
                {
                    var menu = new ContextMenuStrip();
                    menu.Items.Add("Change password");
                    if (listViewUsers.Items.Count > 1)
                    {
                        menu.Items.Add("Delete");
                    }
                    menu.Show(listViewUsers, new Point(e.X, e.Y));

                    menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                    {
                        if (e.ClickedItem?.Text == "Change password")
                        {
                            using (var formChangeUserPassword = new FormChangeUserPassword(_client, (NtUser)itemUnderMouse.Tag))
                            {
                                formChangeUserPassword.ShowDialog();
                            }
                        }
                        else if (e.ClickedItem?.Text == "Delete")
                        {
                            var user = (NtUser)itemUnderMouse.Tag;

                            if (MessageBox.Show($"Delete the user '{user.Username}'?",
                                Constants.FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                _client.Security.ChangeUserPassword(user).ContinueWith(t =>
                                {
                                    if (!t.IsCompletedSuccessfully)
                                    {
                                        throw new Exception("Failed to delete user.");
                                    }

                                    this.CloseFormWithResult(DialogResult.OK);
                                });
                            }
                        }
                        else if (e.ClickedItem?.Text == "Delete Tunnel")
                        {
                            MessageBox.Show("Not implemented");
                        }
                    };
                }
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
