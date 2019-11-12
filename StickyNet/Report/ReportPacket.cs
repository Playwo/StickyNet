using System;
using Newtonsoft.Json;

namespace StickyNet.Report
{
    public class ReportPacket
    {
        [JsonProperty("tk")]
        public string Token { get; }

        [JsonProperty("st")]
        public long Timestamp { get; }

        [JsonProperty("ips")]
        public IpReport[] ReportedIps { get; }

        public ReportPacket(string token, DateTimeOffset starttime, IpReport[] reports)
        {
            Token = token;
            Timestamp = starttime.ToUnixTimeSeconds();
            ReportedIps = reports;
        }
    }
}
