namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The tunnel listener contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfig
    {
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!
        //THIS IS A COPY OF THE CONFIG FROM THE TUNNELL JUST TO GET BUILDS WORKING, THIS IS NOT CORRECT!

        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int DataPort { get; set; }
        public List<NtEndpointOutboundConfig> Connectors { get; set; } = new();
        public List<NtEndpointInboundConfig> Listeners { get; set; } = new();

        public NtEndpointInboundConfig(string name, int dataPort)
        {
            Name = name;
            DataPort = dataPort;
        }

        public NtEndpointInboundConfig Clone()
        {
            return new NtEndpointInboundConfig(Name, DataPort)
            {
                Id = Id
            };
        }

    }
}
