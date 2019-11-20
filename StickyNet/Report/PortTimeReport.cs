using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StickyNet.Report
{
    public class PortTimeReport
    {
        [JsonProperty("p")]
        public int Port { get; }

        [JsonProperty("t")]
        public IReadOnlyList<int> RelativeTimestamps { get; }

        public PortTimeReport(int port, IEnumerable<DateTimeOffset> timestamps, DateTimeOffset startTime)
        {
            Port = port;
            RelativeTimestamps = timestamps.Select(t => (int) (startTime - t).TotalSeconds).ToList().AsReadOnly();
        }
    }
}
