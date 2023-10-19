using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;
using NetTunnel.UI.Forms;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEndpoint : BaseForm
    {
        private readonly NtClient? _client;

        public FormAddEndpoint()
        {
            InitializeComponent();

        }

        public FormAddEndpoint(NtClient client)
        {
            InitializeComponent();

            _client = client;

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxName.Text = "My endpoint";

            textBoxRemoteAddress.Text = "127.0.0.1";
            textBoxRemotePort.Text = "52845";

            textBoxRemoteUsername.Text = "admin";
            textBoxRemotePassword.Text = "abcdefgh";
        }

        private void FormAddEndpoint_Load(object sender, EventArgs e) { }


        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

            try
            {

                if (textBoxName.Text.Length == 0)
                    throw new Exception("You must specify a name This is for your identification only.");
                if (textBoxRemoteAddress.Text.Length == 0)
                    throw new Exception("You must specify a remote endpoint address.");
                if (textBoxRemotePort.Text.Length == 0 || int.TryParse(textBoxRemotePort.Text, out var _) == false)
                    throw new Exception("You must specify a valid remote endpoint port.");
                if (textBoxRemoteUsername.Text.Length == 0)
                    throw new Exception("You must specify a remote endpoint username.");
                if (textBoxRemotePassword.Text.Length < 8)
                    throw new Exception("You must specify a valid remote endpoint password (8 character or more).");

                EnableControl(buttonAdd, false);

                var outgoingEndpoint = new NtOutgoingEndpoint()
                {
                    Name = textBoxName.Text,
                    Address = textBoxRemoteAddress.Text,
                    Port = int.Parse(textBoxRemotePort.Text),
                };

                var incommingEndpoint = new NtIncommingEndpoint()
                {
                    Name = textBoxName.Text,
                    Username = textBoxRemoteUsername.Text,
                    PasswordHash = Utility.CalculateSHA256(textBoxRemotePassword.Text),
                };

                NtClient remoteClient;

                try
                {
                    remoteClient = new NtClient($"https://{outgoingEndpoint.Address}:{outgoingEndpoint.Port}/");
                }
                catch (Exception ex)
                {
                    EnableControl(buttonAdd, false);
                    throw new Exception($"Failed to connect to the remote endpoint: {ex.Message}.");
                }

                try
                {
                    remoteClient.Security.Login(incommingEndpoint.Username, incommingEndpoint.PasswordHash);
                }
                catch (Exception ex)
                {
                    EnableControl(buttonAdd, false);
                    throw new Exception($"Failed to login to the remote endpoint: {ex.Message}.");
                }

                _client.OutgoingEndpoint.Add(outgoingEndpoint).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        EnableControl(buttonAdd, false);
                        throw new Exception("Failed to create local outgoing endpoint.");
                    }

                    remoteClient.IncommingEndpoint.Add(incommingEndpoint).ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully)
                        {
                            EnableControl(buttonAdd, false);
                            throw new Exception("Failed to create remote incomming endpoint.");
                        }

                        CloseFormWithResult(DialogResult.OK);
                    });

                    CloseFormWithResult(DialogResult.OK);
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
