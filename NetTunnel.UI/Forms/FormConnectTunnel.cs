using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.Persistence;
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

            toolTips.AddControls([textBoxName],
                "The name or description you want to use to identify this tunnel.");

            toolTips.AddControls([labelAddress, textBoxAddress],
                "The host name, domain or IP address of the remote NetTunnel service you want the new tunnel to connect to.");

            toolTips.AddControls([labelPort, textBoxPort],
                "The port of the remote NetTunnel service you want the new tunnel to connect to.");

            toolTips.AddControls([labelUsername, textBoxUsername],
                "The username that will be used when connecting the tunnel to the remote NetTunnel service.");

            toolTips.AddControls([labelPassword, textBoxPassword],
                "The password that will be used when connecting the tunnel to the remote NetTunnel service.");

            #endregion

            AcceptButton = buttonConnect;
            CancelButton = buttonCancel;

            Exceptions.Ignore(() =>
            {
                var preferences = LocalUserApplicationData.LoadFromDisk(Constants.FriendlyName, new UILoginPreferences());
                textBoxPort.Text = preferences.Port;
            });

#if DEBUG
            textBoxName.Text = "My First Tunnel";

            textBoxAddress.Text = "10.20.1.120";

            textBoxUsername.Text = "debug";
            textBoxPassword.Text = "123456789";
#endif
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                textBoxName.GetAndValidateText("You must specify a name. This is for your identification only.");
                textBoxAddress.GetAndValidateText("You must specify a remote tunnel address.");
                textBoxPort.GetAndValidateNumeric(1, 65535, "You must specify a valid remote tunnel management port between [min] and [max].");
                textBoxUsername.GetAndValidateText("You must specify a remote tunnel username.");
                textBoxPassword.GetAndValidateText("You must specify a valid remote tunnel password.");

                var tunnelId = Guid.NewGuid(); //The TunnelId is the same on both services.

                var tunnel = new TunnelConfiguration(_client.ServiceId, tunnelId, textBoxName.Text,
                    textBoxAddress.Text, textBoxPort.ValueAs<int>(),
                    textBoxUsername.Text, Utility.ComputeSha256Hash(textBoxPassword.Text));

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
                                try
                                {
                                    _client.QueryCreateTunnel(tunnel);
                                }
                                catch (Exception ex2)
                                {
                                    progressForm.MessageBox(ex2.Message, FriendlyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                }

                                this.InvokeClose(DialogResult.OK);
                                return;
                            }
                        }
                    });
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
