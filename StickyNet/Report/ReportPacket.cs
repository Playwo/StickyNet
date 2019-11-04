using System.Collections.Generic;
using System.Text.Json.Serialization;
using StickyNet.Server;

namespace StickyNet.Report
{
    public class ReportPacket
    {
        [JsonPropertyName("token")]
        public string Token { get; }

        [JsonPropertyName("ips")]
        public List<IpReport> ReportedIps { get; }

        [JsonPropertyName("note")]
        public string Note { get; }

        public ReportPacket(string token, Protocol protocol, List<IpReport> reportedIps)
        {
            Note = protocol switch
            {
                Protocol.None => null,
                _ => protocol.ToString().ToLower()
            };

            Token = token;
            ReportedIps = reportedIps;
        }
    }
}
