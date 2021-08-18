using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Movington.PhotoTransfer.GooglePhotos;
using Movington.PhotoTransfer.GooglePhotos.ApiModels;
using Movington.PhotoTransfer.Helpers;
using Movington.PhotoTransfer.ImageProcessing;
using Movington.PhotoTransfer.Pipeline.Helpers;

namespace Movington.PhotoTransfer.Pipeline.Steps
{
    public class ContentUploadingStep : PipelineStep
    {
        private readonly int _maxParallelUploads;

        private readonly Throttler _throttler;
        private readonly GooglePhotosClient _googlePhotosClient;

        public ContentUploadingStep(
            int maxParallelUploads,
            Throttler throttler,
            GooglePhotosClient googlePhotosClient)
        {
            _maxParallelUploads = maxParallelUploads;

            _throttler = throttler;
            _googlePhotosClient = googlePhotosClient;
        }

        public override IPropagatorBlock<PipelineItem<UploadableMedia>, PipelineItem<MediaItemContainer>> ToBlock(CancellationToken cancellationToken)
            => new TransformBlock<PipelineItem<UploadableMedia>, PipelineItem<MediaItemContainer>>(x => ExecuteAsync(x, cancellationToken), Options);

        private ExecutionDataflowBlockOptions Options => new ExecutionDataflowBlockOptions
        {
            EnsureOrdered = false,
            MaxDegreeOfParallelism = _maxParallelUploads,
            BoundedCapacity = _maxParallelUploads
        };

        private async Task<PipelineItem<MediaItemContainer>> ExecuteAsync(
            PipelineItem<UploadableMedia> pipelineItem, 
            CancellationToken cancellationToken)
        {
            await _throttler.NextDelayAsync();

            pipelineItem.OnContentUploadStarted();

            var mediaItem = await _googlePhotosClient.UploadMediaContentAsync(pipelineItem.Payload, cancellationToken);

            pipelineItem.OnContentUploadFinished();

            return pipelineItem.WithPayload(mediaItem);
        }
    }
}