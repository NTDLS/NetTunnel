namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfiguration : INtEndpointConfiguration
    {
        public Guid EndpointId { get; set; }

        public Guid TunnelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int TransmissionPort { get; set; }

        public NtEndpointOutboundConfiguration() { }

        public NtEndpointOutboundConfiguration(Guid tunnelId, Guid endpointId, string name, string address, int transmissionPort)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Name = name;
            Address = address;
            TransmissionPort = transmissionPort;
        }
    }
}
