using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace StickyNet.Listener.Servers
{
    public class TcpStickyNet : TcpServer
    {
        private readonly ILogger<TcpStickyNet> Logger;

        public TcpProtocol Protocol { get; }

        public TcpStickyNet(IPAddress address, int port, TcpProtocol protocol,
            ILogger<TcpStickyNet> logger)
            : base(address, port)
        {
            Protocol = protocol;
            Logger = logger;
        }

        protected async override void OnConnected(TcpSession session)
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
    }
}
