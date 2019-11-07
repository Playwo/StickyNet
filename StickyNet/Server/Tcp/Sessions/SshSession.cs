using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class SshSession : StickyTcpSession
    {
        public SshSession(TcpServer server, int timeout) 
            : base(server, timeout)
        {
        }
    }
}
