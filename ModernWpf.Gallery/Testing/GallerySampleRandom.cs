using System;

namespace ModernWpf.Gallery.Testing
{
    internal sealed class GallerySampleRandom
    {
        private readonly Random _random;
        private readonly bool _useStableSequence;
        private uint _state;

        public GallerySampleRandom()
        {
            _random = new Random();
        }

        public GallerySampleRandom(int stableSeed)
        {
            _useStableSequence = true;
            _state = unchecked((uint)stableSeed);
        }

        public int Next(int minValue, int maxValue)
        {
            if (!_useStableSequence)
            {
                return _random.Next(minValue, maxValue);
            }

            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue));
            }

            return minValue + (int)(NextUInt32() % (uint)(maxValue - minValue));
        }

        public int Next(int maxValue)
        {
            if (!_useStableSequence)
            {
                return _random.Next(maxValue);
            }

            if (maxValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue));
            }

            return (int)(NextUInt32() % (uint)maxValue);
        }

        public double NextDouble()
        {
            if (!_useStableSequence)
            {
                return _random.NextDouble();
            }

            return NextUInt32() / ((double)uint.MaxValue + 1.0);
        }

        private uint NextUInt32()
        {
            _state = unchecked((_state * 1664525u) + 1013904223u);
            return _state;
        }
    }
}
