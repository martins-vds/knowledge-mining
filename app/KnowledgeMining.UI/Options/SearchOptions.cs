namespace KnowledgeMining.UI.Options
{
    public class SearchOptions
    {
        public const string Search = "Search";

        public string ApiKey { get; set; }
        public string IndexName { get; set; }
        public string IndexerName { get; set; }
        public bool IsPathBase64Encoded { get; set; } = true;
        public string KeyField { get; set; }
        public string ServiceName { get; set; }
    }
}
