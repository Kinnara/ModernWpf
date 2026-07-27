using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Automation.Peers
{
    public class ColorSpectrumAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public ColorSpectrumAutomationPeer(ColorSpectrum owner)
            : base(owner)
        {
        }

        public new ColorSpectrum Owner => (ColorSpectrum)base.Owner;

        public bool IsReadOnly => false;

        public string Value
        {
            get
            {
                return FormatValue(Owner.Color, Owner.HsvColor);
            }
        }

        public void SetValue(string value)
        {
            object converted = ColorConverter.ConvertFromString(value);
            if (!(converted is Color color))
            {
                throw new ArgumentException("The value could not be converted to a color.", nameof(value));
            }

            Owner.SetCurrentValue(ColorSpectrum.ColorProperty, color);
        }

        internal void RaiseValueChanged(Color oldColor, Color newColor, Vector4 oldHsv, Vector4 newHsv)
        {
            RaisePropertyChangedEvent(
                ValuePatternIdentifiers.ValueProperty,
                FormatValue(oldColor, oldHsv),
                FormatValue(newColor, newHsv));
        }

        protected override string GetClassNameCore()
        {
            return nameof(ColorSpectrum);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Slider;
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "2D slider";
        }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();
            return string.IsNullOrEmpty(name) ? "Color picker" : name;
        }

        protected override string GetHelpTextCore()
        {
            return "2D navigation with arrow keys";
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Value ? this : base.GetPattern(patternInterface);
        }

        private static string FormatValue(Color color, Vector4 hsv)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}, Hue {1}, Saturation {2}, Value {3}",
                ColorDisplayNameHelper.ToDisplayName(color),
                Math.Round(hsv.X),
                Math.Round(hsv.Y * 100),
                Math.Round(hsv.Z * 100));
        }
    }
}
