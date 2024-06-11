namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration : INtEndpointConfiguration
    {
        public Guid EndpointId { get; set; }

        public Guid TunnelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int TransmissionPort { get; set; }

        public NtEndpointInboundConfiguration() { }

        public NtEndpointInboundConfiguration(Guid tunnelId, Guid endpointId, string name, int transmissionPort)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Name = name;
            TransmissionPort = transmissionPort;
        }
    }
}
