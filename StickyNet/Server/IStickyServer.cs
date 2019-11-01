using System.Collections.Concurrent;
using System.Net;
using StickyNet.Report;

namespace StickyNet.Server
{
    public interface IStickyServer
    {
        public EndPoint EndPoint { get; }
        public int Port { get; }
        public ConcurrentDictionary<IPAddress, RequestList> ConnectionAttempts { get; }
        public StickyServerConfig Config { get; }

        public bool Start();
        public bool Stop();
    }
}
