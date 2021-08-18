using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Graph;
using SixLabors.ImageSharp;

namespace Movington.PhotoTransfer.OneDrive
{
    public static class OneDriveExtensions
    {
        private static readonly HashSet<string> SupportedMimeTypes = Configuration.Default
            .ImageFormatsManager
            .ImageFormats.SelectMany(x => x.MimeTypes)
            .ToHashSet();

        public static IDriveItemRequestBuilder CameraRoll(this IDriveSpecialCollectionRequestBuilder source)
            => source["cameraroll"];

        public static int GetTotalItemsCount(this IDriveItemChildrenCollectionPage response)
        {
            if (response.AdditionalData.TryGetValue("@odata.count", out var oDataCountValue)
                && oDataCountValue is JsonElement oDataCountJsonElement
                && oDataCountJsonElement.TryGetInt32(out var oDataCount))
            {
                return oDataCount;
            }
            else
            {
                throw new InvalidOperationException("Failed to retrieve @odata.count");
            }
        }

        public static bool IsPhotoItem(this DriveItem driveItem)
        {
            var hasFile = driveItem.File != null;
            var hasPhotoOrImage = driveItem.Photo != null || driveItem.Image != null;
            var hasImageMimeType = driveItem.Video == null && SupportedMimeTypes.Contains(driveItem.File?.MimeType ?? "");

            return hasFile && hasPhotoOrImage && hasImageMimeType;
        }
    }
}