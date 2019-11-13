using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using Newtonsoft.Json;
using StickyNet.Service;

namespace StickyNet.Server.Udp
{
    public class StickyUpdServer : UdpServer, IStickyServer
    {
        private readonly ILogger Logger;

        public event Action<IPAddress, ConnectionAttempt> CatchedIpAdress;

        public StickyServerConfig Config { get; }
        public IUdpProtocol Protocol { get; }

        public EndPoint EndPoint => Endpoint;
        public int Port => (EndPoint as IPEndPoint).Port;

        public StickyUpdServer(IPAddress address, StickyServerConfig config, IUdpProtocol protocol, ILogger logger)
            : base(address, config.Port)
        {
            Config = config;
            Protocol = protocol;
            Logger = logger;
        }

        protected override async void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            try
            {
                var remoteEndPoint = endpoint as IPEndPoint;
                var attempt = new ConnectionAttempt(DateTime.UtcNow, Port);

                CatchedIpAdress?.Invoke(remoteEndPoint.Address, attempt);
                Logger.LogInformation($"Catched someone: {remoteEndPoint.Address}:{remoteEndPoint.Port}");

                if (Config.EnableOutput)
                {
                    await File.AppendAllTextAsync(Config.OutputPath, JsonConvert.SerializeObject(attempt) + "\r\n");
                }

                await Protocol.PerformHandshakeAsync(endpoint);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occured while someone connected to the UDP Server!");
            }
        }

        protected override void OnStarted()
            => Logger.LogInformation($"Started UDP StickyNet on Port {Endpoint.Port}! Protocol : {Protocol.Name}");

        protected override void OnStopped()
            => Logger.LogInformation($"Stopped UDP StickyNet on Port {Endpoint.Port}");

        protected override void OnError(SocketError error)
            => Logger.LogWarning($"An error occured: {error}");

        public override bool Stop() => base.Stop();
        public override bool Start() => base.Start();
    }
}
