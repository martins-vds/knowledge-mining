namespace KnowledgeMining.Application.Documents.Queries.SearchDocuments
{
    public class FacetFilter
    {
        public string? Name { get; set; }
        public IList<string> Values { get; set; }

        public FacetFilter()
        {
            Values = new List<string>();
        }
    }
}
