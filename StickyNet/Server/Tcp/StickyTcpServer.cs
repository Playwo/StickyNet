using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class StickyTcpServer : TcpServer, IStickyServer
    {
        private readonly ILogger Logger;

        public ITcpProtocol Protocol { get; }

        public EndPoint EndPoint => Endpoint;

        public int Port => (EndPoint as IPEndPoint).Port;

        public StickyTcpServer(IPAddress address, int port, ITcpProtocol protocol,
            ILogger logger)
            : base(address, port)
        {
            Protocol = protocol;
            Logger = logger;
        }

        protected override async void OnConnected(TcpSession session)
        {
            var remoteEndPoint = session.Socket.RemoteEndPoint as IPEndPoint;
            Logger.LogInformation($"Catched someone: {remoteEndPoint.Address}:{remoteEndPoint.Port}");

            await Protocol.PerformHandshakeAsync(this, session);

            if (session.IsConnected)
            {
                session.Disconnect();
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
