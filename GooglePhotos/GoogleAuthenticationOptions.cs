using System.ComponentModel.DataAnnotations;
using Google.Apis.Auth.OAuth2;

namespace Movington.PhotoTransfer.GooglePhotos
{
    public sealed class GoogleAuthenticationOptions
    {
        [Required]
        public string ClientId { get; set; } = default!;

        [Required]
        public string ClientSecret { get; set; } = default!;

        public ClientSecrets ToGoogleSecrets()
            => new ClientSecrets
            {
                ClientId = ClientId, 
                ClientSecret = ClientSecret
            };
    }
}