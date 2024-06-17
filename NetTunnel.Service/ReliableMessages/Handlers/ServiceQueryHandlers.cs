﻿using NetTunnel.Library.ReliableMessages.Query;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Service.ReliableMessages.Handlers
{
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
            var tunnelContext = GetServiceConnectionContext(context);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
            var negotiationReply = new QueryReplyKeyExchangeReply(context.ConnectionId, negotiationReplyToken);

            tunnelContext.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

            return negotiationReply;
        }

        public QueryLoginReply OnQueryLogin(RmContext context, QueryLogin query)
        {
            var tunnelContext = EnforceCryptographyAndGetServiceConnectionContext(context);

            if (Singletons.Core.Sessions.Login(context.ConnectionId,
                query.UserName, query.PasswordHash, $"{context.TcpClient.Client.RemoteEndPoint}"))
            {
                tunnelContext.SetAuthenticated(query.UserName);

                return new QueryLoginReply(true);
            }

            return new QueryLoginReply(false);
        }

        public QueryGetTunnelsReply OnGetTunnels(RmContext context, QueryGetTunnels query)
        {
            var tunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            return new QueryGetTunnelsReply
            {
                Collection = Singletons.Core.Tunnels.CloneConfigurations(),
            };
        }

        public QueryCreateTunnelReply OnQueryCreateTunnel(RmContext context, QueryCreateTunnel query)
        {
            var tunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            Singletons.Core.Tunnels.Add(query.Configuration);

            return new QueryCreateTunnelReply();
        }

        public QueryPingReply OnQueryCreateTunnel(RmContext context, QueryPing query)
        {
            var tunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            return new QueryPingReply(query.OriginationTimestamp);
        }

        public QueryUpsertEndpointReply OnQueryUpsertEndpoint(QueryUpsertEndpoint query)
        {
            Singletons.Core.Tunnels.UpsertEndpoint(query.Configuration);

            //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
            //var result = await Singletons.Core.InboundTunnels
            //    .DispatchUpsertEndpointOutboundToAssociatedTunnelService<oldQueryReplyPayloadBoolean>(tunnelId, endpoint.Outbound);

            return new QueryUpsertEndpointReply();
        }
    }
}
