using KnowledgeMining.Domain.Entities;

namespace KnowledgeMining.Application.Common.Options
{
    public class StorageOptions
    {
        public const string Storage = "Storage";

        public Uri? ServiceUri { get; set; }
        public string ContainerName { get; set; } = "documents";

        public DocumentTag[] Tags { get; set; } = Array.Empty<DocumentTag>();
    }
}
