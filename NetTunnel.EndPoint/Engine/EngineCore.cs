using NetTunnel.ClientAPI;
using NetTunnel.EndPoint.Engine.Managers;
using NetTunnel.Library.Types;

namespace NetTunnel.EndPoint.Engine
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

#if DEBUG
            //Add debugging users:
            {
                Users.Add("admin", Utility.CalculateSHA256("abcdefg"));
                Users.Add("root", Utility.CalculateSHA256("123456789"));
            }

            //Add debugging endpoints:
            {
                Endpoints.Collection.Use((o) =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        o.Add(new NtEndpoint()
                        {
                            Direction = Library.Constants.BindDirection.Incomming,
                            Name = $"Test endpoint {i}",
                            Port = 8080 + 1
                        });
                    }
                });
            }
#endif
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
