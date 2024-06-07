using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseServiceConfiguration : NtActionResponse
    {
        public NtServiceConfiguration Configuration { get; set; } = new();

        public NtActionResponseServiceConfiguration() { }

        public NtActionResponseServiceConfiguration(NtServiceConfiguration configuration)
        {
            Configuration = configuration;
        }

        public NtActionResponseServiceConfiguration(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
