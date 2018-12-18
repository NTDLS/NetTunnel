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
        public Management()
        {
        }

        public void SaveConfiguration()
        {
            try
            {
                string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.ClientRegsitryKey, "", "ConfigPath");

                string configurationText = JsonConvert.SerializeObject(Singletons.Config);
                File.WriteAllText(Path.Combine(configPath, Constants.ServiceConfigFileName), configurationText);
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

                AddTestData();

                string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.ClientRegsitryKey, "", "ConfigPath");

                Console.WriteLine("Client configuration...");
                string configurationText = File.ReadAllText(Path.Combine(configPath, Constants.ServiceConfigFileName));
                Singletons.Config = JsonConvert.DeserializeObject<Configuration>(configurationText);
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

        private void AddTestData()
        {
            string configPath = RegistryHelper.GetString(Registry.LocalMachine, Constants.ClientRegsitryKey, "", "ConfigPath");

            var configuration = new Configuration()
            {
                Username = "admin",
                Password = "admin"
            };
            File.WriteAllText(Path.Combine(configPath, Constants.ServiceConfigFileName), JsonConvert.SerializeObject(configuration));
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
