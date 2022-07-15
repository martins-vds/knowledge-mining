using Azure.Core.Extensions;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

namespace KnowledgeMining.Infrastructure.Extensions
{
    public static class SearchClientBuilderExtensions
    {
        public static IAzureClientBuilder<SearchIndexerClient, SearchClientOptions> AddSearchIndexerClient<TBuilder, TConfiguration>(
            this TBuilder builder,
            TConfiguration configuration)
            where TBuilder : IAzureClientFactoryBuilderWithConfiguration<TConfiguration>
        {
            return builder.RegisterClientFactory<SearchIndexerClient, SearchClientOptions>(configuration);
        }
    }
}
