using Movington.PhotoTransfer.GooglePhotos.ApiModels;

namespace Movington.PhotoTransfer.GooglePhotos
{
    public sealed class MediaItemCreationResult
    {
        public static MediaItemCreationResult Success(string token)
            => new MediaItemCreationResult(token);

        public static MediaItemCreationResult Failure(string token, BatchCreateMediaItemsResult.Status error)
            => new MediaItemCreationResult(token, error);

        private MediaItemCreationResult(string token, BatchCreateMediaItemsResult.Status? error = null)
        {
            Token = token;
            Error = error;
        }

        public string Token { get; }

        public BatchCreateMediaItemsResult.Status? Error { get; }
    }
}