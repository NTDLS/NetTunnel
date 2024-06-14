using NetTunnel.Service.ReliableMessages;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library
{
    internal class OutboundTunnelConnection
    {
        private CryptographyProvider? _cryptographyProvider;

        public RmClient Client { get; private set; }
        public Guid ConnectionId { get; set; }

        public OutboundTunnelConnection(RmClient client, Guid connectionId)
        {
            Client = client;
            ConnectionId = connectionId;
        }

        public void InitializeCryptographyProvider(byte[] sharedSecret)
        {
            _cryptographyProvider = new CryptographyProvider(sharedSecret);
        }

        public void ApplyCryptographyProvider()
        {
            Client.SetCryptographyProvider(_cryptographyProvider);
        }
    }
}
