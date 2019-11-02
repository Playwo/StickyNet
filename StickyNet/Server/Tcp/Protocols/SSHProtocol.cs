using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp.Protocols
{
    public class SSHProtocol : ITcpProtocol
    {
        public string Name => "SSH";

        public Task<bool> PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.Disconnect();
            return Task.FromResult(true);
        }
    }
}
