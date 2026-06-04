using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    [TemplatePart(Name = ItemsViewName, Type = typeof(SelectorBarItemsControl))]
    public partial class SelectorBar : Control
    {
        private const string ItemsViewName = "PART_ItemsView";

        static SelectorBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(typeof(SelectorBar)));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            FocusableProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(false));
            IsTabStopProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(false));
        }

        public SelectorBar()
        {
            var items = new ObservableCollection<SelectorBarItem>();
            items.CollectionChanged += OnItemsCollectionChanged;
            SetValue(ItemsPropertyKey, items);

            Loaded += OnLoaded;
        }

        public event TypedEventHandler<SelectorBar, SelectorBarSelectionChangedEventArgs> SelectionChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsView = GetTemplateChild(ItemsViewName) as ItemsControl;
            if (_itemsView != null && _itemsView.ItemsSource == null)
            {
                _itemsView.ItemsSource = Items;
            }

            if (SelectedItem != null)
            {
                ValidateSelectedItem(SelectedItem);
                SyncAllSelectionStates();
            }
        }

        internal bool SelectItem(SelectorBarItem item)
        {
            if (item == null || !item.IsEnabled || !Items.Contains(item))
            {
                return false;
            }

            SelectedItem = item;
            return true;
        }

        internal void OnItemIsSelectedChanged(SelectorBarItem item, bool isSelected)
        {
            if (_updatingSelection)
            {
                return;
            }

            if (isSelected)
            {
                SelectedItem = item;
            }
            else if (ReferenceEquals(SelectedItem, item))
            {
                SelectedItem = null;
            }
        }

        internal bool MoveFocusFrom(SelectorBarItem item, Key key)
        {
            var index = Items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            var flowDirectionIsLeftToRight = FlowDirection == FlowDirection.LeftToRight;
            var delta = 0;
            if ((flowDirectionIsLeftToRight && key == Key.Right) ||
                (!flowDirectionIsLeftToRight && key == Key.Left))
            {
                delta = 1;
            }
            else if ((flowDirectionIsLeftToRight && key == Key.Left) ||
                (!flowDirectionIsLeftToRight && key == Key.Right))
            {
                delta = -1;
            }

            var nextIndex = index + delta;
            while (delta != 0 && nextIndex >= 0 && nextIndex < Items.Count)
            {
                var nextItem = Items[nextIndex];
                if (IsFocusableItem(nextItem) && nextItem.Focus())
                {
                    return true;
                }

                nextIndex += delta;
            }

            return false;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SelectorBarAutomationPeer(this);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            if (SelectedItem == null || !IsFocusableItem(SelectedItem))
            {
                if (e.OriginalSource is SelectorBarItem focusedItem &&
                    Items.Contains(focusedItem) &&
                    IsFocusableItem(focusedItem))
                {
                    SelectedItem = focusedItem;
                }
                else
                {
                    SelectFirstFocusableItem();
                }
            }
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selectorBar = (SelectorBar)d;
            var selectedItem = (SelectorBarItem)e.NewValue;
            selectorBar.ValidateSelectedItem(selectedItem);
            selectorBar.UpdateSelectionStates((SelectorBarItem)e.OldValue, selectedItem);
            selectorBar.SelectionChanged?.Invoke(selectorBar, new SelectorBarSelectionChangedEventArgs());
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SelectorBarItem item in e.OldItems)
                {
                    if (ReferenceEquals(item.Owner, this))
                    {
                        item.Owner = null;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (SelectorBarItem item in e.NewItems)
                {
                    item.Owner = this;
                }
            }

            if (SelectedItem != null && !Items.Contains(SelectedItem))
            {
                SelectedItem = null;
            }
            else if (SelectedItem == null)
            {
                var selectedItem = Items.FirstOrDefault(item => item.IsSelected);
                if (selectedItem != null)
                {
                    SelectedItem = selectedItem;
                }
            }

            SyncAllSelectionStates();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
            {
                UpdateSelectedItemFromItems();
            }
        }

        private void SelectFirstFocusableItem()
        {
            if (SelectedItem != null)
            {
                return;
            }

            foreach (var item in Items)
            {
                if (IsFocusableItem(item))
                {
                    SelectedItem = item;
                    break;
                }
            }
        }

        private void UpdateSelectedItemFromItems()
        {
            var selectedItem = Items.FirstOrDefault(item => item.IsSelected);
            if (selectedItem != null)
            {
                SelectedItem = selectedItem;
            }
        }

        private void ValidateSelectedItem(SelectorBarItem selectedItem)
        {
            if (selectedItem != null && !Items.Contains(selectedItem))
            {
                throw new System.ArgumentException("SelectedItem must be an element of Items.", nameof(SelectedItem));
            }
        }

        private static bool IsFocusableItem(SelectorBarItem item)
        {
            return item != null &&
                item.IsEnabled &&
                item.Visibility == Visibility.Visible &&
                item.Focusable;
        }

        private void UpdateSelectionStates(SelectorBarItem oldItem, SelectorBarItem newItem)
        {
            _updatingSelection = true;
            try
            {
                if (oldItem != null && !ReferenceEquals(oldItem, newItem))
                {
                    oldItem.IsSelected = false;
                }

                if (newItem != null)
                {
                    newItem.IsSelected = true;
                }
            }
            finally
            {
                _updatingSelection = false;
            }
        }

        private void SyncAllSelectionStates()
        {
            _updatingSelection = true;
            try
            {
                foreach (var item in Items)
                {
                    item.IsSelected = ReferenceEquals(item, SelectedItem);
                    item.Owner = this;
                }
            }
            finally
            {
                _updatingSelection = false;
            }
        }

        private ItemsControl _itemsView;
        private bool _updatingSelection;
    }
}
