// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Azure.Test.Stress;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Stress.Metrics;
using Azure.Messaging.ServiceBus.Stress.Options;

namespace Azure.Messaging.ServiceBus.Stress
{
    public class MessageLockRenewalTest : StressTest<LockRenewalOptions, LockRenewalMetrics>
    {
        public MessageLockRenewalTest(LockRenewalOptions options, LockRenewalMetrics metrics)
            : base(options, metrics)
        {
        }

        public override async Task RunAsync(CancellationToken testDurationToken)
        {
#pragma warning disable AZC0100 // ConfigureAwait(false) must be used.
            await using var client = new ServiceBusClient(Options.ConnectionString, new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions { TryTimeout = TimeSpan.FromSeconds(Options.TryTimeout) }
            });
            await using var sender = client.CreateSender(Options.QueueName);
            await using var receiver = client.CreateReceiver(Options.QueueName);
#pragma warning restore AZC0100 // ConfigureAwait(false) must be used.

            var renewDuration = TimeSpan.FromSeconds(Options.RenewDuration);
            while (!testDurationToken.IsCancellationRequested)
            {
                try
                {
                    await sender.SendMessageAsync(new ServiceBusMessage(), testDurationToken).ConfigureAwait(false);
                    Metrics.IncrementSends();

                    var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(Options.ReceiveDuration), testDurationToken).ConfigureAwait(false);
                    if (receivedMessage == null) continue;

                    Metrics.IncrementReceives();

                    var sendStopwatch = Stopwatch.StartNew();
                    while (sendStopwatch.Elapsed < renewDuration)
                    {
                        await receiver.RenewMessageLockAsync(receivedMessage, testDurationToken).ConfigureAwait(false);
                        Metrics.IncrementRenews();

                        await Task.Delay(TimeSpan.FromSeconds(Options.RenewDelay), testDurationToken).ConfigureAwait(false);
                    }

                    await receiver.CompleteMessageAsync(receivedMessage, testDurationToken).ConfigureAwait(false);
                    Metrics.IncrementCompletes();
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
    }
}
