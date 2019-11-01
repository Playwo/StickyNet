using System.Text.Json.Serialization;

namespace StickyNet.Report
{
    public class IpReport
    {
        [JsonPropertyName("ip")]
        public string Ip { get; }

        [JsonPropertyName("r")]
        public Reason Reason { get; }

        [JsonPropertyName("v")]
        public int Valid => 0;

        public IpReport(string ip, Reason reason)
        {
            Ip = ip;
            Reason = reason;
        }
    }
}
