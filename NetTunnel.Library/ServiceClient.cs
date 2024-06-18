﻿using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Library.ReliableMessages.Query;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages;
using NTDLS.NullExtensions;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Library
{
    /// <summary>
    /// Used by both the UI to connect to a service and the service to connect to other services.
    /// </summary>
    public class ServiceClient
    {
        public RmClient Client { get; private set; }

        private readonly ServiceConfiguration _configuration;
        private readonly string _address;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _passwordHash;

        /// <summary>
        /// The id of the service that we are logged into.
        /// </summary>
        public Guid ServiceId { get; private set; }

        public ServiceClient(ServiceConfiguration configuration, RmClient client, string address, int port, string userName, string passwordHash)
        {
            _configuration = configuration;
            Client = client;
            _address = address;
            _port = port;
            _userName = userName;
            _passwordHash = passwordHash;
        }

        #region Factory.

        public static async Task<ServiceClient> CreateConnectAndLogin(string address, int port, string userName, string passwordHash, object? owner = null)
        {
            return await CreateConnectAndLogin(new ServiceConfiguration(), address, port, userName, passwordHash, owner);
        }

        public static async Task<ServiceClient> CreateConnectAndLogin(ServiceConfiguration configuration,
             string address, int port, string userName, string passwordHash, object? owner = null)
        {
            var serviceClient = Create(configuration, address, port, userName, passwordHash, owner);
            await serviceClient.ConnectAndLogin();
            return serviceClient;
        }

        public static ServiceClient Create(ServiceConfiguration configuration, string address, int port, string userName, string passwordHash, object? owner = null)
        {
            var client = new RmClient(new RmConfiguration()
            {
                Parameter = owner,
                InitialReceiveBufferSize = configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = configuration.ReceiveBufferGrowthRate,
            });

            return new ServiceClient(configuration, client, address, port, userName, passwordHash);
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

            ServiceId = login.ServiceId.EnsureNotNullOrEmpty();
        }

        public double Ping()
        {
            return Client.Query(new QueryPing()).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    return (DateTime.UtcNow - t.Result.OriginationTimestamp).TotalMilliseconds;
                }
                return 0;
            }).Result;
        }

        public async Task<QueryCreateTunnelReply> QueryCreateTunnel(TunnelConfiguration configuration)
            => await Client.Query(new QueryCreateTunnel(configuration));

        public async Task<QueryGetTunnelsReply> QueryGetTunnels()
            => await Client.Query(new QueryGetTunnels());

        public async Task<QueryRegisterTunnelReply> QueryRegisterTunnel(TunnelConfiguration Collection)
            => await Client.Query(new QueryRegisterTunnel(Collection));

        public async Task<QueryUpsertEndpointReply> QueryUpsertEndpoint(Guid tunnelId, EndpointConfiguration configuration)
            => await Client.Query(new QueryUpsertEndpoint(tunnelId, configuration));

        public void NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
            => Client.Notify(new NotificationEndpointConnect(tunnelId, endpointId, streamId));

        public void NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
            => Client.Notify(new NotificationEndpointDataExchange(tunnelId, endpointId, streamId, bytes, length));
    }
}