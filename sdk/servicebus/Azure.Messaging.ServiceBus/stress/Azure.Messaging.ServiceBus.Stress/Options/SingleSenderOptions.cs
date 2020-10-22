// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class SingleSenderOptions : ServiceBusStressOptions
    {
        [Option("sendDuration", Default = 10, HelpText = "Send duration in seconds")]
        public int SendDuration { get; set; }

        [Option("sendInterval", Default = 1, HelpText = "Interval to send messages in seconds")]
        public int SendInterval { get; set; }

        [Option("receivePoll", Default = 3, HelpText = "Number of seconds to poll for receivers no longer gaining new messages")]
        public int ReceivePollForCompletion { get; set; }
    }
}
