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
            var inboundTunnelContext = GetServiceConnectionContext(context);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
            var negotiationReply = new QueryReplyKeyExchangeReply(context.ConnectionId, negotiationReplyToken);

            inboundTunnelContext.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

            return negotiationReply;
        }

        public QueryLoginReply OnQueryLogin(RmContext context, QueryLogin query)
        {
            var inboundTunnelContext = EnforceCryptographyAndGetServiceConnectionContext(context);

            if (Singletons.Core.Sessions.Login(context.ConnectionId,
                query.UserName, query.PasswordHash, $"{context.TcpClient.Client.RemoteEndPoint}"))
            {
                inboundTunnelContext.SetAuthenticated(query.UserName);

                return new QueryLoginReply(true);
            }

            return new QueryLoginReply(false);
        }

        public GetInboundTunnelsReply OnGetInboundTunnels(RmContext context, QueryGetInboundTunnels query)
        {
            var inboundTunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            return new GetInboundTunnelsReply
            {
                Collection = Singletons.Core.InboundTunnels.CloneConfigurations(),
            };
        }

        public GetOutboundTunnelsReply OnGetOutboundTunnels(RmContext context, QueryGetOutboundTunnels query)
        {
            var inboundTunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            return new GetOutboundTunnelsReply
            {
                Collection = Singletons.Core.OutboundTunnels.CloneConfigurations(),
            };
        }

        public QueryCreateInboundTunnelReply OnQueryCreateInboundTunnel(RmContext context, QueryCreateInboundTunnel query)
        {
            var inboundTunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            Singletons.Core.InboundTunnels.Add(query.Configuration);
            Singletons.Core.InboundTunnels.SaveToDisk();

            return new QueryCreateInboundTunnelReply();
        }

        public QueryCreateOutboundTunnelReply OnQueryCreateOutboundTunnel(RmContext context, QueryCreateOutboundTunnel query)
        {
            var inboundTunnelContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

            Singletons.Core.OutboundTunnels.Add(query.Configuration);
            Singletons.Core.OutboundTunnels.SaveToDisk();

            return new QueryCreateOutboundTunnelReply();
        }
    }
}