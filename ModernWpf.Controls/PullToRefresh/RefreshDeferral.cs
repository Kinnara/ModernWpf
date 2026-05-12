using System;
using System.Threading;

namespace ModernWpf.Controls
{
    public sealed class RefreshDeferral
    {
        private readonly Action _completed;
        private int _isCompleted;

        internal RefreshDeferral(Action completed)
        {
            _completed = completed;
        }

        public void Complete()
        {
            if (Interlocked.Exchange(ref _isCompleted, 1) == 0)
            {
                _completed();
            }
        }
    }
}
