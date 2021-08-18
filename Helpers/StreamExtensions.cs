using System.IO;

namespace Movington.PhotoTransfer.Helpers
{
    public static class StreamExtensions
    {
        public static void Rewind(this Stream source)
            => source.Seek(0, SeekOrigin.Begin);
    }
}