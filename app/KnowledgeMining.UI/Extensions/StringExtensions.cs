using System.Globalization;
using System.Text.RegularExpressions;

namespace KnowledgeMining.UI.Extensions
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string camelCaseString)
        {
            if (string.IsNullOrEmpty(camelCaseString))
            {
                return string.Empty;
            }

            return Regex.Replace(camelCaseString, @"([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        public static string ToTitleCase(this string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return string.Empty;
            }

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(@string);
        }

        public static string GetFileExtension(this string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return string.Empty;
            }

            return Path.GetExtension(@string).ToLowerInvariant();
        }
    }
}
