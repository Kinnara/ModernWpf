using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    [TemplatePart(Name = ItemsPanelName, Type = typeof(Panel))]
    public class SelectorBar : Control
    {
        private const string ItemsPanelName = "PART_ItemsPanel";

        static SelectorBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(typeof(SelectorBar)));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            FocusableProperty.OverrideMetadata(typeof(SelectorBar), new FrameworkPropertyMetadata(false));
        }

        public SelectorBar()
        {
            var items = new ObservableCollection<SelectorBarItem>();
            items.CollectionChanged += OnItemsCollectionChanged;
            SetValue(ItemsPropertyKey, items);
        }

        private static readonly DependencyPropertyKey ItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Items),
                typeof(ObservableCollection<SelectorBarItem>),
                typeof(SelectorBar),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsProperty = ItemsPropertyKey.DependencyProperty;

        public ObservableCollection<SelectorBarItem> Items => (ObservableCollection<SelectorBarItem>)GetValue(ItemsProperty);

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(SelectorBarItem),
                typeof(SelectorBar),
                new FrameworkPropertyMetadata(null, OnSelectedItemPropertyChanged));

        public SelectorBarItem SelectedItem
        {
            get => (SelectorBarItem)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public event TypedEventHandler<SelectorBar, SelectorBarSelectionChangedEventArgs> SelectionChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsPanel = GetTemplateChild(ItemsPanelName) as Panel;
            RebuildItemsPanel();
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
            if (delta == 0 || nextIndex < 0 || nextIndex >= Items.Count)
            {
                return false;
            }

            return Items[nextIndex].Focus();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SelectorBarAutomationPeer(this);
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
            RebuildItemsPanel();
        }

        private void RebuildItemsPanel()
        {
            if (_itemsPanel == null)
            {
                return;
            }

            _itemsPanel.Children.Clear();

            foreach (var item in Items)
            {
                RemoveFromCurrentParent(item);
                item.Owner = this;
                _itemsPanel.Children.Add(item);
            }
        }

        private static void RemoveFromCurrentParent(SelectorBarItem item)
        {
            if (item.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(item);
            }
        }

        private void ValidateSelectedItem(SelectorBarItem selectedItem)
        {
            if (selectedItem != null && !Items.Contains(selectedItem))
            {
                throw new System.ArgumentException("SelectedItem must be an element of Items.", nameof(SelectedItem));
            }
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

        private Panel _itemsPanel;
        private bool _updatingSelection;
    }
}
