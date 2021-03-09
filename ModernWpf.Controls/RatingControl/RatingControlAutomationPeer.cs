// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class RatingControlAutomationPeer :
        FrameworkElementAutomationPeer,
        IValueProvider,
        IRangeValueProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(RatingControl));

        public RatingControlAutomationPeer(RatingControl owner)
            : base(owner)
        {
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_RatingLocalizedControlType);
        }

        // Properties.
        public bool IsReadOnly => GetRatingControl().IsReadOnly;

        string IValueProvider.Value
        {
            get
            {
                double ratingValue = GetRatingControl().Value;
                string valueString;

                string ratingString;

                if (ratingValue == -1)
                {
                    double placeholderValue = GetRatingControl().PlaceholderValue;
                    if (placeholderValue == -1)
                    {
                        valueString = ResourceAccessor.GetLocalizedStringResource(SR_RatingUnset);
                    }
                    else
                    {
                        valueString = GenerateValue_ValueString(ResourceAccessor.GetLocalizedStringResource(SR_CommunityRatingString), placeholderValue);
                    }
                }
                else
                {
                    valueString = GenerateValue_ValueString(ResourceAccessor.GetLocalizedStringResource(SR_BasicRatingString), ratingValue);
                }

                return valueString;
            }
        }

        public void SetValue(string value)
        {
            if (double.TryParse(value, out double potentialRating))
            {
                GetRatingControl().Value = potentialRating;
            }
        }

        // IRangeValueProvider overrides
        public double SmallChange => 1.0;

        public double LargeChange => 1.0;

        public double Maximum => GetRatingControl().MaxRating;

        public double Minimum => 0;

        public double Value
        {
            get
            {
                // Change this to provide a placeholder value too.
                double value = GetRatingControl().Value;
                if (value == -1)
                {
                    return 0;
                }
                else
                {
                    return value;
                }
            }
        }

        public void SetValue(double value)
        {
            GetRatingControl().Value = value;
        }

        //IAutomationPeerOverrides

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value || patternInterface == PatternInterface.RangeValue)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Slider;
        }

        // Protected methods
        internal void RaisePropertyChangedEvent(double newValue)
        {
            // UIA doesn't tolerate a null doubles, so we convert them to zeroes.
            double oldValue = GetRatingControl().Value;

            if (newValue == -1)
            {
                newValue = 0.0;
            }

            RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue.ToString(), newValue.ToString());
            RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);
        }

        // private methods

        RatingControl GetRatingControl()
        {
            UIElement owner = Owner;
            return (RatingControl)owner;
        }

        int DetermineFractionDigits(double value)
        {
            value = value * 100;
            int intValue = (int)value;

            // When reading out the Value_Value, we want clients to read out the least number of digits
            // possible. We don't want a 3 (represented as a double) to be read out as 3.00...
            // Here we determine the number of digits past the decimal point we care about,
            // and this number is used by the caller to truncate the Value_Value string.

            if (intValue % 100 == 0)
            {
                return 0;
            }
            else if (intValue % 10 == 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        /*int DetermineSignificantDigits(double value, int fractionDigits)
        {
            int sigFigsInt = (int)value;
            int length = 0;

            while (sigFigsInt > 0)
            {
                sigFigsInt /= 10;
                length++;
            }

            return length + fractionDigits;
        }*/

        string GenerateValue_ValueString(string resourceString, double ratingValue)
        {
            string maxRatingString = GetRatingControl().MaxRating.ToString();

            int fractionDigits = DetermineFractionDigits(ratingValue);
            //int sigDigits = DetermineSignificantDigits(ratingValue, fractionDigits);
            string ratingString = ratingValue.ToString("F" + fractionDigits);

            return string.Format(resourceString, ratingString, maxRatingString);
        }
    }
}
