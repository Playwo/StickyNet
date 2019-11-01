using System;

namespace StickyNet.Server
{
    public class StickyServerConfig
    {
        public int Port { get; set; }
        public Protocol Protocol { get; set; }
        public string OutputPath { get; set; }
        public bool EnableOutput => OutputPath != null;

        public Uri ReportServer { get; set; }
        public string ReportToken { get; set; }
        public bool EnableReporting => ReportServer != null;

        public StickyServerConfig(int port, Protocol protocol, string outputPath, Uri reportServer, string reportToken)
        {
            Port = port;
            Protocol = protocol;
            OutputPath = outputPath;
            ReportServer = reportServer;
            ReportToken = reportToken;
        }

        public StickyServerConfig()
        {
        }

        public bool Equals(StickyServerConfig other) => other == null
                                                            ? false
                                                            : Port == other.Port;

        public override bool Equals(object obj) => Equals(obj as StickyServerConfig);

        public override int GetHashCode() => Port;
    }
}
