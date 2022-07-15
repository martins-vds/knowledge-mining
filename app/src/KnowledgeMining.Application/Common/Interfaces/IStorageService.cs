using KnowledgeMining.Application.Documents.Queries.GetDocuments;
using UploadDocument = KnowledgeMining.Application.Documents.Commands.UploadDocument.Document;

namespace KnowledgeMining.Application.Common.Interfaces
{
    public interface IStorageService
    {
        Task<GetDocumentsResponse> GetDocuments(string? searchPrefix, int pageSize, string? continuationToken, CancellationToken cancellationToken);
        Task UploadDocuments(IEnumerable<UploadDocument> documents, CancellationToken cancellationToken);
        ValueTask<byte[]> DownloadDocument(string documentName, CancellationToken cancellationToken);
        ValueTask DeleteDocument(string documentName, CancellationToken cancellationToken);
    }
}