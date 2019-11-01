using System.Text.Json.Serialization;

namespace StickyNet.Report
{
    public class ReportPacket
    {
        [JsonPropertyName("token")]
        public string Token { get; }

        [JsonPropertyName("ips")]
        public IpReport[] ReportedIps { get; }

        public ReportPacket(string token, IpReport[] reportedIps)
        {
            Token = token;
            ReportedIps = reportedIps;
        }
    }
}
