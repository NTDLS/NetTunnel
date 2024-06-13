using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEditEndpoint : Form
    {
        private readonly NtClient? _client;
        private readonly INtTunnelConfiguration? _tunnel;
        private readonly NtDirection _direction;
        private readonly INtEndpointConfiguration? _endpoint;

        public FormAddEditEndpoint(NtClient client, INtTunnelConfiguration tunnel, INtEndpointConfiguration endpoint)
        {
            InitializeComponent();

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _tunnel = tunnel;
            _endpoint = endpoint;

            if (endpoint is NtEndpointInboundConfiguration)
            {
                _direction = NtDirection.Inbound;
            }
            else if (endpoint is NtEndpointOutboundConfiguration)
            {
                _direction = NtDirection.Outbound;
            }
            else
            {
                throw new Exception("Unknown endpoint type.");
            }

            PopulateForm();
        }

        public FormAddEditEndpoint(NtClient client, INtTunnelConfiguration tunnel, NtDirection direction)
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
            Text = $"NetTunnel : {(_endpoint == null ? "Add" : "Edit")} {_direction} Endpoint";

            var trafficTypes = new List<ComboItem>
            {
                new ("Raw", NtTrafficType.Raw),
                new ("HTTP", NtTrafficType.Http),
                new ("HTTPS", NtTrafficType.Https)
            };

            comboBoxTrafficType.DisplayMember = "Display";
            comboBoxTrafficType.ValueMember = "Value";
            comboBoxTrafficType.DataSource = trafficTypes;

            if (_endpoint != null)
            {
                foreach (var rule in _endpoint.HttpHeaderRules)
                {
                    dataGridViewHTTPHeaders.Rows.Add([$"{rule.Enabled}", $"{rule.HeaderType}", $"{rule.Verb}", rule.Name, $"{rule.Action}", rule.Value]);
                }

                comboBoxTrafficType.SelectedValue = _endpoint.TrafficType;

                textBoxName.Text = _endpoint.Name;
                textBoxInboundPort.Text = $"{_endpoint.InboundPort:n0}";
                textBoxOutboundAddress.Text = _endpoint.OutboundAddress;
                textBoxOutboundPort.Text = $"{_endpoint.OutboundPort:n0}";
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
            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
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
                            Action = Enum.Parse<NtHttpHeaderAction>($"{row.Cells[columnAction.Index].Value}"),
                            Name = $"{row.Cells[columnHeader.Index].Value}",
                            Value = $"{row.Cells[columnValue.Index].Value}",
                            Verb = Enum.Parse<NtHttpVerb>($"{row.Cells[columnVerb.Index].Value}")
                        });
                    }
                }

                buttonAdd.InvokeEnableControl(false);

                var endpointId = Guid.NewGuid(); //The endpointId is the same on both services.

                var endpointInbound = new NtEndpointInboundConfiguration(_tunnel.TunnelId, endpointId,
                    textBoxName.Text, textBoxOutboundAddress.Text, int.Parse(textBoxInboundPort.Text), int.Parse(textBoxOutboundPort.Text))
                {
                    TrafficType = Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"),
                    //We give both endpoints the rules, but they will only execute the rules that match their direction type.
                    HttpHeaderRules = endpointHttpHeaderRules
                };

                var endpointOutbound = new NtEndpointOutboundConfiguration(_tunnel.TunnelId, endpointId,
                    textBoxName.Text, textBoxOutboundAddress.Text, int.Parse(textBoxInboundPort.Text), int.Parse(textBoxOutboundPort.Text))
                {
                    TrafficType = Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"),
                    //We give both endpoints the rules, but they will only execute the rules that match their direction type.
                    HttpHeaderRules = endpointHttpHeaderRules
                };

                if (_tunnel is NtTunnelInboundConfiguration)
                {
                    if (_direction == NtDirection.Inbound)
                    {
                        _client.TunnelInbound.AddEndpointInboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add inbound endpoint pair to inbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonAdd.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);

                        });
                    }
                    else
                    {
                        _client.TunnelInbound.AddEndpointOutboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to inbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonAdd.InvokeEnableControl(true);

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
                        _client.TunnelOutbound.AddEndpointInboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to outbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonAdd.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);
                        });
                    }
                    else
                    {
                        _client.TunnelOutbound.AddEndpointOutboundPair(_tunnel.TunnelId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to outbound tunnel.",
                                    FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                buttonAdd.InvokeEnableControl(true);

                                return;
                            }
                            this.InvokeClose(DialogResult.OK);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }
    }
}
