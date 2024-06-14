using NetTunnel.Library.ReliableMessages;
using NetTunnel.Library.ReliableMessages.Query;
using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Library
{
    public static class ClientFactory
    {
        public static RmClient Establish(NtServiceConfiguration configuration, string address, int port, string username, string passwordHash)
        {
            var client = new RmClient(new RmConfiguration()
            {
                //Parameter = this,
                InitialReceiveBufferSize = configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = configuration.ReceiveBufferGrowthRate,
            });

            client.Connect(address, port);

            //The first thing we do when we get a connection is start a new key exchange process.
            var compoundNegotiator = new CompoundNegotiator();
            var negotiationToken = compoundNegotiator.GenerateNegotiationToken(configuration.TunnelCryptographyKeySize);

            var query = new QueryRequestKeyExchange(negotiationToken);
                client.Query(query, configuration.MessageQueryTimeoutMs).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && t.Result != null)
                {
                    compoundNegotiator.ApplyNegotiationResponseToken(t.Result.NegotiationToken);

                    var outboundTunnelConnection = new OutboundTunnelConnection(client, t.Result.ConnectionId);
                    client.Parameter = outboundTunnelConnection;

                    outboundTunnelConnection.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

                    client.Notify(new NotificationApplyCryptography());

                    outboundTunnelConnection.ApplyCryptographyProvider();
                }
            });

            client.Query(new QueryLogin(username, passwordHash)).ContinueWith(o =>
            {
                if (!o.IsCompletedSuccessfully || !o.Result.Successful)
                {
                    return;
                }
            });

            return client;
        }
    }
}
