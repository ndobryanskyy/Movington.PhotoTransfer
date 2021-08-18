using System.Runtime.CompilerServices;
using Serilog;

namespace Movington.PhotoTransfer
{
    public static class SerilogExtensions
    {
        public static ILogger WithEventName(this ILogger logger, string eventName)
            => logger.ForContext("EventName", eventName);

        public static ILogger WithMethodEventName(this ILogger logger, [CallerMemberName] string? methodName = null)
            => WithEventName(logger, methodName!);
    }
}