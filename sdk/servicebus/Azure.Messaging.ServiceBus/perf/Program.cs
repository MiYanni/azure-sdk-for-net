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

            //var track2Client =  new ServiceBusClient(connectionString);
            //await Track2Send1MMessages(track2Client, queueName).ConfigureAwait(false);
            //await Track2Scenario1Processor(track2Client, queueName, ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);
            //await Track2Scenario1Receiver(track2Client, queueName, ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);
            //await Track2Scenario2Sender(track2Client, queueName).ConfigureAwait(false);

            await Track1Send1MMessages(connectionString, queueName).ConfigureAwait(false);
            await Track1Scenario1Processor(connectionString, queueName, T1ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);
            //await Track1Scenario1Receiver(connectionString, queueName, T1ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);
            //await Track1Scenario2Sender(connectionString, queueName).ConfigureAwait(false);
        }

        private static async Task Track2Send1MMessages(ServiceBusClient track2Client, string queueName)
        {
            var sender = track2Client.CreateSender(queueName);
            foreach (var _ in Enumerable.Range(0, 100))
            {
                await sender.SendMessagesAsync(Enumerable.Range(0, 10000).Select(i => new ServiceBusMessage())).ConfigureAwait(false);
            }
        }

        private static async Task Track2Scenario1Processor(ServiceBusClient track2Client, string queueName, ReceiveMode mode)
        {
            var processor = track2Client.CreateProcessor(queueName, new ServiceBusProcessorOptions { ReceiveMode = mode, MaxConcurrentCalls = 32 });
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

        private static async Task Track1Send1MMessages(string connectionString, string queueName)
        {
            var queueClient = new QueueClient(connectionString, queueName);
            foreach (var _ in Enumerable.Range(0, 100))
            {
                await queueClient.SendAsync(Enumerable.Range(0, 10000).Select(i => new Message()).ToList()).ConfigureAwait(false);
            }
            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task Track1Scenario1Processor(string connectionString, string queueName, T1ReceiveMode mode)
        {
            var queueClient = new QueueClient(connectionString, queueName, mode);
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
    }
}
