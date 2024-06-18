using NetTunnel.Library;
using NetTunnel.Service.TunnelEngine;

namespace NetTunnel.Service
{
    internal class SystemService
    {
        private readonly SemaphoreSlim _semaphoreToRequestStop;
        private readonly Thread _thread;

        public SystemService()
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

            Singletons.ServiceEngine.Start();

            while (true)
            {
                if (_semaphoreToRequestStop.Wait(500))
                {
                    Singletons.ServiceEngine.Logger.Verbose("Stopping...");
                    Singletons.ServiceEngine.Stop();
                    break;
                }
            }
        }
    }
}
