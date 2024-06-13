using NetTunnel.ClientAPI;
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
                new ComboItem("Raw", NtTrafficType.Raw),
                new ComboItem("HTTP", NtTrafficType.Http),
                new ComboItem("HTTPS", NtTrafficType.Https)
            };

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
                if (textBoxName.Text.Length == 0)
                    throw new Exception("You must specify a name This is for your identification only.");
                if (textBoxListenPort.Text.Length == 0 || int.TryParse(textBoxListenPort.Text, out var _) == false)
                    throw new Exception("You must specify a valid listen port.");
                if (textBoxTerminationAddress.Text.Length == 0)
                    throw new Exception("You must specify a termination endpoint address.");
                if (textBoxTerminationPort.Text.Length == 0 || int.TryParse(textBoxTerminationPort.Text, out var _) == false)
                    throw new Exception("You must specify a valid termination port.");

                buttonAdd.InvokeEnableControl(false);

                var endpointId = Guid.NewGuid(); //The endpointId is the same on both services.

                var endpointInbound = new NtEndpointInboundConfiguration(_tunnel.TunnelId, endpointId, textBoxName.Text, int.Parse(textBoxListenPort.Text));

                var endpointOutbound = new NtEndpointOutboundConfiguration(_tunnel.TunnelId, endpointId, textBoxName.Text,
                    textBoxTerminationAddress.Text, int.Parse(textBoxTerminationPort.Text));

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
