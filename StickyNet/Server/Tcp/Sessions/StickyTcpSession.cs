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
                AutoReset = false,
            };
            TimeoutTimer.Elapsed += PerformTimeout;
            TimeoutTimer.Start();
        }

        public void ResetTimeout()
        {
            TimeoutTimer.Stop();
            TimeoutTimer.Start();
        }

        private void PerformTimeout(object s, ElapsedEventArgs e)
        {
            try
            {
                OnTimeouted();
            }
            finally
            {
                Disconnect();
                TimeoutTimer.Dispose();
            }
        }

        protected override void OnDisconnected() 
            => TimeoutTimer.Dispose();

        protected virtual void OnTimeouted()
        {
        }
    }
}
