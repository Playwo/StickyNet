using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using StickyNet.Report;

namespace StickyNet.Server.Tcp
{
    public class StickyTcpServer<ISession> : TcpServer, IStickyServer 
        where ISession : TcpSession
    {
        private readonly ILogger Logger;

        public StickyServerConfig Config { get; }

        public EndPoint EndPoint => Endpoint;
        public int Port => (EndPoint as IPEndPoint).Port;

        public ConcurrentDictionary<IPAddress, RequestList> ConnectionAttempts { get; }


        public StickyTcpServer(IPAddress address, StickyServerConfig config, ILogger logger)
            : base(address, config.Port)
        {
            Config = config;
            Logger = logger;

            if (Config.EnableReporting)
            {
                ConnectionAttempts = new ConcurrentDictionary<IPAddress, RequestList>();
            }
        }

        protected override TcpSession CreateSession() 
            => (ISession)Activator.CreateInstance(typeof(ISession), this);

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
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occured while someone connected to the TCP Server!");
            }
            finally
            {
                await Task.Delay(30000); //Close the connection if its open for longer than 30secs
                session.Disconnect();
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
