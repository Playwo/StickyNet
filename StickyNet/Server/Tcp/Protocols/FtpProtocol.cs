using System.Net;
using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp.Protocols
{
    public class FtpProtocol : ITcpProtocol
    {
        public string Name => "FTP";

        public async Task<bool> PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            var localEndpoint = server.Endpoint as IPEndPoint;

            session.SendAsync($"220 ProFTPD 1.3.5b Server (Debian) [::ffff:134.255.225.218]");

            await Task.Delay(10000);

            return true;
        }
    }
}
