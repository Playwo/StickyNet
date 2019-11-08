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
    public class StickyTcpServer<ISession> : TcpServer, IStickyServer 
        where ISession : TcpSession
    {
        private readonly ILogger Logger;

        public event Action<IPAddress, ConnectionAttempt> CatchedIpAdress;

        public StickyServerConfig Config { get; }

        public EndPoint EndPoint => Endpoint;
        public int Port => (EndPoint as IPEndPoint).Port;


        public StickyTcpServer(IPAddress address, StickyServerConfig config, ILogger logger)
            : base(address, config.Port)
        {
            Config = config;
            Logger = logger;
        }

        protected override TcpSession CreateSession() 
            => (ISession)Activator.CreateInstance(typeof(ISession), this, Config.ConnectionTimeout);

        protected override async void OnConnected(TcpSession session)
        {
            try
            {
                var remoteEndPoint = session.Socket.RemoteEndPoint as IPEndPoint;
                var attempt = new ConnectionAttempt(DateTime.UtcNow, Port);

                Logger.LogInformation($"Catched someone: {remoteEndPoint.Address}:{remoteEndPoint.Port}");

                if (Config.EnableReporting)
                {
                    CatchedIpAdress?.Invoke(remoteEndPoint.Address, attempt);
                }
                if (Config.EnableOutput)
                {
                    await File.AppendAllTextAsync(Config.OutputPath, JsonSerializer.Serialize(attempt) + "\n");
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occured while someone connected to the TCP Server!");
            }
        }

        protected override void OnStarted()
            => Logger.LogInformation($"Started TCP StickyNet on Port {Endpoint.Port}! Protocol : {Config.Protocol}");

        protected override void OnStopped()
            => Logger.LogInformation($"Stopped TCP StickyNet on Port {Endpoint.Port}");

        protected override void OnError(SocketError error)
            => Logger.LogWarning($"An error occured: {error}");

        public override bool Start() => base.Start();
        public override bool Stop() => base.Stop();
    }
}
