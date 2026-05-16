using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TitleBar;

[TestClass]
public class TitleBarApiTests
{
    [TestMethod]
    public void VerifyAttachedPropertyDefaultsAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var window = new Window();
            try
            {
                var background = Brushes.Red;
                var foreground = Brushes.Blue;
                var inactiveBackground = Brushes.Green;
                var inactiveForeground = Brushes.Gray;
                var titleBarStyle = new Style(typeof(TitleBarControl));
                var buttonStyle = new Style(typeof(TitleBarButton));
                var backButtonStyle = new Style(typeof(TitleBarButton));
                var command = new RoutedCommand();
                var commandTarget = new Button();
                var commandParameter = new object();

                Assert.IsNull(ModernWpf.Controls.TitleBar.GetBackground(window));
                Assert.IsNull(ModernWpf.Controls.TitleBar.GetForeground(window));
                Assert.IsNull(ModernWpf.Controls.TitleBar.GetInactiveBackground(window));
                Assert.IsNull(ModernWpf.Controls.TitleBar.GetInactiveForeground(window));
                Assert.IsNull(ModernWpf.Controls.TitleBar.GetStyle(window));
                Assert.IsNull(ModernWpf.Controls.TitleBar.GetButtonStyle(window));
                Assert.IsNull(ModernWpf.Controls.TitleBar.GetBackButtonStyle(window));
                Assert.IsFalse(ModernWpf.Controls.TitleBar.GetIsIconVisible(window));
                Assert.IsFalse(ModernWpf.Controls.TitleBar.GetIsBackButtonVisible(window));
                Assert.IsTrue(ModernWpf.Controls.TitleBar.GetIsBackEnabled(window));
                Assert.IsFalse(ModernWpf.Controls.TitleBar.GetExtendViewIntoTitleBar(window));
                Assert.AreEqual(32.0, ModernWpf.Controls.TitleBar.GetHeight(window));
                Assert.AreEqual(0.0, ModernWpf.Controls.TitleBar.GetSystemOverlayLeftInset(window));
                Assert.AreEqual(0.0, ModernWpf.Controls.TitleBar.GetSystemOverlayRightInset(window));

                ModernWpf.Controls.TitleBar.SetBackground(window, background);
                ModernWpf.Controls.TitleBar.SetForeground(window, foreground);
                ModernWpf.Controls.TitleBar.SetInactiveBackground(window, inactiveBackground);
                ModernWpf.Controls.TitleBar.SetInactiveForeground(window, inactiveForeground);
                ModernWpf.Controls.TitleBar.SetStyle(window, titleBarStyle);
                ModernWpf.Controls.TitleBar.SetButtonStyle(window, buttonStyle);
                ModernWpf.Controls.TitleBar.SetBackButtonStyle(window, backButtonStyle);
                ModernWpf.Controls.TitleBar.SetIsIconVisible(window, true);
                ModernWpf.Controls.TitleBar.SetIsBackButtonVisible(window, true);
                ModernWpf.Controls.TitleBar.SetIsBackEnabled(window, false);
                ModernWpf.Controls.TitleBar.SetBackButtonCommand(window, command);
                ModernWpf.Controls.TitleBar.SetBackButtonCommandParameter(window, commandParameter);
                ModernWpf.Controls.TitleBar.SetBackButtonCommandTarget(window, commandTarget);
                ModernWpf.Controls.TitleBar.SetExtendViewIntoTitleBar(window, true);

                Assert.AreSame(background, ModernWpf.Controls.TitleBar.GetBackground(window));
                Assert.AreSame(foreground, ModernWpf.Controls.TitleBar.GetForeground(window));
                Assert.AreSame(inactiveBackground, ModernWpf.Controls.TitleBar.GetInactiveBackground(window));
                Assert.AreSame(inactiveForeground, ModernWpf.Controls.TitleBar.GetInactiveForeground(window));
                Assert.AreSame(titleBarStyle, ModernWpf.Controls.TitleBar.GetStyle(window));
                Assert.AreSame(buttonStyle, ModernWpf.Controls.TitleBar.GetButtonStyle(window));
                Assert.AreSame(backButtonStyle, ModernWpf.Controls.TitleBar.GetBackButtonStyle(window));
                Assert.IsTrue(ModernWpf.Controls.TitleBar.GetIsIconVisible(window));
                Assert.IsTrue(ModernWpf.Controls.TitleBar.GetIsBackButtonVisible(window));
                Assert.IsFalse(ModernWpf.Controls.TitleBar.GetIsBackEnabled(window));
                Assert.AreSame(command, ModernWpf.Controls.TitleBar.GetBackButtonCommand(window));
                Assert.AreSame(commandParameter, ModernWpf.Controls.TitleBar.GetBackButtonCommandParameter(window));
                Assert.AreSame(commandTarget, ModernWpf.Controls.TitleBar.GetBackButtonCommandTarget(window));
                Assert.IsTrue(ModernWpf.Controls.TitleBar.GetExtendViewIntoTitleBar(window));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public void VerifyCoreApplicationViewTitleBarBridge()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = new TestWindowHost(new Grid(), width: 320, height: 180);
            var window = host.Window;
            var coreTitleBar = CoreApplicationViewTitleBar.GetTitleBar(window);
            var layoutMetricsChangedCount = 0;
            var isVisibleChangedCount = 0;

            coreTitleBar.LayoutMetricsChanged += (_, _) => layoutMetricsChangedCount++;
            coreTitleBar.IsVisibleChanged += (_, _) => isVisibleChangedCount++;
            var initialLeftInset = ModernWpf.Controls.TitleBar.GetSystemOverlayLeftInset(window);
            var initialRightInset = ModernWpf.Controls.TitleBar.GetSystemOverlayRightInset(window);

            Assert.IsFalse(coreTitleBar.ExtendViewIntoTitleBar);
            Assert.IsTrue(coreTitleBar.IsVisible);
            Assert.AreEqual(ModernWpf.Controls.TitleBar.GetHeight(window), coreTitleBar.Height);
            Assert.AreEqual(initialLeftInset, coreTitleBar.SystemOverlayLeftInset);
            Assert.AreEqual(initialRightInset, coreTitleBar.SystemOverlayRightInset);

            coreTitleBar.ExtendViewIntoTitleBar = true;
            ModernWpf.Controls.TitleBar.SetHeight(window, 48.0);
            ModernWpf.Controls.TitleBar.SetSystemOverlayLeftInset(window, initialLeftInset + 12.0);
            ModernWpf.Controls.TitleBar.SetSystemOverlayRightInset(window, initialRightInset + 24.0);
            WpfTestHost.DoEvents();

            Assert.IsTrue(coreTitleBar.ExtendViewIntoTitleBar);
            Assert.AreEqual(48.0, coreTitleBar.Height);
            Assert.AreEqual(initialLeftInset + 12.0, coreTitleBar.SystemOverlayLeftInset);
            Assert.AreEqual(initialRightInset + 24.0, coreTitleBar.SystemOverlayRightInset);
            Assert.IsTrue(layoutMetricsChangedCount >= 4, $"Expected layout metric events, got {layoutMetricsChangedCount}.");
            Assert.IsTrue(isVisibleChangedCount >= 1, $"Expected visibility events, got {isVisibleChangedCount}.");
            Assert.AreSame(coreTitleBar, CoreApplicationViewTitleBar.GetTitleBar(window));
            Assert.AreSame(coreTitleBar, CoreApplicationViewTitleBar.GetTitleBar((DependencyObject)window.Content));
        });
    }

    [TestMethod]
    public void VerifyBackRequestedRoutedEvent()
    {
        WpfTestHost.Run(() =>
        {
            var window = new Window();
            try
            {
                var eventCount = 0;
                object? eventSource = null;

                ModernWpf.Controls.TitleBar.AddBackRequestedHandler(window, OnBackRequested);
                ModernWpf.Controls.TitleBar.RaiseBackRequested(window);
                ModernWpf.Controls.TitleBar.RemoveBackRequestedHandler(window, OnBackRequested);
                ModernWpf.Controls.TitleBar.RaiseBackRequested(window);

                Assert.AreEqual(1, eventCount);
                Assert.AreSame(window, eventSource);

                void OnBackRequested(object? sender, ModernWpf.Controls.BackRequestedEventArgs args)
                {
                    eventCount++;
                    eventSource = args.Source;
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public void VerifyTitleBarControlDefaultStyleAndTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var titleBarControl = new TitleBarControl
            {
                Title = "ModernWpf Test Title",
                IsActive = true,
                IsBackButtonVisible = true,
                IsBackEnabled = false
            };

            using var host = new TestWindowHost(titleBarControl, width: 420, height: 180);
            host.UpdateLayout();

            Assert.AreEqual(32.0, titleBarControl.Height);
            Assert.IsFalse(titleBarControl.IsTabStop);
            Assert.IsTrue(TitleBarControl.GetInsideTitleBar(titleBarControl));

            var layoutRoot = FindNamedDescendant<Grid>(titleBarControl, "LayoutRoot");
            AssertBrushEquals(titleBarControl.Background, layoutRoot.Background);

            var backButton = FindNamedDescendant<TitleBarButton>(titleBarControl, "PART_BackButton");
            Assert.AreEqual(Visibility.Visible, backButton.Visibility);
            Assert.IsFalse(backButton.IsEnabled);
            Assert.AreEqual("Back", AutomationProperties.GetName(backButton));

            var title = FindNamedDescendant<TextBlock>(titleBarControl, "Title");
            Assert.AreEqual("ModernWpf Test Title", title.Text);
            Assert.AreEqual(TextTrimming.CharacterEllipsis, title.TextTrimming);
            AssertBrushEquals(titleBarControl.Foreground, title.Foreground);

            Assert.AreEqual("Minimize", AutomationProperties.GetName(FindNamedDescendant<TitleBarButton>(titleBarControl, "MinimizeButton")));
            Assert.AreEqual("Maximize", AutomationProperties.GetName(FindNamedDescendant<TitleBarButton>(titleBarControl, "PART_MaximizeRestoreButton")));
            Assert.AreEqual("Close", AutomationProperties.GetName(FindNamedDescendant<TitleBarButton>(titleBarControl, "CloseButton")));
        });
    }

    [TestMethod]
    public void VerifyTitleBarControlUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var inactiveBackground = Brushes.Green;
            var inactiveForeground = Brushes.Gray;
            var titleBarControl = new TitleBarControl
            {
                Title = "ModernWpf Test Title",
                IsActive = true,
                IsBackButtonVisible = true,
                IsIconVisible = true,
                InactiveBackground = inactiveBackground,
                InactiveForeground = inactiveForeground
            };

            using var host = new TestWindowHost(titleBarControl, width: 420, height: 180);
            host.UpdateLayout();

            var layoutRoot = FindNamedDescendant<Grid>(titleBarControl, "LayoutRoot");
            AssertStateSetter(layoutRoot, "ActivationStateGroup", "Deactivated",
                "LayoutRoot.Background",
                "HighContrastBackground.Fill",
                "Title.Foreground",
                "PART_BackButton.Foreground");
            AssertStateSetter(layoutRoot, "BackButtonVisibilityGroup", "BackButtonVisible", "PART_BackButton.Visibility");
            AssertStateSetter(layoutRoot, "BackButtonVisibilityGroup", "BackButtonCollapsed",
                "PART_BackButton.Visibility",
                "Icon.Margin");
            AssertStateSetter(layoutRoot, "IconVisibilityGroup", "IconVisible",
                "Icon.Visibility",
                "Title.Margin");
            AssertStateSetter(layoutRoot, "IconVisibilityGroup", "IconCollapsed",
                "Icon.Visibility",
                "Title.Margin");
            AssertStateSetter(layoutRoot, "TitleTextVisibilityGroup", "TitleTextVisible", "Title.Visibility");
            AssertStateSetter(layoutRoot, "TitleTextVisibilityGroup", "TitleTextCollapsed", "Title.Visibility");
            AssertStateSetter(layoutRoot, "ExtendViewIntoTitleBarStates", "TitleContentCollapsed",
                "LayoutRoot.Background",
                "IconTitlePanel.Visibility");

            Assert.AreEqual("Activated", FindVisualStateGroup(layoutRoot, "ActivationStateGroup").CurrentState?.Name);
            Assert.AreEqual("BackButtonVisible", FindVisualStateGroup(layoutRoot, "BackButtonVisibilityGroup").CurrentState?.Name);
            Assert.AreEqual("IconVisible", FindVisualStateGroup(layoutRoot, "IconVisibilityGroup").CurrentState?.Name);
            Assert.AreEqual("TitleTextVisible", FindVisualStateGroup(layoutRoot, "TitleTextVisibilityGroup").CurrentState?.Name);

            titleBarControl.IsActive = false;
            titleBarControl.IsBackButtonVisible = false;
            titleBarControl.IsIconVisible = false;
            titleBarControl.Title = string.Empty;
            WpfTestHost.DoEvents();

            Assert.AreEqual("Deactivated", FindVisualStateGroup(layoutRoot, "ActivationStateGroup").CurrentState?.Name);
            Assert.AreEqual("BackButtonCollapsed", FindVisualStateGroup(layoutRoot, "BackButtonVisibilityGroup").CurrentState?.Name);
            Assert.AreEqual("IconCollapsed", FindVisualStateGroup(layoutRoot, "IconVisibilityGroup").CurrentState?.Name);
            Assert.AreEqual("TitleTextCollapsed", FindVisualStateGroup(layoutRoot, "TitleTextVisibilityGroup").CurrentState?.Name);

            var backButton = FindNamedDescendant<TitleBarButton>(titleBarControl, "PART_BackButton");
            var icon = FindNamedDescendant<Image>(titleBarControl, "Icon");
            var title = FindNamedDescendant<TextBlock>(titleBarControl, "Title");
            AssertBrushEquals(inactiveBackground, layoutRoot.Background);
            AssertBrushEquals(inactiveForeground, title.Foreground);
            Assert.AreEqual(Visibility.Collapsed, backButton.Visibility);
            Assert.AreEqual(new Thickness(16, 0, 0, 0), icon.Margin);
            Assert.AreEqual(Visibility.Collapsed, icon.Visibility);
            Assert.AreEqual(new Thickness(12, 0, 12, 0), title.Margin);
            Assert.AreEqual(Visibility.Collapsed, title.Visibility);

            titleBarControl.ExtendViewIntoTitleBar = true;
            WpfTestHost.DoEvents();

            Assert.AreEqual("TitleContentCollapsed", FindVisualStateGroup(layoutRoot, "ExtendViewIntoTitleBarStates").CurrentState?.Name);
            Assert.IsNull(layoutRoot.Background);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<StackPanel>(titleBarControl, "IconTitlePanel").Visibility);
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2TitleBarResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceReference(themeName, "TitleBarForegroundBrush", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "TitleBarDeactivatedForegroundBrush", "TextFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "TitleBarButtonBackgroundColor", "SubtleFillColorTransparent");
                AssertThemeResourceReference(themeName, "TitleBarButtonHoverBackgroundColor", "SubtleFillColorSecondary");
                AssertThemeResourceReference(themeName, "TitleBarButtonPressedBackgroundColor", "SubtleFillColorTertiary");
                AssertThemeResourceValue(themeName, "TitleBarCompactHeight", 32.0);
                AssertThemeResourceValue(themeName, "TitleBarExpandedHeight", 48.0);
            }

            AssertThemeResourceValue("Light", "TitleBarButtonForegroundColor", Color.FromRgb(0x19, 0x19, 0x19));
            AssertThemeResourceValue("Light", "TitleBarButtonHoverForegroundColor", Color.FromRgb(0x19, 0x19, 0x19));
            AssertThemeResourceValue("Light", "TitleBarButtonPressedForegroundColor", Color.FromRgb(0x60, 0x60, 0x60));
            AssertThemeResourceValue("Light", "TitleBarButtonInactiveForegroundColor", Color.FromRgb(0x9b, 0x9b, 0x9b));

            AssertThemeResourceValue("Dark", "TitleBarButtonForegroundColor", Colors.White);
            AssertThemeResourceValue("Dark", "TitleBarButtonHoverForegroundColor", Colors.White);
            AssertThemeResourceValue("Dark", "TitleBarButtonPressedForegroundColor", Color.FromRgb(0xcf, 0xcf, 0xcf));
            AssertThemeResourceValue("Dark", "TitleBarButtonInactiveForegroundColor", Color.FromRgb(0x71, 0x71, 0x71));

            AssertThemeResourceReference("HighContrast", "TitleBarButtonForegroundColor", "TextFillColorPrimary");
            AssertThemeResourceReference("HighContrast", "TitleBarButtonHoverForegroundColor", "TextFillColorPrimary");
            AssertThemeResourceReference("HighContrast", "TitleBarButtonPressedForegroundColor", "TextFillColorSecondary");
            AssertThemeResourceReference("HighContrast", "TitleBarButtonInactiveForegroundColor", "TextFillColorTertiary");
        });
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");

        var actual = themeDictionary[resourceKey];
        var expected = themeDictionary[expectedResourceKey];

        if (expected is null || actual is null || expected.GetType().IsValueType || actual.GetType().IsValueType)
        {
            Assert.AreEqual(expected, actual, $"{themeName}:{resourceKey}");
        }
        else
        {
            Assert.AreSame(expected, actual, $"{themeName}:{resourceKey}");
        }
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        if (expected is null || actual is null)
        {
            Assert.AreSame(expected, actual);
            return;
        }

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
    }

    private static VisualStateEx AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States
            .OfType<VisualState>()
            .SingleOrDefault(candidate => candidate.Name == stateName);
        Assert.IsNotNull(state, $"{groupName} is missing {stateName}.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (var expectedTarget in expectedTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == expectedTarget),
                $"{groupName}.{stateName} is missing setter target {expectedTarget}.");
        }

        return stateEx;
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .SingleOrDefault(candidate => candidate.Name == groupName);
        Assert.IsNotNull(group, $"Missing visual state group {groupName}.");
        return group!;
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

        throw new InvalidOperationException(
            $"Could not find {typeof(T).Name} descendant named '{name}'. Descendants: {string.Join(", ", VisualTreeTestHelper.EnumerateDescendants(root).OfType<FrameworkElement>().Select(element => element.Name).Where(elementName => !string.IsNullOrEmpty(elementName)))}");
    }
}
