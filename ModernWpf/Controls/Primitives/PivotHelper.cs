using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public static class PivotHelper
    {
        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached(
                "Title",
                typeof(object),
                typeof(PivotHelper));

        public static object GetTitle(TabControl tabControl)
        {
            return tabControl.GetValue(TitleProperty);
        }

        public static void SetTitle(TabControl tabControl, object value)
        {
            tabControl.SetValue(TitleProperty, value);
        }

        #endregion

        #region TitleTemplate

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.RegisterAttached(
                "TitleTemplate",
                typeof(DataTemplate),
                typeof(PivotHelper));

        public static DataTemplate GetTitleTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TitleTemplateProperty);
        }

        public static void SetTitleTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TitleTemplateProperty, value);
        }

        #endregion

        #region LeftHeader

        public static readonly DependencyProperty LeftHeaderProperty =
            DependencyProperty.RegisterAttached(
                "LeftHeader",
                typeof(object),
                typeof(PivotHelper));

        public static object GetLeftHeader(TabControl tabControl)
        {
            return tabControl.GetValue(LeftHeaderProperty);
        }

        public static void SetLeftHeader(TabControl tabControl, object value)
        {
            tabControl.SetValue(LeftHeaderProperty, value);
        }

        #endregion

        #region LeftHeaderTemplate

        public static readonly DependencyProperty LeftHeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "LeftHeaderTemplate",
                typeof(DataTemplate),
                typeof(PivotHelper));

        public static DataTemplate GetLeftHeaderTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(LeftHeaderTemplateProperty);
        }

        public static void SetLeftHeaderTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(LeftHeaderTemplateProperty, value);
        }

        #endregion

        #region RightHeader

        public static readonly DependencyProperty RightHeaderProperty =
            DependencyProperty.RegisterAttached(
                "RightHeader",
                typeof(object),
                typeof(PivotHelper));

        public static object GetRightHeader(TabControl tabControl)
        {
            return tabControl.GetValue(RightHeaderProperty);
        }

        public static void SetRightHeader(TabControl tabControl, object value)
        {
            tabControl.SetValue(RightHeaderProperty, value);
        }

        #endregion

        #region RightHeaderTemplate

        public static readonly DependencyProperty RightHeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "RightHeaderTemplate",
                typeof(DataTemplate),
                typeof(PivotHelper));

        public static DataTemplate GetRightHeaderTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(RightHeaderTemplateProperty);
        }

        public static void SetRightHeaderTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(RightHeaderTemplateProperty, value);
        }

        #endregion

        #region HeaderItemVisualStateSettersEnabled

        public static readonly DependencyProperty HeaderItemVisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "HeaderItemVisualStateSettersEnabled",
                typeof(bool),
                typeof(PivotHelper),
                new PropertyMetadata(false, OnHeaderItemVisualStateSettersEnabledChanged));

        public static bool GetHeaderItemVisualStateSettersEnabled(TabItem tabItem)
        {
            return (bool)tabItem.GetValue(HeaderItemVisualStateSettersEnabledProperty);
        }

        public static void SetHeaderItemVisualStateSettersEnabled(TabItem tabItem, bool value)
        {
            tabItem.SetValue(HeaderItemVisualStateSettersEnabledProperty, value);
        }

        private static void OnHeaderItemVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabItem = (TabItem)d;
            if ((bool)e.NewValue)
            {
                DetachHeaderItem(tabItem);
                AttachHeaderItem(tabItem);

                if (tabItem.IsLoaded)
                {
                    UpdateHeaderItemVisualState(tabItem, false);
                }
            }
            else
            {
                DetachHeaderItem(tabItem);
            }
        }

        private static void AttachHeaderItem(TabItem tabItem)
        {
            tabItem.Loaded += OnHeaderItemLoaded;
            IsSelectedPropertyDescriptor.AddValueChanged(tabItem, OnHeaderItemPropertyStateChanged);
            IsEnabledPropertyDescriptor.AddValueChanged(tabItem, OnHeaderItemPropertyStateChanged);
            tabItem.MouseEnter += OnHeaderItemMouseStateChanged;
            tabItem.MouseLeave += OnHeaderItemMouseStateChanged;
            tabItem.PreviewMouseDown += OnHeaderItemMouseButtonStateChanged;
            tabItem.PreviewMouseUp += OnHeaderItemMouseButtonStateChanged;
            tabItem.LostMouseCapture += OnHeaderItemMouseStateChanged;
        }

        private static void DetachHeaderItem(TabItem tabItem)
        {
            tabItem.Loaded -= OnHeaderItemLoaded;
            IsSelectedPropertyDescriptor.RemoveValueChanged(tabItem, OnHeaderItemPropertyStateChanged);
            IsEnabledPropertyDescriptor.RemoveValueChanged(tabItem, OnHeaderItemPropertyStateChanged);
            tabItem.MouseEnter -= OnHeaderItemMouseStateChanged;
            tabItem.MouseLeave -= OnHeaderItemMouseStateChanged;
            tabItem.PreviewMouseDown -= OnHeaderItemMouseButtonStateChanged;
            tabItem.PreviewMouseUp -= OnHeaderItemMouseButtonStateChanged;
            tabItem.LostMouseCapture -= OnHeaderItemMouseStateChanged;
            SetIsHeaderItemPressed(tabItem, false);
        }

        private static void OnHeaderItemLoaded(object sender, RoutedEventArgs e)
        {
            UpdateHeaderItemVisualState((TabItem)sender, false);
        }

        private static void OnHeaderItemPropertyStateChanged(object sender, System.EventArgs e)
        {
            UpdateHeaderItemVisualState((TabItem)sender, true);
        }

        private static void OnHeaderItemMouseStateChanged(object sender, MouseEventArgs e)
        {
            var tabItem = (TabItem)sender;
            if (e.RoutedEvent == UIElement.MouseLeaveEvent || e.RoutedEvent == UIElement.LostMouseCaptureEvent)
            {
                SetIsHeaderItemPressed(tabItem, false);
            }

            ScheduleHeaderItemVisualStateUpdate(tabItem);
        }

        private static void OnHeaderItemMouseButtonStateChanged(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var tabItem = (TabItem)sender;
                SetIsHeaderItemPressed(tabItem, e.RoutedEvent == UIElement.PreviewMouseDownEvent);
                ScheduleHeaderItemVisualStateUpdate(tabItem);
            }
        }

        private static void ScheduleHeaderItemVisualStateUpdate(TabItem tabItem)
        {
            UpdateHeaderItemVisualState(tabItem, true);
            tabItem.Dispatcher.BeginInvoke(
                (System.Action)(() => UpdateHeaderItemVisualState(tabItem, true)),
                DispatcherPriority.Input);
        }

        private static void UpdateHeaderItemVisualState(TabItem tabItem, bool useTransitions)
        {
            VisualStateManager.GoToState(tabItem, GetHeaderItemVisualStateName(tabItem), useTransitions);
        }

        private static string GetHeaderItemVisualStateName(TabItem tabItem)
        {
            if (!tabItem.IsEnabled)
            {
                return "Disabled";
            }

            if (tabItem.IsSelected)
            {
                return tabItem.IsMouseOver ? "SelectedPointerOver" : "Selected";
            }

            if (GetIsHeaderItemPressed(tabItem))
            {
                return "UnselectedPressed";
            }

            if (tabItem.IsMouseOver)
            {
                return "UnselectedPointerOver";
            }

            return "Unselected";
        }

        private static readonly DependencyProperty IsHeaderItemPressedProperty =
            DependencyProperty.RegisterAttached(
                "IsHeaderItemPressed",
                typeof(bool),
                typeof(PivotHelper),
                new PropertyMetadata(false));

        private static bool GetIsHeaderItemPressed(TabItem tabItem)
        {
            return (bool)tabItem.GetValue(IsHeaderItemPressedProperty);
        }

        private static void SetIsHeaderItemPressed(TabItem tabItem, bool value)
        {
            tabItem.SetValue(IsHeaderItemPressedProperty, value);
        }

        private static readonly DependencyPropertyDescriptor IsSelectedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(TabItem.IsSelectedProperty, typeof(TabItem));

        private static readonly DependencyPropertyDescriptor IsEnabledPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(TabItem));

        #endregion
    }
}
