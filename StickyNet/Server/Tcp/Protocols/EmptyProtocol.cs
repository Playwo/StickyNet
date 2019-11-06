using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class EmptyTcpProtocol : ITcpProtocol
    {
        public string Name => "None";

        public Task PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.SendAsync("You've been catched in a StickyNet!");
            session.Disconnect();

            return Task.FromResult(true);
        }
    }
}
