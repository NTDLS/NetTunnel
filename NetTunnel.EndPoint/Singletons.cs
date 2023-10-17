using NetTunnel.Engine;
using NetTunnel.Library.Config;

namespace NetTunnel.EndPoint
{
    public static class Singletons
    {
        private static EngineCore? _core = null;
        public static EngineCore Core
        {
            get
            {
                _core ??= new EngineCore(Configuration);
                return _core;
            }
        }

        private static EndpointServiceConfiguration? _settings = null;
        public static EndpointServiceConfiguration Configuration
        {
            get
            {
                if (_settings == null)
                {
                    IConfiguration config = new ConfigurationBuilder()
                                 .AddJsonFile("appsettings.json")
                                 .AddEnvironmentVariables()
                                 .Build();

                    _settings = config.GetRequiredSection("Settings").Get<EndpointServiceConfiguration>();
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
