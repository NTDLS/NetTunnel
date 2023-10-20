using NetTunnel.Library.Types;

namespace NetTunnel.Service.Engine
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
