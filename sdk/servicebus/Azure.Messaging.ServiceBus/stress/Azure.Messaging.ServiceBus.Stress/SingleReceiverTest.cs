// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Azure.Test.Stress;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Stress.Metrics;
using Azure.Messaging.ServiceBus.Stress.Options;

namespace Azure.Messaging.ServiceBus.Stress
{
    public class SingleReceiverTest : StressTest<ServiceBusStressOptions, SingleReceiverMetrics>
    {
        public SingleReceiverTest(ServiceBusStressOptions options, SingleReceiverMetrics metrics)
            : base(options, metrics)
        {
        }

        public override async Task RunAsync(CancellationToken testDurationToken)
        {
#pragma warning disable AZC0100 // ConfigureAwait(false) must be used.
            await using var client = new ServiceBusClient(Options.ConnectionString, new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions { TryTimeout = TimeSpan.FromSeconds(Options.TryTimeout), MaxRetries = 20 }
            });
            await using var sender = client.CreateSender(Options.QueueName);
            await using var receiver = client.CreateReceiver(Options.QueueName);
#pragma warning restore AZC0100 // ConfigureAwait(false) must be used.

            const int messageCount = 50000;
            var firstRun = true;
            while (!testDurationToken.IsCancellationRequested)
            {
                try
                {
                    if (firstRun)
                    {
                        await SendMessages(sender, messageCount, testDurationToken).ConfigureAwait(false);
                        firstRun = false;
                    }
                    await SendMessages(sender, messageCount, testDurationToken).ConfigureAwait(false);

                    await receiver.ReceiveMessagesAsync(messageCount, cancellationToken: testDurationToken).ConfigureAwait(false);
                    Metrics.IncrementReceives(messageCount);
                }
                catch (Exception e) when (ContainsOperationCanceledException(e) && testDurationToken.IsCancellationRequested)
                {
                    // Ignore this exception as it is normal operation of the test for it to occur.
                }
                catch (Exception e)
                {
                    Metrics.Exceptions.Enqueue(e);
                }
            }
        }

        private async Task SendMessages(ServiceBusSender sender, int messageCount, CancellationToken testDurationToken)
        {
            // This is the max batch size defined by the service itself.
            const int batchChunkSize = 4500;
            var chunkCount = messageCount / batchChunkSize;
            var remainingCount = messageCount % batchChunkSize;

            while (chunkCount > 0)
            {
                var batch = await sender.CreateMessageBatchAsync(testDurationToken).ConfigureAwait(false);
                Enumerable.Range(0, batchChunkSize).ToList().ForEach(_ => batch.TryAddMessage(new ServiceBusMessage()));
                await sender.SendMessagesAsync(batch, testDurationToken).ConfigureAwait(false);
                chunkCount--;
                Metrics.IncrementSends(batchChunkSize);
            }

            if (remainingCount > 0)
            {
                var batch = await sender.CreateMessageBatchAsync(testDurationToken).ConfigureAwait(false);
                Enumerable.Range(0, remainingCount).ToList().ForEach(_ => batch.TryAddMessage(new ServiceBusMessage()));
                await sender.SendMessagesAsync(batch, testDurationToken).ConfigureAwait(false);
                Metrics.IncrementSends(remainingCount);
            }
        }
    }
}
