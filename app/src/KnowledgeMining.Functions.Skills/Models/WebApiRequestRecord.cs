// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace KnowledgeMining.Functions.Skills.Models
{
    public class WebApiRequestRecord
    {
        public string? RecordId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}