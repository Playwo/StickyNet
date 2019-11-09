using System.Timers;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public abstract class StickyTcpSession : TcpSession
    {
        public Timer TimeoutTimer;

        public StickyTcpSession(TcpServer server, int timeout)
            : base(server)
        {
            TimeoutTimer = new Timer(timeout)
            {
                AutoReset = true,
            };
            TimeoutTimer.Elapsed += (s, a) => { OnTimeouted(); Disconnect(); };
            TimeoutTimer.Start();
        }

        public void ResetTimeout()
        {
            TimeoutTimer.Stop();
            TimeoutTimer.Start();
        }

        protected virtual void OnTimeouted()
        {
        }
    }
}
