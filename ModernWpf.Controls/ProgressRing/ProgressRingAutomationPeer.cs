// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class ProgressRingAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
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
                if (progressRing.IsActive && progressRing.IsIndeterminate)
                {
                    var status = ResourceAccessor.GetLocalizedStringResource(SR_ProgressRingIndeterminateStatus);
                    return string.IsNullOrEmpty(name) ? status : status + " " + name;
                }
            }
            return name;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.RangeValue)
            {
                if (Owner is ProgressRing progressRing && !progressRing.IsIndeterminate)
                {
                    return this;
                }

                return null;
            }

            return base.GetPattern(patternInterface);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ProgressBar;
        }

        protected override bool IsControlElementCore()
        {
            return Owner is ProgressRing progressRing && progressRing.IsActive;
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_ProgressRingName);
        }

        bool IRangeValueProvider.IsReadOnly => true;

        double IRangeValueProvider.Minimum => Owner is ProgressRing progressRing ? progressRing.Minimum : 0.0;

        double IRangeValueProvider.Maximum => Owner is ProgressRing progressRing ? progressRing.Maximum : 0.0;

        double IRangeValueProvider.Value => Owner is ProgressRing progressRing ? progressRing.Value : 0.0;

        double IRangeValueProvider.SmallChange => double.NaN;

        double IRangeValueProvider.LargeChange => double.NaN;

        void IRangeValueProvider.SetValue(double value)
        {
            if (Owner is ProgressRing progressRing)
            {
                progressRing.Value = value;
            }
        }
    }
}
