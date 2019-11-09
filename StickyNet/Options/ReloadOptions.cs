using CommandLine;
using Microsoft.Extensions.Logging;

namespace StickyNet.Options
{
    [Verb("reload", HelpText = "Reload the config files")]
    public class ReloadOptions : IOption
    {
        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
