using NetTunnel.Library.ReliablePayloads.Query.UI;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableHandlers.Service.Queries
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server query communication (whether they be UI or other services with inbound tunnels)
    ///     must pass queries though these handlers.
    /// </summary>
    internal class ServiceQueryHandlersForUI : ServiceHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// A UI client is requesting a list of tunnels.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetTunnelsReply OnQuery(RmContext context, UIQueryGetTunnels query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                return new UIQueryGetTunnelsReply
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

        /// <summary>
        /// A user is asking the service to create a tunnel with the given configuration.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryCreateTunnelReply OnQuery(RmContext context, UIQueryCreateTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.UpsertTunnel(query.Configuration);

                return new UIQueryCreateTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// A user is asking that a new endpoint be added to the local tunnel and also sent through the connected tunnel
        ///     to the other service on the other end so that it can also add associated endpoint to its tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryDistributeUpsertEndpointReply OnQuery(RmContext context, UIQueryDistributeUpsertEndpoint query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.UpsertEndpointAndDistribute(query.TunnelKey, query.Configuration);

                return new UIQueryDistributeUpsertEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting general tunnel statistics.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetTunnelStatisticsReply OnQuery(RmContext context, UIQueryGetTunnelStatistics query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                return new UIQueryGetTunnelStatisticsReply()
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

        /// <summary>
        /// The UI is requesting that a tunnel be deleted.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryDeleteTunnelReply OnQuery(RmContext context, UIQueryDeleteTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                //We want to stop and delete the tunnel locally.
                Singletons.ServiceEngine.Tunnels.DeleteTunnel(query.TunnelKey);

                return new UIQueryDeleteTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting a list of all users.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetUsersReply OnQuery(RmContext context, UIQueryGetUsers query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                return new UIQueryGetUsersReply()
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

        /// <summary>
        /// The UI is requesting to delete a given user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryDeleteUserReply OnQuery(RmContext context, UIQueryDeleteUser query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Users.Delete(query.UserName);

                return new UIQueryDeleteUserReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting that a user be altered with the given information (password, role, etc.)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryEditUserReply OnQuery(RmContext context, UIQueryEditUser query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Users.EditUser(query.User);

                return new UIQueryEditUserReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting that a user be created with the given information.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryCreateUserReply OnQuery(RmContext context, UIQueryCreateUser query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Users.Add(query.User);

                return new UIQueryCreateUserReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting the local service configuration.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetServiceConfigurationReply OnQuery(RmContext context, UIQueryGetServiceConfiguration query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                return new UIQueryGetServiceConfigurationReply()
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

        /// <summary>
        /// The UI is requesting that the local service configuration be changed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryPutServiceConfigurationReply OnQuery(RmContext context, UIQueryPutServiceConfiguration query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.UpdateConfiguration(query.Configuration);

                return new UIQueryPutServiceConfigurationReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting that an endpoint be deleted and that we let the
        ///     remote service connected to the tunnel know so it can do the same.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryDeleteEndpointReply OnQuery(RmContext context, UIQueryDeleteEndpoint query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.DeleteEndpointAndDistribute(query.TunnelKey, query.EndpointId);

                return new UIQueryDeleteEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting that a tunnel be started.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryStartTunnelReply OnQuery(RmContext context, UIQueryStartTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.Start(query.TunnelKey);

                return new UIQueryStartTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting that a tunnel be stopped.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryStopTunnelReply OnQuery(RmContext context, UIQueryStopTunnel query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.Stop(query.TunnelKey);

                return new UIQueryStopTunnelReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting general tunnel properties.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetTunnelPropertiesReply OnQuery(RmContext context, UIQueryGetTunnelProperties query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                return new UIQueryGetTunnelPropertiesReply()
                {
                    Properties = Singletons.ServiceEngine.Tunnels.GetTunnelProperties(query.TunnelKey)
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting general endpoint properties.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetEndpointPropertiesReply OnQuery(RmContext context, UIQueryGetEndpointProperties query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                return new UIQueryGetEndpointPropertiesReply()
                {
                    Properties = Singletons.ServiceEngine.Tunnels.GetEndpointProperties(query.TunnelKey, query.EndpointKey)
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting a list of edge connections to a given endpoint.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public UIQueryGetEndpointEdgeConnectionsReply OnQuery(RmContext context, UIQueryGetEndpointEdgeConnections query)
        {
            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                return new UIQueryGetEndpointEdgeConnectionsReply()
                {
                    Collection = Singletons.ServiceEngine.Tunnels.GetEndpointEdgeConnections(query.TunnelKey, query.EndpointKey)
                };
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }
    }
}
