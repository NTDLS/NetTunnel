using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public interface ITunnel
    {
        public bool KeepRunning { get; }
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service tunnel has the same id.
        /// </summary>
        public Guid PairId { get; }
        public string Name { get; }

        public void AddEndpoint(NtEndpointInboundConfiguration endpointInbound,
            NtEndpointOutboundConfiguration endpointOutbound, EndpointDirection whichIsLocal);
    }
}
