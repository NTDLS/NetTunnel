namespace NetTunnel.EndPoint
{
    internal class NetTunnelService
    {
        private SemaphoreSlim _semaphoreToRequestStop;
        private Thread _thread;

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
            Singletons.Core.Start();

            // Add services to the container.
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers(options =>
            {
                options.InputFormatters.Add(new TextPlainInputFormatter());
            });

            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.RunAsync(Singletons.Configuration.BaseAddress);

            if (app.Environment.IsDevelopment())
            {
                //System.Diagnostics.Process.Start("explorer", $"{Configuration.BaseAddress}swagger/index.html");
            }

            Singletons.Core.Log.Write($"Listening on {Singletons.Configuration.BaseAddress}.");

            while (true)
            {
                if (_semaphoreToRequestStop.Wait(500))
                {
                    Singletons.Core.Log.Write($"Stopping...");
                    app.StopAsync();
                    Singletons.Core.Stop();
                    break;
                }
            }
        }
    }
}
