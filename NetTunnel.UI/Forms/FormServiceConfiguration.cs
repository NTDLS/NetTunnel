﻿using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.UI.Helpers;
using System.Reflection.Metadata;

namespace NetTunnel.UI.Forms
{
    public partial class FormServiceConfiguration : Form
    {
        private readonly NtClient? _client;

        private readonly ToolTip _toolTips = ToolTipHelpers.CreateToolTipControl();

        public FormServiceConfiguration()
        {
            InitializeComponent();
        }

        public FormServiceConfiguration(NtClient client)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            ToolTipHelpers.SetToolTip(_toolTips, [labelManagementPort, textBoxManagementPort],
                "The management port is used by both the user-interface and a remote NetTunnel service to communicate configuration changes to this NetTunnel service.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelManagementPortRSASize, textBoxManagementPortRSASize],
                "The key size to use when generating the self-signed SSL certificate for the management port.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelEndpointBufferSize, textBoxEndpointBufferSize],
                "The buffer size (in bytes) used by endpoint connections for sending and receiving data.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelMessageQueryTimeoutMs, textBoxMessageQueryTimeoutMs],
                "The duration in milliseconds to wait on message query operations.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelTunnelAndEndpointHeartbeatDelayMs, textBoxTunnelAndEndpointHeartbeatDelayMs],
                "The delay in milliseconds between tunnel heartbeats.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelTunnelEncryptionKeySize, textBoxTunnelEncryptionKeySize],
                "The number of 12-byte segments to generate for tunnel encryption.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelStaleEndpointExpirationMs, textBoxStaleEndpointExpirationMs],
                "The maximum number of milliseconds to allow an endpoint to remain connected without read/write activity.");

            ToolTipHelpers.SetToolTip(_toolTips, [checkBoxManagementUseSSL],
                "Whether the management web-services should use SSL or not. If checked, the NetTunnel service will generate a self-signed SSL certificate with the encryption key size denoted by the 'Management RSA size'.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelInitialReceiveBufferSize, textBoxInitialReceiveBufferSize],
                "The initial size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelMaxReceiveBufferSize, textBoxMaxReceiveBufferSize],
                "The maximum size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.");

            ToolTipHelpers.SetToolTip(_toolTips, [labelReceiveBufferGrowthRate, textBoxReceiveBufferGrowthRate],
                "The growth rate for auto-resizing the receive buffer from its initial size to its maximum size.");

            ToolTipHelpers.SetToolTip(_toolTips, [checkBoxDebugLogging],
                "Whether to log debug information to file.");

            #endregion

            _client.EnsureNotNull().Service.GetConfiguration().ContinueWith(t =>
            {
                SetFormConfigurationValues(t.Result.Configuration);
            });

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        public void SetFormConfigurationValues(NtServiceConfiguration configuration)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetFormConfigurationValues(configuration));
            }
            else
            {
                textBoxManagementPort.Text = $"{configuration.ManagementPort:n0}";
                textBoxManagementPortRSASize.Text = $"{configuration.ManagementPortRSASize:n0}";
                textBoxEndpointBufferSize.Text = $"{configuration.EndpointBufferSize:n0}";
                textBoxMessageQueryTimeoutMs.Text = $"{configuration.MessageQueryTimeoutMs:n0}";
                textBoxTunnelAndEndpointHeartbeatDelayMs.Text = $"{configuration.TunnelAndEndpointHeartbeatDelayMs:n0}";
                textBoxTunnelEncryptionKeySize.Text = $"{configuration.TunnelEncryptionKeySize:n0}";
                textBoxStaleEndpointExpirationMs.Text = $"{configuration.StaleEndpointExpirationMs:n0}";
                textBoxInitialReceiveBufferSize.Text = $"{configuration.InitialReceiveBufferSize:n0}";
                textBoxMaxReceiveBufferSize.Text = $"{configuration.MaxReceiveBufferSize:n0}";
                textBoxReceiveBufferGrowthRate.Text = $"{configuration.ReceiveBufferGrowthRate:n2}";
                checkBoxManagementUseSSL.Checked = configuration.ManagementPortUseSSL;
                checkBoxDebugLogging.Checked = configuration.DebugLogging;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                var configuration = new NtServiceConfiguration();

                #region Get and validate form values .

                configuration.ManagementPort = FormValidationHelpers.GetAndValidateInteger(textBoxManagementPort, 1, 65535,
                    "The management port must be an integer value between [min] and [max].");

                configuration.ManagementPortRSASize = FormValidationHelpers.GetAndValidateInteger(textBoxManagementPortRSASize, 2048, 4096,
                    "The management RSA size must be an integer value between [min] and 4096.");

                configuration.EndpointBufferSize = FormValidationHelpers.GetAndValidateInteger(textBoxEndpointBufferSize, 1024, 1073741824,
                    "The endpoint buffer size must be an integer value between [min] and [max].");

                configuration.MessageQueryTimeoutMs = FormValidationHelpers.GetAndValidateInteger(textBoxMessageQueryTimeoutMs, 1000, 3600000,
                    "The message query timeout (ms) must be an integer value between [min] and [max].");

                configuration.TunnelAndEndpointHeartbeatDelayMs = FormValidationHelpers.GetAndValidateInteger(textBoxTunnelAndEndpointHeartbeatDelayMs, 1000, 216000000,
                    "The tunnel and endpoint heartbeat (ms) must be an integer value between [min] and [max].");

                configuration.TunnelEncryptionKeySize = FormValidationHelpers.GetAndValidateInteger(textBoxTunnelEncryptionKeySize, 1, 100,
                    "The tunnel encryption key-size must be an integer value between [min] and [max].");

                configuration.StaleEndpointExpirationMs = FormValidationHelpers.GetAndValidateInteger(textBoxStaleEndpointExpirationMs, 1000, 216000000,
                    "The stale endpoint expiration (ms) must be an integer value between [min] and [max].");

                configuration.InitialReceiveBufferSize = FormValidationHelpers.GetAndValidateInteger(textBoxInitialReceiveBufferSize, 1024, 1073741824,
                    "The initial buffer size (bytes) must be an integer value between [min] and [max].");

                configuration.MaxReceiveBufferSize = FormValidationHelpers.GetAndValidateInteger(textBoxMaxReceiveBufferSize, 1024, 1073741824,
                    "The max buffer size (bytes) must be an integer value between [min] and [max].");

                configuration.ReceiveBufferGrowthRate = FormValidationHelpers.GetAndValidateDouble(textBoxReceiveBufferGrowthRate, 0.01, 1.0,
                    "The buffer growth rate (%) must be an decimal value between [min] and [max].");

                configuration.ManagementPortUseSSL = checkBoxManagementUseSSL.Checked;
                configuration.DebugLogging = checkBoxDebugLogging.Checked;

                #endregion

                buttonSave.ThreadSafeEnable(false);

                _client.EnsureNotNull().Service.PutConfiguration(configuration).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        buttonSave.ThreadSafeEnable(true);
                        this.ThreadSafeMessageBox("Failed to save the configuration.", Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }

                    this.ThreadSafeClose(DialogResult.OK);
                });
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
