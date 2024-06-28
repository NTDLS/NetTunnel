using NetTunnel.Library.ReliablePayloads.Query.UIOrService;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableHandlers.Service.Queries
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server query communication (whether they be UI or other services with inbound tunnels)
    ///     must pass queries though these handlers.
    /// </summary>
    internal class ServiceQueryHandlersForServiceToServiceOrUI : ServiceHandlerBase, IRmMessageHandler
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
        public UOSQueryReplyKeyExchangeReply OnQuery(RmContext context, UOSQueryRequestKeyExchange query)
        {
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

                var compoundNegotiator = new CompoundNegotiator();
                var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
                var negotiationReply = new UOSQueryReplyKeyExchangeReply(context.ConnectionId, negotiationReplyToken);

                connectionContext.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

                return negotiationReply;
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// A UI client or remote service is requesting to login.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UOSQueryLoginReply OnQuery(RmContext context, UOSQueryLogin query)
        {
            try
            {
               
                var connectionContext = EnforceCryptographyAndGetServiceConnectionContext(context);

                var userRole = Singletons.ServiceEngine.Users.ValidateLoginAndGetRole(query.UserName, query.PasswordHash);
                if (userRole != NtUserRole.Undefined)
                {
                    connectionContext.SetAuthenticated(query.UserName, userRole, query.LoginType);

                    return new UOSQueryLoginReply(true)
                    {
                        UserRole = userRole,
                        ServiceId = Singletons.Configuration.ServiceId
                    };
                }

                return new UOSQueryLoginReply(false);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }
    }
}
