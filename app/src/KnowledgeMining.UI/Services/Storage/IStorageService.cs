using KnowledgeMining.UI.Services.Storage.Models;

namespace KnowledgeMining.UI.Services.Storage
{
    public interface IStorageService
    {
        Task<SearchDocumentsResponse> SearchDocuments(string? searchText, int pageSize, string? continuationToken, CancellationToken cancellationToken);
        Task UploadDocuments(IEnumerable<UploadDocument> files, CancellationToken cancellationToken);
        ValueTask<byte[]> DownloadDocument(string fileName, CancellationToken cancellationToken);
        ValueTask DeleteDocument(string fileName, CancellationToken cancellationToken);
    }
}