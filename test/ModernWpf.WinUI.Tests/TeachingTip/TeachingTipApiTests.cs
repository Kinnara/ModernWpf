using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using TeachingTipControl = ModernWpf.Controls.TeachingTip;

namespace ModernWpf.WinUI.Tests.TeachingTip;

[TestClass]
public class TeachingTipApiTests
{
    [TestMethod]
    public void TeachingTipDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var teachingTip = new TeachingTipControl();

            Assert.AreEqual(string.Empty, teachingTip.Title);
            Assert.AreEqual(string.Empty, teachingTip.Subtitle);
            Assert.IsFalse(teachingTip.IsOpen);
            Assert.IsNull(teachingTip.Target);
            Assert.AreEqual(TeachingTipTailVisibility.Auto, teachingTip.TailVisibility);
            Assert.IsNull(teachingTip.ActionButtonContent);
            Assert.IsNull(teachingTip.ActionButtonStyle);
            Assert.IsNull(teachingTip.ActionButtonCommand);
            Assert.IsNull(teachingTip.ActionButtonCommandParameter);
            Assert.IsNull(teachingTip.CloseButtonContent);
            Assert.IsNull(teachingTip.CloseButtonStyle);
            Assert.IsNull(teachingTip.CloseButtonCommand);
            Assert.IsNull(teachingTip.CloseButtonCommandParameter);
            Assert.AreEqual(default(Thickness), teachingTip.PlacementMargin);
            Assert.IsTrue(teachingTip.ShouldConstrainToRootBounds);
            Assert.IsFalse(teachingTip.IsLightDismissEnabled);
            Assert.AreEqual(TeachingTipPlacementMode.Auto, teachingTip.PreferredPlacement);
            Assert.AreEqual(TeachingTipHeroContentPlacementMode.Auto, teachingTip.HeroContentPlacement);
            Assert.IsNull(teachingTip.HeroContent);
            Assert.IsNull(teachingTip.IconSource);
            Assert.IsNotNull(teachingTip.TemplateSettings);
        });
    }

    [TestMethod]
    public void TeachingTipContentHeroAndIconDoNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var hero = new Border { Height = 24, Background = Brushes.CornflowerBlue };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Content = new TextBlock { Text = "Some text" },
                HeroContent = hero,
                IconSource = new SymbolIconSource { Symbol = Symbol.People }
            };

            using var host = new TestWindowHost(teachingTip, width: 420, height: 220);

            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<FrameworkElement>(teachingTip, "Container").Visibility);
            Assert.AreSame(hero, teachingTip.HeroContent);
            Assert.IsInstanceOfType(teachingTip.TemplateSettings.IconElement, typeof(SymbolIcon));
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<FrameworkElement>(teachingTip, "HeroContentBorder").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<FrameworkElement>(teachingTip, "IconPresenter").Visibility);
        });
    }

    [TestMethod]
    public void TitleAndSubtitleAreCollapsedWhenUnset()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var teachingTip = new TeachingTipControl { IsOpen = true };
            using var host = new TestWindowHost(teachingTip, width: 420, height: 180);

            Assert.AreEqual(string.Empty, teachingTip.Title);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<TextBlock>(teachingTip, "TitleTextBlock").Visibility);
            Assert.AreEqual(string.Empty, teachingTip.Subtitle);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<TextBlock>(teachingTip, "SubtitleTextBlock").Visibility);

            teachingTip.Title = "New feature";
            teachingTip.Subtitle = "Use this control to guide users.";
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<TextBlock>(teachingTip, "TitleTextBlock").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<TextBlock>(teachingTip, "SubtitleTextBlock").Visibility);
        });
    }

    [TestMethod]
    public void ContentInheritsTypography()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var foreground = new SolidColorBrush(Colors.Red);
            var content = new TextBlock { Text = "Some text" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Content = content,
                FontSize = 22,
                Foreground = foreground
            };

            using var host = new TestWindowHost(teachingTip, width: 420, height: 180);

            Assert.IsTrue(Math.Abs(22 - content.FontSize) < 1);
            Assert.AreSame(foreground, content.Foreground);
        });
    }

    [TestMethod]
    public void HeroContentPlacementUpdatesTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (TeachingTipHeroContentPlacementMode placement in Enum.GetValues(typeof(TeachingTipHeroContentPlacementMode)))
            {
                var teachingTip = new TeachingTipControl
                {
                    IsOpen = true,
                    HeroContent = new Border { Height = 16 },
                    HeroContentPlacement = placement
                };

                using var host = new TestWindowHost(teachingTip, width: 420, height: 180);
                var heroContentBorder = FindNamedDescendant<FrameworkElement>(teachingTip, "HeroContentBorder");

                Assert.AreEqual(
                    placement == TeachingTipHeroContentPlacementMode.Bottom ? 2 : 0,
                    Grid.GetRow(heroContentBorder),
                    placement.ToString());
            }
        });
    }

    [TestMethod]
    public void ActionAndCloseEventsFollowWinUIContract()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                ActionButtonContent = "Try it",
                ActionButtonCommand = new RecordingCommand(),
                ActionButtonCommandParameter = "action",
                CloseButtonContent = "Close",
                CloseButtonCommand = new RecordingCommand(),
                CloseButtonCommandParameter = "close"
            };
            var actionCommand = (RecordingCommand)teachingTip.ActionButtonCommand;
            var closeCommand = (RecordingCommand)teachingTip.CloseButtonCommand;
            var events = new List<string>();
            var cancelClose = false;

            teachingTip.ActionButtonClick += (_, _) => events.Add("ActionButtonClick");
            teachingTip.CloseButtonClick += (_, _) => events.Add("CloseButtonClick");
            teachingTip.Closing += (_, args) =>
            {
                events.Add("Closing: " + args.Reason);
                args.Cancel = cancelClose;
            };
            teachingTip.Closed += (_, args) => events.Add("Closed: " + args.Reason);

            using var host = new TestWindowHost(teachingTip, width: 420, height: 220);
            var actionButton = FindNamedDescendant<Button>(teachingTip, "ActionButton");
            var closeButton = FindNamedDescendant<Button>(teachingTip, "CloseButton");

            actionButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "ActionButtonClick", "CloseButtonClick", "Closing: CloseButton", "Closed: CloseButton" },
                events);
            Assert.AreEqual(1, actionCommand.ExecuteCount);
            Assert.AreEqual("action", actionCommand.LastParameter);
            Assert.AreEqual(1, closeCommand.ExecuteCount);
            Assert.AreEqual("close", closeCommand.LastParameter);
            Assert.IsFalse(teachingTip.IsOpen);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<FrameworkElement>(teachingTip, "Container").Visibility);

            teachingTip.IsOpen = true;
            cancelClose = true;
            events.Clear();

            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "CloseButtonClick", "Closing: CloseButton" },
                events);
            Assert.AreEqual(2, closeCommand.ExecuteCount);
            Assert.IsTrue(teachingTip.IsOpen);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<FrameworkElement>(teachingTip, "Container").Visibility);
        });
    }

    [TestMethod]
    public void ClosingCanBeDeferred()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var teachingTip = new TeachingTipControl { IsOpen = true };
            TeachingTipClosingDeferral? deferral = null;
            var events = new List<string>();

            teachingTip.Closing += (_, args) =>
            {
                events.Add("Closing: " + args.Reason);
                deferral = args.GetDeferral();
            };
            teachingTip.Closed += (_, args) => events.Add("Closed: " + args.Reason);

            using var host = new TestWindowHost(teachingTip, width: 420, height: 180);
            teachingTip.IsOpen = false;

            CollectionAssert.AreEqual(new[] { "Closing: Programmatic" }, events);
            Assert.IsTrue(teachingTip.IsOpen);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<FrameworkElement>(teachingTip, "Container").Visibility);

            deferral!.Complete();
            host.UpdateLayout();

            CollectionAssert.AreEqual(new[] { "Closing: Programmatic", "Closed: Programmatic" }, events);
            Assert.IsFalse(teachingTip.IsOpen);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<FrameworkElement>(teachingTip, "Container").Visibility);
        });
    }

    [TestMethod]
    public void TailVisibilityFollowsTargetAndPlacement()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                PreferredPlacement = TeachingTipPlacementMode.Bottom
            };

            using var host = new TestWindowHost(new StackPanel { Children = { target, teachingTip } }, width: 420, height: 240);
            var tail = FindNamedDescendant<Polygon>(teachingTip, "TailPolygon");

            Assert.AreEqual(Visibility.Collapsed, tail.Visibility);

            teachingTip.Target = target;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, tail.Visibility);
            Assert.AreEqual(new Point(0, 10), tail.Points[0]);

            teachingTip.TailVisibility = TeachingTipTailVisibility.Collapsed;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, tail.Visibility);
        });
    }

    [TestMethod]
    public void TeachingTipOpensInPopupWithTargetedPlacement()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Width = 80,
                Height = 24,
                Content = "Target",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(120, 16, 0, 0)
            };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                PreferredPlacement = TeachingTipPlacementMode.Bottom,
                PlacementMargin = new Thickness(0, 8, 0, 0),
                Content = "Targeted tip"
            };
            var root = new Grid
            {
                Width = 320,
                Height = 200,
                Children =
                {
                    target,
                    teachingTip
                }
            };

            using var host = new TestWindowHost(root, width: 360, height: 240);
            var popup = FindNamedDescendant<Popup>(teachingTip, "Popup");
            var placements = popup.CustomPopupPlacementCallback(new Size(140, 80), new Size(80, 24), default);

            Assert.IsTrue(popup.IsOpen);
            Assert.AreSame(target, popup.PlacementTarget);
            Assert.AreEqual(PlacementMode.Custom, popup.Placement);
            Assert.IsNotNull(popup.Child);
            Assert.AreEqual(8 + 24, placements[0].Point.Y);
        });
    }

    [TestMethod]
    public void TeachingTipPlacementFallsBackInsideRootBounds()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new Grid
            {
                Width = 320,
                Height = 160
            };
            var target = new Button
            {
                Width = 80,
                Height = 24,
                Content = "Target",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(120, 0, 0, 4)
            };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                PreferredPlacement = TeachingTipPlacementMode.Bottom,
                Content = "Targeted tip"
            };

            root.Children.Add(target);
            root.Children.Add(teachingTip);

            using var host = new TestWindowHost(root, width: 360, height: 220);
            var popup = FindNamedDescendant<Popup>(teachingTip, "Popup");
            var placements = popup.CustomPopupPlacementCallback(new Size(140, 80), new Size(80, 24), default);

            Assert.IsTrue(placements[0].Point.Y < 0, "Bottom placement should fall back above the target when it would exceed the root bounds.");
        });
    }

    [TestMethod]
    public void TeachingTipLightDismissClosesWithReason()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Width = 80, Height = 24, Content = "Target" };
            var outside = new Button { Width = 80, Height = 24, Content = "Outside" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                IsLightDismissEnabled = true,
                Target = target,
                Content = "Targeted tip"
            };
            var root = new StackPanel
            {
                Children =
                {
                    target,
                    outside,
                    teachingTip
                }
            };
            TeachingTipCloseReason? closeReason = null;
            teachingTip.Closed += (_, args) => closeReason = args.Reason;

            using var host = new TestWindowHost(root, width: 360, height: 240);
            var popup = FindNamedDescendant<Popup>(teachingTip, "Popup");

            outside.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseDownEvent,
                Source = outside
            });
            host.UpdateLayout();

            Assert.IsFalse(teachingTip.IsOpen);
            Assert.IsFalse(popup.IsOpen);
            Assert.AreEqual(TeachingTipCloseReason.LightDismiss, closeReason);
        });
    }

    [TestMethod]
    public void TeachingTipClosesWhenTargetUnloads()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new StackPanel();
            var target = new Button { Width = 80, Height = 24, Content = "Target" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                Content = "Targeted tip"
            };
            TeachingTipCloseReason? closeReason = null;
            teachingTip.Closed += (_, args) => closeReason = args.Reason;

            root.Children.Add(target);
            root.Children.Add(teachingTip);

            using var host = new TestWindowHost(root, width: 360, height: 240);
            var popup = FindNamedDescendant<Popup>(teachingTip, "Popup");
            Assert.IsTrue(popup.IsOpen);

            root.Children.Remove(target);
            host.UpdateLayout();

            Assert.IsFalse(teachingTip.IsOpen);
            Assert.IsFalse(popup.IsOpen);
            Assert.AreEqual(TeachingTipCloseReason.Programmatic, closeReason);
        });
    }

    [TestMethod]
    public void FinalWinUI2TeachingTipThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/TeachingTip/TeachingTip.xaml", UriKind.Relative)
            };

            AssertResource(resources, "TeachingTipMinHeight", 40.0);
            AssertResource(resources, "TeachingTipMaxHeight", 520.0);
            AssertResource(resources, "TeachingTipMinWidth", 320.0);
            AssertResource(resources, "TeachingTipMaxWidth", 336.0);
            AssertResource(resources, "TeachingTipButtonPanelMargin", new Thickness(0, 12, 0, 0));
            AssertResource(resources, "TeachingTipRightButtonMargin", new Thickness(4, 12, 0, 0));
            AssertResource(resources, "TeachingTipLeftButtonMargin", new Thickness(0, 12, 4, 0));
            AssertResource(resources, "TeachingTipTailShortSideLength", new GridLength(8));
            AssertResource(resources, "TeachingTipTailMargin", new GridLength(10));
            AssertResource(resources, "TeachingTipAlternateCloseButtonSize", 40.0);
            AssertResource(resources, "TeachingTipAlternateCloseButtonGlyphSize", 16.0);
            AssertResource(resources, "TeachingTipContentMargin", new Thickness(12));
            AssertResource(resources, "TeachingTipTopHighlightHeight", 1.0);
            AssertResource(resources, "TeachingTipBorderThickness", 1.0);

            AssertThemeResourceReference("Light", "TeachingTipBorderBrush", "SurfaceStrokeColorDefaultBrush");
            AssertThemeResourceReference("Light", "TeachingTipTransientBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Light", "TeachingTipForegroundBrush", "TextFillColorPrimaryBrush");
            AssertThemeBrushColor("Light", "TeachingTipTopHighlightBrush", Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));

            AssertThemeResourceReference("Dark", "TeachingTipBorderBrush", "SurfaceStrokeColorDefaultBrush");
            AssertThemeResourceReference("Dark", "TeachingTipTransientBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "TeachingTipForegroundBrush", "TextFillColorPrimaryBrush");
            AssertThemeBrushColor("Dark", "TeachingTipTopHighlightBrush", Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF));

            AssertThemeResourceReference("HighContrast", "TeachingTipBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TeachingTipTransientBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("HighContrast", "TeachingTipForegroundBrush", "SystemColorWindowTextColorBrush");
            AssertThemeBrushColor("HighContrast", "TeachingTipTopHighlightBrush", Colors.Transparent);
        });
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected value for '{key}'.");
    }

    private static void AssertThemeBrushColor(string themeName, string resourceKey, Color expectedColor)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");

        var brush = themeDictionary[resourceKey] as SolidColorBrush;
        Assert.IsNotNull(brush, $"{themeName}:{resourceKey} should be a SolidColorBrush.");
        Assert.AreEqual(expectedColor, brush!.Color, $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in EnumerateDescendantsIncludingPopupChildren(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private static IEnumerable<DependencyObject> EnumerateDescendantsIncludingPopupChildren(DependencyObject root)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            yield return descendant;

            if (descendant is Popup { Child: { } child })
            {
                yield return child;

                foreach (var popupChildDescendant in EnumerateDescendantsIncludingPopupChildren(child))
                {
                    yield return popupChildDescendant;
                }
            }
        }
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }
    }
}
