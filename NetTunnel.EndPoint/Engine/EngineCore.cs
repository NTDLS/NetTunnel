using NetTunnel.EndPoint.Engine.Managers;
using NetTunnel.Library.Types;

namespace NetTunnel.EndPoint.Engine
{
    public class EngineCore
    {
        public Logger Log { get; set; }
        public UserSessionManager Sessions { get; set; }
        public OutgoingEndpointManager OutgoingEndpoints { get; set; }
        public IncommingEndpointManager IncommingEndpoints { get; set; }
        public UserManager Users { get; set; }

        public EngineCore(NtEndpointServiceConfiguration config)
        {
            Log = new(this);
            Sessions = new(this);
            OutgoingEndpoints = new(this);
            IncommingEndpoints = new(this);
            Users = new(this);

            /*
            //Add debugging users:
            Users.Add("admin", Utility.CalculateSHA256("abcdefg"));
            Users.Add("root", Utility.CalculateSHA256("123456789"));
            Users.SaveToDisk();
            */

            /*
            //Add debugging endpoints:
            for (int i = 0; i < 10; i++)
            {
                Endpoints.Add(new NtEndpoint()
                {
                    Direction = Library.Constants.BindDirection.Incomming,
                    Name = $"Test endpoint {i}",
                    Port = 8080 + 1
                });
            }
            Endpoints.SaveToDisk();
            */
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
