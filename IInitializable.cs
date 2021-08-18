using System.Threading;
using System.Threading.Tasks;

namespace Movington.PhotoTransfer
{
    public interface IInitializable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}