using System;
using System.Net;
using Newtonsoft.Json;

namespace StickyNet.Server
{
    public class ConnectionAttempt
    {
        [JsonIgnore]
        public IPAddress IP { get; }

        [JsonProperty("t")]
        public DateTimeOffset Time { get; }

        [JsonProperty("p")]
        public int Port { get; }

        public ConnectionAttempt(IPAddress ip, DateTimeOffset time, int port)
        {
            IP = ip;
            Time = time;
            Port = port;
        }
    }
}
