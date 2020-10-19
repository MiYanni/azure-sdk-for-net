// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;
using Azure.Test.Stress;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Stress.Metrics;
using Azure.Messaging.ServiceBus.Stress.Options;

namespace Azure.Messaging.ServiceBus.Stress
{
    public class MessageLockRenewalTest : StressTest<MessageLockRenewalOptions, MessageLockRenewalMetrics>
    {
        public MessageLockRenewalTest(MessageLockRenewalOptions options, MessageLockRenewalMetrics metrics)
            : base(options, metrics)
        {
        }

        public override async Task RunAsync(CancellationToken testDurationToken)
        {
            var client = new ServiceBusClient(Options.ConnectionString);
            var sender = StartSender(client.CreateSender(Options.QueueName), testDurationToken);

            var receiverTokenSource = new CancellationTokenSource();
            var receivers = Enumerable.Range(0, Options.Receivers)
                .Select(_ => StartReceiver(client, receiverTokenSource.Token)).ToArray();

            try
            {
                await sender.ConfigureAwait(false);
            }
            catch (Exception e) when (ContainsOperationCanceledException(e))
            {
            }

            await DelayUntil(() => !Metrics.HasReceivesIncremented(), TimeSpan.FromSeconds(Options.ReceivePollForCompletion)).ConfigureAwait(false);
            receiverTokenSource.Cancel();
            await Task.WhenAll(receivers).ConfigureAwait(false);
        }

        private async Task StartSender(ServiceBusSender sender, CancellationToken testDurationToken)
        {
            while (!testDurationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxSendDelayMs)), testDurationToken).ConfigureAwait(false);
                    var message = new ServiceBusMessage($"{Metrics.Sends}");
                    await sender.SendMessageAsync(message).ConfigureAwait(false);
                    Metrics.IncrementSends();
                }
                catch (Exception e) when (!ContainsOperationCanceledException(e))
                {
                    Metrics.Exceptions.Enqueue(e);
                }
            }
        }

        private async Task StartReceiver(ServiceBusClient client, CancellationToken shutdownReceiversToken)
        {
            var receiver = client.CreateReceiver(Options.QueueName);
            while (!shutdownReceiversToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxReceiveDelayMs)), shutdownReceiversToken).ConfigureAwait(false);
                    var message = await receiver.ReceiveMessageAsync().ConfigureAwait(false);
                    _ = Convert.ToInt64(message.Body.ToString(), CultureInfo.InvariantCulture);
                    Metrics.IncrementReceives();
                }
                catch (Exception e) when (!ContainsOperationCanceledException(e))
                {
                    Metrics.Exceptions.Enqueue(e);
                }
            }
        }
    }
}
