using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using StickyNet.Report;

namespace StickyNet.Server.Tcp
{
    public class StickyTcpServer : TcpServer, IStickyServer
    {
        private readonly ILogger Logger;

        public StickyServerConfig Config { get; }
        public ITcpProtocol Protocol { get; }

        public EndPoint EndPoint => Endpoint;
        public int Port => (EndPoint as IPEndPoint).Port;

        public ConcurrentDictionary<IPAddress, RequestList> ConnectionAttempts { get; }


        public StickyTcpServer(IPAddress address, ITcpProtocol protocol, StickyServerConfig config, ILogger logger)
            : base(address, config.Port)
        {
            Config = config;
            Protocol = protocol;
            Logger = logger;

            if (Config.EnableReporting)
            {
                ConnectionAttempts = new ConcurrentDictionary<IPAddress, RequestList>();
            }
        }

        protected override async void OnConnected(TcpSession session)
        {
            try
            {
                var remoteEndPoint = session.Socket.RemoteEndPoint as IPEndPoint;
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

                await Protocol.PerformHandshakeAsync(this, session);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occured while someone connected to the TCP Server!");
            }
            finally
            {
                if (session.IsConnected)
                {
                    session.Disconnect();
                }
            }
        }

        protected override void OnStarted()
            => Logger.LogInformation($"Started TCP StickyNet on Port {Endpoint.Port}! Protocol : {Protocol.Name}");

        protected override void OnStopped()
            => Logger.LogInformation($"Stopped TCP StickyNet on Port {Endpoint.Port}");

        protected override void OnError(SocketError error)
            => Logger.LogWarning($"An error occured: {error}");

        public override bool Start() => base.Start();
        public override bool Stop() => base.Stop();
    }
}
