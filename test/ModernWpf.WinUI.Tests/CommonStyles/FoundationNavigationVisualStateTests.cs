using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class FoundationNavigationVisualStateTests
{
    [TestMethod]
    public void FoundationStylesExposeOfficialWpfFluentKeys()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            Assert.AreEqual(typeof(ContentControl), AssertStyle(typeof(ContentControl)).TargetType);
            Assert.AreEqual(typeof(HeaderedContentControl), AssertStyle(typeof(HeaderedContentControl)).TargetType);
            AssertImplicitBasedOnDefault<ItemsControl>("DefaultItemsControlStyle");
            AssertImplicitBasedOnDefault<UserControl>("DefaultUserControlStyle");
            AssertImplicitBasedOnDefault<Page>("DefaultPageStyle");
            AssertImplicitBasedOnDefault<Frame>("DefaultFrameStyle");
            AssertImplicitBasedOnDefault<NavigationWindow>("DefaultNavigationWindowStyle");

            Assert.IsInstanceOfType(Application.Current.FindResource("FrameNavigationButtonJournalEntryStyle"), typeof(Style));
            Assert.IsInstanceOfType(Application.Current.FindResource("FrameTemplateKey"), typeof(ControlTemplate));
            Assert.IsInstanceOfType(Application.Current.FindResource("NavigationWindowTemplateKey"), typeof(ControlTemplate));
        });
    }

    [TestMethod]
    public void FoundationTemplatesUseOfficialWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var contentControl = new ContentControl
            {
                Style = AssertStyle(typeof(ContentControl)),
                Content = "Content"
            };
            var headeredContentControl = new HeaderedContentControl
            {
                Style = AssertStyle(typeof(HeaderedContentControl)),
                Header = "Header",
                Content = "Body"
            };
            var itemsControl = new ItemsControl
            {
                Style = AssertStyle("DefaultItemsControlStyle"),
                Items = { "Item" }
            };
            var userControl = new UserControl
            {
                Style = AssertStyle("DefaultUserControlStyle"),
                Content = "User"
            };
            var page = new Page
            {
                Style = AssertStyle("DefaultPageStyle"),
                Content = "Page"
            };
            var pageHost = new Frame
            {
                Content = page
            };
            var frame = new Frame
            {
                Style = AssertStyle("DefaultFrameStyle"),
                Content = new TextBlock { Text = "Frame" }
            };

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { contentControl, headeredContentControl, itemsControl, userControl, pageHost, frame }
            }, width: 420, height: 360);
            host.UpdateLayout();

            AssertWpfPresenter(contentControl, contentControl.Content);
            AssertWpfPresenter(headeredContentControl, headeredContentControl.Header);
            AssertWpfPresenter(headeredContentControl, headeredContentControl.Content);
            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<ItemsPresenter>(itemsControl));
            AssertWpfPresenter(userControl, userControl.Content);
            AssertWpfPresenter(page, page.Content);
            Assert.IsNotNull(frame.Template.FindName("PART_FrameCP", frame));
        });
    }

    [TestMethod]
    public void TextBlockStylesUseOfficialWpfFluentShapeWithLegacyAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var baseStyle = AssertStyle("BaseTextBlockStyle");
            Assert.AreEqual(typeof(TextBlock), baseStyle.TargetType);
            Assert.IsFalse(baseStyle.Setters.OfType<Setter>().Any(setter => setter.Property == TextBlock.FontFamilyProperty));
            AssertStyleSetter(baseStyle, TextBlock.FontWeightProperty, FontWeights.SemiBold);
            AssertStyleSetter(baseStyle, TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            AssertStyleSetter(baseStyle, TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            AssertStyleSetter(baseStyle, TextBlock.LineStackingStrategyProperty, LineStackingStrategy.MaxHeight);

            Assert.AreSame(baseStyle, AssertStyle("BodyStrongTextBlockStyle").BasedOn);
            Assert.IsInstanceOfType(Application.Current.FindResource("HeaderTextBlockStyle"), typeof(Style));
            Assert.IsInstanceOfType(Application.Current.FindResource("SubheaderTextBlockStyle"), typeof(Style));
        });
    }

    [TestMethod]
    public void FoundationThemeResourcesExposeOfficialWpfFluentAliases()
    {
        foreach (var themeName in new[] { "Light", "Dark" })
        {
            AssertThemeResourceReference(themeName, "WindowBackground", "SolidBackgroundFillColorBaseBrush");
            AssertThemeResourceReference(themeName, "WindowForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference(themeName, "FrameBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference(themeName, "FrameForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference(themeName, "FrameMenuItemBackgroundSelected", "SubtleFillColorTertiaryBrush");
            AssertThemeResourceReference(themeName, "FrameMenuItemForegroundDisabled", "TextFillColorDisabledBrush");
            AssertThemeResourceReference(themeName, "NavigationWindowBackground", "SolidBackgroundFillColorBaseBrush");
            AssertThemeResourceReference(themeName, "NavigationWindowForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference(themeName, "PageForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference(themeName, "PageBackground", "SubtleFillColorTransparentBrush");
        }

        AssertThemeResourceReference("HighContrast", "WindowBackground", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "WindowForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "FrameBackground", "SystemControlTransparentBrush");
        AssertThemeResourceReference("HighContrast", "FrameForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "FrameMenuItemBackgroundSelected", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "FrameMenuItemForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "NavigationWindowBackground", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "NavigationWindowForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "PageForeground", "SystemColorWindowTextColorBrush");
        AssertThemeResourceReference("HighContrast", "PageBackground", "SystemControlTransparentBrush");
    }

    [TestMethod]
    public void FoundationFilesUseOfficialWpfPresenterShapeAndBackportSubstitutions()
    {
        var repoRoot = FindRepoRoot();
        foreach (var relativePath in new[]
        {
            Path.Combine("ModernWpf", "Styles", "ContentControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "HeaderedContentControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "ItemsControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "UserControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "Page.xaml"),
            Path.Combine("ModernWpf", "Styles", "Frame.xaml"),
            Path.Combine("ModernWpf", "Styles", "NavigationWindow.xaml"),
            Path.Combine("ModernWpf", "Styles", "TextStyles.xaml"),
            Path.Combine("ModernWpf", "Styles", "Thumb.xaml")
        })
        {
            var text = File.ReadAllText(Path.Combine(repoRoot, relativePath));
            Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal), relativePath);
            Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal), relativePath);
            Assert.IsFalse(text.Contains("FontIconFallback", System.StringComparison.Ordinal), relativePath);
            Assert.IsFalse(text.Contains("Fluent.Controls", System.StringComparison.Ordinal), relativePath);
            Assert.IsFalse(text.Contains("System.Runtime", System.StringComparison.Ordinal), relativePath);
        }

        var textStyles = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "Styles", "TextStyles.xaml"));
        Assert.IsFalse(textStyles.Contains("SystemFonts.MessageFontFamilyKey", System.StringComparison.Ordinal));
        Assert.IsTrue(textStyles.Contains("HeaderTextBlockStyle", System.StringComparison.Ordinal));
        Assert.IsTrue(textStyles.Contains("SubheaderTextBlockStyle", System.StringComparison.Ordinal));
    }

    private static Style AssertStyle(object resourceKey)
    {
        var style = Application.Current.FindResource(resourceKey) as Style;
        Assert.IsNotNull(style, $"Expected style resource {resourceKey}.");
        return style!;
    }

    private static void AssertImplicitBasedOnDefault<TControl>(string defaultStyleKey)
    {
        var defaultStyle = AssertStyle(defaultStyleKey);
        var implicitStyle = AssertStyle(typeof(TControl));
        Assert.AreEqual(typeof(TControl), defaultStyle.TargetType);
        Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
    }

    private static void AssertStyleSetter(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertWpfPresenter(DependencyObject root, object expectedContent)
    {
        var presenter = VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<ContentPresenter>()
            .FirstOrDefault(item => Equals(item.Content, expectedContent))
            ?? throw new AssertFailedException($"Expected {root.GetType().Name} template to use WPF ContentPresenter.");

        Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
