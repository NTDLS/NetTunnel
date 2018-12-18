using NetTunnel.Library.Tunneling;
using System.Collections.Generic;

namespace NetTunnel.Service
{
    public class Configuration
    {
        public List<Client> Clients { get; set; }

        public Configuration()
        {
            Clients = new List<Client>();
        }
    }
}
