using System;
using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Movington.PhotoTransfer.ImageProcessing
{
    public static class ImageSharpExtensions
    {
        public static void UpdateDateTimeOriginal(this Image image, DateTimeOffset newValue)
        {
            var exifProfile = image.Metadata.ExifProfile ?? new ExifProfile();

            var formattedDateTime = newValue.LocalDateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
            exifProfile.SetValue(ExifTag.DateTimeOriginal, formattedDateTime);

            image.Metadata.ExifProfile = exifProfile;
        }
    }
}