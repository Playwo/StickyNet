using System.Net;
using System.Threading.Tasks;

namespace StickyNet.Server.Udp
{
    public interface IUdpProtocol
    {
        public string Name { get; }
        public Task PerformHandshakeAsync(EndPoint endPoint);
    }
}
