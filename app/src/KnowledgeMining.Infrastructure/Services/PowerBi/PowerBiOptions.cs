using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMining.Infrastructure.Services.PowerBi
{
    public class PowerBiOptions
    {
        public const string PowerBi = nameof(PowerBi);

        public const string MSGraphScope = "https://analysis.windows.net/powerbi/api/.default";

        public string TenantId { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    }
}
