using NetTunnel.Library.ReliablePayloads.Notification;
using NetTunnel.Library.ReliablePayloads.Query;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Service.ReliableHandlers
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
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

                var compoundNegotiator = new CompoundNegotiator();
                var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
                var negotiationReply = new QueryReplyKeyExchangeReply(context.ConnectionId, negotiationReplyToken);

                connectionContext.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

                return negotiationReply;
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryLoginReply OnQueryLogin(RmContext context, QueryLogin query)
        {
            try
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
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryGetTunnelsReply OnQueryGetTunnels(RmContext context, QueryGetTunnels query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                return new QueryGetTunnelsReply
                {
                    Collection = Singletons.ServiceEngine.Tunnels.GetForDisplay(),
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryCreateTunnelReply OnQueryCreateTunnel(RmContext context, QueryCreateTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.UpsertTunnel(query.Configuration);

                return new QueryCreateTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryPingReply OnQueryPing(RmContext context, QueryPing query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                if (query.PreviousPing != null)
                {
                    Singletons.ServiceEngine.Tunnels.UpdateLastPing(query.TunnelKey, (double)query.PreviousPing);
                }

                return new QueryPingReply(query.OriginationTimestamp);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryDistributeUpsertEndpointReply OnQueryUpsertEndpoint(RmContext context, QueryDistributeUpsertEndpoint query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.DistributeUpsertEndpoint(query.TunnelKey, query.Configuration);

                return new QueryDistributeUpsertEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryUpsertEndpointReply OnQueryUpsertEndpoint(RmContext context, QueryUpsertEndpoint query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.UpsertEndpoint(query.TunnelKey, query.Configuration);

                return new QueryUpsertEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryRegisterTunnelReply OnQueryRegisterTunnel(RmContext context, QueryRegisterTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                var tunnelKey = Singletons.ServiceEngine.Tunnels.RegisterTunnel(context.ConnectionId, query.Configuration);
                connectionContext.AssociateTunnel(tunnelKey);

                return new QueryRegisterTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryGetTunnelStatisticsReply OnQueryGetTunnelStatistics(RmContext context, QueryGetTunnelStatistics query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                return new QueryGetTunnelStatisticsReply()
                {
                    Statistics = Singletons.ServiceEngine.Tunnels.GetStatistics()
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryDeleteTunnelReply OnQueryDeleteTunnel(RmContext context, QueryDeleteTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                //We want to stop and delete the tunnel locally.
                Singletons.ServiceEngine.Tunnels.DeleteTunnel(query.TunnelKey);

                return new QueryDeleteTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryGetUsersReply OnQueryGetUsers(RmContext context, QueryGetUsers query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                return new QueryGetUsersReply()
                {
                    Collection = Singletons.ServiceEngine.Users.Clone()
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryDeleteUserReply OnQueryDeleteUser(RmContext context, QueryDeleteUser query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Users.Delete(query.UserName);

                return new QueryDeleteUserReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryChangeUserPasswordReply OnQueryChangeUserPassword(RmContext context, QueryChangeUserPassword query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Users.ChangePassword(query.Username, query.PasswordHash);

                return new QueryChangeUserPasswordReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryCreateUserReply OnQueryCreateUser(RmContext context, QueryCreateUser query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Users.Add(query.User);

                return new QueryCreateUserReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryGetServiceConfigurationReply OnQueryGetServiceConfiguration(RmContext context, QueryGetServiceConfiguration query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                return new QueryGetServiceConfigurationReply()
                {
                    Configuration = Singletons.Configuration
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryPutServiceConfigurationReply OnQueryPutServiceConfiguration(RmContext context, QueryPutServiceConfiguration query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.UpdateConfiguration(query.Configuration);

                return new QueryPutServiceConfigurationReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryDeleteEndpointReply OnQueryDeleteEndpoint(RmContext context, QueryDeleteEndpoint query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.DeleteEndpoint(query.TunnelKey, query.EndpointId);

                return new QueryDeleteEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryStartTunnelReply OnQueryStartTunnel(RmContext context, QueryStartTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.Start(query.TunnelKey);

                return new QueryStartTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        public QueryStopTunnelReply OnQueryStopTunnel(RmContext context, QueryStopTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.Stop(query.TunnelKey);

                return new QueryStopTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }
    }
}
