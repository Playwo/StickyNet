using CommandLine;
using Microsoft.Extensions.Logging;
using StickyNet.Server;

namespace StickyNet.Options
{
    [Verb("create", HelpText = "Creates a new service which automatically starts the StickyNet on startup")]
    public class CreateOptions : IOption
    {
        [Option('p', "port", HelpText = "The port to run the StickyNet on", Required = true)]
        public int Port { get; set; }

        [Option('i', "imitate", HelpText = "The protocol the StickyNet imitates", Default = Protocol.None)]
        public Protocol Protocol { get; set; }

        [Option('o', "output", HelpText = "The path to log requests to", Default = null)]
        public string OutputPath { get; set; }

        [Option('t', "connectiontimeout", HelpText = "The timeout in ms after which a inactive connection is closed", Default = 5000)]
        public int ConnectionTimeout { get; set; }

        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
