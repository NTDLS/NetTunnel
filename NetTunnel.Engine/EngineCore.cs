using NetTunnel.Engine.Managers;
using NetTunnel.Library.Types;

namespace NetTunnel.Engine
{
    public class EngineCore
    {
        public Logger Log { get; set; }
        public UserSessionManager Sessions { get; set; }
        public EndpointManager Endpoints { get; set; }
        public UserManager Users { get; set; }

        public EngineCore(NtEndpointServiceConfiguration config)
        {
            Log = new(this);
            Sessions = new(this);
            Endpoints = new(this);
            Users = new(this);

            //Add debugging users:
            Users.Add("admin", "abcdefg");
            Users.Add("root", "123456789");

            //Add debugging endpoints:
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
