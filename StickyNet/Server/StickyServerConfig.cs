using System;

namespace StickyNet.Server
{
    public class StickyServerConfig
    {
        public int Port { get; set; }
        public Protocol Protocol { get; set; }
        public string OutputPath { get; set; }
        public bool EnableOutput => OutputPath != null;
        public int ConnectionTimeout { get; set; }

        public Uri ReportServer { get; set; } = null;
        public string ReportToken { get; set; } = null;
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

        public bool Equals(StickyServerConfig other)
            => other == null
                ? false
                : Port == other.Port;

        public override bool Equals(object obj) => Equals(obj as StickyServerConfig);

        public override int GetHashCode() => Port;
    }
}
