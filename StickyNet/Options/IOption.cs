using Microsoft.Extensions.Logging;

namespace StickyNet.Options
{
    public interface IOption
    {
        public LogLevel LogLevel { get; set; }
    }
}
