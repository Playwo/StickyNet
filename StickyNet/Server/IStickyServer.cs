using System.Net;
using System.Threading.Channels;
using StickyNet.Service;

namespace StickyNet.Server
{
    public interface IStickyServer
    {
        public EndPoint EndPoint { get; }
        public int Port { get; }
        public StickyServerConfig Config { get; }

        public bool Start();
        public bool Stop();

        public void Dispose();
    }
}
