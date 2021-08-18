using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;

namespace Movington.PhotoTransfer.GooglePhotos
{
    public sealed class GoogleAuthenticationHandler
    {
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/photoslibrary.appendonly" };
        
        private readonly GoogleAuthenticationOptions _authenticationOptions;

        public GoogleAuthenticationHandler(IOptions<GoogleAuthenticationOptions> authenticationOptions)
        {
            _authenticationOptions = authenticationOptions.Value;
        }

        public async Task<UserCredential> LoginAsync(CancellationToken cancellationToken)
        {
            var tokenDataStore = new FileDataStore(AppConstants.FilesFolderPath, true);

            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                _authenticationOptions.ToGoogleSecrets(),
                Scopes,
                "user",
                cancellationToken,
                tokenDataStore);
        }
    }
}