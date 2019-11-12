using System;
using System.Linq;
using System.Net;
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
        private readonly IHost Host;

        public IOption Options { get; }

        public StickyNetWorker(IOption options, ConfigService configuration, ILogger<StickyNetWorker> logger, IHost host)
        {
            Configuration = configuration;
            Options = options;
            Logger = logger;
            Host = host;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var operation = Options switch
            {
                ReloadOptions reloadOptions => ReloadAsync(reloadOptions),
                CreateOptions createOptions => CreateAsync(createOptions),
                DeleteOptions deleteOptions => DeleteAsync(deleteOptions),
                ListOptions listOptions => ListAsync(listOptions),
                AddTripLinkOptions addOptions => AddTripLinkAsync(addOptions),
                RemoveTripLinkOptions removeOptions => RemoveTripLinkAsync(removeOptions),
                _ => throw new InvalidOperationException("Invalid options type!"),
            };

            _ = Host.StopAsync();
            return operation;
        }

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
                Logger.LogError($"Could not create StickyNet! This port is already occupied by another StickyNet!");
                return;
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
            Logger.LogInformation($"There are {Configuration.ServerConfigs.Count} StickyNets registered in the config files");

            foreach (var config in Configuration.ServerConfigs)
            {
                Logger.LogInformation($" => {config.ToString()}");
            }

            Logger.LogInformation($"There are {Configuration.StickyConfig.TripLinks.Count} TripLink servers registered in the config:");

            foreach(var tripLinkServer in Configuration.StickyConfig.TripLinks)
            {
                Logger.LogInformation($" => TripLink Address: {tripLinkServer.Server}, Token: {tripLinkServer.Token}");
            }

            return Task.CompletedTask;
        }

        private async Task AddTripLinkAsync(AddTripLinkOptions options)
        {
            if(!Uri.TryCreate(options.ReportServer, UriKind.Absolute, out var url) || 
                (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
            {
                Logger.LogError("The report server address is not valid!");
                return;
            }
            if (Configuration.StickyConfig.TripLinks.Any(x => x.Server == url))
            {
                Logger.LogError("There server address is already registered!");
                return;
            }

            await Configuration.AddReportServerAsync(url, options.ReportToken);
        }

        public async Task RemoveTripLinkAsync(RemoveTripLinkOptions options)
        {
            if (!Uri.TryCreate(options.ReportServer, UriKind.Absolute, out var url) ||
                (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
            {
                Logger.LogError("The report server address is not valid!");
                return;
            }
            if (!Configuration.StickyConfig.TripLinks.Any(x => x.Server == url))
            {
                Logger.LogWarning("There is no TripLink registered on this address!");
                return;
            }

            await Configuration.RemoveReportServerAsync(url);
        }
#pragma warning disable
    }
}
