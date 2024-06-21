namespace NetTunnel.UI
{
    internal class UILoginPreferences
    {
#if DEBUG
        public string Username { get; set; } = "debug";
#else
        public string Username { get; set; } = "root";

#endif
        public string Address { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "52845";
    }
}
