using KnowledgeMining.Application.Common.Interfaces;
using KnowledgeMining.Application.Common.Models;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Infrastructure.Services.PowerBi
{
    public class PowerBiReportService : IReportService
    {
        private readonly IPowerBIClient _powerBiClient;

        public PowerBiReportService(IPowerBIClient powerBiClient)
        {
            _powerBiClient = powerBiClient;
        }

        public async Task<EmbeddedReport> GenerateEmbeddedReport(Guid workspaceId, Guid reportId, CancellationToken cancellationToken = default)
        {
            var report = await _powerBiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);

            var tokenRequest = new GenerateTokenRequestV2(
                reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },
                datasets: new List<GenerateTokenRequestV2Dataset>() { new GenerateTokenRequestV2Dataset(report.DatasetId) },
                targetWorkspaces: new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(workspaceId) }
                );

            var embedToken = await _powerBiClient.EmbedToken.GenerateTokenAsync(tokenRequest, cancellationToken: cancellationToken);

            return new EmbeddedReport
            {
                Id = report.Id,
                Token = embedToken.Token,
                Url = report.EmbedUrl
            };
        }
    }
}
