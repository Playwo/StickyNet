using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;
using StickyNet.Listener.Servers;
using StickyNet.StartParameters;

namespace StickyNet
{
    public class Startup
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly ILogger<Startup> Logger;

        public Startup()
        {
            LoggerFactory = MakeLoggerFactory();
            Logger = LoggerFactory.CreateLogger<Startup>();
        }

        private ILoggerFactory MakeLoggerFactory()
            => Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });

        public void Run(string[] args)
        {
            Logger.LogDebug("Creating parser...");
            var parser = new Parser(x =>
            {
                x.AutoHelp = true;
                x.AutoVersion = true;
                x.CaseInsensitiveEnumValues = false;
                x.CaseSensitive = false;
                x.EnableDashDash = true;
                x.HelpWriter = Console.Out;
                x.IgnoreUnknownArguments = false;
            });
            Logger.LogDebug("Parsing arguments...");
            var result = parser.ParseArguments<ListenerOptions>(args)
                .WithParsed(opt => RunListenerAsync(opt).GetAwaiter().GetResult());
        }

        private async Task RunListenerAsync(ListenerOptions options)
        {
            var logger = LoggerFactory
                .AddFile(options.OutputPath)
                .CreateLogger<TcpStickyNet>();

            Logger.LogInformation("Retrieving local IP Address...");

            var host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            var ip = host.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);

            Logger.LogInformation("Creating TCPStickyNet...");

            var listener = new TcpStickyNet(ip, options.Port, new TcpProtocol(), logger);

            Logger.LogInformation("Starting TCPStickyNet...");

            listener.Start();

            await Task.Delay(-1);
        }
    }
}
