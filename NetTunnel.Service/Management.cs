﻿using System;
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

                AddTestTunnels();

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
            string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.RegsitryKey, "", "ConfigPath");

            var configuration = new Configuration();
            File.WriteAllText(Path.Combine(configPath, Constants.ServerConfigFileName), JsonConvert.SerializeObject(configuration));

            var tunnels = new List<Tunnel>();

            var tunnel = new Tunnel()
            {
                AcceptBacklogSize = 2,
                AutoStart = true,
                BindingProtocal = BindingProtocal.Pv4,
                Description = "Test tunnel #1",
                Id = Guid.NewGuid(),
                InitialBufferSize = 1024,
                MaxBufferSize = 1024 * 1024 * 10,
                ListenPort = 80,
                Name = "TestTunnel#1",
                ListenOnAllAddresses = true,
                Endpoint = new Endpoint("ntdls.com", 80)
            };
            tunnels.Add(tunnel);

            File.WriteAllText(Path.Combine(configPath, Constants.TunnelConfigFileName), JsonConvert.SerializeObject(tunnels));
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
