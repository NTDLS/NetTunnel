using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableHandlers.Service
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server query communication (whether they be UI or other services with inbound tunnels)
    ///     must pass queries though these handlers.
    /// </summary>
    internal class ServiceQueryHandlersForServiceToService : ServiceHandlerBase, IRmMessageHandler
    {
        public S2SQueryPingReply OnQuery(RmContext context, S2SQueryPing query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                if (query.PreviousPing != null)
                {
                    Singletons.ServiceEngine.Tunnels.UpdateLastPing(query.TunnelKey, (double)query.PreviousPing);
                }

                return new S2SQueryPingReply(query.OriginationTimestamp);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public S2SQueryRegisterTunnelReply OnQuery(RmContext context, S2SQueryRegisterTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    //throw new Exception("Unauthorized");
                }

                var tunnelKey = Singletons.ServiceEngine.Tunnels.RegisterTunnel(context.ConnectionId, query.Configuration);
                connectionContext.AssociateTunnel(tunnelKey);

                return new S2SQueryRegisterTunnelReply();
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
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.UpsertEndpoint(query.TunnelKey, query.Configuration);

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
