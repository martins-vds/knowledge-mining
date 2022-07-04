﻿namespace KnowledgeMining.UI.Services.Storage
{
    public class SearchDocumentsResponse
    {
        public IEnumerable<Document> Documents { get; internal set; }
        public string? NextPage { get; internal set; }

        public SearchDocumentsResponse()
        {
            Documents = new List<Document>();
        }
    }
}