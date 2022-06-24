namespace KnowledgeMining.Blazor.Services.Storage
{
    public interface IStorageService
    {
        Task UploadDocuments(IEnumerable<Document> files, CancellationToken cancellationToken);
    }
}