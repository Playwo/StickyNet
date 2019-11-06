using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp.Protocols
{
    public class SSHProtocol : ITcpProtocol
    {
        public string Name => "SSH";

        public Task PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.Disconnect();
            return Task.FromResult(true);
        }
    }
}
