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

        public Channel<ConnectionAttempt> ConnectionAttempts { get; }
        public StickyGlobalConfig Config => Configuration.StickyConfig;

        public StickyNetRunner(ConfigService configuration, ILogger<StickyNetRunner> logger, ILoggerFactory loggerFactory, ReportService reporter)
        {
            Logger = logger;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            Reporter = reporter;

            ConnectionAttempts = Channel.CreateUnbounded<ConnectionAttempt>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

            Servers = new ConcurrentDictionary<int, IStickyServer>();
        }

        private Task StartServerAsync(StickyServerConfig config)
        {
            Logger.LogDebug("Found new StickyNet in config file...");

            var ip = IPAddress.Any;
            var logger = LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]");
            var writer = ConnectionAttempts.Writer;

            var server = config.Protocol switch
            {
                Protocol.None => (IStickyServer) new StickyTcpServer<NoneSession>(ip, config, writer, logger),
                Protocol.FTP => new StickyTcpServer<FtpSession>(ip, config, writer, logger),
                Protocol.SSH => new StickyTcpServer<SshSession>(ip, config, writer, logger),
                Protocol.Telnet => new StickyTcpServer<TelnetSession>(ip, config, writer, logger),
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting StickyNet Launcher...");

            foreach (var config in Configuration.ServerConfigs)
            {
                await StartServerAsync(config);
            }

            Configuration.ServerAdded += StartServerAsync;
            Configuration.ServerRemoved += StopServerAsync;

            _ = Task.Run(() => Reporter.StartReporterAsync(ConnectionAttempts.Reader));
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
    }
}
