namespace KnowledgeMining.UI.Services.Search.Models
{
    public class SchemaField
    {
        // Fields from Azure Search
        public string? Name { get; set; }
        public Type? Type { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsKey { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsSortable { get; set; }

        // Fields to control
        public PreferredFilter FilterPreference { get; set; }
    }
}
