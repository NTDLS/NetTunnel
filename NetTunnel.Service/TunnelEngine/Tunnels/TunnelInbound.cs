using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.NASCCL;
using NTDLS.SecureKeyExchange;
using System.Net;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : BaseTunnel, ITunnel
    {
        private Thread? _inboundConnectionThread;
        public int DataPort { get; private set; }

        private readonly TcpListener _listener;

        public TunnelInbound(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
            : base(core, configuration)
        {
            DataPort = configuration.DataPort;

            _listener = new TcpListener(IPAddress.Any, DataPort);
        }

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(TunnelId, Name, DataPort);

            foreach (var endpoint in Endpoints)
            {
                if (endpoint is EndpointInbound inboundEndpoint)
                {
                    var endpointConfiguration = new NtEndpointInboundConfiguration(TunnelId, inboundEndpoint.EndpointId, inboundEndpoint.Name, inboundEndpoint.TransmissionPort);
                    tunnelConfiguration.EndpointInboundConfigurations.Add(endpointConfiguration);
                }
                else if (endpoint is EndpointOutbound outboundEndpoint)
                {
                    var endpointConfiguration = new NtEndpointOutboundConfiguration(TunnelId, outboundEndpoint.EndpointId, outboundEndpoint.Name, outboundEndpoint.Address, outboundEndpoint.TransmissionPort);
                    tunnelConfiguration.EndpointOutboundConfigurations.Add(endpointConfiguration);
                }
            }

            return tunnelConfiguration;
        }

        public override void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting inbound tunnel '{Name}' on port {DataPort}.");
            base.Start();

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting endpoints for inbound tunnel '{Name}'.");
            Endpoints.ForEach(x => x.Start());
        }

        public override void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopping inbound tunnel '{Name}' on port {DataPort}.");
            base.Stop();

            Utility.TryAndIgnore(_listener.Stop);
            Utility.TryAndIgnore(_listener.Stop);

            if (Environment.CurrentManagedThreadId != _inboundConnectionThread?.ManagedThreadId)
            {
                _inboundConnectionThread?.Join(); //Wait on thread to finish.
            }

            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopped inbound tunnel '{Name}'.");
        }

        private void InboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"InboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            try
            {
                _listener.Start();

                Core.Logging.Write(NtLogSeverity.Verbose, $"Started listiening for inbound tunnel '{Name}' on port {DataPort}.");

                while (KeepRunning)
                {
                    try
                    {
                        Status = NtTunnelStatus.Connecting;

                        Core.Logging.Write(NtLogSeverity.Verbose, $"Waiting on connection for inbound tunnel '{Name}' on port {DataPort}.");

                        var tcpClient = _listener.AcceptTcpClient();
                        Core.Logging.Write(NtLogSeverity.Verbose, $"Accepted connection for inbound tunnel '{Name}' on port {DataPort}.");

                        if (KeepRunning)
                        {
                            TotalConnections++;
                            CurrentConnections++;

                            Status = NtTunnelStatus.Established;

                            using (Stream = tcpClient.GetStream())
                            {
                                //The first thing we do when a client connects is we start a new key exhange process.
                                var compoundNegotiator = new CompoundNegotiator();
                                byte[] negotiationToken = compoundNegotiator.GenerateNegotiationToken(Singletons.Configuration.TunnelEncryptionKeySize / 12);

                                var query = new NtFramePayloadRequestKeyExchange(negotiationToken);
                                SendStreamFramePayloadQuery<NtFramePayloadKeyExchangeReply>(query).ContinueWith((o) =>
                                {
                                    if (o.IsCompletedSuccessfully && o.Result != null)
                                    {
                                        compoundNegotiator.ApplyNegotiationResponseToken(o.Result.NegotiationToken);
                                        EncryptionKey = compoundNegotiator.SharedSecret;
                                        NascclStream = new NASCCLStream(EncryptionKey);
                                        SecureKeyExchangeIsComplete = true;
                                    }
                                });

                                ReceiveAndProcessStreamFrames(ProcessFrameNotificationCallback, ProcessFrameQueryCallback);
                                Utility.TryAndIgnore(Stream.Close);
                            }

                            Status = NtTunnelStatus.Disconnected;

                            Core.Logging.Write(NtLogSeverity.Verbose, $"Disconnected inbound tunnel '{Name}' on port {DataPort}");

                            Utility.TryAndIgnore(tcpClient.Close);
                            Utility.TryAndIgnore(tcpClient.Dispose);
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Logging.Write(NtLogSeverity.Exception, $"InboundConnectionThreadProc: {ex.Message}");
                    }
                    finally
                    {
                        CurrentConnections--;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"InboundConnectionThreadProc: {ex.Message}");
            }
            finally
            {
                Utility.TryAndIgnore(_listener.Stop);
            }
        }
    }
}
