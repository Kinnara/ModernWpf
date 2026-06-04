using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    public class SelectorBarItemsControl : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SelectorBarItemsControlAutomationPeer(this);
        }
    }

    public class SelectorBarItemsControlAutomationPeer : ItemsControlAutomationPeer
    {
        public SelectorBarItemsControlAutomationPeer(SelectorBarItemsControl owner)
            : base(owner)
        {
        }

        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            if (item is SelectorBarItem)
            {
                return new SelectorBarItemsControlItemAutomationPeer(item, this);
            }

            throw new InvalidOperationException("SelectorBarItemsControl only supports SelectorBarItem containers.");
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.List;
        }
    }

    public class SelectorBarItemsControlItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider
    {
        public SelectorBarItemsControlItemAutomationPeer(object item, SelectorBarItemsControlAutomationPeer itemsControlAutomationPeer)
            : base(item, itemsControlAutomationPeer)
        {
            _item = (SelectorBarItem)item;
        }

        public bool IsSelected => _item.IsSelected;

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                if (_item.Owner == null)
                {
                    return null;
                }

                var peer = FrameworkElementAutomationPeer.CreatePeerForElement(_item.Owner) ?? new SelectorBarAutomationPeer(_item.Owner);
                return ProviderFromPeer(peer);
            }
        }

        public void AddToSelection()
        {
            Select();
        }

        public void RemoveFromSelection()
        {
            if (_item.Owner?.SelectedItem == _item)
            {
                _item.Owner.SelectedItem = null;
            }
        }

        public void Select()
        {
            _item.Select();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.SelectionItem ? this : base.GetPattern(patternInterface);
        }

        protected override string GetAutomationIdCore()
        {
            return AutomationProperties.GetAutomationId(_item);
        }

        protected override string GetClassNameCore()
        {
            return nameof(SelectorBarItem);
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return nameof(SelectorBarItem);
        }

        protected override string GetNameCore()
        {
            var name = AutomationProperties.GetName(_item);
            if (string.IsNullOrEmpty(name))
            {
                name = _item.Text;
            }

            if (string.IsNullOrEmpty(name) && _item.Child != null)
            {
                name = _item.Child.ToString();
            }

            return string.IsNullOrEmpty(name) ? nameof(SelectorBarItem) : name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TabItem;
        }

        private readonly SelectorBarItem _item;
    }
}
