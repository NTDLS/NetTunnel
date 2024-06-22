using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Notification;
using NetTunnel.Library.ReliablePayloads.Query;
using NetTunnel.Service.ReliableMessages;
using NetTunnel.Service.ReliableMessages.Notification;
using NTDLS.Helpers;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Library
{
    /// <summary>
    /// Used by both the UI to connect to a service and the service to connect to other services.
    /// </summary>
    public class ServiceClient
    {
        private readonly ServiceConfiguration _configuration;
        private readonly string _address;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _passwordHash;
        public string Address { get { return _address; } }

        /// <summary>
        /// The id of the service that we are logged into.
        /// </summary>
        public Guid ServiceId { get; private set; }
        public RmClient Client { get; private set; }
        public bool IsLoggedIn { get; private set; } = false;
        public ILogger _logger;

        public event ExceptionEvent? OnException;
        public delegate void ExceptionEvent(RmContext? context, Exception ex, IRmPayload? payload);

        public event ConnectedEvent? OnConnected;
        public delegate void ConnectedEvent(RmContext context);

        public event DisconnectedEvent? OnDisconnected;
        public delegate void DisconnectedEvent(RmContext context);

        public ServiceClient(ILogger logger, ServiceConfiguration configuration, RmClient client, string address, int port, string userName, string passwordHash)
        {
            _logger = logger;
            _configuration = configuration;
            Client = client;
            _address = address;
            _port = port;
            _userName = userName;
            _passwordHash = passwordHash;
            IsLoggedIn = false;

            client.OnException += Client_OnException;
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
        }

        private void Client_OnDisconnected(RmContext context)
            => OnDisconnected?.Invoke(context);

        private void Client_OnException(RmContext? context, Exception ex, IRmPayload? payload)
        {
            _logger.Exception($"RPC client exception: '{ex.Message}'"
                + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));

            OnException?.Invoke(context, ex, payload);
        }

        private void Client_OnConnected(RmContext context)
            => OnConnected?.Invoke(context);

        #region Factory.

        public static ServiceClient CreateConnectAndLogin(ILogger logger, string address, int port, string userName, string passwordHash)
        {
            return CreateConnectAndLogin(logger, new ServiceConfiguration()
            {
                MessageQueryTimeoutMs = 1000
            }, address, port, userName, passwordHash);
        }

        public static ServiceClient CreateConnectAndLogin(ILogger logger, ServiceConfiguration configuration,
             string address, int port, string userName, string passwordHash)
        {
            var serviceClient = Create(logger, configuration, address, port, userName, passwordHash);
            serviceClient.ConnectAndLogin();
            return serviceClient;
        }

        public static ServiceClient Create(ILogger logger, ServiceConfiguration configuration,
            string address, int port, string userName, string passwordHash, object? parameter = null)
        {
            var client = new RmClient(new RmConfiguration()
            {
                Parameter = parameter,
                InitialReceiveBufferSize = configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = configuration.ReceiveBufferGrowthRate,
            });

            return new ServiceClient(logger, configuration, client, address, port, userName, passwordHash);
        }

        #endregion

        public bool IsConnected
            => Client.IsConnected;

        public void Disconnect()
        {
            IsLoggedIn = false;
            Exceptions.Ignore(Client.Disconnect);
        }

        public void ConnectAndLogin()
        {
            Client.ClearCryptographyProvider();

            Client.Connect(_address, _port);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationToken = compoundNegotiator.GenerateNegotiationToken((int)(Math.Ceiling(_configuration.TunnelCryptographyKeySize / 128.0)));

            //The first thing we do when we get a connection is start a new key exchange process.
            var queryRequestKeyExchangeReply = Client.Query(
                new QueryRequestKeyExchange(negotiationToken), _configuration.MessageQueryTimeoutMs).Result;

            //We received a reply to the secure key exchange, apply it.
            compoundNegotiator.ApplyNegotiationResponseToken(queryRequestKeyExchangeReply.NegotiationToken);

            //Prop up encryption.
            var cryptographyProvider = new ClientCryptographyProvider(compoundNegotiator.SharedSecret);

            _logger.Verbose(
                $"Tunnel cryptography initialized to {compoundNegotiator.SharedSecret.Length * 8}bits. Hash {Utility.ComputeSha256Hash(compoundNegotiator.SharedSecret)}.");

            //Tell the server we are switching to encryption.
            Client.Notify(new NotificationApplyCryptography());
            Client.SetCryptographyProvider(cryptographyProvider);

            _logger.Verbose("Tunnel cryptography provider has been applied.");

            //Login.
            var login = Client.Query(new QueryLogin(_userName, _passwordHash)).Result;
            if (login.Successful == false)
            {
                throw new Exception("Login failed.");
            }
            else
            {
                ServiceId = login.ServiceId.EnsureNotNullOrEmpty();
                IsLoggedIn = true;
            }
        }

        public double Ping(DirectionalKey tunnelKey, double? previousPing)
        {
            var result = Client.Query(new QueryPing(tunnelKey, previousPing)).Result;
            return (DateTime.UtcNow - result.OriginationTimestamp).TotalMilliseconds;
        }

        public QueryGetTunnelStatisticsReply QueryGetTunnelStatistics()
            => Client.Query(new QueryGetTunnelStatistics()).Result;

        public QueryCreateTunnelReply QueryCreateTunnel(TunnelConfiguration configuration)
            => Client.Query(new QueryCreateTunnel(configuration)).Result;

        public QueryDeleteTunnelReply QueryDeleteTunnel(DirectionalKey tunnelKey)
            => Client.Query(new QueryDeleteTunnel(tunnelKey)).Result;

        public QueryDeleteEndpointReply QueryDeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId)
            => Client.Query(new QueryDeleteEndpoint(tunnelKey, endpointId)).Result;

        public QueryGetTunnelsReply QueryGetTunnels()
            => Client.Query(new QueryGetTunnels()).Result;

        public QueryRegisterTunnelReply QueryRegisterTunnel(TunnelConfiguration Collection)
            => Client.Query(new QueryRegisterTunnel(Collection)).Result;

        public QueryDistributeUpsertEndpointReply QueryDistributeUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
            => Client.Query(new QueryDistributeUpsertEndpoint(tunnelKey, configuration)).Result;

        public QueryGetUsersReply QueryGetUsers()
            => Client.Query(new QueryGetUsers()).Result;

        public QueryDeleteUserReply QueryDeleteUser(string userName)
            => Client.Query(new QueryDeleteUser(userName)).Result;

        public QueryGetServiceConfigurationReply QueryGetServiceConfiguration()
            => Client.Query(new QueryGetServiceConfiguration()).Result;

        public QueryPutServiceConfigurationReply QueryPutServiceConfiguration(ServiceConfiguration configuration)
            => Client.Query(new QueryPutServiceConfiguration(configuration)).Result;

        public QueryChangeUserPasswordReply QueryChangeUserPassword(string username, string passwordHash)
            => Client.Query(new QueryChangeUserPassword(username, passwordHash)).Result;

        public QueryStopTunnelReply QueryStopTunnel(DirectionalKey tunnelKey)
            => Client.Query(new QueryStopTunnel(tunnelKey)).Result;

        public QueryStartTunnelReply QueryStartTunnel(DirectionalKey tunnelKey)
            => Client.Query(new QueryStartTunnel(tunnelKey)).Result;

        public QueryCreateUserReply QueryCreateUser(User user)
            => Client.Query(new QueryCreateUser(user)).Result;

        public void NotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => Client.Notify(new NotificationEndpointConnect(tunnelKey, endpointId, edgeId));

        public void PeerNotifyOfEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => Client.Notify(new NotificationEndpointDisconnect(tunnelKey, endpointId, edgeId));

        public void PeerNotifyOfTunnelDeletion(DirectionalKey tunnelKey)
            => Client.Notify(new NotificationTunnelDeletion(tunnelKey));

        public void PeerNotifyOfEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
            => Client.Notify(new NotificationEndpointDeletion(tunnelKey, endpointId));

        public void NotificationEndpointExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length)
            => Client.Notify(new NotificationEndpointDataExchange(tunnelKey, endpointId, edgeId, bytes, length));

        public QueryUpsertEndpointReply PeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpoint)
            => Client.Query(new QueryUpsertEndpoint(tunnelKey, endpoint)).Result;
    }
}
