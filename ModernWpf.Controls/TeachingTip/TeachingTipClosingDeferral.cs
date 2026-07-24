using System;

namespace ModernWpf.Controls
{
    public sealed class TeachingTipClosingDeferral
    {
        private readonly Action _completed;
        private bool _isComplete;

        internal TeachingTipClosingDeferral(Action completed)
        {
            _completed = completed ?? throw new ArgumentNullException(nameof(completed));
        }

        public void Complete()
        {
            if (_isComplete)
            {
                return;
            }

            _isComplete = true;
            _completed();
        }
    }
}
