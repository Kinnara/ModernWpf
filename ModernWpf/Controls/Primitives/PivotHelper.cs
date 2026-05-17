using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                typeof(PivotHelper),
                new PropertyMetadata(null, OnTitlePropertyChanged));

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
                typeof(PivotHelper),
                new PropertyMetadata(null, OnTitlePropertyChanged));

        public static DataTemplate GetTitleTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TitleTemplateProperty);
        }

        public static void SetTitleTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TitleTemplateProperty, value);
        }

        #endregion

        #region TitleVisibility

        private static readonly DependencyPropertyKey TitleVisibilityPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "TitleVisibility",
                typeof(Visibility),
                typeof(PivotHelper),
                new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty TitleVisibilityProperty =
            TitleVisibilityPropertyKey.DependencyProperty;

        public static Visibility GetTitleVisibility(TabControl tabControl)
        {
            return (Visibility)tabControl.GetValue(TitleVisibilityProperty);
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabControl tabControl)
            {
                UpdateTitleVisibility(tabControl);
            }
        }

        private static void UpdateTitleVisibility(TabControl tabControl)
        {
            var visibility = GetTitle(tabControl) != null || GetTitleTemplate(tabControl) != null
                ? Visibility.Visible
                : Visibility.Collapsed;

            tabControl.SetValue(TitleVisibilityPropertyKey, visibility);
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
                if (GetIsHeaderItemPressed(tabItem))
                {
                    return "SelectedPressed";
                }

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

        #region NavigationButtonsVisualStateSettersEnabled

        public static readonly DependencyProperty NavigationButtonsVisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "NavigationButtonsVisualStateSettersEnabled",
                typeof(bool),
                typeof(PivotHelper),
                new PropertyMetadata(false, OnNavigationButtonsVisualStateSettersEnabledChanged));

        public static bool GetNavigationButtonsVisualStateSettersEnabled(TabControl tabControl)
        {
            return (bool)tabControl.GetValue(NavigationButtonsVisualStateSettersEnabledProperty);
        }

        public static void SetNavigationButtonsVisualStateSettersEnabled(TabControl tabControl, bool value)
        {
            tabControl.SetValue(NavigationButtonsVisualStateSettersEnabledProperty, value);
        }

        private static void OnNavigationButtonsVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabControl = (TabControl)d;
            var controller = GetNavigationButtonsController(tabControl);

            if ((bool)e.NewValue)
            {
                if (controller == null)
                {
                    controller = new NavigationButtonsController(tabControl);
                    SetNavigationButtonsController(tabControl, controller);
                }

                controller.Attach();
            }
            else
            {
                controller?.Detach();
                SetNavigationButtonsController(tabControl, null);
            }
        }

        private static readonly DependencyProperty NavigationButtonsControllerProperty =
            DependencyProperty.RegisterAttached(
                "NavigationButtonsController",
                typeof(NavigationButtonsController),
                typeof(PivotHelper));

        private static NavigationButtonsController GetNavigationButtonsController(TabControl tabControl)
        {
            return (NavigationButtonsController)tabControl.GetValue(NavigationButtonsControllerProperty);
        }

        private static void SetNavigationButtonsController(TabControl tabControl, NavigationButtonsController value)
        {
            tabControl.SetValue(NavigationButtonsControllerProperty, value);
        }

        private sealed class NavigationButtonsController
        {
            public NavigationButtonsController(TabControl tabControl)
            {
                _tabControl = tabControl;
            }

            public void Attach()
            {
                _tabControl.Loaded -= OnLoaded;
                _tabControl.Unloaded -= OnUnloaded;
                _tabControl.Loaded += OnLoaded;
                _tabControl.Unloaded += OnUnloaded;

                if (_tabControl.IsLoaded)
                {
                    HookTemplateParts();
                    UpdateVisualState(false);
                }
            }

            public void Detach()
            {
                _tabControl.Loaded -= OnLoaded;
                _tabControl.Unloaded -= OnUnloaded;
                UnhookTemplateParts();
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                HookTemplateParts();
                UpdateVisualState(false);
            }

            private void OnUnloaded(object sender, RoutedEventArgs e)
            {
                UnhookTemplateParts();
            }

            private void HookTemplateParts()
            {
                UnhookTemplateParts();

                _tabControl.ApplyTemplate();
                _headerPanel = _tabControl.Template?.FindName("headerPanel", _tabControl) as FrameworkElement;
                _contentPanel = _tabControl.Template?.FindName("contentPanel", _tabControl) as FrameworkElement;
                _scrollViewer = _tabControl.Template?.FindName("ScrollViewer", _tabControl) as PivotHeaderScrollViewer;
                _previousButton = _tabControl.Template?.FindName("PreviousButton", _tabControl) as ButtonBase;
                _nextButton = _tabControl.Template?.FindName("NextButton", _tabControl) as ButtonBase;

                AttachMouseState(_headerPanel);
                AttachMouseState(_contentPanel);
                AttachMouseState(_previousButton);
                AttachMouseState(_nextButton);

                if (_scrollViewer != null)
                {
                    CanScrollLeftPropertyDescriptor.AddValueChanged(_scrollViewer, OnNavigationButtonStateChanged);
                    CanScrollRightPropertyDescriptor.AddValueChanged(_scrollViewer, OnNavigationButtonStateChanged);
                }
            }

            private void UnhookTemplateParts()
            {
                DetachMouseState(_headerPanel);
                DetachMouseState(_contentPanel);
                DetachMouseState(_previousButton);
                DetachMouseState(_nextButton);

                if (_scrollViewer != null)
                {
                    CanScrollLeftPropertyDescriptor.RemoveValueChanged(_scrollViewer, OnNavigationButtonStateChanged);
                    CanScrollRightPropertyDescriptor.RemoveValueChanged(_scrollViewer, OnNavigationButtonStateChanged);
                }

                _headerPanel = null;
                _contentPanel = null;
                _scrollViewer = null;
                _previousButton = null;
                _nextButton = null;
            }

            private void AttachMouseState(FrameworkElement element)
            {
                if (element != null)
                {
                    element.MouseEnter += OnMouseStateChanged;
                    element.MouseLeave += OnMouseStateChanged;
                }
            }

            private void DetachMouseState(FrameworkElement element)
            {
                if (element != null)
                {
                    element.MouseEnter -= OnMouseStateChanged;
                    element.MouseLeave -= OnMouseStateChanged;
                }
            }

            private void OnMouseStateChanged(object sender, MouseEventArgs e)
            {
                UpdateVisualState(true);
            }

            private void OnNavigationButtonStateChanged(object sender, System.EventArgs e)
            {
                UpdateVisualState(true);
            }

            private void UpdateVisualState(bool useTransitions)
            {
                string stateName = GetNavigationButtonsStateName();
                if (!GoToNavigationButtonsState(stateName, useTransitions) &&
                    (stateName == "PreviousButtonVisible" || stateName == "NextButtonVisible"))
                {
                    GoToNavigationButtonsState("NavigationButtonsVisible", useTransitions);
                }
            }

            private bool GoToNavigationButtonsState(string stateName, bool useTransitions)
            {
                if (VisualStateManager.GoToState(_tabControl, stateName, useTransitions))
                {
                    return true;
                }

                return _tabControl.GetTemplateRoot() is { } templateRoot &&
                    VisualStateManager.GoToElementState(templateRoot, stateName, useTransitions);
            }

            private string GetNavigationButtonsStateName()
            {
                bool isPointerOverHeaders =
                    _headerPanel?.IsMouseOver == true ||
                    _previousButton?.IsMouseOver == true ||
                    _nextButton?.IsMouseOver == true;

                if (!isPointerOverHeaders || _contentPanel?.IsMouseOver == true)
                {
                    return "NavigationButtonsHidden";
                }

                bool showPreviousButton = _scrollViewer?.CanScrollLeft == true;
                bool showNextButton = _scrollViewer?.CanScrollRight == true;

                if (showPreviousButton && showNextButton)
                {
                    return "NavigationButtonsVisible";
                }

                if (showPreviousButton)
                {
                    return "PreviousButtonVisible";
                }

                return showNextButton ? "NextButtonVisible" : "NavigationButtonsHidden";
            }

            private readonly TabControl _tabControl;
            private FrameworkElement _headerPanel;
            private FrameworkElement _contentPanel;
            private PivotHeaderScrollViewer _scrollViewer;
            private ButtonBase _previousButton;
            private ButtonBase _nextButton;
        }

        private static readonly DependencyPropertyDescriptor CanScrollLeftPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(PivotHeaderScrollViewer.CanScrollLeftProperty, typeof(PivotHeaderScrollViewer));

        private static readonly DependencyPropertyDescriptor CanScrollRightPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(PivotHeaderScrollViewer.CanScrollRightProperty, typeof(PivotHeaderScrollViewer));

        #endregion
    }
}
