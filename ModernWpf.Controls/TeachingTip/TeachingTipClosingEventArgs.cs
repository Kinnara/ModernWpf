using System;

namespace ModernWpf.Controls
{
    public sealed class TeachingTipClosingEventArgs : EventArgs
    {
        private int _deferralCount;

        internal TeachingTipClosingEventArgs(TeachingTipCloseReason reason)
        {
            Reason = reason;
        }

        public TeachingTipCloseReason Reason { get; }

        public bool Cancel { get; set; }

        public TeachingTipClosingDeferral GetDeferral()
        {
            _deferralCount++;
            return new TeachingTipClosingDeferral(OnDeferralCompleted);
        }

        internal bool HasOutstandingDeferrals => _deferralCount > 0;

        internal event EventHandler DeferralsCompleted;

        private void OnDeferralCompleted()
        {
            if (_deferralCount == 0)
            {
                return;
            }

            _deferralCount--;
            if (_deferralCount == 0)
            {
                DeferralsCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
