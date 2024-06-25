using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.ServiceToService
{
    public class S2SQueryRegisterTunnel : IRmQuery<S2SQueryRegisterTunnelReply>
    {
        public TunnelConfiguration Configuration { get; set; } = new();

        public S2SQueryRegisterTunnel()
        {
        }

        public S2SQueryRegisterTunnel(TunnelConfiguration configuration)
        {
            Configuration = configuration;
        }
    }

    public class S2SQueryRegisterTunnelReply : IRmQueryReply
    {

        public S2SQueryRegisterTunnelReply()
        {
        }
    }
}
