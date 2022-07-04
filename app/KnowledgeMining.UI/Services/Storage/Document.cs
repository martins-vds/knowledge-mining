namespace KnowledgeMining.UI.Services.Storage
{
    public record struct Document(string Name, IDictionary<string, string> Tags);
}
