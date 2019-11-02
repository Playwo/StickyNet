using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StickyNet.Report
{
    public class ReportPacket
    {
        [JsonPropertyName("token")]
        public string Token { get; }

        [JsonPropertyName("ips")]
        public List<IpReport> ReportedIps { get; }

        public ReportPacket(string token, List<IpReport> reportedIps)
        {
            Token = token;
            ReportedIps = reportedIps;
        }
    }
}
