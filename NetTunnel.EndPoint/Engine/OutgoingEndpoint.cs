using NetTunnel.Library.Types;

namespace NetTunnel.EndPoint.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening endpoint.
    /// </summary>
    public class OutgoingEndpoint
    {
        public NtOutgoingEndpointConfig _configuration;
        public Guid Id { get => _configuration.Id; }

        public OutgoingEndpoint(NtOutgoingEndpointConfig config)
        {
            _configuration = config;
        }

        public NtOutgoingEndpointConfig CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
