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

        public static void UpdateConfiguration(ServiceConfiguration configuration)
        {
            _configuration = configuration;
            CommonApplicationData.SaveToDisk(Library.Constants.FriendlyName, _configuration);
        }

        private static ServiceConfiguration? _configuration = null;
        public static ServiceConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CommonApplicationData.LoadFromDisk<ServiceConfiguration>(Library.Constants.FriendlyName);

                    if (_configuration == null)
                    {
                        //We didn't find a config file, create one with default values.
                        _configuration = new ServiceConfiguration();
                        CommonApplicationData.SaveToDisk(Library.Constants.FriendlyName, _configuration);
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
