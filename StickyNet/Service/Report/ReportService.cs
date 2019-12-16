using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
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

        public DateTimeOffset LastReport { get; private set; }

        public ReportService(HttpClient client, ConfigService config, ILogger<ReportService> logger)
        {
            Client = client;
            Config = config;
            Logger = logger;

            ReporterStopper = new CancellationTokenSource();
        }

        public async Task StartReporterAsync(ChannelReader<ConnectionAttempt> attemptReader)
        {
            while (true)
            {
                LastReport = DateTimeOffset.UtcNow;
                await attemptReader.WaitToReadAsync(ReporterStopper.Token); //Wait until data is there to prevent empty reports
                await Task.Delay(10000, ReporterStopper.Token); //Wait 10 seconds to stack up some data for a batch
                var attempts = new Dictionary<IPAddress, List<ConnectionAttempt>>();

                while (attemptReader.TryRead(out var attempt)) //Load all attempts that stacked up in the last 10 seconds
                {
                    if (!attempts.TryAdd(attempt.IP, new List<ConnectionAttempt>() { attempt }))
                    {
                        attempts[attempt.IP].Add(attempt); //Order them by IP
                    }
                }

                Logger.LogTrace($"Starting IP Reporting...");

                var startTime = LastReport;
                var ipReports = new List<IpReport>();

                foreach (var item in attempts) //Order the data for a report Package
                {
                    var portReports = item.Value.GroupBy(attempt => attempt.Port)
                                                .Select(group => new PortTimeReport(group.Key,
                                                                                    group.Select(z => z.Time),
                                                                                    startTime));

                    var ipReport = new IpReport(item.Key, portReports);
                    ipReports.Add(ipReport);
                }

                var packet = new ReportPacket(LastReport, ipReports);
                await ReportAsync(packet, ReporterStopper.Token);

                Logger.LogTrace("Finished IP Reporting");
            }
        }

        public void StopReporter()
            => ReporterStopper.Cancel();

        public async Task ReportAsync(ReportPacket packet, CancellationToken cancellationToken)
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
