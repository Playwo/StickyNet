using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        public Task<HttpResponseMessage> SendAsync(HttpClient client, TripLink tripLink)
        {
                string json = JsonConvert.SerializeObject(this);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return client.PostAsync(tripLink.Server, content);
        }
    }
}
