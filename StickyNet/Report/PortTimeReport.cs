using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace StickyNet.Report
{
    public class PortTimeReport
    {
        [JsonPropertyName("p")]
        public int Port { get; }

        [JsonPropertyName("t")]
        public int[] RelativeTimestamps { get; }

        public PortTimeReport(int port, IEnumerable<DateTimeOffset> timestamps, DateTimeOffset startTime)
        {
            Port = port;
            RelativeTimestamps = timestamps.Select(timestamp => (int) (startTime - timestamp).TotalSeconds).ToArray();
        }
    }
}
