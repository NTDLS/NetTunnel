using NetTunnel.Library.Types;

namespace NetTunnel.Service.TunnelEngine
{
    public static class Singletons
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

        private static NtServiceApplicationConfiguration? _settings = null;
        public static NtServiceApplicationConfiguration Configuration
        {
            get
            {
                if (_settings == null)
                {
                    IConfiguration config = new ConfigurationBuilder()
                                 .AddJsonFile("appsettings.json")
                                 .AddEnvironmentVariables()
                                 .Build();

                    _settings = config.GetRequiredSection("Settings").Get<NtServiceApplicationConfiguration>();
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
