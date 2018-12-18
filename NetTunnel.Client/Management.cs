using Microsoft.Win32;
using NetTunnel.Library;
using NetTunnel.Library.Win32;
using Newtonsoft.Json;
using System;
using System.IO;

namespace NetTunnel.Client
{
    public class Management
    {
        private Configuration _config;

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
        }

        public void Start()
        {
            try
            {
                LoadConfiguration();

                Console.WriteLine("starting client...");
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to start client.",
                    Exception = ex
                });
            }
        }

        public void Stop()
        {
            try
            {
                SaveConfiguration();
            }
            catch (Exception ex)
            {
                Singletons.EventLog.WriteEvent(new EventLogging.EventPayload
                {
                    Severity = EventLogging.Severity.Error,
                    CustomText = "Failed to stop client.",
                    Exception = ex
                });
            }
        }
    }
}
