using System;
using System.Diagnostics;

namespace ModernWpf.Controls
{
    public sealed class RefreshRequestedEventArgs : EventArgs
    {
        private readonly Action _completed;
        private bool _eventComplete;
        private bool _completedRaised;
        private int _deferralCount;

        internal RefreshRequestedEventArgs(Action completed)
        {
            _completed = completed;
        }

        public RefreshDeferral GetDeferral()
        {
            _deferralCount++;
            return new RefreshDeferral(CompleteDeferral);
        }

        internal void CompleteEvent()
        {
            _eventComplete = true;
            TryComplete();
        }

        internal void IncrementDeferralCount()
        {
            _deferralCount++;
        }

        internal void DecrementDeferralCount()
        {
            Debug.Assert(_deferralCount > 0);
            _deferralCount--;
            TryComplete();
        }

        private void CompleteDeferral()
        {
            Debug.Assert(_deferralCount > 0);
            _deferralCount--;
            TryComplete();
        }

        private void TryComplete()
        {
            if (!_completedRaised && _eventComplete && _deferralCount == 0)
            {
                _completedRaised = true;
                _completed();
            }
        }
    }
}
