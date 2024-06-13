using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;

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

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelRemoteAddress, textBoxRemoteAddress],
                        "The IP address or hostname of the remote TunnelService instance. This is used so that this instance can reach out to the remote TunnelService instance and configure an inbound tunnel for this outbound tunnel.");

            toolTips.AddControls([labelRemoteUsername, textBoxRemoteUsername],
                    "The username at the remote TunnelService instance.");

            toolTips.AddControls([labelRemotePassword, textBoxRemotePassword],
                    "The password for the specified user name. This user and password need to exist at the remote TunnelService instance.");

            toolTips.AddControls([labelManagementPort, textBoxManagementPort],
                    "The management port of the remote TunnelService. This is used so that this instance can reach out to the remote TunnelService instance and configure an inbound tunnel for this outbound tunnel.");

            toolTips.AddControls([labelName, textBoxName],
                    "The user friendly name of this tunnel.");

            toolTips.AddControls([labelTunnelDataPort, textBoxTunnelDataPort],
                    "The port which the tunnel will use to listen and transmit data. This outbound tunnel will reach out to the specified remote TunnelService on the specified 'Management Port' and ask it to create a corresponding inbound tunnel for this outbound tunnel. The remote inbound tunnel will be configured to listen on this port and this outbound tunnel will make the outbound connection to it.");

            #endregion

            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxManagementPort.Text = $"{client?.BaseAddress?.Port}"; //The port that is used to manage the remote tunnel.

#if DEBUG
            textBoxName.Text = "My First Tunnel";

            textBoxRemoteAddress.Text = "10.20.1.120";
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
                textBoxName.GetAndValidateText("You must specify a name. This is for your identification only.");
                textBoxRemoteAddress.GetAndValidateText("You must specify a remote tunnel address.");
                textBoxManagementPort.GetAndValidateNumeric(1, 65535, "You must specify a valid remote tunnel management port between [min] and [max].");
                textBoxRemoteUsername.GetAndValidateText("You must specify a remote tunnel username.");
                textBoxRemotePassword.GetAndValidateText("You must specify a valid remote tunnel password.");
                textBoxTunnelDataPort.GetAndValidateNumeric(1, 65535, "You must specify a tunnel data port between [min] and [max].");

                var tunnelId = Guid.NewGuid(); //The TunnelId is the same on both services.

                var outboundTunnel = new NtTunnelOutboundConfiguration(tunnelId, textBoxName.Text,
                    textBoxRemoteAddress.Text, textBoxManagementPort.ValueAs<int>(), textBoxTunnelDataPort.ValueAs<int>(),
                    textBoxRemoteUsername.Text, Utility.ComputeSha256Hash(textBoxRemotePassword.Text));

                var inboundTunnel = new NtTunnelInboundConfiguration(tunnelId, textBoxName.Text, textBoxTunnelDataPort.ValueAs<int>());

                buttonAdd.InvokeEnableControl(false);
                buttonCancel.InvokeEnableControl(false);

                NtClient remoteClient;

                try
                {
                    remoteClient = new NtClient($"https://{outboundTunnel.Address}:{outboundTunnel.ManagementPort}/");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to connect to the remote tunnel: {ex.Message}.");
                }

                try
                {
                    //Log into the remote tunnel.
                    remoteClient.Security.Login(outboundTunnel.Username, outboundTunnel.PasswordHash).ContinueWith(o =>
                    {
                        if (!o.IsCompletedSuccessfully)
                        {
                            remoteClient.Dispose();
                            this.InvokeMessageBox($"Login failed: {o.Exception?.Message}.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                            buttonAdd.InvokeEnableControl(true);
                            buttonCancel.InvokeEnableControl(true);

                            return;
                        }

                        ConfigureTunnelPair(remoteClient, outboundTunnel, inboundTunnel);

                        this.InvokeClose(DialogResult.OK);
                    });

                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to login to the remote tunnel: {ex.Message}.");
                }
            }
            catch (Exception ex)
            {
                buttonAdd.InvokeEnableControl(true);
                buttonCancel.InvokeEnableControl(true);

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
                    buttonAdd.InvokeEnableControl(true);
                    this.InvokeMessageBox("Failed to create local outbound tunnel.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
                            buttonAdd.InvokeEnableControl(true);
                            this.InvokeMessageBox("Failed to create remote inbound tunnel.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return;
                        });
                    }

                    //Start the listening tunnel:
                    remoteClient.TunnelInbound.Start(inboundTunnel.TunnelId).Wait();

                    //Start the outbound-connecting tunnel:
                    _client.TunnelOutbound.Start(outboundTunnel.TunnelId).Wait();

                    this.InvokeClose(DialogResult.OK);
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
