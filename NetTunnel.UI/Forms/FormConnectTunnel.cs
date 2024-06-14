using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormConnectTunnel : Form
    {
        private readonly NtServiceClient? _client;

        public FormConnectTunnel()
        {
            InitializeComponent();
        }

        public FormConnectTunnel(NtServiceClient client)
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

            toolTips.AddControls([labelPort, textBoxManagementPort],
                    "The management port of the remote TunnelService. This is used so that this instance can reach out to the remote TunnelService instance and configure an inbound tunnel for this outbound tunnel.");

            toolTips.AddControls([labelName, textBoxName],
                    "The user friendly name of this tunnel.");


            #endregion

            AcceptButton = buttonConnect;
            CancelButton = buttonCancel;

            textBoxManagementPort.Text = "52845"; //TODO: This should be stored in preferences.

#if DEBUG
            textBoxName.Text = "My First Tunnel";

            textBoxRemoteAddress.Text = "10.20.1.120";

            textBoxRemoteUsername.Text = "debug";
            textBoxRemotePassword.Text = "123456789";
#endif
        }

        private void FormConnectTunnel_Load(object sender, EventArgs e)
        {
        }

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

                var tunnelId = Guid.NewGuid(); //The TunnelId is the same on both services.

                var outboundTunnel = new NtTunnelOutboundConfiguration(tunnelId, textBoxName.Text,
                    textBoxRemoteAddress.Text, textBoxManagementPort.ValueAs<int>(),
                    textBoxRemoteUsername.Text, Utility.ComputeSha256Hash(textBoxRemotePassword.Text));

                buttonConnect.InvokeEnableControl(false);
                buttonCancel.InvokeEnableControl(false);

                try
                {
                    var remoteClient = NtServiceClient.CreateAndLogin(outboundTunnel.Address,
                        outboundTunnel.ManagementPort, outboundTunnel.Username, outboundTunnel.PasswordHash);


                    //ConfigureTunnelPair(remoteClient, outboundTunnel, inboundTunnel);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to login to the remote tunnel: {ex.Message}.");
                }
            }
            catch (Exception ex)
            {
                buttonConnect.InvokeEnableControl(true);
                buttonCancel.InvokeEnableControl(true);

                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }

        public void ConfigureTunnelPair(NtClient remoteClient, NtTunnelOutboundConfiguration outboundTunnel)
        {
            /*
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
            */
        }
    }
}
