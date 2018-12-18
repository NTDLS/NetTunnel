namespace NetTunnel.Library.Tunneling
{
    public class Endpoint
    {
        public string Address { get; set; }
        public int Port { get; set; }

        public Endpoint(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public Endpoint()
        {
        }
    }
}
