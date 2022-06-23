using KnowledgeMining.UI.Services.Search;

namespace KnowledgeMining.Blazor.Models
{
    public class AggregateFacet
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public IEnumerable<Facet> Values { get; set; }
    }
}
