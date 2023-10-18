using NetTunnel.Engine.Managers;
using NetTunnel.Library.Config;

namespace NetTunnel.Engine
{
    public class EngineCore
    {
        public Logger Log { get; set; }
        public UserSessionManager Sessions { get; set; }
        public EndpointManager Endpoints { get; set; }

        public EngineCore(EndpointServiceConfiguration config)
        {
            Log = new(this);
            Sessions = new(this);
            Endpoints = new(this);

            Endpoints.Collection.Use((o) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    o.Add(new Library.Types.NtEndpoint()
                    {
                        Direction = Library.Constants.BindDirection.Incomming,
                        Name = $"Test endpoint {i}",
                        Port = 8080 + 1
                    });
                }

            });

        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

    }
}
