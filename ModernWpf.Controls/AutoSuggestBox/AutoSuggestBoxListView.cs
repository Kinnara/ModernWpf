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

        internal void NotifyListItemClicked(AutoSuggestBoxListViewItem item, MouseButton? mouseButton = null)
        {
            if (IsItemClickEnabled)
            {
                OnItemClick(item);
            }

            switch (SelectionMode)
            {
                case SelectionMode.Single:
                    {
                        if (!item.IsSelected)
                        {
                            item.SetCurrentValue(IsSelectedProperty, true);
                        }
                        else if (mouseButton.HasValue && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            item.SetCurrentValue(IsSelectedProperty, false);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
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

        private ScrollViewer m_scrollHost;
    }
}
