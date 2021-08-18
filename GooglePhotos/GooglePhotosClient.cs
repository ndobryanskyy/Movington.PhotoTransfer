using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Http;
using Movington.PhotoTransfer.GooglePhotos.ApiModels;
using Movington.PhotoTransfer.ImageProcessing;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Serilog;

namespace Movington.PhotoTransfer.GooglePhotos
{
    public sealed class GooglePhotosClient : IInitializable
    {
        private const string BaseAddress = "https://photoslibrary.googleapis.com/v1";

        private readonly ILogger _logger;
        private readonly GoogleAuthenticationHandler _authenticationHandler;

        private readonly Lazy<AsyncPolicy<HttpResponseMessage>> _retryPolicyLazy;
        private HttpClient? _client;

        public GooglePhotosClient(
            ILogger logger,
            GoogleAuthenticationHandler authenticationHandler)
        {
            _logger = logger.ForContext<GooglePhotosClient>();
            _authenticationHandler = authenticationHandler;

            _retryPolicyLazy = new Lazy<AsyncPolicy<HttpResponseMessage>>(CreateRetryPolicy);
        }

        private HttpClient Client => _client ?? throw new InvalidOperationException("Client is not ready");

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Initialization started...");

            var credentials = await _authenticationHandler.LoginAsync(cancellationToken);

            var googleHandler = new ConfigurableMessageHandler(new HttpClientHandler())
            {
                Credential = credentials
            };

            _client = new HttpClient(googleHandler);

            _logger.Information("Initialization finished");
        }

        public async Task<MediaItemContainer> UploadMediaContentAsync(
            UploadableMedia media,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseAddress}/uploads")
            {
                Headers =
                {
                    { "X-Goog-Upload-Content-Type", media.MimeType },
                    { "X-Goog-Upload-Protocol", "raw" }
                },
                Content = media.CreateHttpContent()
            };

            using var response = await SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            await media.DisposeAsync();
            var uploadToken = await response.Content.ReadAsStringAsync(cancellationToken);

            return new MediaItemContainer(media.FileName, uploadToken);
        }

        public async Task<IReadOnlyList<MediaItemCreationResult>> AddMediaItemsAsync(IReadOnlyList<MediaItemContainer> mediaItems, CancellationToken cancellationToken)
        {
            using var createResponse = await SendMediaItemsBatchRequestAsync(mediaItems, cancellationToken);

            // TODO: maybe could be moved to outer pipeline
            _logger
                .WithEventName("OnAddPhotosBatch")
                .ForContext("MediaItemsCount", mediaItems.Count)
                .Information("Adding items to google");

            createResponse.EnsureSuccessStatusCode();

            if (createResponse.StatusCode == HttpStatusCode.OK)
            {
                return mediaItems
                    .Select(x => MediaItemCreationResult.Success(x.Item.Token))
                    .ToList();
            }

            var result = await DeserializeBatchCreateResultAsync(createResponse.Content, cancellationToken);

            return result.Items.Select(resultItem =>
            {
                if (resultItem.CreatedItem == null)
                {
                    return MediaItemCreationResult.Failure(resultItem.Token, resultItem.Status);
                }

                return MediaItemCreationResult.Success(resultItem.Token);
            }).ToList();
        }

        private async Task<HttpResponseMessage> SendMediaItemsBatchRequestAsync(
            IReadOnlyList<MediaItemContainer> mediaItems,
            CancellationToken cancellationToken)
        {
            var batchCreateRequest = new BatchCreateMediaItemsRequest(mediaItems);

            var content = JsonContent.Create(batchCreateRequest);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseAddress}/mediaItems:batchCreate")
            {
                Content = content
            };
            
            return await SendAsync(request, cancellationToken);
        }

        private static async Task<BatchCreateMediaItemsResult> DeserializeBatchCreateResultAsync(
            HttpContent createResponseBody,
            CancellationToken cancellationToken)
        {
            await using var bodyStream = await createResponseBody.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<BatchCreateMediaItemsResult>(
                bodyStream,
                cancellationToken: cancellationToken);

            return result ?? throw new InvalidOperationException("Body cannot be null");
        }

        private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _retryPolicyLazy.Value.ExecuteAsync(
                () => Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken));

        private AsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
        {
            var transientErrorsRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));

            var tooManyRequestsRetryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>(x => x.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(15), 2));

            return Policy.WrapAsync(transientErrorsRetryPolicy, tooManyRequestsRetryPolicy);
        }
    }
}