using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace StickyNet.Report
{
    public class IpReport
    {
        [JsonProperty("ip")]
        public string Ip { get; }

        [JsonProperty("prt")]
        public IReadOnlyList<PortTimeReport> Reports { get; }

        public IpReport(IPAddress ip, IEnumerable<PortTimeReport> reports)
        {
            Ip = ip.ToString();
            Reports = reports.ToList().AsReadOnly();
        }

        public override string ToString()
            => $"{Ip} [{Reports.Sum(x => x.RelativeTimestamps.Count)}R | {Reports.Count}P]";
    }
}
