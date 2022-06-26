namespace KnowledgeMining.Blazor.Extensions
{
    public static class UriExtensions
    {
        public static string GetFileExtension(this Uri? uri)
        {
            if (uri == null)
            {
                return FileExtensions.UNKNOWN;
            }

            var pathWithoutQueryString = $"{uri?.Scheme}{Uri.SchemeDelimiter}{uri?.Authority}{uri?.AbsolutePath}";

            return Path.GetExtension(pathWithoutQueryString);
        }
    }
}
