using CommandLine;
using Microsoft.Extensions.Logging;

namespace StickyNet.Arguments
{
    [Verb("run", HelpText = "Run StickyNet on all configured Ports")]
    public class RunOptions
    {
        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
