using NetTunnel.ClientAPI.Exceptions;
using NetTunnel.ClientAPI.Management;

namespace NetTunnel.ClientAPI
{
    public class Client : IDisposable
    {
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
        public ConfigurationClient Configuration { get; private set; }

        private HttpClient? _connection = null;

        /// <summary>
        /// Connects to the server using a URL.
        /// </summary>
        /// <param name="baseAddress">Base address should be in the form http://host:port/</param>
        public Client(string baseAddress)
        {
            BaseAddress = baseAddress;

            Configuration = new ConfigurationClient(this);
        }

        /// <summary>
        /// Connects to the server using a URL and a non-default timeout.
        /// </summary>
        /// <param name="baseAddress">Base address should be in the form http://host:port/</param>
        public Client(string baseAddress, TimeSpan timeout)
        {
            BaseAddress = baseAddress;
            Timeout = timeout;

            Configuration = new ConfigurationClient(this);

            Connect();
        }

        void Connect()
        {
            if (IsConnected)
            {
                throw new GenericException("The client is already connected.");
            }

            try
            {
                SessionId = Guid.NewGuid();
                _connection = new HttpClient
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
                        Configuration.Logout();
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
