// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Test.Stress;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Stress.Metrics;
using Azure.Messaging.ServiceBus.Stress.Options;

namespace Azure.Messaging.ServiceBus.Stress
{
    public class SessionLockRenewalTest : StressTest<MessageLockRenewalOptions, MessageLockRenewalMetrics>
    {
        public SessionLockRenewalTest(MessageLockRenewalOptions options, MessageLockRenewalMetrics metrics)
            : base(options, metrics)
        {
        }

        public override async Task RunAsync(CancellationToken testDurationToken)
        {
            var client = new ServiceBusClient(Options.ConnectionString);
            await using (client.ConfigureAwait(false))
            {
                var sender = client.CreateSender(Options.QueueName);
                ServiceBusSessionReceiver receiver = null;

                while (!testDurationToken.IsCancellationRequested)
                {
                    try
                    {
                        await sender.SendMessageAsync(new ServiceBusMessage { SessionId = "0" }, testDurationToken).ConfigureAwait(false);
                        Metrics.IncrementSends();

                        receiver ??= await client.AcceptNextSessionAsync(Options.QueueName, null, testDurationToken).ConfigureAwait(false);
                        var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(Options.ReceiveDuration), testDurationToken).ConfigureAwait(false);
                        if (receivedMessage == null) continue;

                        Metrics.IncrementReceives();
                        for (var i = 0; i < Options.RenewCount; ++i)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Options.RenewInterval), testDurationToken).ConfigureAwait(false);
                            await receiver.RenewSessionLockAsync(testDurationToken).ConfigureAwait(false);
                            Metrics.IncrementRenews();
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
}
