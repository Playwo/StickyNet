using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class NoneSession : TcpSession
    {
        public NoneSession(TcpServer server) 
            : base(server)
        {
        }

        protected override void OnConnected() => Disconnect();
    }
}
