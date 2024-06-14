using NTDLS.NASCCL;

namespace NetTunnel.Service
{
    public class ServiceConnectionContext
    {
        public Guid ConnectionId { get; set; }
        public NASCCLStream? StreamCryptography { get; private set; }
        public bool SecureKeyExchangeIsComplete { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string UserName { get; set; } = string.Empty;

        public ServiceConnectionContext(Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public void InitializeCryptographyProvider(byte[] sharedSecret)
        {
            StreamCryptography = new NASCCLStream(sharedSecret);
        }

        public void ApplyCryptographyProvider()
        {
            if (StreamCryptography == null)
            {
                throw new Exception("The stream cryptography has not been initialized.");
            }
            SecureKeyExchangeIsComplete = true;
        }

        public void SetAuthenticated(string userName)
        {
            UserName = userName.ToLower();
            IsAuthenticated = true;
        }
    }
}
