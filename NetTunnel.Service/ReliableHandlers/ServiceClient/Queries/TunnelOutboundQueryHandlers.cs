using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Library.ReliablePayloads.Query.UI;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers.ServiceClient.Queries
{
    /// <summary>
    /// Each outbound tunnel makes its own connection using an RmClient. These are the handlers for each outbound tunnel.
    /// </summary>
    internal class TunnelOutboundQueryHandlers : TunnelOutboundHandlersBase, IRmMessageHandler
    {
        public UIQueryDistributeUpsertEndpointReply OnQuery(RmContext context, UIQueryDistributeUpsertEndpoint query)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.UpsertEndpointAndDistribute(query.TunnelKey, query.Configuration, null);

                return new UIQueryDistributeUpsertEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public S2SQueryUpsertEndpointReply OnQuery(RmContext context, S2SQueryUpsertEndpoint query)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.UpsertEndpoint(query.TunnelKey, query.Configuration, null);

                return new S2SQueryUpsertEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }


    }
}
