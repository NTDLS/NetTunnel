using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddTunnel : BaseForm
    {
        private readonly NtClient? _client;

        public FormAddTunnel()
        {
            InitializeComponent();
        }

        public FormAddTunnel(NtClient client)
        {
            InitializeComponent();

            _client = client;

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxName.Text = "My First Tunnel";

            textBoxRemoteAddress.Text = "127.0.0.1";
            textBoxRemotePort.Text = "52845"; //The port that is used to manage the remote endpoint.
            textBoxEndpointDataPort.Text = "52846"; //This is the port that is used to move tunnel data between endpoints

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
                if (textBoxEndpointDataPort.Text.Length == 0 || int.TryParse(textBoxEndpointDataPort.Text, out var _) == false)
                    throw new Exception("You must specify a valid endpoint data port.");

                EnableControl(buttonAdd, false);

                var outgoingEndpoint = new NtTunnelOutboundConfiguration(textBoxName.Text,
                    textBoxRemoteAddress.Text, int.Parse(textBoxRemotePort.Text), int.Parse(textBoxEndpointDataPort.Text),
                    textBoxRemoteUsername.Text, Utility.CalculateSHA256(textBoxRemotePassword.Text));

                var incomingEndpoint = new NtTunnelInboundConfiguration(textBoxName.Text, int.Parse(textBoxEndpointDataPort.Text));

                NtClient remoteClient;

                try
                {
                    //Connect to the remote endpoint.
                    remoteClient = new NtClient($"https://{outgoingEndpoint.Address}:{outgoingEndpoint.ManagementPort}/");
                }
                catch (Exception ex)
                {
                    EnableControl(buttonAdd, false);
                    throw new Exception($"Failed to connect to the remote endpoint: {ex.Message}.");
                }

                try
                {
                    //Log into the remote endpoint.
                    remoteClient.Security.Login(outgoingEndpoint.Username, outgoingEndpoint.PasswordHash);
                }
                catch (Exception ex)
                {
                    EnableControl(buttonAdd, false);
                    throw new Exception($"Failed to login to the remote endpoint: {ex.Message}.");
                }

                //Add the outgoing endpoint config to the local endpoint instance.
                _client.OutgoingTunnel.Add(outgoingEndpoint).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        EnableControl(buttonAdd, false);
                        throw new Exception("Failed to create local outgoing endpoint.");
                    }

                    //Add the incoming endpoint config to the remote endpoint instance.
                    remoteClient.IncomingTunnel.Add(incomingEndpoint).ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully)
                        {
                            //If we failed to create the remote endpoint config, remove the local config.
                            _client.OutgoingTunnel.Delete(outgoingEndpoint.Id).ContinueWith(t =>
                            {
                                EnableControl(buttonAdd, false);
                                throw new Exception("Failed to create remote incoming endpoint.");
                            });
                        }

                        CloseFormWithResult(DialogResult.OK);
                    });
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
