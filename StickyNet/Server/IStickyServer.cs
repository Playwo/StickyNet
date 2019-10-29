using System.Net;

namespace StickyNet.Listener.Server
{
    public interface IStickyServer
    {
        public EndPoint EndPoint { get; }
        public int Port { get; }

        public bool Start();
        public bool Stop();
    }
}
