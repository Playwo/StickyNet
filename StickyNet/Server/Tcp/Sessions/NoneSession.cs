using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class NoneSession : StickyTcpSession
    {
        public NoneSession(TcpServer server) 
            : base(server, 1000)
        {
        }
    }
}
