using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.Localization;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormServiceConfiguration : Form
    {
        private readonly ServiceClient? _client;
        private bool _firstShown = true;
        private ServiceConfiguration? _loadedConfiguration;

        public FormServiceConfiguration()
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);
        }

        public FormServiceConfiguration(ServiceClient client)
        {
            InitializeComponent();
            FormLocalizer.Apply(this);
            Theming.InitializeForm(this);

            _client = client;

            #region Set Tool-tips.

            var defaults = new ServiceConfiguration();

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelManagementPort, textBoxManagementPort],
                Localize("Port is used by both the user-interface and a remote NetTunnel service to communicate configuration changes to this NetTunnel service. Default: {0}", defaults.ServicePort.ToString("n0")));

            toolTips.AddControls([labelMessageQueryTimeoutMs, textBoxMessageQueryTimeoutMs],
                Localize("Duration in milliseconds to wait on message query operations. Default: {0}", defaults.MessageQueryTimeoutMs.ToString("n0")));

            toolTips.AddControls([labelEndpointHeartbeatDelayMs, textBoxEndpointHeartbeatDelayMs],
                Localize("Delay in milliseconds between tunnel heartbeats. Default: {0}", defaults.EndpointHeartbeatDelayMs.ToString("n0")));

            toolTips.AddControls([labelTunnelCryptographyKeySize, textBoxTunnelCryptographyKeySize],
                Localize("Number of encryption key bits to generate for tunnel cryptography. Default: {0}.", defaults.TunnelCryptographyKeySize.ToString("n0")));

            toolTips.AddControls([labelStaleEndpointExpirationMs, textBoxStaleEndpointExpirationMs],
                Localize("Maximum number of milliseconds to allow an endpoint to remain connected without read/write activity. (0 = disabled). Default: {0}", defaults.StaleEndpointExpirationMs.ToString("n0")));

            toolTips.AddControls([labelInitialReceiveBufferSize, textBoxInitialReceiveBufferSize],
                Localize("Initial size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize. Default: {0}", defaults.InitialReceiveBufferSize.ToString("n0")));

            toolTips.AddControls([labelMaxReceiveBufferSize, textBoxMaxReceiveBufferSize],
                Localize("Maximum size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize. Default: {0}", defaults.MaxReceiveBufferSize.ToString("n0")));

            toolTips.AddControls([labelReceiveBufferGrowthRate, textBoxReceiveBufferGrowthRate],
                Localize("Growth rate for auto-resizing the receive buffer from its initial size to its maximum size. Default: {0}", defaults.ReceiveBufferGrowthRate.ToString("n2")));

            toolTips.AddControls([labelPingCadence, textBoxPingCadence],
                Localize("Number of milliseconds to wait between pings to the remote service. (0 = disabled). Default: {0}", defaults.PingCadence.ToString("n0")));

            toolTips.AddControls([labelLogLevel, comboBoxLogLevel],
                Localize("Most detailed level of information to log. Takes effect immediately. Default: {0}", defaults.LogLevel.ToString()));

            toolTips.AddControls([labelLogPath, textBoxLogPath],
                Localize("File to write the log to, on the machine running the service. Relative paths are relative to the service's installation directory. Leave empty to disable file logging."));

            toolTips.AddControls([labelLogFileMaxSizeMB, textBoxLogFileMaxSizeMB],
                Localize("The log file is rolled over (NetTunnel.log -> NetTunnel.1.log, etc.) once it reaches this size. Default: {0}", defaults.LogFileMaxSizeMB.ToString("n0")));

            toolTips.AddControls([labelLogFileRetainedCount, textBoxLogFileRetainedCount],
                Localize("Number of rolled-over log files to keep in addition to the current one. Default: {0}", defaults.LogFileRetainedCount.ToString("n0")));

            toolTips.AddControls([checkBoxLogToEventLog],
                Localize("Also write warnings and exceptions to the Windows Application event log."));

            #endregion

            comboBoxLogLevel.DataSource = Enum.GetValues<NtLogSeverity>();

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

            var progressForm = new ProgressForm(Constants.FriendlyName, Localize("Getting configuration..."));

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
                comboBoxLogLevel.SelectedItem = configuration.LogLevel;
                textBoxLogPath.Text = configuration.LogPath;
                textBoxLogFileMaxSizeMB.Text = $"{configuration.LogFileMaxSizeMB:n0}";
                textBoxLogFileRetainedCount.Text = $"{configuration.LogFileRetainedCount:n0}";
                checkBoxLogToEventLog.Checked = configuration.LogToEventLog;

                _loadedConfiguration = configuration;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_loadedConfiguration == null)
                {
                    throw new Exception(Localize("The service configuration has not been loaded."));
                }

                //Start from what was loaded so that settings which are not on this form (e.g. the service id) are kept.
                var configuration = _loadedConfiguration;

                #region Get and validate form values .

                configuration.ServicePort = textBoxManagementPort.GetAndValidateNumeric(1, 65535,
                    Localize("Service port must be an integer value between [min] and [max]."));

                configuration.MessageQueryTimeoutMs = textBoxMessageQueryTimeoutMs.GetAndValidateNumeric(1000, 3600000,
                    Localize("Message query timeout (ms) must be an integer value between [min] and [max]."));

                configuration.EndpointHeartbeatDelayMs = textBoxEndpointHeartbeatDelayMs.GetAndValidateNumeric(1000, 216000000,
                    Localize("Tunnel and endpoint heartbeat (ms) must be an integer value between [min] and [max]."));

                configuration.TunnelCryptographyKeySize = textBoxTunnelCryptographyKeySize.GetAndValidateNumeric(128, 81920,
                    Localize("Tunnel cryptography key-size must be an integer value between [min] and [max]."));

                configuration.StaleEndpointExpirationMs = textBoxStaleEndpointExpirationMs.GetAndValidateNumeric(0, 216000000,
                    Localize("Stale endpoint expiration (ms) must be an integer value between [min] (infinite) and [max]."));

                configuration.InitialReceiveBufferSize = textBoxInitialReceiveBufferSize.GetAndValidateNumeric(1024, 1073741824,
                    Localize("Initial buffer size (bytes) must be an integer value between [min] and [max]."));

                configuration.MaxReceiveBufferSize = textBoxMaxReceiveBufferSize.GetAndValidateNumeric(1024, 1073741824,
                    Localize("Maximum buffer size (bytes) must be an integer value between [min] and [max]."));

                configuration.ReceiveBufferGrowthRate = textBoxReceiveBufferGrowthRate.GetAndValidateNumeric(0.01, 1.0,
                    Localize("Buffer growth rate (%) must be an decimal value between [min] and [max]."));

                configuration.PingCadence = textBoxPingCadence.GetAndValidateNumeric(0, 1073741824,
                    Localize("Ping cadence must be an decimal value between [min] (disabled) and [max]."));

                configuration.LogLevel = (NtLogSeverity)(comboBoxLogLevel.SelectedItem ?? NtLogSeverity.Verbose);

                configuration.LogPath = textBoxLogPath.Text.Trim();

                configuration.LogFileMaxSizeMB = textBoxLogFileMaxSizeMB.GetAndValidateNumeric(1, 10240,
                    Localize("Log file roll-over size (MB) must be an integer value between [min] and [max]."));

                configuration.LogFileRetainedCount = textBoxLogFileRetainedCount.GetAndValidateNumeric(0, 1000,
                    Localize("Rolled log files to keep must be an integer value between [min] and [max]."));

                configuration.LogToEventLog = checkBoxLogToEventLog.Checked;

                #endregion

                var progressForm = new ProgressForm(Constants.FriendlyName, Localize("Saving configuration..."));

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
