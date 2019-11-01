using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class TelnetProtocol : ITcpProtocol
    {
        public string Name => "Telnet";

        public async Task<bool> PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.SendAsync("Welcome to Telnet! You need to authorize to send commands!");
            await Task.Delay(10000);
            session.Disconnect();

            return true;
        }
    }
}
