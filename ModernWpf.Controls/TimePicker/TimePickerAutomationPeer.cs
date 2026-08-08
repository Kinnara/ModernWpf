using System.Windows.Automation.Peers;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class TimePickerAutomationPeer : FrameworkElementAutomationPeer
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(TimePicker));

        public TimePickerAutomationPeer(TimePicker owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return nameof(TimePicker);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            if (Owner is TimePicker timePicker && timePicker.Header != null)
            {
                name = timePicker.Header.ToString();
            }

            return string.IsNullOrEmpty(name)
                ? ResourceAccessor.GetLocalizedStringResource(SR_TimePickerAutomationName)
                : name;
        }
    }
}
