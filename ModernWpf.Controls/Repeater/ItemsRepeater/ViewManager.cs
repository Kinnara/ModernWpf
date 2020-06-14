// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace ModernWpf.Controls
{
    // Manages elements on behalf of ItemsRepeater.
    // ViewManager automatically pins focused elements.
    internal class ViewManager
    {
        public ViewManager(ItemsRepeater owner)
        {
            m_owner = owner;
            m_resetPool = new UniqueIdElementPool(owner);
            // ItemsRepeater is not fully constructed yet. Don't interact with it.
        }

        public UIElement GetElement(int index, bool forceCreate, bool suppressAutoRecycle)
        {
            UIElement element = forceCreate ? null : GetElementIfAlreadyHeldByLayout(index);
            if (element == null)
            {
                // check if this is the anchor made through repeater in preparation 
                // for a bring into view.
                if (m_owner.MadeAnchor is UIElement madeAnchor)
                {
                    var anchorVirtInfo = ItemsRepeater.TryGetVirtualizationInfo(madeAnchor);
                    if (anchorVirtInfo.Index == index)
                    {
                        element = madeAnchor;
                    }
                }
            }
            if (element == null) { element = GetElementFromUniqueIdResetPool(index); };
            if (element == null) { element = GetElementFromPinnedElements(index); }
            if (element == null) { element = GetElementFromElementFactory(index); }

            var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(element);
            if (suppressAutoRecycle)
            {
                virtInfo.AutoRecycleCandidate = false;
            }
            else
            {
                virtInfo.AutoRecycleCandidate = true;
                virtInfo.KeepAlive = true;
            }

            return element;
        }

        public void ClearElement(UIElement element, bool isClearedDueToCollectionChange)
        {
            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
            int index = virtInfo.Index;
            bool cleared =
                ClearElementToUniqueIdResetPool(element, virtInfo) ||
                ClearElementToAnimator(element, virtInfo) ||
                ClearElementToPinnedPool(element, virtInfo, isClearedDueToCollectionChange);

            if (!cleared)
            {
                ClearElementToElementFactory(element);
            }

            // Both First and Last indices need to be valid or default.
            Debug.Assert((m_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault && m_lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault) ||
                (m_firstRealizedElementIndexHeldByLayout != FirstRealizedElementIndexDefault && m_lastRealizedElementIndexHeldByLayout != LastRealizedElementIndexDefault));

            if (index == m_firstRealizedElementIndexHeldByLayout && index == m_lastRealizedElementIndexHeldByLayout)
            {
                // First and last were pointing to the same element and that is going away.
                InvalidateRealizedIndicesHeldByLayout();
            }
            else if (index == m_firstRealizedElementIndexHeldByLayout)
            {
                // The FirstElement is going away, shrink the range by one.
                ++m_firstRealizedElementIndexHeldByLayout;
            }
            else if (index == m_lastRealizedElementIndexHeldByLayout)
            {
                // Last element is going away, shrink the range by one at the end.
                --m_lastRealizedElementIndexHeldByLayout;
            }
            else
            {
                // Index is either outside the range we are keeping track of or inside the range.
                // In both these cases, we just keep the range we have. If this clear was due to 
                // a collection change, then in the CollectionChanged event, we will invalidate these guys.
            }
        }

        // We need to clear the datacontext to prevent crashes from happening,
        //  however we only do that if we were the ones setting it.
        // That is when one of the following is the case (numbering taken from line ~642):
        // 1.2    No ItemTemplate, data is not a UIElement
        // 2.1    ItemTemplate, data is not FrameworkElement
        // 2.2.2  Itemtemplate, data is FrameworkElement, ElementFactory returned Element different to data
        //
        // In all of those three cases, we the ItemTemplateShim is NOT null.
        // Luckily when we create the items, we store whether we were the once setting the DataContext.
        public void ClearElementToElementFactory(UIElement element)
        {
            m_owner.OnElementClearing(element);

            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
            virtInfo.MoveOwnershipToElementFactory();

            // During creation of this object, we were the one setting the DataContext, so clear it now.
            if (virtInfo.MustClearDataContext)
            {
                if (element is FrameworkElement elementAsFE)
                {
                    elementAsFE.DataContext = null;
                }
            }

            if (m_owner.ItemTemplateShim != null)
            {
                if (m_ElementFactoryRecycleArgs == null)
                {
                    // Create one.
                    m_ElementFactoryRecycleArgs = new ElementFactoryRecycleArgs();
                }

                var context = m_ElementFactoryRecycleArgs;
                context.Element = element;
                context.Parent = m_owner;

                m_owner.ItemTemplateShim.RecycleElement(context);

                context.Element = null;
                context.Parent = null;
            }
            else
            {
                // No ItemTemplate to recycle to, remove the element from the children collection.
                var children = m_owner.Children;
                int childIndex = 0;
                bool found = children.IndexOf(element, out childIndex);
                if (!found)
                {
                    throw new Exception("ItemsRepeater's child not found in its Children collection.");
                }

                children.RemoveAt(childIndex);
            }

            if (m_lastFocusedElement == element)
            {
                // Focused element is going away. Remove the tracked last focused element
                // and pick a reasonable next focus if we can find one within the layout 
                // realized elements.
                int clearedIndex = virtInfo.Index;
                MoveFocusFromClearedIndex(clearedIndex);
            }
        }

        public int GetElementIndex(VirtualizationInfo virtInfo)
        {
            if (virtInfo == null)
            {
                //Element is not a child of this ItemsRepeater.
                return -1;
            }

            return virtInfo.IsRealized || virtInfo.IsInUniqueIdResetPool ? virtInfo.Index : -1;
        }

        public void PrunePinnedElements()
        {
            EnsureEventSubscriptions();

            // Go through pinned elements and make sure they still have
            // a reason to be pinned.
            for (int i = 0; i < m_pinnedPool.Count; ++i)
            {
                var elementInfo = m_pinnedPool[i];
                var virtInfo = elementInfo.VirtualizationInfo;

                Debug.Assert(virtInfo.Owner == ElementOwner.PinnedPool);

                if (!virtInfo.IsPinned)
                {
                    m_pinnedPool.RemoveAt(i);
                    --i;

                    // Pinning was the only thing keeping this element alive.
                    ClearElementToElementFactory(elementInfo.PinnedElement);
                }
            }
        }

        public void UpdatePin(UIElement element, bool addPin)
        {
            var parent = CachedVisualTreeHelpers.GetParent(element);
            DependencyObject child = element;

            while (parent != null)
            {
                if (parent is ItemsRepeater repeater)
                {
                    var virtInfo = ItemsRepeater.GetVirtualizationInfo((UIElement)child);
                    if (virtInfo.IsRealized)
                    {
                        if (addPin)
                        {
                            virtInfo.AddPin();
                        }
                        else if (virtInfo.IsPinned)
                        {
                            if (virtInfo.RemovePin() == 0)
                            {
                                // ElementFactory is invoked during the measure pass.
                                // We will clear the element then.
                                repeater.InvalidateMeasure();
                            }
                        }
                    }
                }

                child = parent;
                parent = CachedVisualTreeHelpers.GetParent(child);
            }
        }

        public void OnItemsSourceChanged(object source, NotifyCollectionChangedEventArgs args)
        {
            // Note: For items that have been removed, the index will not be touched. It will hold
            // the old index before it was removed. It is not valid anymore.
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var newIndex = args.NewStartingIndex;
                        var newCount = args.NewItems.Count;
                        EnsureFirstLastRealizedIndices();
                        if (newIndex <= m_lastRealizedElementIndexHeldByLayout)
                        {
                            m_lastRealizedElementIndexHeldByLayout += newCount;
                            var children = m_owner.Children;
                            var childCount = children.Count;
                            for (int i = 0; i < childCount; ++i)
                            {
                                var element = children[i];
                                var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                                var dataIndex = virtInfo.Index;

                                if (virtInfo.IsRealized && dataIndex >= newIndex)
                                {
                                    UpdateElementIndex(element, virtInfo, dataIndex + newCount);
                                }
                            }
                        }
                        else
                        {
                            // Indices held by layout are not affected
                            // We could still have items in the pinned elements that need updates. This is usually a very small vector.
                            for (int i = 0; i < m_pinnedPool.Count; ++i)
                            {
                                var elementInfo = m_pinnedPool[i];
                                var virtInfo = elementInfo.VirtualizationInfo;
                                var dataIndex = virtInfo.Index;

                                if (virtInfo.IsRealized && dataIndex >= newIndex)
                                {
                                    var element = elementInfo.PinnedElement;
                                    UpdateElementIndex(element, virtInfo, dataIndex + newCount);
                                }
                            }
                        }
                        break;
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        // Requirement: oldStartIndex == newStartIndex. It is not a replace if this is not true.
                        // Two cases here
                        // case 1: oldCount == newCount 
                        //         indices are not affected. nothing to do here.  
                        // case 2: oldCount != newCount
                        //         Replaced with less or more items. This is like an insert or remove
                        //         depending on the counts.
                        var oldStartIndex = args.OldStartingIndex;
                        var newStartingIndex = args.NewStartingIndex;
                        var oldCount = args.OldItems.Count;
                        var newCount = args.NewItems.Count;
                        if (oldStartIndex != newStartingIndex)
                        {
                            throw new Exception("Replace is only allowed with OldStartingIndex equals to NewStartingIndex.");
                        }

                        if (oldCount == 0)
                        {
                            throw new Exception("Replace notification with args.OldItemsCount value of 0 is not allowed. Use Insert action instead.");
                        }

                        if (newCount == 0)
                        {
                            throw new Exception("Replace notification with args.NewItemCount value of 0 is not allowed. Use Remove action instead.");
                        }

                        int countChange = newCount - oldCount;
                        if (countChange != 0)
                        {
                            // countChange > 0 : countChange items were added
                            // countChange < 0 : -countChange  items were removed
                            var children = m_owner.Children;
                            for (int i = 0; i < children.Count; ++i)
                            {
                                var element = children[i];
                                var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                                var dataIndex = virtInfo.Index;

                                if (virtInfo.IsRealized)
                                {
                                    if (dataIndex >= oldStartIndex + oldCount)
                                    {
                                        UpdateElementIndex(element, virtInfo, dataIndex + countChange);
                                    }
                                }
                            }

                            EnsureFirstLastRealizedIndices();
                            m_lastRealizedElementIndexHeldByLayout += countChange;
                        }
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        var oldStartIndex = args.OldStartingIndex;
                        var oldCount = args.OldItems.Count;
                        var children = m_owner.Children;
                        for (int i = 0; i < children.Count; ++i)
                        {
                            var element = children[i];
                            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                            var dataIndex = virtInfo.Index;

                            if (virtInfo.IsRealized)
                            {
                                if (virtInfo.AutoRecycleCandidate && oldStartIndex <= dataIndex && dataIndex < oldStartIndex + oldCount)
                                {
                                    // If we are doing the mapping, remove the element who's data was removed.
                                    m_owner.ClearElementImpl(element);
                                }
                                else if (dataIndex >= (oldStartIndex + oldCount))
                                {
                                    UpdateElementIndex(element, virtInfo, dataIndex - oldCount);
                                }
                            }
                        }

                        InvalidateRealizedIndicesHeldByLayout();
                        break;
                    }

                case NotifyCollectionChangedAction.Reset:
                    {
                        // If we get multiple resets back to back before
                        // running layout, we dont have to clear all the elements again.         
                        if (!m_isDataSourceStableResetPending)
                        {
#if DEBUG
                            // There should be no elements in the reset pool at this time.
                            Debug.Assert(m_resetPool.IsEmpty);
#endif

                            if (m_owner.ItemsSourceView.HasKeyIndexMapping)
                            {
                                m_isDataSourceStableResetPending = true;
                            }

                            // Walk through all the elements and make sure they are cleared, they will go into
                            // the stable id reset pool.
                            var children = m_owner.Children;
                            for (int i = 0; i < children.Count; ++i)
                            {
                                var element = children[i];
                                var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                                if (virtInfo.IsRealized && virtInfo.AutoRecycleCandidate)
                                {
                                    m_owner.ClearElementImpl(element);
                                }
                            }
                        }

                        InvalidateRealizedIndicesHeldByLayout();

                        break;
                    }
            }
        }

        public void OnLayoutChanging()
        {
            if (m_owner.ItemsSourceView != null &&
                m_owner.ItemsSourceView.HasKeyIndexMapping)
            {
                m_isDataSourceStableResetPending = true;
            }
        }

        public void OnOwnerArranged()
        {
            if (m_isDataSourceStableResetPending)
            {
                m_isDataSourceStableResetPending = false;

                foreach (var entry in m_resetPool)
                {
                    // TODO: Task 14204306: ItemsRepeater: Find better focus candidate when focused element is deleted in the ItemsSource.
                    // Focused element is getting cleared. Need to figure out semantics on where
                    // focus should go when the focused element is removed from the data collection.
                    ClearElement(entry.Value, true /* isClearedDueToCollectionChange */);
                }

                m_resetPool.Clear();

                // Flush the realized indices once the stable reset pool is cleared to start fresh.
                InvalidateRealizedIndicesHeldByLayout();
            }
        }

        // We optimize for the case where index is not realized to return null as quickly as we can.
        // Flow layouts manage containers on their own and will never ask for an index that is already realized.
        // If an index that is realized is requested by the layout, we unfortunately have to walk the
        // children. Not ideal, but a reasonable default to provide consistent behavior between virtualizing
        // and non-virtualizing hosts.
        private UIElement GetElementIfAlreadyHeldByLayout(int index)
        {
            UIElement element = null;

            bool cachedFirstLastIndicesInvalid = m_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault;
            Debug.Assert(!cachedFirstLastIndicesInvalid || m_lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault);

            bool isRequestedIndexInRealizedRange = (m_firstRealizedElementIndexHeldByLayout <= index && index <= m_lastRealizedElementIndexHeldByLayout);

            if (cachedFirstLastIndicesInvalid || isRequestedIndexInRealizedRange)
            {
                // Both First and Last indices need to be valid or default.
                Debug.Assert((m_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault && m_lastRealizedElementIndexHeldByLayout == LastRealizedElementIndexDefault) ||
                    (m_firstRealizedElementIndexHeldByLayout != FirstRealizedElementIndexDefault && m_lastRealizedElementIndexHeldByLayout != LastRealizedElementIndexDefault));

                var children = m_owner.Children;
                for (int i = 0; i < children.Count; ++i)
                {
                    var child = children[i];
                    var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(child);
                    if (virtInfo != null && virtInfo.IsHeldByLayout)
                    {
                        // Only give back elements held by layout. If someone else is holding it, they will be served by other methods.
                        int childIndex = virtInfo.Index;
                        m_firstRealizedElementIndexHeldByLayout = Math.Min(m_firstRealizedElementIndexHeldByLayout, childIndex);
                        m_lastRealizedElementIndexHeldByLayout = Math.Max(m_lastRealizedElementIndexHeldByLayout, childIndex);
                        if (virtInfo.Index == index)
                        {
                            element = child;
                            // If we have valid first/last indices, we don't have to walk the rest, but if we 
                            // do not, then we keep walking through the entire children collection to get accurate
                            // indices once.
                            if (!cachedFirstLastIndicesInvalid)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return element;
        }

        private UIElement GetElementFromUniqueIdResetPool(int index)
        {
            UIElement element = null;
            // See if you can get it from the reset pool.
            if (m_isDataSourceStableResetPending)
            {
                element = m_resetPool.Remove(index);
                if (element != null)
                {
                    // Make sure that the index is updated to the current one
                    var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                    virtInfo.MoveOwnershipToLayoutFromUniqueIdResetPool();
                    UpdateElementIndex(element, virtInfo, index);

                    // Update realized indices
                    m_firstRealizedElementIndexHeldByLayout = Math.Min(m_firstRealizedElementIndexHeldByLayout, index);
                    m_lastRealizedElementIndexHeldByLayout = Math.Max(m_lastRealizedElementIndexHeldByLayout, index);
                }
            }

            return element;
        }

        private UIElement GetElementFromPinnedElements(int index)
        {
            UIElement element = null;

            // See if you can find something among the pinned elements.
            for (int i = 0; i < m_pinnedPool.Count; ++i)
            {
                var elementInfo = m_pinnedPool[i];
                var virtInfo = elementInfo.VirtualizationInfo;

                if (virtInfo.Index == index)
                {
                    m_pinnedPool.RemoveAt(i);
                    element = elementInfo.PinnedElement;
                    elementInfo.VirtualizationInfo.MoveOwnershipToLayoutFromPinnedPool();

                    // Update realized indices
                    m_firstRealizedElementIndexHeldByLayout = Math.Min(m_firstRealizedElementIndexHeldByLayout, index);
                    m_lastRealizedElementIndexHeldByLayout = Math.Max(m_lastRealizedElementIndexHeldByLayout, index);
                    break;
                }
            }

            return element;
        }

        private UIElement GetElementFromElementFactory(int index)
        {
            // The view generator is the provider of last resort.
            var data = m_owner.ItemsSourceView.GetAt(index);

            UIElement initElement()
            {
                var providedElementFactory = m_owner.ItemTemplateShim;

                if (providedElementFactory == null)
                {
                    if (data is UIElement dataAsElement)
                    {
                        return dataAsElement;
                    }
                }

                IElementFactoryShim initElementFactory()
                {
                    if (providedElementFactory == null)
                    {
                        var factory = XamlReader.Parse("<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><TextBlock Text='{Binding}'/></DataTemplate>") as DataTemplate;
                        m_owner.ItemTemplate = factory;
                        return m_owner.ItemTemplateShim;
                    }
                    return providedElementFactory;
                }
                var elementFactory = initElementFactory();

                ElementFactoryGetArgs initArgs()
                {
                    if (m_ElementFactoryGetArgs == null)
                    {
                        m_ElementFactoryGetArgs = new ElementFactoryGetArgs();
                    }
                    return m_ElementFactoryGetArgs;
                }
                var args = initArgs();

                try
                {
                    args.Data = data;
                    args.Parent = m_owner;
                    args.Index = index;

                    return elementFactory.GetElement(args);
                }
                finally
                {
                    args.Data = null;
                    args.Parent = null;
                }
            }
            var element = initElement();

            var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(element);
            if (virtInfo == null)
            {
                virtInfo = ItemsRepeater.CreateAndInitializeVirtualizationInfo(element);
            }
            else
            {
                // View obtained from ElementFactory already has a VirtualizationInfo attached to it
                // which means that the element has been recycled and not created from scratch.
            }
            // Clear flag
            virtInfo.MustClearDataContext = false;

            if (data != element)
            {
                if (element is FrameworkElement elementAsFE)
                {
                    // Set data context only if no x:Bind was used. ie. No data template component on the root.
                    // If the passed in data is a UIElement and is different from the element returned by 
                    // the template factory then we need to propagate the DataContext.
                    // Otherwise just set the DataContext on the element as the data.
                    object initElementDataContext()
                    {
                        if (data is FrameworkElement dataAsElement)
                        {
                            var dataDataContext = dataAsElement.DataContext;
                            if (dataDataContext != null)
                            {
                                return dataDataContext;
                            }
                        }
                        return data;
                    }
                    var elementDataContext = initElementDataContext();

                    elementAsFE.DataContext = elementDataContext;
                    virtInfo.MustClearDataContext = true;
                }
                else
                {
                    Debug.Assert(false, "Element returned by factory is not a FrameworkElement!");
                }
            }

            virtInfo.MoveOwnershipToLayoutFromElementFactory(
                index,
                /* uniqueId: */
                m_owner.ItemsSourceView.HasKeyIndexMapping ?
                m_owner.ItemsSourceView.KeyFromIndex(index) :
                        string.Empty);

            // The view generator is the only provider that prepares the element.
            var repeater = m_owner;

            // Add the element to the children collection here before raising OnElementPrepared so 
            // that handlers can walk up the tree in case they want to find their IndexPath in the 
            // nested case.
            var children = repeater.Children;
            if (CachedVisualTreeHelpers.GetParent(element) != repeater)
            {
                children.Add(element);
            }

            repeater.AnimationManager.OnElementPrepared(element);

            repeater.OnElementPrepared(element, index);

            // Update realized indices
            m_firstRealizedElementIndexHeldByLayout = Math.Min(m_firstRealizedElementIndexHeldByLayout, index);
            m_lastRealizedElementIndexHeldByLayout = Math.Max(m_lastRealizedElementIndexHeldByLayout, index);

            return element;
        }

        private bool ClearElementToUniqueIdResetPool(UIElement element, VirtualizationInfo virtInfo)
        {
            if (m_isDataSourceStableResetPending)
            {
                m_resetPool.Add(element);
                virtInfo.MoveOwnershipToUniqueIdResetPoolFromLayout();
            }

            return m_isDataSourceStableResetPending;
        }

        private bool ClearElementToAnimator(UIElement element, VirtualizationInfo virtInfo)
        {
            bool cleared = m_owner.AnimationManager.ClearElement(element);
            if (cleared)
            {
                int clearedIndex = virtInfo.Index;
                virtInfo.MoveOwnershipToAnimator();
                if (m_lastFocusedElement == element)
                {
                    // Focused element is going away. Remove the tracked last focused element
                    // and pick a reasonable next focus if we can find one within the layout 
                    // realized elements.
                    MoveFocusFromClearedIndex(clearedIndex);
                }

            }
            return cleared;
        }

        private bool ClearElementToPinnedPool(UIElement element, VirtualizationInfo virtInfo, bool isClearedDueToCollectionChange)
        {
            bool moveToPinnedPool =
                !isClearedDueToCollectionChange && virtInfo.IsPinned;

            if (moveToPinnedPool)
            {
#if DEBUG
                for (int i = 0; i < m_pinnedPool.Count; ++i)
                {
                    Debug.Assert(m_pinnedPool[i].PinnedElement != element);
                }
#endif
                m_pinnedPool.Add(new PinnedElementInfo(element));
                virtInfo.MoveOwnershipToPinnedPool();
            }

            return moveToPinnedPool;
        }

        private void UpdateFocusedElement()
        {
            UIElement focusedElement = null;

            var child = Keyboard.FocusedElement as DependencyObject;

            if (child != null)
            {
                var parent = CachedVisualTreeHelpers.GetParent(child);
                var owner = m_owner;

                // Find out if the focused element belongs to one of our direct
                // children.
                while (parent != null)
                {
                    var repeater = parent as ItemsRepeater;
                    if (repeater != null)
                    {
                        var element = child as UIElement;
                        if (repeater == owner && ItemsRepeater.GetVirtualizationInfo(element).IsRealized)
                        {
                            focusedElement = element;
                        }

                        break;
                    }

                    child = parent;
                    parent = CachedVisualTreeHelpers.GetParent(child);
                }
            }

            // If the focused element has changed,
            // we need to unpin the old one and pin the new one.
            if (m_lastFocusedElement != focusedElement)
            {
                if (m_lastFocusedElement != null)
                {
                    UpdatePin(m_lastFocusedElement, false /* addPin */);
                }

                if (focusedElement != null)
                {
                    UpdatePin(focusedElement, true /* addPin */);
                }

                m_lastFocusedElement = focusedElement;
            }
        }

        private void OnFocusChanged(object sender, RoutedEventArgs args)
        {
            UpdateFocusedElement();
        }

        private void MoveFocusFromClearedIndex(int clearedIndex)
        {
            UIElement focusedChild = null;
            if (FindFocusCandidate(clearedIndex, ref focusedChild) != null)
            {
                m_lastFocusedElement = focusedChild;
                // Add pin to hold the focused element.
                UpdatePin(focusedChild, true /* addPin */);
            }
            else
            {
                // We could not find a candiate.
                m_lastFocusedElement = null;
            }
        }

        private UIElement FindFocusCandidate(int clearedIndex, ref UIElement focusedChild)
        {
            // Walk through all the children and find elements with index before and after the cleared index.
            // Note that during a delete the next element would now have the same index.
            int previousIndex = int.MinValue;
            int nextIndex = int.MaxValue;
            UIElement nextElement = null;
            UIElement previousElement = null;
            var children = m_owner.Children;
            for (int i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                var virtInfo = ItemsRepeater.TryGetVirtualizationInfo(child);
                if (virtInfo != null && virtInfo.IsHeldByLayout)
                {
                    int currentIndex = virtInfo.Index;
                    if (currentIndex < clearedIndex)
                    {
                        if (currentIndex > previousIndex)
                        {
                            previousIndex = currentIndex;
                            previousElement = child;
                        }
                    }
                    else if (currentIndex >= clearedIndex)
                    {
                        // Note that we use >= above because if we deleted the focused element, 
                        // the next element would have the same index now.
                        if (currentIndex < nextIndex)
                        {
                            nextIndex = currentIndex;
                            nextElement = child;
                        }
                    }
                }
            }

            // Find the next element if one exists, if not use the previous element.
            // If the container itself is not focusable, find a descendent that is.
            UIElement focusCandidate = null;
            if (nextElement != null)
            {
                focusedChild = nextElement as UIElement;
                if (nextElement.Focus())
                {
                    focusCandidate = nextElement;
                }
                else if (nextElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
                {
                    focusCandidate = FocusManager.GetFocusedElement(nextElement) as UIElement;
                }
            }

            if (focusCandidate == null && previousElement != null)
            {
                focusedChild = previousElement as UIElement;
                if (previousElement.Focus())
                {
                    focusCandidate = previousElement;
                }
                else if (previousElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last)))
                {
                    focusCandidate = FocusManager.GetFocusedElement(previousElement) as UIElement;
                }
            }

            return focusCandidate;
        }

        private void EnsureEventSubscriptions()
        {
            if (!m_gotFocus)
            {
                Debug.Assert(!m_lostFocus);
                m_owner.GotFocus += OnFocusChanged;
                m_gotFocus = true;
                m_owner.LostFocus += OnFocusChanged;
                m_lostFocus = true;
            }
        }

        private void UpdateElementIndex(UIElement element, VirtualizationInfo virtInfo, int index)
        {
            var oldIndex = virtInfo.Index;
            if (oldIndex != index)
            {
                virtInfo.UpdateIndex(index);
                m_owner.OnElementIndexChanged(element, oldIndex, index);
            }
        }

        private void InvalidateRealizedIndicesHeldByLayout()
        {
            m_firstRealizedElementIndexHeldByLayout = FirstRealizedElementIndexDefault;
            m_lastRealizedElementIndexHeldByLayout = LastRealizedElementIndexDefault;
        }

        private void EnsureFirstLastRealizedIndices()
        {
            if (m_firstRealizedElementIndexHeldByLayout == FirstRealizedElementIndexDefault)
            {
                // This will ensure that the indexes are updated.
                GetElementIfAlreadyHeldByLayout(0);
            }
        }

        private struct PinnedElementInfo
        {
            public PinnedElementInfo(UIElement element)
            {
                PinnedElement = element;
                VirtualizationInfo = ItemsRepeater.GetVirtualizationInfo(element);
            }

            public UIElement PinnedElement { get; }

            // We hold on VirtualizationInfo to make sure we can
            // quickly access its content rather than go through
            // ItemsRepeater.GetVirtualizationInfo(element) which is
            // slower (assuming it's implemented using attached
            // properties).
            public VirtualizationInfo VirtualizationInfo { get; }
        }

        private readonly ItemsRepeater m_owner;

        // Pinned elements that are currently owned by layout are *NOT* in this pool.
        private readonly List<PinnedElementInfo> m_pinnedPool = new List<PinnedElementInfo>();
        private readonly UniqueIdElementPool m_resetPool;

        // _lastFocusedElement is listed in _pinnedPool.
        // It has to be an element we own (i.e. a direct child).
        private UIElement m_lastFocusedElement;
        private bool m_isDataSourceStableResetPending;

        // Event tokens
        private bool m_gotFocus;
        private bool m_lostFocus;

        // Cached generate/clear contexts to avoid cost of creation every time.
        private ElementFactoryGetArgs m_ElementFactoryGetArgs;
        private ElementFactoryRecycleArgs m_ElementFactoryRecycleArgs;

        // These are first/last indices requested by layout and not cleared yet.
        // These are also not truly first / last because they are a lower / upper bound on the known realized range.
        // For example, if we didn't have the optimization in ElementManager.cpp, m_lastRealizedElementIndexHeldByLayout 
        // will not be accurate. Rather, it will be an upper bound on what we think is the last realized index.
        private int m_firstRealizedElementIndexHeldByLayout = FirstRealizedElementIndexDefault;
        private int m_lastRealizedElementIndexHeldByLayout = LastRealizedElementIndexDefault;
        private const int FirstRealizedElementIndexDefault = int.MaxValue;
        private const int LastRealizedElementIndexDefault = int.MinValue;
    }
}