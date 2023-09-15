using System.Net.Mime;

namespace KnowledgeMining.UI.Extensions
{
    public static class FileExtensions
    {
        public const string PDF = ".pdf";
        public const string TXT = ".txt";
        public const string JSON = ".json";
        public const string LAS = ".las";
        public const string JPG = ".jpg";
        public const string JPEG = ".jpeg";
        public const string PNG = ".png";
        public const string GIF = ".gif";
        public const string XML = ".xml";
        public const string HTM = ".htm";
        public const string MP4 = ".mp4";
        public const string DOC = ".doc";
        public const string DOCX = ".docx";
        public const string PPT = ".ppt";
        public const string PPTX = ".pptx";
        public const string XLS = ".xls";
        public const string XLSX = ".xlsx";
        public const string UNKNOWN = "";

        private static IReadOnlyDictionary<string, string> _contentTypes = new Dictionary<string, string>()
        {
            {PDF     , MediaTypeNames.Application.Pdf   },
            {TXT     , MediaTypeNames.Text.Plain   },
            {JSON    , MediaTypeNames.Application.Json  },
            {LAS     , MediaTypeNames.Application.Octet   },
            {JPG     , MediaTypeNames.Image.Jpeg   },
            {JPEG    , MediaTypeNames.Image.Jpeg  },
            {PNG     , "image/png"   },
            {GIF     , MediaTypeNames.Image.Gif  },
            {XML     , MediaTypeNames.Text.Xml },
            {HTM     , MediaTypeNames.Text.Html   },
            {MP4     , "video/mp4"   },
            {DOC     , "application/msword"   },
            {DOCX    , "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            {PPT     , "application/vnd.ms-powerpoint"   },
            {PPTX    , "application/vnd.openxmlformats-officedocument.presentationml.presentation"   },
            {XLS     , "application/vnd.ms-excel"   },
            {XLSX    , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"   },
            {UNKNOWN , ""       }
        };

        public static string GetContentTypeForFileExtension(string extension)
        {
            if (!string.IsNullOrEmpty(extension) && _contentTypes.ContainsKey(extension))
            {
                return _contentTypes[extension];
            }

            return _contentTypes[UNKNOWN];
        }
    }
}
