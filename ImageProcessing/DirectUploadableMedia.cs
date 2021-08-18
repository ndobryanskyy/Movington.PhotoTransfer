using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Movington.PhotoTransfer.ImageProcessing
{
    public sealed class DirectUploadableMedia : UploadableMedia
    {
        public DirectUploadableMedia(DriveItem driveItem)
            : base(driveItem)
        {
        }

        public override ValueTask DisposeAsync() 
            => DriveItem.Content.DisposeAsync();

        protected override Task OnUploadAsync(Stream responseStream) 
            => DriveItem.Content.CopyToAsync(responseStream);
    }
}