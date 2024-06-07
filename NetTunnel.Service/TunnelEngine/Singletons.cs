using NetTunnel.Library.Types;
using NTDLS.Persistence;

namespace NetTunnel.Service.TunnelEngine
{
    internal static class Singletons
    {
        private static TunnelEngineCore? _core = null;
        public static TunnelEngineCore Core
        {
            get
            {
                _core ??= new TunnelEngineCore();
                return _core;
            }
        }

        public static void UpdateConfiguration(NtServiceConfiguration configuration)
        {
            _configuration = configuration;
            CommonApplicationData.SaveToDisk(Library.Constants.FriendlyName, _configuration);
        }

        private static NtServiceConfiguration? _configuration = null;
        public static NtServiceConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CommonApplicationData.LoadFromDisk<NtServiceConfiguration>(Library.Constants.FriendlyName);

                    if (_configuration == null)
                    {
                        //We didn't find a config file, create one with default values.
                        _configuration = new NtServiceConfiguration();
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
