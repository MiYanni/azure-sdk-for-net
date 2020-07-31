// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using Azure;
using Azure.Core;

namespace Azure.Data.SchemaRegistry
{
    internal class SchemaGetByIdHeaders
    {
        private readonly Response _response;
        public SchemaGetByIdHeaders(Response response)
        {
            _response = response;
        }
        /// <summary> URL location of schema, identified by schema group, schema name, and version. </summary>
        public string Location => _response.Headers.TryGetValue("Location", out string value) ? value : null;
        /// <summary> Serialization type for the schema being stored. </summary>
        public string XSerialization => _response.Headers.TryGetValue("X-Serialization", out string value) ? value : null;
        /// <summary> References specific schema in registry namespace. </summary>
        public string XSchemaId => _response.Headers.TryGetValue("X-Schema-Id", out string value) ? value : null;
        /// <summary> URL location of schema, identified by schema ID. </summary>
        public string XSchemaIdLocation => _response.Headers.TryGetValue("X-Schema-Id-Location", out string value) ? value : null;
        /// <summary> Version of the returned schema. </summary>
        public int? XSchemaVersion => _response.Headers.TryGetValue("X-Schema-Version", out int? value) ? value : null;
    }
}
