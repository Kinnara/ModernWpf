using System;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf.Automation.Peers;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Controls
{
    internal class SelectorBarItemsControl : ItemsControl
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SelectorBarItemsControlAutomationPeer(this);
        }
    }

    internal class SelectorBarItemsControlAutomationPeer : ItemsControlAutomationPeer, ISelectionProvider
    {
        public SelectorBarItemsControlAutomationPeer(SelectorBarItemsControl owner)
            : base(owner)
        {
            _owner = owner;
        }

        public bool CanSelectMultiple => false;

        public bool IsSelectionRequired => false;

        public IRawElementProviderSimple[] GetSelection()
        {
            var selectedItem = _owner.Items
                .OfType<SelectorBarItem>()
                .FirstOrDefault(item => item.IsSelected);
            if (selectedItem == null)
            {
                return Array.Empty<IRawElementProviderSimple>();
            }

            var children = GetChildren();
            var selectedPeer = children?
                .OfType<SelectorBarItemsControlItemAutomationPeer>()
                .FirstOrDefault(peer => ReferenceEquals(peer.OwnerItem, selectedItem));
            return selectedPeer == null
                ? Array.Empty<IRawElementProviderSimple>()
                : new[] { ProviderFromPeer(selectedPeer) };
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);
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

        protected override string GetClassNameCore()
        {
            return "ItemsView";
        }

        private readonly SelectorBarItemsControl _owner;
    }

    internal class SelectorBarItemsControlItemAutomationPeer : ItemAutomationPeer, ISelectionItemProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(SelectorBar));

        public SelectorBarItemsControlItemAutomationPeer(object item, SelectorBarItemsControlAutomationPeer itemsControlAutomationPeer)
            : base(item, itemsControlAutomationPeer)
        {
            _item = (SelectorBarItem)item;
            _itemsControlAutomationPeer = itemsControlAutomationPeer;
        }

        internal SelectorBarItem OwnerItem => _item;

        public bool IsSelected => _item.IsSelected;

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                return ProviderFromPeer(_itemsControlAutomationPeer);
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
            return GetDefaultControlName();
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

            return string.IsNullOrEmpty(name) ? GetDefaultControlName() : name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ListItem;
        }

        private static string GetDefaultControlName()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_SelectorBarItemDefaultControlName);
        }

        private readonly SelectorBarItem _item;
        private readonly SelectorBarItemsControlAutomationPeer _itemsControlAutomationPeer;
    }
}
