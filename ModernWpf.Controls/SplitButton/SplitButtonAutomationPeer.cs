// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class SplitButtonAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider,
        IInvokeProvider
    {
        public SplitButtonAutomationPeer(SplitButton owner) : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse ||
                patternInterface == PatternInterface.Invoke)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(SplitButton);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.SplitButton;
        }

        private SplitButton GetImpl()
        {
            SplitButton impl = null;

            if (Owner is SplitButton splitButton)
            {
                impl = splitButton;
            }

            return impl;
        }

        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                ExpandCollapseState currentState = ExpandCollapseState.Collapsed;

                var splitButton = GetImpl();
                if (splitButton != null)
                {
                    if (splitButton.IsFlyoutOpen)
                    {
                        currentState = ExpandCollapseState.Expanded;
                    }
                }

                return currentState;
            }
        }

        public void Expand()
        {
            var splitButton = GetImpl();
            if (splitButton != null)
            {
                splitButton.OpenFlyout();
            }
        }

        public void Collapse()
        {
            var splitButton = GetImpl();
            if (splitButton != null)
            {
                splitButton.CloseFlyout();
            }
        }

        public void Invoke()
        {
            var splitButton = GetImpl();
            if (splitButton != null)
            {
                splitButton.OnClickPrimary(null, null);
            }
        }
    }
}
