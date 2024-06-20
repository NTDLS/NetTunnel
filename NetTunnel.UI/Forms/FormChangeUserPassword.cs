using NetTunnel.Library;
using NetTunnel.Service;
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

            User = user;

            textBoxUsername.Text = user.Username;

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                if (textBoxUsername.Text.Length == 0)
                    throw new Exception("You must specify a username.");
                if (textBoxPassword.Text.Length == 0)
                    throw new Exception("You must specify a password.");
                if (textBoxPassword.Text != textBoxConfirmPassword.Text)
                    throw new Exception("The password and confirm-passwords must match.");

                var user = new User(textBoxUsername.Text, Utility.ComputeSha256Hash(textBoxPassword.Text));

                buttonSave.InvokeEnableControl(false);
                /*
                _client.Security.ChangeUserPassword(user).ContinueWith(t =>
                 {
                     if (!t.IsCompletedSuccessfully)
                     {
                         this.InvokeMessageBox("Failed to change user password.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                         buttonSave.InvokeEnableControl(true);
                         return;
                     }

                     this.InvokeMessageBox("The password has been changed.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                     this.InvokeClose(DialogResult.OK);
                 });
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
