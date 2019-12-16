using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StickyNet.Report
{
    public class ReportPacket
    {
        [JsonProperty("tk")]
        public string Token { get; private set; }

        [JsonProperty("st")]
        public long Timestamp { get; }

        [JsonProperty("ips")]
        public List<IpReport> ReportedIps { get; }

        public ReportPacket(DateTimeOffset starttime, List<IpReport> reports)
        {
            Timestamp = starttime.ToUnixTimeSeconds();
            ReportedIps = reports;
        }

        public Task<HttpResponseMessage> SendAsync(HttpClient client, TripLink tripLink, CancellationToken cancellationToken)
        {
            Token = tripLink.Token;
            string json = JsonConvert.SerializeObject(this);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PostAsync(tripLink.Server, content, cancellationToken);
        }
    }
}
