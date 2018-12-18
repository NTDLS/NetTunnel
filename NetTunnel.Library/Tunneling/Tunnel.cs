using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace NetTunnel.Library.Tunneling
{
    public class Tunnel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public BindingProtocal BindingProtocal { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ListenPort { get; set; }
        public int AcceptBacklogSize { get; set; }
        public int InitialBufferSize { get; set; }
        public int MaxBufferSize { get; set; }
        public string Description { get; set; }
        public List<Binding> Bindings { get; set; }
        public bool ListenOnAllAddresses { get; set; }
        public Endpoint Destination { get; set; }

        public Tunnel()
        {
            Id = Guid.NewGuid();
            AcceptBacklogSize = Constants.DefaultAcceptBacklogSize;
            InitialBufferSize = Constants.DefaultInitialBufferSize;
            MaxBufferSize = Constants.DefaultMaxBufferSize;
            Bindings = new List<Binding>();
            Destination = new Endpoint();
        }
    }
}
