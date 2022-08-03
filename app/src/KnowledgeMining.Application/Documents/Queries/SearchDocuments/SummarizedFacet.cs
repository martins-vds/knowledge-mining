using System.Diagnostics.CodeAnalysis;

namespace KnowledgeMining.Application.Documents.Queries.SearchDocuments
{
    public class SummarizedFacet : IEquatable<SummarizedFacet>
    {
        public string? Name { get; set; }
        public long Count { get; set; }
        public IEnumerable<Facet>? Values { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SummarizedFacet);
        }

        public bool Equals(SummarizedFacet? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   Count == other.Count &&
                   EqualityComparer<IEnumerable<Facet>?>.Default.Equals(Values, other.Values);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Count, Values);
        }
    }
}
