// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class MessageLockRenewalOptions : ServiceBusStressOptions
    {
        [Option("receiveDuration", Default = 1, HelpText = "Receive duration in seconds")]
        public int ReceiveDuration { get; set; }

        [Option("renewInterval", Default = 10, HelpText = "Interval to renew lock in seconds")]
        public int RenewInterval { get; set; }

        [Option("renewCount", Default = 10, HelpText = "Number of times to renew the same message")]
        public int RenewCount { get; set; }
    }
}
