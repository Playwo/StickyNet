using CommandLine;

namespace StickyNet.StartParameters
{
    [Verb("Start", HelpText = "Start a listener")]
    public class ListenerOptions
    {
        [Option('P', "Port", HelpText = "The port to run the listener on", Required = true)]
        public int Port { get; private set; }

        [Option('O', "Output", HelpText = "The path to log requests to")]
        public string OutputPath { get; private set; }
    }
}
