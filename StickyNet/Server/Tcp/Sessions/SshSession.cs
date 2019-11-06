using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class SshSession : StickyTcpSession
    {
        public SshSession(TcpServer server) 
            : base(server, 1000)
        {
        }
    }
}
