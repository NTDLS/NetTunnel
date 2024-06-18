using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Service.ReliableMessageHandlers;
using NetTunnel.Service.TunnelEngine.Managers;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal class ServiceEngine
    {
        /// <summary>
        /// Contains a list of the connections that have been made TO the local service and the connection state info.
        /// </summary>
        public Dictionary<Guid, ServiceConnectionState> ServiceConnectionStates { get; private set; } = new();


        /// <summary>
        /// Logging provider for event log, console (and file?).
        /// </summary>
        public Logger Logging { get; private set; }

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
            Logging = new(this);
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
            _messageServer.AddHandler(new ServiceQueryHandlers());

            _messageServer.OnConnected += ServiceEngine_OnConnected;
            _messageServer.OnDisconnected += ServiceEngine_OnDisconnected;

            _messageServer.OnException += (RmContext? context, Exception ex, IRmPayload? payload) =>
            {
                Logging.Write(NtLogSeverity.Exception, $"RPC server exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }


        /// <summary>
        ///SEARCH FOR: Process:Endpoint:Connect:003: The local client is communicating through the tunnel that an inbound endpoint
        ///  connection has been made so that it can make the associated outbound endpoint connection.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <param name="streamId"></param>
        public void SendNotificationOfEndpointConnect(Guid connectionId, Guid tunnelId, Guid endpointId, Guid streamId)
        {
            _messageServer.Notify(connectionId, new NotificationEndpointConnect(tunnelId, endpointId, streamId));
        }

        public void SendNotificationOfEndpointDataExchange(Guid connectionId, Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            _messageServer.Notify(connectionId, new NotificationEndpointDataExchange(tunnelId, endpointId, streamId, bytes, length));
        }

        private void ServiceEngine_OnConnected(RmContext context)
        {
            ServiceConnectionStates.Add(context.ConnectionId,
                new ServiceConnectionState(context.ConnectionId, $"{context.TcpClient.Client.RemoteEndPoint}"));
        }

        private void ServiceEngine_OnDisconnected(RmContext context)
        {
            ServiceConnectionStates.Remove(context.ConnectionId);
        }

        public void Start()
        {
            _messageServer.SetCryptographyProvider(new ServiceCryptographyProvider(this));

            _messageServer.Start(Singletons.Configuration.ManagementPort);

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
