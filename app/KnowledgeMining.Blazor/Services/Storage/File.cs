namespace KnowledgeMining.Blazor.Services.Storage
{
    public readonly record struct Document(string Name, string ContentType, Stream Content, bool LeaveOpen = false);
}
