using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RatingControl;

[TestClass]
public class RatingControlApiTests
{
    private const string FontSizeForRenderingResourceKey = "RatingControlFontSizeForRendering";
    private const string ItemSpacingResourceKey = "RatingControlItemSpacing";
    private const string CaptionTopMarginResourceKey = "RatingControlCaptionTopMargin";

    [TestMethod]
    public void VerifyDefaultStyleAndWinUI3Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/RatingControl/RatingControl.xaml", UriKind.Relative)
            };
            var style = (Style)resources[typeof(ModernWpf.Controls.RatingControl)];

            AssertSetterValue(style, FrameworkElement.MinHeightProperty, 32.0);
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "RatingControlCaptionForeground");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.RatingControl.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(style, Control.FontFamilyProperty, "SymbolThemeFontFamily");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.RatingControl.ItemInfoProperty, "MUX_RatingControlDefaultFontInfo");
            Assert.IsInstanceOfType(FindSetter(style, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                Background = Brushes.Yellow,
                Caption = "Rating API Test Caption",
                Style = style
            };

            using var host = new TestWindowHost(ratingControl, width: 420, height: 180);
            host.UpdateLayout();

            Assert.AreEqual(32.0, ratingControl.MinHeight);
            Assert.AreEqual(32.0, ratingControl.Height);
            Assert.AreSame(style, ratingControl.Style);
            AssertBrushEquals((Brush)ratingControl.TryFindResource("RatingControlCaptionForeground"), ratingControl.Foreground);
            Assert.AreEqual(ratingControl.TryFindResource("UseSystemFocusVisuals"), ratingControl.UseSystemFocusVisuals);
            Assert.AreSame(ratingControl.TryFindResource(SystemParameters.FocusVisualStyleKey), ratingControl.FocusVisualStyle);
            Assert.AreEqual(
                ((FontFamily)ratingControl.TryFindResource("SymbolThemeFontFamily")).Source,
                ratingControl.FontFamily.Source);

            AssertRatingFontInfo(ratingControl.ItemInfo, "\uE735", "\uE734");
            AssertRatingFontInfo(ratingControl.TryFindResource("MUX_RatingControlDefaultFontInfo"), "\uE735", "\uE734");
            AssertRatingFontInfo(ratingControl.TryFindResource("RatingControlDefaultFontInfo"), "\uE735", "\uE734");

            var layoutRoot = FindNamedDescendant<Grid>(ratingControl, "LayoutRoot");
            AssertBrushEquals(Brushes.Yellow, layoutRoot.Background);
            var commonStatesGroup = VisualStateManager.GetVisualStateGroups(layoutRoot)
                .Cast<VisualStateGroup>()
                .Single(group => group.Name == "CommonStates");
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "Disabled",
                    "Placeholder",
                    "PointerOverPlaceholder",
                    "PointerOverUnselected",
                    "Set",
                    "PointerOverSet"
                },
                commonStatesGroup.States.Cast<VisualState>().Select(state => state.Name).ToArray());

            var caption = FindNamedDescendant<TextBlock>(ratingControl, "Caption");
            Assert.IsTrue(double.IsNaN(caption.Height));
            Assert.AreEqual(4.0, caption.Margin.Left);
            Assert.AreEqual(-12.5, caption.Margin.Top);
            Assert.AreEqual(20.0, caption.Margin.Right);
            Assert.AreEqual(12.0, caption.FontSize);
            Assert.AreEqual(
                ((FontFamily)ratingControl.TryFindResource("CaptionControlThemeFontFamily")).Source,
                caption.FontFamily.Source);
            AssertBrushEquals(ratingControl.Foreground, caption.Foreground);
            Assert.AreEqual(VerticalAlignment.Center, caption.VerticalAlignment);
            Assert.IsFalse(caption.IsHitTestVisible);
            Assert.AreEqual("RatingCaption", AutomationProperties.GetName(caption));
            Assert.AreEqual("Rating API Test Caption", caption.Text);

            var captionStackPanel = FindNamedDescendant<StackPanel>(ratingControl, "CaptionStackPanel");
            Assert.AreEqual(Orientation.Horizontal, captionStackPanel.Orientation);
            Assert.AreEqual(new Thickness(-20), captionStackPanel.Margin);

            var backgroundStackPanel = FindNamedDescendant<StackPanelEx>(ratingControl, "RatingBackgroundStackPanel");
            Assert.AreEqual(Orientation.Horizontal, backgroundStackPanel.Orientation);
            AssertBrushEquals(Brushes.Transparent, backgroundStackPanel.Background);
            Assert.AreEqual(new Thickness(20, 20, 0, 20), backgroundStackPanel.Margin);
            Assert.AreEqual(5, backgroundStackPanel.Children.Count);
            var backgroundTranslateTransform = FindNamedDescendant<StackPanelEx>(ratingControl, "RatingBackgroundStackPanel").RenderTransform;
            Assert.IsInstanceOfType(backgroundTranslateTransform, typeof(TranslateTransform));
            Assert.AreEqual(0.0, ((TranslateTransform)backgroundTranslateTransform).Y);

            var foregroundContentPresenter = FindNamedDescendant<ContentPresenterEx>(ratingControl, "ForegroundContentPresenter");
            Assert.IsFalse(foregroundContentPresenter.IsHitTestVisible);
            Assert.IsInstanceOfType(foregroundContentPresenter.Content, typeof(StackPanel));

            var foregroundStackPanel = FindNamedDescendant<StackPanelEx>(ratingControl, "RatingForegroundStackPanel");
            Assert.AreEqual(Orientation.Horizontal, foregroundStackPanel.Orientation);
            Assert.IsFalse(foregroundStackPanel.IsHitTestVisible);
            Assert.AreEqual(new Thickness(40), foregroundStackPanel.Margin);
            Assert.AreEqual(5, foregroundStackPanel.Children.Count);
            Assert.IsInstanceOfType(foregroundStackPanel.RenderTransform, typeof(TranslateTransform));
            Assert.AreEqual(0.0, ((TranslateTransform)foregroundStackPanel.RenderTransform).Y);

            AssertDefaultTextRatingItem(backgroundStackPanel.Children[0], "\uE734", ratingControl.FontFamily, ratingControl);
            AssertDefaultTextRatingItem(foregroundStackPanel.Children[0], "\uE735", ratingControl.FontFamily, ratingControl);

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "RatingControlUnselectedForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "RatingControlSelectedForeground", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "RatingControlPlaceholderForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "RatingControlPointerOverPlaceholderForeground", "ControlAltFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "RatingControlPointerOverUnselectedForeground", "ControlAltFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "RatingControlPointerOverSelectedForeground", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "RatingControlDisabledSelectedForeground", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "RatingControlCaptionForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceValue(themeName, FontSizeForRenderingResourceKey, 32.0);
                AssertThemeResourceValue(themeName, ItemSpacingResourceKey, 8.0);
                AssertThemeResourceValue(themeName, CaptionTopMarginResourceKey, -12.5);
                AssertThemeRatingFontInfo(themeName, "MUX_RatingControlDefaultFontInfo");
                AssertThemeRatingFontInfo(themeName, "RatingControlDefaultFontInfo");
            }

            AssertThemeResourceReference("HighContrast", "RatingControlUnselectedForeground", "SystemControlForegroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "RatingControlSelectedForeground", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "RatingControlPlaceholderForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "RatingControlPointerOverPlaceholderForeground", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "RatingControlPointerOverUnselectedForeground", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "RatingControlPointerOverSelectedForeground", "SystemControlHighlightAccentBrush");
            AssertThemeSolidColorBrushReference("HighContrast", "RatingControlDisabledSelectedForeground", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "RatingControlCaptionForeground", "TextFillColorSecondaryBrush");
            AssertThemeResourceValue("HighContrast", FontSizeForRenderingResourceKey, 32.0);
            AssertThemeResourceValue("HighContrast", ItemSpacingResourceKey, 8.0);
            AssertThemeResourceValue("HighContrast", CaptionTopMarginResourceKey, -12.5);
            AssertThemeRatingFontInfo("HighContrast", "MUX_RatingControlDefaultFontInfo");
            AssertThemeRatingFontInfo("HighContrast", "RatingControlDefaultFontInfo");
        });
    }

    [TestMethod]
    public void CommonStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var ratingControl = new ModernWpf.Controls.RatingControl();
            using var host = new TestWindowHost(ratingControl, width: 420, height: 180);
            host.UpdateLayout();

            var layoutRoot = FindNamedDescendant<Grid>(ratingControl, "LayoutRoot");
            var commonStatesGroup = VisualStateManager.GetVisualStateGroups(layoutRoot)
                .Cast<VisualStateGroup>()
                .Single(group => group.Name == "CommonStates");
            var foregroundContentPresenter = FindNamedDescendant<ContentPresenterEx>(ratingControl, "ForegroundContentPresenter");
            var expectedStates = new[]
            {
                new { StateName = "Disabled", ResourceKey = "RatingControlDisabledSelectedForeground" },
                new { StateName = "Placeholder", ResourceKey = "RatingControlPlaceholderForeground" },
                new { StateName = "PointerOverPlaceholder", ResourceKey = "RatingControlPointerOverPlaceholderForeground" },
                new { StateName = "PointerOverUnselected", ResourceKey = "RatingControlPointerOverUnselectedForeground" },
                new { StateName = "Set", ResourceKey = "RatingControlSelectedForeground" },
                new { StateName = "PointerOverSet", ResourceKey = "RatingControlSelectedForeground" }
            };

            foreach (var expectedState in expectedStates)
            {
                var state = commonStatesGroup.States
                    .Cast<VisualState>()
                    .Single(item => item.Name == expectedState.StateName);
                Assert.IsInstanceOfType(state, typeof(VisualStateEx));

                var stateEx = (VisualStateEx)state;
                Assert.AreEqual(1, stateEx.Setters.Count, expectedState.StateName);
                Assert.AreEqual("ForegroundContentPresenter.Foreground", stateEx.Setters[0].Target, expectedState.StateName);

                Assert.IsTrue(
                    VisualStateManager.GoToState(ratingControl, expectedState.StateName, false),
                    $"Expected RatingControl to go to {expectedState.StateName}.");
                host.UpdateLayout();

                AssertBrushEquals(
                    (Brush)foregroundContentPresenter.TryFindResource(expectedState.ResourceKey),
                    foregroundContentPresenter.Foreground);
            }
        });
    }

    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl();
            Assert.IsNotNull(ratingControl);

            Assert.AreEqual(string.Empty, ratingControl.Caption);
            Assert.AreEqual(1, ratingControl.InitialSetValue);
            Assert.IsTrue(ratingControl.IsClearEnabled);
            Assert.IsFalse(ratingControl.IsReadOnly);
            Assert.AreEqual(5, ratingControl.MaxRating);
            Assert.AreEqual(-1.0, ratingControl.PlaceholderValue);
            Assert.AreEqual(-1.0, ratingControl.Value);

            ratingControl.Caption = "Rating API Test Caption";
            ratingControl.InitialSetValue = 2;
            ratingControl.IsClearEnabled = false;
            ratingControl.IsReadOnly = true;
            ratingControl.MaxRating = 10;
            ratingControl.PlaceholderValue = 3.0;
            ratingControl.Value = 2.0;

            var imageUri = new Uri("pack://application:,,,/ModernWpf.WinUI.Tests;component/Assets/rating_set.png", UriKind.Absolute);
            var imageInfo = new RatingItemImageInfo
            {
                Image = new BitmapImage(imageUri)
            };
            ratingControl.ItemInfo = imageInfo;

            WpfTestHost.DoEvents();

            Assert.AreEqual("Rating API Test Caption", ratingControl.Caption);
            Assert.AreEqual(2, ratingControl.InitialSetValue);
            Assert.IsFalse(ratingControl.IsClearEnabled);
            Assert.IsTrue(ratingControl.IsReadOnly);
            Assert.AreEqual(10, ratingControl.MaxRating);
            Assert.AreEqual(3.0, ratingControl.PlaceholderValue);
            Assert.AreEqual(2.0, ratingControl.Value);
            Assert.IsInstanceOfType(ratingControl.ItemInfo, typeof(RatingItemImageInfo));
            var image = ((RatingItemImageInfo)ratingControl.ItemInfo).Image as BitmapImage;
            Assert.IsNotNull(image);
            Assert.AreEqual(imageUri, image!.UriSource);
        });
    }

    [TestMethod]
    public void VerifyDontCrashWhenCollapsedAndValueSet()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                Visibility = Visibility.Collapsed,
                Value = 3.3
            };

            Assert.AreEqual(3.3, ratingControl.Value);
        });
    }

    [TestMethod]
    public void VerifyValuesCoercion()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl();
            Assert.IsNotNull(ratingControl);
            Assert.AreEqual(-1.0, ratingControl.PlaceholderValue);
            Assert.AreEqual(-1.0, ratingControl.Value);

            ratingControl.PlaceholderValue = 0.1;
            ratingControl.Value = 0.1;
            Assert.AreEqual(1.0, ratingControl.PlaceholderValue, "Should coerce small PlaceholderValue values to 1.0");
            Assert.AreEqual(1.0, ratingControl.Value, "Should coerce small Value values to 1.0");

            ratingControl.PlaceholderValue = 6.0;
            ratingControl.Value = 6.0;
            Assert.AreEqual(5.0, ratingControl.PlaceholderValue, "Should coerce PlaceholderValue above MaxRating back to MaxRating");
            Assert.AreEqual(5.0, ratingControl.Value, "Should coerce Value above MaxRating back to MaxRating");

            ratingControl.MaxRating = -2;
            Assert.AreEqual(1, ratingControl.MaxRating, "Should coerce MaxRating below 1 back up to 1.");
            Assert.AreEqual(1.0, ratingControl.PlaceholderValue, "Should auto-coerce now outdated PlaceholderValue above MaxRating back to MaxRating [2]");
            Assert.AreEqual(1.0, ratingControl.Value, "Should auto-coerce now outdated Value above MaxRating back to MaxRating [2]");

            ratingControl.PlaceholderValue = 6.0;
            ratingControl.Value = 6.0;
            Assert.AreEqual(1.0, ratingControl.PlaceholderValue, "Should coerce set PlaceholderValue above MaxRating back to MaxRating");
            Assert.AreEqual(1.0, ratingControl.Value, "Should coerce set Value above MaxRating back to MaxRating");
        });
    }

    [TestMethod]
    public void VerifySizeIsChangeableFromResource()
    {
        WpfTestHost.Run(() =>
        {
            var appResources = TestApplication.EnsureInitialized().Resources;
            var hadFontSizeOverride = appResources.Contains(FontSizeForRenderingResourceKey);
            var hadItemSpacingOverride = appResources.Contains(ItemSpacingResourceKey);
            var originalFontSizeOverride = hadFontSizeOverride ? appResources[FontSizeForRenderingResourceKey] : null;
            var originalItemSpacingOverride = hadItemSpacingOverride ? appResources[ItemSpacingResourceKey] : null;

            try
            {
                appResources.Remove(FontSizeForRenderingResourceKey);
                appResources.Remove(ItemSpacingResourceKey);
                var originalWidth = MeasureRatingWidth();

                appResources[FontSizeForRenderingResourceKey] = 20.0;
                var smallerFontWidth = MeasureRatingWidth();
                Assert.IsTrue(
                    smallerFontWidth < originalWidth,
                    $"Expected a smaller font rendering resource to reduce width. Original={originalWidth}, new={smallerFontWidth}");

                appResources[ItemSpacingResourceKey] = 20.0;
                var widerSpacingWidth = MeasureRatingWidth();
                Assert.IsTrue(
                    widerSpacingWidth > smallerFontWidth,
                    $"Expected a larger item spacing resource to increase width. Previous={smallerFontWidth}, new={widerSpacingWidth}");

                appResources[FontSizeForRenderingResourceKey] = 48.0;
                appResources.Remove(ItemSpacingResourceKey);
                var largerFontWidth = MeasureRatingWidth();
                Assert.IsTrue(
                    largerFontWidth > originalWidth,
                    $"Expected a larger font rendering resource to exceed default width. Original={originalWidth}, new={largerFontWidth}");
                Assert.IsTrue(
                    largerFontWidth > widerSpacingWidth,
                    $"Expected the larger font rendering resource to exceed the spacing-only width. Previous={widerSpacingWidth}, new={largerFontWidth}");
            }
            finally
            {
                RestoreResource(appResources, FontSizeForRenderingResourceKey, hadFontSizeOverride, originalFontSizeOverride);
                RestoreResource(appResources, ItemSpacingResourceKey, hadItemSpacingOverride, originalItemSpacingOverride);
            }
        });
    }

    [TestMethod]
    public void CaptionWidthUsesWinUIPhysicalPixelMetrics()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                Caption = "312 ratings",
                UseLayoutRounding = true
            };
            using var host = new TestWindowHost(ratingControl, width: 420, height: 180);
            host.UpdateLayout();

            var caption = FindNamedDescendant<TextBlock>(ratingControl, "Caption");
            var dpiScale = VisualTreeHelper.GetDpi(caption).DpiScaleX;
            var renderingFontSize = (double)ratingControl.FindResource(FontSizeForRenderingResourceKey);
            var itemSpacing = (double)ratingControl.FindResource(ItemSpacingResourceKey);
            var ratingItemsWidth = (ratingControl.MaxRating * renderingFontSize / 2.0) +
                ((ratingControl.MaxRating - 1) * itemSpacing);
            var expectedWidth = ratingItemsWidth + 12.0 + caption.ActualWidth - (1.0 / dpiScale);

            Assert.AreEqual(expectedWidth, ratingControl.ActualWidth, 0.001);
        });
    }

    private static double MeasureRatingWidth()
    {
        var ratingControl = new ModernWpf.Controls.RatingControl();

        using var host = new TestWindowHost(ratingControl, width: 420, height: 180);
        host.UpdateLayout();
        return ratingControl.ActualWidth;
    }

    private static void RestoreResource(ResourceDictionary resources, string key, bool hadOriginalValue, object? originalValue)
    {
        if (hadOriginalValue)
        {
            resources[key] = originalValue;
        }
        else
        {
            resources.Remove(key);
        }
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertThemeResourceType<T>(string themeName, string resourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(T));
    }

    private static void AssertThemeSolidColorBrushReference(string themeName, string resourceKey, object expectedBrushKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedBrushKey), $"{themeName} is missing {expectedBrushKey}.");
        var brush = (SolidColorBrush)themeDictionary[resourceKey];
        var expectedBrush = (SolidColorBrush)themeDictionary[expectedBrushKey];
        Assert.AreEqual(expectedBrush.Color, brush.Color, $"{themeName}:{resourceKey}");
        Assert.AreEqual(expectedBrush.Opacity, brush.Opacity, $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeRatingFontInfo(string themeName, string resourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        AssertRatingFontInfo(themeDictionary[resourceKey], "\uE735", "\uE734");
    }

    private static void AssertRatingFontInfo(object itemInfo, string expectedGlyph, string expectedUnsetGlyph)
    {
        Assert.IsInstanceOfType(itemInfo, typeof(RatingItemFontInfo));
        var fontInfo = (RatingItemFontInfo)itemInfo;
        Assert.AreEqual(expectedGlyph, fontInfo.Glyph);
        Assert.AreEqual(expectedUnsetGlyph, fontInfo.UnsetGlyph);
    }

    private static void AssertDefaultTextRatingItem(UIElement item, string expectedText, FontFamily expectedFontFamily, FrameworkElement resourceOwner)
    {
        Assert.IsInstanceOfType(item, typeof(TextBlock));
        var textBlock = (TextBlock)item;
        Assert.AreEqual(new Thickness(-8, -8, 0, 0), textBlock.Margin);
        Assert.AreEqual(32.0, textBlock.FontSize);
        Assert.AreEqual(expectedText, textBlock.Text);
        Assert.AreEqual(expectedFontFamily.Source, textBlock.FontFamily.Source);

        if (expectedText == "\uE734")
        {
            AssertBrushEquals((Brush)resourceOwner.TryFindResource("RatingControlUnselectedForeground"), textBlock.Foreground);
        }
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

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var current = style; current != null; current = current.BasedOn)
        {
            var setter = current.Setters
                .OfType<Setter>()
                .FirstOrDefault(item => item.Property == property);

            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
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
}
