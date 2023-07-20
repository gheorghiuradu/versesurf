﻿namespace MusicServer.PerformanceTesting
{
    public class ConnectionSummary
    {
        public int TotalConnected { get; set; }

        public int TotalDisconnected { get; set; }

        public int PeakConnections { get; set; }

        public int CurrentConnections { get; set; }

        public int ReceivedCount { get; set; }
    }
}