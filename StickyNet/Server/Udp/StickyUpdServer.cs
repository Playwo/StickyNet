using System.Net;
using NetCoreServer;

namespace StickyNet.Listener.Server.Udp
{
    public class StickyUpdServer : UdpServer, IStickyServer
    {
        public EndPoint EndPoint => Endpoint;

        public int Port => (EndPoint as IPEndPoint).Port;

        public StickyUpdServer(IPAddress address, int port)
            : base(address, port)
        {
        }

        public override bool Stop() => base.Stop();
        public override bool Start() => base.Start();

        protected override void OnStarted() => base.OnStarted();
    }
}
