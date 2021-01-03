using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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

                var header = owner.Header?.ToString();
                if (!string.IsNullOrEmpty(header))
                {
                    name = header;
                }

                var content = (owner.IsOn ? owner.OnContent : owner.OffContent)?.ToString();
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

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
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
    }
}
