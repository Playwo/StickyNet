using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class NoneSession : StickyTcpSession
    {
        public NoneSession(TcpServer server, int timeout)
            : base(server, timeout)
        {
        }
    }
}
