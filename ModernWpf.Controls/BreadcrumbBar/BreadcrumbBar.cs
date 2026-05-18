using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = ItemsRepeaterName, Type = typeof(ItemsRepeater))]
    public partial class BreadcrumbBar : Control
    {
        private const string ItemsRepeaterName = "PART_ItemsRepeater";

        static BreadcrumbBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(typeof(BreadcrumbBar)));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            FocusableProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(false));
            IsTabStopProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(false));
        }

        public BreadcrumbBar()
        {
            _itemsRepeaterElementFactory = new BreadcrumbElementFactory();
            _itemsRepeaterLayout = new BreadcrumbLayout(this);
        }

        public event TypedEventHandler<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs> ItemClicked;

        public override void OnApplyTemplate()
        {
            RevokeItemsRepeaterListeners();

            base.OnApplyTemplate();

            _itemsRepeater = GetTemplateChild(ItemsRepeaterName) as ItemsRepeater;

            if (_itemsRepeater != null)
            {
                _itemsRepeater.Layout = _itemsRepeaterLayout;
                _itemsRepeater.ItemsSource = Array.Empty<object>();
                _itemsRepeater.ItemTemplate = _itemsRepeaterElementFactory;
                _itemsRepeater.ElementPrepared += OnElementPrepared;
                _itemsRepeater.ElementIndexChanged += OnElementIndexChanged;
                _itemsRepeater.ElementClearing += OnElementClearing;
                _itemsRepeater.Loaded += OnItemsRepeaterLoaded;
            }

            UpdateItemTemplate();
            UpdateItemsRepeaterItemsSource();
        }

        internal IReadOnlyList<BreadcrumbBarItem> Containers =>
            new ReadOnlyCollection<BreadcrumbBarItem>(GetRealizedBreadcrumbItems().ToList());

        internal BreadcrumbBarItem ContainerFromIndex(int index)
        {
            if (index < 0 || _itemsRepeater == null)
            {
                return null;
            }

            return _itemsRepeater.TryGetElement(index + 1) as BreadcrumbBarItem;
        }

        internal IReadOnlyList<object> HiddenElements()
        {
            if (_itemsRepeater != null &&
                _itemsRepeaterLayout != null &&
                _itemsRepeaterLayout.EllipsisIsRendered)
            {
                return GetHiddenElementsList(_itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis);
            }

            return Array.Empty<object>();
        }

        internal bool MoveFocusFrom(BreadcrumbBarItem item, Key key)
        {
            if (_itemsRepeater == null || item == null)
            {
                return false;
            }

            var focusedIndex = _itemsRepeater.GetElementIndex(item);
            if (focusedIndex < 0)
            {
                return false;
            }

            FocusElementAt(focusedIndex);

            var flowDirectionIsLeftToRight = FlowDirection == FlowDirection.LeftToRight;
            if ((flowDirectionIsLeftToRight && key == Key.Right) ||
                (!flowDirectionIsLeftToRight && key == Key.Left))
            {
                return MoveFocusNext();
            }

            if ((flowDirectionIsLeftToRight && key == Key.Left) ||
                (!flowDirectionIsLeftToRight && key == Key.Right))
            {
                return MoveFocusPrevious();
            }

            return false;
        }

        internal void RaiseItemClickedEvent(object content, int index)
        {
            ItemClicked?.Invoke(this, new BreadcrumbBarItemClickedEventArgs(content, index));
        }

        internal void ReIndexVisibleElementsForAccessibility()
        {
            if (_itemsRepeater == null || _itemsRepeaterLayout == null)
            {
                return;
            }

            var visibleItemsCount = _itemsRepeaterLayout.VisibleItemsCount;
            var firstItemToIndex = 1;

            if (_itemsRepeaterLayout.EllipsisIsRendered)
            {
                firstItemToIndex = _itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis;
            }

            if (_ellipsisBreadcrumbBarItem != null)
            {
                _ellipsisBreadcrumbBarItem.IsHitTestVisible = _itemsRepeaterLayout.EllipsisIsRendered;
            }

#if NET48_OR_NEWER
            for (int accessibilityIndex = 1, itemToIndex = firstItemToIndex;
                accessibilityIndex <= visibleItemsCount;
                accessibilityIndex++, itemToIndex++)
            {
                if (_itemsRepeater.TryGetElement(itemToIndex) is DependencyObject element)
                {
                    AutomationProperties.SetPositionInSet(element, accessibilityIndex);
                    AutomationProperties.SetSizeOfSet(element, visibleItemsCount);
                }
            }
#endif
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new BreadcrumbBarAutomationPeer(this);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == FlowDirectionProperty)
            {
                UpdateBreadcrumbBarItemsFlowDirection();
            }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BreadcrumbBar)d).UpdateItemsRepeaterItemsSource();
        }

        private static void OnItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var breadcrumbBar = (BreadcrumbBar)d;
            breadcrumbBar.UpdateItemTemplate();
            breadcrumbBar.UpdateEllipsisBreadcrumbBarItemDropDownItemTemplate();
        }

        private void RevokeItemsRepeaterListeners()
        {
            if (_itemsRepeater != null)
            {
                _itemsRepeater.ElementPrepared -= OnElementPrepared;
                _itemsRepeater.ElementIndexChanged -= OnElementIndexChanged;
                _itemsRepeater.ElementClearing -= OnElementClearing;
                _itemsRepeater.Loaded -= OnItemsRepeaterLoaded;
            }
        }

        private void OnItemsRepeaterLoaded(object sender, RoutedEventArgs e)
        {
            OnBreadcrumbBarItemsSourceCollectionChanged(null, null);
        }

        private void UpdateItemTemplate()
        {
            _itemsRepeaterElementFactory.UserElementFactory(ItemTemplate);
        }

        private void UpdateEllipsisBreadcrumbBarItemDropDownItemTemplate()
        {
            _ellipsisBreadcrumbBarItem?.SetEllipsisDropDownItemDataTemplate(ItemTemplate);
        }

        private void UpdateItemsRepeaterItemsSource()
        {
            if (_breadcrumbItemsSourceView != null)
            {
                _breadcrumbItemsSourceView.CollectionChanged -= OnBreadcrumbBarItemsSourceCollectionChanged;
                _breadcrumbItemsSourceView = null;
            }

            if (_itemsRepeater == null)
            {
                return;
            }

            if (ItemsSource == null)
            {
                _itemsRepeater.ItemsSource = Array.Empty<object>();
                ResetLastBreadcrumbBarItem();
                _ellipsisBreadcrumbBarItem = null;
                return;
            }

            _breadcrumbItemsSourceView = new ItemsSourceView(ItemsSource);
            _breadcrumbItemsSourceView.CollectionChanged += OnBreadcrumbBarItemsSourceCollectionChanged;
            _itemsRepeater.ItemsSource = new BreadcrumbIterable(ItemsSource);

            ForceUpdateLastElement();
            UpdateBreadcrumbBarItemsFlowDirection();
        }

        private void OnBreadcrumbBarItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_itemsRepeater == null || ItemsSource == null)
            {
                return;
            }

            _itemsRepeater.ItemsSource = new BreadcrumbIterable(ItemsSource);
            ForceUpdateLastElement();
        }

        private void ResetLastBreadcrumbBarItem()
        {
            if (_lastBreadcrumbBarItem != null)
            {
                _lastBreadcrumbBarItem.ResetVisualProperties();
                _lastBreadcrumbBarItem = null;
            }
        }

        private void ForceUpdateLastElement()
        {
            if (_breadcrumbItemsSourceView == null)
            {
                ResetLastBreadcrumbBarItem();
                return;
            }

            var itemCount = _breadcrumbItemsSourceView.Count;
            if (_itemsRepeater?.TryGetElement(itemCount) is BreadcrumbBarItem newLastItem)
            {
                UpdateLastElement(newLastItem);
            }

            if (itemCount == 0)
            {
                ResetLastBreadcrumbBarItem();
            }
        }

        private void UpdateLastElement(BreadcrumbBarItem newLastBreadcrumbBarItem)
        {
            ResetLastBreadcrumbBarItem();

            if (newLastBreadcrumbBarItem != null)
            {
                newLastBreadcrumbBarItem.SetPropertiesForLastItem();
                _lastBreadcrumbBarItem = newLastBreadcrumbBarItem;
            }
        }

        private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is not BreadcrumbBarItem item)
            {
                return;
            }

            item.SetIsEllipsisDropDownItem(false);
            item.SetParentBreadcrumb(this);
            item.SetIndex(args.Index);
            item.FlowDirection = FlowDirection;

            if (args.Index == 0)
            {
                item.SetPropertiesForEllipsisItem();
                _ellipsisBreadcrumbBarItem = item;
                UpdateEllipsisBreadcrumbBarItemDropDownItemTemplate();
                AutomationProperties.SetName(item, "More");
                return;
            }

            if (_breadcrumbItemsSourceView != null)
            {
                var itemCount = _breadcrumbItemsSourceView.Count;
                if (args.Index == itemCount)
                {
                    UpdateLastElement(item);
                }
                else
                {
                    item.ResetVisualProperties();
                }
            }
        }

        private void OnElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            if (_focusedIndex == args.OldIndex)
            {
                if (args.Element is BreadcrumbBarItem item)
                {
                    item.SetIndex(args.NewIndex);
                }

                FocusElementAt(args.NewIndex);
            }
        }

        private void OnElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
        {
            if (args.Element is BreadcrumbBarItem item)
            {
                item.ResetVisualProperties();
            }
        }

        private IReadOnlyList<object> GetHiddenElementsList(int firstShownElement)
        {
            if (_breadcrumbItemsSourceView == null || firstShownElement <= 1)
            {
                return Array.Empty<object>();
            }

            var hiddenElements = new List<object>();
            for (var i = 0; i < firstShownElement - 1 && i < _breadcrumbItemsSourceView.Count; i++)
            {
                hiddenElements.Add(_breadcrumbItemsSourceView.GetAt(i));
            }

            return hiddenElements;
        }

        private void UpdateBreadcrumbBarItemsFlowDirection()
        {
            if (_itemsRepeater == null || _itemsRepeater.ItemsSourceView == null)
            {
                return;
            }

            for (var i = 0; i < _itemsRepeater.ItemsSourceView.Count; i++)
            {
                if (_itemsRepeater.TryGetElement(i) is BreadcrumbBarItem item)
                {
                    item.FlowDirection = FlowDirection;
                }
            }
        }

        private IEnumerable<BreadcrumbBarItem> GetRealizedBreadcrumbItems()
        {
            if (_itemsRepeater == null)
            {
                yield break;
            }

            var items = _itemsRepeater.Children
                .OfType<BreadcrumbBarItem>()
                .Select(item => new { Item = item, Index = _itemsRepeater.GetElementIndex(item) })
                .Where(entry => entry.Index > 0)
                .OrderBy(entry => entry.Index);

            foreach (var entry in items)
            {
                yield return entry.Item;
            }
        }

        private void FocusElementAt(int index)
        {
            if (index >= 0)
            {
                _focusedIndex = index;
            }
        }

        private bool MoveFocus(int indexIncrement)
        {
            if (_itemsRepeater == null || indexIncrement == 0)
            {
                return false;
            }

            if (Keyboard.FocusedElement is not UIElement focusedElement)
            {
                return false;
            }

            var focusedIndex = _itemsRepeater.GetElementIndex(focusedElement);
            if (focusedIndex < 0)
            {
                return false;
            }

            focusedIndex += indexIncrement;
            var itemCount = _itemsRepeater.ItemsSourceView?.Count ?? 0;

            while (focusedIndex >= 0 && focusedIndex < itemCount)
            {
                if (_itemsRepeater.TryGetElement(focusedIndex) is Control item && item.Focus())
                {
                    FocusElementAt(focusedIndex);
                    return true;
                }

                focusedIndex += indexIncrement;
            }

            return false;
        }

        private bool MoveFocusPrevious()
        {
            var movementPrevious = -1;

            if (_focusedIndex == 1)
            {
                movementPrevious = 0;
            }
            else if (_itemsRepeaterLayout.EllipsisIsRendered &&
                _focusedIndex == _itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis)
            {
                movementPrevious = -_focusedIndex;
            }

            return MoveFocus(movementPrevious);
        }

        private bool MoveFocusNext()
        {
            var movementNext = 1;

            if (_focusedIndex == 0)
            {
                movementNext = _itemsRepeaterLayout.FirstRenderedItemIndexAfterEllipsis;
            }

            return MoveFocus(movementNext);
        }

        private ItemsRepeater _itemsRepeater;
        private readonly BreadcrumbElementFactory _itemsRepeaterElementFactory;
        private readonly BreadcrumbLayout _itemsRepeaterLayout;
        private ItemsSourceView _breadcrumbItemsSourceView;
        private BreadcrumbBarItem _ellipsisBreadcrumbBarItem;
        private BreadcrumbBarItem _lastBreadcrumbBarItem;
        private int _focusedIndex = 1;
    }
}
