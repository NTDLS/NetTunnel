using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;

namespace NetTunnel.UI.Forms
{
    public partial class FormCreateOutboundTunnel : Form
    {
        private readonly NtClient? _client;

        public FormCreateOutboundTunnel()
        {
            InitializeComponent();
        }

        public FormCreateOutboundTunnel(NtClient client)
        {
            InitializeComponent();

            _client = client;

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxRemotePort.Text = $"{client?.BaseAddress?.Port}"; //The port that is used to manage the remote tunnel.

#if DEBUG
            textBoxName.Text = "My First Tunnel";

            textBoxRemoteAddress.Text = "127.0.0.1";
            textBoxTunnelDataPort.Text = "52846"; //This is the port that is used to move tunnel data between tunnels

            textBoxRemoteUsername.Text = "debug";
            textBoxRemotePassword.Text = "123456789";
#endif
        }

        private void FormCreateOutboundTunnel_Load(object sender, EventArgs e) { }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                if (textBoxName.Text.Length == 0)
                    throw new Exception("You must specify a name This is for your identification only.");
                if (textBoxRemoteAddress.Text.Length == 0)
                    throw new Exception("You must specify a remote tunnel address.");
                if (textBoxRemotePort.Text.Length == 0 || int.TryParse(textBoxRemotePort.Text, out var _) == false)
                    throw new Exception("You must specify a valid remote tunnel port.");
                if (textBoxRemoteUsername.Text.Length == 0)
                    throw new Exception("You must specify a remote tunnel username.");
                if (textBoxRemotePassword.Text.Length == 0)
                    throw new Exception("You must specify a valid remote tunnel password.");
                if (textBoxTunnelDataPort.Text.Length == 0 || int.TryParse(textBoxTunnelDataPort.Text, out var _) == false)
                    throw new Exception("You must specify a valid tunnel data port.");

                buttonAdd.ThreadSafeEnable(false);

                var tunnelId = Guid.NewGuid(); //The TunnelId is the same on both services.

                var outboundTunnel = new NtTunnelOutboundConfiguration(tunnelId, textBoxName.Text,
                    textBoxRemoteAddress.Text, int.Parse(textBoxRemotePort.Text), int.Parse(textBoxTunnelDataPort.Text),
                    textBoxRemoteUsername.Text, Utility.CalculateSHA256(textBoxRemotePassword.Text));

                var inboundTunnel = new NtTunnelInboundConfiguration(tunnelId, textBoxName.Text, int.Parse(textBoxTunnelDataPort.Text));

                NtClient remoteClient;

                try
                {
                    //Connect to the remote tunnel.
                    remoteClient = new NtClient($"https://{outboundTunnel.Address}:{outboundTunnel.ManagementPort}/");
                }
                catch (Exception ex)
                {
                    buttonAdd.ThreadSafeEnable(true);
                    throw new Exception($"Failed to connect to the remote tunnel: {ex.Message}.");
                }

                try
                {
                    //Log into the remote tunnel.
                    remoteClient.Security.Login(outboundTunnel.Username, outboundTunnel.PasswordHash);
                }
                catch (Exception ex)
                {
                    buttonAdd.ThreadSafeEnable(true);
                    throw new Exception($"Failed to login to the remote tunnel: {ex.Message}.");
                }

                //Add the outbound tunnel config to the local tunnel instance.
                _client.TunnelOutbound.Add(outboundTunnel).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        buttonAdd.ThreadSafeEnable(true);
                        this.ThreadSafeMessageBox("Failed to create local outbound tunnel.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    //Add the inbound tunnel config to the remote tunnel instance.
                    remoteClient.TunnelInbound.Add(inboundTunnel).ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully)
                        {
                            //If we failed to create the remote tunnel config, remove the local config.
                            _client.TunnelOutbound.Delete(outboundTunnel.TunnelId).ContinueWith(t =>
                            {
                                buttonAdd.ThreadSafeEnable(true);
                                this.ThreadSafeMessageBox("Failed to create remote inbound tunnel.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                return;
                            });
                        }

                        //Start the listening tunnel:
                        remoteClient.TunnelInbound.Start(inboundTunnel.TunnelId).Wait();

                        //Start the outbound-connecting tunnel:
                        _client.TunnelOutbound.Start(outboundTunnel.TunnelId).Wait();

                        this.ThreadSafeClose(DialogResult.OK);
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
            this.Close();
        }
    }
}
