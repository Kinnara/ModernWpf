using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    [TemplatePart(Name = InnerListViewName, Type = typeof(ListBox))]
    public class RadioButtons : Control
    {
        private const string InnerListViewName = "InnerListView";

        static RadioButtons()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtons),
                new FrameworkPropertyMetadata(typeof(RadioButtons)));
        }

        public RadioButtons()
        {
            SetValue(ItemsProperty, new List<object>());
        }

        #region ItemsSource

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(OnItemsSourcePropertyChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateItemsSource();
        }

        #endregion

        #region Items

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(IList),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(OnItemsPropertyChanged));

        public IList Items
        {
            get => (IList)GetValue(ItemsProperty);
        }

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateItemsSource();
        }

        #endregion

        #region ItemTemplate

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(RadioButtons));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        #endregion

        #region SelectedIndex

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(-1, OnSelectedIndexPropertyChanged));

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateSelectedIndex();
        }

        #endregion

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(OnSelectedItemPropertyChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateSelectedItem();
        }

        #endregion

        #region MaximumColumns

        public static readonly DependencyProperty MaximumColumnsProperty =
            DependencyProperty.Register(
                nameof(MaximumColumns),
                typeof(int),
                typeof(RadioButtons),
                new FrameworkPropertyMetadata(1, OnMaximumColumnsChanged));

        public int MaximumColumns
        {
            get => (int)GetValue(MaximumColumnsProperty);
            set => SetValue(MaximumColumnsProperty, value);
        }

        private static void OnMaximumColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioButtons)d).UpdateMaximumColumns();
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            ControlHelper.HeaderProperty.AddOwner(typeof(RadioButtons));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        public event SelectionChangedEventHandler SelectionChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_listView = GetTemplateChild(InnerListViewName) as ListBox;
            if (m_listView != null)
            {
                m_listView.Loaded += OnListBoxLoaded;
                m_listView.SelectionChanged += OnListBoxSelectionChanged;
            }

            UpdateItemsSource();
        }

        public DependencyObject ContainerFromItem(object item)
        {
            return m_listView?.ItemContainerGenerator.ContainerFromItem(item);
        }

        public DependencyObject ContainerFromIndex(int index)
        {
            return m_listView?.ItemContainerGenerator.ContainerFromIndex(index);
        }

        private void OnListBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (m_listView != null)
            {
                m_listView.Loaded -= OnListBoxLoaded;

                m_uniformGrid = VisualTree.FindDescendant<UniformGrid>(m_listView);
                if (m_uniformGrid != null)
                {
                    UpdateSelectedIndex();
                    UpdateSelectedItem();
                    UpdateMaximumColumns();
                }
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_listView != null)
            {
                SelectedIndex = m_listView.SelectedIndex;
                SelectedItem = m_listView.SelectedItem;
            }

            SelectionChanged?.Invoke(this, e);
        }

        private void UpdateItemsSource()
        {
            if (m_listView != null)
            {
                if (ItemsSource != null)
                {
                    m_listView.ItemsSource = ItemsSource;
                }
                else
                {
                    m_listView.ItemsSource = Items;
                }
            }
        }

        private void UpdateMaximumColumns()
        {
            if (m_uniformGrid != null)
            {
                m_uniformGrid.Columns = MaximumColumns;
            }
        }

        private void UpdateSelectedItem()
        {
            if (m_listView != null)
            {
                if (m_listView.ItemContainerGenerator.ContainerFromItem(SelectedItem) is ListBoxItem lbi)
                {
                    lbi.IsSelected = true;
                }
            }
        }

        private void UpdateSelectedIndex()
        {
            if (m_listView != null)
            {
                m_listView.SelectedIndex = SelectedIndex;
            }
        }

        private ListBox m_listView;
        private UniformGrid m_uniformGrid;
    }
}
