// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;
using System.Text.Json;

namespace KnowledgeMining.Functions.Skills.Models
{
    public class WebApiSkillResponse
    {
        public List<WebApiResponseRecord> Values { get; set; } = new List<WebApiResponseRecord>();

        public override string ToString()
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            return JsonSerializer.Serialize(this, serializeOptions);
        }
    }
}