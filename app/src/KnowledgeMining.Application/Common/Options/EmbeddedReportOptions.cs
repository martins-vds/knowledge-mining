using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Options
{
    public class EmbeddedReportOptions
    {
        public const string EmbeddedReport = nameof(EmbeddedReport);

        public Guid WorkspaceId { get; set; }

        public Guid ReportId { get; set; }

        public string FallbackUrl { get; set; } = string.Empty;
    }
}
