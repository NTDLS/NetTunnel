using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryUpsertUserEndpoint : IRmQuery<UIQueryUpsertUserEndpointReply>
    {
        public string Username { get; set; }
        public EndpointConfiguration Configuration { get; set; }

        public UIQueryUpsertUserEndpoint(string username, EndpointConfiguration configuration)
        {
            Username = username;
            Configuration = configuration;
        }
    }

    public class UIQueryUpsertUserEndpointReply : IRmQueryReply
    {
        public UIQueryUpsertUserEndpointReply()
        {
        }
    }
}
