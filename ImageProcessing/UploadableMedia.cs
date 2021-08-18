using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Movington.PhotoTransfer.ImageProcessing
{
    public abstract class UploadableMedia : IAsyncDisposable
    {
        protected UploadableMedia(DriveItem driveItem)
        {
            DriveItem = driveItem;
        }

        protected DriveItem DriveItem { get; }

        public string FileName => DriveItem.Name;

        public string MimeType => DriveItem.File.MimeType;

        public HttpContent CreateHttpContent()
        {
            var mediaType = new MediaTypeHeaderValue("application/octet-stream");

            return new PushStreamContent(async (responseStream, _, _) =>
            {
                using (responseStream)
                {
                    await OnUploadAsync(responseStream);
                }
            }, mediaType);
        }

        protected abstract Task OnUploadAsync(Stream responseStream);

        public abstract ValueTask DisposeAsync();
    }
}