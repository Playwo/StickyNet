using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
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
using StickyNet.Service;

namespace StickyNet
{
    public class StickyNetWorker : BackgroundService
    {
        private readonly ILogger<StickyNetWorker> Logger;
        private readonly ConfigService Configuration;
        private readonly ILoggerFactory LoggerFactory;
        private readonly HttpClient HttpClient;

        private DateTimeOffset LastReport { get; set; }

        public System.Timers.Timer ReporterTimer { get; }
        public List<IStickyServer> Servers { get; }
        public ConcurrentDictionary<IPAddress, ConcurrentBag<ConnectionAttempt>> AttemptCache { get; }

        public StickyNetWorker(ConfigService configuration, ILogger<StickyNetWorker> logger, ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            Logger = logger;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            HttpClient = httpClient;
            LastReport = DateTimeOffset.UtcNow;

            Servers = new List<IStickyServer>();
            ReporterTimer = new System.Timers.Timer(10000); //10 Seconds
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

            var server =  config.Protocol switch
            {
                Protocol.None => (IStickyServer) new StickyTcpServer<NoneSession>(ip, config, logger),
                Protocol.FTP => new StickyTcpServer<FtpSession>(ip, config, logger),
                Protocol.SSH => new StickyTcpServer<SshSession>(ip, config, logger),
                Protocol.Telnet => new StickyTcpServer<TelnetSession>(ip, config, logger),
                _ => null
            };

            Servers.Add(server);
            if (server.Config.EnableReporting)
            {
                server.CatchedIpAdress += Server_CatchedIpAdress;
            }
            server.Start();

            return Task.CompletedTask;
        }

        private Task StopServerAsync(StickyServerConfig config)
        {
            var server = Servers.Where(x => x.Port == config.Port).First();
            server.CatchedIpAdress -= Server_CatchedIpAdress;
            server.Stop();
            server.Dispose();

            Servers.RemoveAll(x => x.Port == config.Port);

            return Task.CompletedTask;
        }

        private void Server_CatchedIpAdress(IPAddress ip, ConnectionAttempt attempt) 
            => AttemptCache.AddOrUpdate(ip, x => new ConcurrentBag<ConnectionAttempt>(
                new ConcurrentBag<ConnectionAttempt>() { attempt }), 
                (ip, bag) =>
                {
                    bag.Add(attempt);
                    return bag;
                });


        private void ReporterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => _ = Task.Run(() => ReportAdressesAsync());
        private async Task ReportAdressesAsync()
        {
            Logger.LogDebug($"Starting IP Reporting...");

            var attempts = AttemptCache.ToList();
            AttemptCache.Clear();

            var startTime = LastReport;

            var ipReports = new List<IpReport>();

            foreach(var item in attempts)
            {
                var portReports = item.Value.GroupBy(attempt => attempt.Port)
                                            .Select(group => new PortTimeReport(group.Key, 
                                                                                group.Select(z => z.Time), 
                                                                                startTime))
                                            .ToArray();

                var ipReport = new IpReport(item.Key, portReports);
                ipReports.Add(ipReport);
            }

            if (ipReports.Count > 0)
            {
                var reportPacket = new ReportPacket(, startTime, ipReports.ToArray());

                try
                {
                    string json = JsonSerializer.Serialize(reportPacket);
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
                }

                Logger.LogInformation($"Reported {ipReports.Count} IPs: \n{string.Join("\n",reportPacket.ReportedIps.AsEnumerable())}");
            }

            Logger.LogDebug("Finished IP Reporting");
        }
    }
}
