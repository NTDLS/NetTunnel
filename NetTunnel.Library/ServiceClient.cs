using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Notification.ServiceToService;
using NetTunnel.Library.ReliablePayloads.Notification.UI;
using NetTunnel.Library.ReliablePayloads.Notification.UIOrService;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Library.ReliablePayloads.Query.UI;
using NetTunnel.Library.ReliablePayloads.Query.UIOrService;
using NetTunnel.Service.ReliableMessages;
using NTDLS.Helpers;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;
using static NetTunnel.Library.Constants;

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

        public NtUserRole Role { get; private set; } = NtUserRole.Undefined;

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

        public static ServiceClient UICreateConnectAndLogin(ILogger logger, string address, int port, string userName, string passwordHash)
            => UICreateConnectAndLogin(logger, new ServiceConfiguration(), address, port, userName, passwordHash);

        public static ServiceClient UICreateConnectAndLogin(ILogger logger,
            ServiceConfiguration configuration, string address, int port, string userName, string passwordHash)
        {
            var serviceClient = Create(logger, configuration, address, port, userName, passwordHash);
            serviceClient.Client.Parameter = serviceClient;
            serviceClient.ConnectAndLogin(NtLoginType.UI);
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
            Exceptions.Ignore(() => Client.Disconnect(false));
        }

        public void ConnectAndLogin(NtLoginType loginType)
        {
            Client.ClearCryptographyProvider();

            Client.Connect(_address, _port);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationToken = compoundNegotiator.GenerateNegotiationToken((int)(Math.Ceiling(_configuration.TunnelCryptographyKeySize / 128.0)));

            //The first thing we do when we get a connection is start a new key exchange process.
            var queryRequestKeyExchangeReply = Client.Query(
                new UOSQueryRequestKeyExchange(negotiationToken), _configuration.MessageQueryTimeoutMs).Result;

            //We received a reply to the secure key exchange, apply it.
            compoundNegotiator.ApplyNegotiationResponseToken(queryRequestKeyExchangeReply.NegotiationToken);

            //Prop up encryption.
            var cryptographyProvider = new ClientCryptographyProvider(compoundNegotiator.SharedSecret);

            _logger.Verbose(
                $"Tunnel cryptography initialized to {compoundNegotiator.SharedSecret.Length * 8}bits. Hash {Utility.ComputeSha256Hash(compoundNegotiator.SharedSecret)}.");

            //Tell the server we are switching to encryption.
            Client.Notify(new UOSNotificationApplyCryptography());
            Client.SetCryptographyProvider(cryptographyProvider);

            _logger.Verbose("Tunnel cryptography provider has been applied.");

            //Login.
            var login = Client.Query(new UOSQueryLogin(_userName, _passwordHash, loginType)).Result;
            if (login.Successful == false)
            {
                Exceptions.Ignore(() => Client.Disconnect());
                throw new Exception("Login failed.");
            }
            else
            {
                Role = login.UserRole;
                ServiceId = login.ServiceId.EnsureNotNullOrEmpty();
                IsLoggedIn = true;
            }
        }

        #region Service-to-Service.

        public double S2SQueryPing(DirectionalKey tunnelKey, double? previousPing)
        {
            var result = Client.Query(new S2SQueryPing(tunnelKey, previousPing)).Result;
            return (DateTime.UtcNow - result.OriginationTimestamp).TotalMilliseconds;
        }

        public S2SQueryRegisterTunnelReply S2SQueryRegisterTunnel(TunnelConfiguration Collection)
            => Client.Query(new S2SQueryRegisterTunnel(Collection)).Result;

        public void S2SNotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => Client.Notify(new S2SNotificationEndpointConnect(tunnelKey, endpointId, edgeId));

        public void S2SPeerNotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => Client.Notify(new S2SNotificationEndpointDisconnect(tunnelKey, endpointId, edgeId));

        public void S2SPeerNotificationTunnelDeletion(DirectionalKey tunnelKey)
            => Client.Notify(new S2SNotificationTunnelDeletion(tunnelKey));

        public void S2SPeerNotificationEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
            => Client.Notify(new S2SNotificationEndpointDeletion(tunnelKey, endpointId));

        public void S2SNotificationEndpointExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length)
            => Client.Notify(new S2SNotificationEndpointDataExchange(tunnelKey, endpointId, edgeId, bytes, length));

        public S2SQueryUpsertEndpointReply S2SPeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpoint)
            => Client.Query(new S2SQueryUpsertEndpoint(tunnelKey, endpoint)).Result;

        #endregion

        #region User Interface (UI).

        public UIQueryGetTunnelStatisticsReply UIQueryGetTunnelStatistics()
            => Client.Query(new UIQueryGetTunnelStatistics()).Result;

        public UIQueryCreateTunnelReply UIQueryCreateTunnel(TunnelConfiguration configuration)
            => Client.Query(new UIQueryCreateTunnel(configuration)).Result;

        public UIQueryGetEndpointPropertiesReply UIQueryGetEndpointProperties(DirectionalKey tunnelKey, DirectionalKey endpointKey)
            => Client.Query(new UIQueryGetEndpointProperties(tunnelKey, endpointKey)).Result;

        public UIQueryGetEndpointEdgeConnectionsReply UIQueryGetEndpointEdgeConnections(DirectionalKey tunnelKey, DirectionalKey endpointKey)
            => Client.Query(new UIQueryGetEndpointEdgeConnections(tunnelKey, endpointKey)).Result;

        public UIQueryGetTunnelPropertiesReply UIQueryGetTunnelProperties(DirectionalKey tunnelKey)
            => Client.Query(new UIQueryGetTunnelProperties(tunnelKey)).Result;

        public UIQueryDisconnectTunnelReply UIQueryDisconnectTunnel(DirectionalKey tunnelKey)
            => Client.Query(new UIQueryDisconnectTunnel(tunnelKey)).Result;

        public UIQueryDeleteTunnelReply UIQueryDeleteTunnel(DirectionalKey tunnelKey)
            => Client.Query(new UIQueryDeleteTunnel(tunnelKey)).Result;

        public UIQueryDeleteEndpointReply UIQueryDeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId)
            => Client.Query(new UIQueryDeleteEndpoint(tunnelKey, endpointId)).Result;

        public UIQueryGetTunnelsReply UIQueryGetTunnels()
            => Client.Query(new UIQueryGetTunnels()).Result;

        public UIQueryDistributeUpsertEndpointReply UIQueryDistributeUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
            => Client.Query(new UIQueryDistributeUpsertEndpoint(tunnelKey, configuration)).Result;

        public UIQueryUpsertUserEndpointReply UIQueryUpsertUserEndpoint(string username, EndpointConfiguration configuration)
            => Client.Query(new UIQueryUpsertUserEndpoint(username, configuration)).Result;

        public UIQueryGetUsersReply UIQueryGetUsers()
            => Client.Query(new UIQueryGetUsers()).Result;

        public UIQueryDeleteUserReply UIQueryDeleteUser(string userName)
            => Client.Query(new UIQueryDeleteUser(userName)).Result;

        public UIQueryGetServiceConfigurationReply UIQueryGetServiceConfiguration()
            => Client.Query(new UIQueryGetServiceConfiguration()).Result;

        public UIQueryPutServiceConfigurationReply UIQueryPutServiceConfiguration(ServiceConfiguration configuration)
            => Client.Query(new UIQueryPutServiceConfiguration(configuration)).Result;

        public UIQueryEditUserReply UIQueryEditUser(User user)
            => Client.Query(new UIQueryEditUser(user)).Result;

        public UIQueryStopTunnelReply UIQueryStopTunnel(DirectionalKey tunnelKey)
            => Client.Query(new UIQueryStopTunnel(tunnelKey)).Result;

        public UIQueryStartTunnelReply UIQueryStartTunnel(DirectionalKey tunnelKey)
            => Client.Query(new UIQueryStartTunnel(tunnelKey)).Result;

        public UIQueryCreateUserReply UIQueryCreateUser(User user)
            => Client.Query(new UIQueryCreateUser(user)).Result;

        public void UINotifyTerminateEndpointEdgeConnection(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => Client.Notify(new UINotifyTerminateEndpointEdgeConnection(tunnelKey, endpointId, edgeId));

        #endregion
    }
}
