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
            listViewUsers.MouseDoubleClick += ListViewUsers_MouseDoubleClick;
            Shown += FormUsers_Shown;
            listViewUsers.FullRowSelect = true;

            var cancelButton = new Button();
            cancelButton.Click += (object? sender, EventArgs e) =>
            {
                Close();
            };

            CancelButton = cancelButton;
        }

        private void ListViewUsers_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            _client.EnsureNotNull();

            if (e.Button == MouseButtons.Left)
            {
                listViewUsers.SelectedItems.Clear();

                var selectedItem = listViewUsers.GetItemAt(e.X, e.Y);
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;

                    var uTag = UserTag.FromItem(selectedItem);
                    using var form = new FormAddEditUser(_client, uTag.User);
                    form.ShowDialog();
                }
            }
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
                    var result = _client.EnsureNotNull().UIQueryGetUsers();

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
                var subitem = item.SubItems.Add(user.Role.ToString());
                item.SubItems.Add($"{user.Endpoints.Count}");
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

                var selectedItem = listViewUsers.GetItemAt(e.X, e.Y);
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }

                var uTag = UserTag.FromItemOrDefault(selectedItem);
                var menu = new ContextMenuStrip();

                if (uTag != null)
                {
                    menu.Items.Add("Edit");
                    if (selectedItem != null)
                    {
                        menu.Items.Add("Delete");
                    }
                }
                else
                {
                    menu.Items.Add("Add new user");
                }

                menu.Show(listViewUsers, new Point(e.X, e.Y));

                menu.ItemClicked += (object? sender, ToolStripItemClickedEventArgs e) =>
                {
                    menu.Hide();

                    if (e.ClickedItem?.Text == "Add new user")
                    {
                        _client.EnsureNotNull();

                        using var form = new FormAddEditUser(_client);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            AddUserToGrid(form.User.EnsureNotNull());
                        }
                    }
                    else if (uTag != null && e.ClickedItem?.Text == "Edit")
                    {
                        using var form = new FormAddEditUser(_client, uTag.User);
                        form.ShowDialog();
                    }
                    else if (uTag != null && e.ClickedItem?.Text == "Delete")
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
                                _client.UIQueryDeleteUser(uTag.User.Username);
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

        private void ButtonCreateUser_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            using var form = new FormAddEditUser(_client);
            if (form.ShowDialog() == DialogResult.OK)
            {
                AddUserToGrid(form.User.EnsureNotNull());
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
