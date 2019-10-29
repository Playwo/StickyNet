using System;
using System.Net;

namespace StickyNet.Server
{
    public class ConnectionAttempt
    {
        public string Ip { get; }
        public DateTime Time { get; }
        public int Port { get; }

        public ConnectionAttempt(IPAddress ip, DateTime time, int port)
        {
            Ip = ip.ToString();
            Time = time;
            Port = port;
        }
    }
}
