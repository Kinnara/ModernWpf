// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class HyperlinkButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
    {
        public HyperlinkButtonAutomationPeer(HyperlinkButton owner)
            : base(owner)
        { }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke)
            {
                return this;
            }
            else
            {
                return base.GetPattern(patternInterface);
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Hyperlink;
        }

        protected override string GetClassNameCore()
        {
            return "Hyperlink";
        }

        protected override bool IsControlElementCore()
        {
            return true;
        }

        void IInvokeProvider.Invoke()
        {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            HyperlinkButton owner = (HyperlinkButton)Owner;
            owner.AutomationButtonBaseClick();
        }
    }
}
