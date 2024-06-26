﻿using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI.Forms
{
    public partial class FormServiceConfiguration : Form
    {
        private readonly ServiceClient? _client;
        private bool _firstShown = true;

        public FormServiceConfiguration()
        {
            InitializeComponent();
        }

        public FormServiceConfiguration(ServiceClient client)
        {
            InitializeComponent();

            _client = client;

            #region Set Tool-tips.

            var defaults = new ServiceConfiguration();

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelManagementPort, textBoxManagementPort],
                $"The port is used by both the user-interface and a remote NetTunnel service to communicate configuration changes to this NetTunnel service. Default: {defaults.ServicePort:n0}");

            toolTips.AddControls([labelMessageQueryTimeoutMs, textBoxMessageQueryTimeoutMs],
                $"The duration in milliseconds to wait on message query operations. Default: {defaults.MessageQueryTimeoutMs:n0}");

            toolTips.AddControls([labelEndpointHeartbeatDelayMs, textBoxEndpointHeartbeatDelayMs],
                $"The delay in milliseconds between tunnel heartbeats. Default: {defaults.EndpointHeartbeatDelayMs:n0}");

            toolTips.AddControls([labelTunnelCryptographyKeySize, textBoxTunnelCryptographyKeySize],
                $"The number of encryption key bits to generate for tunnel cryptography. Default: {defaults.TunnelCryptographyKeySize:n0}.");

            toolTips.AddControls([labelStaleEndpointExpirationMs, textBoxStaleEndpointExpirationMs],
                $"The maximum number of milliseconds to allow an endpoint to remain connected without read/write activity. (0 = disabled). Default: {defaults.StaleEndpointExpirationMs:n0}");

            toolTips.AddControls([labelInitialReceiveBufferSize, textBoxInitialReceiveBufferSize],
                $"The initial size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize. Default: {defaults.InitialReceiveBufferSize:n0}");

            toolTips.AddControls([labelMaxReceiveBufferSize, textBoxMaxReceiveBufferSize],
                $"The maximum size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize. Default: {defaults.MaxReceiveBufferSize:n0}");

            toolTips.AddControls([labelReceiveBufferGrowthRate, textBoxReceiveBufferGrowthRate],
                $"The growth rate for auto-resizing the receive buffer from its initial size to its maximum size. Default: {defaults.ReceiveBufferGrowthRate:n2}");

            toolTips.AddControls([labelPingCadence, textBoxPingCadence],
                $"The number of milliseconds to wait between pings to the remote service. (0 = disabled). Default: {defaults.PingCadence:n0}");

            #endregion

            Shown += FormServiceConfiguration_Shown;

            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void FormServiceConfiguration_Shown(object? sender, EventArgs e)
        {
            if (!_firstShown)
            {
                return;
            }
            _firstShown = false;

            var progressForm = new ProgressForm(Constants.FriendlyName, "Getting configuration...");

            progressForm.Execute(() =>
            {
                try
                {
                    var result = _client.EnsureNotNull().UIQueryGetServiceConfiguration();

                    SetFormConfigurationValues(result.Configuration);
                }
                catch (Exception ex)
                {
                    progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            });
        }

        public void SetFormConfigurationValues(ServiceConfiguration configuration)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetFormConfigurationValues(configuration));
            }
            else
            {
                textBoxManagementPort.Text = $"{configuration.ServicePort:n0}";
                textBoxMessageQueryTimeoutMs.Text = $"{configuration.MessageQueryTimeoutMs:n0}";
                textBoxEndpointHeartbeatDelayMs.Text = $"{configuration.EndpointHeartbeatDelayMs:n0}";
                textBoxTunnelCryptographyKeySize.Text = $"{configuration.TunnelCryptographyKeySize:n0}";
                textBoxStaleEndpointExpirationMs.Text = $"{configuration.StaleEndpointExpirationMs:n0}";
                textBoxInitialReceiveBufferSize.Text = $"{configuration.InitialReceiveBufferSize:n0}";
                textBoxMaxReceiveBufferSize.Text = $"{configuration.MaxReceiveBufferSize:n0}";
                textBoxReceiveBufferGrowthRate.Text = $"{configuration.ReceiveBufferGrowthRate:n2}";
                textBoxPingCadence.Text = $"{configuration.PingCadence:n0}";
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                var configuration = new ServiceConfiguration();

                #region Get and validate form values .

                configuration.ServicePort = textBoxManagementPort.GetAndValidateNumeric(1, 65535,
                    "The service port must be an integer value between [min] and [max].");

                configuration.MessageQueryTimeoutMs = textBoxMessageQueryTimeoutMs.GetAndValidateNumeric(1000, 3600000,
                    "The message query timeout (ms) must be an integer value between [min] and [max].");

                configuration.EndpointHeartbeatDelayMs = textBoxEndpointHeartbeatDelayMs.GetAndValidateNumeric(1000, 216000000,
                    "The tunnel and endpoint heartbeat (ms) must be an integer value between [min] and [max].");

                configuration.TunnelCryptographyKeySize = textBoxTunnelCryptographyKeySize.GetAndValidateNumeric(128, 81920,
                    "The tunnel cryptography key-size must be an integer value between [min] and [max].");

                configuration.StaleEndpointExpirationMs = textBoxStaleEndpointExpirationMs.GetAndValidateNumeric(0, 216000000,
                    "The stale endpoint expiration (ms) must be an integer value between [min] (infinite) and [max].");

                configuration.InitialReceiveBufferSize = textBoxInitialReceiveBufferSize.GetAndValidateNumeric(1024, 1073741824,
                    "The initial buffer size (bytes) must be an integer value between [min] and [max].");

                configuration.MaxReceiveBufferSize = textBoxMaxReceiveBufferSize.GetAndValidateNumeric(1024, 1073741824,
                    "The max buffer size (bytes) must be an integer value between [min] and [max].");

                configuration.ReceiveBufferGrowthRate = textBoxReceiveBufferGrowthRate.GetAndValidateNumeric(0.01, 1.0,
                    "The buffer growth rate (%) must be an decimal value between [min] and [max].");

                configuration.PingCadence = textBoxPingCadence.GetAndValidateNumeric(0, 1073741824,
                    "The ping cadence must be an decimal value between [min] (disabled) and [max].");

                #endregion

                var progressForm = new ProgressForm(Constants.FriendlyName, "Saving configuration...");

                progressForm.Execute(() =>
                {
                    try
                    {
                        _client.EnsureNotNull().UIQueryPutServiceConfiguration(configuration);
                        this.InvokeClose(DialogResult.OK);
                    }
                    catch (Exception ex)
                    {
                        progressForm.MessageBox(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Constants.FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
