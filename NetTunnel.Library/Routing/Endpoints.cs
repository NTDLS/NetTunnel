using System.Collections.Generic;

namespace NetTunnel.Library.Routing
{
    public class Endpoints
    {
        public List<Endpoint> List = new List<Endpoint>();

        public void Add(Endpoint peer)
        {
            List.Add(peer);
        }
    }
}
