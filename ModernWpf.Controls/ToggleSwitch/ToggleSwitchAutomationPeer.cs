using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class ToggleSwitchAutomationPeer : FrameworkElementAutomationPeer, IToggleProvider
    {
        public ToggleSwitchAutomationPeer(ToggleSwitch owner) : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Toggle)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(ToggleSwitch);
        }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (string.IsNullOrEmpty(name))
            {
                var owner = GetImpl();

                var header = GetStringFromObject(owner.Header);
                if (!string.IsNullOrEmpty(header))
                {
                    name = header;
                }

                var content = GetStringFromObject(GetOnOffContentForName(owner));
                if (!string.IsNullOrEmpty(content))
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        name += " ";
                    }

                    name += content;
                }
            }

            return name ?? string.Empty;
        }

        protected override Point GetClickablePointCore()
        {
            var clickableElement = GetImpl().GetAutomationClickableElement();
            if (!ReferenceEquals(clickableElement, Owner))
            {
                var peer = UIElementAutomationPeer.FromElement(clickableElement) ??
                           UIElementAutomationPeer.CreatePeerForElement(clickableElement);
                if (peer != null)
                {
                    return peer.GetClickablePoint();
                }
            }

            return base.GetClickablePointCore();
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        protected override System.Collections.Generic.List<AutomationPeer> GetChildrenCore()
        {
            return null;
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return "toggle switch";
        }

        public ToggleState ToggleState => GetImpl().IsOn ? ToggleState.On : ToggleState.Off;

        public void Toggle()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            GetImpl().Toggle();
        }

        private ToggleSwitch GetImpl()
        {
            return (ToggleSwitch)Owner;
        }

        private static object GetOnOffContentForName(ToggleSwitch owner)
        {
            var contentProperty = owner.IsOn
                ? ToggleSwitch.OnContentProperty
                : ToggleSwitch.OffContentProperty;

            return HasCustomValue(owner, contentProperty)
                ? owner.GetValue(contentProperty)
                : null;
        }

        private static bool HasCustomValue(DependencyObject owner, DependencyProperty property)
        {
            return DependencyPropertyHelper.GetValueSource(owner, property).BaseValueSource != BaseValueSource.Default;
        }

        private static string GetStringFromObject(object value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case string text:
                    return text;
                case TextBlock textBlock:
                    return textBlock.Text ?? string.Empty;
                case AccessText accessText:
                    return accessText.Text ?? string.Empty;
                case ContentControl contentControl:
                    return GetStringFromObject(contentControl.Content);
                case FrameworkElement:
                    return string.Empty;
                default:
                    return GetPropertyValueString(value);
            }
        }

        private static string GetPropertyValueString(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return ((byte)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int16:
                    return ((short)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.UInt16:
                    return ((ushort)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int32:
                    return ((int)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.UInt32:
                    return ((uint)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Int64:
                    return ((long)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.UInt64:
                    return ((ulong)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Single:
                    return ((float)value).ToString("G", CultureInfo.InvariantCulture);
                case TypeCode.Double:
                    return ((double)value).ToString("G", CultureInfo.InvariantCulture);
                case TypeCode.Char:
                    return ((char)value).ToString();
                case TypeCode.Boolean:
                    return (bool)value ? "1" : "0";
                case TypeCode.String:
                    return (string)value;
            }

            return value is Guid guid
                ? guid.ToString("B").ToUpperInvariant()
                : string.Empty;
        }
    }
}
