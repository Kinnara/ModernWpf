using System;
using System.Globalization;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
using ModernWpf.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class ColorPickerSliderAutomationPeer : SliderAutomationPeer, IValueProvider
    {
        public ColorPickerSliderAutomationPeer(ColorPickerSlider owner)
            : base(owner)
        {
        }

        public new ColorPickerSlider Owner => (ColorPickerSlider)base.Owner;

        public bool IsReadOnly => false;

        public string Value
        {
            get
            {
                ColorPicker colorPicker = Owner.GetParentColorPicker();
                return colorPicker == null
                    ? FormatValue(null, Owner.Value)
                    : FormatValue(colorPicker.Color, Owner.Value);
            }
        }

        public void SetValue(string value)
        {
            throw new InvalidOperationException();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (Owner.ColorChannel != ColorPickerHsvChannel.Alpha && patternInterface == PatternInterface.Value)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        internal void RaiseValueChanged(Color oldColor, Color newColor, double oldValue, double newValue)
        {
            RaisePropertyChangedEvent(
                ValuePatternIdentifiers.ValueProperty,
                FormatValue(oldColor, oldValue),
                FormatValue(newColor, newValue));
        }

        private static string FormatValue(Color? color, double value)
        {
            string numericValue = Math.Round(value).ToString("0", CultureInfo.InvariantCulture);
            return color.HasValue
                ? numericValue + ", " + ColorDisplayNameHelper.ToDisplayName(color.Value)
                : numericValue;
        }
    }
}
