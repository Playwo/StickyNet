using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Options;
using StickyNet.Service;

namespace StickyNet.Workers
{
    public class StickyNetWorker : BackgroundService
    {
        private readonly ConfigService Configuration;
        private readonly ILogger<StickyNetWorker> Logger;

        public IOption Options { get; }

        public StickyNetWorker(IOption options, ConfigService configuration, ILogger<StickyNetWorker> logger)
        {
            Configuration = configuration;
            Options = options;
            Logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Options switch
            {
                ReloadOptions reloadOptions => ReloadAsync(reloadOptions),
                CreateOptions createOptions => CreateAsync(createOptions),
                DeleteOptions deleteOptions => DeleteAsync(deleteOptions),
                ListOptions listOptions => ListAsync(listOptions),
                _ => throw new InvalidOperationException("Invalid options type!"),
            };

#pragma warning disable IDE0060
        private async Task ReloadAsync(ReloadOptions options)
        {
            await Configuration.MarkChangesAsync();
            Logger.LogInformation("Success, Stickynet will refresh in the next cycle if it is running at the moment! [max 1s]");
        }

        private async Task CreateAsync(CreateOptions options)
        {
            var existingConfig = Configuration.ServerConfigs.Where(x => x.Port == options.Port).FirstOrDefault();

            if (existingConfig != null)
            {
                Logger.LogError($"Could not create StickyNet! This port is already occupied by this StickyNet:\n{existingConfig}");
            }

            var config = new StickyServerConfig(options.Port, options.Protocol, options.OutputPath, options.ConnectionTimeout);
            Logger.LogInformation($"Creating {config}");

            await Configuration.AddServerConfigAsync(config);
        }

        private async Task DeleteAsync(DeleteOptions options)
        {
            var config = Configuration.ServerConfigs.Where(x => x.Port == options.Port).FirstOrDefault();

            if (config == null)
            {
                Logger.LogWarning($"There is no StickyNet on Port {options.Port} registered in the config files!");
                return;
            }

            await Configuration.DeleteServerConfigAsync(config);
        }

        private Task ListAsync(ListOptions options)
        {
            Logger.LogInformation($"There are {Configuration.ServerConfigs.Count} StickyNets registered in the config files:");

            foreach (var config in Configuration.ServerConfigs)
            {
                Logger.LogInformation(config.ToString());
            }

            return Task.CompletedTask;
        }
#pragma warning disable
    }
}
