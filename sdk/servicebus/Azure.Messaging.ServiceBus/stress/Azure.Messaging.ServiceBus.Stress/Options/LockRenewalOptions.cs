// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class LockRenewalOptions : ServiceBusStressOptions
    {
        [Option("renewDuration", Default = 120, HelpText = "Duration to renew the same message in seconds")]
        public int RenewDuration { get; set; }

        [Option("renewDelay", Default = 10, HelpText = "Delay in renewing the lock in seconds")]
        public int RenewDelay { get; set; }

        [Option("receiveDuration", Default = 1, HelpText = "Receive duration in seconds")]
        public int ReceiveDuration { get; set; }
    }
}
