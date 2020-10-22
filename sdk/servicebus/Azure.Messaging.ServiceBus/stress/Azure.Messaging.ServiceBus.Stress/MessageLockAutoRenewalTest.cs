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
    public class MessageLockAutoRenewalTest : StressTest<LockAutoRenewalOptions, LockAutoRenewalMetrics>
    {
        public MessageLockAutoRenewalTest(LockAutoRenewalOptions options, LockAutoRenewalMetrics metrics)
            : base(options, metrics)
        {
        }

        public override async Task RunAsync(CancellationToken testDurationToken)
        {
#pragma warning disable AZC0100 // ConfigureAwait(false) must be used.
            await using var client = new ServiceBusClient(Options.ConnectionString);
            await using var sender = client.CreateSender(Options.QueueName);

            while (!testDurationToken.IsCancellationRequested)
            {
                try
                {
                    await using var processor = client.CreateProcessor(Options.QueueName,
                        new ServiceBusProcessorOptions
                        {
                            MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(Options.RenewDuration)
                        });
#pragma warning restore AZC0100 // ConfigureAwait(false) must be used.
                    var messageCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    processor.ProcessMessageAsync += MessageHandler;
                    processor.ProcessErrorAsync += ErrorHandler;

                    async Task MessageHandler(ProcessMessageEventArgs args)
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Options.RenewDuration - Options.ReceiveDuration), testDurationToken).ConfigureAwait(false);
                            var receivedMessage = args.Message;
                            if (receivedMessage == null) return;

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
                        finally
                        {
                            messageCompletionSource.SetResult(true);
                        }
                    }

                    Task ErrorHandler(ProcessErrorEventArgs args)
                    {
                        if (!(ContainsOperationCanceledException(args.Exception) && testDurationToken.IsCancellationRequested))
                        {
                            Metrics.Exceptions.Enqueue(args.Exception);
                        }

                        return Task.CompletedTask;
                    }

                    await sender.SendMessageAsync(new ServiceBusMessage(), testDurationToken).ConfigureAwait(false);
                    Metrics.IncrementSends();

                    await processor.StartProcessingAsync(testDurationToken).ConfigureAwait(false);

                    await messageCompletionSource.Task.ConfigureAwait(false);

                    await processor.StopProcessingAsync(testDurationToken).ConfigureAwait(false);
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
