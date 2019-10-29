using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public interface ITcpProtocol
    {
        public string Name { get; }
        public Task<bool> PerformHandshakeAsync(TcpServer server, TcpSession session);
    }
}
