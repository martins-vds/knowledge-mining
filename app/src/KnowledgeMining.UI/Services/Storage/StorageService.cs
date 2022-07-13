using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Services.Storage.Models;
using Microsoft.Extensions.Options;
using System.Net;

namespace KnowledgeMining.UI.Services.Storage
{
    public class StorageService : IStorageService
    {
        private const int MAX_ITEMS_PER_REQUEST = 5_000;
        private const int DEFAULT_PAGE_SIZE = 10;

        private readonly BlobServiceClient _blobServiceClient;
        private readonly StorageOptions _storageOptions;

        private readonly ILogger<StorageService> _logger;

        public StorageService(BlobServiceClient blobServiceClient,
                                 IOptions<StorageOptions> storageOptions,
                                 ILogger<StorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _storageOptions = storageOptions.Value;
            _logger = logger;
        }

        public async Task<SearchDocumentsResponse> SearchDocuments(string? searchText, int pageSize, string? continuationToken, CancellationToken cancellationToken)
        {
            searchText ??= string.Empty;
            pageSize = pageSize is > 0 and <= MAX_ITEMS_PER_REQUEST ? pageSize : DEFAULT_PAGE_SIZE;

            var pages = GetBlobContainerClient()
                            .GetBlobsAsync(prefix: searchText, cancellationToken: cancellationToken)
                            .AsPages(continuationToken, pageSize);

            var iterator = pages.GetAsyncEnumerator(cancellationToken);

            try
            {
                await iterator.MoveNextAsync();

                var page = iterator.Current;

                return new SearchDocumentsResponse()
                {
                    Documents = page?.Values?.Select(b => new Document(b.Name, b.Tags)) ?? Enumerable.Empty<Document>(),
                    NextPage = page?.ContinuationToken
                };
            }
            finally
            {
                await iterator.DisposeAsync();
            }
        }

        public async Task UploadDocuments(IEnumerable<UploadDocument> files, CancellationToken cancellationToken)
        {
            if (files.Any())
            {
                var container = GetBlobContainerClient();

                foreach (var file in files)
                {
                    if (file.Content.Length > 0)
                    {
                        try
                        {
                            var blob = container.GetBlobClient(file.Name);

                            var blobHttpHeader = new BlobHttpHeaders
                            {
                                ContentType = file.ContentType
                            };

                            await blob.UploadAsync(file.Content, httpHeaders: blobHttpHeader, cancellationToken: cancellationToken).ConfigureAwait(false);

                            if (file.Tags?.Any() ?? false)
                            {
                                var nonEmptyTags = RemoveEmptyTags(file.Tags);
                                await blob.SetTagsAsync(nonEmptyTags, cancellationToken: cancellationToken);
                                await blob.SetMetadataAsync(nonEmptyTags, cancellationToken: cancellationToken);
                            }
                        }
                        finally
                        {
                            if (!file.LeaveOpen)
                            {
                                await file.Content.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        private IDictionary<string, string> RemoveEmptyTags(IDictionary<string, string> tags)
        {
            return tags.Where(t => !string.IsNullOrWhiteSpace(t.Key) && !string.IsNullOrWhiteSpace(t.Value)).ToDictionary(t => t.Key, t => t.Value);
        }

        public async ValueTask<byte[]> DownloadDocument(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return Array.Empty<byte>();
            }

            var decodedFilename = WebUtility.UrlDecode(fileName);

            var container = GetBlobContainerClient();
            var blob = container.GetBlobClient(decodedFilename);

            if (!await blob.ExistsAsync(cancellationToken))
            {
                return Array.Empty<byte>();
            }

            using var ms = new MemoryStream();
            var downloadResult = await blob.DownloadAsync(cancellationToken);

            await downloadResult.Value.Content.CopyToAsync(ms, cancellationToken);
            downloadResult.Value.Dispose();

            return ms.ToArray();
        }

        public async ValueTask DeleteDocument(string fileName, CancellationToken cancellationToken)
        {
            var blobContainer = GetBlobContainerClient();

            await blobContainer.DeleteBlobIfExistsAsync(fileName, cancellationToken: cancellationToken);
        }

        private BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
        }
    }
}
