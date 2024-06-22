using NetTunnel.Library.Payloads;
using Newtonsoft.Json.Converters;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

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
        public TunnelStatisticsProperties Properties { get; set; } = new();

        public QueryGetTunnelPropertiesReply()
        {
        }
    }
}

