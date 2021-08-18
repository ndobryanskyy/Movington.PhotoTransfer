using System.ComponentModel.DataAnnotations;

namespace Movington.PhotoTransfer.Pipeline
{
    public sealed class TransferPipelineOptions
    {
        [Range(1, 500)]
        public int OneDrivePageSize { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxParallelDownloads { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxParallelUploads { get; set; }

        /// <summary>
        /// Max throttle delay expressed in milliseconds.
        /// <remarks>When using zero or value below it, delay is turned off</remarks>
        /// </summary>
        public int MaxThrottleDelayMs { get; set; } = 0;
    }
}