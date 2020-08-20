// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    partial class NavigationView
    {
        #region IsPaneOpen

        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register(
                nameof(IsPaneOpen),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(true, OnIsPaneOpenPropertyChanged));

        public bool IsPaneOpen
        {
            get => (bool)GetValue(IsPaneOpenProperty);
            set => SetValue(IsPaneOpenProperty, value);
        }

        private static void OnIsPaneOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region CompactModeThresholdWidth

        public static readonly DependencyProperty CompactModeThresholdWidthProperty =
            DependencyProperty.Register(
                nameof(CompactModeThresholdWidth),
                typeof(double),
                typeof(NavigationView),
                new PropertyMetadata(641.0, OnCompactModeThresholdWidthPropertyChanged, CoerceToGreaterThanZero));

        public double CompactModeThresholdWidth
        {
            get => (double)GetValue(CompactModeThresholdWidthProperty);
            set => SetValue(CompactModeThresholdWidthProperty, value);
        }

        private static void OnCompactModeThresholdWidthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region ExpandedModeThresholdWidth

        public static readonly DependencyProperty ExpandedModeThresholdWidthProperty =
            DependencyProperty.Register(
                nameof(ExpandedModeThresholdWidth),
                typeof(double),
                typeof(NavigationView),
                new PropertyMetadata(1008.0, OnExpandedModeThresholdWidthPropertyChanged, CoerceToGreaterThanZero));

        public double ExpandedModeThresholdWidth
        {
            get => (double)GetValue(ExpandedModeThresholdWidthProperty);
            set => SetValue(ExpandedModeThresholdWidthProperty, value);
        }

        private static void OnExpandedModeThresholdWidthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region FooterMenuItems

        private static readonly DependencyPropertyKey FooterMenuItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(FooterMenuItems),
                typeof(IList),
                typeof(NavigationView),
                new PropertyMetadata(OnFooterMenuItemsPropertyChanged));

        private static readonly DependencyProperty FooterMenuItemsProperty =
            FooterMenuItemsPropertyKey.DependencyProperty;

        public IList FooterMenuItems
        {
            get => (IList)GetValue(FooterMenuItemsProperty);
        }

        private static void OnFooterMenuItemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region FooterMenuItemsSource

        public static readonly DependencyProperty FooterMenuItemsSourceProperty =
            DependencyProperty.Register(
                nameof(FooterMenuItemsSource),
                typeof(object),
                typeof(NavigationView),
                new PropertyMetadata(OnFooterMenuItemsSourcePropertyChanged));

        public object FooterMenuItemsSource
        {
            get => GetValue(FooterMenuItemsSourceProperty);
            set => SetValue(FooterMenuItemsSourceProperty, value);
        }

        private static void OnFooterMenuItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region PaneFooter

        public static readonly DependencyProperty PaneFooterProperty =
            DependencyProperty.Register(
                nameof(PaneFooter),
                typeof(UIElement),
                typeof(NavigationView),
                new PropertyMetadata(OnPaneFooterPropertyChanged));

        public UIElement PaneFooter
        {
            get => (UIElement)GetValue(PaneFooterProperty);
            set => SetValue(PaneFooterProperty, value);
        }

        private static void OnPaneFooterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(NavigationView),
                new PropertyMetadata(OnHeaderPropertyChanged));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void OnHeaderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region HeaderTemplate

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(NavigationView),
                new PropertyMetadata(OnHeaderTemplatePropertyChanged));

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        private static void OnHeaderTemplatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region DisplayMode

        private static readonly DependencyPropertyKey DisplayModePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(DisplayMode),
                typeof(NavigationViewDisplayMode),
                typeof(NavigationView),
                new PropertyMetadata(NavigationViewDisplayMode.Minimal, OnDisplayModePropertyChanged));

        public static readonly DependencyProperty DisplayModeProperty = DisplayModePropertyKey.DependencyProperty;

        public NavigationViewDisplayMode DisplayMode
        {
            get => (NavigationViewDisplayMode)GetValue(DisplayModeProperty);
        }

        private static void OnDisplayModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region IsSettingsVisible

        public static readonly DependencyProperty IsSettingsVisibleProperty =
            DependencyProperty.Register(
                nameof(IsSettingsVisible),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(true, OnIsSettingsVisiblePropertyChanged));

        public bool IsSettingsVisible
        {
            get => (bool)GetValue(IsSettingsVisibleProperty);
            set => SetValue(IsSettingsVisibleProperty, value);
        }

        private static void OnIsSettingsVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region IsPaneToggleButtonVisible

        public static readonly DependencyProperty IsPaneToggleButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPaneToggleButtonVisible),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(true, OnIsPaneToggleButtonVisiblePropertyChanged));

        public bool IsPaneToggleButtonVisible
        {
            get => (bool)GetValue(IsPaneToggleButtonVisibleProperty);
            set => SetValue(IsPaneToggleButtonVisibleProperty, value);
        }

        private static void OnIsPaneToggleButtonVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region AlwaysShowHeader

        public static readonly DependencyProperty AlwaysShowHeaderProperty =
            DependencyProperty.Register(
                nameof(AlwaysShowHeader),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(true, OnAlwaysShowHeaderPropertyChanged));

        public bool AlwaysShowHeader
        {
            get => (bool)GetValue(AlwaysShowHeaderProperty);
            set => SetValue(AlwaysShowHeaderProperty, value);
        }

        private static void OnAlwaysShowHeaderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region CompactPaneLength

        public static readonly DependencyProperty CompactPaneLengthProperty =
            DependencyProperty.Register(
                nameof(CompactPaneLength),
                typeof(double),
                typeof(NavigationView),
                new PropertyMetadata(48.0, OnCompactPaneLengthPropertyChanged, CoerceToGreaterThanZero));

        public double CompactPaneLength
        {
            get => (double)GetValue(CompactPaneLengthProperty);
            set => SetValue(CompactPaneLengthProperty, value);
        }

        private static void OnCompactPaneLengthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region OpenPaneLength

        public static readonly DependencyProperty OpenPaneLengthProperty =
            DependencyProperty.Register(
                nameof(OpenPaneLength),
                typeof(double),
                typeof(NavigationView),
                new PropertyMetadata(320.0, OnOpenPaneLengthPropertyChanged, CoerceToGreaterThanZero));

        public double OpenPaneLength
        {
            get => (double)GetValue(OpenPaneLengthProperty);
            set => SetValue(OpenPaneLengthProperty, value);
        }

        private static void OnOpenPaneLengthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region PaneToggleButtonStyle

        public static readonly DependencyProperty PaneToggleButtonStyleProperty =
            DependencyProperty.Register(
                nameof(PaneToggleButtonStyle),
                typeof(Style),
                typeof(NavigationView),
                new PropertyMetadata(OnPaneToggleButtonStylePropertyChanged));

        public Style PaneToggleButtonStyle
        {
            get => (Style)GetValue(PaneToggleButtonStyleProperty);
            set => SetValue(PaneToggleButtonStyleProperty, value);
        }

        private static void OnPaneToggleButtonStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region SelectedItem

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(NavigationView),
                new PropertyMetadata(OnSelectedItemPropertyChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region MenuItems

        private static readonly DependencyPropertyKey MenuItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(MenuItems),
                typeof(IList),
                typeof(NavigationView),
                new PropertyMetadata(OnMenuItemsPropertyChanged));

        private static readonly DependencyProperty MenuItemsProperty =
            MenuItemsPropertyKey.DependencyProperty;

        public IList MenuItems
        {
            get => (IList)GetValue(MenuItemsProperty);
        }

        private static void OnMenuItemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region MenuItemsSource

        public static readonly DependencyProperty MenuItemsSourceProperty =
            DependencyProperty.Register(
                nameof(MenuItemsSource),
                typeof(object),
                typeof(NavigationView),
                new PropertyMetadata(OnMenuItemsSourcePropertyChanged));

        public object MenuItemsSource
        {
            get => GetValue(MenuItemsSourceProperty);
            set => SetValue(MenuItemsSourceProperty, value);
        }

        private static void OnMenuItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region SettingsItem

        private static readonly DependencyPropertyKey SettingsItemPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(SettingsItem),
                typeof(object),
                typeof(NavigationView),
                new PropertyMetadata(OnSettingsItemPropertyChanged));

        public static readonly DependencyProperty SettingsItemProperty = SettingsItemPropertyKey.DependencyProperty;

        public object SettingsItem
        {
            get => GetValue(SettingsItemProperty);
        }

        private static void OnSettingsItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region AutoSuggestBox

        public static readonly DependencyProperty AutoSuggestBoxProperty =
            DependencyProperty.Register(
                nameof(AutoSuggestBox),
                typeof(AutoSuggestBox),
                typeof(NavigationView),
                new PropertyMetadata(OnAutoSuggestBoxPropertyChanged));

        public AutoSuggestBox AutoSuggestBox
        {
            get => (AutoSuggestBox)GetValue(AutoSuggestBoxProperty);
            set => SetValue(AutoSuggestBoxProperty, value);
        }

        private static void OnAutoSuggestBoxPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region MenuItemTemplate

        public static readonly DependencyProperty MenuItemTemplateProperty =
            DependencyProperty.Register(
                nameof(MenuItemTemplate),
                typeof(DataTemplate),
                typeof(NavigationView),
                new PropertyMetadata(OnMenuItemTemplatePropertyChanged));

        public DataTemplate MenuItemTemplate
        {
            get => (DataTemplate)GetValue(MenuItemTemplateProperty);
            set => SetValue(MenuItemTemplateProperty, value);
        }

        private static void OnMenuItemTemplatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region MenuItemTemplateSelector

        public static readonly DependencyProperty MenuItemTemplateSelectorProperty =
            DependencyProperty.Register(
                nameof(MenuItemTemplateSelector),
                typeof(DataTemplateSelector),
                typeof(NavigationView),
                new PropertyMetadata(OnMenuItemTemplateSelectorPropertyChanged));

        public DataTemplateSelector MenuItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(MenuItemTemplateSelectorProperty);
            set => SetValue(MenuItemTemplateSelectorProperty, value);
        }

        private static void OnMenuItemTemplateSelectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region MenuItemContainerStyle

        public static readonly DependencyProperty MenuItemContainerStyleProperty =
            DependencyProperty.Register(
                nameof(MenuItemContainerStyle),
                typeof(Style),
                typeof(NavigationView),
                new PropertyMetadata(OnMenuItemContainerStylePropertyChanged));

        public Style MenuItemContainerStyle
        {
            get => (Style)GetValue(MenuItemContainerStyleProperty);
            set => SetValue(MenuItemContainerStyleProperty, value);
        }

        private static void OnMenuItemContainerStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region MenuItemContainerStyleSelector

        public static readonly DependencyProperty MenuItemContainerStyleSelectorProperty =
            DependencyProperty.Register(
                nameof(MenuItemContainerStyleSelector),
                typeof(StyleSelector),
                typeof(NavigationView),
                new PropertyMetadata(OnMenuItemContainerStyleSelectorPropertyChanged));

        public StyleSelector MenuItemContainerStyleSelector
        {
            get => (StyleSelector)GetValue(MenuItemContainerStyleSelectorProperty);
            set => SetValue(MenuItemContainerStyleSelectorProperty, value);
        }

        private static void OnMenuItemContainerStyleSelectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NavigationView)sender).OnMenuItemContainerStyleSelectorPropertyChanged(args);
        }

        private void OnMenuItemContainerStyleSelectorPropertyChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        #endregion

        #region IsBackButtonVisible

        public static readonly DependencyProperty IsBackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsBackButtonVisible),
                typeof(NavigationViewBackButtonVisible),
                typeof(NavigationView),
                new PropertyMetadata(NavigationViewBackButtonVisible.Auto, OnIsBackButtonVisiblePropertyChanged));

        public NavigationViewBackButtonVisible IsBackButtonVisible
        {
            get => (NavigationViewBackButtonVisible)GetValue(IsBackButtonVisibleProperty);
            set => SetValue(IsBackButtonVisibleProperty, value);
        }

        private static void OnIsBackButtonVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region IsBackEnabled

        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(
                nameof(IsBackEnabled),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(OnIsBackEnabledPropertyChanged));

        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        private static void OnIsBackEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region PaneTitle

        public static readonly DependencyProperty PaneTitleProperty =
            DependencyProperty.Register(
                nameof(PaneTitle),
                typeof(string),
                typeof(NavigationView),
                new PropertyMetadata(string.Empty, OnPaneTitlePropertyChanged));

        public string PaneTitle
        {
            get => (string)GetValue(PaneTitleProperty);
            set => SetValue(PaneTitleProperty, value);
        }

        private static void OnPaneTitlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region PaneDisplayMode

        public static readonly DependencyProperty PaneDisplayModeProperty =
            DependencyProperty.Register(
                nameof(PaneDisplayMode),
                typeof(NavigationViewPaneDisplayMode),
                typeof(NavigationView),
                new PropertyMetadata(NavigationViewPaneDisplayMode.Auto, OnPaneDisplayModePropertyChanged));

        public NavigationViewPaneDisplayMode PaneDisplayMode
        {
            get => (NavigationViewPaneDisplayMode)GetValue(PaneDisplayModeProperty);
            set => SetValue(PaneDisplayModeProperty, value);
        }

        private static void OnPaneDisplayModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region PaneHeader

        public static readonly DependencyProperty PaneHeaderProperty =
            DependencyProperty.Register(
                nameof(PaneHeader),
                typeof(UIElement),
                typeof(NavigationView),
                null);

        public UIElement PaneHeader
        {
            get => (UIElement)GetValue(PaneHeaderProperty);
            set => SetValue(PaneHeaderProperty, value);
        }

        #endregion

        #region PaneCustomContent

        public static readonly DependencyProperty PaneCustomContentProperty =
            DependencyProperty.Register(
                nameof(PaneCustomContent),
                typeof(UIElement),
                typeof(NavigationView),
                null);

        public UIElement PaneCustomContent
        {
            get => (UIElement)GetValue(PaneCustomContentProperty);
            set => SetValue(PaneCustomContentProperty, value);
        }

        #endregion

        #region ContentOverlay

        public static readonly DependencyProperty ContentOverlayProperty =
            DependencyProperty.Register(
                nameof(ContentOverlay),
                typeof(UIElement),
                typeof(NavigationView),
                null);

        public UIElement ContentOverlay
        {
            get => (UIElement)GetValue(ContentOverlayProperty);
            set => SetValue(ContentOverlayProperty, value);
        }

        #endregion

        #region IsPaneVisible

        public static readonly DependencyProperty IsPaneVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPaneVisible),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(true, OnIsPaneVisiblePropertyChanged));

        public bool IsPaneVisible
        {
            get => (bool)GetValue(IsPaneVisibleProperty);
            set => SetValue(IsPaneVisibleProperty, value);
        }

        private static void OnIsPaneVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region SelectionFollowsFocus

        public static readonly DependencyProperty SelectionFollowsFocusProperty =
            DependencyProperty.Register(
                nameof(SelectionFollowsFocus),
                typeof(NavigationViewSelectionFollowsFocus),
                typeof(NavigationView),
                new PropertyMetadata(NavigationViewSelectionFollowsFocus.Disabled, OnSelectionFollowsFocusPropertyChanged));

        public NavigationViewSelectionFollowsFocus SelectionFollowsFocus
        {
            get => (NavigationViewSelectionFollowsFocus)GetValue(SelectionFollowsFocusProperty);
            set => SetValue(SelectionFollowsFocusProperty, value);
        }

        private static void OnSelectionFollowsFocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(NavigationViewTemplateSettings),
                typeof(NavigationView),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public NavigationViewTemplateSettings TemplateSettings
        {
            get => (NavigationViewTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsPropertyKey, value);
        }

        #endregion

        #region ShoulderNavigationEnabled

        public static readonly DependencyProperty ShoulderNavigationEnabledProperty =
            DependencyProperty.Register(
                nameof(ShoulderNavigationEnabled),
                typeof(NavigationViewShoulderNavigationEnabled),
                typeof(NavigationView),
                new PropertyMetadata(NavigationViewShoulderNavigationEnabled.Never, OnShoulderNavigationEnabledPropertyChanged));

        public NavigationViewShoulderNavigationEnabled ShoulderNavigationEnabled
        {
            get => (NavigationViewShoulderNavigationEnabled)GetValue(ShoulderNavigationEnabledProperty);
            set => SetValue(ShoulderNavigationEnabledProperty, value);
        }

        private static void OnShoulderNavigationEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region OverflowLabelMode

        public static readonly DependencyProperty OverflowLabelModeProperty =
            DependencyProperty.Register(
                nameof(OverflowLabelMode),
                typeof(NavigationViewOverflowLabelMode),
                typeof(NavigationView),
                new PropertyMetadata(NavigationViewOverflowLabelMode.MoreLabel, OnOverflowLabelModePropertyChanged));

        public NavigationViewOverflowLabelMode OverflowLabelMode
        {
            get => (NavigationViewOverflowLabelMode)GetValue(OverflowLabelModeProperty);
            set => SetValue(OverflowLabelModeProperty, value);
        }

        private static void OnOverflowLabelModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        #region IsTitleBarAutoPaddingEnabled

        public static readonly DependencyProperty IsTitleBarAutoPaddingEnabledProperty =
            DependencyProperty.Register(
                nameof(IsTitleBarAutoPaddingEnabled),
                typeof(bool),
                typeof(NavigationView),
                new PropertyMetadata(true, OnIsTitleBarAutoPaddingEnabledPropertyChanged));

        public bool IsTitleBarAutoPaddingEnabled
        {
            get => (bool)GetValue(IsTitleBarAutoPaddingEnabledProperty);
            set => SetValue(IsTitleBarAutoPaddingEnabledProperty, value);
        }

        private static void OnIsTitleBarAutoPaddingEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (NavigationView)sender;
            owner.PropertyChanged(args);
        }

        #endregion

        public event TypedEventHandler<NavigationView, NavigationViewSelectionChangedEventArgs> SelectionChanged;
        public event TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> ItemInvoked;
        public event TypedEventHandler<NavigationView, NavigationViewDisplayModeChangedEventArgs> DisplayModeChanged;
        public event TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> BackRequested;
        public event TypedEventHandler<NavigationView, object> PaneClosed;
        public event TypedEventHandler<NavigationView, NavigationViewPaneClosingEventArgs> PaneClosing;
        public event TypedEventHandler<NavigationView, object> PaneOpened;
        public event TypedEventHandler<NavigationView, object> PaneOpening;
        public event TypedEventHandler<NavigationView, NavigationViewItemExpandingEventArgs> Expanding;
        public event TypedEventHandler<NavigationView, NavigationViewItemCollapsedEventArgs> Collapsed;

        private static object CoerceToGreaterThanZero(DependencyObject d, object baseValue)
        {
            if (baseValue is double value)
            {
                ((NavigationView)d).CoerceToGreaterThanZero(ref value);
                return value;
            }
            return baseValue;
        }
    }
}
