using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWpf.Controls
{
    public sealed class TeachingTipClosingEventArgs : EventArgs
    {
        private Deferral m_deferral;
        private int m_deferralCount;

        internal TeachingTipClosingEventArgs(TeachingTipCloseReason reason)
        {
            Reason = reason;
        }

        public bool Cancel { get; set; }

        public TeachingTipCloseReason Reason { get; }

        public Deferral GetDeferral()
        {
            m_deferralCount++;

            return new Deferral(() =>
            {
                DecrementDeferralCount();
            });
        }

        internal void SetDeferral(Deferral deferral)
        {
            m_deferral = deferral;
        }

        internal void DecrementDeferralCount()
        {
            Debug.Assert(m_deferralCount > 0);
            m_deferralCount--;
            if (m_deferralCount == 0)
            {
                m_deferral.Complete();
            }
        }

        internal void IncrementDeferralCount()
        {
            m_deferralCount++;
        }
    }
}
