// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace KnowledgeMining.Functions.Skills.Models
{
    public class WebApiResponseRecord
    {
        public string? RecordId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public List<WebApiErrorWarning> Errors { get; set; } = new List<WebApiErrorWarning>();
        public List<WebApiErrorWarning> Warnings { get; set; } = new List<WebApiErrorWarning>();
    }
}