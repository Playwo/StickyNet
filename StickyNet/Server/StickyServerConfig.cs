using System;
using System.Text.Json.Serialization;

namespace StickyNet.Server
{
    public class StickyServerConfig
    {
        public int Port { get; set; } 
        public Protocol Protocol { get; set; } = Protocol.None;
        public string OutputPath { get; set; } = null;

        [JsonIgnore]
        public bool EnableOutput => OutputPath != null;
        public int ConnectionTimeout { get; set; } = 5000;

        public Uri ReportServer { get; set; } = null;
        public string ReportToken { get; set; } = null;

        [JsonIgnore]
        public bool EnableReporting => ReportServer != null;

        public StickyServerConfig(int port, Protocol protocol,
            string outputPath, int connectionTimeout,
            Uri reportServer, string reportToken)
        {
            Port = port;
            Protocol = protocol;
            OutputPath = outputPath;
            ConnectionTimeout = connectionTimeout;
            ReportServer = reportServer;
            ReportToken = reportToken;
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
    }
}
