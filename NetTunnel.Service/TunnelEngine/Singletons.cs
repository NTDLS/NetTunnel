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

        private static NtServiceConfiguration? _settings = null;
        public static NtServiceConfiguration Configuration
        {
            get
            {
                if (_settings == null)
                {
                    _settings = CommonApplicationData.LoadFromDisk<NtServiceConfiguration>(Library.Constants.FriendlyName);

                    if (_settings == null)
                    {
                        //We didn't find a config file, create one with default values.
                        _settings = new NtServiceConfiguration();
                        CommonApplicationData.SaveToDisk(Library.Constants.FriendlyName, _settings);
                    }

                    if (_settings == null)
                    {
                        throw new Exception("Failed to load configuration.");
                    }
                }

                return _settings;
            }
        }
    }
}
