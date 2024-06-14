using Azure.Identity;
using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Application.Common.Options;
using KnowledgeMining.Application.Documents.Commands.DeleteDocument;
using KnowledgeMining.Infrastructure.Extensions;
using KnowledgeMining.Infrastructure.Jobs;
using KnowledgeMining.Infrastructure.Services.OpenAI;
using KnowledgeMining.Infrastructure.Services.PowerBi;
using KnowledgeMining.Infrastructure.Services.Search;
using KnowledgeMining.Infrastructure.Services.Storage;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using OpenAI;
using System.Threading.Channels;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.ConfigureDefaults(configuration.GetSection("AzureDefaults"));
                clientBuilder.UseCredential(new DefaultAzureCredential());

                clientBuilder.AddBlobServiceClient(configuration.GetSection(StorageOptions.Storage));
                clientBuilder.AddSearchClient(configuration.GetSection(SearchOptions.Search));
                clientBuilder.AddSearchIndexClient(configuration.GetSection(SearchOptions.Search));
                clientBuilder.AddSearchIndexerClient(configuration.GetSection(SearchOptions.Search));
            });

            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IStorageService, StorageService>();

            services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.OpenAI));
            services.AddScoped(services =>
            {
                var options = services.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                return new OpenAIClient(new System.ClientModel.ApiKeyCredential(options.ApiKey), new OpenAIClientOptions() { Endpoint = options.Endpoint });
            });
            services.AddScoped<IChatService, OpenAIService>();

            services.AddSingleton(Channel.CreateUnbounded<SearchIndexerJobContext>(new UnboundedChannelOptions() { SingleWriter = true, SingleReader = true }));
            services.AddSingleton(provider =>
            {
                return provider.GetService<Channel<SearchIndexerJobContext>>()!.Writer;
            });
            services.AddSingleton(provider =>
            {
                return provider.GetService<Channel<SearchIndexerJobContext>>()!.Reader;
            });
            services.AddHostedService<SearchIndexerJob>();

            services.AddBlazorPowerBI(configuration);

            return services;
        }

        private static IServiceCollection AddBlazorPowerBI(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PowerBiOptions>(configuration.GetSection(PowerBiOptions.PowerBi));
            services.AddTransient<PowerBiTokenProvider>();
            services.AddScoped<IPowerBIClient, PowerBIClient>(options =>
            {
                var tokenProvider = options.GetRequiredService<PowerBiTokenProvider>();

                return new PowerBIClient(new TokenCredentials(tokenProvider));
            });
            services.AddTransient<IReportService, PowerBiReportService>();

            return services;
        }
    }
}
