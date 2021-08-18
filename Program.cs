using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Movington.PhotoTransfer.GooglePhotos;
using Movington.PhotoTransfer.OneDrive;
using Movington.PhotoTransfer.Pipeline;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Movington.PhotoTransfer
{
    public static class Program
    {
        private const string AuthenticationSectionName = "Authentication";

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting host...");
                CreateHostBuilder(args).Build().Run();

                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(LogEventLevel.Warning)
                    .WriteTo.File(
                        new CompactJsonFormatter(),
                        $"{AppConstants.FilesFolderPath}/Logs/{DateTime.UtcNow:yyyy_MM_dd___HH_mm_ss}.log",
                        buffered: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(3),
                        rollingInterval: RollingInterval.Infinite, 
                        fileSizeLimitBytes: null
                    ))
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddOptions<OneDriveAuthenticationOptions>()
                        .BindConfiguration($"{AuthenticationSectionName}:OneDrive")
                        .ValidateDataAnnotations();

                    services
                        .AddSingleton<OneDriveAuthenticationHandler>()
                        .AddSingleton<OneDriveClient>()
                        .AddSingleton<IInitializable>(container => container.GetRequiredService<OneDriveClient>())
                        .AddSingleton<OneDrivePhotosService>();

                    services
                        .AddOptions<GoogleAuthenticationOptions>()
                        .BindConfiguration($"{AuthenticationSectionName}:GooglePhotos")
                        .ValidateDataAnnotations();

                    services
                        .AddSingleton<GoogleAuthenticationHandler>()
                        .AddSingleton<GooglePhotosClient>()
                        .AddSingleton<IInitializable>(container => container.GetRequiredService<GooglePhotosClient>());

                    services
                        .AddSingleton<TransferPipeline>()
                        .AddSingleton<PipelineStepsFactory>()
                        .AddOptions<TransferPipelineOptions>()
                        .BindConfiguration("TransferPipeline");

                    services.AddHostedService<Worker>();
                });
    }
}
