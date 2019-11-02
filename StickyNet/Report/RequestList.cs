using System;
using System.Collections.Generic;
using System.Linq;
using StickyNet.Server;

namespace StickyNet.Report
{
    public class RequestList
    {
        public List<DateTime> ConnectionTimes { get; }

        public int TotalRequests => ConnectionTimes.Count;

        public TimeSpan RequestSpanTime => ConnectionTimes.OrderByDescending(x => x.Ticks).First() - ConnectionTimes.OrderBy(x => x.Ticks).First();

        public double AverageRequestPerMinute => TotalRequests / RequestSpanTime.TotalMinutes;

        public double MaximumRequestsPerMinute
        {
            get {
                double highestCountPerMinute = 0;

                foreach(var connectionTime in ConnectionTimes)
                {
                    int requestsInRange = 0;

                    foreach(var otherConnectionTime in ConnectionTimes)
                    {
                        var timeDiff = otherConnectionTime - connectionTime;

                        if (timeDiff.TotalSeconds <= 60 && timeDiff.TotalSeconds >= 0)
                        {
                            requestsInRange++;
                        }
                    }
                    
                    if (requestsInRange > highestCountPerMinute)
                    {
                        highestCountPerMinute = requestsInRange;
                    }
                }

                return highestCountPerMinute;
            }
        }

        public RequestList()
        {
            ConnectionTimes = new List<DateTime>();
        }

        public RequestList AddConnection(ConnectionAttempt attempt)
        {
            ConnectionTimes.Add(attempt.Time);
            return this;
        }

        public Reason CalculateReason() 
            => MaximumRequestsPerMinute > 100 
                ? Reason.Hacker 
                : TotalRequests > 6 
                    ? Reason.Spammer 
                    : Reason.Scanner;

    }
}
