using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEndpoint : Form
    {
        private readonly NtClient? _client;
        private readonly INtTunnelConfiguration? _tunnel;

        public FormAddEndpoint(NtClient client, INtTunnelConfiguration tunnel)
        {
            InitializeComponent();

            _client = client;
            _tunnel = tunnel;
        }

        public FormAddEndpoint()
        {
            InitializeComponent();
        }

        private void FormAddEndpoint_Load(object sender, EventArgs e)
        {
            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

#if DEBUG

            textBoxName.Text = "Website Redirector Endpoint";
            textBoxListenPort.Text = "8080";
            textBoxTerminationAddress.Text = "127.0.0.1";
            textBoxTerminationPort.Text = "80";
#endif
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);
            Utility.EnsureNotNull(_tunnel);

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
                if ((!radioButtonLocalEndpoint.Checked && !radioButtonRemoteEndpoint.Checked)
                    || (radioButtonLocalEndpoint.Checked && radioButtonRemoteEndpoint.Checked))
                {
                    throw new Exception("You must select a single listen endpoint location.");
                }

                this.EnableControl(buttonAdd, false);

                var endpointPairId = Guid.NewGuid(); //The endpointId is the same on both services.

                var endpointInbound = new NtEndpointInboundConfiguration(_tunnel.PairId, endpointPairId, textBoxName.Text, int.Parse(textBoxListenPort.Text));

                var endpointOutbound = new NtEndpointOutboundConfiguration(_tunnel.PairId, endpointPairId, textBoxName.Text,
                    textBoxTerminationAddress.Text, int.Parse(textBoxTerminationPort.Text));

                if (_tunnel is NtTunnelInboundConfiguration tunnelInbound)
                {
                    if (radioButtonLocalEndpoint.Checked)
                    {
                        _client.TunnelInbound.AddInboundEndpointPair(tunnelInbound.PairId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.EnableControl(buttonAdd, true);
                                throw new Exception("Failed to add inbound endpoint pair to inbound tunnel.");
                            }
                            this.CloseFormWithResult(DialogResult.OK);

                        });
                    }
                    else
                    {
                        _client.TunnelInbound.AddOutboundEndpointPair(tunnelInbound.PairId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.EnableControl(buttonAdd, true);
                                throw new Exception("Failed to add outbound endpoint pair to inbound tunnel.");
                            }
                            this.CloseFormWithResult(DialogResult.OK);
                        });
                    }
                }
                if (_tunnel is NtTunnelOutboundConfiguration tunnelOutbound)
                {
                    if (radioButtonLocalEndpoint.Checked)
                    {
                        _client.TunnelOutbound.AddInboundEndpointPair(tunnelOutbound.PairId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.EnableControl(buttonAdd, true);
                                throw new Exception("Failed to add outbound endpoint pair to outbound tunnel.");
                            }
                            this.CloseFormWithResult(DialogResult.OK);
                        });
                    }
                    else
                    {
                        _client.TunnelOutbound.AddOutboundEndpointPair(tunnelOutbound.PairId, endpointInbound, endpointOutbound).ContinueWith((o) =>
                        {
                            if (!o.IsCompletedSuccessfully)
                            {
                                this.EnableControl(buttonAdd, true);
                                throw new Exception("Failed to add outbound endpoint pair to outbound tunnel.");
                            }
                            this.CloseFormWithResult(DialogResult.OK);
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
