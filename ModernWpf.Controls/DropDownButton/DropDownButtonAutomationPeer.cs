// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class DropDownButtonAutomationPeer : ButtonAutomationPeer, IExpandCollapseProvider
    {
        public DropDownButtonAutomationPeer(DropDownButton owner) : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(DropDownButton);
        }

        private DropDownButton GetImpl()
        {
            DropDownButton impl = null;

            if (Owner is DropDownButton dropDownButton)
            {
                impl = dropDownButton;
            }

            return impl;
        }

        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                ExpandCollapseState currentState = ExpandCollapseState.Collapsed;

                var dropDownButton = GetImpl();
                if (dropDownButton != null)
                {
                    if (dropDownButton.IsFlyoutOpen)
                    {
                        currentState = ExpandCollapseState.Expanded;
                    }
                }

                return currentState;
            }
        }

        public void Expand()
        {
            var dropDownButton = GetImpl();
            if (dropDownButton != null)
            {
                dropDownButton.OpenFlyout();
            }
        }

        public void Collapse()
        {
            var dropDownButton = GetImpl();
            if (dropDownButton != null)
            {
                dropDownButton.CloseFlyout();
            }
        }
    }
}
