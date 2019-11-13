using System.Text;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class TelnetSession : StickyTcpSession
    {
        public int RemainingTries = 4;

        public TelnetSession(TcpServer server, int timeout)
            : base(server, timeout)
        {
        }

        protected override void OnConnected() => SendAsync("Welcome to Telnet! Please authenticate!\r\r\n");
        protected override void OnTimeouted() => SendAsync("Timeouted...\r\r\n");
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            ResetTimeout();

            string message = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);

            if (RemainingTries > 0)
            {
                ProcessReceived(message);
            }
            if (RemainingTries == 0)
            {
                SendAsync("Out of authentication tries!\r\r\n");
                Disconnect();
            }
        }

        private void ProcessReceived(string received)
        {
            if (received.Contains("\r\r\n"))
            {
                SendAsync("Invalid password!\r\r\n");
                RemainingTries--;

                if (received.Length > 100)
                {
                    SendAsync("Exceeded maximum!\r\r\n");
                    Disconnect();
                }
            }
        }
    }
}
