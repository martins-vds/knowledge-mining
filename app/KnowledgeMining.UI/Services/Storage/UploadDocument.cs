namespace KnowledgeMining.UI.Services.Storage
{
    public readonly record struct UploadDocument(string Name, string ContentType, Stream Content, bool LeaveOpen = false);
}
