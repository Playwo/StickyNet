using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StickyNet.Report;
using StickyNet.Server;
using StickyNet.Server.Tcp;
using StickyNet.Server.Tcp.Protocols;
using StickyNet.Service;

namespace StickyNet
{
    public class StickyNetWorker : BackgroundService
    {
        private readonly ILogger<StickyNetWorker> Logger;
        private readonly ConfigService Configuration;
        private readonly ILoggerFactory LoggerFactory;
        private readonly HttpClient HttpClient;

        private readonly System.Timers.Timer ReporterTimer;

        public List<IStickyServer> Servers { get; }

        public StickyNetWorker(ConfigService configuration, ILogger<StickyNetWorker> logger, ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            Logger = logger;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            HttpClient = httpClient;

            Servers = new List<IStickyServer>();
            ReporterTimer = new System.Timers.Timer(600000); //10 Minutes
            ReporterTimer.Elapsed += ReporterTimer_Elapsed;
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

        private async Task StartServerAsync(StickyServerConfig config)
        {
            Logger.LogDebug("Found new StickyNet in config file...");
            var hosts = await Dns.GetHostEntryAsync(Dns.GetHostName());
            var ip = hosts.AddressList.First();

            IStickyServer server = config.Protocol switch
            {
                Protocol.None => new StickyTcpServer(ip, new EmptyTcpProtocol(), config, LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]")),
                Protocol.FTP => new StickyTcpServer(ip, new FtpProtocol(), config, LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]")),
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

        private void ReporterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => _ = Task.Run(() => ReportAdressesAsync());
        private async Task ReportAdressesAsync()
        {
            foreach(var server in Servers)
            {
                if(!server.Config.EnableReporting)
                {
                    continue;
                }

                var ipReports = new List<IpReport>();

                foreach(var attempt in server.ConnectionAttempts)
                {
                    var reason = attempt.Value.CalculateReason();
                    ipReports.Add(new IpReport(attempt.Key.ToString(), reason));
                }

                server.ConnectionAttempts.Clear();

                var parameters = new Dictionary<string, object>()
                {
                    ["token"] = server.Config.ReportToken,
                    ["ips"] = new ReportPacket(server.Config.ReportToken, ipReports.ToArray())
                };

                var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
                var response = await HttpClient.PostAsync(server.Config.ReportServer, content);

                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError($"{response.StatusCode} : {response.ReasonPhrase}");
                }
            }
        }
    }
}
