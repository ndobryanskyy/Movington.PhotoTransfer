using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Movington.PhotoTransfer.GooglePhotos;
using Movington.PhotoTransfer.GooglePhotos.ApiModels;
using Movington.PhotoTransfer.Pipeline.Helpers;

namespace Movington.PhotoTransfer.Pipeline.Steps
{
    public sealed class MediaAddingStep : PipelineStep
    {
        private readonly GooglePhotosClient _googlePhotosClient;

        public MediaAddingStep(GooglePhotosClient googlePhotosClient)
        {
            _googlePhotosClient = googlePhotosClient;
        }

        public override ITargetBlock<PipelineItem<MediaItemContainer>[]> ToBlock(CancellationToken cancellationToken)
            => new ActionBlock<PipelineItem<MediaItemContainer>[]>(x => ExecuteAsync(x, cancellationToken), Options);

        private ExecutionDataflowBlockOptions Options => new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 1,
            EnsureOrdered = false
        };

        private async Task ExecuteAsync(PipelineItem<MediaItemContainer>[] batchedPipelineItems, CancellationToken cancellationToken)
        {
            var mediaItems = batchedPipelineItems.Select(x => x.Payload).ToList();
            var results = await _googlePhotosClient.AddMediaItemsAsync(mediaItems, cancellationToken);

            var correlatedResults = results.Join(
                batchedPipelineItems,
                x => x.Token, x => x.Payload.Item.Token,
                (creationResult, pipelineItem) => (creationResult, pipelineItem));

            foreach (var (creationResult, pipelineItem) in correlatedResults)
            {
                if (creationResult.Error is null)
                {
                    pipelineItem.OnAddedToGoogle();
                }
                else
                {
                    var errorCode = creationResult.Error.ErrorCode ?? -1;
                    var errorMessage = creationResult.Error.Message;

                    pipelineItem.OnAddingToGoogleFailed(errorCode, errorMessage);
                }
            }
        }
    }
}