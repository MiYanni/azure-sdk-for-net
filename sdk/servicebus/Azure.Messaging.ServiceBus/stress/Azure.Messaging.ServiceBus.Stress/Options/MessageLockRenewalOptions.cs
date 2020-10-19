// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class MessageLockRenewalOptions : ServiceBusStressOptions
    {
        [Option("maxSendDelayMs", Default = 50, HelpText = "Max send delay (in milliseconds)")]
        public int MaxSendDelayMs { get; set; }

        [Option("maxReceiveDelayMs", Default = 200, HelpText = "Max send delay (in milliseconds)")]
        public int MaxReceiveDelayMs { get; set; }

        [Option("receivers", Default = 3, HelpText = "Number of receivers")]
        public int Receivers { get; set; }

        [Option("receivePoll", Default = 3, HelpText = "Number of seconds to poll for receivers no longer gaining new messages")]
        public int ReceivePollForCompletion { get; set; }
    }
}
