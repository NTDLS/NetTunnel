using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormChangeUserPassword : Form
    {
        private readonly ServiceClient? _client;
        public User? User { get; private set; }

        public FormChangeUserPassword()
        {
            InitializeComponent();
        }

        public FormChangeUserPassword(ServiceClient? client, User user)
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

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
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

                var progressForm = new ProgressForm(Constants.FriendlyName, "Changing password...");

                var result = progressForm.Execute(() =>
                {
                    try
                    {
                        _client.QueryChangeUserPassword(username, password);
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
                        Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Information);

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
    }
}
