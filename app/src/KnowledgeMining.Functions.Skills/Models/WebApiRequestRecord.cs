// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace KnowledgeMining.Functions.Skills.Models
{
    public class WebApiRequestRecord
    {
        public string? RecordId { get; set; }

        private IDictionary<string, object>? _data;
        public IDictionary<string, object> Data
        {
            get { return _data!; }

            set
            {
                if (value is not null)
                {
                    this._data = value;
                }
                else
                {
                    this._data = new Dictionary<string, object>();
                }
            }
        }
    }
}