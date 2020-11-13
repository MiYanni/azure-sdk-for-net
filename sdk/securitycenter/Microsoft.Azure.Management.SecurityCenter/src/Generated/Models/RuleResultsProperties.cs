// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.Security.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Rule results properties.
    /// </summary>
    public partial class RuleResultsProperties
    {
        /// <summary>
        /// Initializes a new instance of the RuleResultsProperties class.
        /// </summary>
        public RuleResultsProperties()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the RuleResultsProperties class.
        /// </summary>
        /// <param name="results">Expected results in the baseline.</param>
        public RuleResultsProperties(IList<IList<string>> results = default(IList<IList<string>>))
        {
            Results = results;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets expected results in the baseline.
        /// </summary>
        [JsonProperty(PropertyName = "results")]
        public IList<IList<string>> Results { get; set; }

    }
}
