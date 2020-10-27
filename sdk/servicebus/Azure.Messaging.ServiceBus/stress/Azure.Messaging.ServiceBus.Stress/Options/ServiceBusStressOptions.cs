// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Test.Stress;
using CommandLine;

namespace Azure.Messaging.ServiceBus.Stress.Options
{
    public class ServiceBusStressOptions : StressOptions
    {
        [Option("connectionString", Required = true, HelpText = "Service Bus namespace connection string")]
        public string ConnectionString { get; set; }

        [Option("queueName", Required = true, HelpText = "Queue name in a Service Bus namespace")]
        public string QueueName { get; set; }

        [Option("tryTimeout", Default = 10, HelpText = "TryTimeout value for the client in seconds")]
        public int TryTimeout { get; set; }

        [Option("maxRetries", Default = 10, HelpText = "MaxRetries value for the client")]
        public int MaxRetries { get; set; }
    }
}
