using System.Text.Json.Serialization;

namespace Movington.PhotoTransfer.GooglePhotos.ApiModels
{
    public sealed class MediaItemContainer
    {
        public MediaItemContainer(string fileName, string token, string? description = null)
        {
            Description = description;
            Item = new SimpleMediaItem(fileName, token);
        }

        [JsonPropertyName("description")]
        public string? Description { get; }

        [JsonPropertyName("simpleMediaItem")]
        public SimpleMediaItem Item { get; }

        public sealed class SimpleMediaItem
        {
            public SimpleMediaItem(string fileName, string token)
            {
                FileName = fileName;
                Token = token;
            }

            [JsonPropertyName("fileName")]
            public string FileName { get; }

            [JsonPropertyName("uploadToken")]
            public string Token { get; }
        }
    }
}