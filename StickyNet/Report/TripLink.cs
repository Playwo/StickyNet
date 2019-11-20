using System;

namespace StickyNet.Report
{
    public class TripLink
    {
        public Uri Server { get; set; }
        public string Token { get; private set; }

        public TripLink(Uri server, string token)
        {
            Server = server;
            Token = token;
        }
    }
}
