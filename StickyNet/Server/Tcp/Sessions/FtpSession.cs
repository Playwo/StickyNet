using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class FtpSession : TcpSession
    {
        public FtpSession(TcpServer server) 
            : base(server)
        {
        }

        protected override void OnConnected() 
            => SendAsync("220 ProFTPD 1.3.5b Server (Debian) [::ffff:134.255.225.218]");
    }
}
