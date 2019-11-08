using System.Net;
using System.Text.Json.Serialization;

namespace StickyNet.Report
{
    public class IpReport
    {
        [JsonPropertyName("ip")]
        public string Ip { get; }

        [JsonPropertyName("prt")]
        public PortTimeReport[] Reports { get; }

        public IpReport(IPAddress ip, PortTimeReport[] reports)
        {
            Ip = ip.ToString();
            Reports = reports;
        }

        public override string ToString() => $"{Ip} [{Reports.Length + 1}]";
    }
}
