using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Options;
using StickyNet.Workers;

namespace StickyNet
{
    public class Startup
    {
        private readonly ILogger<Startup> Logger;

        public Startup()
        {
            Logger = MakeLogger<Startup>();
        }

        private ILogger<T> MakeLogger<T>()
            => LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.ClearProviders();
                builder.AddConsole();
                builder.AddDebug();
            })
            .CreateLogger<T>();

        public void Run(string[] args)
        {
            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = true;
                x.CaseInsensitiveEnumValues = false;
                x.CaseSensitive = false;
                x.EnableDashDash = true;
                x.HelpWriter = Console.Out;
                x.IgnoreUnknownArguments = false;
            });

            var result = parser.ParseArguments<RunOptions, CreateOptions, DeleteOptions, ListOptions>(args)
                .WithParsed<RunOptions>(opt => StartStickyNetRunnerAsync(opt).GetAwaiter().GetResult())
                .WithParsed<IOption>(opt => StartStickyNetWorkerAsync(opt).GetAwaiter().GetResult());

            Thread.Sleep(100); //Wait for all logs to appear
        }

        private async Task StartStickyNetWorkerAsync(IOption option)
        {
            Logger.LogInformation("Starting StickyNet Worker...");

            var hostBuilder = Host.CreateDefaultBuilder()
                           .ConfigureServices(async (hostContext, services) =>
                           {
#pragma warning disable IDE0001
                               services.AddSingleton<IOption>(option);
#pragma warning restore
                               services.AddHostedService<StickyNetWorker>();
                               services.AddStickyServices();
                               await services.InitializeStickyServicesAsync();
                           })
                           .ConfigureLogging(logging =>
                           {
                               logging.SetMinimumLevel(option.LogLevel);
                               logging.AddConsole();
                               logging.AddDebug();
                           })
                           .UseConsoleLifetime();

            await hostBuilder.RunConsoleAsync();
        }

        private async Task StartStickyNetRunnerAsync(RunOptions options)
        {
            Logger.LogInformation("Starting StickyNet Runner...");

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(async (hostContext, services) =>
                {
                    services.AddSingleton<HttpClient>();
                    services.AddHostedService<StickyNetRunner>();
                    services.AddStickyServices();
                    await services.InitializeStickyServicesAsync();
                })
                .ConfigureLogging(logging =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        logging.AddEventLog();
                    }

                    logging.SetMinimumLevel(options.LogLevel);
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseConsoleLifetime();

            var host = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? hostBuilder.UseSystemd().Build()
                : hostBuilder.UseWindowsService().Build();

            await host.RunAsync();
        }
    }
}
