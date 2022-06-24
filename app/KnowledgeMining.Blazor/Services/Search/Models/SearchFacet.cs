namespace KnowledgeMining.UI.Services.Search.Models
{
    public class SearchFacet
    {
        public string Name { get; set; }
        public IList<string> Values { get; set; }

        public SearchFacet()
        {
            Values = new List<string>();
        }
    }
}
