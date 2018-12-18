namespace NetTunnel.Library.Tunneling
{
    public class Client
    {
        public Endpoint Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Client()
        {
            Address = new Endpoint();
        }
    }
}
