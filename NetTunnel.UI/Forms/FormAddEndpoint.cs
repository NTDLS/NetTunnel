using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEndpoint : Form
    {
        private readonly NtClient? _client;
        private readonly INtTunnelConfiguration? _tunnel;
        private readonly NtDirection _direction;

        public FormAddEndpoint(NtClient client, INtTunnelConfiguration tunnel, NtDirection direction)
        {
            InitializeComponent();

            dataGridViewHTTPHeaders.DataError += DataGridViewHTTPHeaders_DataError;

            _client = client;
            _tunnel = tunnel;
            _direction = direction;

            Text = $"NetTunnel : Add {direction} Endpoint";

            var trafficTypes = new List<ComboItem>
            {
                new ("Raw", NtTrafficType.Raw),
                new ("HTTP", NtTrafficType.Http),
                new ("HTTPS", NtTrafficType.Https)
            };

            /*
            //Populate inbound rules:
            foreach (var config in tunnel.EndpointInboundConfigurations)
            {
                dataGridViewHTTPHeaders.Rows.Add(
                    [ $"{httpHeaderRule.Enabled}", $"{httpHeaderRule.HeaderType}",
                    $"{httpHeaderRule.Verb}", httpHeaderRule.Name, $"{httpHeaderRule.Action}", httpHeaderRule.Value ]
                );
            }

            //Populate outbound rules:
            foreach (var httpHeaderRule in proxy.HttpHeaderRules.Collection)
            {
                dataGridViewHTTPHeaders.Rows.Add(
                    [ $"{httpHeaderRule.Enabled}", $"{httpHeaderRule.HeaderType}",
                    $"{httpHeaderRule.Verb}", httpHeaderRule.Name, $"{httpHeaderRule.Action}", httpHeaderRule.Value ]
                );
            }
            */

            comboBoxTrafficType.DisplayMember = "Display";
            comboBoxTrafficType.ValueMember = "Value";
            comboBoxTrafficType.DataSource = trafficTypes;
            comboBoxTrafficType.SelectedValue = NtTrafficType.Raw;

#if DEBUG

            textBoxName.Text = "Website Redirector Endpoint";
            textBoxListenPort.Text = "8080";
            textBoxTerminationAddress.Text = "127.0.0.1";
            textBoxTerminationPort.Text = "80";
#endif
        }

        private void DataGridViewHTTPHeaders_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {

        }

        public FormAddEndpoint()
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
                textBoxListenPort.GetAndValidateNumeric(1, 65535, "You must specify a valid listen port between [min] and [max].");
                textBoxTerminationAddress.GetAndValidateText("You must specify a termination endpoint address (ip, hostname or domain). ");
                textBoxTerminationPort.GetAndValidateNumeric(1, 65535, "You must specify a valid termination port between [min] and [max].");

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

                var endpointInbound = new NtEndpointInboundConfiguration(_tunnel.TunnelId, endpointId, textBoxName.Text, int.Parse(textBoxListenPort.Text))
                {
                    TrafficType = Enum.Parse<NtTrafficType>($"{comboBoxTrafficType.SelectedValue}"),
                    //We give both endpoints the rules, but they will only execute the rules that match their direction type.
                    HttpHeaderRules = endpointHttpHeaderRules
                };

                var endpointOutbound = new NtEndpointOutboundConfiguration(_tunnel.TunnelId, endpointId, textBoxName.Text,
                    textBoxTerminationAddress.Text, int.Parse(textBoxTerminationPort.Text))
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
                                this.InvokeMessageBox("Failed to add inbound endpoint pair to inbound tunnel.", FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

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
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to inbound tunnel.", FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

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
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to outbound tunnel.", FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

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
                                this.InvokeMessageBox("Failed to add outbound endpoint pair to outbound tunnel.", FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

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
