using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    /// <summary>
    /// TabViewItem Properties
    /// </summary>
    public static class TabItemHelper
    {
        #region IsEnabled

        public static bool GetIsEnabled(TabItem element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TabItem element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TabItemHelper),
            new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (TabItem)d;
            if ((bool)e.NewValue)
            {
                item.Loaded += OnLoaded;
                item.SizeChanged += OnSizeChanged;
            }
            else
            {
                item.Loaded -= OnLoaded;
                item.SizeChanged -= OnSizeChanged;
            }
        }

        #endregion

        #region Icon

        /// <summary>
        /// Sets the value for the Icon to be displayed within the tab.
        /// </summary>
        /// <param name="tabItem">The element from which to read the property value.</param>
        /// <returns>The Icon to be displayed within the tab.</returns>
        public static object GetIcon(TabItem tabItem)
        {
            return tabItem.GetValue(IconProperty);
        }

        /// <summary>
        /// Gets the value for the Icon to be displayed within the tab.
        /// </summary>
        /// <param name="tabItem">The element from which to read the property value.</param>
        /// <param name="value">The Icon to be displayed within the tab.</param>
        public static void SetIcon(TabItem tabItem, object value)
        {
            tabItem.SetValue(IconProperty, value);
        }

        /// <summary>
        /// Identifies the Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(object),
                typeof(TabItemHelper));

        #endregion

        #region TabGeometry

        public static object GetTabGeometry(TabItem tabItem)
        {
            return tabItem.GetValue(TabGeometryProperty);
        }

        private static void SetTabGeometry(TabItem tabItem, object value)
        {
            tabItem.SetValue(TabGeometryProperty, value);
        }

        public static readonly DependencyProperty TabGeometryProperty =
            DependencyProperty.RegisterAttached(
                "TabGeometry",
                typeof(Geometry),
                typeof(TabItemHelper));

        #endregion

        #region CloseButtonOverlayMode

        public static readonly DependencyProperty CloseButtonOverlayModeProperty = DependencyProperty.RegisterAttached(
            "CloseButtonOverlayMode",
            typeof(TabViewCloseButtonOverlayMode),
            typeof(TabItemHelper),
            null);

        public static TabViewCloseButtonOverlayMode GetCloseButtonOverlayMode(TabControl element)
        {
            return (TabViewCloseButtonOverlayMode)element.GetValue(CloseButtonOverlayModeProperty);
        }

        #endregion

        #region IsAddTabButtonVisible

        public static readonly DependencyProperty IsAddTabButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "IsAddTabButtonVisible",
            typeof(bool),
            typeof(TabItemHelper),
            new PropertyMetadata(false));

        public static bool GetIsAddTabButtonVisible(TabItem element)
        {
            return (bool)element.GetValue(IsAddTabButtonVisibleProperty);
        }

        #endregion

        #region IsAddTabButtonVisible

        public static readonly DependencyProperty CloseTabButtonCommandProperty = DependencyProperty.RegisterAttached(
            "CloseTabButtonCommand",
            typeof(ICommand),
            typeof(TabItemHelper),
            null);

        public static ICommand GetCloseTabButtonCommand(TabItem element)
        {
            return (ICommand)element.GetValue(CloseTabButtonCommandProperty);
        }

        private static void SetCloseTabButtonCommand(TabItem tabItem, ICommand value)
        {
            tabItem.SetValue(CloseTabButtonCommandProperty, value);
        }

        #endregion

        #region TabItemHelperEvents

        /// <summary>
        /// Identifies the TabItemHelperEvents dependency property.
        /// </summary>
        public static readonly DependencyProperty TabItemHelperEventsProperty = DependencyProperty.RegisterAttached(
            "TabItemHelperEvents",
            typeof(TabItemHelperEvents),
            typeof(TabItemHelper),
            new PropertyMetadata(new TabItemHelperEvents()));

        public static TabItemHelperEvents GetTabItemHelperEvents(TabItem element)
        {
            return (TabItemHelperEvents)element.GetValue(TabItemHelperEventsProperty);
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            TabItem TabItem = sender as TabItem;
            UpdateTabGeometry(TabItem);
            TabControl TabControl = TabItem.FindAscendant<TabControl>();

            if (TabControl != null)
            {
                TabItem.SetBinding(IsAddTabButtonVisibleProperty, new Binding
                {
                    Source = TabControl,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(TabControlHelper.IsAddTabButtonVisibleProperty)
                });
                TabItem.SetBinding(CloseButtonOverlayModeProperty, new Binding
                {
                    Source = TabControl,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(TabControlHelper.CloseButtonOverlayModeProperty)
                });

                var CloseTabButtonCommand = new RoutedCommand();

                void ExecutedCustomCommand(object sender, ExecutedRoutedEventArgs e)
                {
                    TabControlHelper.GetTabControlHelperEvents(TabControl).TabCloseRequested?.Invoke(TabControl, new TabViewTabCloseRequestedEventArgs(TabItem.Content, TabItem));
                    GetTabItemHelperEvents(TabItem).CloseRequested?.Invoke(TabItem, new TabViewTabCloseRequestedEventArgs(TabItem.Content, TabItem));
                    if (TabControl.SelectedItem == TabItem)
                    {
                        TabControl.SelectedIndex--;
                    }
                    TabControl.Items.Remove(sender);
                    e.Handled = true;
                }

                void CanExecuteCustomCommand(object sender, CanExecuteRoutedEventArgs e)
                {
                    if (TabControl != null)
                    {
                        e.CanExecute = true;
                    }
                    else
                    {
                        e.CanExecute = false;
                    }
                    e.Handled = true;
                }

                CommandBinding CloseTabButtonCommandBinding = new CommandBinding(CloseTabButtonCommand, ExecutedCustomCommand, CanExecuteCustomCommand);
                TabItem.CommandBindings.Add(CloseTabButtonCommandBinding);
                SetCloseTabButtonCommand(TabItem, CloseTabButtonCommand);
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTabGeometry(sender as TabItem);
        }

        private static void UpdateTabGeometry(TabItem tabItem)
        {
            var scaleFactor = 1.5;
#if NET462_OR_NEWER
            scaleFactor = VisualTreeHelper.GetDpi(tabItem).DpiScaleX;
#else
            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(tabItem);
            Matrix transformToDevice = hwnd.CompositionTarget.TransformToDevice;
            scaleFactor = transformToDevice.M11;
#endif
            var height = tabItem.ActualHeight;
            var popupRadius = ControlHelper.GetCornerRadius(tabItem);
            var leftCorner = popupRadius.TopLeft;
            var rightCorner = popupRadius.TopRight;

            // Assumes 4px curving-out corners, which are hardcoded in the markup
            var data = $"F1 M0,{height - 1f / scaleFactor}  a 4,4 0 0 0 4,-4  L 4,{leftCorner}  a {leftCorner},{leftCorner} 0 0 1 {leftCorner},-{leftCorner}  l {tabItem.ActualWidth - (leftCorner + rightCorner + 1.0f / scaleFactor)},0  a {rightCorner},{rightCorner} 0 0 1 {rightCorner},{rightCorner}  l 0,{height - (4 + rightCorner + 1.0f / scaleFactor)}  a 4,4 0 0 0 4,4 Z";

            var geometry = Geometry.Parse(data);

            SetTabGeometry(tabItem, geometry);
        }
    }

    /// <summary>
    /// Events for TabItemHelper.
    /// </summary>
    public class TabItemHelperEvents
    {
        /// <summary>
        /// Raised when the user attempts to close the TabViewItem via clicking the x-to-close button, CTRL+F4, or mousewheel.
        /// </summary>
        public TypedEventHandler<TabItem, TabViewTabCloseRequestedEventArgs> CloseRequested;
    }
}
