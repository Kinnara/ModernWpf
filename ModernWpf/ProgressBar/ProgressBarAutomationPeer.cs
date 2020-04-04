// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ProgressBar = ModernWpf.Controls.ProgressBar;

namespace ModernWpf.Automation.Peers
{
    /// <summary>
    /// Exposes ProgressBar types to Microsoft UI Automation.
    /// </summary>
    public class ProgressBarAutomationPeer : RangeBaseAutomationPeer, IRangeValueProvider
    {
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
                    return Strings.ProgressBarErrorStatus;
                }
                else if (progressBar.ShowPaused)
                {
                    return Strings.ProgressBarPausedStatus;
                }
                else if (progressBar.IsIndeterminate)
                {
                    return Strings.ProgressBarIndeterminateStatus;
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
