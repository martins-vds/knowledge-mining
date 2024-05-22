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

        public async Task<EmbeddedReport> GenerateEmbeddedReport(Guid workspaceId, Guid reportId)
        {
            var report = await _powerBiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);

            var tokenRequest = new GenerateTokenRequest(TokenAccessLevel.View, report.DatasetId);
            var embedToken = await _powerBiClient.Reports.GenerateTokenInGroupAsync(workspaceId, reportId, tokenRequest);

            return new EmbeddedReport
            {
                Id = report.Id,
                Token = embedToken.Token,
                Url = report.EmbedUrl
            };
        }
    }
}
