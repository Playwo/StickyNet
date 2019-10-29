using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class StickyTcpServer : TcpServer, IStickyServer
    {
        private readonly ILogger Logger;

        public ITcpProtocol Protocol { get; }
        public string OutputPath { get; }
        public bool EnableOutput { get; }

        public EndPoint EndPoint => Endpoint;
        public int Port => (EndPoint as IPEndPoint).Port;

        public StickyTcpServer(IPAddress address, ITcpProtocol protocol, StickyServerConfig config, ILogger logger)
            : base(address, config.Port)
        {
            OutputPath = config.OutputPath;
            EnableOutput = config.EnableOutput;
            Protocol = protocol;
            Logger = logger;
        }

        protected override async void OnConnected(TcpSession session)
        {
            try
            {
                var remoteEndPoint = session.Socket.RemoteEndPoint as IPEndPoint;
                Logger.LogInformation($"Catched someone: {remoteEndPoint.Address}:{remoteEndPoint.Port}");

                if (EnableOutput)
                {
                    var attempt = new ConnectionAttempt(remoteEndPoint.Address, DateTime.UtcNow, Port);
                    await File.AppendAllTextAsync(OutputPath, JsonSerializer.Serialize(attempt) + "\n");
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
