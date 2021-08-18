using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Movington.PhotoTransfer.Pipeline.Helpers
{
    public abstract class PipelineStep
    {
        public abstract IDataflowBlock ToBlock(CancellationToken cancellationToken);
    }
}