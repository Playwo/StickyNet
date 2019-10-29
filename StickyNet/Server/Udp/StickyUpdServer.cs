using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace StickyNet.Server.Udp
{
    public class StickyUpdServer : UdpServer, IStickyServer
    {
        private readonly ILogger Logger;

        public IUdpProtocol Protocol { get; }

        public EndPoint EndPoint => Endpoint;

        public int Port => (EndPoint as IPEndPoint).Port;

        public StickyUpdServer(IPAddress address, int port,
            IUdpProtocol protocol, ILogger logger)
            : base(address, port)
        {
            Protocol = protocol;
            Logger = logger;
        }

        protected override async void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size) 
            => await Protocol.PerformHandshakeAsync(endpoint);

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
