using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NetTunnel.UI.Types;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEditUser : Form
    {
        private readonly ServiceClient? _client;
        public User? User { get; private set; }
        private readonly bool _isCreatingNewUser;

        public FormAddEditUser()
        {
            InitializeComponent();
        }

        public FormAddEditUser(ServiceClient? client, User? user = null)
        {
            InitializeComponent();

            _client = client;
            _isCreatingNewUser = user == null;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelUsername, textBoxUsername],
                "The user name which the user will use when logging in to the NetTunnel service.");

            toolTips.AddControls([labelPassword, textBoxPassword, labelConfirmPassword, textBoxConfirmPassword],
                "The password which the user will use when logging in to the NetTunnel service.");

            #endregion

            User = user ?? new User();

            textBoxUsername.Text = User.Username;
            textBoxUsername.Enabled = !_isCreatingNewUser;
            checkBoxAdministrator.Checked = User.Role == NtUserRole.Administrator;

            Text = _isCreatingNewUser ? $"{FriendlyName} : Create User" : $"{FriendlyName} : Edit User";

            listViewEndpoints.MouseDoubleClick += ListViewEndpoint_MouseDoubleClick;

            foreach (var endpoint in User.Endpoints)
            {
                var item = new ListViewItem(endpoint.Name)
                {
                    Tag = new EndpointTag(endpoint)
                };
                item.SubItems.Add($"{endpoint.Direction}");

                if (endpoint.Direction == NtDirection.Inbound)
                {
                    item.SubItems.Add($"*:{endpoint.InboundPort}");
                }
                else
                {
                    item.SubItems.Add($"{endpoint.OutboundAddress}:{endpoint.OutboundPort}");
                }

                listViewEndpoints.Items.Add(item);
            }

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void ListViewEndpoint_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            _client.EnsureNotNull();

            if (e.Button == MouseButtons.Left)
            {
                listViewEndpoints.SelectedItems.Clear();

                var itemUnderMouse = listViewEndpoints.GetItemAt(e.X, e.Y);
                if (itemUnderMouse != null)
                {
                    itemUnderMouse.Selected = true;

                    var eTag = EndpointTag.FromItem(itemUnderMouse);

                    using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), eTag.Endpoint);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        eTag.Endpoint = form.Endpoint;
                    }
                }
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                string username = textBoxUsername.GetAndValidateText("You must specify a username.");

                string plainTextPassword = textBoxPassword.Text.Trim();
                string? passwordHash = null;

                if (string.IsNullOrEmpty(plainTextPassword))
                {
                    passwordHash = null;
                }
                else
                {
                    if (plainTextPassword != textBoxConfirmPassword.Text)
                    {
                        throw new Exception("The password and confirm-passwords must match.");
                    }

                    passwordHash = Utility.ComputeSha256Hash(plainTextPassword);
                }

                var progressForm = new ProgressForm(FriendlyName, "Saving user...");

                progressForm.Execute(() =>
                {
                    try
                    {
                        User = new User(username, passwordHash, checkBoxAdministrator.Checked ? NtUserRole.Administrator : NtUserRole.Limited);

                        if (_isCreatingNewUser)
                        {
                            _client.UIQueryCreateUser(User);
                        }
                        else
                        {
                            _client.UIQueryEditUser(User);
                        }

                        /* //Add endpoints, make this a list and add them all at once.
                        if (string.IsNullOrEmpty(_username) == false)
                        {
                            //If _tunnel is != null, then we are editing an endpoint for a user..

                            progressForm.Execute(() =>
                            {
                                try
                                {
                                    _client.UIQueryUpsertUserEndpoint(_username, endpoint);
                                    this.InvokeClose(DialogResult.OK);
                                }
                                catch (Exception ex)
                                {
                                    progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                }
                            });
                        }
                        */

                        this.InvokeClose(DialogResult.OK);
                    }
                    catch (Exception ex)
                    {
                        progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonAddInbound_Click(object sender, EventArgs e)
        {
            using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), NtDirection.Inbound);
            form.ShowDialog();
        }

        private void ButtonAddOutbound_Click(object sender, EventArgs e)
        {
            using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), NtDirection.Outbound);
            form.ShowDialog();
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            //TODO:
            throw new NotImplementedException();
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            var selectedItem = listViewEndpoints.SelectedItems.Count == 1 ? listViewEndpoints.SelectedItems[0] : null;
            if (selectedItem != null)
            {
                var eTag = EndpointTag.FromItem(selectedItem);
                using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), eTag.Endpoint);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    eTag.Endpoint = form.Endpoint;
                }
            }
        }
    }
}
