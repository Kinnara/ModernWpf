// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class ProgressRingAutomationPeer : FrameworkElementAutomationPeer
    {
        public ProgressRingAutomationPeer(ProgressRing owner) : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return nameof(ProgressRing);
        }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (Owner is ProgressRing progressRing)
            {
                if (progressRing.IsActive)
                {
                    return Strings.ProgressRingIndeterminateStatus + name;
                }
            }
            return name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ProgressBar;
        }
    }
}
