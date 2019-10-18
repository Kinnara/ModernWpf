using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace ModernWpf.Automation.Peers
{
    public class RadioButtonsListViewItemAutomationPeer : ListBoxItemWrapperAutomationPeer
    {
        public RadioButtonsListViewItemAutomationPeer(ListBoxItem owner) : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.RadioButton;
        }
    }
}
