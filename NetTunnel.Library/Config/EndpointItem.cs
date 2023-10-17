using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Config
{
    public class EndpointItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public BindDirection Direction { get; set; }
        public int Port { get; set; }
    }
}
