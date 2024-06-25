﻿using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Notification;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Library.ReliablePayloads.Query.UI;
using NetTunnel.Library.ReliablePayloads.Query.UIOrService;
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
            => CreateConnectAndLogin(logger, new ServiceConfiguration(), address, port, userName, passwordHash);

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
                new UOSQueryRequestKeyExchange(negotiationToken), _configuration.MessageQueryTimeoutMs).Result;

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
            var login = Client.Query(new UOSQueryLogin(_userName, _passwordHash)).Result;
            if (login.Successful == false)
            {
                Exceptions.Ignore(() => Client.Disconnect());
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
            var result = Client.Query(new S2SQueryPing(tunnelKey, previousPing)).Result;
            return (DateTime.UtcNow - result.OriginationTimestamp).TotalMilliseconds;
        }

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

        public UIQueryDeleteTunnelReply UIQueryDeleteTunnel(DirectionalKey tunnelKey)
            => Client.Query(new UIQueryDeleteTunnel(tunnelKey)).Result;

        public UIQueryDeleteEndpointReply UIQueryDeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId)
            => Client.Query(new UIQueryDeleteEndpoint(tunnelKey, endpointId)).Result;

        public UIQueryGetTunnelsReply UIQueryGetTunnels()
            => Client.Query(new UIQueryGetTunnels()).Result;

        public S2SQueryRegisterTunnelReply S2SQueryRegisterTunnel(TunnelConfiguration Collection)
            => Client.Query(new S2SQueryRegisterTunnel(Collection)).Result;

        public UIQueryDistributeUpsertEndpointReply UIQueryDistributeUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
            => Client.Query(new UIQueryDistributeUpsertEndpoint(tunnelKey, configuration)).Result;

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

        public void NotifyTerminateEndpointEdgeConnection(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => Client.Notify(new NotifyTerminateEndpointEdgeConnection(tunnelKey, endpointId, edgeId));

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

        public S2SQueryUpsertEndpointReply PeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpoint)
            => Client.Query(new S2SQueryUpsertEndpoint(tunnelKey, endpoint)).Result;
    }
}
