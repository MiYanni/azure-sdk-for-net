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

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            var connectionString = "";
            var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender("");
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
                    await sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
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
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            async Task Receive(ProcessMessageEventArgs args)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxReceiveDelayMs)), cancellationToken).ConfigureAwait(false);
                _ = Convert.ToInt32(args.Message.Body.ToString());
                await args.CompleteMessageAsync(args.Message, cancellationToken).ConfigureAwait(false);
                tcs.SetResult(true);
            }
            var processor = client.CreateProcessor("");
            processor.ProcessMessageAsync += Receive;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //await processor.Receive(cancellationToken);
                    await processor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
                    await tcs.Task.ConfigureAwait(false);
                    await processor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
                    Interlocked.Increment(ref Metrics.Receives);
                    tcs.SetResult(false);
                }
                catch (Exception e) when (!ContainsOperationCanceledException(e))
                {
                    Metrics.Exceptions.Enqueue(e);
                }
            }
        }

        //private async Task Receive(ProcessMessageEventArgs args)
        //{
        //    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxReceiveDelayMs))).ConfigureAwait(false);
        //    Convert.ToInt32(args.Message.Body.ToString());
        //    await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        //}

        //// Simulates method in SDK
        //private async Task Send(int index, CancellationToken cancellationToken)
        //{
        //    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxSendDelayMs)), cancellationToken);
        //    var d = Random.NextDouble();
        //    if (d < Options.SendExceptionRate)
        //    {
        //        throw new SendException(d.ToString());
        //    }
        //    else
        //    {
        //        await _channel.Writer.WriteAsync(index, cancellationToken);
        //    }
        //}

        //// Simulates method in SDK
        //private async Task Receive(CancellationToken cancellationToken)
        //{
        //    await Task.Delay(TimeSpan.FromMilliseconds(Random.Next(0, Options.MaxReceiveDelayMs)), cancellationToken);
        //    var d = Random.NextDouble();
        //    if (d < Options.ReceiveExceptionRate)
        //    {
        //        throw new ReceiveException(d.ToString());
        //    }
        //    else
        //    {
        //        await _channel.Reader.ReadAsync(cancellationToken);
        //    }
        //}

        public class MessageLockRenewalOptions : StressOptions
        {
            [Option("maxSendDelayMs", Default = 50, HelpText = "Max send delay (in milliseconds)")]
            public int MaxSendDelayMs { get; set; }

            [Option("maxReceiveDelayMs", Default = 200, HelpText = "Max send delay (in milliseconds)")]
            public int MaxReceiveDelayMs { get; set; }

            [Option("receivers", Default = 3, HelpText = "Number of receivers")]
            public int Receivers { get; set; }

            [Option("sendExceptionRate", Default = .01, HelpText = "Rate of send exceptions")]
            public double SendExceptionRate { get; set; }

            [Option("receiveExceptionRate", Default = .02, HelpText = "Rate of receive exceptions")]
            public double ReceiveExceptionRate { get; set; }
        }

        public class MessageLockRenewalMetrics : StressMetrics
        {
            public long Sends;
            public long Receives;
            public long Unprocessed => (Interlocked.Read(ref Sends) - Interlocked.Read(ref Receives));
        }

        //public class SendException : Exception
        //{
        //    public SendException()
        //    {
        //    }

        //    public SendException(string message) : base(message)
        //    {
        //    }
        //}

        //public class ReceiveException : Exception
        //{
        //    public ReceiveException()
        //    {
        //    }

        //    public ReceiveException(string message) : base(message)
        //    {
        //    }
        //}
    }
}
