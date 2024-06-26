using NetTunnel.Library.Interfaces;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using System.Net;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableHandlers.Service.Queries
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server query communication (whether they be UI or other services with inbound tunnels)
    ///     must pass queries though these handlers.
    /// </summary>
    internal class ServiceQueryHandlersForServiceToService : ServiceHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service is requesting that this service respond to a ping request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
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

        /// <summary>
        /// The remote outbound service is asking this service to register the given tunnel.
        /// The service will connect the tunnel and send back the endpoints for the logged in user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public S2SQueryRegisterTunnelReply OnQuery(RmContext context, S2SQueryRegisterTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    //throw new Exception("Unauthorized");
                }

                if (context.TcpClient.Client.RemoteEndPoint is IPEndPoint address)
                {
                    //The address is really on used by outbound tunnels to make an outgoing connection.
                    //Since that ↑ is the case, we are going to relace it with the address of the connecting service
                    //  so that it shows in the UI.
                    query.Configuration.Address = address.Address.ToString(); //Only used for UI for inbound tunnels.
                    query.Configuration.ServicePort = address.Port; //Only used for UI for inbound tunnels.
                }

                var endpoints = Singletons.ServiceEngine.Users.GetEndpoints(connectionContext.UserName);

                var tunnelKey = Singletons.ServiceEngine.Tunnels.RegisterTunnel(context.ConnectionId, query.Configuration, endpoints);
                connectionContext.AssociateTunnel(tunnelKey);

                return new S2SQueryRegisterTunnelReply()
                {
                    Endpoints = endpoints
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// A new endpoint has been added to the remote service, so that remote service is asking this service to add the endpoint locally too.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
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
