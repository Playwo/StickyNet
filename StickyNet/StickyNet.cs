using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Listener.Server;
using StickyNet.Server;
using StickyNet.Service;
using StickyNet.Tcp;

namespace StickyNet.Listener
{
    public class StickyNet : BackgroundService
    {
        private readonly ILogger<StickyNet> Logger;
        private readonly ConfigService Configuration;
        private readonly ILoggerFactory LoggerFactory;

        public List<IStickyServer> Servers { get; }

        public StickyNet(ConfigService configuration, ILogger<StickyNet> logger, ILoggerFactory loggerFactory)
        {
            Logger = logger;
            Configuration = configuration;
            LoggerFactory = loggerFactory;

            Servers = new List<IStickyServer>();
        }

        private async Task StartServerAsync(StickyServerConfig config)
        {
            Logger.LogDebug("Found new StickyNet in config file...");
            var hosts = await Dns.GetHostEntryAsync(Dns.GetHostName());
            var ip = hosts.AddressList.First();

            IStickyServer server = config.Protocol switch
            {
                Protocol.None => new StickyTcpServer(ip, config.Port, new TcpProtocol(), LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]")),
                Protocol.FTP => throw new NotImplementedException(),
                Protocol.SSH => throw new NotImplementedException(),
                Protocol.Telnet => throw new NotImplementedException(),
                _ => null
            };
            Servers.Add(server);
            server.Start();
        }

        private Task StopServerAsync(StickyServerConfig config)
        {
            var server = Servers.Where(x => x.Port == config.Port).First();
            server.Stop();

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Starting StickyNet Launcher...");

            foreach (var config in Configuration.Configs)
            {
                await StartServerAsync(config);
            }

            Configuration.ServerAdded += StartServerAsync;
            Configuration.ServerRemoved += StopServerAsync;

            var exitSource = new TaskCompletionSource<object>();

            stoppingToken.Register(() => exitSource.SetResult(null));

            await exitSource.Task;

            Logger.LogInformation("Stopping StickyNet Launcher...");

            foreach (var server in Servers)
            {
                server.Stop();
            }
        }
    }
}
