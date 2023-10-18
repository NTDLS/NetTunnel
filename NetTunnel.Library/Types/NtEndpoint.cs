using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class NtEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public BindDirection Direction { get; set; }
        public int Port { get; set; }

        public NtEndpoint Clone()
        {
            return new NtEndpoint
            {
                Direction = Direction,
                Name = Name,
                Port = Port
            };
        }

    }
}
