using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public interface IProtocol
    {
        public string Name { get; }
    }
}
