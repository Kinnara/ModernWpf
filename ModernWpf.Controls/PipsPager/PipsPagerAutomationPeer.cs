using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace ModernWpf.Controls
{
    internal class PipsPagerAutomationPeer : FrameworkElementAutomationPeer
    {
        public PipsPagerAutomationPeer(PipsPager owner) :
            base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return nameof(PipsPager);
        }

        protected override string GetNameCore()
        {
            string name = base.GetNameCore();

            if (string.IsNullOrEmpty(name))
            {
                if (Owner is PipsPager pipsPager)
                {
                    name = SharedHelpers.TryGetStringRepresentationFromObject(pipsPager.GetValue(AutomationProperties.NameProperty));
                }
            }

            return name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Menu;
        }

        public void RaiseSelectionChanged(double oldIndex, double newIndex)
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                //RaisePropertyChangedEvent(SelectionPatternIdentifiers.SelectionProperty, oldIndex, newIndex);
            }
        }
    }
}
