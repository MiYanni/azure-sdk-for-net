// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Test.Stress;
using System.Threading.Tasks;

namespace Azure.Messaging.ServiceBus.Stress
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await StressProgram.Main(typeof(Program).Assembly, args).ConfigureAwait(false);
        }
    }
}
