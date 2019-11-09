using CommandLine;
using Microsoft.Extensions.Logging;

namespace StickyNet.Options
{
    [Verb("list", HelpText = "List all running StickyNets on this machine")]
    public class ListOptions : IOption
    {
        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
