namespace KnowledgeMining.UI.Services.Search.Models
{
    public class SearchFacet
    {
        public string Key { get; set; }
        public IEnumerable<string> Value { get; set; } = Enumerable.Empty<string>();
    }
}
