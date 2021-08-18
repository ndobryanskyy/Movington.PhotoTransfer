using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Graph;
using Movington.PhotoTransfer.Helpers;
using Movington.PhotoTransfer.OneDrive;
using Movington.PhotoTransfer.Pipeline.Helpers;

namespace Movington.PhotoTransfer.Pipeline.Steps
{
    public sealed class DownloadStep : PipelineStep
    {
        private readonly int _maxParallelDownloads;

        private readonly Throttler _throttler;
        private readonly OneDrivePhotosService _photosService;

        public DownloadStep(
            int maxParallelDownloads,
            Throttler throttler,
            OneDrivePhotosService photosService)
        {
            _maxParallelDownloads = maxParallelDownloads;

            _throttler = throttler;
            _photosService = photosService;
        }

        public override IPropagatorBlock<PipelineItem<DriveItem>, PipelineItem<DriveItem>> ToBlock(CancellationToken cancellationToken) 
            => new TransformBlock<PipelineItem<DriveItem>, PipelineItem<DriveItem>>(x => ExecuteAsync(x, cancellationToken), Options);

        private ExecutionDataflowBlockOptions Options => new ExecutionDataflowBlockOptions
        {
            EnsureOrdered = false,
            BoundedCapacity = _maxParallelDownloads,
            MaxDegreeOfParallelism = _maxParallelDownloads,
            MaxMessagesPerTask = 1
        };

        private async Task<PipelineItem<DriveItem>> ExecuteAsync(PipelineItem<DriveItem> pipelineItem, CancellationToken cancellationToken)
        {
            await _throttler.NextDelayAsync();

            pipelineItem.OnStarted();

            await _photosService.PopulateItemContentAsync(pipelineItem.Payload, cancellationToken);

            pipelineItem.OnPhotoContentReadyForDownload();

            return pipelineItem;
        }
    }
}