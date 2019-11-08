using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using StickyNet.Server;

namespace StickyNet.Report
{
    public class ReportPacket
    {
        [JsonPropertyName("tk")]
        public string Token { get; }

        [JsonPropertyName("st")]
        public long Timestamp { get; }

        [JsonPropertyName("ips")]
        public IpReport[] ReportedIps { get; }

        public ReportPacket(string token, DateTimeOffset starttime, IpReport[] reports)
        {
            Token = token;
            Timestamp = starttime.ToUnixTimeSeconds();
            ReportedIps = reports;
        }
    }
}
