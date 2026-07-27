using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;

using static ModernWpf.Controls.ColorConversion;

namespace ModernWpf.Controls.Primitives
{
    public partial class ColorPickerSlider : Slider
    {
        public ColorPickerSlider()
        {
            _toolTip = new ToolTip
            {
                Placement = PlacementMode.Top,
                PlacementTarget = this,
                StaysOpen = true
            };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateToolTip();
        }

        internal bool AdjustValueForTesting(Key key, bool isControlDown)
        {
            return AdjustValueFromKey(key, isControlDown);
        }

        internal string GetToolTipStringForTesting()
        {
            return GetToolTipString();
        }

        internal ColorPicker GetParentColorPicker()
        {
            DependencyObject current = this;
            while (current != null)
            {
                if (current is ColorPicker colorPicker)
                {
                    return colorPicker;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ColorPickerSliderAutomationPeer(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.Up && e.Key != Key.Down)
            {
                base.OnKeyDown(e);
                return;
            }

            e.Handled = AdjustValueFromKey(e.Key, (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateToolTip();
            _toolTip.IsOpen = true;
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            _toolTip.IsOpen = false;
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            ColorPicker colorPicker = GetParentColorPicker();
            Color oldColor = colorPicker?.Color ?? Colors.Transparent;

            base.OnValueChanged(oldValue, newValue);

            UpdateToolTip();
            if (colorPicker != null &&
                FrameworkElementAutomationPeer.FromElement(this) is ColorPickerSliderAutomationPeer peer)
            {
                peer.RaiseValueChanged(oldColor, colorPicker.Color, oldValue, newValue);
            }
        }

        private bool AdjustValueFromKey(Key key, bool isControlDown)
        {
            ColorPicker colorPicker = GetParentColorPicker();
            if (colorPicker == null)
            {
                return false;
            }

            Hsv currentHsv = colorPicker.GetCurrentHsv();
            double currentAlpha = colorPicker.Color.A / 255.0;
            double minBound;
            double maxBound;

            switch (ColorChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    minBound = colorPicker.MinHue;
                    maxBound = colorPicker.MaxHue;
                    currentHsv.H = Value;
                    break;

                case ColorPickerHsvChannel.Saturation:
                    minBound = colorPicker.MinSaturation;
                    maxBound = colorPicker.MaxSaturation;
                    currentHsv.S = Value / 100.0;
                    break;

                case ColorPickerHsvChannel.Value:
                    minBound = colorPicker.MinValue;
                    maxBound = colorPicker.MaxValue;
                    currentHsv.V = Value / 100.0;
                    break;

                case ColorPickerHsvChannel.Alpha:
                    minBound = 0;
                    maxBound = 100;
                    currentAlpha = Value / 100.0;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported ColorPicker slider channel.");
            }

            bool invertHorizontalDirection = FlowDirection == System.Windows.FlowDirection.RightToLeft && !IsDirectionReversed;
            IncrementDirection direction =
                ((key == Key.Left && !invertHorizontalDirection) ||
                 (key == Key.Right && invertHorizontalDirection) ||
                 key == Key.Down)
                    ? IncrementDirection.Lower
                    : IncrementDirection.Higher;
            IncrementAmount amount = isControlDown ? IncrementAmount.Large : IncrementAmount.Small;

            if (ColorChannel == ColorPickerHsvChannel.Alpha)
            {
                currentAlpha = IncrementAlphaChannel(currentAlpha, direction, amount, false, minBound, maxBound);
                SetCurrentValue(ValueProperty, currentAlpha * 100.0);
                return true;
            }

            currentHsv = IncrementColorChannel(currentHsv, ColorChannel, direction, amount, false, minBound, maxBound);
            switch (ColorChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    SetCurrentValue(ValueProperty, currentHsv.H);
                    break;
                case ColorPickerHsvChannel.Saturation:
                    SetCurrentValue(ValueProperty, currentHsv.S * 100.0);
                    break;
                case ColorPickerHsvChannel.Value:
                    SetCurrentValue(ValueProperty, currentHsv.V * 100.0);
                    break;
            }

            return true;
        }

        private void UpdateToolTip()
        {
            _toolTip.Content = GetToolTipString();
        }

        private string GetToolTipString()
        {
            uint sliderValue = (uint)Math.Round(Value);
            if (ColorChannel == ColorPickerHsvChannel.Alpha)
            {
                return sliderValue + "% opacity";
            }

            string channelName;
            Hsv currentHsv;
            ColorPicker colorPicker = GetParentColorPicker();
            if (colorPicker == null)
            {
                channelName = GetChannelName();
                return channelName + " " + sliderValue;
            }

            currentHsv = colorPicker.GetCurrentHsv();
            switch (ColorChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    currentHsv.H = Value;
                    break;
                case ColorPickerHsvChannel.Saturation:
                    currentHsv.S = Value / 100.0;
                    break;
                case ColorPickerHsvChannel.Value:
                    currentHsv.V = Value / 100.0;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported ColorPicker slider channel.");
            }

            channelName = GetChannelName();
            Color color = ColorFromRgba(HsvToRgb(currentHsv));
            return channelName + " " + sliderValue + " (" + ColorDisplayNameHelper.ToDisplayName(color) + ")";
        }

        private string GetChannelName()
        {
            switch (ColorChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    return "Hue";
                case ColorPickerHsvChannel.Saturation:
                    return "Saturation";
                case ColorPickerHsvChannel.Value:
                    return "Value";
                default:
                    throw new InvalidOperationException("Unsupported ColorPicker slider channel.");
            }
        }

        private readonly ToolTip _toolTip;
    }
}
