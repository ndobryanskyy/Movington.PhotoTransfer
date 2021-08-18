using System;
using System.Threading.Tasks;

namespace Movington.PhotoTransfer.Helpers
{
    public sealed class Throttler
    {
        private readonly int _maxDelayMs;
        private readonly Random _rng = new Random();

        public Throttler(int maxDelayMs)
        {
            _maxDelayMs = maxDelayMs;
        }

        public Task NextDelayAsync()
        {
            if (_maxDelayMs <= 0)
            {
                return Task.CompletedTask;
            }

            var nextDelay = TimeSpan.FromMilliseconds(_rng.Next(1, 150));
            return Task.Delay(nextDelay);
        }
    }
}