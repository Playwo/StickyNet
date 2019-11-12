using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StickyNet.Report;

namespace StickyNet.Service
{
    public class StickyGlobalConfig
    {
        [JsonProperty("TripLinks")]
        private List<TripLink> TripLinkServers { get; set; } = new List<TripLink>();

        [JsonIgnore]
        public IReadOnlyList<TripLink> TripLinks => TripLinkServers.AsReadOnly();

        [JsonIgnore]
        public bool EnableReporting => TripLinkServers.Count > 0;

        [JsonIgnore]
        public bool IsValid => TripLinkServers != null;

        public StickyGlobalConfig()
        {
        }

        public void AddTripLink(Uri server, string token)
        {
            var tripLink = new TripLink(server, token);
            TripLinkServers.Add(tripLink);
        }

        public void RemoveTripLink(Uri server) 
            => TripLinkServers.RemoveAll(x => x.Server == server);
    }
}
