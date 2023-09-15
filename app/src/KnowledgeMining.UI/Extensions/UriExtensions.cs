namespace KnowledgeMining.UI.Extensions
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

        public static string GetFileName(this Uri? uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            return Path.GetFileName(uri.AbsolutePath);
        }
    }
}
