using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryGetTunnelProperties : IRmQuery<QueryGetTunnelPropertiesReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public QueryGetTunnelProperties(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }

        public QueryGetTunnelProperties()
        {
            TunnelKey = new();
        }
    }

    public class QueryGetTunnelPropertiesReply : IRmQueryReply
    {
        public TunnelProperties Properties { get; set; } = new();

        public QueryGetTunnelPropertiesReply()
        {
        }
    }
}

