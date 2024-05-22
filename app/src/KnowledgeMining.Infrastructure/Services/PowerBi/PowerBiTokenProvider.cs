using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System.Net.Http.Headers;

namespace KnowledgeMining.Infrastructure.Services.PowerBi
{
    public class PowerBiTokenProvider : ITokenProvider
    {
        private readonly PowerBiOptions _options;

        public PowerBiTokenProvider(IOptionsMonitor<PowerBiOptions> optionsMonitor)
        {
            _options = optionsMonitor.CurrentValue;
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            var authToken = await GetAccessTokenAsync(cancellationToken);

            if (authToken == null)
            {
                throw new InvalidOperationException("Failed to obtain Power BI access token");
            }

            return new AuthenticationHeaderValue(authToken.TokenType, authToken.AccessToken);
        }

        private async Task<AuthenticationResult> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var daemonClient = ConfidentialClientApplicationBuilder.Create(_options.ClientId)
                                                                                              .WithAuthority(_options.Authority)
                                                                                              .WithClientSecret(_options.ClientSecret)
                                                                                              .Build();
            
            return await daemonClient.AcquireTokenForClient([PowerBiOptions.MSGraphScope]).ExecuteAsync(cancellationToken);
        }
    }
}
