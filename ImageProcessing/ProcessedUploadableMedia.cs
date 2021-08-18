using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using Image = SixLabors.ImageSharp.Image;

namespace Movington.PhotoTransfer.ImageProcessing
{
    public sealed class ProcessedUploadableMedia : UploadableMedia
    {
        private readonly Image _processedImage;
        private readonly IImageFormat _format;

        public ProcessedUploadableMedia(
            DriveItem driveItem, 
            Image processedImage,
            IImageFormat format)
            : base(driveItem)
        {
            _processedImage = processedImage;
            _format = format;
        }

        public override ValueTask DisposeAsync()
        {
            _processedImage.Dispose();

            return ValueTask.CompletedTask;
        }

        protected override Task OnUploadAsync(Stream responseStream)
            => _processedImage.SaveAsync(responseStream, _format);
    }
}