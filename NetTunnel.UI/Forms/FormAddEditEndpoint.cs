using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEditEndpoint : Form
    {
        private readonly NtServiceClient? _client;
        private readonly NtTunnelConfiguration? _tunnel;
        private readonly NtDirection _direction;
        private readonly NtEndpointConfiguration? _existingEndpoint;

        /// <summary>
        /// Creates a form for a editing an existing endpoint.
        /// </summary>
        public FormAddEditEndpoint(NtServiceClient client, NtTunnelConfiguration tunnel, NtEndpointConfiguration existingEndpoint)
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
        public FormAddEditEndpoint(NtServiceClient client, NtTunnelConfiguration tunnel, NtDirection direction)
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

            try
            {
                textBoxName.GetAndValidateText("You must specify a name This is for your identification only.");
                textBoxInboundPort.GetAndValidateNumeric(1, 65535, "You must specify a valid listen port between [min] and [max].");
                textBoxOutboundAddress.GetAndValidateText("You must specify a termination endpoint address (ip, hostname or domain). ");
                textBoxOutboundPort.GetAndValidateNumeric(1, 65535, "You must specify a valid termination port between [min] and [max].");

                var endpointHttpHeaderRules = new List<NtHttpHeaderRule>();

                foreach (DataGridViewRow row in dataGridViewHTTPHeaders.Rows)
                {
                    if (string.IsNullOrWhiteSpace($"{row.Cells[columnHeader.Index].Value}") == false)
                    {
                        var headerType = Enum.Parse<NtHttpHeaderType>($"{row.Cells[columnType.Index].Value}");

                        endpointHttpHeaderRules.Add(new NtHttpHeaderRule
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

                buttonSave.InvokeEnableControl(false);

                var endpointId = _existingEndpoint?.EndpointId ?? Guid.NewGuid(); //The endpointId is the same on both services.

                var inboundEndpoint = new NtEndpointConfiguration(_tunnel.TunnelId, endpointId, NtDirection.Inbound,
                    textBoxName.Text, textBoxOutboundAddress.Text, textBoxInboundPort.ValueAs<int>(),
                    textBoxOutboundPort.ValueAs<int>(), endpointHttpHeaderRules, Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"));

                var outboundEndpoint = new NtEndpointConfiguration(_tunnel.TunnelId, endpointId, NtDirection.Outbound,
                    textBoxName.Text, textBoxOutboundAddress.Text, textBoxInboundPort.ValueAs<int>(),
                    textBoxOutboundPort.ValueAs<int>(), endpointHttpHeaderRules, Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"));

                /*
                if (_tunnel is NtTunnelInboundConfiguration)
                {
                    if (_direction == NtDirection.Inbound)
                    {
                        _client.UpsertEndpointInboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add inbound endpoint pair to inbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonSave.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);

                        });
                    }
                    else
                    {
                        _client.TunnelInbound.UpsertEndpointOutboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to inbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonSave.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);
                        });
                    }
                }
                if (_tunnel is NtTunnelOutboundConfiguration)
                {
                    if (_direction == NtDirection.Inbound)
                    {
                        _client.TunnelOutbound.UpsertEndpointInboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to outbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonSave.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);
                        });
                    }
                    else
                    {
                        _client.TunnelOutbound.UpsertEndpointOutboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to outbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonSave.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);
                        });
                    }
                }
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }
    }
}
