using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    public class EndpointInbound
    {
        private readonly EngineCore _core;
        private NtEndpointInboundConfiguration _configuration;
        private Thread? _incomingConnectionThread;
        private bool _keepRunning = false;
        private List<Thread> _handlerThreads = new();

        public Guid Id { get => _configuration.Id; }

        public EndpointInbound(EngineCore core, NtEndpointInboundConfiguration config)
        {
            _core = core;
            _configuration = config;
        }

        public NtEndpointInboundConfiguration CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting incoming endpoint '{_configuration.Name}' on port {_configuration.ListenPort}");

            _incomingConnectionThread = new Thread(IncomingConnectionThreadProc);
            _incomingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        void IncomingConnectionThreadProc()
        {
            var listener = new TcpListener(IPAddress.Any, _configuration.ListenPort);

            try
            {
                listener.Start();

                _core.Logging.Write($"Listening incoming endpoint '{_configuration.Name}' on port {_configuration.ListenPort}");

                while (_keepRunning)
                {
                    _core.Logging.Write($"Waiting for connection for incoming endpoint '{_configuration.Name}' on port {_configuration.ListenPort}");

                    var client = listener.AcceptTcpClient();

                    _core.Logging.Write($"Accepted on incoming endpoint '{_configuration.Name}' on port {_configuration.ListenPort}");

                    var handlerThread = new Thread(HandleClientThreadProc);
                    _handlerThreads.Add(handlerThread);
                    handlerThread.Start();

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Stop listening and close the listener when done.
                listener.Stop();
            }
        }

        private void HandleClientThreadProc(object? obj)
        {
            Utility.EnsureNotNull(obj);
            var client = (TcpClient)obj;

            //Here we need to tell the remote service to start an outgoing connection for this endpoint and on its owning tunnel.


            //
            //_configuration.TunnelId
            //this.Id


            while (_keepRunning)
            {
                //TODO: Pump endpoint data.
                Thread.Sleep(1);
            }

            /*
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //stream.Write().
            }
            */

            client.Close();
        }
    }
}
