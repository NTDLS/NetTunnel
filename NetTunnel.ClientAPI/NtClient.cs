using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library;
using NetTunnel.Library.Exceptions;
using Newtonsoft.Json;

namespace NetTunnel.ClientAPI
{
    public class NtClient : IDisposable
    {
        public bool AutoAcceptSSLCertificate { get; set; } = true;
        public bool IsConnected => _connection != null;
        public string Address { get; private set; }
        public Uri? BaseAddress { get => Connection.BaseAddress; }

        public TimeSpan Timeout { get; private set; } = new TimeSpan(0, 8, 0, 0, 0);
        public HttpClient Connection
        {
            get
            {
                return _connection.EnsureNotNull();
            }
        }

        public Guid SessionId { get; internal set; } = Guid.Empty;
        public NtTunnelInboundClient TunnelInbound { get; private set; }
        public NtTunnelOutboundClient TunnelOutbound { get; private set; }

        public NtSecurityClient Security { get; private set; }

        private HttpClient? _connection = null;

        /// <summary>
        /// Connects to the server using a URL.
        /// </summary>
        /// <param name="address">Base address should be in the form http://host:port/</param>
        public NtClient(string address)
        {
            Address = address;

            TunnelInbound = new(this);
            TunnelOutbound = new(this);
            Security = new(this);

            Connect();
        }

        public NtClient(string address, string username, string passwordHash)
        {
            Address = address;

            TunnelInbound = new(this);
            TunnelOutbound = new(this);
            Security = new(this);

            Connect();

            Security.Login(username, passwordHash);
        }

        /// <summary>
        /// Connects to the server using a URL and a non-default timeout.
        /// </summary>
        /// <param name="address">Base address should be in the form http://host:port/</param>
        public NtClient(string address, TimeSpan timeout)
        {
            Address = address;
            Timeout = timeout;

            TunnelInbound = new(this);
            TunnelOutbound = new(this);
            Security = new(this);

            Connect();
        }

        private void Connect()
        {
            if (IsConnected)
            {
                throw new NtGenericException("The client is already connected.");
            }

            try
            {
                SessionId = Guid.NewGuid();

                var handler = new HttpClientHandler();

                if (AutoAcceptSSLCertificate)
                {
                    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                }

                _connection = new HttpClient(handler)
                {
                    BaseAddress = new Uri(Address),
                    Timeout = Timeout
                };
            }
            catch
            {
                if (_connection != null)
                {
                    Utility.TryAndIgnore(_connection.Dispose);
                }

                SessionId = Guid.Empty;
                _connection = null;
                throw;
            }
        }

        public async Task<NtActionResponseStatistics> GetStatistics()
        {
            string url = $"api/Service/{SessionId}/GetStatistics";

            using var response = await Connection.GetAsync(url);
            string resultText = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NtActionResponseStatistics>(resultText);
            if (result == null || result.Success == false)
            {
                throw new NtAPIResponseException(result == null ? "Invalid response" : result.ExceptionText);
            }

            return result;
        }

        void Disconnect()
        {
            try
            {
                try
                {
                    if (IsConnected && SessionId != Guid.Empty)
                    {
                        Security.Logout();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                SessionId = Guid.Empty;
                _connection = null;
            }
        }

        #region IDisposable.

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (IsConnected)
                {
                    Utility.TryAndIgnore(Disconnect);
                }
            }

            disposed = true;
        }

        #endregion
    }
}
