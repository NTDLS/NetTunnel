using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.Persistence;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public string Address { get; private set; } = string.Empty;
        public string ServerURL { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        public FormLogin()
        {
            InitializeComponent();

            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;

            var preferences = LocalUserApplicationData.LoadFromDisk(Constants.FriendlyName, new UILoginPreferences());

            textBoxAddress.Text = preferences.Address;
            textBoxPort.Text = preferences.Port;
            textBoxUsername.Text = preferences.Username;

#if DEBUG
            textBoxPassword.Text = "123456789";
#endif
            textBoxPassword.Focus();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            buttonLogin.InvokeEnableControl(false);
            buttonCancel.InvokeEnableControl(false);

            try
            {
                int port = textBoxPort.GetAndValidateNumeric(1, 65535, "A port number between [min] and [max] is required.");
                Username = textBoxUsername.GetAndValidateText("A username is required.");
                PasswordHash = Utility.ComputeSha256Hash(textBoxPassword.Text);
                Address = textBoxAddress.GetAndValidateText("A hostname or IP address is required.");

                var client = ClientFactory.Establish(new NtServiceConfiguration(), Address, port, Username, PasswordHash);

                var preferences = new UILoginPreferences()
                {
                    Address = textBoxAddress.Text,
                    Port = textBoxPort.Text,
                    Username = textBoxUsername.Text,
                };

                LocalUserApplicationData.SaveToDisk(Constants.FriendlyName, preferences);
                this.InvokeClose(DialogResult.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
                buttonLogin.InvokeEnableControl(true);
                buttonCancel.InvokeEnableControl(true);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            using var form = new FormAbout();
            form.ShowDialog();
        }
    }
}
