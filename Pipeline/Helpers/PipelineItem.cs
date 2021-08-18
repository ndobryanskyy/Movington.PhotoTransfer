using System;
using Microsoft.Graph;
using Serilog;
using Constants = Serilog.Core.Constants;

namespace Movington.PhotoTransfer.Pipeline.Helpers
{
    public sealed class PipelineItem<TPayload> where TPayload : class
    {
        private readonly ILogger _logger;

        public PipelineItem(int itemNumber, TPayload initialPayload)
        {
            _logger = Log
                .ForContext(Constants.SourceContextPropertyName, $"PipelineItem:{itemNumber}");

            Payload = initialPayload;
        }

        private PipelineItem(TPayload  payload, ILogger logger)
        {
            _logger = logger;
            Payload = payload;
        }

        public TPayload  Payload { get; }

        public PipelineItem<TOther> WithPayload<TOther>(TOther payload) where TOther : class 
            => new PipelineItem<TOther>(payload, _logger);

        public void OnCreated(DriveItem driveItem)
        {
            _logger
                .WithMethodEventName()
                .ForContext("DriveItemId", driveItem.Id)
                .ForContext("DriveItemName", driveItem.Name)
                .ForContext("DriveItemSize", driveItem.Size)
                .Information("Pipeline item created");
        }

        public void OnStarted()
        {
            _logger
                .WithMethodEventName()
                .Information("Processing started");
        }

        public void OnItemSkipped()
        {
            _logger
                .WithMethodEventName()
                .Warning("Skipping item");
        }

        public void OnPhotoContentReadyForDownload()
        {
            _logger
                .WithMethodEventName()
                .Information("Photo content ready for download");
        }

        public void OnAddingPhotoMetadata()
        {
            _logger
                .WithMethodEventName()
                .Information("Date taken is missing - using created date");
        }

        public void OnAddingMetadataFailed(Exception exception)
        {
            _logger
                .WithMethodEventName()
                .Warning(exception, "Failed to add metadata to photo. Using content as is");
        }

        public void OnUsingPhotoContentAsIs()
        {
            _logger
                .WithMethodEventName()
                .Information("Using photo content as is");
        }

        public void OnContentUploadStarted()
        {
            _logger
                .WithMethodEventName()
                .Information("Started uploading content to Google Photos");
        }

        public void OnContentUploadFinished()
        {
            _logger
                .WithMethodEventName()
                .Information("Finished uploading content to Google Photos");
        }

        public void OnAddedToGoogle()
        {
            _logger
                .WithMethodEventName()
                .Information("Successfully added to Google Photos");
        }

        public void OnAddingToGoogleFailed(int errorCode, string message)
        {
            _logger
                .WithMethodEventName()
                .ForContext("GoogleErrorCode", errorCode)
                .ForContext("GoogleErrorMessage", message)
                .Warning("Failed adding to Google Photos");
        }
    }
}