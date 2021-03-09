// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class NavigationViewItemAutomationPeer :
        FrameworkElementAutomationPeer,
        IInvokeProvider,
        ISelectionItemProvider,
        IExpandCollapseProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(NavigationView));

        public NavigationViewItemAutomationPeer(NavigationViewItem owner) :
            base(owner)
        {
        }

        protected override string GetNameCore()
        {
            string returnHString = base.GetNameCore();

            // If a name hasn't been provided by AutomationProperties.Name in markup:
            if (string.IsNullOrEmpty(returnHString))
            {
                if (Owner is NavigationViewItem lvi)
                {
                    returnHString = SharedHelpers.TryGetStringRepresentationFromObject(lvi.Content);
                }
            }

            if (string.IsNullOrEmpty(returnHString))
            {
                // NB: It'll be up to the app to determine the automation label for
                // when they're using a PlaceholderValue vs. Value.

                returnHString = ResourceAccessor.GetLocalizedStringResource(SR_NavigationViewItemDefaultControlName);
            }

            return returnHString;
        }

        public override object GetPattern(PatternInterface pattern)
        {
            if (pattern == PatternInterface.SelectionItem ||
                // Only provide expand collapse pattern if we have children!
                (pattern == PatternInterface.ExpandCollapse && HasChildren()))
            {
                return this;
            }

            return base.GetPattern(pattern);
        }

        protected override string GetClassNameCore()
        {
            return nameof(NavigationViewItem);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            // To be compliant with MAS 4.1.2, in DisplayMode 'Top',
            //  a NavigationViewItem should report itsself as TabItem
            if (IsOnTopNavigation())
            {
                return AutomationControlType.TabItem;
            }
            else
            {
                // TODO: Should this be ListItem in minimal mode and
                // TreeItem otherwise.
                return AutomationControlType.ListItem;
            }
        }

#if NET48_OR_NEWER
        protected override int GetPositionInSetCore()
        {
            int positionInSet = 0;

            if (IsOnTopNavigation() && !IsOnFooterNavigation())
            {
                positionInSet = GetPositionOrSetCountInTopNavHelper(AutomationOutput.Position);
            }
            else
            {
                positionInSet = GetPositionOrSetCountInLeftNavHelper(AutomationOutput.Position);
            }

            return positionInSet;
        }

        protected override int GetSizeOfSetCore()
        {
            int sizeOfSet = 0;

            if (IsOnTopNavigation() && !IsOnFooterNavigation())
            {
                if (GetParentNavigationView() is { } navview)
                {
                    sizeOfSet = GetPositionOrSetCountInTopNavHelper(AutomationOutput.Size);
                }
            }
            else
            {
                sizeOfSet = GetPositionOrSetCountInLeftNavHelper(AutomationOutput.Size);
            }

            return sizeOfSet;
        }
#endif

        void IInvokeProvider.Invoke()
        {
            if (GetParentNavigationView() is { } navView)
            {
                if (Owner is NavigationViewItem navigationViewItem)
                {
                    if (navigationViewItem == navView.SettingsItem)
                    {
                        navView.OnSettingsInvoked();
                    }
                    else
                    {
                        navView.OnNavigationViewItemInvoked(navigationViewItem);
                    }
                }
            }
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                var state = ExpandCollapseState.LeafNode;
                if (Owner is NavigationViewItem navigationViewItem)
                {
                    state = navigationViewItem.IsExpanded ?
                        ExpandCollapseState.Expanded :
                        ExpandCollapseState.Collapsed;
                }

                return state;
            }
        }

        void IExpandCollapseProvider.Collapse()
        {
            if (GetParentNavigationView() is { } navView)
            {
                if (Owner is NavigationViewItem navigationViewItem)
                {
                    navView.Collapse(navigationViewItem);
                    RaiseExpandCollapseAutomationEvent(ExpandCollapseState.Collapsed);
                }
            }
        }

        void IExpandCollapseProvider.Expand()
        {
            if (GetParentNavigationView() is { } navView)
            {
                if (Owner is NavigationViewItem navigationViewItem)
                {
                    navView.Expand(navigationViewItem);
                    RaiseExpandCollapseAutomationEvent(ExpandCollapseState.Expanded);
                }
            }
        }

        internal void RaiseExpandCollapseAutomationEvent(ExpandCollapseState newState)
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                ExpandCollapseState oldState = (newState == ExpandCollapseState.Expanded) ?
                    ExpandCollapseState.Collapsed :
                    ExpandCollapseState.Expanded;

                // box_value(oldState) doesn't work here, use ReferenceWithABIRuntimeClassName to make Narrator can unbox it.
                RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                    oldState,
                    newState);
            }
        }



        NavigationView GetParentNavigationView()
        {
            NavigationView navigationView = null;

            NavigationViewItemBase navigationViewItem = Owner as NavigationViewItemBase;
            if (navigationViewItem != null)
            {
                navigationView = navigationViewItem.GetNavigationView();
            }
            return navigationView;
        }

        int GetNavigationViewItemCountInPrimaryList()
        {
            int count = 0;
            if (GetParentNavigationView() is { } navigationView)
            {
                count = navigationView.GetNavigationViewItemCountInPrimaryList();
            }
            return count;
        }

        int GetNavigationViewItemCountInTopNav()
        {
            int count = 0;
            if (GetParentNavigationView() is { } navigationView)
            {
                count = navigationView.GetNavigationViewItemCountInTopNav();
            }
            return count;
        }

        bool IsSettingsItem()
        {
            if (GetParentNavigationView() is { } navView)
            {
                NavigationViewItem item = Owner as NavigationViewItem;
                var settingsItem = navView.SettingsItem;
                if (item != null && settingsItem != null && (item == settingsItem || item.Content == settingsItem))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsOnTopNavigation()
        {
            var position = GetNavigationViewRepeaterPosition();
            return position != NavigationViewRepeaterPosition.LeftNav && position != NavigationViewRepeaterPosition.LeftFooter;
        }

        internal bool IsOnTopNavigationOverflow()
        {
            return GetNavigationViewRepeaterPosition() == NavigationViewRepeaterPosition.TopOverflow;
        }

        bool IsOnFooterNavigation()
        {
            var position = GetNavigationViewRepeaterPosition();
            return position == NavigationViewRepeaterPosition.LeftFooter || position == NavigationViewRepeaterPosition.TopFooter;
        }

        NavigationViewRepeaterPosition GetNavigationViewRepeaterPosition()
        {
            if (Owner is NavigationViewItemBase navigationViewItem)
            {
                return navigationViewItem.Position;
            }
            return NavigationViewRepeaterPosition.LeftNav;
        }

        ItemsRepeater GetParentItemsRepeater()
        {
            if (GetParentNavigationView() is { } navview)
            {
                if (Owner is NavigationViewItemBase navigationViewItem)
                {
                    return navview.GetParentItemsRepeaterForContainer(navigationViewItem);
                }
            }
            return null;
        }

        // Get either the position or the size of the set for this particular item in the case of left nav. 
        // We go through all the items and then we determine if the listviewitem from the left listview can be a navigation view item header
        // or a navigation view item. If it's the former, we just reset the count. If it's the latter, we increment the counter.
        // In case of calculating the position, if this is the NavigationViewItemAutomationPeer we're iterating through we break the loop.
        int GetPositionOrSetCountInLeftNavHelper(AutomationOutput automationOutput)
        {
            int returnValue = 0;

            if (GetParentItemsRepeater() is { } repeater)
            {
                if (FrameworkElementAutomationPeer.CreatePeerForElement(repeater) is AutomationPeer parent)
                {
                    if (parent.GetChildren() is { } children)
                    {
                        int index = 0;
                        bool itemFound = false;

                        foreach (var child in children)
                        {
                            if (repeater.TryGetElement(index) is { } dependencyObject)
                            {
                                if (dependencyObject is NavigationViewItemHeader)
                                {
                                    if (automationOutput == AutomationOutput.Size && itemFound)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        returnValue = 0;
                                    }
                                }
                                else if (dependencyObject is NavigationViewItem navviewItem)
                                {
                                    if (navviewItem.Visibility == Visibility.Visible)
                                    {
                                        returnValue++;

                                        if (FrameworkElementAutomationPeer.FromElement(navviewItem) == (this))
                                        {
                                            if (automationOutput == AutomationOutput.Position)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                itemFound = true;
                                            }
                                        }
                                    }
                                }
                            }
                            index++;
                        }
                    }
                }
            }

            return returnValue;
        }

        // Get either the position or the size of the set for this particular item in the case of top nav (primary/overflow items). 
        // Basically, we do the same here as GetPositionOrSetCountInLeftNavHelper without dealing with the listview directly, because 
        // TopDataProvider provcides two methods: GetOverflowItems() and GetPrimaryItems(), so we can break the loop (in case of position) by 
        // comparing the value of the FrameworkElementAutomationPeer we can get from the item we're iterating through to this object.
        int GetPositionOrSetCountInTopNavHelper(AutomationOutput automationOutput)
        {
            int returnValue = 0;
            bool itemFound = false;

            if (GetParentItemsRepeater() is { } parentRepeater)
            {
                if (parentRepeater.ItemsSourceView is { } itemsSourceView)
                {
                    var numberOfElements = itemsSourceView.Count;

                    for (int i = 0; i < numberOfElements; i++)
                    {
                        if (parentRepeater.TryGetElement(i) is { } child)
                        {
                            if (child is NavigationViewItemHeader)
                            {
                                if (automationOutput == AutomationOutput.Size && itemFound)
                                {
                                    break;
                                }
                                else
                                {
                                    returnValue = 0;
                                }
                            }
                            else if (child is NavigationViewItem navviewitem)
                            {
                                if (navviewitem.Visibility == Visibility.Visible)
                                {
                                    returnValue++;

                                    if (FrameworkElementAutomationPeer.FromElement(navviewitem) == (this))
                                    {
                                        if (automationOutput == AutomationOutput.Position)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            itemFound = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return returnValue;
        }

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                if (Owner is NavigationViewItem nvi)
                {
                    return nvi.IsSelected;
                }
                return false;
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                if (GetParentNavigationView() is { } navview)
                {
                    if (FrameworkElementAutomationPeer.CreatePeerForElement(navview) is { } peer)
                    {
                        return ProviderFromPeer(peer);
                    }
                }

                return null;
            }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            ChangeSelection(true);
        }

        void ISelectionItemProvider.Select()
        {
            ChangeSelection(true);
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            ChangeSelection(false);
        }

        void ChangeSelection(bool isSelected)
        {
            if (Owner is NavigationViewItem nvi)
            {
                nvi.IsSelected = isSelected;
            }
        }

        bool HasChildren()
        {
            if (Owner is NavigationViewItem navigationViewItem)
            {
                return navigationViewItem.HasChildren();
            }
            return false;
        }

        enum AutomationOutput
        {
            Position,
            Size,
        }
    }
}