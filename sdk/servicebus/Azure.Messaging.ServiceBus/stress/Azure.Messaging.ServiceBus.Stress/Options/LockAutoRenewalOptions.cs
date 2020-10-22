// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class LockAutoRenewalOptions : ServiceBusStressOptions
    {
        [Option("renewDuration", Default = 120, HelpText = "Duration to auto-renew the same message in seconds")]
        public int RenewDuration { get; set; }

        [Option("receiveDuration", Default = 10, HelpText = "Duration prior to auto-renew expiry to receive the message in seconds")]
        public int ReceiveDuration { get; set; }
    }
}
