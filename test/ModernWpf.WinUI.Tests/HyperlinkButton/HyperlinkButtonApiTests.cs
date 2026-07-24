using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.HyperlinkButtonTests;

[TestClass]
public class HyperlinkButtonApiTests
{
    [TestMethod]
    public void VerifyWinUI3ApiSurfaceAndDefaults()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var resources = (ResourceDictionary)Application.LoadComponent(
                new Uri("/ModernWpf.Controls;component/HyperlinkButton/HyperlinkButton.xaml", UriKind.Relative));

            var hyperlinkButton = new ModernWpf.Controls.HyperlinkButton
            {
                Content = "Link"
            };

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            Assert.IsNull(hyperlinkButton.NavigateUri);
            Assert.IsNull(typeof(ModernWpf.Controls.HyperlinkButton).GetProperty("TargetName"));
            Assert.IsNull(typeof(ModernWpf.Controls.HyperlinkButton).GetField("TargetNameProperty", BindingFlags.Public | BindingFlags.Static));

            var defaultStyle = (Style)resources["DefaultHyperlinkButtonStyle"];
            AssertDynamicResourceSetter(defaultStyle, Control.BackgroundProperty, "HyperlinkButtonBackground");
            AssertSetterValue(defaultStyle, ControlHelper.BackgroundSizingProperty, BackgroundSizing.OuterBorderEdge);
            AssertDynamicResourceSetter(defaultStyle, Control.ForegroundProperty, "HyperlinkButtonForeground");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "HyperlinkButtonBorderBrush");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderThicknessProperty, "HyperlinkButtonBorderThemeThickness");
            AssertDynamicResourceSetter(defaultStyle, Control.PaddingProperty, "ButtonPadding");
            AssertSetterValue(defaultStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(defaultStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertDynamicResourceSetter(defaultStyle, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(defaultStyle, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertDynamicResourceSetter(defaultStyle, ModernWpf.Controls.HyperlinkButton.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertSetterValue(defaultStyle, ModernWpf.Controls.HyperlinkButton.FocusVisualMarginProperty, new Thickness(-3));
            AssertDynamicResourceSetter(defaultStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(defaultStyle, ModernWpf.Controls.HyperlinkButton.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(defaultStyle, ButtonHelper.VisualStateSettersEnabledProperty, true);
            Assert.IsInstanceOfType(FindSetter(defaultStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
            AssertStyleTriggerSetter(defaultStyle, UIElement.IsEnabledProperty, true, FrameworkElement.CursorProperty, Cursors.Hand);

            var implicitStyle = (Style)resources[typeof(ModernWpf.Controls.HyperlinkButton)];
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            Assert.AreSame(hyperlinkButton.TryFindResource("HyperlinkButtonBackground"), hyperlinkButton.Background);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, ControlHelper.GetBackgroundSizing(hyperlinkButton));
            Assert.AreSame(hyperlinkButton.TryFindResource("HyperlinkButtonForeground"), hyperlinkButton.Foreground);
            Assert.AreSame(hyperlinkButton.TryFindResource("HyperlinkButtonBorderBrush"), hyperlinkButton.BorderBrush);
            Assert.AreEqual(hyperlinkButton.TryFindResource("HyperlinkButtonBorderThemeThickness"), hyperlinkButton.BorderThickness);
            Assert.AreEqual(hyperlinkButton.TryFindResource("ButtonPadding"), hyperlinkButton.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, hyperlinkButton.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, hyperlinkButton.VerticalAlignment);
            Assert.AreEqual(
                ((FontFamily)hyperlinkButton.TryFindResource("ContentControlThemeFontFamily")).Source,
                hyperlinkButton.FontFamily.Source);
            Assert.AreEqual(hyperlinkButton.TryFindResource("ControlContentThemeFontSize"), hyperlinkButton.FontSize);
            Assert.AreEqual(hyperlinkButton.TryFindResource("UseSystemFocusVisuals"), hyperlinkButton.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(-3), hyperlinkButton.FocusVisualMargin);
            Assert.AreSame(hyperlinkButton.TryFindResource(SystemParameters.FocusVisualStyleKey), hyperlinkButton.FocusVisualStyle);
            Assert.AreEqual(hyperlinkButton.TryFindResource("ControlCornerRadius"), hyperlinkButton.CornerRadius);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(hyperlinkButton));

            var presenter = FindVisualChild<ContentPresenterEx>(hyperlinkButton)
                ?? throw new AssertFailedException("Expected HyperlinkButton template to use ContentPresenterEx.");
            Assert.AreSame(hyperlinkButton.Background, presenter.Background);
            Assert.AreEqual(ControlHelper.GetBackgroundSizing(hyperlinkButton), presenter.BackgroundSizing);
            Assert.AreSame(hyperlinkButton.Foreground, presenter.Foreground);
            Assert.AreSame(hyperlinkButton.BorderBrush, presenter.BorderBrush);
            Assert.AreEqual(hyperlinkButton.BorderThickness, presenter.BorderThickness);
            Assert.AreEqual(hyperlinkButton.Content, presenter.Content);
            Assert.AreEqual(hyperlinkButton.Padding, presenter.Padding);
            Assert.AreEqual(hyperlinkButton.CornerRadius, presenter.CornerRadius);
            Assert.AreEqual(hyperlinkButton.HorizontalContentAlignment, presenter.HorizontalContentAlignment);
            Assert.AreEqual(hyperlinkButton.VerticalContentAlignment, presenter.VerticalContentAlignment);
            Assert.IsTrue(presenter.RecognizesAccessKey);
            Assert.AreEqual(hyperlinkButton.SnapsToDevicePixels, presenter.SnapsToDevicePixels);
            Assert.AreEqual("Normal", AnimatedIcon.GetState(presenter));
            Assert.IsNotNull(presenter.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), presenter.BackgroundTransition.Duration);
        });
    }

    [TestMethod]
    public void VerifyWinUI3TemplateStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var hyperlinkButton = new ModernWpf.Controls.HyperlinkButton
            {
                Content = "Link"
            };

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(hyperlinkButton)
                ?? throw new AssertFailedException("Expected HyperlinkButton template to use ContentPresenterEx.");

            AssertStateSetter(presenter, "CommonStates", "PointerOver",
                "ContentPresenter.(ui:AnimatedIcon.State)",
                "ContentPresenter.Foreground",
                "ContentPresenter.Background",
                "ContentPresenter.BorderBrush");
            AssertStateSetterValue(presenter, "CommonStates", "PointerOver", "ContentPresenter.(ui:AnimatedIcon.State)", "PointerOver");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "PointerOver", "ContentPresenter.Foreground", "HyperlinkButtonForegroundPointerOver");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "PointerOver", "ContentPresenter.Background", "HyperlinkButtonBackgroundPointerOver");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "PointerOver", "ContentPresenter.BorderBrush", "HyperlinkButtonBorderBrushPointerOver");
            AssertStateSetter(presenter, "CommonStates", "Pressed",
                "ContentPresenter.(ui:AnimatedIcon.State)",
                "ContentPresenter.Foreground",
                "ContentPresenter.Background",
                "ContentPresenter.BorderBrush");
            AssertStateSetterValue(presenter, "CommonStates", "Pressed", "ContentPresenter.(ui:AnimatedIcon.State)", "Pressed");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "Pressed", "ContentPresenter.Foreground", "HyperlinkButtonForegroundPressed");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "Pressed", "ContentPresenter.Background", "HyperlinkButtonBackgroundPressed");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "Pressed", "ContentPresenter.BorderBrush", "HyperlinkButtonBorderBrushPressed");
            AssertStateSetter(presenter, "CommonStates", "Disabled",
                "ContentPresenter.(ui:AnimatedIcon.State)",
                "ContentPresenter.Foreground",
                "ContentPresenter.Background",
                "ContentPresenter.BorderBrush");
            AssertStateSetterValue(presenter, "CommonStates", "Disabled", "ContentPresenter.(ui:AnimatedIcon.State)", "Normal");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "Disabled", "ContentPresenter.Foreground", "HyperlinkButtonForegroundDisabled");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "Disabled", "ContentPresenter.Background", "HyperlinkButtonBackgroundDisabled");
            AssertStateSetterDynamicResource(presenter, "CommonStates", "Disabled", "ContentPresenter.BorderBrush", "HyperlinkButtonBorderBrushDisabled");

            Assert.IsTrue(VisualStateManager.GoToState(hyperlinkButton, "PointerOver", false));
            AssertPresenterUsesStateResources(hyperlinkButton, presenter, "PointerOver");

            Assert.IsTrue(VisualStateManager.GoToState(hyperlinkButton, "Pressed", false));
            AssertPresenterUsesStateResources(hyperlinkButton, presenter, "Pressed");

            Assert.IsTrue(VisualStateManager.GoToState(hyperlinkButton, "Disabled", false));
            AssertPresenterUsesStateResources(hyperlinkButton, presenter, "Disabled");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI3HyperlinkButtonHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "HyperlinkButtonForeground", "AccentTextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonForegroundPointerOver", "AccentTextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonForegroundPressed", "AccentTextFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonForegroundDisabled", "AccentTextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBackgroundPressed", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBackgroundDisabled", "SubtleFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBorderBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBorderBrushPressed", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "HyperlinkButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
                AssertThemeResourceValue(themeName, "HyperlinkButtonBorderThemeThickness", new Thickness(1));
            }

            AssertThemeResourceReference("HighContrast", "HyperlinkButtonForeground", "SystemControlHyperlinkTextBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonForegroundPointerOver", "SystemControlPageTextBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonForegroundPressed", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBackground", "SystemControlPageBackgroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBackgroundPointerOver", "SystemControlPageBackgroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBackgroundPressed", "SystemControlPageBackgroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBackgroundDisabled", "SystemControlPageBackgroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBorderBrushPointerOver", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBorderBrushPressed", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkButtonBorderBrushDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceValue("HighContrast", "HyperlinkButtonBorderThemeThickness", new Thickness(1));
        });
    }

    [TestMethod]
    public void VerifyWinUI3AutomationPeerInvoke()
    {
        WpfTestHost.Run(() =>
        {
            var hyperlinkButton = new ModernWpf.Controls.HyperlinkButton
            {
                Content = "Link"
            };
            var clickCount = 0;
            hyperlinkButton.Click += (sender, args) => clickCount++;

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(hyperlinkButton);
            Assert.AreEqual("Hyperlink", peer.GetClassName());
            Assert.AreEqual(AutomationControlType.Hyperlink, peer.GetAutomationControlType());

            var invokeProvider = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
            invokeProvider.Invoke();

            Assert.AreEqual(1, clickCount);

            hyperlinkButton.IsEnabled = false;
            Assert.ThrowsExactly<ElementNotEnabledException>(() => invokeProvider.Invoke());
            Assert.AreEqual(1, clickCount);
        });
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        foreach (var child in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (child is T typedChild)
            {
                return typedChild;
            }
        }

        return null;
    }

    private static void AssertPresenterUsesStateResources(
        ModernWpf.Controls.HyperlinkButton hyperlinkButton,
        ContentPresenterEx presenter,
        string stateName)
    {
        Assert.AreSame(hyperlinkButton.TryFindResource($"HyperlinkButtonForeground{stateName}"), presenter.Foreground);
        Assert.AreSame(hyperlinkButton.TryFindResource($"HyperlinkButtonBackground{stateName}"), presenter.Background);
        Assert.AreSame(hyperlinkButton.TryFindResource($"HyperlinkButtonBorderBrush{stateName}"), presenter.BorderBrush);
        Assert.AreEqual(stateName == "Disabled" ? "Normal" : stateName, AnimatedIcon.GetState(presenter));
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] expectedTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .FirstOrDefault(candidate => candidate.Name == groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = group!.States
            .OfType<VisualState>()
            .FirstOrDefault(candidate => candidate.Name == stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (var expectedTarget in expectedTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.OfType<VisualStateSetter>().Any(setter => setter.Target == expectedTarget),
                $"Expected visual state '{groupName}.{stateName}' to contain setter '{expectedTarget}'.");
        }
    }

    private static void AssertStateSetterValue(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedValue)
    {
        var setter = FindStateSetter(stateGroupsRoot, groupName, stateName, target);
        Assert.AreEqual(expectedValue, setter.Value, $"{stateName}:{target}");
    }

    private static void AssertStateSetterDynamicResource(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedResourceKey)
    {
        var setter = FindStateSetter(stateGroupsRoot, groupName, stateName, target);

        if (setter.Value is DynamicResourceExtension dynamicResource)
        {
            Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey, $"{stateName}:{target}");
            return;
        }

        Assert.AreSame(stateGroupsRoot.TryFindResource(expectedResourceKey), setter.Value, $"{stateName}:{target}");
    }

    private static VisualStateSetter FindStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .FirstOrDefault(candidate => candidate.Name == groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = group!.States
            .OfType<VisualState>()
            .FirstOrDefault(candidate => candidate.Name == stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        return stateEx.Setters
            .OfType<VisualStateSetter>()
            .Single(setter => setter.Target == target);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertStyleTriggerSetter(Style style, DependencyProperty triggerProperty, object triggerValue, DependencyProperty setterProperty, object expectedValue)
    {
        var trigger = style.Triggers
            .OfType<Trigger>()
            .Single(item => item.Property == triggerProperty && Equals(item.Value, triggerValue));
        var setter = trigger.Setters
            .OfType<Setter>()
            .Single(item => item.Property == setterProperty);

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var current = style; current != null; current = current.BasedOn)
        {
            var setter = current.Setters
                .OfType<Setter>()
                .SingleOrDefault(item => item.Property == property);

            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);

        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
