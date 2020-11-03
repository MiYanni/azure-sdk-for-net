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
                RetryOptions = new ServiceBusRetryOptions { TryTimeout = TimeSpan.FromSeconds(Options.TryTimeout), MaxRetries = Options.MaxRetries }
            });
            await using var sender = client.CreateSender(Options.QueueName);
            await using var receiver = client.CreateReceiver(Options.QueueName, new ServiceBusReceiverOptions
            {
                ReceiveMode = ReceiveMode.ReceiveAndDelete
            });
#pragma warning restore AZC0100 // ConfigureAwait(false) must be used.

            const int messageCount = 50000;
            var firstRun = true;
            while (!testDurationToken.IsCancellationRequested)
            {
                try
                {
                    if (firstRun)
                    {
                        await SendMessagesAsync(sender, messageCount, testDurationToken).ConfigureAwait(false);
                        firstRun = false;
                    }
                    await SendMessagesAsync(sender, messageCount, testDurationToken).ConfigureAwait(false);

                    await ReceiveMessagesAsync(receiver, messageCount, testDurationToken).ConfigureAwait(false);
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

        private async Task ReceiveMessagesAsync(ServiceBusReceiver receiver, int messageCount, CancellationToken testDurationToken)
        {
            var currentMaxCount = messageCount;
            while (currentMaxCount != 0)
            {
                var messages = await receiver.ReceiveMessagesAsync(currentMaxCount, cancellationToken: testDurationToken).ConfigureAwait(false);
                currentMaxCount -= messages.Count;
                Metrics.IncrementReceives(messages.Count);
            }
        }

        private async Task SendMessagesAsync(ServiceBusSender sender, int messageCount, CancellationToken testDurationToken)
        {
            // This is the max batch size defined by the service itself.
            const int batchChunkSize = 4500;
            var chunkCount = messageCount / batchChunkSize;
            var remainingCount = messageCount % batchChunkSize;

            while (chunkCount > 0)
            {
                await SendBatchAsync(batchChunkSize).ConfigureAwait(false);
                chunkCount--;
            }

            if (remainingCount > 0)
            {
                await SendBatchAsync(remainingCount).ConfigureAwait(false);
            }

            async Task SendBatchAsync(int batchCount)
            {
                var batch = await sender.CreateMessageBatchAsync(testDurationToken).ConfigureAwait(false);
                Enumerable.Range(0, batchCount).ToList().ForEach(_ => batch.TryAddMessage(new ServiceBusMessage()));
                await sender.SendMessagesAsync(batch, testDurationToken).ConfigureAwait(false);
                Metrics.IncrementSends(batchCount);
            }
        }
    }
}
