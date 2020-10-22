// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using Azure.Test.Stress;

namespace Azure.Messaging.ServiceBus.Stress.Metrics
{
    public class SingleSenderMetrics : StressMetrics
    {
        private long _sends;
        public long Sends
        {
            get => Interlocked.Read(ref _sends);
            set => Interlocked.Exchange(ref _sends, value);
        }

        public long IncrementSends() => Interlocked.Increment(ref _sends);

        private long _receives;
        public long Receives
        {
            get => Interlocked.Read(ref _receives);
            set => Interlocked.Exchange(ref _receives, value);
        }

        public long IncrementReceives() => Interlocked.Increment(ref _receives);

        private long _previousReceives = long.MinValue;
        public bool IsActivelyReceiving()
        {
            if (Interlocked.Read(ref _previousReceives) == Receives)
            {
                // Reset receiving activity
                Interlocked.Exchange(ref _previousReceives, long.MinValue);
                return false;
            }

            Interlocked.Exchange(ref _previousReceives, Receives);
            return true;
        }
    }
}
