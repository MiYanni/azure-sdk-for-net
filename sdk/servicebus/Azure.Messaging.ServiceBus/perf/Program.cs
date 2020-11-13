// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using T1ReceiveMode = Microsoft.Azure.ServiceBus.ReceiveMode;

namespace Azure.Messaging.ServiceBus.Stress
{
    internal class Program
    {
        private static async Task Main()
        {
            var connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING");
            var queueName = Environment.GetEnvironmentVariable("SERVICEBUS_QUEUE_NAME");
            //var topicName = Environment.GetEnvironmentVariable("SERVICEBUS_TOPIC_NAME");
            //var topicSubscriptionName = Environment.GetEnvironmentVariable("SERVICEBUS_TOPIC_SUBSCRIPTION_NAME");

            //var track2Client =  new ServiceBusClient(connectionString);

            //await Track2Send1MMessages(track2Client, queueName).ConfigureAwait(false);
            //await Track2Scenario1Processor(track2Client, queueName, ReceiveMode.ReceiveAndDelete, 10).ConfigureAwait(false);
            //await Track2Scenario1Receiver(track2Client, queueName, ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);

            //await Track2Scenario1LongRunProcessor(track2Client, queueName, ReceiveMode.PeekLock).ConfigureAwait(false);
            //await Track2Scenario1PayloadProcessor(track2Client, queueName, ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);

            //await Track2Send1MMessagesTopic(track2Client, topicName).ConfigureAwait(false);
            //await Track2Scenario1TopicProcessor(track2Client, topicName, topicSubscriptionName, ReceiveMode.PeekLock).ConfigureAwait(false);

            //await Track2Scenario2Sender(track2Client, queueName).ConfigureAwait(false);
            //await Track2Scenario2SenderPayload(track2Client, queueName).ConfigureAwait(false);

            ////////////////////////////////////////////////////////////////////////////////////

            //await Track1Send1MMessages(connectionString, queueName).ConfigureAwait(false);
            //await Track1Scenario1Processor(connectionString, queueName, T1ReceiveMode.ReceiveAndDelete, 10).ConfigureAwait(false);
            //await Track1Scenario1Receiver(connectionString, queueName, T1ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);

            //await Track1Scenario1LongRunProcessor(connectionString, queueName, T1ReceiveMode.PeekLock).ConfigureAwait(false);
            await Track1Scenario1PayloadProcessor(connectionString, queueName, T1ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);

            //await Track1Send1MMessagesTopic(connectionString, topicName).ConfigureAwait(false);
            //await Track1Scenario1TopicProcessor(connectionString, topicName, topicSubscriptionName, T1ReceiveMode.PeekLock).ConfigureAwait(false);

            //await Track1Scenario2Sender(connectionString, queueName).ConfigureAwait(false);
            //await Track1Scenario2SenderPayload(connectionString, queueName).ConfigureAwait(false);
        }

        private static async Task Track2Send1MMessages(ServiceBusClient track2Client, string queueName)
        {
            var sender = track2Client.CreateSender(queueName);
            foreach (var _ in Enumerable.Range(0, 100))
            {
                await sender.SendMessagesAsync(Enumerable.Range(0, 10000).Select(i => new ServiceBusMessage())).ConfigureAwait(false);
            }
        }

        private static async Task Track2SendBulkPayloadMessages(ServiceBusClient track2Client, string queueName)
        {
            var sender = track2Client.CreateSender(queueName);
            foreach (var _ in Enumerable.Range(0, 50))
            {
                await sender.SendMessagesAsync(Enumerable.Range(0, 10000).Select(i =>
                    new ServiceBusMessage(Enumerable.Repeat((byte)1, 20).ToArray()))).ConfigureAwait(false);
            }
        }

        private static async Task Track2Send1MMessagesTopic(ServiceBusClient track2Client, string topicName)
        {
            var sender = track2Client.CreateSender(topicName);
            foreach (var _ in Enumerable.Range(0, 100))
            {
                await sender.SendMessagesAsync(Enumerable.Range(0, 10000).Select(i => new ServiceBusMessage())).ConfigureAwait(false);
            }
        }

        private static async Task Track2Scenario1Processor(ServiceBusClient track2Client, string queueName, ReceiveMode mode, int prefetch = 0)
        {
            var processor = track2Client.CreateProcessor(queueName, new ServiceBusProcessorOptions { AutoComplete = true, ReceiveMode = mode, MaxConcurrentCalls = 32, PrefetchCount = prefetch });
            long receives = 0;
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                var receivedMessage = args.Message;
                if (receivedMessage == null) return Task.CompletedTask;

                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync().ConfigureAwait(false);

            var loopCount = 0;
            while (Interlocked.Read(ref receives) < 1000000)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                loopCount++;

                var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                Console.WriteLine($"Receives per second: {receivesPerSecond}");
            }

            await processor.StopProcessingAsync().ConfigureAwait(false);
        }

        private static async Task Track2Scenario1TopicProcessor(ServiceBusClient track2Client, string topicName, string subscriptionName, ReceiveMode mode)
        {
            var processor = track2Client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions { AutoComplete = true, ReceiveMode = mode, MaxConcurrentCalls = 32 });
            long receives = 0;
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                var receivedMessage = args.Message;
                if (receivedMessage == null)
                    return Task.CompletedTask;

                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync().ConfigureAwait(false);

            var loopCount = 0;
            while (Interlocked.Read(ref receives) < 1000000)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                loopCount++;

                var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                Console.WriteLine($"Receives per second: {receivesPerSecond}");
            }

            await processor.StopProcessingAsync().ConfigureAwait(false);
        }

        private static async Task Track2Scenario1LongRunProcessor(ServiceBusClient track2Client, string queueName, ReceiveMode mode, int prefetch = 0)
        {
            var processor = track2Client.CreateProcessor(queueName, new ServiceBusProcessorOptions { AutoComplete = true, ReceiveMode = mode, MaxConcurrentCalls = 32, PrefetchCount = prefetch });
            long receives = 0;
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                return Task.CompletedTask;
            }

            var loopCount = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await processor.StartProcessingAsync().ConfigureAwait(false);

            while (stopwatch.Elapsed < TimeSpan.FromDays(2.5))
            {
                await Track2SendBulkPayloadMessages(track2Client, queueName).ConfigureAwait(false);

                while (Interlocked.Read(ref receives) < 500000)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                    loopCount++;

                    var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                    Console.WriteLine($"Receives per second: {receivesPerSecond}");
                }
            }

            await processor.StopProcessingAsync().ConfigureAwait(false);
        }

        private static async Task Track2Scenario1PayloadProcessor(ServiceBusClient track2Client, string queueName, ReceiveMode mode, int prefetch = 0)
        {
            var processor = track2Client.CreateProcessor(queueName, new ServiceBusProcessorOptions { AutoComplete = true, ReceiveMode = mode, MaxConcurrentCalls = 32, PrefetchCount = prefetch });
            long receives = 0;
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                return Task.CompletedTask;
            }

            var loopCount = 0;
            await processor.StartProcessingAsync().ConfigureAwait(false);

            await Track2SendBulkPayloadMessages(track2Client, queueName).ConfigureAwait(false);

            while (Interlocked.Read(ref receives) < 500000)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                loopCount++;

                var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                Console.WriteLine($"Receives per second: {receivesPerSecond}");
            }

            await processor.StopProcessingAsync().ConfigureAwait(false);
        }

        private static async Task Track2Scenario1Receiver(ServiceBusClient track2Client, string queueName, ReceiveMode mode)
        {
            var receiver = track2Client.CreateReceiver(queueName, new ServiceBusReceiverOptions { ReceiveMode = mode });
            var stopwatch = new Stopwatch();
            var messageTotal = 0;
            var loopCount = 0;

            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromHours(3))
            {
                var message = await receiver.ReceiveMessageAsync().ConfigureAwait(false);
                if (message == null)
                {
                    Console.WriteLine("Out of messages");
                    return;
                }
                if (mode == ReceiveMode.PeekLock)
                {
                    await receiver.CompleteMessageAsync(message).ConfigureAwait(false);
                }
                loopCount++;

                if (loopCount >= 50)
                {
                    messageTotal += loopCount;
                    var receivesPerSecond = messageTotal / stopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"Receives per second: {receivesPerSecond}");
                    loopCount = 0;
                }
            }
        }

        private static async Task Track2Scenario2Sender(ServiceBusClient track2Client, string queueName)
        {
            var sender = track2Client.CreateSender(queueName);
            var stopwatch = new Stopwatch();
            var messageTotal = 0;
            var loopCount = 0;

            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromHours(3))
            {
                await sender.SendMessageAsync(new ServiceBusMessage()).ConfigureAwait(false);
                loopCount++;

                if (loopCount >= 50)
                {
                    messageTotal += loopCount;
                    var sendsPerSecond = messageTotal / stopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"Sends per second: {sendsPerSecond}");
                    loopCount = 0;
                }
            }
        }

        private static async Task Track2Scenario2SenderPayload(ServiceBusClient track2Client, string queueName)
        {
            var sender = track2Client.CreateSender(queueName);
            var stopwatch = new Stopwatch();
            var messageTotal = 0;
            var loopCount = 0;
            var drainCount = 0;

            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromHours(1))
            {
                if ((drainCount + 1) > 1023)
                {
                    stopwatch.Stop();
                    await Track2Drain(track2Client, queueName, drainCount).ConfigureAwait(false);
                    drainCount = 0;
                    stopwatch.Start();
                }

                await sender.SendMessageAsync(new ServiceBusMessage(Enumerable.Repeat((byte)1, 1048549).ToArray())).ConfigureAwait(false);
                loopCount++;
                messageTotal++;
                drainCount++;

                if (loopCount >= 50)
                {
                    var sendsPerSecond = messageTotal / stopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"Sends per second: {sendsPerSecond}");
                    loopCount = 0;
                }
            }
        }

        private static async Task Track2Drain(ServiceBusClient track2Client, string queueName, int messageCount)
        {
            var processor = track2Client.CreateProcessor(queueName, new ServiceBusProcessorOptions { ReceiveMode = ReceiveMode.ReceiveAndDelete, MaxConcurrentCalls = 32 });
            long receives = 0;
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                var receivedMessage = args.Message;
                if (receivedMessage == null)
                    return Task.CompletedTask;

                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync().ConfigureAwait(false);

            while (Interlocked.Read(ref receives) < messageCount)
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }

            await processor.StopProcessingAsync().ConfigureAwait(false);
        }

        private static async Task Track1Send1MMessages(string connectionString, string queueName)
        {
            var queueClient = new QueueClient(connectionString, queueName);
            foreach (var _ in Enumerable.Range(0, 100))
            {
                await queueClient.SendAsync(Enumerable.Range(0, 10000).Select(i => new Message()).ToList()).ConfigureAwait(false);
            }
            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1SendBulkPayloadMessages(string connectionString, string queueName)
        {
            var queueClient = new QueueClient(connectionString, queueName);
            foreach (var _ in Enumerable.Range(0, 50))
            {
                await queueClient.SendAsync(Enumerable.Range(0, 10000).Select(i =>
                    new Message(Enumerable.Repeat((byte)1, 20).ToArray())).ToList()).ConfigureAwait(false);
            }
            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Send1MMessagesTopic(string connectionString, string topicName)
        {
            var topicClient = new TopicClient(connectionString, topicName);
            foreach (var _ in Enumerable.Range(0, 100))
            {
                await topicClient.SendAsync(Enumerable.Range(0, 10000).Select(i => new Message()).ToList()).ConfigureAwait(false);
            }
            await topicClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Scenario1Processor(string connectionString, string queueName, T1ReceiveMode mode, int prefetch = 0)
        {
            var queueClient = new QueueClient(connectionString, queueName, mode) { PrefetchCount = prefetch };
            long receives = 0;
            queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler) { AutoComplete = true, MaxConcurrentCalls = 32 });

            Task MessageHandler(Message message, CancellationToken token)
            {
                if (message == null) return Task.CompletedTask;

                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ExceptionReceivedEventArgs args)
            {
                return Task.CompletedTask;
            }

            var loopCount = 0;
            while (Interlocked.Read(ref receives) < 1000000)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                loopCount++;

                var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                Console.WriteLine($"Receives per second: {receivesPerSecond}");
            }

            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Scenario1TopicProcessor(string connectionString, string topicName, string subscriptionName, T1ReceiveMode mode)
        {
            var subscriptionClient = new SubscriptionClient(connectionString, topicName, subscriptionName, mode);
            long receives = 0;
            subscriptionClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler) { AutoComplete = true, MaxConcurrentCalls = 32 });

            Task MessageHandler(Message message, CancellationToken token)
            {
                if (message == null)
                    return Task.CompletedTask;

                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ExceptionReceivedEventArgs args)
            {
                return Task.CompletedTask;
            }

            var loopCount = 0;
            while (Interlocked.Read(ref receives) < 1000000)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                loopCount++;

                var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                Console.WriteLine($"Receives per second: {receivesPerSecond}");
            }

            await subscriptionClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Scenario1LongRunProcessor(string connectionString, string queueName, T1ReceiveMode mode, int prefetch = 0)
        {
            var queueClient = new QueueClient(connectionString, queueName, mode) { PrefetchCount = prefetch };
            long receives = 0;

            Task MessageHandler(Message message, CancellationToken token)
            {
                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ExceptionReceivedEventArgs args)
            {
                return Task.CompletedTask;
            }

            var loopCount = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler) { AutoComplete = true, MaxConcurrentCalls = 32 });

            while (stopwatch.Elapsed < TimeSpan.FromDays(2.5))
            {
                await Track1SendBulkPayloadMessages(connectionString, queueName).ConfigureAwait(false);

                while (Interlocked.Read(ref receives) < 500000)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                    loopCount++;

                    var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                    Console.WriteLine($"Receives per second: {receivesPerSecond}");
                }
            }

            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Scenario1PayloadProcessor(string connectionString, string queueName, T1ReceiveMode mode, int prefetch = 0)
        {
            var queueClient = new QueueClient(connectionString, queueName, mode) { PrefetchCount = prefetch };
            long receives = 0;

            Task MessageHandler(Message message, CancellationToken token)
            {
                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ExceptionReceivedEventArgs args)
            {
                return Task.CompletedTask;
            }

            var loopCount = 0;
            queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler) { AutoComplete = true, MaxConcurrentCalls = 32 });

            await Track1SendBulkPayloadMessages(connectionString, queueName).ConfigureAwait(false);

            while (Interlocked.Read(ref receives) < 500000)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                loopCount++;

                var receivesPerSecond = (double)Interlocked.Read(ref receives) / (loopCount * 5);
                Console.WriteLine($"Receives per second: {receivesPerSecond}");
            }

            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Scenario1Receiver(string connectionString, string queueName, T1ReceiveMode mode)
        {
            var receiver = new MessageReceiver(connectionString, queueName, mode);
            var stopwatch = new Stopwatch();
            var messageTotal = 0;
            var loopCount = 0;

            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromHours(3))
            {
                var message = await receiver.ReceiveAsync().ConfigureAwait(false);
                if (message == null)
                {
                    Console.WriteLine("Out of messages");
                    return;
                }
                if (mode == T1ReceiveMode.PeekLock)
                {
                    await receiver.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                }
                loopCount++;

                if (loopCount >= 50)
                {
                    messageTotal += loopCount;
                    var receivesPerSecond = messageTotal / stopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"Receives per second: {receivesPerSecond}");
                    loopCount = 0;
                }
            }
        }

        private static async Task Track1Scenario2Sender(string connectionString, string queueName)
        {
            var sender = new MessageSender(connectionString, queueName);
            var stopwatch = new Stopwatch();
            var messageTotal = 0;
            var loopCount = 0;

            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromHours(3))
            {
                await sender.SendAsync(new Message()).ConfigureAwait(false);
                loopCount++;

                if (loopCount >= 50)
                {
                    messageTotal += loopCount;
                    var sendsPerSecond = messageTotal / stopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"Sends per second: {sendsPerSecond}");
                    loopCount = 0;
                }
            }
        }

        private static async Task Track1Scenario2SenderPayload(string connectionString, string queueName)
        {
            var sender = new MessageSender(connectionString, queueName);
            var stopwatch = new Stopwatch();
            var messageTotal = 0;
            var loopCount = 0;
            var drainCount = 0;

            stopwatch.Start();
            while (stopwatch.Elapsed < TimeSpan.FromHours(1))
            {
                if ((drainCount + 1) > 1023)
                {
                    stopwatch.Stop();
                    await Track1Drain(connectionString, queueName, drainCount).ConfigureAwait(false);
                    drainCount = 0;
                    stopwatch.Start();
                }

                await sender.SendAsync(new Message(Enumerable.Repeat((byte)1, 1048549).ToArray())).ConfigureAwait(false);
                loopCount++;
                messageTotal++;
                drainCount++;

                if (loopCount >= 50)
                {
                    var sendsPerSecond = messageTotal / stopwatch.Elapsed.TotalSeconds;
                    Console.WriteLine($"Sends per second: {sendsPerSecond}");
                    loopCount = 0;
                }
            }
        }

        private static async Task Track1Drain(string connectionString, string queueName, int messageCount)
        {
            var queueClient = new QueueClient(connectionString, queueName, T1ReceiveMode.ReceiveAndDelete);
            long receives = 0;
            queueClient.RegisterMessageHandler(MessageHandler, new MessageHandlerOptions(ErrorHandler) { AutoComplete = true, MaxConcurrentCalls = 32 });

            Task MessageHandler(Message message, CancellationToken token)
            {
                if (message == null) return Task.CompletedTask;

                Interlocked.Increment(ref receives);

                return Task.CompletedTask;
            }

            Task ErrorHandler(ExceptionReceivedEventArgs args)
            {
                return Task.CompletedTask;
            }

            while (Interlocked.Read(ref receives) < messageCount)
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }

            await queueClient.CloseAsync().ConfigureAwait(false);
        }
    }
}
