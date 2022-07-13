// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace KnowledgeMining.Functions.Skills.Models
{
    public class WebApiSkillRequest
    {
        public IList<WebApiRequestRecord> Values { get; set; } = new List<WebApiRequestRecord>();
    }
}