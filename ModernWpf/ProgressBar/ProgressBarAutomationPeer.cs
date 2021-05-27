// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ProgressBar = ModernWpf.Controls.ProgressBar;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    /// <summary>
    /// Exposes ProgressBar types to Microsoft UI Automation.
    /// </summary>
    public class ProgressBarAutomationPeer : RangeBaseAutomationPeer, IRangeValueProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(ProgressBar));

        /// <summary>
        /// Initializes a new instance of the ProgressBarAutomationPeer class.
        /// </summary>
        /// <param name="owner">The ProgressBar to create a peer for.</param>
        public ProgressBarAutomationPeer(ProgressBar owner)
            : base(owner)
        {
        }

        // IAutomationPeerOverrides
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.RangeValue)
            {
                if (Owner is ProgressBar progressBar)
                {
                    if (progressBar.IsIndeterminate)
                    {
                        return null;
                    }
                }
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(ProgressBar);
        }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (Owner is ProgressBar progressBar)
            {
                if (progressBar.ShowError)
                {
                    return ResourceAccessor.GetLocalizedStringResource(SR_ProgressBarErrorStatus);
                }
                else if (progressBar.ShowPaused)
                {
                    return ResourceAccessor.GetLocalizedStringResource(SR_ProgressBarPausedStatus);
                }
                else if (progressBar.IsIndeterminate)
                {
                    return ResourceAccessor.GetLocalizedStringResource(SR_ProgressBarIndeterminateStatus);
                }
            }
            return name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ProgressBar;
        }

        // IRangeValueProvider
        bool IRangeValueProvider.IsReadOnly => true;
        double IRangeValueProvider.SmallChange => double.NaN;
        double IRangeValueProvider.LargeChange => double.NaN;
    }
}
