using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using KnowledgeMining.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Infrastructure.Services.Search
{
    public class MockChunkSearchService : IChunkSearchService
    {
        public Task<IEnumerable<string>> QueryDocumentChuncksAsync(float[] embedding, string document, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<string>>(["dummy"]);
        }
    }

    public class ChunkSearchService([FromKeyedServices("chunk")]SearchClient searchClient) : IChunkSearchService
    {
        private readonly SearchClient _searchClient = searchClient;

        public async Task<IEnumerable<string>> QueryDocumentChuncksAsync(float[] embedding, string document, CancellationToken cancellationToken = default)
        {
            var filter = $"metadata_storage_path eq '{document}'";

            SearchOptions searchOptions = new()
            {
                Filter = filter,
                QueryType = SearchQueryType.Semantic,
                SemanticSearch = new()
                {
                    SemanticConfigurationName = "default",
                    QueryCaption = new(QueryCaptionType.Extractive),
                },
                Size = 5
            };

            var k = 50;
            var vectorQuery = new VectorizedQuery(embedding)
            {
                KNearestNeighborsCount = k
            };

            vectorQuery.Fields.Add("embedding");
            searchOptions.VectorSearch = new();
            searchOptions.VectorSearch.Queries.Add(vectorQuery);

            var searchResultResponse = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions, cancellationToken);

            if (searchResultResponse.Value is null)
            {
                throw new InvalidOperationException("failed to get search result");
            }

            SearchResults<SearchDocument> searchResult = searchResultResponse.Value;

            var chunks = new List<string>();
            foreach (var doc in searchResult.GetResults())
            {
                string? contentValue;
                try
                {
                    doc.Document.TryGetValue("chunk", out var value);
                    contentValue = (string)value;
                }
                catch (ArgumentNullException)
                {
                    contentValue = null;
                }

                if (contentValue is string content)
                {
                    content = content.Replace('\r', ' ').Replace('\n', ' ');
                    chunks.Add(content);
                }
            }

            return [.. chunks];
        }
    }
}
