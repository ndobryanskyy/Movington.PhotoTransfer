using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Movington.PhotoTransfer.GooglePhotos.ApiModels
{
    public sealed class BatchCreateMediaItemsResult
    {
        [JsonPropertyName("newMediaItemResults")]
        public IReadOnlyList<NewMediaItemResult> Items { get; set; } = new List<NewMediaItemResult>();

        public sealed class NewMediaItemResult
        {
            [JsonPropertyName("uploadToken")]
            public string Token { get; set; } = default!;

            [JsonPropertyName("status")]
            public Status Status { get; set; } = default!;

            [JsonPropertyName("mediaItem")]
            public MediaItem? CreatedItem { get; set; }
        }

        public sealed class MediaItem
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = default!;
        }

        public sealed class Status
        {
            [JsonPropertyName("code")]
            public int? ErrorCode { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; } = default!;
        }
    }
}