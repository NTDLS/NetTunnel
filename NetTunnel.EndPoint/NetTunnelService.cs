using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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

        public static X509Certificate2 CreateSelfSignedCertificate()
        {
            using (RSA rsa = RSA.Create(Singletons.Configuration.RSAKeyLength))
            {
                var request = new CertificateRequest($"CN=NetTunnel.EndPoint.private", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

                return new X509Certificate2(certificate.Export(X509ContentType.Pkcs12), "");
            }
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

            builder.Services.AddHttpContextAccessor();
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            var certificate = CreateSelfSignedCertificate();

            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(Singletons.Configuration.ManagementPort, listenOptions =>
                {
                    listenOptions.UseHttps(certificate);
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                Console.WriteLine($"https://localhost:{Singletons.Configuration.ManagementPort}/swagger/index.html");
            }

            app.UseAuthorization();
            app.MapControllers();
            app.RunAsync($"https://localhost:{Singletons.Configuration.ManagementPort}/");

            if (app.Environment.IsDevelopment())
            {
                //System.Diagnostics.Process.Start("explorer", $"{Configuration.BaseAddress}swagger/index.html");
            }

            Singletons.Core.Log.Write($"Listening on {Singletons.Configuration.ManagementPort}.");

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
