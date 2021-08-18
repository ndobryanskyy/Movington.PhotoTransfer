using Microsoft.Extensions.Options;
using Movington.PhotoTransfer.GooglePhotos;
using Movington.PhotoTransfer.Helpers;
using Movington.PhotoTransfer.OneDrive;
using Movington.PhotoTransfer.Pipeline.Steps;

namespace Movington.PhotoTransfer.Pipeline
{
    public sealed class PipelineStepsFactory
    {
        private readonly OneDrivePhotosService _oneDrivePhotosService;
        private readonly GooglePhotosClient _googlePhotosClient;
        private readonly TransferPipelineOptions _pipelineOptions;
        private readonly Throttler _throttler;

        public PipelineStepsFactory(
            IOptions<TransferPipelineOptions> pipelineOptions,
            OneDrivePhotosService oneDrivePhotosService,
            GooglePhotosClient googlePhotosClient)
        {
            _oneDrivePhotosService = oneDrivePhotosService;
            _googlePhotosClient = googlePhotosClient;
            _pipelineOptions = pipelineOptions.Value;

            _throttler = new Throttler(_pipelineOptions.MaxThrottleDelayMs);
        }

        public DownloadStep CreateDownloadStep()
            => new DownloadStep(_pipelineOptions.MaxParallelDownloads, _throttler, _oneDrivePhotosService);

        public MediaProcessorStep CreateMediaProcessorStep()
            => new MediaProcessorStep(_pipelineOptions.MaxParallelDownloads);

        public ContentUploadingStep CreateContentUploadingStep()
            => new ContentUploadingStep(_pipelineOptions.MaxParallelUploads, _throttler, _googlePhotosClient);

        public MediaItemsBatchingStep CreateMediaItemsBatchingStep()
            => new MediaItemsBatchingStep();

        public MediaAddingStep CreateMediaAddingStep()
            => new MediaAddingStep(_googlePhotosClient);
    }
}