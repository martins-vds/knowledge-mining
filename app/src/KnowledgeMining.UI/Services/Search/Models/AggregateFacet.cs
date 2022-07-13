namespace KnowledgeMining.UI.Services.Search.Models
{
    public class AggregateFacet
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public IEnumerable<Facet> Values { get; set; }

        public AggregateFacet()
        {

        }
    }
}
