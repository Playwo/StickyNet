using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Server;
using StickyNet.Service;
using StickyNet.Arguments;
using System.Net.Http;
using System.Net;

namespace StickyNet
{
    public class Startup
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly ILogger<Startup> Logger;

        public Startup()
        {
            LoggerFactory = MakeLoggerFactory();
            Logger = LoggerFactory.CreateLogger<Startup>();
        }

        private ILoggerFactory MakeLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.ClearProviders();
                builder.AddConsole();
            });

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

            var result = parser.ParseArguments<RunOptions, CreateOptions, DeleteOptions>(args)
                .WithParsed<RunOptions>(opt => RunStickyNetAsync(opt).GetAwaiter().GetResult())
                .WithParsed<CreateOptions>(opt => CreateStickyNetAsync(opt).GetAwaiter().GetResult())
                .WithParsed<DeleteOptions>(opt => DeleteStickyNetAsync(opt).GetAwaiter().GetResult());
        }

        private async Task DeleteStickyNetAsync(DeleteOptions options)
        {
            Logger.LogInformation($"Deleting StickyNet from port {options.Port}...");

            var service = new ConfigService();
            await service.InitializeAsync();

            var result = await service.RemoveStickyNetAsync(options.Port);

            if (!result.Item1)
            {
                Logger.LogError(result.Item2);
            }
            else
            {
                Logger.LogInformation($"Successfully removed StickyNet from port {options.Port}");
            }
        }

        private async Task CreateStickyNetAsync(CreateOptions options)
        {
            Logger.LogInformation($"Creating StickyNet on port {options.Port} imitating {options.Protocol}...");
           
            if ((options.ReportToken != null && options.ReportServer == null) || (options.ReportToken == null && options.ReportServer != null))
            {
                Logger.LogError("You need to provide a reportserver and a reporttoken, not just one of them!");
                return;
            }

            Uri url = null;

            if (options.ReportServer != null && options.ReportToken != null)
            {
                if (!Uri.TryCreate(string.Format("http://{0}", options.ReportServer), UriKind.Absolute, out url) || !IPAddress.TryParse(url.Host, out _))
                {
                    Logger.LogError("The Report Server could not be parsed! Please use the following format : ipv4:port");
                    return;
                }
            }


            var service = new ConfigService();
            await service.InitializeAsync();

            var cfg = new StickyServerConfig(options.Port, options.Protocol, options.OutputPath, url, options.ReportToken);

            var result = await service.AddStickyNetAsync(cfg);

            if (!result.Item1)
            {
                Logger.LogError(result.Item2);
            }
            else
            {
                Logger.LogInformation($"Successfully added a StickyNet to port {cfg.Port} imitating {cfg.Protocol} and logging to {cfg.OutputPath}");
            }
        }

        private async Task RunStickyNetAsync(RunOptions options)
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(async (hostContext, services) =>
                {
                    services.AddSingleton<HttpClient>();
                    services.AddHostedService<StickyNetWorker>();
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
