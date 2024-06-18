using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEditEndpoint : Form
    {
        private readonly ServiceClient? _client;
        private readonly TunnelConfiguration? _tunnel;
        private readonly NtDirection _direction = NtDirection.Undefined;
        private readonly EndpointConfiguration? _existingEndpoint;

        /// <summary>
        /// Creates a form for a editing an existing endpoint.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, TunnelConfiguration tunnel, EndpointConfiguration existingEndpoint)
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
        /// Creates a form for a adding a new endpoint.
        /// </summary>
        public FormAddEditEndpoint(ServiceClient client, TunnelConfiguration tunnel, NtDirection direction)
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
                    dataGridViewHTTPHeaders.Rows.Add([$"{rule.Enabled}", $"{rule.HeaderType}", $"{rule.Verb}", rule.Name, $"{rule.Action}", rule.Value]);
                }

                comboBoxTrafficType.SelectedValue = _existingEndpoint.TrafficType;

                textBoxName.Text = _existingEndpoint.Name;
                textBoxInboundPort.Text = $"{_existingEndpoint.InboundPort:n0}";
                textBoxOutboundAddress.Text = _existingEndpoint.OutboundAddress;
                textBoxOutboundPort.Text = $"{_existingEndpoint.OutboundPort:n0}";
            }
            else
            {
                comboBoxTrafficType.SelectedValue = NtTrafficType.Raw;

#if DEBUG

                textBoxName.Text = "Website Redirector Endpoint";
                textBoxInboundPort.Text = "8080";
                textBoxOutboundAddress.Text = "127.0.0.1";
                textBoxOutboundPort.Text = "80";
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

        private void buttonSave_Click(object sender, EventArgs e)
        {
            _client.EnsureNotNull();
            _tunnel.EnsureNotNull();

            buttonSave.InvokeDisable();

            try
            {
                textBoxName.GetAndValidateText("You must specify a name This is for your identification only.");
                textBoxInboundPort.GetAndValidateNumeric(1, 65535, "You must specify a valid listen port between [min] and [max].");
                textBoxOutboundAddress.GetAndValidateText("You must specify a termination endpoint address (ip, hostname or domain). ");
                textBoxOutboundPort.GetAndValidateNumeric(1, 65535, "You must specify a valid termination port between [min] and [max].");

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

                var endpointId = _existingEndpoint?.EndpointId ?? Guid.NewGuid(); //The endpointId is the same on both services.

                var endpoint = new EndpointConfiguration(endpointId, _direction,
                    textBoxName.Text, textBoxOutboundAddress.Text, textBoxInboundPort.ValueAs<int>(),
                    textBoxOutboundPort.ValueAs<int>(), endpointHttpHeaderRules, Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"));

                _client.QueryUpsertEndpoint(_tunnel.TunnelId, endpoint).ContinueWith((o) =>
                    {
                        if (!o.IsCompletedSuccessfully)
                        {
                            this.InvokeMessageBox("Failed to save endpoint to tunnel.",
                                FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                            buttonSave.InvokeEnable();

                            return;
                        }

                        this.InvokeClose(DialogResult.OK);

                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
                buttonSave.InvokeEnable();
            }
        }
    }
}
