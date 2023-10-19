using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class NtEndpoint
    {
        public EndpointDirection Direction { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Address { get; set; }
        public int Port { get; set; }

        public NtEndpoint Clone()
        {
            return new NtEndpoint
            {
                Direction = Direction,
                Name = Name,
                Address = Address,
                Port = Port
            };
        }
    }
}
