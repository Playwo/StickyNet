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

        [Option('o', "output", HelpText = "The path to log requests to", Default = "Output.log")]
        public string OutputPath { get; set; }
    }
}
