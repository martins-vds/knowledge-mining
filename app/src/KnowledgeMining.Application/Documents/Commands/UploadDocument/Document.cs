namespace KnowledgeMining.Application.Documents.Commands.UploadDocument
{
    public readonly record struct Document(string Name, string ContentType, IDictionary<string, string>? Tags, Stream Content, bool LeaveOpen = false);
}
