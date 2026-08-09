using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls
{
    public partial class TabView
    {
        private static readonly DependencyPropertyKey TabItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TabItems),
                typeof(ObservableCollection<object>),
                typeof(TabView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TabItemsProperty = TabItemsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty TabWidthModeProperty =
            DependencyProperty.Register(
                nameof(TabWidthMode),
                typeof(TabViewWidthMode),
                typeof(TabView),
                new FrameworkPropertyMetadata(TabViewWidthMode.Equal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnLayoutPropertyChanged));

        public static readonly DependencyProperty CloseButtonOverlayModeProperty =
            DependencyProperty.Register(
                nameof(CloseButtonOverlayMode),
                typeof(TabViewCloseButtonOverlayMode),
                typeof(TabView),
                new FrameworkPropertyMetadata(TabViewCloseButtonOverlayMode.Auto, OnLayoutPropertyChanged));

        public static readonly DependencyProperty TabStripHeaderProperty =
            DependencyProperty.Register(nameof(TabStripHeader), typeof(object), typeof(TabView));

        public static readonly DependencyProperty TabStripHeaderTemplateProperty =
            DependencyProperty.Register(nameof(TabStripHeaderTemplate), typeof(DataTemplate), typeof(TabView));

        public static readonly DependencyProperty TabStripFooterProperty =
            DependencyProperty.Register(nameof(TabStripFooter), typeof(object), typeof(TabView));

        public static readonly DependencyProperty TabStripFooterTemplateProperty =
            DependencyProperty.Register(nameof(TabStripFooterTemplate), typeof(DataTemplate), typeof(TabView));

        public static readonly DependencyProperty IsAddTabButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsAddTabButtonVisible), typeof(bool), typeof(TabView), new PropertyMetadata(true));

        public static readonly DependencyProperty AddTabButtonCommandProperty =
            DependencyProperty.Register(nameof(AddTabButtonCommand), typeof(ICommand), typeof(TabView));

        public static readonly DependencyProperty AddTabButtonCommandParameterProperty =
            DependencyProperty.Register(nameof(AddTabButtonCommandParameter), typeof(object), typeof(TabView));

        public static readonly DependencyProperty TabItemsSourceProperty =
            DependencyProperty.Register(
                nameof(TabItemsSource),
                typeof(IEnumerable),
                typeof(TabView),
                new PropertyMetadata(null, OnTabItemsSourcePropertyChanged));

        public static readonly DependencyProperty TabItemTemplateProperty =
            DependencyProperty.Register(nameof(TabItemTemplate), typeof(DataTemplate), typeof(TabView), new PropertyMetadata(null, OnItemTemplatePropertyChanged));

        public static readonly DependencyProperty TabItemTemplateSelectorProperty =
            DependencyProperty.Register(nameof(TabItemTemplateSelector), typeof(DataTemplateSelector), typeof(TabView), new PropertyMetadata(null, OnItemTemplatePropertyChanged));

        public static readonly DependencyProperty CanDragTabsProperty =
            DependencyProperty.Register(nameof(CanDragTabs), typeof(bool), typeof(TabView), new PropertyMetadata(false));

        public static readonly DependencyProperty CanReorderTabsProperty =
            DependencyProperty.Register(nameof(CanReorderTabs), typeof(bool), typeof(TabView), new PropertyMetadata(true));

        public static readonly DependencyProperty AllowDropTabsProperty =
            DependencyProperty.Register(nameof(AllowDropTabs), typeof(bool), typeof(TabView), new PropertyMetadata(true));

        public static readonly DependencyProperty CanTearOutTabsProperty =
            DependencyProperty.Register(nameof(CanTearOutTabs), typeof(bool), typeof(TabView), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(TabView),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexPropertyChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(TabView),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemPropertyChanged));

        public ObservableCollection<object> TabItems => (ObservableCollection<object>)GetValue(TabItemsProperty);

        public TabViewWidthMode TabWidthMode
        {
            get => (TabViewWidthMode)GetValue(TabWidthModeProperty);
            set => SetValue(TabWidthModeProperty, value);
        }

        public TabViewCloseButtonOverlayMode CloseButtonOverlayMode
        {
            get => (TabViewCloseButtonOverlayMode)GetValue(CloseButtonOverlayModeProperty);
            set => SetValue(CloseButtonOverlayModeProperty, value);
        }

        public object TabStripHeader
        {
            get => GetValue(TabStripHeaderProperty);
            set => SetValue(TabStripHeaderProperty, value);
        }

        public DataTemplate TabStripHeaderTemplate
        {
            get => (DataTemplate)GetValue(TabStripHeaderTemplateProperty);
            set => SetValue(TabStripHeaderTemplateProperty, value);
        }

        public object TabStripFooter
        {
            get => GetValue(TabStripFooterProperty);
            set => SetValue(TabStripFooterProperty, value);
        }

        public DataTemplate TabStripFooterTemplate
        {
            get => (DataTemplate)GetValue(TabStripFooterTemplateProperty);
            set => SetValue(TabStripFooterTemplateProperty, value);
        }

        public bool IsAddTabButtonVisible
        {
            get => (bool)GetValue(IsAddTabButtonVisibleProperty);
            set => SetValue(IsAddTabButtonVisibleProperty, value);
        }

        public ICommand AddTabButtonCommand
        {
            get => (ICommand)GetValue(AddTabButtonCommandProperty);
            set => SetValue(AddTabButtonCommandProperty, value);
        }

        public object AddTabButtonCommandParameter
        {
            get => GetValue(AddTabButtonCommandParameterProperty);
            set => SetValue(AddTabButtonCommandParameterProperty, value);
        }

        public IEnumerable TabItemsSource
        {
            get => (IEnumerable)GetValue(TabItemsSourceProperty);
            set => SetValue(TabItemsSourceProperty, value);
        }

        public DataTemplate TabItemTemplate
        {
            get => (DataTemplate)GetValue(TabItemTemplateProperty);
            set => SetValue(TabItemTemplateProperty, value);
        }

        public DataTemplateSelector TabItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(TabItemTemplateSelectorProperty);
            set => SetValue(TabItemTemplateSelectorProperty, value);
        }

        public bool CanDragTabs
        {
            get => (bool)GetValue(CanDragTabsProperty);
            set => SetValue(CanDragTabsProperty, value);
        }

        public bool CanReorderTabs
        {
            get => (bool)GetValue(CanReorderTabsProperty);
            set => SetValue(CanReorderTabsProperty, value);
        }

        public bool AllowDropTabs
        {
            get => (bool)GetValue(AllowDropTabsProperty);
            set => SetValue(AllowDropTabsProperty, value);
        }

        public bool CanTearOutTabs
        {
            get => (bool)GetValue(CanTearOutTabsProperty);
            set => SetValue(CanTearOutTabsProperty, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
    }
}
