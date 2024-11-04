using NetTunnel.Library.ReliablePayloads.Query.UI;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using System.Reflection;
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
        public UIQueryGetTunnelsReply UIQueryGetTunnels(RmContext context, UIQueryGetTunnels query)
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
        public UIQueryCreateTunnelReply UIQueryCreateTunnel(RmContext context, UIQueryCreateTunnel query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryDistributeUpsertEndpointReply UIQueryDistributeUpsertEndpoint(RmContext context, UIQueryDistributeUpsertEndpoint query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.UpsertEndpointAndDistribute(query.TunnelKey, query.Configuration, connectionContext.UserName);

                return new UIQueryDistributeUpsertEndpointReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// A user is asking the service to upsert an endpoint associated with a user.
        /// </summary>
        public UIQueryUpsertUserEndpointReply UIQueryUpsertUserEndpoint(RmContext context, UIQueryUpsertUserEndpoint query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Users.UpsertEndpoint(query.Username, query.Configuration);

                return new UIQueryUpsertUserEndpointReply();
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
        public UIQueryGetTunnelStatisticsReply UIQueryGetTunnelStatistics(RmContext context, UIQueryGetTunnelStatistics query)
        {
            try
            {
                Singletons.Logger.Debug(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        /// The UI is requesting that a tunnel be disconnected, but not deleted from the associated remote service.
        /// </summary>
        public UIQueryDisconnectTunnelReply UIQueryDisconnectTunnel(RmContext context, UIQueryDisconnectTunnel query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

                //We want to stop and delete the tunnel locally.
                Singletons.ServiceEngine.Tunnels.DisconnectAndRemoveTunnel(query.TunnelKey);

                return new UIQueryDisconnectTunnelReply();
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
        public UIQueryDeleteTunnelReply UIQueryDeleteTunnel(RmContext context, UIQueryDeleteTunnel query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                //We want to stop and delete the tunnel locally.
                Singletons.ServiceEngine.Tunnels.DeleteBothEndsOfTunnel(query.TunnelKey);

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
        public UIQueryGetUsersReply UIQueryGetUsers(RmContext context, UIQueryGetUsers query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryDeleteUserReply UIQueryDeleteUser(RmContext context, UIQueryDeleteUser query)
        {
            try
            {
                Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryEditUserReply UIQueryEditUser(RmContext context, UIQueryEditUser query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryCreateUserReply UIQueryCreateUser(RmContext context, UIQueryCreateUser query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryGetServiceConfigurationReply UIQueryGetServiceConfiguration(RmContext context, UIQueryGetServiceConfiguration query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryPutServiceConfigurationReply UIQueryPutServiceConfiguration(RmContext context, UIQueryPutServiceConfiguration query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryDeleteEndpointReply UIQueryDeleteEndpoint(RmContext context, UIQueryDeleteEndpoint query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
                if (connectionContext.UserRole != NtUserRole.Administrator)
                {
                    throw new Exception("Unauthorized");
                }

                Singletons.ServiceEngine.Tunnels.DeleteEndpointAndDistribute(query.TunnelKey, query.EndpointId, connectionContext.UserName);

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
        public UIQueryStartTunnelReply UIQueryStartTunnel(RmContext context, UIQueryStartTunnel query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
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
        public UIQueryStopTunnelReply UIQueryStopTunnel(RmContext context, UIQueryStopTunnel query)
        {
            Singletons.Logger.Verbose(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);
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
        public UIQueryGetTunnelPropertiesReply UIQueryGetTunnelProperties(RmContext context, UIQueryGetTunnelProperties query)
        {
            Singletons.Logger.Debug(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

            try
            {
                var connectionContext = EnforceLoginCryptographyAndGetServiceConnectionContext(context);

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
        public UIQueryGetEndpointPropertiesReply UIQueryGetEndpointProperties(RmContext context, UIQueryGetEndpointProperties query)
        {
            Singletons.Logger.Debug(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
        public UIQueryGetEndpointEdgeConnectionsReply UIQueryGetEndpointEdgeConnections(RmContext context, UIQueryGetEndpointEdgeConnections query)
        {
            Singletons.Logger.Debug(NTDLS.Helpers.Text.SeperateCamelCase(MethodBase.GetCurrentMethod()?.Name ?? string.Empty));

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
