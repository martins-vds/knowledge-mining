namespace KnowledgeMining.UI.Services.Storage.Models
{
    public readonly record struct UploadDocument(string Name, string ContentType, IDictionary<string, string>? Tags, Stream Content, bool LeaveOpen = false);
}
