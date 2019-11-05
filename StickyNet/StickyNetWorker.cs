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

        public System.Timers.Timer ReporterTimer { get; }
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
            ReporterTimer.Start();
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
                await StopServerAsync(server.Config);
            }
        }

        private Task StartServerAsync(StickyServerConfig config)
        {
            Logger.LogDebug("Found new StickyNet in config file...");

            var ip = IPAddress.Any;
            var logger = LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]");

            IStickyServer server = config.Protocol switch
            {
                Protocol.None => new StickyTcpServer(ip, new EmptyTcpProtocol(), config, logger),
                Protocol.FTP => new StickyTcpServer(ip, new FtpProtocol(), config, logger),
                Protocol.SSH => new StickyTcpServer(ip, new SSHProtocol(), config, logger),
                Protocol.Telnet => new StickyTcpServer(ip, new TelnetProtocol(), config, logger),
                _ => null
            };

            Servers.Add(server);
            server.Start();

            return Task.CompletedTask;
        }

        private Task StopServerAsync(StickyServerConfig config)
        {
            var server = Servers.Where(x => x.Port == config.Port).First();
            server.Stop();
            server.Dispose();

            Servers.RemoveAll(x => x.Port == config.Port);

            return Task.CompletedTask;
        }

        private void ReporterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => _ = Task.Run(() => ReportAdressesAsync());
        private async Task ReportAdressesAsync()
        {
            Logger.LogInformation($"Started IP Reporting...");

            foreach (var server in Servers)
            {
                if (!server.Config.EnableReporting)
                {
                    continue;
                }

                var ipReports = new List<IpReport>();

                foreach (var attempt in server.ConnectionAttempts)
                {
                    var reason = attempt.Value.CalculateReason();
                    ipReports.Add(new IpReport(attempt.Key.ToString(), reason));
                }

                server.ConnectionAttempts.Clear();

                var ips = ipReports.ToList();

                if (ips.Count > 0)
                {
                    try
                    {
                        Logger.LogDebug($"Reporting these IPs: {string.Join(", ", ips)}");

                        var packet = new ReportPacket(server.Config.ReportToken, server.Config.Protocol, ips);
                        string json = JsonSerializer.Serialize(packet);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await HttpClient.PostAsync(server.Config.ReportServer, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            Logger.LogError($"{response.StatusCode} : {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error while reporting IPs!");
                        throw;
                    }
                }

                Logger.LogDebug($"Finished Reporting catched IPs from StickyNet Port {server.Port}");
            }

            Logger.LogInformation("Finished IP Reporting!");
        }
    }
}
