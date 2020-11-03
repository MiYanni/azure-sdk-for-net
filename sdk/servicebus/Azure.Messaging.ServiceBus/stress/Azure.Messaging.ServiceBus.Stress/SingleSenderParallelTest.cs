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
    public class SingleSenderParallelTest : StressTest<SingleSenderParallelOptions, SingleSenderMetrics>
    {
        public SingleSenderParallelTest(SingleSenderParallelOptions options, SingleSenderMetrics metrics)
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
#pragma warning restore AZC0100 // ConfigureAwait(false) must be used.

            var sendDuration = TimeSpan.FromSeconds(Options.SendDuration);
            while (!testDurationToken.IsCancellationRequested)
            {
                try
                {
                    var senders = Enumerable.Range(0, Options.ParallelOperations)
                        .Select(_ => SendMessagesAsync(sender, sendDuration, testDurationToken)).ToArray();

                    await Task.WhenAll(senders).ConfigureAwait(false);

                    await ReceiveAndDeleteAllMessagesAsync(client, testDurationToken).ConfigureAwait(false);
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

        private async Task SendMessagesAsync(ServiceBusSender sender, TimeSpan sendDuration, CancellationToken testDurationToken)
        {
            var sendStopwatch = Stopwatch.StartNew();
            while (sendStopwatch.Elapsed < sendDuration)
            {
                await sender.SendMessageAsync(new ServiceBusMessage(), testDurationToken).ConfigureAwait(false);
                Metrics.IncrementSends();

                await Task.Delay(TimeSpan.FromSeconds(Options.SendDelay), testDurationToken).ConfigureAwait(false);
            }
        }

        private async Task ReceiveAndDeleteAllMessagesAsync(ServiceBusClient client, CancellationToken testDurationToken)
        {
#pragma warning disable AZC0100 // ConfigureAwait(false) must be used.
            await using var processor = client.CreateProcessor(Options.QueueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 10,
                ReceiveMode = ReceiveMode.ReceiveAndDelete
            });
#pragma warning restore AZC0100 // ConfigureAwait(false) must be used.
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                try
                {
                    var receivedMessage = args.Message;
                    if (receivedMessage == null) return Task.CompletedTask;

                    Metrics.IncrementReceives();
                }
                catch (Exception e) when (ContainsOperationCanceledException(e) && testDurationToken.IsCancellationRequested)
                {
                    // Ignore this exception as it is normal operation of the test for it to occur.
                }
                catch (Exception e)
                {
                    Metrics.Exceptions.Enqueue(e);
                }

                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                if (!(ContainsOperationCanceledException(args.Exception) && testDurationToken.IsCancellationRequested))
                {
                    Metrics.Exceptions.Enqueue(args.Exception);
                }

                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync(testDurationToken).ConfigureAwait(false);

            await DelayUntil(() => !Metrics.IsActivelyReceiving(), TimeSpan.FromSeconds(Options.ReceiveDuration), testDurationToken).ConfigureAwait(false);

            await processor.StopProcessingAsync(testDurationToken).ConfigureAwait(false);
        }
    }
}
