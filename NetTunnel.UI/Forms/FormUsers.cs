using NetTunnel.Library;
using NetTunnel.Service;
using NTDLS.NullExtensions;

namespace NetTunnel.UI.Forms
{
    public partial class FormUsers : Form
    {
        private readonly NtServiceClient? _client;

        public FormUsers()
        {
            InitializeComponent();
        }

        public FormUsers(NtServiceClient client)
        {
            InitializeComponent();
            _client = client;

            listViewUsers.MouseUp += ListViewUsers_MouseUp;
        }

        private void FormUsers_Load(object sender, EventArgs e)
        {
            /*
            _client.EnsureNotNull().Security.ListUsers().ContinueWith(t =>
            {
                foreach (var user in t.Result.Collection)
                {
                    AddUserToGrid(user);
                }
            });
            */
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
            /*
            _client.EnsureNotNull();

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
                            var user = ((NtUser?)itemUnderMouse.Tag).EnsureNotNull();

                            using (var formChangeUserPassword = new FormChangeUserPassword(_client, user))
                            {
                                formChangeUserPassword.ShowDialog();
                            }
                        }
                        else if (e.ClickedItem?.Text == "Delete")
                        {
                            var user = ((NtUser?)itemUnderMouse.Tag).EnsureNotNull();

                            if (MessageBox.Show($"Delete the user '{user.Username}'?",
                                Constants.FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                _client.Security.ChangeUserPassword(user).ContinueWith(t =>
                                {
                                    if (!t.IsCompletedSuccessfully)
                                    {
                                        this.InvokeMessageBox("Failed to delete user.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                        return;
                                    }

                                    this.InvokeClose(DialogResult.OK);
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
            */
        }

        private void buttonAddUser_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

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
