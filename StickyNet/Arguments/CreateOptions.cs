using CommandLine;
using StickyNet.Server;

namespace StickyNet.Arguments
{
    [Verb("create", HelpText = "Creates a new service which automatically starts the StickyNet on startup")]
    public class CreateOptions
    {
        [Option('p', "port", HelpText = "The port to run the StickyNet on", Required = true)]
        public int Port { get; set; }

        [Option('i', "imitate", HelpText = "The protocol the StickyNet imitates", Default = Protocol.None)]
        public Protocol Protocol { get; set; }

        [Option('o', "output", HelpText = "The path to log requests to", Default = null)]
        public string OutputPath { get; set; }

        [Option('s', "reportserver", HelpText = "The ip and port of the server to report the connection attempts to", Default = null)]
        public string ReportServer { get; set; }

        [Option('t', "reporttoken", HelpText = "The token to use to authenticate on the reportserver")]
        public string ReportToken { get; set; }
    }
}
