using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Service;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddUser : Form
    {
        private readonly NtClient? _client;
        public NtUser? CreatedUser { get; set; }

        public FormAddUser()
        {
            InitializeComponent();
        }

        public FormAddUser(NtClient? client)
        {
            InitializeComponent();

            _client = client;

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

            try
            {
                if (textBoxUsername.Text.Length == 0)
                    throw new Exception("You must specify a username.");
                if (textBoxPassword.Text.Length < 8)
                    throw new Exception("You must specify a password of at least 8 characters.");
                if (textBoxPassword.Text != textBoxConfirmPassword.Text)
                    throw new Exception("The password and confirm-passwords must match.");

                CreatedUser = new NtUser(textBoxUsername.Text, Utility.CalculateSHA256(textBoxPassword.Text));

                this.EnableControl(buttonSave, false);

                _client.Security.CreateUser(CreatedUser).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        this.EnableControl(buttonSave, true);
                        throw new Exception("Failed to create new user.");
                    }

                    this.CloseFormWithResult(DialogResult.OK);
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
    }
}
