// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Test.Stress;
using CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Messaging.ServiceBus.Stress
{
    public class MessageLockRenewalTest : StressTest<MessageLockRenewalTest.MessageLockRenewalOptions, MessageLockRenewalTest.MessageLockRenewalMetrics>
    {
        public MessageLockRenewalTest(MessageLockRenewalOptions options, MessageLockRenewalMetrics metrics)
            : base(options, metrics)
        {
        }

        private const string ConnectionString = "";
        private const string QueueName = "";

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            var client = new ServiceBusClient(ConnectionString);
            var sender = client.CreateSender(QueueName);
            var senderTask = Sender(sender, cancellationToken);
            var receiverCts = new CancellationTokenSource();

            var receiverTasks = new Task[Options.Receivers];
            for (var i = 0; i < Options.Receivers; i++)
            {
                receiverTasks[i] = Receiver(client, receiverCts.Token);
            }

            try
            {
                await senderTask.ConfigureAwait(false);
            }
            catch (Exception e) when (ContainsOperationCanceledException(e))
            {
            }

            // Block until all messages have been received
            await DelayUntil(() => Metrics.Unprocessed == 0, cancellationToken).ConfigureAwait(false);

            receiverCts.Cancel();

            await Task.WhenAll(receiverTasks).ConfigureAwait(false);
        }

        private async Task Sender(ServiceBusSender sender, CancellationToken cancellationToken)
        {
            var index = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxSendDelayMs)), cancellationToken).ConfigureAwait(false);
                    var message = new ServiceBusMessage($"{index}");
                    await sender.SendMessageAsync(message).ConfigureAwait(false);
                    Interlocked.Increment(ref Metrics.Sends);
                    index++;
                }
                catch (Exception e) when (!ContainsOperationCanceledException(e))
                {
                    Metrics.Exceptions.Enqueue(e);
                }
            }
        }

        private async Task Receiver(ServiceBusClient client, CancellationToken cancellationToken)
        {
            var receiver = client.CreateReceiver(QueueName);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxReceiveDelayMs)), cancellationToken).ConfigureAwait(false);
                    var message = await receiver.ReceiveMessageAsync().ConfigureAwait(false);
                    _ = Convert.ToInt32(message.Body.ToString());
                    Interlocked.Increment(ref Metrics.Receives);
                }
                catch (Exception e) when (!ContainsOperationCanceledException(e))
                {
                    Metrics.Exceptions.Enqueue(e);
                }
            }
        }

        public class MessageLockRenewalOptions : ServiceBusStressOptions
        {
            [Option("maxSendDelayMs", Default = 50, HelpText = "Max send delay (in milliseconds)")]
            public int MaxSendDelayMs { get; set; }

            [Option("maxReceiveDelayMs", Default = 200, HelpText = "Max send delay (in milliseconds)")]
            public int MaxReceiveDelayMs { get; set; }

            [Option("receivers", Default = 3, HelpText = "Number of receivers")]
            public int Receivers { get; set; }
        }

        public class MessageLockRenewalMetrics : StressMetrics
        {
            public long Sends;
            public long Receives;
            public long Unprocessed => (Interlocked.Read(ref Sends) - Interlocked.Read(ref Receives));
        }
    }
}
