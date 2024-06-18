using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormServiceConfiguration : Form
    {
        private readonly ServiceClient? _client;

        public FormServiceConfiguration()
        {
            InitializeComponent();
        }

        public FormServiceConfiguration(ServiceClient client)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelManagementPort, textBoxManagementPort],
                "The management port is used by both the user-interface and a remote NetTunnel service to communicate configuration changes to this NetTunnel service.");

            toolTips.AddControls([labelManagementPortRSASize, textBoxManagementPortRSASize],
                "The key size to use when generating the self-signed SSL certificate for the management port.");

            toolTips.AddControls([labelMessageQueryTimeoutMs, textBoxMessageQueryTimeoutMs],
                "The duration in milliseconds to wait on message query operations.");

            toolTips.AddControls([labelTunnelAndEndpointHeartbeatDelayMs, textBoxTunnelAndEndpointHeartbeatDelayMs],
                "The delay in milliseconds between tunnel heartbeats.");

            toolTips.AddControls([labelTunnelCryptographyKeySize, textBoxTunnelCryptographyKeySize],
                "The number of 12-byte segments to generate for tunnel cryptography.");

            toolTips.AddControls([labelStaleEndpointExpirationMs, textBoxStaleEndpointExpirationMs],
                "The maximum number of milliseconds to allow an endpoint to remain connected without read/write activity.");

            toolTips.AddControls([checkBoxManagementUseSSL],
                "Whether the management web-services should use SSL or not. If checked, the NetTunnel service will generate a self-signed SSL certificate with the encryption key size denoted by the 'Management RSA size'.");

            toolTips.AddControls([labelInitialReceiveBufferSize, textBoxInitialReceiveBufferSize],
                "The initial size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.");

            toolTips.AddControls([labelMaxReceiveBufferSize, textBoxMaxReceiveBufferSize],
                "The maximum size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.");

            toolTips.AddControls([labelReceiveBufferGrowthRate, textBoxReceiveBufferGrowthRate],
                "The growth rate for auto-resizing the receive buffer from its initial size to its maximum size.");

            toolTips.AddControls([checkBoxDebugLogging],
                "Whether to log debug information to the console and event log.");

            toolTips.AddControls([checkBoxVerboseLogging],
                "Whether to log verbose information to the console and event log.");

            #endregion

            /*
            _client.EnsureNotNull().Service.GetConfiguration().ContinueWith(t =>
            {
                SetFormConfigurationValues(t.Result.Configuration);
            });
            */

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        public void SetFormConfigurationValues(ServiceConfiguration configuration)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetFormConfigurationValues(configuration));
            }
            else
            {
                textBoxManagementPort.Text = $"{configuration.ManagementPort:n0}";
                textBoxManagementPortRSASize.Text = $"{configuration.ManagementPortRSASize:n0}";
                textBoxMessageQueryTimeoutMs.Text = $"{configuration.MessageQueryTimeoutMs:n0}";
                textBoxTunnelAndEndpointHeartbeatDelayMs.Text = $"{configuration.TunnelAndEndpointHeartbeatDelayMs:n0}";
                textBoxTunnelCryptographyKeySize.Text = $"{configuration.TunnelCryptographyKeySize:n0}";
                textBoxStaleEndpointExpirationMs.Text = $"{configuration.StaleEndpointExpirationMs:n0}";
                textBoxInitialReceiveBufferSize.Text = $"{configuration.InitialReceiveBufferSize:n0}";
                textBoxMaxReceiveBufferSize.Text = $"{configuration.MaxReceiveBufferSize:n0}";
                textBoxReceiveBufferGrowthRate.Text = $"{configuration.ReceiveBufferGrowthRate:n2}";
                checkBoxManagementUseSSL.Checked = configuration.ManagementPortUseSSL;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                var configuration = new ServiceConfiguration();

                #region Get and validate form values .

                configuration.ManagementPort = textBoxManagementPort.GetAndValidateNumeric(1, 65535,
                    "The management port must be an integer value between [min] and [max].");

                configuration.ManagementPortRSASize = textBoxManagementPortRSASize.GetAndValidateNumeric(2048, 4096,
                    "The management RSA size must be an integer value between [min] and 4096.");

                configuration.MessageQueryTimeoutMs = textBoxMessageQueryTimeoutMs.GetAndValidateNumeric(1000, 3600000,
                    "The message query timeout (ms) must be an integer value between [min] and [max].");

                configuration.TunnelAndEndpointHeartbeatDelayMs = textBoxTunnelAndEndpointHeartbeatDelayMs.GetAndValidateNumeric(1000, 216000000,
                    "The tunnel and endpoint heartbeat (ms) must be an integer value between [min] and [max].");

                configuration.TunnelCryptographyKeySize = textBoxTunnelCryptographyKeySize.GetAndValidateNumeric(1, 100,
                    "The tunnel cryptography key-size must be an integer value between [min] and [max].");

                configuration.StaleEndpointExpirationMs = textBoxStaleEndpointExpirationMs.GetAndValidateNumeric(1000, 216000000,
                    "The stale endpoint expiration (ms) must be an integer value between [min] and [max].");

                configuration.InitialReceiveBufferSize = textBoxInitialReceiveBufferSize.GetAndValidateNumeric(1024, 1073741824,
                    "The initial buffer size (bytes) must be an integer value between [min] and [max].");

                configuration.MaxReceiveBufferSize = textBoxMaxReceiveBufferSize.GetAndValidateNumeric(1024, 1073741824,
                    "The max buffer size (bytes) must be an integer value between [min] and [max].");

                configuration.ReceiveBufferGrowthRate = textBoxReceiveBufferGrowthRate.GetAndValidateNumeric(0.01, 1.0,
                    "The buffer growth rate (%) must be an decimal value between [min] and [max].");

                configuration.ManagementPortUseSSL = checkBoxManagementUseSSL.Checked;

                #endregion

                buttonSave.InvokeEnableControl(false);

                /*
                _client.EnsureNotNull().Service.PutConfiguration(configuration).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        this.InvokeMessageBox("Failed to save the configuration.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        buttonSave.InvokeEnableControl(true);
                        return;
                    }

                    this.InvokeClose(DialogResult.OK);
                });
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
