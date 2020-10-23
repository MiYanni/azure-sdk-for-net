// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class SingleSenderParallelOptions : ServiceBusStressOptions
    {
        [Option("sendDuration", Default = 10, HelpText = "Duration to continually send messages in seconds")]
        public int SendDuration { get; set; }

        [Option("sendDelay", Default = 1, HelpText = "Delay in-between sending messages in seconds")]
        public int SendDelay { get; set; }

        [Option("receiveDuration", Default = 3, HelpText = "Duration for the receiver to no longer gain new messages in seconds")]
        public int ReceiveDuration { get; set; }

        [Option("parallelOperations", Default = 6, HelpText = "Number of parallel send operations")]
        public int ParallelOperations { get; set; }
    }
}
