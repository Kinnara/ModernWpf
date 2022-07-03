using System;
using System.Diagnostics;
using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class MessageBoxClosingEventArgs : EventArgs
    {
        private MessageBoxClosingDeferral _deferral;
        private int _deferralCount;

        internal MessageBoxClosingEventArgs(MessageBoxResult result)
        {
            Result = result;
        }

        public bool Cancel { get; set; }

        public MessageBoxResult Result { get; }

        public MessageBoxClosingDeferral GetDeferral()
        {
            _deferralCount++;

            return new MessageBoxClosingDeferral(() =>
            {
                DecrementDeferralCount();
            });
        }

        internal void SetDeferral(MessageBoxClosingDeferral deferral)
        {
            _deferral = deferral;
        }

        internal void DecrementDeferralCount()
        {
            Debug.Assert(_deferralCount > 0);
            _deferralCount--;
            if (_deferralCount == 0)
            {
                _deferral.Complete();
            }
        }

        internal void IncrementDeferralCount()
        {
            _deferralCount++;
        }
    }
}
