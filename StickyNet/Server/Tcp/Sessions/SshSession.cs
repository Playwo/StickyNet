using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class SshSession : TcpSession
    {
        public SshSession(TcpServer server) 
            : base(server)
        {
        }

        protected override void OnConnected() => Disconnect();
    }
}
