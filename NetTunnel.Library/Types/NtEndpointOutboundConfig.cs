namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfig
    {
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!

        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<NtEndpointOutboundConfig> Connectors { get; set; } = new();
        public List<NtEndpointInboundConfig> Listeners { get; set; } = new();

        public NtEndpointOutboundConfig(string name, string address, int managementPort, int dataPort, string username, string passwordHash)
        {
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            DataPort = dataPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtEndpointOutboundConfig Clone()
        {
            return new NtEndpointOutboundConfig(Name, Address, ManagementPort, DataPort, Username, PasswordHash)
            {
                Id = Id
            };
        }
    }
}
