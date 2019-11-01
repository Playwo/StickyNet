using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using StickyNet.Report;

namespace StickyNet.Server.Udp
{
    public class StickyUpdServer : UdpServer, IStickyServer
    {
        private readonly ILogger Logger;

        public StickyServerConfig Config { get; }
        public IUdpProtocol Protocol { get; }

        public EndPoint EndPoint => Endpoint;
        public int Port => (EndPoint as IPEndPoint).Port;

        public ConcurrentDictionary<IPAddress, RequestList> ConnectionAttempts { get; }

        public StickyUpdServer(IPAddress address, StickyServerConfig config, IUdpProtocol protocol, ILogger logger)
            : base(address, config.Port)
        {
            ConnectionAttempts = new ConcurrentDictionary<IPAddress, RequestList>();

            Config = config;
            Protocol = protocol;
            Logger = logger;
        }

        protected override async void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            try
            {
                var remoteEndPoint = endpoint as IPEndPoint;
                Logger.LogInformation($"Catched someone: {remoteEndPoint.Address}:{remoteEndPoint.Port}");

                var attempt = new ConnectionAttempt(remoteEndPoint.Address, DateTime.UtcNow, Port);

                if (Config.EnableOutput)
                {
                    await File.AppendAllTextAsync(Config.OutputPath, JsonSerializer.Serialize(attempt) + "\n");
                }
                if (Config.EnableReporting)
                {
                    ConnectionAttempts.AddOrUpdate(remoteEndPoint.Address,
                        ip => new RequestList().AddConnection(attempt),
                        (ip, list) => list.AddConnection(attempt));
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
