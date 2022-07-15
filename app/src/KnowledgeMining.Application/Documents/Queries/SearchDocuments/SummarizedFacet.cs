namespace KnowledgeMining.Application.Documents.Queries.SearchDocuments
{
    public class SummarizedFacet
    {
        public string? Name { get; set; }
        public long Count { get; set; }
        public IEnumerable<Facet>? Values { get; set; }
    }
}
