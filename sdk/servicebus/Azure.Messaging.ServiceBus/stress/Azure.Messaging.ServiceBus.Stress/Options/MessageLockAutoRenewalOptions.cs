// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class MessageLockAutoRenewalOptions : ServiceBusStressOptions
    {
        [Option("renewDuration", Default = 300, HelpText = "Duration to auto-renew the message in seconds")]
        public int RenewDuration { get; set; }

        [Option("priorToExpiry", Default = 10, HelpText = "Duration prior to auto-renew expiry to retrieve the message in seconds")]
        public int PriorToExpiry { get; set; }
    }
}
