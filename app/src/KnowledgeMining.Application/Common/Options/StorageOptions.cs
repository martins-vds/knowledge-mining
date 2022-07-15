namespace KnowledgeMining.Application.Common.Options
{
    public class StorageOptions
    {
        public const string Storage = "Storage";

        public Uri? ServiceUri { get; set; }
        public string ContainerName { get; set; } = "documents";

        public StorageTag[] Tags { get; set; } = Array.Empty<StorageTag>();
    }

    public class StorageTag
    {
        public string Name { get; set; }
        public string[] AllowedValues { get; set; } = Array.Empty<string>();
    }
}
