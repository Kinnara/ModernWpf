using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.DropDownButton;

[TestClass]
public class DropDownButtonApiTests
{
    [TestMethod]
    public void VerifyDropDownButtonPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "Flyout content"
                }
            };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new ModernWpf.Controls.DropDownButton
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                CharacterSpacing = 17,
                Content = "Options",
                ContentTransitions = transitions,
                Flyout = flyout,
                CornerRadius = new CornerRadius(4),
                IsTextScaleFactorEnabled = false,
                UseSystemFocusVisuals = true,
                FocusVisualMargin = new Thickness(2)
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, button.BackgroundSizing);
            Assert.AreEqual(17, button.CharacterSpacing);
            Assert.AreEqual("Options", button.Content);
            Assert.AreSame(transitions, button.ContentTransitions);
            Assert.AreSame(flyout, button.Flyout);
            Assert.AreEqual(new CornerRadius(4), button.CornerRadius);
            Assert.IsFalse(button.IsTextScaleFactorEnabled);
            Assert.IsTrue(button.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(2), button.FocusVisualMargin);

            button.Flyout = null;

            Assert.IsNull(button.Flyout);
        });
    }

    [TestMethod]
    public void VerifyDropDownButtonTemplateAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options"
            };

            using var host = new TestWindowHost(button, width: 320, height: 160);
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, button.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Center, button.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, button.VerticalContentAlignment);
            Assert.AreEqual(new Thickness(-3, -3, -4, -3), button.FocusVisualMargin);
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondary"));
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondaryPointerOver"));
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondaryPressed"));

            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button);

            Assert.IsNotNull(chevron);
            Assert.AreEqual("ChevronIcon", chevron!.Name);
            Assert.AreEqual(12d, chevron.Width);
            Assert.AreEqual(12d, chevron.Height);
            Assert.IsNotNull(chevron.Foreground);
        });
    }

    [TestMethod]
    public void DropDownButtonStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/DropDownButton/DropDownButton.xaml", UriKind.Relative)
            };
            var style = (Style)resources[typeof(ModernWpf.Controls.DropDownButton)];
            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options",
                Style = style
            };
            button.Resources.MergedDictionaries.Add(resources);

            using var host = new TestWindowHost(button, width: 320, height: 160);
            host.UpdateLayout();

            AssertDynamicResourceSetter(style, Control.BackgroundProperty, "ButtonBackground");
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "ButtonForeground");
            AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "ButtonBorderBrush");
            AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "ButtonBorderThemeThickness");
            AssertDynamicResourceSetter(style, Control.PaddingProperty, "ButtonPadding");
            AssertDynamicResourceSetter(style, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(style, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.DropDownButton.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(style, ModernWpf.Controls.DropDownButton.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(style, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(style, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(style, Control.FontWeightProperty, FontWeights.Normal);
            AssertSetterValue(style, ModernWpf.Controls.DropDownButton.FocusVisualMarginProperty, new Thickness(-3, -3, -4, -3));
            AssertSetterValue(style, ModernWpf.Controls.DropDownButton.BackgroundSizingProperty, BackgroundSizing.InnerBorderEdge);
            AssertSetterValue(style, ModernWpf.Controls.Primitives.ButtonHelper.VisualStateSettersEnabledProperty, true);

            Assert.AreSame(button.TryFindResource("ButtonBackground"), button.Background);
            Assert.AreSame(button.TryFindResource("ButtonForeground"), button.Foreground);
            Assert.AreSame(button.TryFindResource("ButtonBorderBrush"), button.BorderBrush);
            Assert.AreEqual(button.TryFindResource("ButtonBorderThemeThickness"), button.BorderThickness);
            Assert.AreEqual(button.TryFindResource("ButtonPadding"), button.Padding);
            Assert.AreSame(button.TryFindResource("ContentControlThemeFontFamily"), button.FontFamily);
            Assert.AreEqual(button.TryFindResource("ControlContentThemeFontSize"), button.FontSize);
            Assert.AreEqual(button.TryFindResource("UseSystemFocusVisuals"), button.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(-3, -3, -4, -3), button.FocusVisualMargin);
            Assert.AreEqual(BackgroundSizing.InnerBorderEdge, button.BackgroundSizing);
            Assert.IsTrue(ModernWpf.Controls.Primitives.ButtonHelper.GetVisualStateSettersEnabled(button));

            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template root to use GridEx chrome.");
            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template to use ContentPresenterEx.");
            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button)
                ?? throw new AssertFailedException("Expected DropDownButton chevron icon.");

            Assert.AreSame(button.Background, rootGrid.Background);
            Assert.AreSame(button.BorderBrush, rootGrid.BorderBrush);
            Assert.AreEqual(button.BorderThickness, rootGrid.BorderThickness);
            Assert.AreEqual(button.Padding, rootGrid.Padding);
            Assert.AreEqual(button.CornerRadius, rootGrid.CornerRadius);
            Assert.AreEqual(button.BackgroundSizing, rootGrid.BackgroundSizing);
            Assert.AreSame(button.Foreground, presenter.Foreground);
            Assert.IsInstanceOfType(presenter.RenderTransform, typeof(System.Windows.Media.TranslateTransform));
            Assert.AreEqual(-1d, ((System.Windows.Media.TranslateTransform)presenter.RenderTransform).Y);
            Assert.AreSame(button.TryFindResource("DropDownButtonForegroundSecondary"), chevron.Foreground);
            Assert.AreSame(button.TryFindResource("SymbolThemeFontFamily"), chevron.FontFamily);
            Assert.AreEqual(12d, chevron.Width);
            Assert.AreEqual(12d, chevron.Height);
            Assert.AreEqual(8d, chevron.FontSize);
            Assert.AreEqual(new Thickness(8, 0, 0, 0), chevron.Margin);
            Assert.IsInstanceOfType(chevron.RenderTransform, typeof(System.Windows.Media.TranslateTransform));
            Assert.AreEqual(1d, ((System.Windows.Media.TranslateTransform)chevron.RenderTransform).X);
            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));

            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "PointerOver", "RootGrid.Background", "ButtonBackgroundPointerOver");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "PointerOver", "RootGrid.BorderBrush", "ButtonBorderBrushPointerOver");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "PointerOver", "ContentPresenter.Foreground", "ButtonForegroundPointerOver");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "PointerOver", "ChevronIcon.Foreground", "DropDownButtonForegroundSecondaryPointerOver");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Pressed", "RootGrid.Background", "ButtonBackgroundPressed");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Pressed", "RootGrid.BorderBrush", "ButtonBorderBrushPressed");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Pressed", "ContentPresenter.Foreground", "ButtonForegroundPressed");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Pressed", "ChevronIcon.Foreground", "DropDownButtonForegroundSecondaryPressed");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Disabled", "RootGrid.Background", "ButtonBackgroundDisabled");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Disabled", "RootGrid.BorderBrush", "ButtonBorderBrushDisabled");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Disabled", "ContentPresenter.Foreground", "ButtonForegroundDisabled");
            AssertStateSetterDynamicResource(rootGrid, "CommonStates", "Disabled", "ChevronIcon.Foreground", "ButtonForegroundDisabled");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2DropDownButtonHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "DropDownButtonForegroundSecondary", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "DropDownButtonForegroundSecondaryPointerOver", "TextFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "DropDownButtonForegroundSecondaryPressed", "TextFillColorTertiaryBrush");
            }

            AssertThemeResourceReference("HighContrast", "DropDownButtonForegroundSecondary", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "DropDownButtonForegroundSecondaryPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "DropDownButtonForegroundSecondaryPressed", "SystemColorHighlightColorBrush");
        });
    }

    [TestMethod]
    public void VerifyDropDownButtonTemplateUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new ModernWpf.Controls.DropDownButton
            {
                Width = 140,
                Height = 44,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                CharacterSpacing = 23,
                Content = "Options",
                ContentTransitions = transitions,
                IsTextScaleFactorEnabled = false
            };

            using var host = new TestWindowHost(button, width: 220, height: 120);
            host.UpdateLayout();

            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template root to use GridEx chrome.");
            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template to use ContentPresenterEx.");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, rootGrid.BackgroundSizing);
            Assert.IsNotNull(rootGrid.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), rootGrid.BackgroundTransition.Duration);
            Assert.AreEqual(23, presenter.CharacterSpacing);
            Assert.AreEqual("Options", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentControlEx>(button));
        });
    }

    [TestMethod]
    public void DropDownButtonChevronParticipatesInLayout()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Email"
            };

            using var host = new TestWindowHost(button, width: 220, height: 120);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template to use ContentPresenterEx.");
            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button)
                ?? throw new AssertFailedException("Expected DropDownButton chevron icon.");

            var presenterBounds = presenter.TransformToAncestor(button).TransformBounds(new Rect(presenter.RenderSize));
            var chevronBounds = chevron.TransformToAncestor(button).TransformBounds(new Rect(chevron.RenderSize));

            Assert.IsTrue(chevronBounds.Width > 0, "Chevron should have positive layout width.");
            Assert.IsTrue(chevronBounds.Height > 0, "Chevron should have positive layout height.");
            Assert.IsTrue(
                chevronBounds.Left > presenterBounds.Left,
                $"Chevron should be laid out to the right of content. Content={presenterBounds}; Chevron={chevronBounds}");
            Assert.IsTrue(
                chevronBounds.Right <= button.ActualWidth,
                $"Chevron should fit inside the DropDownButton bounds. ButtonWidth={button.ActualWidth}; Chevron={chevronBounds}");
        });
    }

    [TestMethod]
    public void AnimatedChevronStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options"
            };

            using var host = new TestWindowHost(button, width: 220, height: 120);
            host.UpdateLayout();

            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template root to use GridEx chrome.");
            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button)
                ?? throw new AssertFailedException("Expected DropDownButton chevron icon.");

            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "ChevronIcon.(ui:AnimatedIcon.State)", "PointerOver");
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "RootGrid.Background", button.TryFindResource("ButtonBackgroundPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "RootGrid.BorderBrush", button.TryFindResource("ButtonBorderBrushPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "ContentPresenter.Foreground", button.TryFindResource("ButtonForegroundPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "ChevronIcon.Foreground", button.TryFindResource("DropDownButtonForegroundSecondaryPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "ChevronIcon.(ui:AnimatedIcon.State)", "Pressed");
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "RootGrid.Background", button.TryFindResource("ButtonBackgroundPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "RootGrid.BorderBrush", button.TryFindResource("ButtonBorderBrushPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "ContentPresenter.Foreground", button.TryFindResource("ButtonForegroundPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "ChevronIcon.Foreground", button.TryFindResource("DropDownButtonForegroundSecondaryPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "ChevronIcon.(ui:AnimatedIcon.State)", "Normal");
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "RootGrid.Background", button.TryFindResource("ButtonBackgroundDisabled"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "RootGrid.BorderBrush", button.TryFindResource("ButtonBorderBrushDisabled"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "ContentPresenter.Foreground", button.TryFindResource("ButtonForegroundDisabled"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "ChevronIcon.Foreground", button.TryFindResource("ButtonForegroundDisabled"));

            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "PointerOver", false));
            Assert.AreEqual("PointerOver", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "Pressed", false));
            Assert.AreEqual("Pressed", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "Disabled", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "Normal", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));
        });
    }

    [TestMethod]
    public void FlyoutEventsTrackExpandCollapseState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "Flyout content",
                    MinWidth = 120,
                    MinHeight = 32
                }
            };
            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options",
                Flyout = flyout
            };

            using var host = new TestWindowHost(button, width: 320, height: 160);
            host.UpdateLayout();

            var provider = GetExpandCollapseProvider(button);

            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);
            Assert.IsFalse(button.IsFlyoutOpen);

            provider.Expand();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.IsTrue(button.IsFlyoutOpen);
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
            Assert.IsFalse(button.IsFlyoutOpen);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);
        });
    }

    private static void AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, string target, object expectedValue)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var setter = state.Setters.Single(item => item.Target == target);

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertStateSetterDynamicResource(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedResourceKey)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var setter = state.Setters.Single(item => item.Target == target);

        AssertResourceReferenceExpression(
            setter.ReadLocalValue(VisualStateSetter.ValueProperty),
            expectedResourceKey);
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

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static IExpandCollapseProvider GetExpandCollapseProvider(ModernWpf.Controls.DropDownButton button)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.ExpandCollapse) is IExpandCollapseProvider provider)
        {
            return provider;
        }

        Assert.Fail("DropDownButton should expose IExpandCollapseProvider.");
        throw new InvalidOperationException();
    }
}
