﻿using CommandLine;
using Microsoft.Extensions.Logging;

namespace StickyNet.Options
{
    [Verb("delete", HelpText = "Removes a StickyNet from the given port")]
    public class DeleteOptions : IOption
    {
        [Option('p', "port", HelpText = "The port of the StickyNet to remove", Required = true)]
        public int Port { get; set; }

        [Option('l', "loglevel", HelpText = "The minimum log level to display", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }
}
