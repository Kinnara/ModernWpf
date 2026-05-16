using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
            var iconPresenter = FindNamedDescendant<ContentPresenterEx>(teachingTip, "IconPresenter");
            Assert.AreEqual(Visibility.Visible, iconPresenter.Visibility);
            Assert.AreSame(teachingTip.TemplateSettings.IconElement, iconPresenter.Content);
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
            WaitFor(() => events.Contains("Closed: CloseButton"), "TeachingTip close animation did not complete.");

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
            WaitFor(() => events.Contains("Closed: Programmatic"), "TeachingTip deferred close animation did not complete.");

            CollectionAssert.AreEqual(new[] { "Closing: Programmatic", "Closed: Programmatic" }, events);
            Assert.IsFalse(teachingTip.IsOpen);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<FrameworkElement>(teachingTip, "Container").Visibility);
        });
    }

    [TestMethod]
    public void TeachingTipUsesWinUIScaleAnimationTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Width = 80, Height = 24, Content = "Target" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                PreferredPlacement = TeachingTipPlacementMode.Bottom,
                Content = "Targeted tip"
            };
            var root = new StackPanel
            {
                Children =
                {
                    target,
                    teachingTip
                }
            };

            using var host = new TestWindowHost(root, width: 360, height: 240);
            var popup = FindNamedDescendant<Popup>(teachingTip, "Popup");
            var tailOcclusionGrid = FindNamedDescendant<Grid>(teachingTip, "TailOcclusionGrid");
            var scaleTransform = tailOcclusionGrid.RenderTransform as ScaleTransform;

            Assert.AreEqual(PopupAnimation.None, popup.PopupAnimation);
            Assert.IsNotNull(scaleTransform, "TeachingTip should animate the tip scale like WinUI instead of using PopupAnimation.");
            Assert.AreEqual(Math.Max(tailOcclusionGrid.ActualWidth, tailOcclusionGrid.MinWidth) / 2.0, scaleTransform!.CenterX, 0.5);
            Assert.AreEqual(8.0, scaleTransform.CenterY, 0.5);
        });
    }

    [TestMethod]
    public void TeachingTipContentMarginFollowsContentState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Width = 80, Height = 24, Content = "Target" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                Title = "This is the title",
                Subtitle = "And this is the subtitle"
            };
            var root = new StackPanel
            {
                Children =
                {
                    target,
                    teachingTip
                }
            };

            using var host = new TestWindowHost(root, width: 360, height: 240);
            var mainContentPresenter = FindNamedDescendant<ContentPresenterEx>(teachingTip, "MainContentPresenter");

            Assert.AreEqual(new Thickness(0), mainContentPresenter.Margin);

            teachingTip.Content = "Details";
            host.UpdateLayout();

            Assert.AreEqual(new Thickness(0, 12, 0, 0), mainContentPresenter.Margin);
        });
    }

    [TestMethod]
    public void TeachingTipTemplateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Width = 80, Height = 24, Content = "Target" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                Title = "Title",
                Subtitle = "Subtitle",
                Content = "Details",
                HeroContent = new Border { Height = 24 },
                HeroContentPlacement = TeachingTipHeroContentPlacementMode.Bottom,
                IconSource = new SymbolIconSource { Symbol = Symbol.People },
                ActionButtonContent = "Action",
                CloseButtonContent = "Close",
                PreferredPlacement = TeachingTipPlacementMode.TopRight,
                TailVisibility = TeachingTipTailVisibility.Visible,
                IsLightDismissEnabled = true,
                CornerRadius = new CornerRadius(2, 4, 6, 8)
            };
            var root = new StackPanel
            {
                Children =
                {
                    target,
                    teachingTip
                }
            };

            using var host = new TestWindowHost(root, width: 420, height: 260);
            var layoutRoot = FindNamedDescendant<Grid>(teachingTip, "LayoutRoot");

            AssertStateSetter(layoutRoot, "LightDismissStates", "LightDismiss",
                "TailPolygon.Fill",
                "ContentRootGrid.Background",
                "MainContentPresenter.Background",
                "HeroContentBorder.Background");
            AssertStateSetter(layoutRoot, "ButtonsStates", "NoButtonsVisible",
                "CloseButton.Visibility",
                "ActionButton.Visibility");
            AssertStateSetter(layoutRoot, "ButtonsStates", "ActionButtonVisible",
                "CloseButton.Visibility",
                "ActionButton.Visibility",
                "ActionButton.(Grid.ColumnSpan)",
                "ActionButton.Margin");
            AssertStateSetter(layoutRoot, "ButtonsStates", "CloseButtonVisible",
                "CloseButton.Visibility",
                "CloseButton.Margin",
                "CloseButton.(Grid.Column)",
                "CloseButton.(Grid.ColumnSpan)",
                "ActionButton.Visibility");
            AssertStateSetter(layoutRoot, "ButtonsStates", "BothButtonsVisible",
                "CloseButton.Visibility",
                "CloseButton.Margin",
                "ActionButton.Visibility",
                "ActionButton.(Grid.Column)",
                "ActionButton.Margin");
            AssertStateSetter(layoutRoot, "ContentStates", "Content", "MainContentPresenter.Margin");
            AssertStateSetter(layoutRoot, "ContentStates", "NoContent", "MainContentPresenter.Margin");
            AssertStateSetter(layoutRoot, "CloseButtonLocations", "HeaderCloseButton",
                "TitlesStackPanel.Margin",
                "AlternateCloseButton.Visibility");
            AssertStateSetter(layoutRoot, "CloseButtonLocations", "FooterCloseButton",
                "TitlesStackPanel.Margin",
                "AlternateCloseButton.Visibility");
            AssertStateSetter(layoutRoot, "IconStates", "Icon",
                "IconPresenter.Visibility",
                "IconPresenter.Margin");
            AssertStateSetter(layoutRoot, "IconStates", "NoIcon",
                "IconPresenter.Visibility",
                "IconPresenter.Margin");
            AssertStateSetter(layoutRoot, "HeroContentPlacementStates", "HeroContentTop",
                "HeroContentBorder.(Grid.Row)",
                "HeroContentBorder.CornerRadius");
            AssertStateSetter(layoutRoot, "HeroContentPlacementStates", "HeroContentBottom",
                "HeroContentBorder.(Grid.Row)",
                "HeroContentBorder.CornerRadius");

            foreach (var placementState in new[]
            {
                "Top",
                "Bottom",
                "Left",
                "Right",
                "TopRight",
                "TopLeft",
                "BottomRight",
                "BottomLeft",
                "LeftTop",
                "LeftBottom",
                "RightTop",
                "RightBottom",
                "Center"
            })
            {
                AssertStateSetter(layoutRoot, "PlacementStates", placementState,
                    "TailPolygon.Visibility",
                    "TailPolygon.Points",
                    "TailPolygon.(Grid.Row)",
                    "TailPolygon.(Grid.Column)",
                    "TailPolygon.HorizontalAlignment",
                    "TailPolygon.VerticalAlignment",
                    "TailPolygon.Margin");
            }

            AssertStateSetter(layoutRoot, "PlacementStates", "Untargeted", "TailPolygon.Visibility");
            AssertStateSetter(layoutRoot, "TitleBlockStates", "ShowTitleTextBlock", "TitleTextBlock.Visibility");
            AssertStateSetter(layoutRoot, "SubtitleBlockStates", "ShowSubtitleTextBlock", "SubtitleTextBlock.Visibility");

            AssertCurrentState(layoutRoot, "LightDismissStates", "LightDismiss");
            AssertCurrentState(layoutRoot, "ButtonsStates", "BothButtonsVisible");
            AssertCurrentState(layoutRoot, "ContentStates", "Content");
            AssertCurrentState(layoutRoot, "CloseButtonLocations", "FooterCloseButton");
            AssertCurrentState(layoutRoot, "IconStates", "Icon");
            AssertCurrentState(layoutRoot, "HeroContentPlacementStates", "HeroContentBottom");
            AssertCurrentState(layoutRoot, "PlacementStates", "TopRight");
            AssertCurrentState(layoutRoot, "TitleBlockStates", "ShowTitleTextBlock");
            AssertCurrentState(layoutRoot, "SubtitleBlockStates", "ShowSubtitleTextBlock");

            var actionButton = FindNamedDescendant<Button>(teachingTip, "ActionButton");
            var closeButton = FindNamedDescendant<Button>(teachingTip, "CloseButton");
            var alternateCloseButton = FindNamedDescendant<Button>(teachingTip, "AlternateCloseButton");
            var mainContentPresenter = FindNamedDescendant<ContentPresenterEx>(teachingTip, "MainContentPresenter");
            var heroContentBorder = FindNamedDescendant<Border>(teachingTip, "HeroContentBorder");
            var iconPresenter = FindNamedDescendant<ContentPresenterEx>(teachingTip, "IconPresenter");
            var tail = FindNamedDescendant<Polygon>(teachingTip, "TailPolygon");

            Assert.AreEqual(Visibility.Visible, actionButton.Visibility);
            Assert.AreEqual(Visibility.Visible, closeButton.Visibility);
            Assert.AreEqual(Visibility.Collapsed, alternateCloseButton.Visibility);
            Assert.AreEqual(new Thickness(0, 12, 0, 0), mainContentPresenter.Margin);
            Assert.AreEqual(2, Grid.GetRow(heroContentBorder));
            Assert.AreEqual(new CornerRadius(0, 0, 6, 8), heroContentBorder.CornerRadius);
            Assert.AreEqual(Visibility.Visible, iconPresenter.Visibility);
            Assert.AreEqual(Visibility.Visible, tail.Visibility);
            Assert.AreEqual(HorizontalAlignment.Left, tail.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, tail.VerticalAlignment);

            teachingTip.Title = string.Empty;
            teachingTip.Subtitle = string.Empty;
            teachingTip.Content = null;
            teachingTip.HeroContentPlacement = TeachingTipHeroContentPlacementMode.Top;
            teachingTip.IconSource = null;
            teachingTip.ActionButtonContent = null;
            teachingTip.CloseButtonContent = null;
            teachingTip.PreferredPlacement = TeachingTipPlacementMode.BottomLeft;
            teachingTip.IsLightDismissEnabled = false;
            host.UpdateLayout();

            AssertCurrentState(layoutRoot, "LightDismissStates", "NormalDismiss");
            AssertCurrentState(layoutRoot, "ButtonsStates", "NoButtonsVisible");
            AssertCurrentState(layoutRoot, "ContentStates", "NoContent");
            AssertCurrentState(layoutRoot, "CloseButtonLocations", "HeaderCloseButton");
            AssertCurrentState(layoutRoot, "IconStates", "NoIcon");
            AssertCurrentState(layoutRoot, "HeroContentPlacementStates", "HeroContentTop");
            AssertCurrentState(layoutRoot, "PlacementStates", "BottomLeft");
            AssertCurrentState(layoutRoot, "TitleBlockStates", "CollapseTitleTextBlock");
            AssertCurrentState(layoutRoot, "SubtitleBlockStates", "CollapseSubtitleTextBlock");

            Assert.AreEqual(Visibility.Collapsed, actionButton.Visibility);
            Assert.AreEqual(Visibility.Collapsed, closeButton.Visibility);
            Assert.AreEqual(Visibility.Visible, alternateCloseButton.Visibility);
            Assert.AreEqual(new Thickness(), mainContentPresenter.Margin);
            Assert.AreEqual(0, Grid.GetRow(heroContentBorder));
            Assert.AreEqual(new CornerRadius(2, 4, 0, 0), heroContentBorder.CornerRadius);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);
            Assert.AreEqual(HorizontalAlignment.Right, tail.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, tail.VerticalAlignment);
        });
    }

    [TestMethod]
    public void TeachingTipExpandAnimationStartsAtWinUIMinimumScale()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Width = 80, Height = 24, Content = "Target" };
            var teachingTip = new TeachingTipControl
            {
                IsOpen = true,
                Target = target,
                PreferredPlacement = TeachingTipPlacementMode.Bottom,
                Content = "Targeted tip"
            };
            var root = new StackPanel
            {
                Children =
                {
                    target,
                    teachingTip
                }
            };

            using var host = new TestWindowHost(root, width: 360, height: 240);
            var tailOcclusionGrid = FindNamedDescendant<Grid>(teachingTip, "TailOcclusionGrid");
            var method = typeof(TeachingTipControl).GetMethod("GetExpandStartScale", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);

            var startScale = (Size)method!.Invoke(teachingTip, null)!;

            Assert.AreEqual(Math.Min(0.01, 20.0 / tailOcclusionGrid.ActualWidth), startScale.Width, 0.005);
            Assert.AreEqual(Math.Min(0.01, 20.0 / tailOcclusionGrid.ActualHeight), startScale.Height, 0.005);
            Assert.AreEqual(0.01, startScale.Width, 0.005, "WinUI starts the expand scale from its minimum expression value.");
        });
    }

    [TestMethod]
    public void TeachingTipAutoPlacementUsesWinUIPriorities()
    {
        WpfTestHost.Run(() =>
        {
            var method = typeof(TeachingTipControl).GetMethod("GetEffectivePlacement", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);

            var target = new Button { Content = "Target" };
            var targetedTeachingTip = new TeachingTipControl
            {
                Target = target
            };
            var untargetedTeachingTip = new TeachingTipControl();

            Assert.AreEqual(
                TeachingTipPlacementMode.Top,
                (TeachingTipPlacementMode)method!.Invoke(targetedTeachingTip, null)!,
                "WinUI targeted Auto placement starts with Top when it fits.");
            Assert.AreEqual(
                TeachingTipPlacementMode.Bottom,
                (TeachingTipPlacementMode)method.Invoke(untargetedTeachingTip, null)!,
                "WinUI untargeted Auto placement starts with Bottom when it fits.");
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
            WaitFor(() => !popup.IsOpen && closeReason == TeachingTipCloseReason.LightDismiss, "TeachingTip light-dismiss close animation did not complete.");

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
            WaitFor(() => !popup.IsOpen && closeReason == TeachingTipCloseReason.Programmatic, "TeachingTip target-unload close animation did not complete.");

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

    private static VisualStateEx AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = FindVisualState(group, stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (var expectedTarget in expectedTargets)
        {
            var found = false;
            foreach (var setter in stateEx.Setters)
            {
                if (setter.Target == expectedTarget)
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, $"{groupName}.{stateName} is missing setter target '{expectedTarget}'.");
        }

        return stateEx;
    }

    private static void AssertCurrentState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        Assert.AreEqual(expectedStateName, FindVisualStateGroup(stateGroupsRoot, groupName).CurrentState?.Name);
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(stateGroupsRoot))
        {
            if (group.Name == groupName)
            {
                return group;
            }
        }

        throw new InvalidOperationException($"Could not find visual state group '{groupName}'.");
    }

    private static VisualState FindVisualState(VisualStateGroup group, string stateName)
    {
        foreach (VisualState state in group.States)
        {
            if (state.Name == stateName)
            {
                return state;
            }
        }

        throw new InvalidOperationException($"Could not find visual state '{group.Name}.{stateName}'.");
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

    private static void WaitFor(Func<bool> predicate, string failureMessage, int timeoutMilliseconds = 1500)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);

        while (!predicate() && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
            WpfTestHost.DoEvents();
        }

        Assert.IsTrue(predicate(), failureMessage);
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
