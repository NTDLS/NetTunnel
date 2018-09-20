using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NetTunnel.Library;
using NetTunnel.Library.Tunneling;
using NetTunnel.Library.Win32;
using NetTunnel.Service.Tunneling;
using Newtonsoft.Json;

namespace NetTunnel.Service
{
    public class Management
    {
        private Configuration _config;
        private readonly Tunnelers _tunnelers = new Tunnelers();

        public Management()
        {
        }

        public void SaveConfiguration()
        {
            try
            {
                string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.RegsitryKey, "", "ConfigPath");

                string configurationText = JsonConvert.SerializeObject(_config);
                File.WriteAllText(Path.Combine(configPath, Constants.ServerConfigFileName), configurationText);

                string tunnelText = JsonConvert.SerializeObject(_tunnelers.Tunnels());
                File.WriteAllText(Path.Combine(configPath, Constants.TunnelConfigFileName), tunnelText);
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to save configuration.",
                    Exception = ex
                });
            }
        }

        public void LoadConfiguration()
        {
            try
            {
                Console.WriteLine("Loading configuration...");

                //AddTestTunnels();

                string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.RegsitryKey, "", "ConfigPath");

                Console.WriteLine("Server configuration...");
                string configurationText = File.ReadAllText(Path.Combine(configPath, Constants.ServerConfigFileName));
                _config = JsonConvert.DeserializeObject<Configuration>(configurationText);

                Console.WriteLine("Tunnel configuration...");
                string tunnelText = File.ReadAllText(Path.Combine(configPath, Constants.TunnelConfigFileName));
                List<Tunnel> tunnels = JsonConvert.DeserializeObject<List<Tunnel>>(tunnelText);

                if (tunnels != null)
                {
                    foreach (var tunnel in tunnels)
                    {
                        Console.WriteLine("Adding tunnel {0}.", tunnel.Name);
                        _tunnelers.Add(new Tunneler(tunnel));
                    }
                }
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to load configuration.",
                    Exception = ex
                });
            }
        }

        private void AddTestTunnels()
        {
            /*
            Routers routers = new Routers();

            //------------------------------------------------------------------------------------------------------------------
            {
                Route route = new Route()
                {
                    Name = "NetworkDLS",
                    ListenPort = 80,
                    MaxBufferSize = 1021 * 1024,
                    ListenOnAllAddresses = false,
                    AutoStart = true,
                    Description = "Default example route."

                };

                route.Bindings.Add(new Binding { Enabled = true, Address = "127.0.0.1" });

                route.Endpoints.ConnectionPattern = ConnectionPattern.FailOver;
                route.Endpoints.Add(new Endpoint("www.NetworkDLS.com", 80));

                route.HttpHeaderRules.Add(new HttpHeaderRule(HttpHeaderType.Request, HttpVerb.Any, "Host", HttpHeaderAction.Upsert, "www.NetworkDLS.com"));

                routers.Add(new Router(route));
            }
            //------------------------------------------------------------------------------------------------------------------
            {
                Route route = new Route()
                {
                    Name = "Ingenuity",
                    ListenPort = 81,
                    TrafficType = TrafficType.Http,
                    MaxBufferSize = 1021 * 1024,
                    ListenOnAllAddresses = true,
                    AutoStart = true
                };

                //route.Bindings.Add(new Binding { Enabled = true, Address = "127.0.0.1" });

                route.Endpoints.ConnectionPattern = ConnectionPattern.FailOver;
                route.Endpoints.Add(new Endpoint("www.IngenuitySC.com", 80));

                route.HttpHeaderRules.Add(new HttpHeaderRule(HttpHeaderType.Request, HttpVerb.Any, "Host", HttpHeaderAction.Upsert, "www.IngenuitySC.com"));

                routers.Add(new Router(route));
            }
            //------------------------------------------------------------------------------------------------------------------
            {
                Route route = new Route()
                {
                    Name = "Microsoft LIVE!",
                    ListenPort = 82,
                    TrafficType = TrafficType.Https,
                    MaxBufferSize = 1021 * 1024,
                    ListenOnAllAddresses = true,
                    AutoStart = true
                };

                //route.Bindings.Add(new Binding { Enabled = true, Address = "127.0.0.1" });

                route.Endpoints.ConnectionPattern = ConnectionPattern.FailOver;
                route.Endpoints.Add(new Endpoint("login.live.com", 443));

                route.HttpHeaderRules.Add(new HttpHeaderRule(HttpHeaderType.Request, HttpVerb.Any, "Host", HttpHeaderAction.Upsert, "login.live.com"));

                routers.Add(new Router(route));
            }
            //------------------------------------------------------------------------------------------------------------------

            string routesText = JsonConvert.SerializeObject(routers.Routes());

            string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.RegsitryKey, "", "ConfigPath");
            File.WriteAllText(Path.Combine(configPath, Constants.RoutesConfigFileName), routesText);
            */
        }

        public void Start()
        {
            try
            {
                LoadConfiguration();

                Console.WriteLine("starting tunneler...");
                _tunnelers.Start();
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to start tunneler.",
                    Exception = ex
                });
            }
        }

        public void Stop()
        {
            try
            {
                SaveConfiguration();

                _tunnelers.Stop();
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to stop tunneler.",
                    Exception = ex
                });
            }
        }
    }
}
