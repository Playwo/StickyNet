using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class TelnetProtocol : ITcpProtocol
    {
        public string Name => "Telnet";

        public async Task PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.SendAsync("Welcome to Telnet! You need to authorize to send commands!");

            for(int i = 0; i < 4; i++)
            {
                string response = await session.ReceiveAsync(10000);

                if (response == null)
                {
                    session.Send("Timeout...");
                    session.Disconnect();
                    return;
                }

                if (response.Length > 100)
                {
                    session.Send("Exceeded maximum password lenght!");
                    session.Disconnect();
                    return;
                }

                session.Send("Invalid password!");
            }

            session.Send("Out of authentication tries!");
            session.Disconnect();
        }
    }
}
