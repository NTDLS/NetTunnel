﻿using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Notification;
using NetTunnel.Library.ReliablePayloads.Query;
using NetTunnel.Service.ReliableMessages;
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
        }

        #region Factory.

        public static async Task<ServiceClient> CreateConnectAndLogin(ILogger logger, string address, int port, string userName, string passwordHash, object? owner = null)
        {
            //using var logger = new ConsoleLogger(NtLogSeverity.Warning);
            return await CreateConnectAndLogin(logger, new ServiceConfiguration()
            {
                MessageQueryTimeoutMs = 1000
            }, address, port, userName, passwordHash, owner);
        }

        public static async Task<ServiceClient> CreateConnectAndLogin(ILogger logger, ServiceConfiguration configuration,
             string address, int port, string userName, string passwordHash, object? owner = null)
        {
            var serviceClient = Create(logger, configuration, address, port, userName, passwordHash, owner);
            await serviceClient.ConnectAndLogin();
            return serviceClient;
        }

        public static ServiceClient Create(ILogger logger, ServiceConfiguration configuration,
            string address, int port, string userName, string passwordHash, object? owner = null)
        {
            var client = new RmClient(new RmConfiguration()
            {
                Parameter = owner,
                InitialReceiveBufferSize = configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = configuration.ReceiveBufferGrowthRate,
            });

            client.OnException += (RmContext? context, Exception ex, IRmPayload? payload) =>
            {
                logger.Exception($"RPC client exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };

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

        public async Task ConnectAndLogin()
        {
            Client.ClearCryptographyProvider();

            Client.Connect(_address, _port);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationToken = compoundNegotiator.GenerateNegotiationToken(_configuration.TunnelCryptographyKeySize);

            //The first thing we do when we get a connection is start a new key exchange process.
            var queryRequestKeyExchangeReply = await Client.Query(
                new QueryRequestKeyExchange(negotiationToken), _configuration.MessageQueryTimeoutMs);

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
            var login = await Client.Query(new QueryLogin(_userName, _passwordHash));
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

        public async Task<QueryGetTunnelStatisticsReply> QueryGetTunnelStatistics()
            => await Client.Query(new QueryGetTunnelStatistics());

        public async Task<QueryCreateTunnelReply> QueryCreateTunnel(TunnelConfiguration configuration)
            => await Client.Query(new QueryCreateTunnel(configuration));

        public async Task<QueryDeleteTunnelReply> QueryDeleteTunnel(DirectionalKey tunnelKey)
            => await Client.Query(new QueryDeleteTunnel(tunnelKey));

        public async Task<QueryGetTunnelsReply> QueryGetTunnels()
            => await Client.Query(new QueryGetTunnels());

        public async Task<QueryRegisterTunnelReply> QueryRegisterTunnel(TunnelConfiguration Collection)
            => await Client.Query(new QueryRegisterTunnel(Collection));

        public async Task<QueryUpsertEndpointReply> QueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
            => await Client.Query(new QueryUpsertEndpoint(tunnelKey, configuration));

        public async Task<QueryGetUsersReply> QueryGetUsers()
            => await Client.Query(new QueryGetUsers());

        public void NotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid streamId)
            => Client.Notify(new NotificationEndpointConnect(tunnelKey, endpointId, streamId));

        public void SendNotificationOfTunnelDeletion(DirectionalKey tunnelKey)
            => Client.Notify(new NotificationTunnelDeletion(tunnelKey));

        public void NotificationEndpointExchange(DirectionalKey tunnelKey, Guid endpointId, Guid streamId, byte[] bytes, int length)
            => Client.Notify(new NotificationEndpointDataExchange(tunnelKey, endpointId, streamId, bytes, length));
    }
}
