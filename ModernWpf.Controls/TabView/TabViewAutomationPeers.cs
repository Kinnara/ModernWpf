using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class TabViewAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        public TabViewAutomationPeer(TabView owner)
            : base(owner)
        {
        }

        public bool CanSelectMultiple => false;

        public bool IsSelectionRequired => true;

        public IRawElementProviderSimple[] GetSelection()
        {
            var selectedTab = OwnerTabView.SelectedTab;
            if (selectedTab == null)
            {
                return Array.Empty<IRawElementProviderSimple>();
            }

            var peer = CreatePeerForElement(selectedTab) ?? new TabViewItemAutomationPeer(selectedTab);
            return new[] { ProviderFromPeer(peer) };
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Selection ? this : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(TabView);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Tab;
        }

        private TabView OwnerTabView => (TabView)Owner;
    }

    public class TabViewItemAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider, IScrollItemProvider
    {
        public TabViewItemAutomationPeer(TabViewItem owner)
            : base(owner)
        {
        }

        public bool IsSelected => OwnerItem.IsSelected;

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                var owner = OwnerItem.Owner;
                if (owner == null)
                {
                    return null;
                }

                var peer = CreatePeerForElement(owner) ?? new TabViewAutomationPeer(owner);
                return ProviderFromPeer(peer);
            }
        }

        public void AddToSelection()
        {
            Select();
        }

        public void RemoveFromSelection()
        {
            // TabView requires one selected item whenever a selectable item exists.
        }

        public void Select()
        {
            OwnerItem.Owner?.SelectTab(OwnerItem);
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            OwnerItem.BringIntoView();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.SelectionItem || patternInterface == PatternInterface.ScrollItem
                ? this
                : base.GetPattern(patternInterface);
        }

        internal void RaiseIsSelectedChanged(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(
                SelectionItemPatternIdentifiers.IsSelectedProperty,
                oldValue,
                newValue);
            if (newValue)
            {
                RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
            }
        }

        protected override string GetClassNameCore()
        {
            return nameof(TabViewItem);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.TabItem;
        }

        protected override string GetNameCore()
        {
            var name = AutomationProperties.GetName(OwnerItem);
            if (string.IsNullOrEmpty(name))
            {
                name = OwnerItem.Header?.ToString();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = OwnerItem.Content?.ToString();
            }

            return name ?? string.Empty;
        }

        private TabViewItem OwnerItem => (TabViewItem)Owner;
    }
}
