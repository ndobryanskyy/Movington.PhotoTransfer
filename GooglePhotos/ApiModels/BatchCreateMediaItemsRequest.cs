using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Movington.PhotoTransfer.GooglePhotos.ApiModels
{
    public sealed class BatchCreateMediaItemsRequest
    {
        public BatchCreateMediaItemsRequest(IReadOnlyList<MediaItemContainer> items)
        {
            Items = items;
        }

        [JsonPropertyName("newMediaItems")]
        public IReadOnlyList<MediaItemContainer> Items { get; }
    }
}