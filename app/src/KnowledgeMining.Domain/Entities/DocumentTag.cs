namespace KnowledgeMining.Domain.Entities
{
    public class DocumentTag
    {
        public string? Name { get; set; }
        public string[] AllowedValues { get; set; } = Array.Empty<string>();
    }
}
