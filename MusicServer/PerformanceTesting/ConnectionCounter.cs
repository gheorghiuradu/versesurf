﻿using System;

namespace MusicServer.PerformanceTesting
{
    public class ConnectionCounter
    {
        private int _totalConnectedCount;
        private int _peakConnectedCount;
        private int _totalDisconnectedCount;
        private int _receivedCount;

        private readonly object _lock = new object();

        public ConnectionSummary Summary
        {
            get
            {
                lock (_lock)
                {
                    return new ConnectionSummary
                    {
                        CurrentConnections = _totalConnectedCount - _totalDisconnectedCount,
                        PeakConnections = _peakConnectedCount,
                        TotalConnected = _totalConnectedCount,
                        TotalDisconnected = _totalDisconnectedCount,
                        ReceivedCount = _receivedCount
                    };
                }
            }
        }

        public void Receive(string payload)
        {
            lock (_lock)
            {
                _receivedCount += payload.Length;
            }
        }

        public void Connected()
        {
            lock (_lock)
            {
                _totalConnectedCount++;
                _peakConnectedCount = Math.Max(_totalConnectedCount - _totalDisconnectedCount, _peakConnectedCount);
            }
        }

        public void Disconnected()
        {
            lock (_lock)
            {
                _totalDisconnectedCount++;
            }
        }
    }
}