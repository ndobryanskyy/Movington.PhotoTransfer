using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Movington.PhotoTransfer.OneDrive
{
    public sealed class OneDrivePhotosService
    {
        private readonly OneDriveClient _oneDriveClient;

        public OneDrivePhotosService(OneDriveClient oneDriveClient)
        {
            _oneDriveClient = oneDriveClient;
        }

        public IAsyncEnumerable<DriveItem> GetAllCameraRollItemsAsync(int pageSize, CancellationToken cancellationToken) 
            => _oneDriveClient.GetAllItemsAsync(drive => BuildCameraRollRequest(drive, pageSize), cancellationToken);

        public async Task PopulateItemContentAsync(DriveItem driveItem, CancellationToken cancellationToken)
        {
            var content = await _oneDriveClient.GetItemContentAsync(driveItem.Id, cancellationToken);
            driveItem.Content = content;
        }

        private IDriveItemChildrenCollectionRequest BuildCameraRollRequest(IDriveRequestBuilder drive, int pageSize)
            => drive
                .Special
                .CameraRoll()
                .Children
                .Request()
                .OrderBy(nameof(DriveItem.CreatedDateTime))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.CreatedDateTime,
                    x.Size,
                    x.File,
                    x.Image,
                    x.Photo,
                    x.Video,
                })
                .Top(pageSize);
    }
}