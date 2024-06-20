namespace KnowledgeMining.Application.Common.Options
{
    public class ChunkSearchOptions
    {
        public const string ChunkSearch = "ChunkSearch";

        public string? Endpoint { get; set; }
        public string? IndexName { get; set; }

        public string? KeyField { get; set; }
        public int PageSize { get; set; }        
    }
}
