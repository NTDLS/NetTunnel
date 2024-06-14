using NetTunnel.Library;
using NetTunnel.Service.TunnelEngine;

namespace NetTunnel.Service
{
    internal class NetTunnelService
    {
        private readonly SemaphoreSlim _semaphoreToRequestStop;
        private readonly Thread _thread;

        public NetTunnelService()
        {
            _semaphoreToRequestStop = new SemaphoreSlim(0);
            _thread = new Thread(DoWork);

        }

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            _semaphoreToRequestStop.Release();
            _thread.Join();
        }

        private void DoWork()
        {
            Thread.CurrentThread.Name = $"DoWork:{Environment.CurrentManagedThreadId}";

            Singletons.Core.Start();

            while (true)
            {
                if (_semaphoreToRequestStop.Wait(500))
                {
                    Singletons.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopping...");
                    Singletons.Core.Stop();
                    break;
                }
            }
        }
    }
}
