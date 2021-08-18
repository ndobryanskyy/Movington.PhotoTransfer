using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Graph;
using Movington.PhotoTransfer.Helpers;
using Movington.PhotoTransfer.ImageProcessing;
using Movington.PhotoTransfer.Pipeline.Helpers;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Movington.PhotoTransfer.Pipeline.Steps
{
    public sealed class MediaProcessorStep : PipelineStep
    {
        private readonly int _maxParallelDownloads;

        public MediaProcessorStep(int maxParallelDownloads)
        {
            _maxParallelDownloads = maxParallelDownloads;
        }

        public override IPropagatorBlock<PipelineItem<DriveItem>, PipelineItem<UploadableMedia>> ToBlock(CancellationToken cancellationToken)
            => new TransformBlock<PipelineItem<DriveItem>, PipelineItem<UploadableMedia>>(x => ExecuteAsync(x, cancellationToken), Options);

        private ExecutionDataflowBlockOptions Options => new ExecutionDataflowBlockOptions
        {
            EnsureOrdered = false,
            BoundedCapacity = _maxParallelDownloads,
            MaxDegreeOfParallelism = _maxParallelDownloads
        };

        private async Task<PipelineItem<UploadableMedia>> ExecuteAsync(PipelineItem<DriveItem> pipelineItem, CancellationToken cancellationToken)
        {
            var driveItem = pipelineItem.Payload;

            if (driveItem is { Photo: { TakenDateTime: null }, CreatedDateTime: not null })
            {
                pipelineItem.OnAddingPhotoMetadata();

                var bufferedPhotoContent = await LoadContentIntoMemoryAsync(driveItem, cancellationToken);

                try
                {
                    var (image, format) = await Image.LoadWithFormatAsync(Configuration.Default, bufferedPhotoContent, cancellationToken);
                    image.UpdateDateTimeOriginal(driveItem.CreatedDateTime.Value);

                    return pipelineItem.WithPayload<UploadableMedia>(new ProcessedUploadableMedia(driveItem, image, format));
                }
                catch (Exception exception)
                {
                    pipelineItem.OnAddingMetadataFailed(exception);

                    bufferedPhotoContent.Rewind();
                    driveItem.Content = bufferedPhotoContent;

                    return pipelineItem.WithPayload<UploadableMedia>(new DirectUploadableMedia(driveItem));
                }
            }

            pipelineItem.OnUsingPhotoContentAsIs();
            return pipelineItem.WithPayload<UploadableMedia>(new DirectUploadableMedia(driveItem));
        }

        private async Task<MemoryStream> LoadContentIntoMemoryAsync(DriveItem driveItem, CancellationToken cancellationToken)
        {
            await using var photoContent = driveItem.Content;

            var memoryStream = new MemoryStream();
            await photoContent.CopyToAsync(memoryStream, cancellationToken);

            memoryStream.Rewind();

            return memoryStream;
        }
    }
}