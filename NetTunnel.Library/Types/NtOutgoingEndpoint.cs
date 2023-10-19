namespace NetTunnel.Library.Types
{
    public class NtOutgoingEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Port { get; set; }

        public NtOutgoingEndpoint Clone()
        {
            return new NtOutgoingEndpoint
            {
                Name = Name,
                Address = Address,
                Port = Port
            };
        }
    }
}
