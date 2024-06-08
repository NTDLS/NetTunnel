using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.UI.Helpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormCreateOutboundTunnel : Form
    {
        private readonly NtClient? _client;
        private readonly ToolTip _toolTips = ToolTipHelpers.CreateToolTipControl();

        public FormCreateOutboundTunnel()
        {
            InitializeComponent();
        }

        public FormCreateOutboundTunnel(NtClient client)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            ToolTipHelpers.SetToolTip(_toolTips, [labelRemoteAddress, textBoxRemoteAddress],
                    "The IP address or hostname of the remote TunnelService instance. This is used so that this instance can reach out to the remote TunnelService instance and configure an inbound tunnel for this outbound tunnel.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelRemoteUsername, textBoxRemoteUsername],
                    "The username at the remote TunnelService instance.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelRemotePassword, textBoxRemotePassword],
                    "The password for the specified user name. This user and password need to exist at the remote TunnelService instance.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelManagementPort, textBoxManagementPort],
                    "The management port of the remote TunnelService. This is used so that this instance can reach out to the remote TunnelService instance and configure an inbound tunnel for this outbound tunnel.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelName, textBoxName],
                    "The user friendly name of this tunnel.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelTunnelDataPort, textBoxTunnelDataPort],
                    "The port which the tunnel will use to listen and transmit data. This outbound tunnel will reach out to the specified remote TunnelService on the specified 'Management Port' and ask it to create a corresponding inbound tunnel for this outbound tunnel. The remote inbound tunnel will be configured to listen on this port and this outbound tunnel will make the outbound connection to it.");

            #endregion

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxManagementPort.Text = $"{client?.BaseAddress?.Port}"; //The port that is used to manage the remote tunnel.

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
                if (textBoxManagementPort.Text.Length == 0 || int.TryParse(textBoxManagementPort.Text, out var _) == false)
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
                    textBoxRemoteAddress.Text, int.Parse(textBoxManagementPort.Text), int.Parse(textBoxTunnelDataPort.Text),
                    textBoxRemoteUsername.Text, Utility.ComputeSha256Hash(textBoxRemotePassword.Text));

                var inboundTunnel = new NtTunnelInboundConfiguration(tunnelId, textBoxName.Text, int.Parse(textBoxTunnelDataPort.Text));

                NtClient remoteClient;

                buttonAdd.ThreadSafeEnable(false);
                buttonCancel.ThreadSafeEnable(false);

                try
                {
                    remoteClient = new NtClient($"https://{outboundTunnel.Address}:{outboundTunnel.ManagementPort}/");
                }
                catch (Exception ex)
                {
                    buttonAdd.ThreadSafeEnable(true);
                    buttonCancel.ThreadSafeEnable(true);
                    throw new Exception($"Failed to connect to the remote tunnel: {ex.Message}.");
                }

                try
                {
                    //Log into the remote tunnel.
                    remoteClient.Security.Login(outboundTunnel.Username, outboundTunnel.PasswordHash).ContinueWith(o =>
                    {
                        buttonAdd.ThreadSafeEnable(true);
                        buttonCancel.ThreadSafeEnable(true);

                        if (!o.IsCompletedSuccessfully)
                        {
                            remoteClient.Dispose();
                            this.ThreadSafeMessageBox($"Login failed: {o.Exception?.Message}.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return;
                        }

                        ConfigureTunnelPair(remoteClient, outboundTunnel, inboundTunnel);

                        this.ThreadSafeClose(DialogResult.OK);
                    });

                }
                catch (Exception ex)
                {
                    buttonAdd.ThreadSafeEnable(true);
                    throw new Exception($"Failed to login to the remote tunnel: {ex.Message}.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }

        public void ConfigureTunnelPair(NtClient remoteClient, NtTunnelOutboundConfiguration outboundTunnel, NtTunnelInboundConfiguration inboundTunnel)
        {
            //Add the outbound tunnel config to the local tunnel instance.
            _client.EnsureNotNull().TunnelOutbound.Add(outboundTunnel).ContinueWith(t =>
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

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
