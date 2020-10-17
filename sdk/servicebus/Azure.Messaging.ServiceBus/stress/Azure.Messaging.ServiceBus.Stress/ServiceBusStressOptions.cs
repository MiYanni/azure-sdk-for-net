// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Test.Stress;
using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress
{
    public class ServiceBusStressOptions : StressOptions
    {
        [Option("connectionString", HelpText = "Service Bus namespace connection string")]
        public string ConnectionString { get; set; }

        [Option("queueName", HelpText = "Queue name in a Service Bus namespace")]
        public string QueueName { get; set; }
    }
}
