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

            var service = new ConfigService();
            await service.InitializeAsync();

            var cfg = new StickyServerConfig(options.Port, options.Protocol, options.OutputPath);

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
