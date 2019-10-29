namespace StickyNet.Server
{
    public class StickyServerConfig
    {
        public int Port { get; set; }
        public Protocol Protocol { get; set; }
        public string OutputPath { get; set; }

        public StickyServerConfig(int port, Protocol protocol, string outputPath)
        {
            Port = port;
            Protocol = protocol;
            OutputPath = outputPath;
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
