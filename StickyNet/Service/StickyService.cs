using System.Threading.Tasks;

namespace StickyNet.Service
{
    public abstract class StickyService
    {
        public virtual Task InitializeAsync() => Task.CompletedTask;
    }
}
