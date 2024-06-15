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

        private readonly NtServiceConfiguration _configuration;
        private readonly string _address;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _passwordHash;

        public NtServiceClient(NtServiceConfiguration configuration, RmClient client, string address, int port, string userName, string passwordHash)
        {
            _configuration = configuration;
            Client = client;
            _address = address;
            _port = port;
            _userName = userName;
            _passwordHash = passwordHash;
        }

        #region Factory.

        public static async Task<NtServiceClient> CreateConnectAndLogin(string address, int port, string userName, string passwordHash, object? owner = null)
        {
            return await CreateConnectAndLogin(new NtServiceConfiguration(), address, port, userName, passwordHash, owner);
        }

        public static async Task<NtServiceClient> CreateConnectAndLogin(NtServiceConfiguration configuration,
             string address, int port, string userName, string passwordHash, object? owner = null)
        {
            var serviceClient = Create(configuration, address, port, userName, passwordHash, owner);
            await serviceClient.ConnectAndLogin();
            return serviceClient;
        }

        public static NtServiceClient Create(NtServiceConfiguration configuration, string address, int port, string userName, string passwordHash, object? owner = null)
        {
            var client = new RmClient(new RmConfiguration()
            {
                Parameter = owner,
                InitialReceiveBufferSize = configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = configuration.ReceiveBufferGrowthRate,
            });

            return new NtServiceClient(configuration, client, address, port, userName, passwordHash);
        }

        #endregion

        public bool IsConnected
            => Client.IsConnected;

        public void Disconnect()
            => Utility.TryAndIgnore(Client.Disconnect);

        public async Task ConnectAndLogin()
        {
            Client.Connect(_address, _port);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationToken = compoundNegotiator.GenerateNegotiationToken(_configuration.TunnelCryptographyKeySize);

            //The first thing we do when we get a connection is start a new key exchange process.
            var queryRequestKeyExchangeReply = await Client.Query(
                new QueryRequestKeyExchange(negotiationToken), _configuration.MessageQueryTimeoutMs);

            //We received a reply to the secure key exchange, apply it.
            compoundNegotiator.ApplyNegotiationResponseToken(queryRequestKeyExchangeReply.NegotiationToken);

            //var tunnelConnection = new ClientConnectionContext(queryRequestKeyExchangeReply.ConnectionId);
            //Client.Parameter = tunnelConnection;

            //Prop up encryption.
            var cryptographyProvider = new ClientCryptographyProvider(compoundNegotiator.SharedSecret);

            //Tell the server we are switching to encryption.
            Client.Notify(new NotificationApplyCryptography());
            Client.SetCryptographyProvider(cryptographyProvider);

            //Login.
            var login = await Client.Query(new QueryLogin(_userName, _passwordHash));
            if (login.Successful == false)
            {
                throw new Exception("Login failed.");
            }
        }

        public async Task<QueryGetTunnelsReply> GetTunnels()
        {
            return await Client.Query(new QueryGetTunnels());
        }

        public async Task<QueryCreateTunnelReply> CreateTunnel(NtTunnelConfiguration configuration)
        {
            return await Client.Query(new QueryCreateTunnel(configuration));
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
