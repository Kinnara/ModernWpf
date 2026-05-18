using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class ListViewBase : ListBox
    {
        static ListViewBase()
        {
            SelectionModeProperty.OverrideMetadata(typeof(ListViewBase), new FrameworkPropertyMetadata(OnSelectionModePropertyChanged));
        }

        protected ListViewBase()
        {
            UpdateMultiSelectEnabled();
        }

        #region IsSelectionEnabled

        private static void OnIsSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lvb = (ListViewBase)d;
            lvb.UpdateMultiSelectEnabled();
            if (!(bool)e.NewValue)
            {
                if (lvb.SelectedItems.Count > 0)
                {
                    lvb.UnselectAll();
                }
            }
        }

        #endregion

        #region IsMultiSelectCheckBoxEnabled

        private static void OnIsMultiSelectCheckBoxEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ListViewBase)d).UpdateMultiSelectEnabled();
        }

        #endregion

        internal bool MultiSelectEnabled
        {
            get => m_multiSelectEnabled;
            set
            {
                if (m_multiSelectEnabled != value)
                {
                    m_multiSelectEnabled = value;
                    MultiSelectEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event ItemClickEventHandler ItemClick;

        internal event EventHandler MultiSelectEnabledChanged;

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is ListViewBaseItem lvi)
            {
                lvi.SubscribeToMultiSelectEnabledChanged(this);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            if (element is ListViewBaseItem lvi)
            {
                lvi.UnsubscribeFromMultiSelectEnabledChanged(this);
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (IsSelectionEnabled)
            {
                base.OnSelectionChanged(e);
            }
            else
            {
                if (SelectedItems.Count > 0)
                {
                    UnselectAll();
                }
            }
        }

        internal void NotifyListItemClicked(ListViewBaseItem item)
        {
            if (IsItemClickEnabled)
            {
                var clickedItem = ItemContainerGenerator.ItemFromContainer(item);
                if (clickedItem == DependencyProperty.UnsetValue || ReferenceEquals(clickedItem, item))
                {
                    clickedItem = item.Content;
                }

                ItemClick?.Invoke(this, new ItemClickEventArgs { ClickedItem = clickedItem });
            }
        }

        private static void OnSelectionModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ListViewBase)d).UpdateMultiSelectEnabled();
        }

        private void UpdateMultiSelectEnabled()
        {
            MultiSelectEnabled = IsSelectionEnabled &&
                                 SelectionMode == SelectionMode.Multiple &&
                                 IsMultiSelectCheckBoxEnabled;
        }

        private bool m_multiSelectEnabled;
    }
}
