using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Listener.Servers
{
    public class TcpProtocol
    {
        public virtual string Name => "None"; 

        public virtual Task<bool> PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.Send("Intruder Alert! You got stuck in a StickyNet!");
            session.Disconnect();
            return Task.FromResult(true);
        }
    }
}
