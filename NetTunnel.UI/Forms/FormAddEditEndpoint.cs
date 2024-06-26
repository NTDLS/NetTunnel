using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEditEndpoint : Form
    {
        private readonly ServiceClient? _client;
        private readonly TunnelDisplay? _tunnel;
        private readonly NtDirection _direction = NtDirection.Undefined;
        private readonly EndpointConfiguration? _existingEndpoint;

        public EndpointConfiguration Endpoint { get; private set; } = new();

        /// <summary>
        /// Creates a form adding a new or editing an existing endpoint for a connected tunnel.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, TunnelDisplay tunnel, EndpointConfiguration existingEndpoint)
        {
            InitializeComponent();

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _tunnel = tunnel;
            _existingEndpoint = existingEndpoint;
            _direction = existingEndpoint.Direction;

            PopulateForm();
        }

        /// <summary>
        /// Creates a form adding a new or editing an existing endpoint for a user account.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, EndpointConfiguration existingEndpoint)
        {
            InitializeComponent();

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _existingEndpoint = existingEndpoint;
            _direction = existingEndpoint.Direction;

            PopulateForm();
        }

        /// <summary>
        /// Creates a form adding a new or editing an existing endpoint for a user account.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, NtDirection direction)
        {
            InitializeComponent();

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _direction = direction;

            PopulateForm();
        }

        /// <summary>
        /// Creates a form for a adding a new endpoint.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, TunnelDisplay tunnel, NtDirection direction)
        {
            InitializeComponent();

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _tunnel = tunnel;
            _direction = direction;

            PopulateForm();
        }

        private void PopulateForm()
        {
            Text = $"NetTunnel : {(_existingEndpoint == null ? "Add" : "Edit")} {_direction} Endpoint";

            #region Set Tool-tips.

            var toolTips = ToolTipHelpers.CreateToolTipControl(this);

            toolTips.AddControls([labelName, textBoxName],
                "name or description you want to use to identify this endpoint.");

            toolTips.AddControls([labelListenPort, textBoxListenPort],
                "The port that will accept new connections at the inbound endpoint.");

            toolTips.AddControls([labelTerminationAddress, textBoxTerminationAddress],
                "The host name, domain or IP address that the outbound endpoint will make a connection to.");

            toolTips.AddControls([labelTerminationPort, textBoxTerminationPort],
                "The host name, domain or IP address that the outbound endpoint will make a connection to.");

            toolTips.AddControls([labelTrafficType, comboBoxTrafficType],
                "The type of traffic that is expected be tunneled on this endpoint. Note that HTTP headers can only be manipulated if HTTP is selected (HTTPS is not supported for header manipulation). ");

            #endregion

            var trafficTypes = new List<ComboItem>
            {
                new ("Raw", NtTrafficType.Raw),
                new ("HTTP", NtTrafficType.Http),
                new ("HTTPS", NtTrafficType.Https)
            };

            comboBoxTrafficType.DisplayMember = "Display";
            comboBoxTrafficType.ValueMember = "Value";
            comboBoxTrafficType.DataSource = trafficTypes;

            if (_existingEndpoint != null)
            {
                foreach (var rule in _existingEndpoint.HttpHeaderRules)
                {
                    dataGridViewHTTPHeaders.Rows.Add([$"{rule.Enabled}", $"{rule.HeaderType}",
                        $"{rule.Verb}", rule.Name, $"{rule.Action}", rule.Value]);
                }

                comboBoxTrafficType.SelectedValue = _existingEndpoint.TrafficType;

                textBoxName.Text = _existingEndpoint.Name;
                textBoxListenPort.Text = $"{_existingEndpoint.InboundPort:n0}";
                textBoxTerminationAddress.Text = _existingEndpoint.OutboundAddress;
                textBoxTerminationPort.Text = $"{_existingEndpoint.OutboundPort:n0}";
            }
            else
            {
                comboBoxTrafficType.SelectedValue = NtTrafficType.Raw;
#if DEBUG

                textBoxName.Text = "Website Redirector Endpoint";
                textBoxListenPort.Text = "8080";
                textBoxTerminationAddress.Text = "127.0.0.1";
                textBoxTerminationPort.Text = "80";
#endif
            }
        }

        private void DataGridViewHTTPHeaders_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
        }

        public FormAddEditEndpoint()
        {
            InitializeComponent();
        }

        private void FormAddEndpoint_Load(object sender, EventArgs e)
        {
            AcceptButton = buttonSave;
            CancelButton = buttonCancel;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();

            try
            {
                textBoxName.GetAndValidateText("You must specify a name.");
                textBoxListenPort.GetAndValidateNumeric(1, 65535, "You must specify a valid listen port between [min] and [max].");
                textBoxTerminationAddress.GetAndValidateText("You must specify a termination address (ip, hostname or domain). ");
                textBoxTerminationPort.GetAndValidateNumeric(1, 65535, "You must specify a valid termination port between [min] and [max].");

                var endpointHttpHeaderRules = new List<HttpHeaderRule>();

                foreach (DataGridViewRow row in dataGridViewHTTPHeaders.Rows)
                {
                    if (string.IsNullOrWhiteSpace($"{row.Cells[columnHeader.Index].Value}") == false)
                    {
                        var headerType = Enum.Parse<NtHttpHeaderType>($"{row.Cells[columnType.Index].Value}");

                        endpointHttpHeaderRules.Add(new HttpHeaderRule
                        {
                            Enabled = bool.Parse(row.Cells[columnEnabled.Index].Value?.ToString() ?? "True"),
                            HeaderType = Enum.Parse<NtHttpHeaderType>($"{row.Cells[columnType.Index].Value}"),
                            Action = Enum.Parse<NtHttpHeaderAction>($"{row.Cells[columnAction.Index].Value}"),
                            Name = $"{row.Cells[columnHeader.Index].Value}",
                            Value = $"{row.Cells[columnValue.Index].Value}",
                            Verb = Enum.Parse<NtHttpVerb>($"{row.Cells[columnVerb.Index].Value}")
                        });
                    }
                }

                Endpoint = new EndpointConfiguration(
                    _existingEndpoint?.EndpointId ?? Guid.NewGuid(),
                    _direction,
                    textBoxName.Text,
                    textBoxTerminationAddress.Text,
                    textBoxListenPort.ValueAs<int>(),
                    textBoxTerminationPort.ValueAs<int>(),
                    endpointHttpHeaderRules,
                    Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"));

                var progressForm = new ProgressForm(FriendlyName, "Saving endpoint...");

                if (_tunnel != null)
                {
                    //If _tunnel is != null, then we are editing an endpoint for a connected tunnel.
                    progressForm.Execute(() =>
                    {
                        try
                        {
                            _client.UIQueryDistributeUpsertEndpoint(_tunnel.TunnelKey, Endpoint);
                            this.InvokeClose(DialogResult.OK);
                        }
                        catch (Exception ex)
                        {
                            progressForm.MessageBox(ex.Message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    });
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}
