using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Notification.ServiceToService;
using NetTunnel.Library.ReliablePayloads.Notification.UI;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Service.ReliableHandlers.Service.Notifications;
using NetTunnel.Service.ReliableHandlers.Service.Queries;
using NetTunnel.Service.TunnelEngine.Managers;
using NTDLS.ReliableMessaging;
using NTDLS.Semaphore;
using System.Diagnostics.CodeAnalysis;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal class ServiceEngine : IServiceEngine
    {
        /// <summary>
        /// Contains a list of the connections that have been made TO the local service and their connection state info.
        /// </summary>
        public PessimisticCriticalResource<Dictionary<Guid, ServiceConnectionState>> ServiceConnectionStates { get; private set; } = new();

        /// <summary>
        /// Contains the information for all tunnels, inbound and outbound. Keep in mind that we only persist
        ///     outbound connection information and that inbound tunnels are simply ephemeral registrations. 
        /// </summary>
        public TunnelManager Tunnels { get; private set; }

        /// <summary>
        /// Contains a list of users and their password hashes.
        /// </summary>
        public UserManager Users { get; private set; }

        /// <summary>
        /// The message server that accepts all inbound tunnel connections and sends/receives all messages for all tunnels.
        /// </summary>
        private readonly RmServer _messageServer;

        public ServiceEngine()
        {
            Tunnels = new(this);
            Users = new(this);

            _messageServer = new RmServer();

            _messageServer = new RmServer(new RmConfiguration()
            {
                Parameter = this,
                //FrameDelimiter = Singletons.Configuration.FrameDelimiter,
                InitialReceiveBufferSize = Singletons.Configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = Singletons.Configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = Singletons.Configuration.ReceiveBufferGrowthRate,
            });

            _messageServer.AddHandler(new ServiceNotificationHandlers());
            _messageServer.AddHandler(new ServiceQueryHandlersForUI());
            _messageServer.AddHandler(new ServiceQueryHandlersForServiceToService());
            _messageServer.AddHandler(new ServiceQueryHandlersForServiceToServiceOrUI());

            _messageServer.OnConnected += ServiceEngine_OnConnected;
            _messageServer.OnDisconnected += ServiceEngine_OnDisconnected;

            _messageServer.OnException += (RmContext? context, Exception ex, IRmPayload? payload) =>
            {
                Singletons.Logger.Exception($"RPC server exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };

            Singletons.Logger.OnLog += (DateTime dateTime, NtLogSeverity severity, string message) =>
            {
                //Loop through all UI connections.
                var uiConnectionIds = Singletons.ServiceEngine.ServiceConnectionStates.Use(o =>
                    o.Where(o => o.Value.LoginType == NtLoginType.UI).Select(o => o.Value.ConnectionId).ToList());

                foreach (var connectionId in uiConnectionIds)
                {
                    Singletons.ServiceEngine.UINotifyLog(connectionId, dateTime, severity, message);
                }
            };
        }




        public bool TryGetServiceConnectionState(Guid connectionId, [NotNullWhen(true)] out ServiceConnectionState? outState)
        {
            var state = ServiceConnectionStates.Use(o =>
            {
                o.TryGetValue(connectionId, out var connection);
                return connection;
            });

            outState = state;

            return state != null;
        }


        #region Interface: IServiceEngine

        public void UINotifyLog(Guid connectionId, DateTime timestamp, NtLogSeverity severity, string text)
            => _messageServer.Notify(connectionId, new UILoggerNotification(timestamp, severity, text));

        public S2SQueryUpsertEndpointReply S2SPeerQueryUpsertEndpoint(Guid connectionId, DirectionalKey tunnelKey, EndpointConfiguration endpoint)
            => _messageServer.Query(connectionId, new S2SQueryUpsertEndpoint(tunnelKey, endpoint)).Result;

        /// <summary>
        /// Sends a notification to the remote tunnel service to let it know to connect
        ///     the associated outbound endpoint for an incoming endpoint connection.
        ///     
        ///SEARCH FOR: Process:Endpoint:Connect:003: The local client is communicating through the tunnel that an inbound endpoint
        ///  connection has been made so that it can make the associated outbound endpoint connection.
        /// </summary>
        /// <param name="tunnelId">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        public void S2SPeerNotificationEndpointConnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => _messageServer.Notify(connectionId, new S2SNotificationEndpointConnect(tunnelKey, endpointId, edgeId));

        public void S2SPeerNotificationEndpointDisconnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => _messageServer.Notify(connectionId, new S2SNotificationEndpointDisconnect(tunnelKey, endpointId, edgeId));

        /// <summary>
        /// Notify the remote tunnel service that the tunnel is being deleted.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="tunnelId"></param>
        public void S2SPeerNotificationTunnelDeletion(Guid connectionId, DirectionalKey tunnelKey)
            => _messageServer.Notify(connectionId, new S2SNotificationTunnelDeletion(tunnelKey));

        /// <summary>
        /// Notify the remote tunnel service that the endpoint is being deleted.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="tunnelKey"></param>
        /// <param name="endpointId"></param>
        public void S2SPeerNotificationEndpointDeletion(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId)
            => _messageServer.Notify(connectionId, new S2SNotificationEndpointDeletion(tunnelKey, endpointId));

        /// <summary>
        /// Sends a notification to the remote tunnel service containing the data that was received
        ///     by an endpoint. This data is to be sent to the endpoint connection with the matching
        ///     edgeId (which was originally sent to S2SPeerNotificationEndpointConnect()
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        /// <param name="bytes">Bytes to be sent to endpoint connection.</param>
        /// <param name="length">Number of bytes to be sent to the endpoint connection.</param>
        public void S2SPeerNotificationEndpointDataExchange(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length)
            => _messageServer.Notify(connectionId, new S2SNotificationEndpointDataExchange(tunnelKey, endpointId, edgeId, bytes, length));

        #endregion

        private void ServiceEngine_OnConnected(RmContext context)
        {
            ServiceConnectionStates.Use(o => o.Add(context.ConnectionId,
                new ServiceConnectionState(context.ConnectionId, $"{context.TcpClient.Client.RemoteEndPoint}")));
        }

        private void ServiceEngine_OnDisconnected(RmContext context)
        {
            Tunnels.DeregisterTunnel(context.ConnectionId);
            ServiceConnectionStates.Use(o => o.Remove(context.ConnectionId));
        }

        public void Start()
        {
            _messageServer.SetCryptographyProvider(new ServiceCryptographyProvider(this));

            _messageServer.Start(Singletons.Configuration.ServicePort);

            Tunnels.StartAll();
        }

        public void Stop()
        {
            Tunnels.StopAll();

            _messageServer.Stop();

            _messageServer.ClearCryptographyProvider();
        }
    }
}
