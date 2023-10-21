namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfiguration
    {
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!

        public Guid PairId { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<NtEndpointOutboundConfiguration> Connectors { get; set; } = new();
        public List<NtEndpointInboundConfiguration> Listeners { get; set; } = new();

        public NtEndpointOutboundConfiguration(string name, string address, int managementPort, int dataPort, string username, string passwordHash)
        {
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            DataPort = dataPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtEndpointOutboundConfiguration Clone()
        {
            return new NtEndpointOutboundConfiguration(Name, Address, ManagementPort, DataPort, Username, PasswordHash)
            {
                PairId = PairId
            };
        }
    }
}
