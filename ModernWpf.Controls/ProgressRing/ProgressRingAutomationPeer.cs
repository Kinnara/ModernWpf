// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class ProgressRingAutomationPeer : FrameworkElementAutomationPeer
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(ProgressRing));

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
                    return ResourceAccessor.GetLocalizedStringResource(SR_ProgressRingIndeterminateStatus) + name;
                }
            }
            return name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ProgressBar;
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_ProgressRingName);
        }
    }
}
