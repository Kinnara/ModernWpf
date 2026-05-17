using System;
using System.Numerics;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
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
                Vector4 hsv = Owner.HsvColor;
                return string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Hue {0}, saturation {1}, value {2}",
                    Math.Round(hsv.X),
                    Math.Round(hsv.Y * 100),
                    Math.Round(hsv.Z * 100));
            }
        }

        public void SetValue(string value)
        {
            throw new InvalidOperationException();
        }

        internal void RaiseValueChanged(Color oldColor, Color newColor, Vector4 oldHsv, Vector4 newHsv)
        {
            RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, FormatValue(oldHsv), FormatValue(newHsv));
        }

        protected override string GetClassNameCore()
        {
            return nameof(ColorSpectrum);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "color spectrum";
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Value ? this : base.GetPattern(patternInterface);
        }

        private static string FormatValue(Vector4 hsv)
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "Hue {0}, saturation {1}, value {2}",
                Math.Round(hsv.X),
                Math.Round(hsv.Y * 100),
                Math.Round(hsv.Z * 100));
        }
    }
}
