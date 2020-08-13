// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.ComponentModel;

namespace Azure.Data.SchemaRegistry.Models
{
    /// <summary> The SerializationType. </summary>
    public readonly partial struct SerializationType : IEquatable<SerializationType>
    {
        private readonly string _value;

        /// <summary> Determines if two <see cref="SerializationType"/> values are the same. </summary>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        public SerializationType(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private const string AvroValue = "avro";

        /// <summary> Avro Serialization schema type. </summary>
        public static SerializationType Avro { get; } = new SerializationType(AvroValue);
        /// <summary> Determines if two <see cref="SerializationType"/> values are the same. </summary>
        public static bool operator ==(SerializationType left, SerializationType right) => left.Equals(right);
        /// <summary> Determines if two <see cref="SerializationType"/> values are not the same. </summary>
        public static bool operator !=(SerializationType left, SerializationType right) => !left.Equals(right);
        /// <summary> Converts a string to a <see cref="SerializationType"/>. </summary>
        public static implicit operator SerializationType(string value) => new SerializationType(value);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is SerializationType other && Equals(other);
        /// <inheritdoc />
        public bool Equals(SerializationType other) => string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        /// <inheritdoc />
        public override string ToString() => _value;
    }
}
