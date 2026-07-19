using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.InfoBar;

[TestClass]
public class InfoBarApiTests
{
    [TestMethod]
    public void InfoBarDefaultsTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar();

            Assert.IsFalse(infoBar.IsOpen);
            Assert.AreEqual(string.Empty, infoBar.Title);
            Assert.AreEqual(string.Empty, infoBar.Message);
            Assert.AreEqual(InfoBarSeverity.Informational, infoBar.Severity);
            Assert.IsNull(infoBar.IconSource);
            Assert.IsTrue(infoBar.IsIconVisible);
            Assert.IsTrue(infoBar.IsClosable);
            Assert.IsNull(infoBar.ActionButton);
            Assert.IsNull(infoBar.Content);
            Assert.IsNull(infoBar.ContentTemplate);
            Assert.IsNotNull(infoBar.TemplateSettings);
        });
    }

    [TestMethod]
    public void InfoBarCloseEventsTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Title = "Update",
                Message = "Restart required"
            };

            var events = new List<string>();
            var cancelClosing = false;
            infoBar.CloseButtonClick += (_, _) => events.Add("CloseButtonClick");
            infoBar.Closing += (_, args) =>
            {
                events.Add($"Closing: {args.Reason}");
                args.Cancel = cancelClosing;
            };
            infoBar.Closed += (_, args) => events.Add($"Closed: {args.Reason}");

            using var host = new TestWindowHost(infoBar, width: 400, height: 120);
            var closeButton = FindNamedDescendant<Button>(infoBar, "CloseButton");

            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "CloseButtonClick", "Closing: CloseButton", "Closed: CloseButton" },
                events);
            Assert.IsFalse(infoBar.IsOpen);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Border>(infoBar, "ContentRoot").Visibility);

            infoBar.IsOpen = true;
            cancelClosing = true;
            events.Clear();

            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "CloseButtonClick", "Closing: CloseButton" },
                events);
            Assert.IsTrue(infoBar.IsOpen);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<Border>(infoBar, "ContentRoot").Visibility);

            cancelClosing = false;
            events.Clear();

            infoBar.IsOpen = false;
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "Closing: Programmatic", "Closed: Programmatic" },
                events);
        });
    }

    [TestMethod]
    public void InfoBarIconAndCloseVisibilityTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar { IsOpen = true };
            using var host = new TestWindowHost(infoBar, width: 400, height: 120);

            var standardIconArea = FindNamedDescendant<FrameworkElement>(infoBar, "StandardIconArea");
            var userIconBox = FindNamedDescendant<FrameworkElement>(infoBar, "UserIconBox");
            var closeButton = FindNamedDescendant<Button>(infoBar, "CloseButton");
            var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");

            Assert.AreEqual("Close", AutomationProperties.GetName(closeButton));
            Assert.IsInstanceOfType(closeButton.ToolTip, typeof(ToolTip));
            Assert.AreEqual("Close", ((ToolTip)closeButton.ToolTip).Content);
            Assert.AreEqual(Symbol.Cancel, FindDescendant<SymbolIcon>(closeButton).Symbol);
            Assert.AreEqual("StandardIconVisible", GetCurrentStateName(contentRoot, "IconStates"));
            Assert.AreEqual("CloseButtonVisible", GetCurrentStateName(contentRoot, "CloseButtonStates"));
            Assert.AreEqual(Visibility.Visible, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Collapsed, userIconBox.Visibility);
            Assert.AreEqual(Visibility.Visible, closeButton.Visibility);

            infoBar.IconSource = new SymbolIconSource { Symbol = Symbol.Setting };
            host.UpdateLayout();

            Assert.IsInstanceOfType(infoBar.TemplateSettings.IconElement, typeof(SymbolIcon));
            Assert.AreEqual("UserIconVisible", GetCurrentStateName(contentRoot, "IconStates"));
            Assert.AreEqual(Visibility.Collapsed, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Visible, userIconBox.Visibility);

            infoBar.IconSource = null;
            host.UpdateLayout();

            Assert.IsNull(infoBar.TemplateSettings.IconElement);
            Assert.AreEqual("StandardIconVisible", GetCurrentStateName(contentRoot, "IconStates"));
            Assert.AreEqual(Visibility.Visible, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Collapsed, userIconBox.Visibility);

            infoBar.IsIconVisible = false;
            host.UpdateLayout();

            Assert.AreEqual("NoIconVisible", GetCurrentStateName(contentRoot, "IconStates"));
            Assert.AreEqual(Visibility.Collapsed, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Collapsed, userIconBox.Visibility);

            infoBar.IsClosable = false;
            host.UpdateLayout();

            Assert.AreEqual("CloseButtonCollapsed", GetCurrentStateName(contentRoot, "CloseButtonStates"));
            Assert.AreEqual(Visibility.Collapsed, closeButton.Visibility);
        });
    }

    [TestMethod]
    public void InfoBarSeverityAndContentPositionTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Content = new TextBlock { Text = "details" }
            };

            using var host = new TestWindowHost(infoBar, width: 400, height: 140);
            var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
            var standardIcon = FindNamedDescendant<TextBlock>(infoBar, "StandardIcon");
            var contentArea = FindNamedDescendant<FrameworkElement>(infoBar, "ContentArea");

            Assert.AreEqual("Informational", GetCurrentStateName(contentRoot, "SeverityLevels"));
            Assert.AreEqual("NoBannerContent", GetCurrentStateName(contentRoot, "ContentStates"));
            Assert.AreEqual(0, Grid.GetRow(contentArea));
            Assert.AreEqual("\uF13F", standardIcon.Text);
            Assert.AreEqual("Informational icon", AutomationProperties.GetName(standardIcon));

            infoBar.Title = "Title";
            host.UpdateLayout();

            Assert.AreEqual("BannerContent", GetCurrentStateName(contentRoot, "ContentStates"));
            Assert.AreEqual(1, Grid.GetRow(contentArea));

            infoBar.Severity = InfoBarSeverity.Error;
            host.UpdateLayout();

            Assert.AreEqual("Error", GetCurrentStateName(contentRoot, "SeverityLevels"));
            Assert.AreEqual("\uF13D", standardIcon.Text);
            Assert.AreEqual("Error icon", AutomationProperties.GetName(standardIcon));
        });
    }

    [TestMethod]
    public void InfoBarForegroundStateUsesVisualStateBindingSetter()
    {
        WpfTestHost.Run(() =>
        {
            var firstForeground = new SolidColorBrush(Colors.Red);
            var secondForeground = new SolidColorBrush(Colors.Green);
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Title = "Title",
                Message = "Message",
                Foreground = firstForeground
            };

            using var host = new TestWindowHost(infoBar, width: 400, height: 120);
            var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
            var title = FindNamedDescendant<TextBlock>(infoBar, "Title");
            var message = FindNamedDescendant<TextBlock>(infoBar, "Message");

            Assert.AreEqual("ForegroundSet", GetCurrentStateName(contentRoot, "ForegroundStates"));
            Assert.AreSame(firstForeground, title.Foreground);
            Assert.AreSame(firstForeground, message.Foreground);

            infoBar.Foreground = secondForeground;
            host.UpdateLayout();

            Assert.AreSame(secondForeground, title.Foreground);
            Assert.AreSame(secondForeground, message.Foreground);
        });
    }

    [TestMethod]
    public void InfoBarTemplateUsesWinUIContentPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            var bannerContent = new TextBlock { Text = "details" };
            var actionButton = new HyperlinkButton { Content = "Learn more" };
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Title = "Title",
                Content = bannerContent,
                ActionButton = actionButton
            };

            using var host = new TestWindowHost(infoBar, width: 400, height: 140);

            var closeButton = FindNamedDescendant<Button>(infoBar, "CloseButton");
            var closeButtonChrome = FindNamedDescendant<Border>(closeButton, "ContentBorder");
            Assert.AreEqual(closeButton.Width, closeButtonChrome.Width);
            Assert.AreEqual(closeButton.Height, closeButtonChrome.Height);
            Assert.AreEqual(1, closeButtonChrome.BorderThickness.Left);

            var contentArea = FindNamedDescendant<ContentPresenterEx>(infoBar, "ContentArea");
            Assert.AreSame(bannerContent, contentArea.Content);
            Assert.AreEqual(1, Grid.GetColumn(contentArea));
            Assert.AreEqual(1, Grid.GetRow(contentArea));
            Assert.AreEqual(VerticalAlignment.Center, contentArea.VerticalAlignment);

            var layoutRoot = FindNamedDescendant<Border>(infoBar, "LayoutRoot");
            Assert.AreEqual(new Thickness(16, 0, 0, 0), layoutRoot.Padding);
            Assert.AreEqual(infoBar.CornerRadius, layoutRoot.CornerRadius);

            var actionPresenter = FindContentPresenter(infoBar, actionButton);
            Assert.AreEqual(VerticalAlignment.Top, actionPresenter.VerticalAlignment);
            Assert.AreEqual(
                new Thickness(16, 8, 0, 0),
                InfoBarPanel.GetHorizontalOrientationMargin(actionPresenter));
            Assert.AreEqual(
                new Thickness(0, 12, 0, 0),
                InfoBarPanel.GetVerticalOrientationMargin(actionPresenter));
            Assert.AreEqual(new Thickness(-12, 0, 0, 0), actionButton.Margin);
        });
    }

    [TestMethod]
    public void InfoBarStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/InfoBar/InfoBar.xaml", UriKind.Relative)
            };
            var infoBarStyle = (Style)resources[typeof(ModernWpf.Controls.InfoBar)];
            var closeButtonStyle = (Style)resources["InfoBarCloseButtonStyle"];
            var actionButton = new HyperlinkButton { Content = "Learn more" };
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Title = "Title",
                Message = "Message",
                ActionButton = actionButton,
                Style = infoBarStyle
            };
            infoBar.Resources.MergedDictionaries.Add(resources);

            using var host = new TestWindowHost(infoBar, width: 400, height: 120);
            host.UpdateLayout();

            Assert.AreEqual(typeof(ModernWpf.Controls.InfoBar), infoBarStyle.TargetType);
            AssertSetterValue(infoBarStyle, Control.IsTabStopProperty, false);
            AssertSetterValue(infoBarStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(infoBarStyle, ModernWpf.Controls.InfoBar.CloseButtonStyleProperty, closeButtonStyle);
            AssertSolidColorBrushSetterColor(infoBarStyle, Control.BackgroundProperty, Colors.Transparent);
            AssertDynamicResourceSetter(infoBarStyle, Control.BorderBrushProperty, "InfoBarBorderBrush");
            AssertSetterValue(infoBarStyle, Control.BorderThicknessProperty, resources["InfoBarBorderThickness"]);
            AssertDynamicResourceSetter(infoBarStyle, ModernWpf.Controls.InfoBar.CornerRadiusProperty, "ControlCornerRadius");
            var infoBarTemplate = GetSetterValue(infoBarStyle, Control.TemplateProperty) as ControlTemplate;
            Assert.IsNotNull(infoBarTemplate);
            Assert.AreEqual(typeof(ModernWpf.Controls.InfoBar), infoBarTemplate!.TargetType);

            Assert.AreEqual(typeof(ButtonBase), closeButtonStyle.TargetType);
            AssertDynamicResourceSetter(closeButtonStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(closeButtonStyle, Control.BackgroundProperty, "ButtonBackground");
            AssertDynamicResourceSetter(closeButtonStyle, Control.ForegroundProperty, "ButtonForeground");
            AssertDynamicResourceSetter(closeButtonStyle, Control.BorderBrushProperty, "ButtonBorderBrush");
            AssertDynamicResourceSetter(closeButtonStyle, Control.BorderThicknessProperty, "ButtonBorderThemeThickness");
            AssertDynamicResourceSetter(closeButtonStyle, Control.PaddingProperty, "ButtonPadding");
            AssertDynamicResourceSetter(closeButtonStyle, FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(closeButtonStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(closeButtonStyle, FocusVisualHelper.FocusVisualMarginProperty, new Thickness(-3));
            AssertSetterValue(closeButtonStyle, FrameworkElement.WidthProperty, resources["InfoBarCloseButtonSize"]);
            AssertSetterValue(closeButtonStyle, FrameworkElement.HeightProperty, resources["InfoBarCloseButtonSize"]);
            AssertSetterValue(closeButtonStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            AssertSetterValue(closeButtonStyle, FrameworkElement.MarginProperty, new Thickness(5));
            var closeButtonTemplate = GetSetterValue(closeButtonStyle, Control.TemplateProperty) as ControlTemplate;
            Assert.IsNotNull(closeButtonTemplate);
            Assert.AreEqual(typeof(ButtonBase), closeButtonTemplate!.TargetType);

            AssertResourceReference(infoBar, "InfoBarBorderBrush", "CardStrokeColorDefaultBrush");
            AssertResourceReference(infoBar, "InfoBarTitleForeground", "TextFillColorPrimaryBrush");
            AssertResourceReference(infoBar, "InfoBarMessageForeground", "TextFillColorPrimaryBrush");
            AssertResourceReference(infoBar, "InfoBarHyperlinkButtonForeground", "AccentTextFillColorPrimaryBrush");
            AssertResourceReference(infoBar, "InfoBarErrorSeverityBackgroundBrush", "SystemFillColorCriticalBackgroundBrush");
            AssertResourceReference(infoBar, "InfoBarWarningSeverityBackgroundBrush", "SystemFillColorCautionBackgroundBrush");
            AssertResourceReference(infoBar, "InfoBarSuccessSeverityBackgroundBrush", "SystemFillColorSuccessBackgroundBrush");
            AssertResourceReference(infoBar, "InfoBarInformationalSeverityBackgroundBrush", "SystemFillColorAttentionBackgroundBrush");
            AssertResourceReference(infoBar, "InfoBarErrorSeverityIconBackground", "SystemFillColorCriticalBrush");
            AssertResourceReference(infoBar, "InfoBarWarningSeverityIconBackground", "SystemFillColorCautionBrush");
            AssertResourceReference(infoBar, "InfoBarSuccessSeverityIconBackground", "SystemFillColorSuccessBrush");
            AssertResourceReference(infoBar, "InfoBarInformationalSeverityIconBackground", "SystemFillColorAttentionBrush");
            AssertResourceReference(infoBar, "InfoBarErrorSeverityIconForeground", "TextFillColorInverseBrush");
            AssertResourceReference(infoBar, "InfoBarWarningSeverityIconForeground", "TextFillColorInverseBrush");
            AssertResourceReference(infoBar, "InfoBarSuccessSeverityIconForeground", "TextFillColorInverseBrush");
            AssertResourceReference(infoBar, "InfoBarInformationalSeverityIconForeground", "TextFillColorInverseBrush");

            Assert.IsFalse(infoBar.IsTabStop);
            Assert.AreSame(closeButtonStyle, infoBar.CloseButtonStyle);
            Assert.AreSame(infoBar.TryFindResource("InfoBarBorderBrush"), infoBar.BorderBrush);
            Assert.AreEqual(infoBar.TryFindResource("InfoBarBorderThickness"), infoBar.BorderThickness);
            Assert.AreEqual(infoBar.TryFindResource("ControlCornerRadius"), infoBar.CornerRadius);

            var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
            var layoutRoot = FindNamedDescendant<Border>(infoBar, "LayoutRoot");
            var iconBackground = FindNamedDescendant<TextBlock>(infoBar, "IconBackground");
            var standardIcon = FindNamedDescendant<TextBlock>(infoBar, "StandardIcon");
            var title = FindNamedDescendant<TextBlock>(infoBar, "Title");
            var message = FindNamedDescendant<TextBlock>(infoBar, "Message");
            var closeButton = FindNamedDescendant<Button>(infoBar, "CloseButton");

            Assert.AreSame(contentRoot.TryFindResource("InfoBarInformationalSeverityBackgroundBrush"), contentRoot.Background);
            Assert.AreSame(infoBar.BorderBrush, contentRoot.BorderBrush);
            Assert.AreEqual(infoBar.BorderThickness, contentRoot.BorderThickness);
            Assert.AreEqual(infoBar.CornerRadius, contentRoot.CornerRadius);
            Assert.AreEqual(resources["InfoBarMinHeight"], layoutRoot.MinHeight);
            Assert.AreEqual(resources["InfoBarContentRootPadding"], layoutRoot.Padding);
            Assert.AreEqual(infoBar.CornerRadius, layoutRoot.CornerRadius);
            Assert.AreSame(contentRoot.TryFindResource("InfoBarInformationalSeverityIconBackground"), iconBackground.Foreground);
            Assert.AreSame(contentRoot.TryFindResource("InfoBarInformationalSeverityIconForeground"), standardIcon.Foreground);
            Assert.AreEqual(resources["InfoBarInformationalIconGlyph"], standardIcon.Text);
            Assert.AreSame(contentRoot.TryFindResource("InfoBarTitleForeground"), title.Foreground);
            Assert.AreSame(contentRoot.TryFindResource("InfoBarMessageForeground"), message.Foreground);
            Assert.AreSame(actionButton.TryFindResource("InfoBarHyperlinkButtonForeground"), actionButton.Foreground);
            Assert.AreEqual(resources["InfoBarHyperlinkButtonMargin"], actionButton.Margin);

            Assert.AreSame(closeButtonStyle, closeButton.Style);
            AssertResourceReference(closeButton, "ButtonBackground", "AppBarButtonBackground");
            AssertResourceReference(closeButton, "ButtonForeground", "AppBarButtonForeground");
            AssertResourceReference(closeButton, "ButtonBorderBrush", "AppBarButtonBorderBrush");
            Assert.AreSame(closeButton.TryFindResource("ButtonBackground"), closeButton.Background);
            Assert.AreSame(closeButton.TryFindResource("ButtonForeground"), closeButton.Foreground);
            Assert.AreSame(closeButton.TryFindResource("ButtonBorderBrush"), closeButton.BorderBrush);
            Assert.AreEqual(closeButton.TryFindResource("ButtonBorderThemeThickness"), closeButton.BorderThickness);
            Assert.AreEqual(closeButton.TryFindResource("ButtonPadding"), closeButton.Padding);
            Assert.AreEqual(resources["InfoBarCloseButtonSize"], closeButton.Width);
            Assert.AreEqual(resources["InfoBarCloseButtonSize"], closeButton.Height);
            Assert.AreEqual(new Thickness(5), closeButton.Margin);
            Assert.AreEqual(VerticalAlignment.Top, closeButton.VerticalAlignment);
            Assert.AreEqual(closeButton.TryFindResource("UseSystemFocusVisuals"), FocusVisualHelper.GetUseSystemFocusVisuals(closeButton));
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(closeButton));
            Assert.AreEqual(closeButton.TryFindResource("ControlCornerRadius"), closeButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));

            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Error", "ContentRoot.Background", "InfoBarErrorSeverityBackgroundBrush");
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Error", "IconBackground.Foreground", "InfoBarErrorSeverityIconBackground");
            AssertStateSetterValue(contentRoot, "SeverityLevels", "Error", "StandardIcon.Text", resources["InfoBarErrorIconGlyph"]);
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Error", "StandardIcon.Foreground", "InfoBarErrorSeverityIconForeground");
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Warning", "ContentRoot.Background", "InfoBarWarningSeverityBackgroundBrush");
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Warning", "IconBackground.Foreground", "InfoBarWarningSeverityIconBackground");
            AssertStateSetterValue(contentRoot, "SeverityLevels", "Warning", "StandardIcon.Text", resources["InfoBarWarningIconGlyph"]);
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Warning", "StandardIcon.Foreground", "InfoBarWarningSeverityIconForeground");
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Success", "ContentRoot.Background", "InfoBarSuccessSeverityBackgroundBrush");
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Success", "IconBackground.Foreground", "InfoBarSuccessSeverityIconBackground");
            AssertStateSetterValue(contentRoot, "SeverityLevels", "Success", "StandardIcon.Text", resources["InfoBarSuccessIconGlyph"]);
            AssertStateSetterDynamicResource(contentRoot, "SeverityLevels", "Success", "StandardIcon.Foreground", "InfoBarSuccessSeverityIconForeground");
            AssertStateSetterValue(contentRoot, "InfoBarVisibility", "InfoBarVisible", "ContentRoot.Visibility", "Visible");
            AssertStateSetterValue(contentRoot, "InfoBarVisibility", "InfoBarCollapsed", "ContentRoot.Visibility", "Collapsed");

            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Informational,
                "InfoBarInformationalSeverityBackgroundBrush",
                "InfoBarInformationalSeverityIconBackground",
                "InfoBarInformationalSeverityIconForeground",
                resources["InfoBarInformationalIconGlyph"]);
            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Error,
                "InfoBarErrorSeverityBackgroundBrush",
                "InfoBarErrorSeverityIconBackground",
                "InfoBarErrorSeverityIconForeground",
                resources["InfoBarErrorIconGlyph"]);
            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Warning,
                "InfoBarWarningSeverityBackgroundBrush",
                "InfoBarWarningSeverityIconBackground",
                "InfoBarWarningSeverityIconForeground",
                resources["InfoBarWarningIconGlyph"]);
            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Success,
                "InfoBarSuccessSeverityBackgroundBrush",
                "InfoBarSuccessSeverityIconBackground",
                "InfoBarSuccessSeverityIconForeground",
                resources["InfoBarSuccessIconGlyph"]);
        });
    }

    [TestMethod]
    public void InfoBarHighContrastTemplateResourcesUseSystemColorTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Severity = InfoBarSeverity.Error,
                Title = "Title",
                Message = "Message",
                ActionButton = new HyperlinkButton { Content = "Learn more" }
            };

            using var host = new TestWindowHost(infoBar, width: 400, height: 120);
            var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
            var iconBackground = FindNamedDescendant<TextBlock>(infoBar, "IconBackground");
            var standardIcon = FindNamedDescendant<TextBlock>(infoBar, "StandardIcon");
            var title = FindNamedDescendant<TextBlock>(infoBar, "Title");
            var message = FindNamedDescendant<TextBlock>(infoBar, "Message");
            var actionButton = (HyperlinkButton)infoBar.ActionButton;

            Assert.IsTrue(ThemeManager.GetHasThemeResources(contentRoot));
            var resources = contentRoot.Resources as ResourceDictionaryEx;
            Assert.IsNotNull(resources);
            resources!.Update("HighContrast");
            host.UpdateLayout();

            AssertResourceReference(contentRoot, "InfoBarBorderBrush", "SystemColorButtonTextColorBrush");
            AssertResourceReference(contentRoot, "InfoBarTitleForeground", "SystemColorButtonTextColorBrush");
            AssertResourceReference(contentRoot, "InfoBarMessageForeground", "SystemColorButtonTextColorBrush");
            AssertSolidColorBrushColor(contentRoot, "InfoBarHyperlinkButtonForeground", SystemColors.HotTrackColor);
            AssertResourceReference(contentRoot, "InfoBarErrorSeverityBackgroundBrush", "SystemColorWindowColorBrush");
            AssertResourceReference(contentRoot, "InfoBarWarningSeverityBackgroundBrush", "SystemColorWindowColorBrush");
            AssertResourceReference(contentRoot, "InfoBarSuccessSeverityBackgroundBrush", "SystemColorWindowColorBrush");
            AssertResourceReference(contentRoot, "InfoBarInformationalSeverityBackgroundBrush", "SystemColorWindowColorBrush");
            AssertResourceReference(contentRoot, "InfoBarErrorSeverityIconBackground", "SystemColorHighlightColorBrush");
            AssertResourceReference(contentRoot, "InfoBarWarningSeverityIconBackground", "SystemColorHighlightColorBrush");
            AssertResourceReference(contentRoot, "InfoBarSuccessSeverityIconBackground", "SystemColorHighlightColorBrush");
            AssertResourceReference(contentRoot, "InfoBarInformationalSeverityIconBackground", "SystemColorHighlightColorBrush");
            AssertResourceReference(contentRoot, "InfoBarErrorSeverityIconForeground", "SystemColorHighlightTextColorBrush");
            AssertResourceReference(contentRoot, "InfoBarWarningSeverityIconForeground", "SystemColorHighlightTextColorBrush");
            AssertResourceReference(contentRoot, "InfoBarSuccessSeverityIconForeground", "SystemColorHighlightTextColorBrush");
            AssertResourceReference(contentRoot, "InfoBarInformationalSeverityIconForeground", "SystemColorHighlightTextColorBrush");

            Assert.AreSame(contentRoot.TryFindResource("SystemColorWindowColorBrush"), contentRoot.Background);
            Assert.AreSame(contentRoot.TryFindResource("SystemColorHighlightColorBrush"), iconBackground.Foreground);
            Assert.AreSame(contentRoot.TryFindResource("SystemColorHighlightTextColorBrush"), standardIcon.Foreground);
            Assert.AreSame(contentRoot.TryFindResource("SystemColorButtonTextColorBrush"), title.Foreground);
            Assert.AreSame(contentRoot.TryFindResource("SystemColorButtonTextColorBrush"), message.Foreground);
            AssertSolidColorBrushColor(actionButton, "InfoBarHyperlinkButtonForeground", SystemColors.HotTrackColor);
            Assert.AreSame(actionButton.TryFindResource("InfoBarHyperlinkButtonForeground"), actionButton.Foreground);

            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Informational,
                "InfoBarInformationalSeverityBackgroundBrush",
                "InfoBarInformationalSeverityIconBackground",
                "InfoBarInformationalSeverityIconForeground",
                "\uF13F");
            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Error,
                "InfoBarErrorSeverityBackgroundBrush",
                "InfoBarErrorSeverityIconBackground",
                "InfoBarErrorSeverityIconForeground",
                "\uF13D");
            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Warning,
                "InfoBarWarningSeverityBackgroundBrush",
                "InfoBarWarningSeverityIconBackground",
                "InfoBarWarningSeverityIconForeground",
                "\uF13C");
            AssertSeverityResourceConsumption(infoBar, host, contentRoot, iconBackground, standardIcon,
                InfoBarSeverity.Success,
                "InfoBarSuccessSeverityBackgroundBrush",
                "InfoBarSuccessSeverityIconBackground",
                "InfoBarSuccessSeverityIconForeground",
                "\uF13E");
        });
    }

    [TestMethod]
    public void InfoBarAutomationPeerTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar { IsOpen = true };
            using var host = new TestWindowHost(infoBar, width: 300, height: 100);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(infoBar);

            Assert.IsNotNull(peer);
            Assert.AreEqual(AutomationControlType.StatusBar, peer.GetAutomationControlType());
            Assert.AreEqual(nameof(ModernWpf.Controls.InfoBar), peer.GetClassName());
            Assert.IsTrue(peer.IsControlElement());

            infoBar.IsOpen = false;
            host.UpdateLayout();

            Assert.IsFalse(peer.IsControlElement());
        });
    }

    [TestMethod]
    public void InfoBarPanelLayoutTest()
    {
        WpfTestHost.Run(() =>
        {
            var first = CreatePanelChild(20, 10);
            var second = CreatePanelChild(30, 10);
            var third = CreatePanelChild(10, 10);
            var panel = new InfoBarPanel
            {
                HorizontalOrientationPadding = new Thickness(1, 2, 3, 4),
                VerticalOrientationPadding = new Thickness(2, 3, 4, 5)
            };

            InfoBarPanel.SetHorizontalOrientationMargin(first, new Thickness(0, 1, 2, 3));
            InfoBarPanel.SetHorizontalOrientationMargin(second, new Thickness(4, 5, 6, 7));
            InfoBarPanel.SetHorizontalOrientationMargin(third, new Thickness(8, 9, 10, 11));
            InfoBarPanel.SetVerticalOrientationMargin(first, new Thickness(0, 1, 0, 2));
            InfoBarPanel.SetVerticalOrientationMargin(second, new Thickness(0, 3, 0, 4));
            InfoBarPanel.SetVerticalOrientationMargin(third, new Thickness(0, 5, 0, 6));

            panel.Children.Add(first);
            panel.Children.Add(second);
            panel.Children.Add(third);

            panel.Measure(new Size(100, 100));
            panel.Arrange(new Rect(0, 0, 100, 100));

            Assert.AreEqual(new Rect(1, 3, 20, 10), LayoutInformation.GetLayoutSlot(first));
            Assert.AreEqual(new Rect(27, 7, 30, 10), LayoutInformation.GetLayoutSlot(second));
            Assert.AreEqual(new Rect(71, 11, 29, 10), LayoutInformation.GetLayoutSlot(third));

            panel.Measure(new Size(40, 100));
            panel.Arrange(new Rect(0, 0, 40, 100));

            Assert.AreEqual(new Rect(2, 3, 20, 10), LayoutInformation.GetLayoutSlot(first));
            Assert.AreEqual(new Rect(2, 18, 30, 10), LayoutInformation.GetLayoutSlot(second));
            Assert.AreEqual(new Rect(2, 37, 10, 10), LayoutInformation.GetLayoutSlot(third));
        });
    }

    [TestMethod]
    public void InfoBarPanelUsesWinUITextBlockLayoutRounding()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TextBlock { Width = 20.1, Height = 10.1 };
            var second = new TextBlock { Width = 30.1, Height = 10.1 };
            var panel = new InfoBarPanel
            {
                VerticalOrientationPadding = new Thickness(2, 3, 4, 5)
            };
            InfoBarPanel.SetVerticalOrientationMargin(second, new Thickness(0, 4, 0, 0));
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 100, height: 100);
            var dpi = VisualTreeHelper.GetDpi(panel);

            panel.InvalidateMeasure();
            panel.Measure(new Size(40, 100));
            var roundedWidth = Math.Ceiling(second.DesiredSize.Width * dpi.DpiScaleX) / dpi.DpiScaleX;
            var firstRoundedHeight = Math.Ceiling(first.DesiredSize.Height * dpi.DpiScaleY) / dpi.DpiScaleY;
            var secondRoundedHeight = Math.Ceiling(second.DesiredSize.Height * dpi.DpiScaleY) / dpi.DpiScaleY;
            panel.Arrange(new Rect(0, 0, roundedWidth + 6, firstRoundedHeight + secondRoundedHeight + 12));

            Assert.AreEqual(roundedWidth + 6, panel.DesiredSize.Width, 0.001);
            Assert.AreEqual(firstRoundedHeight + secondRoundedHeight + 12, panel.DesiredSize.Height, 0.001);

            var firstAdjustment = firstRoundedHeight - first.DesiredSize.Height;
            var secondAdjustment = secondRoundedHeight - second.DesiredSize.Height;
            Assert.AreEqual(3 + firstAdjustment, LayoutInformation.GetLayoutSlot(first).Y, 0.001);
            Assert.AreEqual(3 + firstRoundedHeight + 4 + secondAdjustment, LayoutInformation.GetLayoutSlot(second).Y, 0.001);
        });
    }

    private static FrameworkElement CreatePanelChild(double width, double height)
    {
        return new Border
        {
            Width = width,
            Height = height,
            Background = Brushes.Transparent
        };
    }

    private static void AssertSeverityResourceConsumption(
        ModernWpf.Controls.InfoBar infoBar,
        TestWindowHost host,
        Border contentRoot,
        TextBlock iconBackground,
        TextBlock standardIcon,
        InfoBarSeverity severity,
        object backgroundResourceKey,
        object iconBackgroundResourceKey,
        object iconForegroundResourceKey,
        object expectedGlyph)
    {
        infoBar.Severity = severity;
        host.UpdateLayout();

        Assert.AreSame(contentRoot.TryFindResource(backgroundResourceKey), contentRoot.Background);
        Assert.AreSame(contentRoot.TryFindResource(iconBackgroundResourceKey), iconBackground.Foreground);
        Assert.AreSame(contentRoot.TryFindResource(iconForegroundResourceKey), standardIcon.Foreground);
        Assert.AreEqual(expectedGlyph, standardIcon.Text);
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private static T FindDescendant<T>(DependencyObject root)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant of type '{typeof(T).Name}'.");
    }

    private static ContentPresenterEx FindContentPresenter(DependencyObject root, object content)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is ContentPresenterEx presenter && ReferenceEquals(presenter.Content, content))
            {
                return presenter;
            }
        }

        throw new InvalidOperationException("Could not find ContentPresenterEx for the expected content.");
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static void AssertResourceReference(FrameworkElement element, object resourceKey, object expectedResourceKey)
    {
        Assert.AreSame(
            element.TryFindResource(expectedResourceKey),
            element.TryFindResource(resourceKey),
            $"{resourceKey} should resolve through {expectedResourceKey}.");
    }

    private static void AssertSolidColorBrushColor(FrameworkElement element, object resourceKey, Color expectedColor)
    {
        var brush = element.TryFindResource(resourceKey) as SolidColorBrush;
        Assert.IsNotNull(brush, $"{resourceKey} should resolve to a SolidColorBrush.");
        Assert.AreEqual(expectedColor, brush!.Color);
    }

    private static object? GetSetterValue(Style style, DependencyProperty property)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        return setter!.Value;
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object? expectedValue)
    {
        Assert.AreEqual(expectedValue, GetSetterValue(style, property));
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var dynamicResource = GetSetterValue(style, property) as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertSolidColorBrushSetterColor(Style style, DependencyProperty property, Color expectedColor)
    {
        var brush = GetSetterValue(style, property) as SolidColorBrush;
        Assert.IsNotNull(brush, $"Expected {property.Name} to use a SolidColorBrush.");
        Assert.AreEqual(expectedColor, brush!.Color);
    }

    private static void AssertStateSetterDynamicResource(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedResourceKey)
    {
        var setter = FindStateSetter(stateGroupsRoot, groupName, stateName, target);

        AssertResourceReferenceExpression(
            setter.ReadLocalValue(VisualStateSetter.ValueProperty),
            expectedResourceKey);
    }

    private static void AssertStateSetterValue(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedValue)
    {
        var setter = FindStateSetter(stateGroupsRoot, groupName, stateName, target);

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static VisualStateSetter FindStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        var state = group.States
            .OfType<VisualStateEx>()
            .Single(candidate => candidate.Name == stateName);
        return state.Setters.Single(candidate => candidate.Target == target);
    }

    private static void AssertResourceReferenceExpression(object value, object expectedResourceKey)
    {
        Assert.IsNotNull(value, "Expected dynamic resource local value.");
        Assert.AreEqual("System.Windows.ResourceReferenceExpression", value.GetType().FullName);
        var resourceKeyProperty = value.GetType().GetProperty(
            "ResourceKey",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(resourceKeyProperty, "Expected ResourceReferenceExpression.ResourceKey.");
        Assert.AreEqual(expectedResourceKey, resourceKeyProperty!.GetValue(value));
    }
}
