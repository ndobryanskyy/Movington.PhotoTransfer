using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;

namespace Movington.PhotoTransfer.OneDrive
{
    public sealed class OneDriveAuthenticationOptions
    {
        [Required]
        public string ClientId { get; set; } = default!;

        [Required]
        public AzureCloudInstance CloudInstance { get; set; } = AzureCloudInstance.AzurePublic;

        [Required]
        public string Tenant { get; set; } = "common";
    }
}