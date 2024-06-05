using NetTunnel.Library;
using NetTunnel.Service.Extensions;
using NetTunnel.Service.TunnelEngine;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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

        public static X509Certificate2? CreateSelfSignedCertificate()
        {
            try
            {
                using RSA rsa = RSA.Create(Singletons.Configuration.RSAKeyLength);
                var request = new CertificateRequest($"CN=NetTunnel.EndPoint.private", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-10), DateTimeOffset.Now.AddYears(3));

                return new X509Certificate2(certificate.Export(X509ContentType.Pkcs12), "");
            }
            catch (Exception ex)
            {
                Singletons.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"Could not instantiate SSL: {ex.Message}.");
            }

            return null;
        }

        private void DoWork()
        {
            Thread.CurrentThread.Name = $"DoWork:{Environment.CurrentManagedThreadId}";

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
                    if (certificate != null)
                    {
                        listenOptions.UseHttps(certificate);
                    }
                });
            });

            var app = builder.Build();

            /*
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                Console.WriteLine($"https://localhost:{Singletons.Configuration.ManagementPort}/swagger/index.html");
                Singletons.Core.Logging.Write(Constants.NtLogSeverity.Verbose,$"Listening on {Singletons.Configuration.ManagementPort}.");
            }
            */

            app.UseAuthorization();
            app.MapControllers();
            app.RunAsync($"https://localhost:{Singletons.Configuration.ManagementPort}/");

            if (app.Environment.IsDevelopment())
            {
                //System.Diagnostics.Process.Start("explorer", $"{Configuration.BaseAddress}swagger/index.html");
            }

            while (true)
            {
                if (_semaphoreToRequestStop.Wait(500))
                {
                    Singletons.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopping...");
                    app.StopAsync();
                    Singletons.Core.Stop();
                    break;
                }
            }
        }
    }
}
