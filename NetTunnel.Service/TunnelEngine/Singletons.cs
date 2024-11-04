using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Persistence;

namespace NetTunnel.Service.TunnelEngine
{
    internal static class Singletons
    {
        private static ServiceEngine? _serviceEngine = null;
        public static ServiceEngine ServiceEngine
        {
            get
            {
                _serviceEngine ??= new ServiceEngine();
                return _serviceEngine;
            }
        }

        /// <summary>
        /// Logging provider for event log, console (and file?).
        /// </summary>
        private static ILogger? _logger;
        public static ILogger Logger
        {
            get
            {
                _logger ??= new ConsoleLogger(Configuration.LogLevel, Configuration.LogPath);
                return _logger;
            }
        }

        public static void UpdateConfiguration(ServiceConfiguration configuration)
        {
            _configuration = configuration;
            CommonApplicationData.SaveToDisk(Constants.FriendlyName, _configuration);
        }

        private static ServiceConfiguration? _configuration = null;
        public static ServiceConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CommonApplicationData.LoadFromDisk<ServiceConfiguration>(Constants.FriendlyName);

                    if (_configuration == null)
                    {
                        //We didn't find a config file, create one with default values.
                        _configuration = new ServiceConfiguration();
                        CommonApplicationData.SaveToDisk(Constants.FriendlyName, _configuration);
                    }

                    if (_configuration == null)
                    {
                        throw new Exception("Failed to load configuration.");
                    }
                }

                return _configuration;
            }
        }
    }
}
