using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class TelnetSession : TcpSession
    {
        public int RemainingTries = 4;

        public TelnetSession(TcpServer server) 
            : base(server)
        {
        }

        protected override void OnConnected() => SendAsync("Welcome to Telnet! Please authenticate!\n");
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (RemainingTries > 0)
            {
                SendAsync("Invalid password!\n");

                if (size > 100)
                {
                    SendAsync("Exceeded maximum!");
                    Disconnect();
                }

                RemainingTries--;
            }

            SendAsync("Out of authentication tries!\n");
            Disconnect();
        }
    }
}
