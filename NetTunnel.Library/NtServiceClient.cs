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

        public static async Task<NtServiceClient> CreateAndLogin(string address, int port, string username, string passwordHash)
        {
            return await CreateAndLogin(new NtServiceConfiguration(), address, port, username, passwordHash);
        }

        public static async Task<NtServiceClient> CreateAndLogin(NtServiceConfiguration configuration,
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
            var queryRequestKeyExchangeReply = await client.Query(
                new QueryRequestKeyExchange(negotiationToken), configuration.MessageQueryTimeoutMs);

            //We received a reply to the secure key exchange, apply it.
            compoundNegotiator.ApplyNegotiationResponseToken(queryRequestKeyExchangeReply.NegotiationToken);

            var tunnelConnection = new ClientConnectionContext(queryRequestKeyExchangeReply.ConnectionId);
            client.Parameter = tunnelConnection;

            //Prop up encryption.
            var cryptographyProvider = new ClientCryptographyProvider(compoundNegotiator.SharedSecret);

            //Tell the server we are switching to encryption.
            client.Notify(new NotificationApplyCryptography());
            client.SetCryptographyProvider(cryptographyProvider);

            //Login.
            var login = await client.Query(new QueryLogin(username, passwordHash));
            if (login.Successful)
            {
                return new NtServiceClient(client);
            }

            throw new Exception("Login failed.");
        }

        #endregion

        public async Task<QueryGetTunnelsReply> GetTunnels()
        {
            return await Client.Query(new QueryGetTunnels());
        }

        /*
        public void UpsertEndpointInboundPair()
        {
            var endpoint = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value).EnsureNotNull();

            //Add the inbound endpoint to the local tunnel.
            Singletons.Core.InboundTunnels.UpsertEndpointInbound(tunnelId, endpoint.Inbound);
            Singletons.Core.InboundTunnels.SaveToDisk();

            //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
            var result = await Singletons.Core.InboundTunnels
                .DispatchUpsertEndpointOutboundToAssociatedTunnelService<oldQueryReplyPayloadBoolean>(tunnelId, endpoint.Outbound);

        }
        */
    }
}
