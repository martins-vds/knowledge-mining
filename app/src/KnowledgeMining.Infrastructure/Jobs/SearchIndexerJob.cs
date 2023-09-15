using Azure.Search.Documents.Indexes;
using KnowledgeMining.Application.Common.Options;
using KnowledgeMining.Application.Documents.Commands.DeleteDocument;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace KnowledgeMining.Infrastructure.Jobs
{
    public class SearchIndexerJob : BackgroundService
    {
        private readonly SearchOptions _searchOptions;
        private readonly ChannelReader<SearchIndexerJobContext> _jobChannel;
        private readonly CancellationToken _applicationStoppingToken;
        private readonly IServiceProvider _serviceProvider;

        public SearchIndexerJob(
            ChannelReader<SearchIndexerJobContext> jobChannel,
            IOptions<SearchOptions> options,
            IServiceProvider serviceProvider,
            IHostApplicationLifetime applicationLifetime)
        {
            _jobChannel = jobChannel;
            _searchOptions = options.Value;
            _serviceProvider = serviceProvider;
            _applicationStoppingToken = applicationLifetime.ApplicationStopping;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _applicationStoppingToken);
            while (!cancellationTokenSource.Token.IsCancellationRequested && await _jobChannel.WaitToReadAsync(cancellationTokenSource.Token))
            {
                while (_jobChannel.TryRead(out var jobContext))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var searchIndexerClient = scope.ServiceProvider.GetRequiredService<SearchIndexerClient>();

                    await searchIndexerClient.RunIndexerAsync(_searchOptions.IndexerName, cancellationTokenSource.Token);
                }
            }
        }
    }
}
