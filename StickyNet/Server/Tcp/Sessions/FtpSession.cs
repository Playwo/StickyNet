using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class FtpSession : StickyTcpSession
    {
        public FtpSession(TcpServer server, int timeout) 
            : base(server, timeout)
        {
        }

        protected override void OnConnected() 
            => SendAsync("220 ProFTPD 1.3.5b Server (Debian) [::ffff:134.255.225.218]");
    }
}
