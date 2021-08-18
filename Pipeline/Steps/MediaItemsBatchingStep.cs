using System.Threading;
using System.Threading.Tasks.Dataflow;
using Movington.PhotoTransfer.GooglePhotos.ApiModels;
using Movington.PhotoTransfer.Pipeline.Helpers;

namespace Movington.PhotoTransfer.Pipeline.Steps
{
    public sealed class MediaItemsBatchingStep : PipelineStep
    {
        private const int GooglePhotosBatchSize = 50;

        public override IPropagatorBlock<PipelineItem<MediaItemContainer>, PipelineItem<MediaItemContainer>[]> ToBlock(CancellationToken cancellationToken)
            => new BatchBlock<PipelineItem<MediaItemContainer>>(GooglePhotosBatchSize, Options);

        private GroupingDataflowBlockOptions Options => new GroupingDataflowBlockOptions
        {
            EnsureOrdered = false
        };
    }
}