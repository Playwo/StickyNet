using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Server;
using StickyNet.Server.Tcp;
using StickyNet.Service;

namespace StickyNet
{
    public class StickyNetRunner : IHostedService
    {
        private readonly ILogger<StickyNetRunner> Logger;
        private readonly ConfigService Configuration;
        private readonly ILoggerFactory LoggerFactory;
        private readonly ReportService Reporter;

        public ConcurrentDictionary<int, IStickyServer> Servers { get; }
        
        public StickyGlobalConfig Config => Configuration.StickyConfig;

        public StickyNetRunner(ConfigService configuration, ILogger<StickyNetRunner> logger, ILoggerFactory loggerFactory, ReportService reporter)
        {
            Logger = logger;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            Reporter = reporter;
            Servers = new ConcurrentDictionary<int, IStickyServer>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting StickyNet Launcher...");

            foreach (var config in Configuration.ServerConfigs)
            {
                await StartServerAsync(config);
            }

            Configuration.ServerAdded += StartServerAsync;
            Configuration.ServerRemoved += StopServerAsync;

            Reporter.StartReporter();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping StickyNet Launcher...");

            foreach (var server in Servers.Select(x => x.Value))
            {
                await StopServerAsync(server.Config);
            }

            Reporter.StopReporter();
        }

        private Task StartServerAsync(StickyServerConfig config)
        {
            Logger.LogDebug("Found new StickyNet in config file...");

            var ip = IPAddress.Any;
            var logger = LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]");

            var server = config.Protocol switch
            {
                Protocol.None => (IStickyServer) new StickyTcpServer<NoneSession>(ip, config, Reporter, logger),
                Protocol.FTP => new StickyTcpServer<FtpSession>(ip, config, Reporter, logger),
                Protocol.SSH => new StickyTcpServer<SshSession>(ip, config, Reporter, logger),
                Protocol.Telnet => new StickyTcpServer<TelnetSession>(ip, config, Reporter, logger),
                _ => null
            };

            Servers.TryAdd(server.Port, server);
            server.Start();

            return Task.CompletedTask;
        }

        private Task StopServerAsync(StickyServerConfig config)
        {
            if (!Servers.TryGetValue(config.Port, out var server))
            {
                return Task.CompletedTask;
            }

            server.Stop();
            server.Dispose();

            Servers.TryRemove(server.Port, out _);

            return Task.CompletedTask;
        }
    }
}
