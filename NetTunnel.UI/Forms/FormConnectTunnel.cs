using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
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

                var progressForm = new ProgressForm(FriendlyName, "Logging in to remote tunnel...");

                try
                {
                    progressForm.Execute(() =>
                    {
                        try
                        {
                            var remoteClient = ServiceClient.CreateConnectAndLogin(_delegateLogger, tunnel.Address,
                                tunnel.ServicePort, tunnel.Username, tunnel.PasswordHash);

                            _client.QueryCreateTunnel(tunnel);

                            this.InvokeClose(DialogResult.OK);
                        }
                        catch (Exception ex)
                        {
                            if (progressForm.MessageBox(ex.Message + "\r\n\r\n" + "Would you like to add the tunnel anyway?",
                                FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                _client.QueryCreateTunnel(tunnel);
                                this.InvokeClose(DialogResult.OK);
                                return;
                            }
                        }
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
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}
