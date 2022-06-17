using System;

namespace KnowledgeMining.UI.Options
{
    public class StorageOptions
    {
        public const string Storage = "Storage";

        public Uri ServiceUri { get; set; }
        public string ContainerName { get; set; }
    }
}
