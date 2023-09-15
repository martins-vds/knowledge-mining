// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace KnowledgeMining.Application.Documents.Queries.SearchDocuments
{
    public class Facet : IEquatable<Facet>
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public long Count { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Facet);
        }

        public bool Equals(Facet? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }
    }

}
