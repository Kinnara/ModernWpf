using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    /// <summary>
    /// TabView Properties
    /// </summary>
    public static class TabControlHelper
    {
        #region IsEnabled

        public static bool GetIsEnabled(TabControl element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TabControl element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TabControlHelper),
            new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (TabControl)d;
            if ((bool)e.NewValue)
            {
                item.Loaded += OnLoaded;
            }
            else
            {
                item.Loaded -= OnLoaded;
            }
        }

        #endregion

        #region TabStripHeader

        /// <summary>
        /// Identifies the TabStripHeader dependency property.
        /// </summary>
        public static readonly DependencyProperty TabStripHeaderProperty =
            DependencyProperty.RegisterAttached(
                "TabStripHeader",
                typeof(object),
                typeof(TabControlHelper));

        /// <summary>
        /// Gets the content that is shown to the left of the tab strip.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <returns>The element that is shown to the left of the tab strip.</returns>
        public static object GetTabStripHeader(TabControl tabControl)
        {
            return tabControl.GetValue(TabStripHeaderProperty);
        }

        /// <summary>
        /// Sets the content that is shown to the left of the tab strip.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <param name="value">The element that is shown to the left of the tab strip.</param>
        public static void SetTabStripHeader(TabControl tabControl, object value)
        {
            tabControl.SetValue(TabStripHeaderProperty, value);
        }

        #endregion

        #region TabStripHeaderTemplate

        /// <summary>
        /// Identifies the TabStripHeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty TabStripHeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "TabStripHeaderTemplate",
                typeof(DataTemplate),
                typeof(TabControlHelper));

        /// <summary>
        /// Gets the DataTemplate used to display the content of the TabStripHeader.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <returns>The DataTemplate used to display the content of the TabStripHeader.</returns>
        public static DataTemplate GetTabStripHeaderTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TabStripHeaderTemplateProperty);
        }

        /// <summary>
        /// Sets the DataTemplate used to display the content of the TabStripHeader.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <param name="value">The DataTemplate used to display the content of the TabStripHeader.</param>
        public static void SetTabStripHeaderTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TabStripHeaderTemplateProperty, value);
        }

        #endregion

        #region TabStripFooter

        /// <summary>
        /// Identifies the TabStripFooter dependency property.
        /// </summary>
        public static readonly DependencyProperty TabStripFooterProperty =
            DependencyProperty.RegisterAttached(
                "TabStripFooter",
                typeof(object),
                typeof(TabControlHelper));

        /// <summary>
        /// Gets the content that is shown to the right of the tab strip.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <returns>The element that is shown to the right of the tab strip.</returns>
        public static object GetTabStripFooter(TabControl tabControl)
        {
            return tabControl.GetValue(TabStripFooterProperty);
        }

        /// <summary>
        /// Sets the content that is shown to the right of the tab strip.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <param name="value">The element that is shown to the right of the tab strip.</param>
        public static void SetTabStripFooter(TabControl tabControl, object value)
        {
            tabControl.SetValue(TabStripFooterProperty, value);
        }

        #endregion

        #region TabStripFooterTemplate

        /// <summary>
        /// Identifies the TabStripFooterTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty TabStripFooterTemplateProperty =
            DependencyProperty.RegisterAttached(
                "TabStripFooterTemplate",
                typeof(DataTemplate),
                typeof(TabControlHelper));

        /// <summary>
        /// Gets the DataTemplate used to display the content of the TabStripFooter.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <returns>The DataTemplate used to display the content of the TabStripFooter</returns>
        public static DataTemplate GetTabStripFooterTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TabStripFooterTemplateProperty);
        }

        /// <summary>
        /// Sets the DataTemplate used to display the content of the TabStripFooter.
        /// </summary>
        /// <param name="tabControl">The element from which to read the property value.</param>
        /// <param name="value">The DataTemplate used to display the content of the TabStripFooter</param>
        public static void SetTabStripFooterTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TabStripFooterTemplateProperty, value);
        }

        #endregion

        #region TabStripFooterTemplate

        /// <summary>
        /// Identifies the ContentBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ContentBackground",
                typeof(Brush),
                typeof(TabControlHelper),
                new PropertyMetadata(Brushes.Transparent));

        public static Brush GetContentBackground(TabControl tabControl)
        {
            return (Brush)tabControl.GetValue(ContentBackgroundProperty);
        }

        public static void SetContentBackground(TabControl tabControl, Brush value)
        {
            tabControl.SetValue(ContentBackgroundProperty, value);
        }

        #endregion

        #region CloseButtonOverlayMode

        /// <summary>
        /// Identifies the CloseButtonOverlayMode dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseButtonOverlayModeProperty = DependencyProperty.RegisterAttached(
            "CloseButtonOverlayMode",
            typeof(TabViewCloseButtonOverlayMode),
            typeof(TabControlHelper),
            new PropertyMetadata(TabViewCloseButtonOverlayMode.Auto));

        /// <summary>
        /// Gets a value that indicates the behavior of the close button within tabs.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>A value of the enumeration that describes the behavior of the close button within tabs. The default is Auto.</returns>
        public static TabViewCloseButtonOverlayMode GetCloseButtonOverlayMode(TabControl element)
        {
            return (TabViewCloseButtonOverlayMode)element.GetValue(CloseButtonOverlayModeProperty);
        }

        /// <summary>
        /// Sets a value that indicates the behavior of the close button within tabs.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <param name="value">A value of the enumeration that describes the behavior of the close button within tabs. The default is Auto.</param>
        public static void SetCloseButtonOverlayMode(TabControl element, TabViewCloseButtonOverlayMode value)
        {
            element.SetValue(CloseButtonOverlayModeProperty, value);
        }

        #endregion

        #region TabWidthMode

        /// <summary>
        /// Identifies the TabWidthMode dependency property.
        /// </summary>
        public static readonly DependencyProperty TabWidthModeProperty = DependencyProperty.RegisterAttached(
            "TabWidthMode",
            typeof(TabViewWidthMode),
            typeof(TabControlHelper),
            new PropertyMetadata(TabViewWidthMode.SizeToContent));

        /// <summary>
        /// Gets how the tabs should be sized.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The enum for how the tabs should be sized.</returns>
        public static TabViewWidthMode TabWidthModeMode(TabControl element)
        {
            return (TabViewWidthMode)element.GetValue(TabWidthModeProperty);
        }

        /// <summary>
        /// Sets how the tabs should be sized.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <param name="value">The enum for how the tabs should be sized.</param>
        public static void SetTabWidthMode(TabControl element, TabViewWidthMode value)
        {
            element.SetValue(TabWidthModeProperty, value);
        }

        #endregion

        #region IsAddTabButtonVisible

        /// <summary>
        /// Identifies the IsAddTabButtonVisible dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAddTabButtonVisibleProperty = DependencyProperty.RegisterAttached(
            "IsAddTabButtonVisible",
            typeof(bool),
            typeof(TabControlHelper),
            new PropertyMetadata(true));

        /// <summary>
        /// Gets whether the add (+) tab button is visible.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>Whether the add (+) tab button is visible. The default is Ture.</returns>
        public static bool GetIsAddTabButtonVisible(TabControl element)
        {
            return (bool)element.GetValue(IsAddTabButtonVisibleProperty);
        }

        /// <summary>
        /// Sets whether the add (+) tab button is visible.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <param name="value">Whether the add (+) tab button is visible.</param>
        public static void SetIsAddTabButtonVisible(TabControl element, bool value)
        {
            element.SetValue(IsAddTabButtonVisibleProperty, value);
        }

        #endregion

        #region TabControlHelperEvents

        /// <summary>
        /// Identifies the TabControlHelperEvents dependency property.
        /// </summary>
        public static readonly DependencyProperty TabControlHelperEventsProperty = DependencyProperty.RegisterAttached(
            "TabControlHelperEvents",
            typeof(TabControlHelperEvents),
            typeof(TabControlHelper),
            new PropertyMetadata(new TabControlHelperEvents()));

        public static TabControlHelperEvents GetTabControlHelperEvents(TabControl element)
        {
            return (TabControlHelperEvents)element.GetValue(TabControlHelperEventsProperty);
        }

        #endregion

        #region AddTabButtonCommand

        /// <summary>
        /// Identifies the AddTabButtonCommand dependency property.
        /// </summary>
        public static readonly DependencyProperty AddTabButtonCommandProperty = DependencyProperty.RegisterAttached(
            "AddTabButtonCommand",
            typeof(ICommand),
            typeof(TabControlHelper));

        /// <summary>
        /// Gets the command to invoke when the add (+) button is tapped.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The command to invoke when the add (+) button is tapped.</returns>
        public static ICommand GetAddTabButtonCommand(TabControl element)
        {
            return (ICommand)element.GetValue(AddTabButtonCommandProperty);
        }

        /// <summary>
        /// Sets the command to invoke when the add (+) button is tapped.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <param name="value">The command to invoke when the add (+) button is tapped.</param>
        public static void SetAddTabButtonCommand(TabControl element, ICommand value)
        {
            element.SetValue(AddTabButtonCommandProperty, value);
        }

        #endregion

        #region AddTabButtonCommandParameter

        /// <summary>
        /// Identifies the AddTabButtonCommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty AddTabButtonCommandParameterProperty = DependencyProperty.RegisterAttached(
            "AddTabButtonCommandParameter",
            typeof(object),
            typeof(TabControlHelper));

        /// <summary>
        /// Gets the parameter to pass to the AddTabButtonCommand property.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The parameter to pass to the AddTabButtonCommand property.</returns>
        public static object GetAddTabButtonCommandParameter(TabControl element)
        {
            return (object)element.GetValue(AddTabButtonCommandParameterProperty);
        }

        /// <summary>
        /// Sets the parameter to pass to the AddTabButtonCommand property.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <param name="value">The parameter to pass to the AddTabButtonCommand property.</param>
        public static void SetAddTabButtonCommandParameter(TabControl element, object value)
        {
            element.SetValue(AddTabButtonCommandParameterProperty, value);
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            TabControl TabControl = sender as TabControl;
            Button AddButton = (Button)TabControl.FindDescendantByName("AddButton");

            if (AddButton != null)
            {
                void OnAddButtonClick(object sender, RoutedEventArgs e)
                {
                    GetTabControlHelperEvents(TabControl).AddTabButtonClick?.Invoke(TabControl, e);
                }
                AddButton.Click += OnAddButtonClick;
            }
        }
    }

    /// <summary>
    /// Defines constants that specify the width of the tabs.
    /// </summary>
    public enum TabViewWidthMode
    {
        /// <summary>
        /// Each tab has the same width.
        /// </summary>
        Equal = 0,
        /// <summary>
        /// Each tab adjusts its width to the content within the tab.
        /// </summary>
        SizeToContent = 1,
        /// <summary>
        /// Unselected tabs collapse to show only their icon. The selected tab adjusts to display the content within the tab.
        /// </summary>
        Compact = 2,
    }

    /// <summary>
    /// Defines constants that describe the behavior of the close button contained within each <see cref="TabItem"/>.
    /// </summary>
    public enum TabViewCloseButtonOverlayMode
    {
        /// <summary>
        /// Behavior is defined by the framework. Default.
        /// This value maps to Always.
        /// </summary>
        Auto = 0,
        /// <summary>
        /// The selected tab always shows the close button if it is closable. Unselected tabs show the close button when the tab is closable and the user has their pointer over the tab.
        /// </summary>
        OnPointerOver = 1,
        /// <summary>
        /// The selected tab always shows the close button if it is closable. Unselected tabs always show the close button if they are closable.
        /// </summary>
        Always = 2,
    }

    /// <summary>
    /// Provides data for a tab close event.
    /// </summary>
    public sealed class TabViewTabCloseRequestedEventArgs
    {
        /// <summary>
        /// Gets a value that represents the data context for the tab in which a close is being requested.
        /// </summary>
        public object Item { get; private set; }

        /// <summary>
        /// Gets the tab in which a close is being requested.
        /// </summary>
        public TabItem Tab { get; private set; }

        internal TabViewTabCloseRequestedEventArgs(object item, TabItem tab)
        {
            Item = item;
            Tab = tab;
        }
    }

    /// <summary>
    /// Events for TabControlHelper.
    /// </summary>
    public class TabControlHelperEvents
    {
        /// <summary>
        /// Occurs when the add (+) tab button has been clicked.
        /// </summary>
        public TypedEventHandler<TabControl, object> AddTabButtonClick;

        /// <summary>
        /// Raised when the user attempts to close a Tab via clicking the x-to-close button, CTRL+F4, or mousewheel.
        /// </summary>
        public TypedEventHandler<TabControl, TabViewTabCloseRequestedEventArgs> TabCloseRequested;
    }
}
