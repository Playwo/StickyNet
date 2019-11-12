using CommandLine;
using Microsoft.Extensions.Logging;

namespace StickyNet.Options
{
    [Verb("addtriplink", HelpText = "Add a TripLink server to send the catches to")]
    public class AddTripLinkOptions : IOption
    {
        [Option('s', "server", HelpText = "The address of the TripLink server", Required = true)]
        public string ReportServer { get; set; }

        [Option('t', "token", HelpText = "The user token to use for the report", Required = true)]
        public string ReportToken { get; set; }

        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
