using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormConnectTunnel : Form
    {
        private readonly ServiceClient? _client;
        private readonly DelegateLogger _delegateLogger = UIUtility.CreateActiveWindowMessageBoxLogger(NtLogSeverity.Exception);

        public FormConnectTunnel()
        {
            InitializeComponent();
        }

        public FormConnectTunnel(ServiceClient client)
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

        private void buttonConnect_Click(object sender, EventArgs e)
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

                var tunnel = new TunnelConfiguration(_client.ServiceId, tunnelId, textBoxName.Text,
                    textBoxRemoteAddress.Text, textBoxManagementPort.ValueAs<int>(),
                    textBoxRemoteUsername.Text, Utility.ComputeSha256Hash(textBoxRemotePassword.Text));

                buttonConnect.InvokeEnableControl(false);
                buttonCancel.InvokeEnableControl(false);

                try
                {
                    //Just to test the login.
                    //TODO: If the connection fails, prompt if the user wants to still add the tunnel.
                    var remoteClient = ServiceClient.CreateConnectAndLogin(_delegateLogger, tunnel.Address,
                        tunnel.ManagementPort, tunnel.Username, tunnel.PasswordHash).ContinueWith(async x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox(x.Exception?.Message ?? "An unknown exception occurred.",
                                    Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonConnect.InvokeEnableControl(true);
                                buttonCancel.InvokeEnableControl(true);
                                return;
                            }

                            await _client.QueryCreateTunnel(tunnel);

                            this.InvokeClose(DialogResult.OK);
                        });

                    //ConfigureTunnelPair(remoteClient, tunnel, inboundTunnel);
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
    }
}
