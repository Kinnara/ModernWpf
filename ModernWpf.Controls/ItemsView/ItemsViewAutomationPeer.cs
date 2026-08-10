// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Automation.Peers
{
    public class ItemsViewAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        public ItemsViewAutomationPeer(ItemsView owner)
            : base(owner)
        {
        }

        public bool CanSelectMultiple =>
            OwnerItemsView.SelectionMode == ItemsViewSelectionMode.Multiple ||
            OwnerItemsView.SelectionMode == ItemsViewSelectionMode.Extended;

        public bool IsSelectionRequired => false;

        public IRawElementProviderSimple[] GetSelection()
        {
            var providers = new List<IRawElementProviderSimple>();
            foreach (IndexPath selectedIndex in OwnerItemsView.SelectedIndices)
            {
                if (selectedIndex.GetSize() != 1)
                {
                    continue;
                }

                ItemContainer itemContainer = OwnerItemsView.GetRealizedItemContainer(selectedIndex.GetAt(0));
                if (itemContainer == null)
                {
                    continue;
                }

                AutomationPeer peer = CreatePeerForElement(itemContainer) ??
                    new ItemContainerAutomationPeer(itemContainer);
                providers.Add(ProviderFromPeer(peer));
            }

            return providers.ToArray();
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection &&
                OwnerItemsView.SelectionMode != ItemsViewSelectionMode.None)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        internal void RaiseSelectionChanged()
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated))
            {
                RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
            }
        }

        protected override string GetClassNameCore()
        {
            return nameof(ItemsView);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.List;
        }

        private ItemsView OwnerItemsView => (ItemsView)Owner;
    }
}
