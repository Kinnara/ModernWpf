using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Pages.WpfGallery.Samples;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryApplicationResourceTests
    {
        private const string PlatformFluentThemePrefix =
            "pack://application:,,,/PresentationFramework.Fluent;component/Themes/";

        [TestMethod]
        public void GalleryUsesRecommendedFluentResourceEntry()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                Assert.IsTrue(app.Resources.MergedDictionaries.OfType<ThemeResources>().Any());
                Assert.IsTrue(app.Resources.MergedDictionaries.OfType<FluentControlsResources>().Any());
                Assert.IsFalse(app.Resources.MergedDictionaries.OfType<XamlControlsResources>().Any());

                var fontFamily = (FontFamily)app.FindResource("SymbolThemeFontFamily");
                Assert.AreEqual("Segoe Fluent Icons, Segoe MDL2 Assets", fontFamily.Source);

#if NET10_0_OR_GREATER
#pragma warning disable WPF0001
                Assert.AreEqual("System", app.ThemeMode.Value);
#pragma warning restore WPF0001
                Assert.AreEqual(1, CountPlatformFluentThemeDictionaries(app.Resources));
#else
                Assert.AreEqual(0, CountPlatformFluentThemeDictionaries(app.Resources));
#endif
            });
        }

        [TestMethod]
        public void GalleryMergesWpfGalleryPageStylesResourceDictionary()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                Assert.IsTrue(HasMergedDictionarySource(app.Resources, "Resources/PageStyles.xaml"));

                Assert.AreEqual(12d, app.FindResource("CaptionTextBlockFontSize"));
                Assert.AreEqual(14d, app.FindResource("BodyTextBlockFontSize"));
                Assert.AreEqual(20d, app.FindResource("SubtitleTextBlockFontSize"));
                Assert.AreEqual(28d, app.FindResource("TitleTextBlockFontSize"));
                Assert.AreEqual(40d, app.FindResource("TitleLargeTextBlockFontSize"));
                Assert.AreEqual(68d, app.FindResource("DisplayTextBlockFontSize"));

                var galleryRootStyle = (Style)app.FindResource("GalleryPageRootStyle");
                AssertDynamicResourceSetter(galleryRootStyle, Panel.BackgroundProperty, "SolidBackgroundFillColorTertiaryBrush");
                AssertStyleSetter(galleryRootStyle, TextElement.FontSizeProperty, app.FindResource("BodyTextBlockFontSize"));

                var baseTextStyle = (Style)app.FindResource("BaseTextBlockStyle");
                AssertNoStyleSetter(baseTextStyle, TextBlock.ForegroundProperty);
                AssertStyleSetter(baseTextStyle, TextBlock.FontSizeProperty, app.FindResource("BodyTextBlockFontSize"));
                AssertStyleSetter(baseTextStyle, TextBlock.FontWeightProperty, FontWeights.SemiBold);
                AssertStyleSetter(baseTextStyle, TextBlock.LineStackingStrategyProperty, LineStackingStrategy.MaxHeight);
                AssertStyleSetter(baseTextStyle, TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
                AssertStyleSetter(baseTextStyle, TextBlock.TextWrappingProperty, TextWrapping.Wrap);

                var titleStyle = (Style)app.FindResource("TitleTextBlockStyle");
                AssertStyleSetter(titleStyle, TextBlock.FontSizeProperty, app.FindResource("TitleTextBlockFontSize"));

                var displayStyle = (Style)app.FindResource("DisplayTextBlockStyle");
                AssertStyleSetter(displayStyle, TextBlock.FontSizeProperty, app.FindResource("DisplayTextBlockFontSize"));

                Assert.IsNull(app.TryFindResource("ControlExampleDisplayBrush"));

                AssertUserDashboardImageBrushResources(app);

                var colorTilesStyle = (Style)app.FindResource("ColorTilesPanelStyle");
                AssertDynamicResourceSetter(colorTilesStyle, Border.BackgroundProperty, "ControlExampleDisplayBrush");
                AssertDynamicResourceSetter(colorTilesStyle, Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");
                AssertStyleSetter(colorTilesStyle, Border.BorderThicknessProperty, new Thickness(1));
                AssertStyleSetter(colorTilesStyle, Border.CornerRadiusProperty, new CornerRadius(8));
            });
        }

        [TestMethod]
        public void UserDashboardImageConverterResolvesWpfGalleryPageStyleBrushes()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                var converter = new ImageIdToBrushConverter();

                Assert.AreSame(
                    app.FindResource("p91"),
                    converter.Convert("p91", typeof(ImageBrush), null, CultureInfo.InvariantCulture));
                Assert.AreSame(
                    app.FindResource("p91"),
                    converter.Convert("91", typeof(ImageBrush), null, CultureInfo.InvariantCulture));
                Assert.AreSame(
                    app.FindResource("p91"),
                    converter.Convert(null, typeof(ImageBrush), null, CultureInfo.InvariantCulture));
            });
        }

        [TestMethod]
        public void ColorTileStyleKeepsWpfGalleryNaturalHeight()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                var colorTileStyle = (Style)app.FindResource(typeof(ColorTile));
                Assert.IsNotNull(colorTileStyle);
                Assert.IsFalse(colorTileStyle.Setters.OfType<Setter>()
                    .Any(setter => setter.Property == FrameworkElement.MinHeightProperty));

                AssertStyleSetter(colorTileStyle, Control.FocusableProperty, false);
                AssertStyleSetter(colorTileStyle, UIElement.SnapsToDevicePixelsProperty, true);
                AssertStyleSetter(colorTileStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
                AssertStyleSetter(colorTileStyle, ColorTile.TileRadiusProperty, new CornerRadius(0));
            });
        }

        [TestMethod]
        public void GalleryMergesWpfGalleryTemplatesResourceDictionary()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                Assert.IsTrue(HasMergedDictionarySource(app.Resources, "Resources/Templates.xaml"));

                var wrapPanelTemplate = (ItemsPanelTemplate)app.FindResource("WrapPanelTemplate");
                var wrapPanel = (System.Windows.Controls.WrapPanel)wrapPanelTemplate.LoadContent();
                Assert.AreEqual(new Thickness(10), wrapPanel.Margin);
                Assert.AreEqual(Orientation.Horizontal, wrapPanel.Orientation);

                var navigationCardTemplate = (DataTemplate)app.FindResource("NavigationCardTemplate");
                var button = (Button)navigationCardTemplate.LoadContent();
                Assert.AreEqual(360d, button.Width);
                Assert.AreEqual(90d, button.Height);
                Assert.AreEqual(new Thickness(7), button.Margin);
                Assert.AreEqual(new Thickness(20, 10, 20, 10), button.Padding);
                Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalContentAlignment);
                AssertBindingPath(button, Button.CommandProperty, "ViewModel.NavigateCommand");
                AssertBindingPath(button, Button.CommandParameterProperty, "PageType");

                var automationNameBinding = BindingOperations.GetBindingExpression(button, AutomationProperties.NameProperty);
                Assert.IsNotNull(automationNameBinding);
                Assert.AreEqual("Title", automationNameBinding.ParentBinding.Path.Path);
                Assert.AreEqual("{0}Page", automationNameBinding.ParentBinding.StringFormat);

                var outerStack = (StackPanel)button.Content;
                Assert.AreEqual(Orientation.Horizontal, outerStack.Orientation);

                var image = (Image)outerStack.Children[0];
                Assert.AreEqual(50d, image.Width);
                Assert.AreEqual(50d, image.Height);
                Assert.AreEqual(new Thickness(0, 0, 8, 0), image.Margin);
                Assert.AreEqual(Stretch.Uniform, image.Stretch);

                var innerStack = (StackPanel)outerStack.Children[1];
                Assert.AreEqual(Orientation.Vertical, innerStack.Orientation);

                var textBlocks = innerStack.Children.OfType<TextBlock>().ToArray();
                Assert.AreEqual(2, textBlocks.Length);
                Assert.AreEqual(new Thickness(10, 0, 0, 0), textBlocks[0].Margin);
                Assert.AreEqual(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(textBlocks[0]));
                Assert.AreEqual(240d, textBlocks[1].Width);
                Assert.AreEqual(0.7, textBlocks[1].Opacity, 0.001);
                AssertBindingPath(textBlocks[1], TextBlock.TextProperty, "Description");
            });
        }

        [TestMethod]
        public void HomeHeaderTilesMatchWpfGalleryReferenceSlotGeometry()
        {
            WpfTestHost.Run(() =>
            {
                var page = new HomePage();
                RenderElement(page, () =>
                {
                    var tileGallery = (TileGallery)page.FindName("HomeTileGallery");
                    var tilesPanel = (StackPanel)tileGallery.FindName("TilesPanel");
                    var tiles = tilesPanel.Children.OfType<HeaderTile>().ToArray();

                    Assert.AreEqual(5, tiles.Length);

                    foreach (var tile in tiles)
                    {
                        Assert.AreEqual(198d, tile.Width);
                        Assert.AreEqual(220d, tile.Height);
                        var rootButton = (Button)tile.FindName("RootButton");
                        Assert.AreEqual(new Thickness(6), rootButton.Margin);
                        AssertBindingPath(rootButton, AutomationProperties.NameProperty, "Title");
                    }

                    Assert.AreEqual(new Thickness(24, 0, 6, 0), tiles[0].Margin);

                    foreach (var tile in tiles.Skip(1))
                    {
                        Assert.AreEqual(new Thickness(0), tile.Margin);
                    }

                    CollectionAssert.AreEqual(
                        new[] { "Getting started", "Windows design", "WPF GitHub", "Code samples", "Partner Center" },
                        tiles.Select(GetHeaderTileAutomationName).ToArray());

                    Assert.AreEqual("Scroll left", AutomationProperties.GetName((Button)tileGallery.FindName("ScrollBackButton")));
                    Assert.AreEqual("Scroll right", AutomationProperties.GetName((Button)tileGallery.FindName("ScrollForwardButton")));
                });
            });
        }

        [TestMethod]
        public void HomeHeaderTilesUseWpfGalleryAcrylicFillResources()
        {
            WpfTestHost.Run(() =>
            {
                var originalTheme = ThemeManager.Current.ApplicationTheme;
                try
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    var lightTile = new HeaderTile();
                    var lightRootButton = (Button)lightTile.FindName("RootButton");
                    var lightColor = Color.FromRgb(0xF9, 0xF9, 0xF9);
                    AssertHeaderTileBrush(lightRootButton.Resources["ButtonBackground"], lightColor, 0.8);
                    AssertHeaderTileBrush(lightRootButton.Resources["ButtonBackgroundPointerOver"], lightColor, 0.9);
                    AssertHeaderTileBrush(lightRootButton.Resources["ButtonBackgroundPressed"], lightColor, 1.0);

                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    var darkTile = new HeaderTile();
                    var darkRootButton = (Button)darkTile.FindName("RootButton");
                    var darkColor = Color.FromRgb(0x2C, 0x2C, 0x2C);
                    AssertHeaderTileBrush(darkRootButton.Resources["ButtonBackground"], darkColor, 0.8);
                    AssertHeaderTileBrush(darkRootButton.Resources["ButtonBackgroundPointerOver"], darkColor, 0.9);
                    AssertHeaderTileBrush(darkRootButton.Resources["ButtonBackgroundPressed"], darkColor, 1.0);
                }
                finally
                {
                    ThemeManager.Current.ApplicationTheme = originalTheme;
                }
            });
        }

        [TestMethod]
        public void HomeHeaderTilesExposeRootButtonAutomationPeer()
        {
            WpfTestHost.Run(() =>
            {
                var tile = new HeaderTile
                {
                    Title = "Getting started",
                    Description = "An overview of app development options, tools, and samples."
                };

                RenderElement(tile, () =>
                {
                    var peer = UIElementAutomationPeer.CreatePeerForElement(tile);
                    var children = peer.GetChildren();

                    Assert.IsNotNull(children);
                    Assert.AreEqual(1, children.Count);
                    Assert.AreEqual(AutomationControlType.Button, children[0].GetAutomationControlType());
                    Assert.AreEqual("RootButton", children[0].GetAutomationId());
                    Assert.AreEqual("Getting started", children[0].GetName());
                });
            });
        }

        [TestMethod]
        public void ControlExampleSourceCodeTextStyleUsesWpfGalleryReferenceResources()
        {
            WpfTestHost.Run(() =>
            {
                var style = (Style)Application.Current.FindResource("SelectionTextBox");

                AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TextControlForeground");
                AssertDynamicResourceSetter(style, Control.FontSizeProperty, "ControlContentThemeFontSize");
            });
        }

        [TestMethod]
        public void ControlExampleTemplateMatchesWpfGalleryReferenceDivider()
        {
            WpfTestHost.Run(() =>
            {
                var controlExample = new ControlExample
                {
                    HeaderText = "Reference sample",
                    ExampleContent = new Button { Content = "Example" },
                    XamlCode = "<Button Content=\"Example\" />",
                    CSharpCode = "button.Content = \"Example\";"
                };

                var window = new Window
                {
                    Width = 480,
                    Height = 360,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = controlExample
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var displayBorder = (Border)controlExample.Template.FindName("ExampleDisplayBorder", controlExample);
                    Assert.IsNotNull(displayBorder);
                    Assert.AreEqual(
                        (double)Application.Current.FindResource("BodyTextBlockFontSize"),
                        TextElement.GetFontSize(displayBorder));

                    var sourceCodeExpander = (Expander)controlExample.Template.FindName("SourceCodeExpander", controlExample);
                    Assert.IsNotNull(sourceCodeExpander);
                    Assert.AreEqual("Source code", sourceCodeExpander.Header);
                    Assert.AreEqual(42.0, sourceCodeExpander.MinHeight);
                    Assert.IsTrue(
                        sourceCodeExpander.ActualHeight >= 42.0 && sourceCodeExpander.ActualHeight <= 43.5,
                        "Expected collapsed source expander height near the official WPF Gallery 42-43px row; actual " + sourceCodeExpander.ActualHeight);

                    var divider = (Border)controlExample.Template.FindName("Border", controlExample);
                    Assert.IsNotNull(divider);
                    Assert.AreEqual(new Thickness(0, 20, 0, 20), divider.Margin);
                    Assert.AreEqual(new Thickness(1), divider.BorderThickness);
                    Assert.IsNull(divider.BorderBrush);
                    Assert.AreEqual(Visibility.Visible, divider.Visibility);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ControlExampleLoadsSourceCodeFromReferenceStyleUris()
        {
            WpfTestHost.Run(() =>
            {
                var controlExample = new ControlExample
                {
                    XamlCodeSource = new Uri("Samples/SampleCode/XamlUICommand/XamlUICommandSample1_xaml.txt", UriKind.Relative),
                    CSharpCodeSource = new Uri("Samples/SampleCode/XamlUICommand/XamlUICommandSample1_cs.txt", UriKind.Relative)
                };

                StringAssert.Contains(controlExample.XamlCode, "<XamlUICommand");
                StringAssert.Contains(controlExample.CSharpCode, "ExecuteRequested");
            });
        }

        [TestMethod]
        public void LocalControlImagesIncludeOfficialWpfGalleryReferenceAssets()
        {
            WpfTestHost.Run(() =>
            {
                var officialOnlyAssets = new[]
                {
                    "AutomationProperties.png",
                    "InkCanvas.png",
                    "InkToolbar.png",
                    "InputValidation.png",
                    "RadioButtons.png",
                    "RevealFocus.png"
                };

                foreach (var asset in officialOnlyAssets)
                {
                    AssertGalleryResourceExists("Assets/ControlImages/" + asset);
                }
            });
        }

        [TestMethod]
        public void WpfGalleryCatalogControlImagesResolveFromApplicationResources()
        {
            WpfTestHost.Run(() =>
            {
                var wpfGalleryGroupIds = new[]
                {
                    "DesignGuidance",
                    "Samples",
                    "BasicInput",
                    "Collections",
                    "DateAndCalendar",
                    "Layout",
                    "Media",
                    "Navigation",
                    "StatusAndInfo",
                    "Text",
                    "System"
                };

                var imagePaths = wpfGalleryGroupIds
                    .Select(GalleryCatalog.FindGroup)
                    .Where(group => group != null)
                    .SelectMany(group => new[] { group.ImagePath }.Concat(group.Items.Select(item => item.ImagePath)))
                    .Where(path => path.IndexOf("/Assets/ControlImages/", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(GetGalleryComponentRelativePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                Assert.IsTrue(imagePaths.Length > 0);

                foreach (var imagePath in imagePaths)
                {
                    AssertGalleryResourceExists(imagePath);
                }
            });
        }

        private static int CountPlatformFluentThemeDictionaries(ResourceDictionary resources)
        {
            var count = IsPlatformFluentThemeDictionary(resources) ? 1 : 0;

            foreach (var dictionary in resources.MergedDictionaries)
            {
                count += CountPlatformFluentThemeDictionaries(dictionary);
            }

            return count;
        }

        private static bool IsPlatformFluentThemeDictionary(ResourceDictionary dictionary)
        {
            return dictionary.Source?.ToString().StartsWith(PlatformFluentThemePrefix, StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool HasMergedDictionarySource(ResourceDictionary resources, string sourceSuffix)
        {
            foreach (var dictionary in resources.MergedDictionaries)
            {
                if (dictionary.Source?.ToString().EndsWith(sourceSuffix, StringComparison.OrdinalIgnoreCase) == true ||
                    HasMergedDictionarySource(dictionary, sourceSuffix))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertHeaderTileBrush(object resource, Color expectedColor, double opacity)
        {
            var brush = resource as SolidColorBrush;
            Assert.IsNotNull(brush);
            Assert.AreEqual(opacity, brush.Opacity, 0.001);
            Assert.AreEqual(expectedColor, brush.Color);
        }

        private static string GetHeaderTileAutomationName(HeaderTile tile)
        {
            var rootButton = (Button)tile.FindName("RootButton");
            BindingOperations.GetBindingExpression(rootButton, AutomationProperties.NameProperty)?.UpdateTarget();
            return AutomationProperties.GetName(rootButton);
        }

        private static void RenderElement(FrameworkElement element, Action assert)
        {
            var window = new Window
            {
                Width = 1180,
                Height = 820,
                Left = -32000,
                Top = -32000,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = element
            };

            try
            {
                window.Show();
                WpfTestHost.DoEvents();
                window.UpdateLayout();
                WpfTestHost.DoEvents();
                assert();
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        }

        private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object resourceKey)
        {
            var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
            var dynamicResource = setter.Value as DynamicResourceExtension;

            Assert.IsNotNull(dynamicResource);
            Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
        }

        private static void AssertBindingPath(DependencyObject target, DependencyProperty property, string expectedPath)
        {
            var bindingExpression = BindingOperations.GetBindingExpression(target, property);
            Assert.IsNotNull(bindingExpression);
            Assert.AreEqual(expectedPath, bindingExpression.ParentBinding.Path?.Path ?? string.Empty);
        }

        private static void AssertUserDashboardImageBrushResources(Application app)
        {
            var expectedImageIds = new[]
            {
                "64",
                "65",
                "91",
                "103",
                "177",
                "334",
                "338",
                "342",
                "349",
                "366",
                "367",
                "373",
                "375",
                "378",
                "399",
                "447",
                "453",
                "473",
                "469",
                "505"
            };

            foreach (var imageId in expectedImageIds)
            {
                var brush = app.FindResource("p" + imageId) as ImageBrush;
                Assert.IsNotNull(brush, "p" + imageId);
                StringAssert.Contains(
                    brush.ImageSource.ToString(),
                    "ModernWpf.Gallery;component/Assets/UserDashboard/" + imageId + "-100x100.jpg");
            }
        }

        private static void AssertStyleSetter(Style style, DependencyProperty property, object value)
        {
            var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
            Assert.AreEqual(value, setter.Value);
        }

        private static void AssertNoStyleSetter(Style style, DependencyProperty property)
        {
            Assert.IsFalse(
                style.Setters.OfType<Setter>().Any(item => item.Property == property),
                "Did not expect a setter for " + property.Name);
        }

        private static string GetGalleryComponentRelativePath(string imagePath)
        {
            const string prefix = "pack://application:,,,/ModernWpf.Gallery;component/";

            Assert.IsTrue(
                imagePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase),
                "Expected '{0}' to start with '{1}'.",
                imagePath,
                prefix);

            return imagePath.Substring(prefix.Length);
        }

        private static void AssertGalleryResourceExists(string relativePath)
        {
            var resource = Application.GetResourceStream(new Uri(
                "pack://application:,,,/ModernWpf.Gallery;component/" + relativePath,
                UriKind.Absolute));

            Assert.IsNotNull(resource, relativePath);
            resource.Stream.Dispose();
        }
    }
}
