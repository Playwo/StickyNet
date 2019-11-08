using System;

namespace StickyNet.Server
{
    public class ConnectionAttempt
    {
        public DateTimeOffset Time { get; }
        public int Port { get; }

        public ConnectionAttempt(DateTimeOffset time, int port)
        {
            Time = time;
            Port = port;
        }
    }
}
