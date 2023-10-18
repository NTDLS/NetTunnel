using NetTunnel.ClientAPI;

namespace NetTunnel.UI
{
    public partial class FormLogin : Form
    {
        public string Address { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public FormLogin()
        {
            InitializeComponent();

            textBoxAddress.Text = "127.0.0.1";
            textBoxPort.Text = "52845";

            textBoxUsername.Text = "admin";
            textBoxPassword.Text = "abcdefg";
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(textBoxPort.Text, out var port) == false)
                {
                    throw new Exception("Invalid port.");
                }

                Username = textBoxUsername.Text;
                Password = textBoxPassword.Text;

                Address = $"https://{textBoxAddress.Text}:{port}/";

                using var _ = new NtClient(Address, Username, Password);

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
