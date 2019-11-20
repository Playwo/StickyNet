using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StickyNet.Report;
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
        private readonly HttpClient HttpClient;

        private DateTimeOffset LastReport { get; set; }

        public System.Timers.Timer ReporterTimer { get; }
        public ConcurrentDictionary<int, IStickyServer> Servers { get; }
        public ConcurrentDictionary<IPAddress, ConcurrentBag<ConnectionAttempt>> AttemptCache { get; }
        public StickyGlobalConfig Config => Configuration.StickyConfig;

        public StickyNetRunner(ConfigService configuration, ILogger<StickyNetRunner> logger, ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            Logger = logger;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            HttpClient = httpClient;
            LastReport = DateTimeOffset.UtcNow;

            AttemptCache = new ConcurrentDictionary<IPAddress, ConcurrentBag<ConnectionAttempt>>();
            Servers = new ConcurrentDictionary<int, IStickyServer>();
            ReporterTimer = new System.Timers.Timer(10000); //10 Seconds
            ReporterTimer.Elapsed += ReporterTimer_Elapsed;
            ReporterTimer.Start();
        }

        private Task StartServerAsync(StickyServerConfig config)
        {
            Logger.LogDebug("Found new StickyNet in config file...");

            var ip = IPAddress.Any;
            var logger = LoggerFactory.CreateLogger($"StickyNet Port{config.Port} [{config.Protocol}]");

            var server = config.Protocol switch
            {
                Protocol.None => (IStickyServer) new StickyTcpServer<NoneSession>(ip, config, logger),
                Protocol.FTP => new StickyTcpServer<FtpSession>(ip, config, logger),
                Protocol.SSH => new StickyTcpServer<SshSession>(ip, config, logger),
                Protocol.Telnet => new StickyTcpServer<TelnetSession>(ip, config, logger),
                _ => null
            };

            Servers.TryAdd(server.Port, server);
            server.CatchedIpAdress += Server_CatchedIpAdress;
            server.Start();

            return Task.CompletedTask;
        }

        private Task StopServerAsync(StickyServerConfig config)
        {
            if (!Servers.TryGetValue(config.Port, out var server))
            {
                return Task.CompletedTask;
            }

            server.CatchedIpAdress -= Server_CatchedIpAdress;
            server.Stop();
            server.Dispose();

            Servers.TryRemove(server.Port, out _);

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


        private void ReporterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Config.EnableReporting)
            {
                _ = ReportAdressesAsync();
            }
        }

        private async Task ReportAdressesAsync()
        {
            Logger.LogDebug($"Starting IP Reporting...");

            var attempts = AttemptCache.ToList();
            AttemptCache.Clear();

            var startTime = LastReport;
            LastReport = DateTimeOffset.UtcNow;

            var ipReports = new List<IpReport>();

            foreach (var item in attempts)
            {
                var portReports = item.Value.GroupBy(attempt => attempt.Port)
                                            .Select(group => new PortTimeReport(group.Key,
                                                                                group.Select(z => z.Time),
                                                                                startTime))
                                            .ToArray();

                var ipReport = new IpReport(item.Key, portReports);
                ipReports.Add(ipReport);
            }

            if (ipReports.Count <= 0)
            {
                Logger.LogDebug("There were no IPs to report!");
            }
            else
            {
                foreach (var tripLink in Config.TripLinks)
                {
                    var reportPacket = new ReportPacket(tripLink.Token, startTime, ipReports.ToArray());

                    try
                    {
                        string json = JsonConvert.SerializeObject(reportPacket);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await HttpClient.PostAsync(tripLink.Server, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            Logger.LogError($"{response.StatusCode} : {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error while reporting IPs!");
                    }

                    Logger.LogInformation($"Reported {ipReports.Count} IPs: \r\n{string.Join("\r\n", reportPacket.ReportedIps.AsEnumerable())}");
                }
            }

            Logger.LogDebug("Finished IP Reporting");
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping StickyNet Launcher...");

            foreach (var server in Servers.Select(x => x.Value))
            {
                await StopServerAsync(server.Config);
            }
        }
    }
}
