// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using Azure.Test.Stress;

namespace Azure.Messaging.ServiceBus.Stress.Metrics
{
    public class LockRenewalMetrics : StressMetrics
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

        private long _renews;
        public long Renews
        {
            get => Interlocked.Read(ref _renews);
            set => Interlocked.Exchange(ref _renews, value);
        }

        public long IncrementRenews() => Interlocked.Increment(ref _renews);

        private long _completes;
        public long Completes
        {
            get => Interlocked.Read(ref _completes);
            set => Interlocked.Exchange(ref _completes, value);
        }

        public long IncrementCompletes() => Interlocked.Increment(ref _completes);
    }
}
