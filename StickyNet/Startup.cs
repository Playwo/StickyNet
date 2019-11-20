using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Options;
using StickyNet.Service;
using StickyNet.Workers;

namespace StickyNet
{
    public class Startup
    {
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

            var result = parser.ParseArguments<RunOptions, CreateOptions, DeleteOptions, ListOptions, AddTripLinkOptions, RemoveTripLinkOptions, ReloadOptions>(args)
                .WithParsed<RunOptions>(opt => StartStickyNetRunnerAsync(opt).GetAwaiter().GetResult())
                .WithParsed<IOption>(opt => StartStickyNetWorkerAsync(opt).GetAwaiter().GetResult());
        }

        private async Task StartStickyNetWorkerAsync(IOption option)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ConsoleLifetimeOptions>(lifeTimeOptions => lifeTimeOptions.SuppressStatusMessages = true);
                    services.AddSingleton(option);
                    services.AddSingleton<ConfigService>();
                    services.AddHostedService<StickyNetWorker>();

                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(option.LogLevel);
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .Build();

            await host.RunAsync();
        }

        private async Task StartStickyNetRunnerAsync(RunOptions options)
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<HttpClient>();
                    services.AddSingleton<ConfigService>();
                    services.AddHostedService<StickyNetRunner>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();

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
