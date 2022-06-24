using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Services.Search;
using Microsoft.Extensions.Options;

namespace KnowledgeMining.Blazor.Services.Storage
{
    public class StorageService : IStorageService
    {
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

        public async Task UploadDocuments(IEnumerable<Document> files, CancellationToken cancellationToken)
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
                            await blob.UploadAsync(file.Content, blobHttpHeader, cancellationToken: cancellationToken).ConfigureAwait(false);
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

        private BlobContainerClient GetBlobContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_storageOptions.ContainerName);
        }
    }
}
