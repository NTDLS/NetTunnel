using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Library.ReliableMessages.Query;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Library
{
    /// <summary>
    /// Used by both the UI to connect to a service and the service to connect to other services.
    /// </summary>
    public class NtServiceClient
    {
        public RmClient Client { get; private set; }

        public NtServiceClient(RmClient client)
        {
            Client = client;
        }

        #region Factory.

        public static NtServiceClient CreateAndLogin(string address, int port, string username, string passwordHash)
        {
            return CreateAndLogin(new NtServiceConfiguration(), address, port, username, passwordHash);
        }

        public static NtServiceClient CreateAndLogin(NtServiceConfiguration configuration,
             string address, int port, string username, string passwordHash)
        {
            var client = new RmClient(new RmConfiguration()
            {
                //Parameter = this,
                InitialReceiveBufferSize = configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = configuration.ReceiveBufferGrowthRate,
            });

            client.Connect(address, port);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationToken = compoundNegotiator.GenerateNegotiationToken(configuration.TunnelCryptographyKeySize);

            //The first thing we do when we get a connection is start a new key exchange process.
            var queryRequestKeyExchangeReply = client.Query(
                new QueryRequestKeyExchange(negotiationToken), configuration.MessageQueryTimeoutMs).Result;

            //We received a reply to the secure key exchange, apply it.
            compoundNegotiator.ApplyNegotiationResponseToken(queryRequestKeyExchangeReply.NegotiationToken);

            var outboundTunnelConnection = new ClientConnectionContext(queryRequestKeyExchangeReply.ConnectionId);
            client.Parameter = outboundTunnelConnection;

            //Prop up encryption.
            var cryptographyProvider = new ClientCryptographyProvider(compoundNegotiator.SharedSecret);

            //Tell the server we are switching to encryption.
            client.Notify(new NotificationApplyCryptography());
            client.SetCryptographyProvider(cryptographyProvider);

            //Login
            var login = client.Query(new QueryLogin(username, passwordHash)).Result;
            if (login.Successful)
            {
                return new NtServiceClient(client);
            }

            throw new Exception("Login failed.");
        }

        #endregion

        public async Task<GetInboundTunnelsReply> GetInboundTunnels()
        {
            return await Client.Query(new QueryGetInboundTunnels());
        }

        public async Task<GetOutboundTunnelsReply> GetOutboundTunnels()
        {
            return await Client.Query(new QueryGetOutboundTunnels());
        }

        public async Task<QueryCreateInboundTunnelReply> CreateInboundTunnel(NtTunnelInboundConfiguration configuration)
        {
            return await Client.Query(new QueryCreateInboundTunnel(configuration));
        }
    }
}
