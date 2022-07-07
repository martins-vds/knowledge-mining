﻿namespace KnowledgeMining.UI.Services.Storage
{
    public readonly record struct UploadDocument(string Name, string ContentType, IDictionary<string, string>? Tags, Stream Content, bool LeaveOpen = false);
}
