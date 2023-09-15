using Azure.Search.Documents.Indexes.Models;
using Microsoft.Spatial;

namespace KnowledgeMining.UI.Services.Search.Models
{
    public class Schema
    {
        public IReadOnlyCollection<SchemaField> Facets { get; set; }
        public IReadOnlyCollection<SchemaField> Tags { get; set; }

        public IReadOnlyCollection<string> SelectFilter { get; set; }

        public IReadOnlyCollection<string> SearchableFields { get; set; }

        public Schema(IList<Azure.Search.Documents.Indexes.Models.SearchField> fields)
        {
            SelectFilter = fields.Select(f => f.Name).ToList().AsReadOnly();
            Facets = fields.Where(f => f.IsFacetable ?? false).Select(f => ToSearchField(f)).ToList().AsReadOnly();
            Tags = fields.Where(f => f.IsFacetable ?? false).Select(f => ToSearchField(f)).ToList().AsReadOnly();
            SearchableFields = fields.Where(f => f.IsSearchable ?? false).Select(f => f.Name).ToList().AsReadOnly();
        }

        private SchemaField ToSearchField(Azure.Search.Documents.Indexes.Models.SearchField field)
        {
            return new SchemaField()
            {
                Name = field.Name,
                Type = ConvertTypeToCrlType(field.Type),
                IsFacetable = field.IsFacetable ?? false,
                IsFilterable = field.IsFilterable ?? false,
                IsKey = field.IsKey ?? false,
                IsHidden = field.IsHidden ?? false,
                IsSearchable = field.IsSearchable ?? false,
                IsSortable = field.IsSortable ?? false
            };
        }

        private Type? ConvertTypeToCrlType(SearchFieldDataType fieldType)
        {
            Type? type = default;
            if (fieldType == SearchFieldDataType.Boolean) type = typeof(bool);
            else if (fieldType == SearchFieldDataType.DateTimeOffset) type = typeof(DateTime);
            else if (fieldType == SearchFieldDataType.Double) type = typeof(double);
            else if (fieldType == SearchFieldDataType.Int32) type = typeof(int);
            else if (fieldType == SearchFieldDataType.Int64) type = typeof(long);
            else if (fieldType == SearchFieldDataType.String) type = typeof(string);
            else if (fieldType == SearchFieldDataType.GeographyPoint) type = typeof(GeographyPoint);

            // Azure Search SearchFieldDataType objects don't follow value comparisons, so use overloaded string conversion operator to be a consistent representation
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.String).ToString()) type = typeof(string[]);
            else if (fieldType == SearchFieldDataType.Complex) type = typeof(string);
            else if (fieldType == SearchFieldDataType.Collection(SearchFieldDataType.Complex)) type = typeof(string[]);
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.DateTimeOffset).ToString()) type = typeof(DateTime[]);
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.GeographyPoint).ToString()) type = typeof(GeographyPoint[]);
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.Double).ToString()) type = typeof(double[]);
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.Boolean).ToString()) type = typeof(bool[]);
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.Int32).ToString()) type = typeof(int[]);
            else if (fieldType.ToString() == SearchFieldDataType.Collection(SearchFieldDataType.Int64).ToString()) type = typeof(long[]);

            return type;
        }
    }
}
