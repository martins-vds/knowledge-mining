namespace KnowledgeMining.Infrastructure.Services.Storage.Models
{
    public record struct Document(string Name, IDictionary<string, string> Tags);
}
