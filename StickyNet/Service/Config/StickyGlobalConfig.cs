using System;
using System.Text.Json.Serialization;

namespace StickyNet.Service
{
    public class StickyGlobalConfig
    {
        public Uri ReportServer { get; set; } = null;
        public string ReportToken { get; set; } = null;

        [JsonIgnore]
        public bool EnableReporting => ReportServer != null;

        public override bool Equals(object obj) => base.Equals(obj);

        public bool Equals(StickyGlobalConfig config)
            => config.ReportServer.Equals(ReportServer) &&
               config.ReportToken.Equals(ReportToken);
        public override int GetHashCode()
            => HashCode.Combine(ReportServer, ReportToken);
    }
}
