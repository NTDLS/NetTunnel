using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NetTunnel.UI.Types;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormEditUser : Form
    {
        private readonly ServiceClient? _client;
        public User? User { get; private set; }

        public FormEditUser()
        {
            InitializeComponent();
        }

        public FormEditUser(ServiceClient? client, User user)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelUsername, textBoxUsername],
                "The user name which the user will use when logging in to the NetTunnel service.");

            toolTips.AddControls([labelPassword, textBoxPassword, labelConfirmPassword, textBoxConfirmPassword],
                "The password which the user will use when logging in to the NetTunnel service.");

            #endregion

            User = user;

            textBoxUsername.Text = user.Username;
            checkBoxAdministrator.Checked = user.Role == NtUserRole.Administrator;

            listViewEndpoints.MouseDoubleClick += ListViewEndpoint_MouseDoubleClick;

            foreach (var endpoint in user.Endpoints)
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
                    form.ShowDialog();
                }
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                string username = textBoxUsername.GetAndValidateText("You must specify a username.");
                string password = Utility.ComputeSha256Hash(textBoxPassword.GetAndValidateText("You must specify a password."));
                string passwordConfirm = Utility.ComputeSha256Hash(textBoxConfirmPassword.GetAndValidateText("You must specify a confirm-password."));

                if (password != passwordConfirm)
                {
                    throw new Exception("The password and confirm-passwords must match.");
                }

                var progressForm = new ProgressForm(Constants.FriendlyName, "Saving user...");

                var result = progressForm.Execute(() =>
                {
                    try
                    {
                        var user = new User(username, password, checkBoxAdministrator.Checked ? NtUserRole.Administrator : NtUserRole.Limited);
                        _client.UIQueryEditUser(user);

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


                        return true;
                    }
                    catch (Exception ex)
                    {
                        progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    return false;
                });

                if (result)
                {
                    this.InvokeMessageBox("The password has been changed.",
                        FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Close();
                }
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

        private void linkLabelEditEndpoints_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void buttonAddInbound_Click(object sender, EventArgs e)
        {
            using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), NtDirection.Inbound);
            form.ShowDialog();
        }

        private void buttonAddOutbound_Click(object sender, EventArgs e)
        {
            using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), NtDirection.Outbound);
            form.ShowDialog();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {

        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            var selectedItem = listViewEndpoints.SelectedItems.Count == 1 ? listViewEndpoints.SelectedItems[0] : null;
            if (selectedItem != null)
            {
                var eTag = EndpointTag.FromItem(selectedItem);
                using var form = new FormAddEditEndpoint(_client.EnsureNotNull(), eTag.Endpoint);
                form.ShowDialog();
            }
        }
    }
}
