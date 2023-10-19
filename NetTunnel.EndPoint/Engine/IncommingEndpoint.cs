using NetTunnel.Library.Types;

namespace NetTunnel.EndPoint.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote endpoint.
    /// </summary>
    public class IncommingEndpoint
    {
        public NtIncommingEndpointConfig _configuration;
        public Guid Id { get => _configuration.Id; }

        public IncommingEndpoint(NtIncommingEndpointConfig config)
        {
            _configuration = config;
        }

        public NtIncommingEndpointConfig CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
