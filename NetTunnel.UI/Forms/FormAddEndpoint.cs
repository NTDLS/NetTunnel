using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;

namespace NetTunnel.UI.Forms
{
    public partial class FormAddEndpoint : BaseForm
    {
        private readonly NtClient? _client;
        private readonly NtTunnelInboundConfiguration? _tunnelInbound;
        private readonly NtTunnelOutboundConfiguration? _tunnelOutbound;

        public FormAddEndpoint(NtClient client, NtTunnelInboundConfiguration tunnelInbound)
        {
            InitializeComponent();

            _client = client;
            _tunnelInbound = tunnelInbound;
        }

        public FormAddEndpoint(NtClient client, NtTunnelOutboundConfiguration tunnelOutbound)
        {
            InitializeComponent();

            _client = client;
            _tunnelOutbound = tunnelOutbound;
        }

        public FormAddEndpoint()
        {
            InitializeComponent();
        }

        private void FormAddEndpoint_Load(object sender, EventArgs e)
        {
            AcceptButton = buttonAdd;
            CancelButton = buttonCancel;

            textBoxName.Text = "Website Redirector Endpoint";
            textBoxListenPort.Text = "8080";
            textBoxTerminationAddress.Text = "127.0.0.1";
            textBoxTerminationPort.Text = "80";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Utility.EnsureNotNull(_client);

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

                EnableControl(buttonAdd, false);

                var endpointPairId = Guid.NewGuid(); //The endpointId is the same on both services.

                var endpointInbound = new NtEndpointInboundConfiguration(endpointPairId, textBoxName.Text, int.Parse(textBoxListenPort.Text));

                var endpointOutbound = new NtEndpointOutboundConfiguration(endpointPairId, textBoxName.Text,
                    textBoxTerminationAddress.Text, int.Parse(textBoxTerminationPort.Text));


                if (_tunnelInbound != null)
                {
                    if (radioButtonLocalEndpoint.Checked)
                    {
                        _client.TunnelInbound.AddInboundEndpointPair(endpointPairId, endpointInbound, endpointOutbound).RunSynchronously();
                    }
                    else
                    {
                        _client.TunnelInbound.AddOutboundEndpointPair(endpointPairId, endpointInbound, endpointOutbound).RunSynchronously();
                    }
                }
                else if (_tunnelOutbound != null)
                {
                    if (radioButtonLocalEndpoint.Checked)
                    {
                        _client.TunnelOutbound.AddOutboundEndpointPair(endpointPairId, endpointInbound, endpointOutbound).RunSynchronously();
                    }
                    else
                    {
                        _client.TunnelOutbound.AddInboundEndpointPair(endpointPairId, endpointInbound, endpointOutbound).RunSynchronously();
                    }
                }                                

                //_tunnel.AddEndpoint(endpointInbound, endpointOutbound,
                //radioButtonLocalEndpoint.Checked ? Library.Constants.EndpointDirection.Inbound : Library.Constants.EndpointDirection.Outbound);

                /*
                NtClient remoteClient;

                try
                {
                    //Connect to the remote endpoint.
                    remoteClient = new NtClient($"https://{outgoingEndpoint.Address}:{outgoingEndpoint.ManagementPort}/");
                }
                catch (Exception ex)
                {
                    EnableControl(buttonAdd, false);
                    throw new Exception($"Failed to connect to the remote endpoint: {ex.Message}.");
                }

                try
                {
                    //Log into the remote endpoint.
                    remoteClient.Security.Login(outgoingEndpoint.Username, outgoingEndpoint.PasswordHash);
                }
                catch (Exception ex)
                {
                    EnableControl(buttonAdd, false);
                    throw new Exception($"Failed to login to the remote endpoint: {ex.Message}.");
                }

                //Add the outgoing endpoint config to the local endpoint instance.
                _client.OutgoingEndpoint.Add(outgoingEndpoint).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        EnableControl(buttonAdd, false);
                        throw new Exception("Failed to create local outgoing endpoint.");
                    }

                    //Add the incoming endpoint config to the remote endpoint instance.
                    remoteClient.IncomingEndpoint.Add(incomingEndpoint).ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully)
                        {
                            //If we failed to create the remote endpoint config, remove the local config.
                            _client.OutgoingEndpoint.Delete(outgoingEndpoint.PairId).ContinueWith(t =>
                            {
                                EnableControl(buttonAdd, false);
                                throw new Exception("Failed to create remote incoming endpoint.");
                            });
                        }

                        CloseFormWithResult(DialogResult.OK);
                    });
                });
                */

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK);
            }
        }
    }
}
