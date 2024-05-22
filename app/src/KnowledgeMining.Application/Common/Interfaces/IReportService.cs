using KnowledgeMining.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Application.Common.Interfaces
{
    public interface IReportService
    {
        Task<EmbeddedReport> GenerateEmbeddedReport(Guid workspaceId, Guid reportId);
    }
}
