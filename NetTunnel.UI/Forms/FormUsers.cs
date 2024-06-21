using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NetTunnel.UI.Types;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormUsers : Form
    {
        private readonly ServiceClient? _client;
        private bool _firstShown = true;

        public FormUsers()
        {
            InitializeComponent();
        }

        public FormUsers(ServiceClient client)
        {
            InitializeComponent();
            _client = client;
            listViewUsers.MouseUp += ListViewUsers_MouseUp;
            Shown += FormUsers_Shown;
        }

        private void FormUsers_Shown(object? sender, EventArgs e)
        {
            if (!_firstShown)
            {
                return;
            }
            _firstShown = false;

            var progressForm = new ProgressForm(Constants.FriendlyName, "Getting users...");

            progressForm.Execute(() =>
            {

                try
                {
                    var result = _client.EnsureNotNull().QueryGetUsers();

                    result.Collection.ForEach(u => AddUserToGrid(u));
                }
                catch (Exception ex)
                {
                    progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            });
        }

        private void AddUserToGrid(User user)
        {
            if (listViewUsers.InvokeRequired)
            {
                listViewUsers.Invoke(AddUserToGrid, user);
            }
            else
            {
                var item = new ListViewItem(user.Username);
                item.Tag = new UserTag(user);
                listViewUsers.Items.Add(item);
            }
        }

        private void ListViewUsers_MouseUp(object? sender, MouseEventArgs e)
        {
            _client.EnsureNotNull();

            if (e.Button == MouseButtons.Right)
            {
                listViewUsers.SelectedItems.Clear();

                var itemUnderMouse = listViewUsers.GetItemAt(e.X, e.Y);
                if (itemUnderMouse != null)
                {
                    itemUnderMouse.Selected = true;
                }

                var uTag = UserTag.FromItemOrDefault(itemUnderMouse);

                if (uTag != null)
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
                        menu.Hide();

                        if (e.ClickedItem?.Text == "Change password")
                        {
                            using var formChangeUserPassword = new FormChangeUserPassword(_client, uTag.User);
                            formChangeUserPassword.ShowDialog();
                        }
                        else if (e.ClickedItem?.Text == "Delete")
                        {
                            if (MessageBox.Show($"Delete the user '{uTag.User.Username}'?",
                                Constants.FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            {
                                return;
                            }

                            var progressForm = new ProgressForm(Constants.FriendlyName, "Deleting user...");

                            progressForm.Execute(() =>
                            {
                                try
                                {
                                    _client.QueryDeleteUser(uTag.User.Username);
                                    listViewUsers.InvokeDeleteSelectedItems();
                                }
                                catch (Exception ex)
                                {
                                    progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                }
                            });
                        }
                    };
                }
            }
        }

        private void ButtonAddUser_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            using var formAddUser = new FormAddUser(_client);
            if (formAddUser.ShowDialog() == DialogResult.OK)
            {
                AddUserToGrid(formAddUser.CreatedUser.EnsureNotNull());
            }
        }
    }
}
