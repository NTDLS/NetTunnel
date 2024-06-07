using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NTDLS.Persistence;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public string Address { get; set; } = string.Empty;
        public string ServerURL { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSSL { get; set; }

        public FormLogin()
        {
            InitializeComponent();

            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;

            var preferences = LocalUserApplicationData.LoadFromDisk(Constants.FriendlyName, new UILoginPreferences());

            textBoxAddress.Text = preferences.Address;
            textBoxPort.Text = preferences.Port;
            textBoxUsername.Text = preferences.Username;
            checkBoxUseSSL.Checked = preferences.UseSSL;

#if DEBUG
            textBoxPassword.Text = "123456789";
#endif
            textBoxPassword.Focus();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(textBoxPort.Text, out var port) == false)
                    throw new Exception("Invalid port.");

                Username = textBoxUsername.Text;
                Password = Utility.CalculateSHA256(textBoxPassword.Text);
                UseSSL = checkBoxUseSSL.Checked;
                Address = textBoxAddress.Text;

                if (UseSSL)
                {
                    ServerURL = $"https://{textBoxAddress.Text}:{port}/";
                }
                else
                {
                    ServerURL = $"http://{textBoxAddress.Text}:{port}/";
                }

                using var _ = new NtClient(ServerURL, Username, Password);

                var preferences = new UILoginPreferences()
                {
                    Address = textBoxAddress.Text,
                    Port = textBoxPort.Text,
                    Username = textBoxUsername.Text,
                    UseSSL = checkBoxUseSSL.Checked
                };

                LocalUserApplicationData.SaveToDisk(Constants.FriendlyName, preferences);

                DialogResult = DialogResult.OK;
                Close();
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

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            using var form = new FormAbout();
            form.ShowDialog();
        }
    }
}
