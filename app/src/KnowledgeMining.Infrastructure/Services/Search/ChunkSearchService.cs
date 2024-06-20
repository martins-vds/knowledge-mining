using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Application.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    public class ChunkSearchService([FromKeyedServices("chunk")]SearchClient searchClient, IOptions<ChunkSearchOptions> options) : IChunkSearchService
    {
        private readonly SearchClient _searchClient = searchClient;
        private readonly ChunkSearchOptions _options = options.Value;

        public async Task<IEnumerable<string>> QueryDocumentChuncksAsync(float[] embedding, string document, CancellationToken cancellationToken = default)
        {
            var filter = $"{_options.KeyField} eq '{document}'";

            Azure.Search.Documents.SearchOptions searchOptions = new()
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

            vectorQuery.Fields.Add("text_vector");
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
