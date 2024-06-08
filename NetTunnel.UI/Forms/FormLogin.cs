using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NTDLS.Persistence;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public string Address { get; private set; } = string.Empty;
        public string ServerURL { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool UseSSL { get; private set; }
        public NtClient? Client { get; private set; }

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
            buttonLogin.ThreadSafeEnable(false);
            buttonCancel.ThreadSafeEnable(false);

            try
            {
                if (int.TryParse(textBoxPort.Text, out var port) == false)
                    throw new Exception("Invalid port.");

                Username = textBoxUsername.Text;
                Password = Utility.ComputeSha256Hash(textBoxPassword.Text);
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

                var client = new NtClient(ServerURL);

                client.Security.Login(Username, Password).ContinueWith(o =>
                {
                    buttonLogin.ThreadSafeEnable(true);
                    buttonCancel.ThreadSafeEnable(true);

                    if (!o.IsCompletedSuccessfully)
                    {
                        client.Dispose();
                        this.ThreadSafeMessageBox($"Login failed: {o.Exception?.Message}.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                    var preferences = new UILoginPreferences()
                    {
                        Address = textBoxAddress.Text,
                        Port = textBoxPort.Text,
                        Username = textBoxUsername.Text,
                        UseSSL = checkBoxUseSSL.Checked
                    };

                    LocalUserApplicationData.SaveToDisk(Constants.FriendlyName, preferences);

                    Client = client;

                    this.ThreadSafeClose(DialogResult.OK);
                });
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
