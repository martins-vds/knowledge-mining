// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace KnowledgeMining.UI.Services.Search
{
    public class Facet
    {
        public string key { get; set; }
        public List<FacetValue> value { get; set; }
    }

}
