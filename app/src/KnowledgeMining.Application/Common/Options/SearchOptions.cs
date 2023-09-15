namespace KnowledgeMining.Application.Common.Options
{
    public class SearchOptions
    {
        public const string Search = "Search";

        public string? Endpoint { get; set; }
        public string? IndexName { get; set; }

        public string? IndexerName { get; set; }
        public string? SuggesterName { get; set; }
        public bool IsPathBase64Encoded { get; set; } = true;
        public string? KeyField { get; set; }
        public int PageSize { get; set; }

    }
}
