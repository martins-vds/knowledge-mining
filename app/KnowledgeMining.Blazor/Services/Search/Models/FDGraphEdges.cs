using System.Text.Json.Serialization;

namespace KnowledgeMining.UI.Services.Search
{

    public partial class FacetGraphGenerator
    {
        public class FDGraphEdges
        {
            public int Source { get; set; }
            public int Target { get; set; }
            public int Distance { get; set; }
        }
    }
}
