using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Movington.PhotoTransfer.OneDrive;
using Movington.PhotoTransfer.Pipeline.Helpers;

namespace Movington.PhotoTransfer.Pipeline
{
    public sealed class TransferPipeline
    {
        private readonly ILogger<TransferPipeline> _logger;
        private readonly OneDrivePhotosService _oneDrivePhotosService;
        private readonly PipelineStepsFactory _stepsFactory;

        private readonly TransferPipelineOptions _pipelineOptions;

        public TransferPipeline(
            ILogger<TransferPipeline> logger,
            IOptions<TransferPipelineOptions> pipelineOptions,
            OneDrivePhotosService oneDrivePhotosService,
            PipelineStepsFactory stepsFactory)
        {
            _logger = logger;
            _oneDrivePhotosService = oneDrivePhotosService;
            _stepsFactory = stepsFactory;

            _pipelineOptions = pipelineOptions.Value;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var transferPipeline = BuildDataflowPipeline(stoppingToken);

            _logger.LogInformation("Transferring started with {@Options}", _pipelineOptions);

            var itemNumber = 0;
            var cameraRollItems = _oneDrivePhotosService.GetAllCameraRollItemsAsync(_pipelineOptions.OneDrivePageSize, stoppingToken);

            try
            {
                await foreach (var driveItem in cameraRollItems)
                {
                    itemNumber++;

                    var pipelineItem = new PipelineItem<DriveItem>(itemNumber, driveItem);
                    pipelineItem.OnCreated(driveItem);

                    if (driveItem.IsPhotoItem() == false)
                    {
                        pipelineItem.OnItemSkipped();
                        continue;
                    }

                    var itemAccepted = await transferPipeline.SendAsync(pipelineItem, stoppingToken);
                    if (!itemAccepted)
                    {
                        _logger.LogCritical("Item {PipelineItemNumber} was rejected, terminating...", itemNumber);
                        break;
                    }
                }

                await transferPipeline.CompleteAsync();

                _logger.LogInformation("Transferring succeeded!");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(
                    exception,
                    "Pipeline stopped unexpectedly on item number {LastItemNumber}",
                    itemNumber);
            }
        }

        private DataflowPipeline<PipelineItem<DriveItem>> BuildDataflowPipeline(CancellationToken stoppingToken)
        {
            var downloadBlock = _stepsFactory.CreateDownloadStep().ToBlock(stoppingToken);
            var mediaProcessorBlock = _stepsFactory.CreateMediaProcessorStep().ToBlock(stoppingToken);
            var contentUploadingBlock = _stepsFactory.CreateContentUploadingStep().ToBlock(stoppingToken);
            var mediaItemsBatchingBlock = _stepsFactory.CreateMediaItemsBatchingStep().ToBlock(stoppingToken);
            var mediaAddingBlock = _stepsFactory.CreateMediaAddingStep().ToBlock(stoppingToken);

            var propagatingLinkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            downloadBlock.LinkTo(mediaProcessorBlock, propagatingLinkOptions);
            mediaProcessorBlock.LinkTo(contentUploadingBlock, propagatingLinkOptions);
            contentUploadingBlock.LinkTo(mediaItemsBatchingBlock, propagatingLinkOptions);
            mediaItemsBatchingBlock.LinkTo(mediaAddingBlock, propagatingLinkOptions);

            return new DataflowPipeline<PipelineItem<DriveItem>>(downloadBlock, mediaAddingBlock);
        }
    }
}
