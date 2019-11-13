using System.Net;
using Newtonsoft.Json;

namespace StickyNet.Report
{
    public class IpReport
    {
        [JsonProperty("ip")]
        public string Ip { get; }

        [JsonProperty("prt")]
        public PortTimeReport[] Reports { get; }

        public IpReport(IPAddress ip, PortTimeReport[] reports)
        {
            Ip = ip.ToString();
            Reports = reports;
        }

        public override string ToString() => $"{Ip} [{Reports.Length}]";
    }
}
