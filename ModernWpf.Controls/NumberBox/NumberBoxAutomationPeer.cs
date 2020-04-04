// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class NumberBoxAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
    {
        public NumberBoxAutomationPeer(NumberBox owner) : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.RangeValue)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(NumberBox);
        }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (string.IsNullOrEmpty(name))
            {
                if (Owner is NumberBox numberBox)
                {
                    name = numberBox.Header?.ToString();
                }
            }

            return name ?? string.Empty;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Spinner;
        }

        #region IRangeValueProvider

        NumberBox GetImpl()
        {
            NumberBox impl = null;

            if (Owner is NumberBox numberBox)
            {
                impl = numberBox;
            }

            return impl;
        }

        public bool IsReadOnly => false;

        public double Minimum => GetImpl().Minimum;

        public double Maximum => GetImpl().Maximum;

        public double Value => GetImpl().Value;

        public double SmallChange => GetImpl().SmallChange;

        public double LargeChange => GetImpl().LargeChange;

        public void SetValue(double value)
        {
            GetImpl().Value = value;
        }

        public void RaiseValueChangedEvent(double oldValue, double newValue)
        {
            RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty,
                oldValue,
                newValue);
        }

        #endregion
    }
}
