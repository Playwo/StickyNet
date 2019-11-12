using Newtonsoft.Json;
using StickyNet.Server;

namespace StickyNet.Service
{
    public class StickyServerConfig
    {
        public int Port { get; set; }
        public Protocol Protocol { get; set; } = Protocol.None;
        public string OutputPath { get; set; } = null;
        public int ConnectionTimeout { get; set; } = 5000;

        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public bool EnableOutput => OutputPath != null;

        public StickyServerConfig(int port, Protocol protocol,
            string outputPath, int connectionTimeout)
        {
            Port = port;
            Protocol = protocol;
            OutputPath = outputPath;
            ConnectionTimeout = connectionTimeout;
        }

        public StickyServerConfig()
        {
        }

        public bool IsValid()
            => Port >= 1 &&
                ConnectionTimeout >= 10 &&
                Port <= 65636;

        public bool Equals(StickyServerConfig other)
            => other == null
                ? false
                : Port == other.Port;

        public override bool Equals(object obj) => Equals(obj as StickyServerConfig);
        public override int GetHashCode() => Port;

        public override string ToString()
            => $"StickyNet Port: {Port}, Protocol: {Protocol}, Output: {(EnableOutput ? OutputPath : "None")}";
    }
}
