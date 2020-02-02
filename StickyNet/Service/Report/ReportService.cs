using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using StickyNet.Report;
using StickyNet.Server;

namespace StickyNet.Service
{
    public class ReportService
    {
        private readonly HttpClient Client;
        private readonly ConfigService Config;
        private readonly ILogger<ReportService> Logger;

        private readonly CancellationTokenSource ReporterStopper;
        private readonly System.Timers.Timer ReportTimer;

        private readonly ConcurrentDictionary<IPAddress, ConcurrentDictionary<int, ConcurrentBag<DateTimeOffset>>> Attempts;
        private readonly object AttemptsLock;
        public DateTimeOffset LastReport { get; private set; }

        public ReportService(HttpClient client, ConfigService config, ILogger<ReportService> logger)
        {
            Client = client;
            Config = config;
            Logger = logger;

            Attempts = new ConcurrentDictionary<IPAddress, ConcurrentDictionary<int, ConcurrentBag<DateTimeOffset>>>();
            AttemptsLock = new object();
            ReporterStopper = new CancellationTokenSource();

            ReportTimer = new System.Timers.Timer()
            {
                AutoReset = false,
                Interval = 10000,
            };

            ReportTimer.Elapsed += StartReportAsync;
        }

        private async void StartReportAsync(object s, ElapsedEventArgs e)
        {
            if (!Config.HasTripLinks)
            {
                ReportTimer.Start();
                return;
            }

            Logger.LogTrace($"Starting IP Reporting...");

            var reportTime = DateTime.UtcNow;

            var ipReports = Attempts.Select(x => new IpReport(x.Key, x.Value.Select(x => new PortTimeReport(x.Key, x.Value, reportTime)))).ToList();
            Attempts.Clear();

            var packet = new ReportPacket(reportTime, ipReports);
            await SendReportAsync(packet, ReporterStopper.Token);

            Logger.LogTrace("Finished IP Reporting");

            LastReport = reportTime;
            ReportTimer.Start();
        }

        public void StartReporter()
        {
            if (!Config.HasTripLinks)
            {
                Logger.LogWarning("There are no TripLinks configured! => Your catches are not reported to a server!");
            }

            ReportTimer.Start();
        }

        public void StopReporter()
        {
            ReporterStopper.Cancel();
            ReportTimer.Stop();
        }

        public void Report(ConnectionAttempt attempt)
        {
            if (!Config.HasTripLinks)
            {
                return;
            }

            Attempts.AddOrUpdate(attempt.IP, (ip) =>
            {
                var newDict = new ConcurrentDictionary<int, ConcurrentBag<DateTimeOffset>>();
                newDict.TryAdd(attempt.Port, new ConcurrentBag<DateTimeOffset>() { attempt.Time });
                return newDict;
            },
            (ip, innerDict) =>
            {
                innerDict.AddOrUpdate(attempt.Port,
                    (port) =>
                    {
                        return new ConcurrentBag<DateTimeOffset>() { attempt.Time };
                    },
                    (port, bag) =>
                    {
                        bag.Add(attempt.Time);
                        return bag;
                    });
                return innerDict;
            });
        }

        private async Task SendReportAsync(ReportPacket packet, CancellationToken cancellationToken)
        {
            int successfulReports = 0;

            foreach (var tripLink in Config.StickyConfig.TripLinks)
            {
                try
                {
                    var response = await packet.SendAsync(Client, tripLink, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.LogError($"Error reporting to {tripLink.Server}! {response.StatusCode} : {response.ReasonPhrase}");
                        continue;
                    }

                    Logger.LogTrace($"Successfully reported to {tripLink.Server}!");
                    successfulReports++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error while reporting IPs!");
                    continue;
                }
            }

            if (successfulReports > 0)
            {
                Logger.LogInformation($"Reported {packet.ReportedIps.Count} IPs to {successfulReports} Hosts: {string.Join("; ", packet.ReportedIps)}");
            }
        }
    }
}
