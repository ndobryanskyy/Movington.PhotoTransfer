using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Movington.PhotoTransfer.Pipeline.Helpers
{
    public sealed class DataflowPipeline<TSource>
    {
        private readonly ITargetBlock<TSource> _headBlock;
        private readonly IDataflowBlock _terminationBlock;

        public DataflowPipeline(
            ITargetBlock<TSource> headBlock,
            IDataflowBlock terminationBlock)
        {
            _headBlock = headBlock;
            _terminationBlock = terminationBlock;
        }

        public Task<bool> SendAsync(TSource source, CancellationToken cancellationToken)
            => _headBlock.SendAsync(source, cancellationToken);

        public Task CompleteAsync()
        {
            _headBlock.Complete();

            return _terminationBlock.Completion;
        }
    }
}