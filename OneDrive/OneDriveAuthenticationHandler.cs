using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Movington.PhotoTransfer.OneDrive
{
    public sealed class OneDriveAuthenticationHandler
    {
        private static readonly string[] Scopes = { "Files.Read.All" };

        private readonly ILogger<OneDriveAuthenticationHandler> _logger;
        private readonly OneDriveAuthenticationOptions _authenticationOptions;

        public OneDriveAuthenticationHandler(
            ILogger<OneDriveAuthenticationHandler> logger,
            IOptions<OneDriveAuthenticationOptions> authenticationOptions)
        {
            _logger = logger;
            _authenticationOptions = authenticationOptions.Value;
        }

        public async Task<IAuthenticationProvider> CreateLoggedInAuthenticationProviderAsync(CancellationToken cancellationToken)
        {
            var authenticationClient = PublicClientApplicationBuilder.Create(_authenticationOptions.ClientId)
                .WithDefaultRedirectUri()
                .WithAuthority(_authenticationOptions.CloudInstance, _authenticationOptions.Tenant)
                .Build();

            var tokenCacheProperties = new StorageCreationPropertiesBuilder(
                    "Msal.UserTokenCache",
                    AppConstants.FilesFolderPath)
                .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(tokenCacheProperties);
            cacheHelper.RegisterCache(authenticationClient.UserTokenCache);

            await LoginAsync(authenticationClient, cancellationToken);

            return new InteractiveAuthenticationProvider(authenticationClient, Scopes);
        }

        private async Task LoginAsync(IPublicClientApplication authenticationClient, CancellationToken cancellationToken)
        {
            try
            {
                var accounts = (await authenticationClient.GetAccountsAsync()).ToList();

                if (accounts.Any())
                {
                    await authenticationClient
                        .AcquireTokenSilent(Scopes, accounts.First())
                        .WithForceRefresh(true)
                        .ExecuteAsync(cancellationToken);

                    return;
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to silently acquire token");
            }

            await authenticationClient
                .AcquireTokenInteractive(Scopes)
                .ExecuteAsync(cancellationToken);
        }
    }
}