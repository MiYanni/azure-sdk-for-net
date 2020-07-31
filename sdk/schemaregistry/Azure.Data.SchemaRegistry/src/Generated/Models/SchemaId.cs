// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

namespace Azure.Data.SchemaRegistry.Models
{
    /// <summary> The id of a Schema Registry schema. </summary>
    public partial class SchemaId
    {
        /// <summary> Initializes a new instance of SchemaId. </summary>
        internal SchemaId()
        {
        }

        /// <summary> Initializes a new instance of SchemaId. </summary>
        /// <param name="id"> The id of the schema. </param>
        internal SchemaId(string id)
        {
            Id = id;
        }

        /// <summary> The id of the schema. </summary>
        public string Id { get; }
    }
}
