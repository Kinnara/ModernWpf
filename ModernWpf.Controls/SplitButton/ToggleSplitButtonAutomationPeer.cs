// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class ToggleSplitButtonAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider,
        IToggleProvider
    {
        public ToggleSplitButtonAutomationPeer(ToggleSplitButton owner) : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse ||
                patternInterface == PatternInterface.Toggle)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(ToggleSplitButton);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.SplitButton;
        }

        private ToggleSplitButton GetImpl()
        {
            ToggleSplitButton impl = null;

            if (Owner is ToggleSplitButton splitButton)
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

        public ToggleState ToggleState
        {
            get
            {
                ToggleState state = ToggleState.Off;

                var splitButton = GetImpl();
                if (splitButton != null)
                {
                    if (splitButton.IsChecked)
                    {
                        state = ToggleState.On;
                    }
                }

                return state;
            }
        }

        public void Toggle()
        {
            var splitButton = GetImpl();
            if (splitButton != null)
            {
                splitButton.Toggle();
            }
        }
    }
}
