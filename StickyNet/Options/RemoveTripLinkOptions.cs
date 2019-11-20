using CommandLine;
using Microsoft.Extensions.Logging;

namespace StickyNet.Options
{
    [Verb("removetriplink", HelpText = "Remove a TripLink Server from the config file")]
    public class RemoveTripLinkOptions : IOption
    {
        [Option('s', "server", HelpText = "The server address of the TripLink server to remove", Required = true)]
        public string ReportServer { get; set; }

        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
