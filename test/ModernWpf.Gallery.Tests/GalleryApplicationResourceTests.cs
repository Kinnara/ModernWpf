using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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

                AssertSolidColorBrush(app, "SystemFillColorAttentionBrush", Color.FromRgb(0x00, 0x78, 0xD4));
                AssertSolidColorBrush(app, "SurfaceStrokeColorDefaultBrush", Color.FromArgb(0x66, 0x75, 0x75, 0x75));
                AssertGalleryControlElevationBorderBrush(app);

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
        public void ColorTileHighContrastTemplateMatchesWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                var colorTileStyle = (Style)app.FindResource(typeof(ColorTile));
                Assert.IsNotNull(colorTileStyle);

                var templateSetter = colorTileStyle.Setters.OfType<Setter>()
                    .Single(item => item.Property == Control.TemplateProperty);
                var template = (ControlTemplate)templateSetter.Value;
                var highContrastTrigger = template.Triggers.OfType<DataTrigger>()
                    .Single(item =>
                        string.Equals(item.Value?.ToString(), "True", StringComparison.OrdinalIgnoreCase) &&
                        HasSystemParametersHighContrastBinding(item));

                AssertDynamicResourceTriggerSetter(highContrastTrigger, "ColorExplanationTextBlock", "Foreground", "SystemColorWindowTextColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "ColorExplanationTextBlock", "Background", "SystemColorWindowColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "ColorBrushNameTextBlock", "Foreground", "SystemColorWindowTextColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "ColorBrushNameTextBlock", "Background", "SystemColorWindowColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "ColorNameTextBlock", "Foreground", "SystemColorWindowTextColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "ColorNameTextBlock", "Background", "SystemColorWindowColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "CopyBrushNameButton", "Foreground", "SystemColorWindowTextColorBrush");
                AssertDynamicResourceTriggerSetter(highContrastTrigger, "CopyBrushNameButton", "Background", "SystemColorWindowColorBrush");
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
                Assert.AreEqual(DependencyProperty.UnsetValue, image.ReadLocalValue(Image.StretchProperty));
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
                    var tileGallery = FindVisualChildren<TileGallery>(page).Single();
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
        public void HomeHeaderTilesUseWpfGalleryHighContrastFillResources()
        {
            WpfTestHost.Run(() =>
            {
                var tile = new HeaderTile();
                var rootButton = (Button)tile.FindName("RootButton");

                tile.ApplyButtonResources(true);

                Assert.AreSame(SystemColors.ControlBrush, rootButton.Resources["ButtonBackground"]);
                Assert.AreSame(SystemColors.ControlBrush, rootButton.Resources["ButtonBackgroundPointerOver"]);
                Assert.AreSame(SystemColors.ControlBrush, rootButton.Resources["ButtonBackgroundPressed"]);
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

                    var templateRoot = (DependencyObject)VisualTreeHelper.GetChild(controlExample, 0);
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(templateRoot));

                    Assert.IsNull(controlExample.Template.FindName("ExampleDisplayBorder", controlExample));
                    var displayBorder = FindVisualChildren<Border>(controlExample)
                        .Single(border =>
                            Grid.GetRow(border) == 1 &&
                            border.Padding == new Thickness(16) &&
                            border.CornerRadius == new CornerRadius(8, 8, 0, 0));
                    Assert.AreEqual(
                        (double)Application.Current.FindResource("BodyTextBlockFontSize"),
                        TextElement.GetFontSize(displayBorder));

                    Assert.IsNull(controlExample.Template.FindName("SourceCodeExpander", controlExample));
                    var sourceCodeExpander = FindVisualChildren<Expander>(controlExample).Single();
                    Assert.AreEqual("Source code", sourceCodeExpander.Header);
                    Assert.AreEqual("View Source Code for Reference sample", AutomationProperties.GetName(sourceCodeExpander));
                    Assert.AreEqual(0.0, sourceCodeExpander.MinHeight);
                    Assert.AreEqual(Visibility.Visible, sourceCodeExpander.Visibility);
                    Assert.IsTrue(
                        sourceCodeExpander.ActualHeight >= 42.0 && sourceCodeExpander.ActualHeight <= 44.5,
                        "Expected collapsed source expander height near the official WPF Gallery default row; actual " + sourceCodeExpander.ActualHeight);

                    var sourceHeaderToggle = FindVisualChildren<System.Windows.Controls.Primitives.ToggleButton>(sourceCodeExpander).Single();
                    Assert.IsTrue(sourceHeaderToggle.Focusable);
                    Assert.AreEqual(
                        "View Source Code for Reference sample",
                        AutomationProperties.GetName(sourceHeaderToggle));
                    var sourceHeaderPeer = UIElementAutomationPeer.CreatePeerForElement(sourceHeaderToggle);
                    Assert.AreEqual("View Source Code for Reference sample", sourceHeaderPeer.GetName());
                    Assert.IsTrue(
                        sourceHeaderToggle.ActualWidth >= displayBorder.ActualWidth - 2.0,
                        "Expected source header to span the official full-width row; actual " + sourceHeaderToggle.ActualWidth + " display " + displayBorder.ActualWidth);

                    var divider = (Border)controlExample.Template.FindName("Border", controlExample);
                    Assert.IsNotNull(divider);
                    Assert.AreEqual(new Thickness(0, 20, 0, 20), divider.Margin);
                    Assert.AreEqual(new Thickness(1), divider.BorderThickness);
                    Assert.IsNull(divider.BorderBrush);
                    Assert.AreEqual(Visibility.Visible, divider.Visibility);

                    sourceCodeExpander.IsExpanded = true;
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var copyButtons = FindVisualChildren<Button>(controlExample)
                        .Where(button => button.Command == ApplicationCommands.Copy)
                        .ToArray();
                    Assert.AreEqual(2, copyButtons.Length);

                    var xamlCopyButton = copyButtons.Single(button => Equals(button.CommandParameter, "Copy_XamlCode"));
                    Assert.AreEqual("Copy XAML Code", AutomationProperties.GetName(xamlCopyButton));
                    Assert.AreEqual("Copy to clipboard", ToolTipService.GetToolTip(xamlCopyButton));

                    var csharpCopyButton = copyButtons.Single(button => Equals(button.CommandParameter, "Copy_CSharpCode"));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetName(csharpCopyButton));
                    Assert.IsNull(ToolTipService.GetToolTip(csharpCopyButton));
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
        public void ControlExampleTemplateKeepsOfficialSourceExpanderWhenCodeIsEmpty()
        {
            WpfTestHost.Run(() =>
            {
                var controlExample = new ControlExample
                {
                    HeaderText = "No source sample",
                    ExampleContent = new Button { Content = "Example" }
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

                    Assert.IsNull(controlExample.Template.FindName("SourceCodeExpander", controlExample));
                    var sourceCodeExpander = FindVisualChildren<Expander>(controlExample).Single();
                    Assert.AreEqual("Source code", sourceCodeExpander.Header);
                    Assert.AreEqual(Visibility.Visible, sourceCodeExpander.Visibility);

                    var xamlCodeBlock = (StackPanel)controlExample.Template.FindName("XamlCodeBlock", controlExample);
                    var csharpCodeBlock = (StackPanel)controlExample.Template.FindName("CSharpCodeBlock", controlExample);
                    var divider = (Border)controlExample.Template.FindName("Border", controlExample);

                    Assert.AreEqual(Visibility.Collapsed, xamlCodeBlock.Visibility);
                    Assert.AreEqual(Visibility.Collapsed, csharpCodeBlock.Visibility);
                    Assert.AreEqual(Visibility.Collapsed, divider.Visibility);
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
                    XamlCodeSource = new Uri("Samples/SampleCode/ContentDialog/ContentDialogSample1_xaml.txt", UriKind.Relative),
                    CSharpCodeSource = new Uri("Samples/SampleCode/ContentDialog/ContentDialogSample1_cs.txt", UriKind.Relative)
                };

                StringAssert.Contains(controlExample.XamlCode, "ContentDialogContent");
                StringAssert.Contains(controlExample.CSharpCode, "ShowAsync");
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

        [TestMethod]
        public void ShippedSampleCodeFilesAreConsumedByRetainedPages()
        {
            WpfTestHost.Run(() =>
            {
                var sampleCodeRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode");
                Assert.IsTrue(Directory.Exists(sampleCodeRoot), sampleCodeRoot);

                var consumedSampleCode = new HashSet<string>(StringComparer.Ordinal);
                foreach (var item in GalleryCatalog.Items)
                {
                    var page = new ItemPage(item);
                    foreach (var example in page.Examples)
                    {
                        if (!string.IsNullOrEmpty(example.XamlCode))
                        {
                            consumedSampleCode.Add(example.XamlCode);
                        }

                        if (!string.IsNullOrEmpty(example.CSharpCode))
                        {
                            consumedSampleCode.Add(example.CSharpCode);
                        }

                        foreach (var consumedSnippetText in example.ConsumedSnippetTexts)
                        {
                            if (!string.IsNullOrEmpty(consumedSnippetText))
                            {
                                consumedSampleCode.Add(consumedSnippetText);
                            }
                        }
                    }
                }

                var unusedFiles = Directory.GetFiles(sampleCodeRoot, "*.txt", SearchOption.AllDirectories)
                    .Where(path => !consumedSampleCode.Contains(File.ReadAllText(path)))
                    .Select(path => path.Substring(sampleCodeRoot.Length + 1))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                Assert.AreEqual(
                    0,
                    unusedFiles.Length,
                    "Unused sample-code files should not be shipped for deleted or hidden gallery pages: " + string.Join(", ", unusedFiles));
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

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object resourceKey)
        {
            var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
            var dynamicResource = setter.Value as DynamicResourceExtension;

            Assert.IsNotNull(dynamicResource);
            Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
        }

        private static bool HasSystemParametersHighContrastBinding(DataTrigger trigger)
        {
            var binding = trigger.Binding as Binding;
            var path = binding?.Path?.Path;
            return string.Equals(path, "HighContrast", StringComparison.Ordinal) ||
                string.Equals(path, "SystemParameters.HighContrast", StringComparison.Ordinal) ||
                string.Equals(path, "(SystemParameters.HighContrast)", StringComparison.Ordinal) ||
                string.Equals(path, "(System.Windows.SystemParameters.HighContrast)", StringComparison.Ordinal) ||
                string.Equals(path, "(0)", StringComparison.Ordinal);
        }

        private static void AssertDynamicResourceTriggerSetter(
            DataTrigger trigger,
            string targetName,
            string propertyName,
            object resourceKey)
        {
            var setter = trigger.Setters.OfType<Setter>().Single(item =>
                string.Equals(item.TargetName, targetName, StringComparison.Ordinal) &&
                string.Equals(item.Property.Name, propertyName, StringComparison.Ordinal));
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

        private static void AssertSolidColorBrush(Application app, string resourceKey, Color expectedColor)
        {
            var brush = app.FindResource(resourceKey) as SolidColorBrush;
            Assert.IsNotNull(brush, resourceKey);
            Assert.AreEqual(expectedColor, brush.Color, resourceKey);
        }

        private static void AssertGalleryControlElevationBorderBrush(Application app)
        {
            var originalTheme = ThemeManager.Current.ApplicationTheme;

            try
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                WpfTestHost.DoEvents();
                AssertControlElevationBorderBrush(
                    app,
                    Color.FromArgb(0x29, 0x00, 0x00, 0x00),
                    Color.FromArgb(0x0F, 0x00, 0x00, 0x00));
            }
            finally
            {
                ThemeManager.Current.ApplicationTheme = originalTheme;
            }
        }

        private static void AssertControlElevationBorderBrush(
            Application app,
            Color expectedSecondary,
            Color expectedDefault)
        {
            var brush = app.FindResource("ControlElevationBorderBrush") as LinearGradientBrush;
            Assert.IsNotNull(brush);
            Assert.AreEqual(BrushMappingMode.Absolute, brush.MappingMode);
            Assert.AreEqual(new Point(0, 0), brush.StartPoint);
            Assert.AreEqual(new Point(0, 3), brush.EndPoint);
            Assert.IsFalse(brush.RelativeTransform is ScaleTransform);
            Assert.AreEqual(2, brush.GradientStops.Count);

            Assert.AreEqual(0.33, brush.GradientStops[0].Offset, 0.001);
            Assert.AreEqual(expectedSecondary, brush.GradientStops[0].Color);
            Assert.AreEqual(1.0, brush.GradientStops[1].Offset, 0.001);
            Assert.AreEqual(expectedDefault, brush.GradientStops[1].Color);
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
