using NetTunnel.Library.ReliableMessages.Query;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server query communication (whether they be UI or other services with inbound tunnels)
    ///     must pass queries though these handlers.
    /// </summary>
    internal class ServiceQueryHandlers : ServiceHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service has made an outgoing tunnel connection and has started the process of exchanging a key.
        /// Here we need to apply the diffie–hellman negation token and reply with the diffie–hellman reply token
        /// We will then "Initialize the Cryptography Provider" but will not use apply (use it) until the remote service
        /// sends the NotificationApplyCryptography notification. This is because if we apply the cryptography now, then
        /// the reply will be encrypted before the remote service has the data it needs to decrypt it.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyKeyExchangeReply OnQueryRequestKeyExchange(RmContext context, QueryRequestKeyExchange query)
        {
            var connectionContext = GetServiceConnectionContext(context);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
            var negotiationReply = new QueryReplyKeyExchangeReply(context.ConnectionId, negotiationReplyToken);

            connectionContext.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

            return negotiationReply;
        }

        public QueryLoginReply OnQueryLogin(RmContext context, QueryLogin query)
        {
            var connectionContext = EnforceCryptographyAndGetServiceConnectionContext(context);

            if (Singletons.ServiceEngine.Users.ValidatePassword(query.UserName, query.PasswordHash))
            {
                connectionContext.SetAuthenticated(query.UserName);

                return new QueryLoginReply(true)
                {
                    ServiceId = Singletons.Configuration.ServiceId
                };
            }

            return new QueryLoginReply(false);
        }

        public QueryGetTunnelsReply OnGetTunnels(RmContext context, QueryGetTunnels query)
        {
            var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            return new QueryGetTunnelsReply
            {
                Collection = Singletons.ServiceEngine.Tunnels.Clone(),
            };
        }

        public QueryCreateTunnelReply OnQueryCreateTunnel(RmContext context, QueryCreateTunnel query)
        {
            var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            Singletons.ServiceEngine.Tunnels.UpsertTunnel(query.Configuration);

            return new QueryCreateTunnelReply();
        }

        public QueryPingReply OnQueryCreateTunnel(RmContext context, QueryPing query)
        {
            var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            return new QueryPingReply(query.OriginationTimestamp);
        }

        public QueryUpsertEndpointReply OnQueryUpsertEndpoint(QueryUpsertEndpoint query)
        {
            Singletons.ServiceEngine.Tunnels.UpsertEndpoint(query.TunnelId, query.Configuration);

            //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
            //var result = await Singletons.ServiceEngine.InboundTunnels
            //    .DispatchUpsertEndpointOutboundToAssociatedTunnelService<oldQueryReplyPayloadBoolean>(tunnelId, endpoint.Outbound);

            return new QueryUpsertEndpointReply();
        }

        public QueryRegisterTunnelReply OnRegisterTunnel(RmContext context, QueryRegisterTunnel query)
        {
            var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            Singletons.ServiceEngine.Tunnels.RegisterTunnel(context.ConnectionId, query.Configuration);

            return new QueryRegisterTunnelReply();
        }
    }
}
