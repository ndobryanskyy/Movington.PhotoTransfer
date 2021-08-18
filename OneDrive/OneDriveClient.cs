using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Serilog;

namespace Movington.PhotoTransfer.OneDrive
{
    public sealed class OneDriveClient : IInitializable
    {
        private readonly ILogger _logger;
        private readonly OneDriveAuthenticationHandler _authenticationHandler;

        private GraphServiceClient? _graphClient;

        public OneDriveClient(
            ILogger logger,
            OneDriveAuthenticationHandler authenticationHandler)
        {
            _logger = logger.ForContext<OneDriveClient>();
            _authenticationHandler = authenticationHandler;
        }

        private GraphServiceClient GraphClient 
            => _graphClient ?? throw new InvalidOperationException("Client is not ready");

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Initialization started...");

            var authenticationProvider = await _authenticationHandler.CreateLoggedInAuthenticationProviderAsync(cancellationToken);

            var innerClient = GraphClientFactory.Create(authenticationProvider, finalHandler: new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            });

            _graphClient = new GraphServiceClient(innerClient);

            _logger.Information("Initialization finished");
        }

        public async IAsyncEnumerable<DriveItem> GetAllItemsAsync(
            Func<IDriveRequestBuilder, IDriveItemChildrenCollectionRequest> requestSelector,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pageNumber = 1;

            var firstPageRequest = requestSelector(GraphClient.Me.Drive);
            var firstPage = await ExecutePageRequestAsync(firstPageRequest, pageNumber, cancellationToken);

            _logger.Information("Total OneDrive items count: {TotalDriveItemsCount}", firstPage.GetTotalItemsCount());

            var currentPage = firstPage;
            while (currentPage != null)
            {
                foreach (var item in currentPage)
                {
                    yield return item;
                }

                if (currentPage.NextPageRequest == null)
                {
                    currentPage = null;
                }
                else
                {
                    pageNumber++;
                    currentPage = await ExecutePageRequestAsync(currentPage.NextPageRequest, pageNumber, cancellationToken);
                }
            }
        }

        public Task<Stream> GetItemContentAsync(string driveItemId, CancellationToken cancellationToken) 
            => GraphClient
                .Me
                .Drive
                .Items[driveItemId]
                .Content
                .Request()
                .GetAsync(cancellationToken, HttpCompletionOption.ResponseHeadersRead);

        private async Task<IDriveItemChildrenCollectionPage> ExecutePageRequestAsync(
            IDriveItemChildrenCollectionRequest pageRequest,
            int pageNumber,
            CancellationToken cancellationToken)
        {
            var scopedLogger = _logger
                .ForContext("DrivePageNumber", pageNumber)
                .WithEventName("OnDrivePageLoading");

            scopedLogger.Information("Started loading page...");

            var page = await pageRequest.GetAsync(cancellationToken);

            scopedLogger.Information("Finished loading page");

            return page;
        }
    }
}