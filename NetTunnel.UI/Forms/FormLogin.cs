using NetTunnel.ClientAPI;
using NetTunnel.Library;

namespace NetTunnel.UI.Forms
{
    public partial class FormLogin : Form
    {
        public string Address { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public FormLogin()
        {
            InitializeComponent();

            AcceptButton = buttonLogin;
            CancelButton = buttonCancel;

            var preferences = Persistence.LoadFromDisk<UIPreferences>();
            if (preferences != null)
            {
                textBoxAddress.Text = preferences.Address;
                textBoxPort.Text = preferences.Port;
                textBoxUsername.Text = preferences.Username;
            }

#if DEBUG
            textBoxPassword.Text = Environment.MachineName.ToLower();
#endif
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(textBoxPort.Text, out var port) == false)
                    throw new Exception("Invalid port.");

                Username = textBoxUsername.Text;
                Password = Utility.CalculateSHA256(textBoxPassword.Text);

                Address = $"https://{textBoxAddress.Text}:{port}/";

                using var _ = new NtClient(Address, Username, Password);


                var preferences = new UIPreferences()
                {
                    Address = textBoxAddress.Text,
                    Port = textBoxPort.Text,
                    Username = textBoxUsername.Text,
                };

                Persistence.SaveToDisk(preferences);

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
    }
}
