﻿using Azure.Search.Documents.Indexes;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Services.Search.Models;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace KnowledgeMining.UI.Services.Search
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
