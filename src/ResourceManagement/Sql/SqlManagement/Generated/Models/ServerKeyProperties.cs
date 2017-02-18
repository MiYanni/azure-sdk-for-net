// 
// Copyright (c) Microsoft and contributors.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// 
// See the License for the specific language governing permissions and
// limitations under the License.
// 

// Warning: This code was generated by a tool.
// 
// Changes to this file may cause incorrect behavior and will be lost if the
// code is regenerated.

using System;
using System.Linq;

namespace Microsoft.Azure.Management.Sql.Models
{
    /// <summary>
    /// Represents an Azure SQL Server Key.
    /// </summary>
    public partial class ServerKeyProperties
    {
        private DateTime _creationDate;
        
        /// <summary>
        /// Optional. The date the Server Key was created in Azure SQL Database.
        /// </summary>
        public DateTime CreationDate
        {
            get { return this._creationDate; }
            set { this._creationDate = value; }
        }
        
        private string _serverKeyType;
        
        /// <summary>
        /// Optional. The type of the Server Key.
        /// </summary>
        public string ServerKeyType
        {
            get { return this._serverKeyType; }
            set { this._serverKeyType = value; }
        }
        
        private string _thumbprint;
        
        /// <summary>
        /// Optional. The thumbprint of the Server Key.
        /// </summary>
        public string Thumbprint
        {
            get { return this._thumbprint; }
            set { this._thumbprint = value; }
        }
        
        private string _uri;
        
        /// <summary>
        /// Optional. The Uri of the Server Key.
        /// </summary>
        public string Uri
        {
            get { return this._uri; }
            set { this._uri = value; }
        }
        
        /// <summary>
        /// Initializes a new instance of the ServerKeyProperties class.
        /// </summary>
        public ServerKeyProperties()
        {
        }
    }
}
