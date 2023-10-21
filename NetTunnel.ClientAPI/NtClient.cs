using NetTunnel.ClientAPI.Exceptions;

namespace NetTunnel.ClientAPI
{
    public class NtClient : IDisposable
    {
        public bool AutoAcceptSSLCertificate { get; set; } = true;
        public bool IsConnected => _connection != null;
        public string BaseAddress { get; private set; }
        public TimeSpan Timeout { get; private set; } = new TimeSpan(0, 8, 0, 0, 0);
        public HttpClient Connection
        {
            get
            {
                Utility.EnsureNotNull(_connection);
                return _connection;
            }
        }

        public Guid SessionId { get; internal set; } = Guid.Empty;
        public NtTunnelInboundClient TunnelInbound { get; private set; }
        public NtTunnelOutboundClient TunnelOutbound { get; private set; }
        public NtEndpointInboundClient EndpointInbound { get; private set; }
        public NtEndpointOutboundClient EndpointOutbound { get; private set; }

        public NtSecurityClient Security { get; private set; }

        private HttpClient? _connection = null;

        /// <summary>
        /// Connects to the server using a URL.
        /// </summary>
        /// <param name="baseAddress">Base address should be in the form http://host:port/</param>
        public NtClient(string baseAddress)
        {
            BaseAddress = baseAddress;

            TunnelInbound = new(this);
            TunnelOutbound = new(this);
            EndpointInbound = new(this);
            EndpointOutbound = new(this);
            Security = new(this);

            Connect();
        }

        public NtClient(string baseAddress, string username, string passwordHash)
        {
            BaseAddress = baseAddress;

            TunnelInbound = new(this);
            TunnelOutbound = new(this);
            EndpointInbound = new(this);
            EndpointOutbound = new(this);
            Security = new(this);

            Connect();

            Security.Login(username, passwordHash);
        }

        /// <summary>
        /// Connects to the server using a URL and a non-default timeout.
        /// </summary>
        /// <param name="baseAddress">Base address should be in the form http://host:port/</param>
        public NtClient(string baseAddress, TimeSpan timeout)
        {
            BaseAddress = baseAddress;
            Timeout = timeout;

            TunnelInbound = new(this);
            TunnelOutbound = new(this);
            EndpointInbound = new(this);
            EndpointOutbound = new(this);
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
                    BaseAddress = new Uri(BaseAddress),
                    Timeout = Timeout
                };
            }
            catch
            {
                if (_connection != null)
                {
                    try
                    {
                        _connection.Dispose();
                    }
                    catch { }
                }

                SessionId = Guid.Empty;
                _connection = null;
                throw;
            }
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
                    try
                    {
                        Disconnect();
                    }
                    catch { }
                }
            }

            disposed = true;
        }

        #endregion
    }
}
