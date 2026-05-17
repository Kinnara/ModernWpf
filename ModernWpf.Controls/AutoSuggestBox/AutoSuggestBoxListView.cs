using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class AutoSuggestBoxListView : System.Windows.Controls.ListView
    {
        static AutoSuggestBoxListView()
        {
            SelectionModeProperty.OverrideMetadata(typeof(AutoSuggestBoxListView), new FrameworkPropertyMetadata(SelectionMode.Single));
        }

        #region IsItemClickEnabled

        public static readonly DependencyProperty IsItemClickEnabledProperty =
            DependencyProperty.Register(
                nameof(IsItemClickEnabled),
                typeof(bool),
                typeof(AutoSuggestBoxListView),
                new PropertyMetadata(false));

        public bool IsItemClickEnabled
        {
            get => (bool)GetValue(IsItemClickEnabledProperty);
            set => SetValue(IsItemClickEnabledProperty, value);
        }

        #endregion

        public event ItemClickEventHandler ItemClick;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_scrollHost = this.FindDescendant<ScrollViewer>();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AutoSuggestBoxListViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AutoSuggestBoxListViewItem();
        }

        internal void NotifyListItemClicked(AutoSuggestBoxListViewItem item, MouseButton? mouseButton = null, bool isSecondaryGesture = false)
        {
            if (IsItemClickEnabled)
            {
                OnItemClick(item);
            }

            isSecondaryGesture |= mouseButton.HasValue && IsControlPressed();

            if (isSecondaryGesture)
            {
                SelectItemSecondary(item);
            }
            else
            {
                SelectItemPrimary(item);
            }
        }

        internal void ScrollToTop()
        {
            m_scrollHost?.ScrollToTop();
        }

        private void OnItemClick(AutoSuggestBoxListViewItem lvi)
        {
            var item = ItemContainerGenerator.ItemFromContainer(lvi);
            if (item != null)
            {
                ItemClick?.Invoke(this, new ItemClickEventArgs { ClickedItem = item });
            }
        }

        private void SelectItemPrimary(AutoSuggestBoxListViewItem item)
        {
            switch (SelectionMode)
            {
                case SelectionMode.Single:
                    MakeSingleSelection(item);
                    break;
                case SelectionMode.Multiple:
                    if (IsShiftPressed())
                    {
                        MakeRangeSelection(item, clearOldSelection: false);
                    }
                    else
                    {
                        MakeToggleSelection(item);
                    }
                    break;
                case SelectionMode.Extended:
                    if (IsShiftPressed())
                    {
                        MakeRangeSelection(item, clearOldSelection: true);
                    }
                    else
                    {
                        MakeSingleSelection(item);
                    }
                    break;
            }
        }

        private void SelectItemSecondary(AutoSuggestBoxListViewItem item)
        {
            switch (SelectionMode)
            {
                case SelectionMode.Single:
                    MakeToggleSelection(item);
                    break;
                case SelectionMode.Multiple:
                    if (IsShiftPressed())
                    {
                        MakeRangeSelection(item, clearOldSelection: false);
                    }
                    else
                    {
                        MakeToggleSelection(item);
                    }
                    break;
                case SelectionMode.Extended:
                    if (IsShiftPressed())
                    {
                        MakeRangeSelection(item, clearOldSelection: false);
                    }
                    else
                    {
                        MakeToggleSelection(item);
                    }
                    break;
            }
        }

        private void MakeSingleSelection(AutoSuggestBoxListViewItem item)
        {
            var itemIndex = GetItemIndex(item);
            SetAnchorIndex(itemIndex);

            if (SelectionMode != SelectionMode.Single)
            {
                UnselectAll();
            }

            item.SetCurrentValue(ListViewItem.IsSelectedProperty, true);
        }

        private void MakeToggleSelection(AutoSuggestBoxListViewItem item)
        {
            SetAnchorIndex(GetItemIndex(item));
            item.SetCurrentValue(ListViewItem.IsSelectedProperty, !item.IsSelected);
        }

        private void MakeRangeSelection(AutoSuggestBoxListViewItem item, bool clearOldSelection)
        {
            var itemIndex = GetItemIndex(item);
            if (itemIndex < 0)
            {
                return;
            }

            if (m_anchorIndex < 0 || m_anchorIndex >= Items.Count)
            {
                m_anchorIndex = SelectedIndex >= 0 ? SelectedIndex : itemIndex;
            }

            if (clearOldSelection)
            {
                UnselectAll();
            }

            var startIndex = Math.Min(m_anchorIndex, itemIndex);
            var endIndex = Math.Max(m_anchorIndex, itemIndex);

            for (var index = startIndex; index <= endIndex; index++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(index) is ListViewItem container)
                {
                    container.SetCurrentValue(ListViewItem.IsSelectedProperty, true);
                }
                else
                {
                    var dataItem = Items[index];
                    if (!SelectedItems.Contains(dataItem))
                    {
                        SelectedItems.Add(dataItem);
                    }
                }
            }
        }

        private int GetItemIndex(AutoSuggestBoxListViewItem item)
        {
            return ItemContainerGenerator.IndexFromContainer(item);
        }

        private void SetAnchorIndex(int itemIndex)
        {
            if (itemIndex >= 0)
            {
                m_anchorIndex = itemIndex;
            }
        }

        private static bool IsControlPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        private static bool IsShiftPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }

        private ScrollViewer m_scrollHost;
        private int m_anchorIndex = -1;
    }
}
