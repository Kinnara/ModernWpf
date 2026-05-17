// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class AutoSuggestBoxAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
    {
        public AutoSuggestBoxAutomationPeer(AutoSuggestBox owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(AutoSuggestBox);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        public void Invoke()
        {
            ((AutoSuggestBox)Owner).ProgrammaticSubmitQuery();
        }
    }
}
