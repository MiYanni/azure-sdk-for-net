// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Azure.Messaging.ServiceBus.Amqp
{
    /// <summary>
    /// TODO.
    /// </summary>
    public struct AmqpBodyReader
    {
        //public AmqpBodyReader(ReadOnlyMemory<byte> amqpMessage)
        //{

        //}

        private readonly ServiceBusReceivedMessage _message;

        /// <summary>
        /// TODO.
        /// </summary>
        /// <param name="message"></param>
        public AmqpBodyReader(ServiceBusReceivedMessage message)
        {
            _message = message;
        }

        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static bool Read()
        {
            return false;
        }

        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static bool GetBoolean() { return false; } // DecodeBoolean
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static byte GetByte() { return byte.MinValue; } // DecodeUByte
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static char GetChar() { return (char)0; } // DecodeChar
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetDateTime() { return DateTime.MinValue; } // DecodeTimeStamp
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static decimal GetDecimal() { return decimal.MinValue; } // DecodeDecimal
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static double GetDouble() { return double.MinValue; } // DecodeDouble
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static Guid GetGuid() { return Guid.Empty; } // DecodeUuid
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static short GetInt16() { return short.MinValue; } // DecodeShort
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static int GetInt32() { return int.MinValue; } // DecodeInt
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static long GetInt64() { return long.MinValue; } // DecodeLong
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static sbyte GetSByte() { return sbyte.MinValue; } // DecodeByte
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static float GetSingle() { return float.MinValue; } // DecodeFloat
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static string GetString() { return null; } // DecodeString
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static ushort GetUInt16() { return ushort.MinValue; } // DecodeUShort
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static uint GetUInt32() { return uint.MinValue; } // DecodeUInt
        /// <summary>
        /// TODO.
        /// </summary>
        /// <returns></returns>
        public static ulong GetUInt64() { return ulong.MinValue; } // DecodeULong
    }
}
