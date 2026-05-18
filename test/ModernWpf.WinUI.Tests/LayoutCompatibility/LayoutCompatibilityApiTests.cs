using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using ModernCanvasEx = ModernWpf.Controls.CanvasEx;
using ModernContentControlEx = ModernWpf.Controls.ContentControlEx;
using ModernGridEx = ModernWpf.Controls.GridEx;
using ModernItemsStackPanel = ModernWpf.Controls.ItemsStackPanel;
using ModernItemsWrapGrid = ModernWpf.Controls.ItemsWrapGrid;
using ModernRelativePanel = ModernWpf.Controls.RelativePanel;
using ModernStackPanelEx = ModernWpf.Controls.StackPanelEx;
using ModernVariableSizedWrapGrid = ModernWpf.Controls.VariableSizedWrapGrid;
using ModernWrapGrid = ModernWpf.Controls.WrapGrid;

namespace ModernWpf.WinUI.Tests.LayoutCompatibility;

[TestClass]
public class LayoutCompatibilityApiTests
{
    [TestMethod]
    public void BorderExAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) },
                ChildTransitions = new ModernWpf.Media.Animation.TransitionCollection()
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, border.BackgroundSizing);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), border.BackgroundTransition.Duration);
            Assert.IsNotNull(border.ChildTransitions);
        });
    }

    [TestMethod]
    public void LayoutChromeControlsUseBackgroundTransitionBrush()
    {
        WpfTestHost.Run(() =>
        {
            if (!Helper.IsAnimationsEnabled)
            {
                return;
            }

            AssertTransitionBrush(
                new BorderEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(BorderEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernCanvasEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernCanvasEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ContentPresenterEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ContentPresenterEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernGridEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernGridEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernRelativePanel(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernRelativePanel.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernStackPanelEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernStackPanelEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernItemsStackPanel(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernItemsStackPanel.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernVariableSizedWrapGrid(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernVariableSizedWrapGrid.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernWrapGrid(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernWrapGrid.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernItemsWrapGrid(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernItemsWrapGrid.BackgroundTransitionProperty),
                control => control.EffectiveBackground);
        });
    }

    [TestMethod]
    public void CanvasExAcceptsWinUIPanelSurfaceAndAttachedProperties()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var child = new Border
            {
                Width = 12,
                Height = 8,
                Background = Brushes.Red
            };
            var canvas = new ModernCanvasEx
            {
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            canvas.Children.Add(child);
            ModernCanvasEx.SetLeft(child, 13);
            ModernCanvasEx.SetTop(child, 17);
            ModernCanvasEx.SetZIndex(child, 5);

            Assert.AreSame(backgroundTransition, canvas.BackgroundTransition);
            Assert.AreSame(childrenTransitions, canvas.ChildrenTransitions);
            Assert.AreEqual(13, Canvas.GetLeft(child));
            Assert.AreEqual(17, Canvas.GetTop(child));
            Assert.AreEqual(5, Panel.GetZIndex(child));

            canvas.Measure(new Size(100, 100));

            Assert.AreEqual(0, canvas.DesiredSize.Width, 0.1);
            Assert.AreEqual(0, canvas.DesiredSize.Height, 0.1);

            canvas.Arrange(new Rect(0, 0, 100, 100));
            canvas.UpdateLayout();

            var origin = child.TranslatePoint(new Point(), canvas);
            Assert.AreEqual(13, origin.X, 0.1);
            Assert.AreEqual(17, origin.Y, 0.1);
        });
    }

    [TestMethod]
    public void CanvasExParsesWinUIPanelSurfaceXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:CanvasEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    Background="Transparent">
                    <controls:CanvasEx.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:CanvasEx.BackgroundTransition>
                    <controls:CanvasEx.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:CanvasEx.ChildrenTransitions>
                    <Border
                        Width="12"
                        Height="8"
                        Background="Red"
                        controls:CanvasEx.Left="7"
                        controls:CanvasEx.Top="9"
                        controls:CanvasEx.ZIndex="3" />
                </controls:CanvasEx>
                """;

            var canvas = (ModernCanvasEx)XamlReader.Parse(xaml);
            var child = (UIElement)canvas.Children[0];

            Assert.IsNotNull(canvas.BackgroundTransition);
            Assert.IsNotNull(canvas.ChildrenTransitions);
            Assert.AreEqual(7, ModernCanvasEx.GetLeft(child));
            Assert.AreEqual(9, ModernCanvasEx.GetTop(child));
            Assert.AreEqual(3, ModernCanvasEx.GetZIndex(child));
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernVariableSizedWrapGrid
            {
                ItemHeight = 40,
                ItemWidth = 50,
                Orientation = Orientation.Horizontal,
                HorizontalChildrenAlignment = HorizontalAlignment.Center,
                VerticalChildrenAlignment = VerticalAlignment.Bottom,
                MaximumRowsOrColumns = 3,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(40, panel.ItemHeight);
            Assert.AreEqual(50, panel.ItemWidth);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.AreEqual(3, panel.MaximumRowsOrColumns);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridWrapsHorizontallyAndVertically()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateVariableSizedWrapGrid(Orientation.Horizontal, 7);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200)
                });

            var verticalPanel = CreateVariableSizedWrapGrid(Orientation.Vertical, 7);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0)
                });
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridSupportsRowAndColumnSpans()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateVariableSizedWrapGrid(Orientation.Horizontal, 7);

            ModernVariableSizedWrapGrid.SetColumnSpan(panel.Children[0], 2);
            ModernVariableSizedWrapGrid.SetRowSpan(panel.Children[2], 2);

            AssertVariableSizedWrapGridPositions(
                panel,
                new[]
                {
                    new Point(50, 0),
                    new Point(200, 0),
                    new Point(0, 150),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(100, 200),
                    new Point(200, 200)
                });
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridStopsPlacementWhenSourceOccupancyMapIsFull()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateVariableSizedWrapGrid(Orientation.Horizontal, 10);
            horizontalPanel.Measure(new Size(horizontalPanel.Width, horizontalPanel.Height));
            horizontalPanel.Arrange(new Rect(0, 0, horizontalPanel.Width, horizontalPanel.Height));
            horizontalPanel.UpdateLayout();

            Assert.AreEqual(300, horizontalPanel.DesiredSize.Width, 0.1);
            Assert.AreEqual(300, horizontalPanel.DesiredSize.Height, 0.1);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200),
                    new Point(100, 200),
                    new Point(200, 200)
                },
                expectedArrangedCount: 9);
            Assert.AreEqual(new Size(), ((UIElement)horizontalPanel.Children[9]).RenderSize);

            var verticalPanel = CreateVariableSizedWrapGrid(Orientation.Vertical, 10);
            verticalPanel.Measure(new Size(verticalPanel.Width, verticalPanel.Height));
            verticalPanel.Arrange(new Rect(0, 0, verticalPanel.Width, verticalPanel.Height));
            verticalPanel.UpdateLayout();

            Assert.AreEqual(300, verticalPanel.DesiredSize.Width, 0.1);
            Assert.AreEqual(300, verticalPanel.DesiredSize.Height, 0.1);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0),
                    new Point(200, 100),
                    new Point(200, 200)
                },
                expectedArrangedCount: 9);
            Assert.AreEqual(new Size(), ((UIElement)verticalPanel.Children[9]).RenderSize);
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:VariableSizedWrapGrid
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    ItemWidth="40"
                    ItemHeight="30"
                    Orientation="Horizontal"
                    MaximumRowsOrColumns="2"
                    HorizontalChildrenAlignment="Center"
                    VerticalChildrenAlignment="Bottom">
                    <controls:VariableSizedWrapGrid.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:VariableSizedWrapGrid.BackgroundTransition>
                    <controls:VariableSizedWrapGrid.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:VariableSizedWrapGrid.ChildrenTransitions>
                    <Border
                        Width="10"
                        Height="10"
                        Background="Red"
                        controls:VariableSizedWrapGrid.ColumnSpan="2"
                        controls:VariableSizedWrapGrid.RowSpan="3" />
                </controls:VariableSizedWrapGrid>
                """;

            var panel = (ModernVariableSizedWrapGrid)XamlReader.Parse(xaml);
            var child = (UIElement)panel.Children[0];

            Assert.AreEqual(40, panel.ItemWidth);
            Assert.AreEqual(30, panel.ItemHeight);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.MaximumRowsOrColumns);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(2, ModernVariableSizedWrapGrid.GetColumnSpan(child));
            Assert.AreEqual(3, ModernVariableSizedWrapGrid.GetRowSpan(child));
        });
    }

    [TestMethod]
    public void WrapGridAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernWrapGrid
            {
                ItemHeight = 40,
                ItemWidth = 50,
                Orientation = Orientation.Horizontal,
                HorizontalChildrenAlignment = HorizontalAlignment.Center,
                VerticalChildrenAlignment = VerticalAlignment.Bottom,
                MaximumRowsOrColumns = 3,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(40, panel.ItemHeight);
            Assert.AreEqual(50, panel.ItemWidth);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.AreEqual(3, panel.MaximumRowsOrColumns);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void WrapGridWrapsHorizontallyAndVertically()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateWrapGrid(Orientation.Horizontal, 7);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200)
                });

            var verticalPanel = CreateWrapGrid(Orientation.Vertical, 7);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0)
                });
        });
    }

    [TestMethod]
    public void WrapGridParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:WrapGrid
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    ItemWidth="40"
                    ItemHeight="30"
                    Orientation="Horizontal"
                    MaximumRowsOrColumns="2"
                    HorizontalChildrenAlignment="Center"
                    VerticalChildrenAlignment="Bottom">
                    <controls:WrapGrid.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:WrapGrid.BackgroundTransition>
                    <controls:WrapGrid.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:WrapGrid.ChildrenTransitions>
                    <Border Width="10" Height="10" Background="Red" />
                </controls:WrapGrid>
                """;

            var panel = (ModernWrapGrid)XamlReader.Parse(xaml);

            Assert.AreEqual(40, panel.ItemWidth);
            Assert.AreEqual(30, panel.ItemHeight);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.MaximumRowsOrColumns);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void ItemsStackPanelAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var defaultPanel = new ModernItemsStackPanel();

            Assert.AreEqual(Orientation.Vertical, defaultPanel.Orientation);
            Assert.AreEqual(new Thickness(), defaultPanel.GroupPadding);
            Assert.AreEqual(GroupHeaderPlacement.Top, defaultPanel.GroupHeaderPlacement);
            Assert.AreEqual(ItemsUpdatingScrollMode.KeepItemsInView, defaultPanel.ItemsUpdatingScrollMode);
            Assert.AreEqual(0.0, defaultPanel.CacheLength);
            Assert.IsTrue(defaultPanel.AreStickyGroupHeadersEnabled);
            Assert.AreEqual(-1, defaultPanel.FirstCacheIndex);
            Assert.AreEqual(-1, defaultPanel.FirstVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, defaultPanel.ScrollingDirection);

            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernItemsStackPanel
            {
                GroupPadding = new Thickness(1, 2, 3, 4),
                Orientation = Orientation.Horizontal,
                GroupHeaderPlacement = GroupHeaderPlacement.Left,
                ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
                CacheLength = 2.5,
                AreStickyGroupHeadersEnabled = false,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(ItemsUpdatingScrollMode.KeepLastItemInView, panel.ItemsUpdatingScrollMode);
            Assert.AreEqual(2.5, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void ItemsStackPanelStacksChildrenAndReportsRealizedRange()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateItemsStackPanel(Orientation.Horizontal, 3);
            AssertItemsStackPanelPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100)
                });

            Assert.AreEqual(0, horizontalPanel.FirstCacheIndex);
            Assert.AreEqual(0, horizontalPanel.FirstVisibleIndex);
            Assert.AreEqual(2, horizontalPanel.LastVisibleIndex);
            Assert.AreEqual(2, horizontalPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, horizontalPanel.ScrollingDirection);

            var verticalPanel = CreateItemsStackPanel(Orientation.Vertical, 3);
            AssertItemsStackPanelPositions(
                verticalPanel,
                new[]
                {
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200)
                });
        });
    }

    [TestMethod]
    public void ItemsStackPanelParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ItemsStackPanel
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    GroupPadding="1,2,3,4"
                    Orientation="Horizontal"
                    GroupHeaderPlacement="Left"
                    ItemsUpdatingScrollMode="KeepLastItemInView"
                    CacheLength="2"
                    AreStickyGroupHeadersEnabled="False">
                    <controls:ItemsStackPanel.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ItemsStackPanel.BackgroundTransition>
                    <controls:ItemsStackPanel.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:ItemsStackPanel.ChildrenTransitions>
                    <Border Width="10" Height="10" Background="Red" />
                </controls:ItemsStackPanel>
                """;

            var panel = (ModernItemsStackPanel)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(ItemsUpdatingScrollMode.KeepLastItemInView, panel.ItemsUpdatingScrollMode);
            Assert.AreEqual(2.0, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void ItemsWrapGridAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var defaultPanel = new ModernItemsWrapGrid();

            Assert.AreEqual(Orientation.Vertical, defaultPanel.Orientation);
            Assert.AreEqual(-1, defaultPanel.MaximumRowsOrColumns);
            Assert.IsTrue(double.IsNaN(defaultPanel.ItemWidth));
            Assert.IsTrue(double.IsNaN(defaultPanel.ItemHeight));
            Assert.AreEqual(new Thickness(), defaultPanel.GroupPadding);
            Assert.AreEqual(GroupHeaderPlacement.Top, defaultPanel.GroupHeaderPlacement);
            Assert.AreEqual(0.0, defaultPanel.CacheLength);
            Assert.IsTrue(defaultPanel.AreStickyGroupHeadersEnabled);
            Assert.AreEqual(-1, defaultPanel.FirstCacheIndex);
            Assert.AreEqual(-1, defaultPanel.FirstVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, defaultPanel.ScrollingDirection);

            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernItemsWrapGrid
            {
                GroupPadding = new Thickness(1, 2, 3, 4),
                Orientation = Orientation.Horizontal,
                MaximumRowsOrColumns = 3,
                ItemWidth = 50,
                ItemHeight = 40,
                GroupHeaderPlacement = GroupHeaderPlacement.Left,
                CacheLength = 2.5,
                AreStickyGroupHeadersEnabled = false,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(3, panel.MaximumRowsOrColumns);
            Assert.AreEqual(50, panel.ItemWidth);
            Assert.AreEqual(40, panel.ItemHeight);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(2.5, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void ItemsWrapGridWrapsChildrenAndReportsRealizedRange()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateItemsWrapGrid(Orientation.Horizontal, 7);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200)
                });

            Assert.AreEqual(0, horizontalPanel.FirstCacheIndex);
            Assert.AreEqual(0, horizontalPanel.FirstVisibleIndex);
            Assert.AreEqual(6, horizontalPanel.LastVisibleIndex);
            Assert.AreEqual(6, horizontalPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, horizontalPanel.ScrollingDirection);

            var verticalPanel = CreateItemsWrapGrid(Orientation.Vertical, 7);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0)
                });
        });
    }

    [TestMethod]
    public void ItemsWrapGridParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ItemsWrapGrid
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    GroupPadding="1,2,3,4"
                    ItemWidth="40"
                    ItemHeight="30"
                    Orientation="Horizontal"
                    MaximumRowsOrColumns="2"
                    GroupHeaderPlacement="Left"
                    CacheLength="2"
                    AreStickyGroupHeadersEnabled="False">
                    <controls:ItemsWrapGrid.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ItemsWrapGrid.BackgroundTransition>
                    <controls:ItemsWrapGrid.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:ItemsWrapGrid.ChildrenTransitions>
                    <Border Width="10" Height="10" Background="Red" />
                </controls:ItemsWrapGrid>
                """;

            var panel = (ModernItemsWrapGrid)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(40, panel.ItemWidth);
            Assert.AreEqual(30, panel.ItemHeight);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.MaximumRowsOrColumns);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(2.0, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void BrushTransitionHelperUsesWinUISolidColorRules()
    {
        WpfTestHost.Run(() =>
        {
            if (!ModernWpf.Helper.IsAnimationsEnabled)
            {
                Assert.Inconclusive("BrushTransition follows the shared animation-enabled switch.");
            }

            var invalidations = 0;
            var helper = new BrushTransitionHelper(() => invalidations++);
            var transition = new BrushTransition { Duration = TimeSpan.FromHours(1) };
            var red = new SolidColorBrush(Colors.Red);

            helper.OnBrushChanged(null, red, transition);

            var fadeInBrush = AssertSolidColorBrush(helper.GetEffectiveBrush(red), Color.FromArgb(0, 255, 0, 0));
            Assert.AreNotSame(red, fadeInBrush);
            Assert.IsTrue(helper.IsTransitioning);

            var blue = new SolidColorBrush(Colors.Blue);
            helper.OnBrushChanged(red, blue, transition);

            Assert.AreSame(fadeInBrush, helper.GetEffectiveBrush(blue));
            Assert.IsTrue(helper.IsTransitioning);

            var gradient = new LinearGradientBrush(Colors.Red, Colors.Blue, 0);
            helper.OnBrushChanged(blue, gradient, transition);

            Assert.AreSame(gradient, helper.GetEffectiveBrush(gradient));
            Assert.IsFalse(helper.IsTransitioning);
            Assert.IsTrue(invalidations >= 3);
        });
    }

    [TestMethod]
    public void CoreTextInputDescriptionPresentersUseWinUIPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var controls = new Control[]
            {
                new TextBox(),
                new PasswordBox(),
                new DatePicker()
            };

            foreach (var control in controls)
            {
                ControlHelper.SetDescription(control, control.GetType().Name + " description");
            }

            using var host = new TestWindowHost(new StackPanel { Children = { controls[0], controls[1], controls[2] } });
            host.UpdateLayout();

            foreach (var control in controls)
            {
                var descriptionPresenter = FindTemplateChild<ContentPresenterEx>(control, "DescriptionPresenter");
                Assert.AreEqual(ControlHelper.GetDescription(control), descriptionPresenter.Content);
                Assert.AreEqual(Visibility.Visible, descriptionPresenter.Visibility);
                Assert.AreSame(
                    descriptionPresenter.TryFindResource("SystemControlDescriptionTextForegroundBrush"),
                    descriptionPresenter.Foreground);
            }
        });
    }

    [TestMethod]
    public void CoreItemTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var listBoxItem = new ListBoxItem
            {
                Content = "ListBox content",
                IsEnabled = false
            };
            var listViewItem = new System.Windows.Controls.ListViewItem
            {
                Content = "ListView content",
                IsSelected = true
            };
            var header = new GridViewColumnHeader
            {
                Content = "Header content"
            };

            using var host = new TestWindowHost(new StackPanel { Children = { listBoxItem, listViewItem, header } });
            host.UpdateLayout();

            var listBoxPresenter = FindTemplateChild<ContentPresenterEx>(listBoxItem, "ContentPresenter");
            Assert.AreEqual(listBoxItem.Content, listBoxPresenter.Content);
            Assert.AreSame(listBoxPresenter.TryFindResource("ListBoxItemForegroundDisabled"), listBoxPresenter.Foreground);

            var listViewPresenter = FindTemplateChild<ContentPresenterEx>(listViewItem, "ContentPresenter");
            Assert.AreEqual(listViewItem.Content, listViewPresenter.Content);
            Assert.AreSame(listViewPresenter.TryFindResource("ListViewItemForegroundSelected"), listViewPresenter.Foreground);

            var headerPresenter = FindVisualChild<ContentPresenterEx>(header)
                ?? throw new AssertFailedException("Expected GridViewColumnHeader template to use ContentPresenterEx.");
            Assert.AreEqual(header.Content, headerPresenter.Content);
        });
    }

    [TestMethod]
    public void CoreMenuItemTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var topLevelItem = CreateMenuItemWithTemplate("TopLevelItemTemplateKey", "File", null, isEnabled: true);
            var topLevelHeader = CreateMenuItemWithTemplate("TopLevelHeaderTemplateKey", "Edit", null, isEnabled: true);
            var submenuItem = CreateMenuItemWithTemplate("SubmenuItemTemplateKey", "Open", new TextBlock { Text = "Icon" }, isEnabled: false);
            var submenuHeader = CreateMenuItemWithTemplate("SubmenuHeaderTemplateKey", "More", new TextBlock { Text = "Icon" }, isEnabled: false);

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { topLevelItem, topLevelHeader, submenuItem, submenuHeader }
            });
            host.UpdateLayout();

            Assert.AreEqual(topLevelItem.Header, FindVisualChild<ContentPresenterEx>(topLevelItem)?.Content);
            Assert.AreEqual(topLevelHeader.Header, FindVisualChild<ContentPresenterEx>(topLevelHeader)?.Content);

            AssertMenuTemplatePresenterSlot(
                submenuItem,
                expectedForegroundResource: "MenuFlyoutItemForegroundDisabled");
            AssertMenuTemplatePresenterSlot(
                submenuHeader,
                expectedForegroundResource: "MenuFlyoutSubItemForegroundDisabled");
        });
    }

    [TestMethod]
    public void CoreTabControlTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var tabItem = new TabItem
            {
                Header = "Tab Header",
                Content = "Tab Content"
            };
            var tabControl = new TabControl
            {
                Width = 320,
                Height = 160
            };
            TabControlHelper.SetTabStripHeader(tabControl, "Strip Header");
            TabControlHelper.SetTabStripFooter(tabControl, "Strip Footer");
            tabControl.Items.Add(tabItem);

            using var host = new TestWindowHost(tabControl, width: 380, height: 220);
            host.UpdateLayout();

            var itemPresenter = FindTemplateChild<ContentPresenterEx>(tabItem, "ContentPresenter");
            Assert.AreEqual(tabItem.Header, itemPresenter.Content);
            Assert.AreSame(itemPresenter.TryFindResource("TabViewItemHeaderForegroundSelected"), itemPresenter.Foreground);

            var headerPresenter = FindTemplateChild<ContentPresenterEx>(tabControl, "HeaderContentPresenter");
            Assert.AreEqual(TabControlHelper.GetTabStripHeader(tabControl), headerPresenter.Content);

            var footerPresenter = FindTemplateChild<ContentPresenterEx>(tabControl, "FooterContentPresenter");
            Assert.AreEqual(TabControlHelper.GetTabStripFooter(tabControl), footerPresenter.Content);

            var selectedContentHost = FindTemplateChild<ContentPresenterEx>(tabControl, "PART_SelectedContentHost");
            Assert.AreEqual(tabItem.Content, selectedContentHost.Content);
        });
    }

    [TestMethod]
    public void CorePivotTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var pivotItem = new TabItem
            {
                Header = "Pivot Header",
                Content = "Pivot Content"
            };
            var pivot = new TabControl
            {
                Style = FindStyleResource("TabControlPivotStyle"),
                Width = 320,
                Height = 160
            };
            PivotHelper.SetTitle(pivot, "Pivot Title");
            PivotHelper.SetLeftHeader(pivot, "Left Header");
            PivotHelper.SetRightHeader(pivot, "Right Header");
            pivot.Items.Add(pivotItem);

            using var host = new TestWindowHost(pivot, width: 380, height: 220);
            host.UpdateLayout();

            var itemPresenter = FindTemplateChild<ContentPresenterEx>(pivotItem, "ContentPresenter");
            Assert.AreEqual(pivotItem.Header, itemPresenter.Content);
            Assert.AreSame(itemPresenter.TryFindResource("PivotHeaderItemForegroundSelected"), itemPresenter.Foreground);

            var titleControl = FindTemplateChild<ContentControl>(pivot, "TitleContentControl");
            var titlePresenter = FindVisualChild<ContentPresenterEx>(titleControl)
                ?? throw new AssertFailedException("Expected Pivot title template to use ContentPresenterEx.");
            Assert.AreEqual(PivotHelper.GetTitle(pivot), titlePresenter.Content);

            var leftHeader = FindTemplateChild<ContentPresenterEx>(pivot, "LeftHeaderPresenter");
            Assert.AreEqual(PivotHelper.GetLeftHeader(pivot), leftHeader.Content);

            var rightHeader = FindTemplateChild<ContentPresenterEx>(pivot, "RightHeaderPresenter");
            Assert.AreEqual(PivotHelper.GetRightHeader(pivot), rightHeader.Content);

            var selectedContentHost = FindTemplateChild<ContentPresenterEx>(pivot, "PART_SelectedContentHost");
            Assert.AreEqual(pivotItem.Content, selectedContentHost.Content);
        });
    }

    [TestMethod]
    public void CorePivotHeaderItemSelectionStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var selectedItem = new TabItem
            {
                Header = "Selected",
                Content = "Selected content"
            };
            var unselectedItem = new TabItem
            {
                Header = "Unselected",
                Content = "Unselected content"
            };
            var pivot = new TabControl
            {
                Style = FindStyleResource("TabControlPivotStyle"),
                Width = 320,
                Height = 160
            };
            pivot.Items.Add(selectedItem);
            pivot.Items.Add(unselectedItem);
            pivot.SelectedItem = selectedItem;

            using var host = new TestWindowHost(pivot, width: 380, height: 220);
            host.UpdateLayout();

            var root = FindTemplateChild<FrameworkElement>(unselectedItem, "Border");
            var selectedPipe = FindTemplateChild<FrameworkElement>(unselectedItem, "SelectedPipe");

            AssertStateSetter(root, "Disabled", "SelectedPipe.Visibility");
            AssertStateSetter(root, "Unselected", "SelectedPipe.Visibility");
            AssertStateSetter(root, "UnselectedLocked", "SelectedPipe.Visibility");
            AssertStateSetter(root, "UnselectedPointerOver", "SelectedPipe.Visibility");
            AssertStateSetter(root, "UnselectedPressed", "SelectedPipe.Visibility");
            Assert.AreEqual("Unselected", GetCurrentStateName(root, "SelectionStates"));
            Assert.AreEqual(Visibility.Collapsed, selectedPipe.Visibility);

            pivot.SelectedItem = unselectedItem;
            host.UpdateLayout();

            Assert.AreEqual("Selected", GetCurrentStateName(root, "SelectionStates"));
            Assert.AreEqual(Visibility.Visible, selectedPipe.Visibility);

            unselectedItem.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreEqual("Disabled", GetCurrentStateName(root, "SelectionStates"));
            Assert.AreEqual(Visibility.Collapsed, selectedPipe.Visibility);
        });
    }

    [TestMethod]
    public void CoreResidualTemplatesUseExpectedPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = new RadioButton
            {
                Content = "Radio content",
                Foreground = Brushes.Red
            };
            var listViewHeaderItem = new ListViewHeaderItem
            {
                Content = "List header",
                Foreground = Brushes.Blue
            };
            var titleBarButton = new TitleBarButton
            {
                Content = "X",
                Foreground = Brushes.Green,
                IsActive = true
            };

            var hostPanel = new StackPanel();
            hostPanel.Children.Add(radioButton);
            hostPanel.Children.Add(listViewHeaderItem);
            hostPanel.Children.Add(titleBarButton);

            using var host = new TestWindowHost(hostPanel, width: 320, height: 180);
            host.UpdateLayout();

            var radioPresenter = FindTemplateChild<ContentPresenter>(radioButton, "ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), radioPresenter.GetType());
            Assert.AreEqual(radioButton.Content, radioPresenter.Content);
            Assert.AreSame(radioButton.Foreground, TextElement.GetForeground(radioPresenter));

            var headerPresenter = FindTemplateChild<ContentPresenterEx>(listViewHeaderItem, "ContentPresenter");
            Assert.AreEqual(listViewHeaderItem.Content, headerPresenter.Content);
            Assert.AreSame(listViewHeaderItem.Foreground, headerPresenter.Foreground);

            var titlePresenter = FindTemplateChild<ContentPresenterEx>(titleBarButton, "Content");
            Assert.AreEqual(titleBarButton.Content, titlePresenter.Content);
            Assert.AreSame(titleBarButton.Foreground, titlePresenter.Foreground);
            Assert.AreEqual(titleBarButton.FontSize, titlePresenter.FontSize);
        });
    }

    [TestMethod]
    public void SimpleShellTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var page = new ModernWpf.Controls.Page
            {
                Content = "Page content",
                Foreground = Brushes.Red
            };
            var frame = new ModernWpf.Controls.Frame();
            var expander = new System.Windows.Controls.Expander
            {
                Header = "Expander header",
                Content = "Expander content",
                Foreground = Brushes.Purple,
                IsExpanded = true
            };

            var hostPanel = new StackPanel();
            hostPanel.Children.Add(frame);
            hostPanel.Children.Add(expander);

            using var pageHost = new TestWindowHost(page, width: 240, height: 120);
            using var host = new TestWindowHost(hostPanel, width: 360, height: 320);
            pageHost.UpdateLayout();
            host.UpdateLayout();

            var pagePresenter = FindVisualChild<ContentPresenterEx>(page)
                ?? throw new AssertFailedException("Expected Page template to use ContentPresenterEx.");
            Assert.AreEqual(page.Content, pagePresenter.Content);
            Assert.AreSame(page.Foreground, pagePresenter.Foreground);

            Assert.IsInstanceOfType(FindTemplateChild<ContentPresenterEx>(frame, "FirstContentPresenter"), typeof(ContentPresenterEx));
            Assert.IsInstanceOfType(FindTemplateChild<ContentPresenterEx>(frame, "SecondContentPresenter"), typeof(ContentPresenterEx));

            var expandSite = FindTemplateChild<ContentPresenterEx>(expander, "ExpandSite");
            Assert.AreEqual(expander.Content, expandSite.Content);
            Assert.AreSame(expander.Foreground, expandSite.Foreground);

            var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            Assert.IsTrue(
                VisualTreeTestHelper.EnumerateDescendants(headerSite)
                    .OfType<ContentPresenterEx>()
                    .Any(presenter => Equals(expander.Header, presenter.Content)),
                "Expected Expander header template to use ContentPresenterEx.");
        });
    }

    [TestMethod]
    public void StatusBarTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var statusBar = new StatusBar();
            var statusBarItem = new StatusBarItem
            {
                Content = "Status content"
            };
            statusBar.Items.Add(statusBarItem);

            using var host = new TestWindowHost(statusBar, width: 260, height: 80);
            host.UpdateLayout();

            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemBackground"), statusBarItem.Background);
            Assert.AreEqual((Thickness)statusBarItem.TryFindResource("StatusBarItemPadding"), statusBarItem.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, statusBarItem.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, statusBarItem.VerticalContentAlignment);

            var border = FindVisualChild<Border>(statusBarItem)
                ?? throw new AssertFailedException("Expected StatusBarItem template to use official WPF Border chrome.");
            var presenter = VisualTreeTestHelper.EnumerateDescendants(statusBarItem)
                .OfType<ContentPresenter>()
                .Single(item => Equals(item.Content, statusBarItem.Content));

            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
            Assert.AreEqual(HorizontalAlignment.Left, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, presenter.VerticalAlignment);
            Assert.AreSame(statusBarItem.Background, border.Background);
            Assert.AreEqual(statusBarItem.Padding, border.Padding);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(statusBarItem));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(statusBarItem));

            statusBarItem.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemBackgroundDisabled"), statusBarItem.Background);
            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemForegroundDisabled"), statusBarItem.Foreground);
        });
    }

    [TestMethod]
    public void GroupBoxTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var groupBox = new GroupBox
            {
                Header = "Group header",
                Content = "Group content"
            };

            using var host = new TestWindowHost(groupBox, width: 240, height: 140);
            host.UpdateLayout();

            Assert.IsTrue(groupBox.OverridesDefaultStyle);
            Assert.AreSame(groupBox.TryFindResource("GroupBoxBackground"), groupBox.Background);
            Assert.AreSame(groupBox.TryFindResource("GroupBoxBorderBrush"), groupBox.BorderBrush);
            Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxBorderThickness"), groupBox.BorderThickness);
            Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxPadding"), groupBox.Padding);

            var border = FindVisualChild<Border>(groupBox)
                ?? throw new AssertFailedException("Expected GroupBox template to use official WPF Border chrome.");
            var presenters = VisualTreeTestHelper.EnumerateDescendants(groupBox)
                .OfType<ContentPresenter>()
                .ToArray();
            var headerPresenter = presenters.Single(item => Equals(item.Content, groupBox.Header));
            var contentPresenter = presenters.Single(item => Equals(item.Content, groupBox.Content));

            Assert.AreSame(groupBox.Background, border.Background);
            Assert.AreSame(groupBox.BorderBrush, border.BorderBrush);
            Assert.AreEqual(groupBox.BorderThickness, border.BorderThickness);
            Assert.AreEqual(typeof(ContentPresenter), headerPresenter.GetType());
            Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
            Assert.AreEqual(0, Grid.GetRow(headerPresenter));
            Assert.AreEqual(1, Grid.GetRow(contentPresenter));
            Assert.AreEqual((double)groupBox.TryFindResource("GroupBoxHeaderFontSize"), TextElement.GetFontSize(headerPresenter));
            Assert.AreSame(groupBox.TryFindResource("GroupBoxHeaderForeground"), TextElement.GetForeground(headerPresenter));
            Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(groupBox.Padding, contentPresenter.Margin);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(groupBox));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(groupBox));
        });
    }

    [TestMethod]
    public void LabelStyleUsesOfficialWpfFluentStyleSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultLabelStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(Label));
            Assert.AreEqual(typeof(Label), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Label), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            Assert.IsFalse(defaultStyle.Setters.OfType<Setter>().Any(item => item.Property == Control.TemplateProperty));
            Assert.IsFalse(defaultStyle.Setters.OfType<Setter>().Any(item => item.Property == Control.OverridesDefaultStyleProperty));

            var label = new Label
            {
                Width = 160,
                Height = 40,
                Content = "_Label content"
            };

            using var host = new TestWindowHost(label, width: 200, height: 80);
            host.UpdateLayout();

            Assert.AreEqual(new Thickness(0, 0, 0, 4), label.Padding);
            Assert.IsFalse(label.Focusable);
            Assert.IsTrue(label.SnapsToDevicePixels);
            Assert.IsFalse(label.OverridesDefaultStyle);
            Assert.AreSame(label.TryFindResource("LabelForeground"), label.Foreground);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(label));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(label));
        });
    }

    [TestMethod]
    public void CalendarNavigationButtonsUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var calendar = new Calendar();
            using var host = new TestWindowHost(calendar, width: 360, height: 320);
            host.UpdateLayout();

            var calendarItem = FindTemplateChild<CalendarItem>(calendar, "PART_CalendarItem");
            AssertCalendarNavigationPresenter(FindTemplateChild<Button>(calendarItem, "PART_HeaderButton"));
            AssertCalendarNavigationPresenter(FindTemplateChild<Button>(calendarItem, "PART_PreviousButton"));
            AssertCalendarNavigationPresenter(FindTemplateChild<Button>(calendarItem, "PART_NextButton"));
        });
    }

    [TestMethod]
    public void DataGridWpfSpecificTemplatesUseModernPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var cell = new DataGridCell
            {
                Style = FindStyleResource("DataGridCellExpanded"),
                Content = "Cell content",
                Foreground = Brushes.Red
            };
            var columnHeader = new DataGridColumnHeader
            {
                Style = FindStyleResource("DefaultDataGridColumnHeaderStyle"),
                Content = "Column header",
                Foreground = Brushes.Blue
            };
            var rowHeader = new DataGridRowHeader
            {
                Style = FindStyleResource("DefaultDataGridRowHeaderStyle"),
                Content = "Row header",
                Foreground = Brushes.Green
            };
            var groupHeader = new ToggleButton
            {
                Style = FindStyleResource("DataGridRowGroupHeaderStyle"),
                Content = "Group header",
                Foreground = Brushes.Purple
            };

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { cell, columnHeader, rowHeader, groupHeader }
            }, width: 360, height: 220);
            host.UpdateLayout();

            AssertDataGridPresenter(cell, cell.Content, cell.Foreground);
            AssertDataGridPresenter(columnHeader, columnHeader.Content, columnHeader.Foreground);
            AssertDataGridPresenter(rowHeader, rowHeader.Content, rowHeader.Foreground);
            AssertDataGridPresenter(groupHeader, groupHeader.Content, groupHeader.Foreground);
        });
    }

    [TestMethod]
    public void BorderExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:BorderEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Button Content="Parsed" />
                </controls:BorderEx>
                """;

            var border = (BorderEx)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(2), border.Padding);
            Assert.AreEqual(new CornerRadius(3), border.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, border.BackgroundSizing);
            Assert.IsInstanceOfType(border.Child, typeof(Button));
        });
    }

    [TestMethod]
    public void BorderExOuterBackgroundSizingPaintsBehindBorder()
    {
        WpfTestHost.Run(() =>
        {
            var inner = RenderBorderEdgePixel(BackgroundSizing.InnerBorderEdge);
            var outer = RenderBorderEdgePixel(BackgroundSizing.OuterBorderEdge);

            Assert.IsTrue(outer.R > inner.R + 40, $"Expected outer edge red channel above inner edge. Inner={inner}, Outer={outer}");
            Assert.IsTrue(outer.A > inner.A + 40, $"Expected outer edge alpha above inner edge. Inner={inner}, Outer={outer}");
        });
    }

    [TestMethod]
    public void BorderExOuterBackgroundSizingInflatesOuterCornerByHalfBorder()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            var roundedCorner = RenderBorderPixel(border, 27, 1, 30, 30);
            var straightEdge = RenderBorderPixel(border, 1, 15, 30, 30);

            Assert.IsTrue(roundedCorner.A < 30, $"Expected inflated WinUI outer corner to clip the pixel. Pixel={roundedCorner}");
            Assert.IsTrue(straightEdge.R > 200 && straightEdge.A > 200, $"Expected outer edge background under the transparent border. Pixel={straightEdge}");
        });
    }

    [TestMethod]
    public void BorderExLayoutClipUsesNonUniformCornerRadius()
    {
        WpfTestHost.Run(() =>
        {
            var border = new TestBorderEx
            {
                Width = 24,
                Height = 24,
                ClipToBounds = true,
                CornerRadius = new CornerRadius(0, 12, 0, 0)
            };
            border.Measure(new Size(24, 24));
            border.Arrange(new Rect(0, 0, 24, 24));
            border.UpdateLayout();

            var clip = border.GetLayoutClipForTest(new Size(24, 24));

            Assert.IsNotNull(clip);
            Assert.IsTrue(clip.FillContains(new Point(1, 1)), "Top-left corner should remain square.");
            Assert.IsFalse(clip.FillContains(new Point(23, 1)), "Top-right corner should be clipped by the non-uniform radius.");
            Assert.IsTrue(clip.FillContains(new Point(12, 12)), "Center should remain inside the clip.");
        });
    }

    [TestMethod]
    public void RoundedLayoutClipPreservesBaseLayoutClip()
    {
        WpfTestHost.Run(() =>
        {
            var baseClip = new RectangleGeometry(new Rect(0, 0, 12, 24));

            var clip = LayoutChromeHelper.CreateRoundedLayoutClip(
                new Size(24, 24),
                new CornerRadius(12),
                baseClip);

            Assert.IsNotNull(clip);
            Assert.IsTrue(clip.FillContains(new Point(6, 12)), "Point inside both clips should remain visible.");
            Assert.IsFalse(clip.FillContains(new Point(18, 12)), "Point outside the base layout clip should be clipped.");
            Assert.IsFalse(clip.FillContains(new Point(1, 1)), "Point outside the rounded corner should be clipped.");
        });
    }

    [TestMethod]
    public void LayoutChromeCornerRadiusChangeRefreshesChildClip()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                Width = 30,
                Height = 30,
                Child = CreateRedChildBox()
            };
            AssertDynamicRoundedChildClip(border, value => border.CornerRadius = value);

            var presenter = new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Content = CreateRedChildBox()
            };
            AssertDynamicRoundedChildClip(presenter, value => presenter.CornerRadius = value);

            var stackPanel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30
            };
            stackPanel.Children.Add(CreateRedChildBox());
            AssertDynamicRoundedChildClip(stackPanel, value => stackPanel.CornerRadius = value);

            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30
            };
            grid.Children.Add(CreateRedChildBox());
            AssertDynamicRoundedChildClip(grid, value => grid.CornerRadius = value);

            var relativePanel = new ModernRelativePanel
            {
                Width = 30,
                Height = 30
            };
            relativePanel.Children.Add(CreateRedChildBox());
            AssertDynamicRoundedChildClip(relativePanel, value => relativePanel.CornerRadius = value);
        });
    }

    [TestMethod]
    public void BorderExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new BorderEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void BorderExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChildRenderClip(new BorderEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0),
                Child = CreateRedChildBox()
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExOffsetsContentByChrome()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var presenter = new ContentPresenterEx
            {
                Width = 120,
                Height = 80,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10),
                Content = button
            };

            using var host = new TestWindowHost(presenter, width: 140, height: 100);

            AssertBoundsRelativeTo(button, presenter, new Rect(15, 15, 90, 50));
        });
    }

    [TestMethod]
    public void ContentPresenterExAlignsContentInsideChrome()
    {
        WpfTestHost.Run(() =>
        {
            var button = CreateButton(40, 20);
            var presenter = new ContentPresenterEx
            {
                Width = 140,
                Height = 100,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Content = button
            };

            using var host = new TestWindowHost(presenter, width: 160, height: 120);

            Assert.AreEqual(HorizontalAlignment.Stretch, new ContentPresenterEx().HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, new ContentPresenterEx().VerticalContentAlignment);
            AssertBoundsRelativeTo(button, presenter, new Rect(85, 65, 40, 20));
        });
    }

    [TestMethod]
    public void ContentPresenterExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ContentPresenterEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge"
                    CharacterSpacing="15"
                    HorizontalContentAlignment="Right"
                    IsTextScaleFactorEnabled="False"
                    LineHeight="37"
                    LineStackingStrategy="MaxHeight"
                    MaxLines="2"
                    OpticalMarginAlignment="TrimSideBearings"
                    TextLineBounds="TrimToBaseline"
                    TextWrapping="Wrap"
                    VerticalContentAlignment="Bottom">
                    <Button Content="Parsed" />
                </controls:ContentPresenterEx>
                """;

            var presenter = (ContentPresenterEx)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(2), presenter.Padding);
            Assert.AreEqual(new CornerRadius(3), presenter.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(15, presenter.CharacterSpacing);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(37, presenter.LineHeight);
            Assert.AreEqual(LineStackingStrategy.MaxHeight, presenter.LineStackingStrategy);
            Assert.AreEqual(2, presenter.MaxLines);
            Assert.AreEqual(ModernWpf.OpticalMarginAlignment.TrimSideBearings, presenter.OpticalMarginAlignment);
            Assert.AreEqual(ModernWpf.TextLineBounds.TrimToBaseline, presenter.TextLineBounds);
            Assert.AreEqual(TextWrapping.Wrap, presenter.TextWrapping);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
            Assert.IsInstanceOfType(presenter.Content, typeof(Button));
        });
    }

    [TestMethod]
    public void ContentPresenterExUsesWinUIInheritedTextMetadata()
    {
        WpfTestHost.Run(() =>
        {
            Assert.AreSame(ControlHelper.CharacterSpacingProperty, ContentPresenterEx.CharacterSpacingProperty);
            Assert.AreSame(ControlHelper.IsTextScaleFactorEnabledProperty, ContentPresenterEx.IsTextScaleFactorEnabledProperty);
            Assert.AreSame(ControlHelper.CharacterSpacingProperty, ModernContentControlEx.CharacterSpacingProperty);
            Assert.AreSame(ControlHelper.IsTextScaleFactorEnabledProperty, ModernContentControlEx.IsTextScaleFactorEnabledProperty);

            AssertInheritedTextMetadata(ContentPresenterEx.CharacterSpacingProperty, typeof(ContentPresenterEx));
            AssertInheritedTextMetadata(ContentPresenterEx.IsTextScaleFactorEnabledProperty, typeof(ContentPresenterEx));
            AssertInheritedTextMetadata(ModernContentControlEx.CharacterSpacingProperty, typeof(ModernContentControlEx));
            AssertInheritedTextMetadata(ModernContentControlEx.IsTextScaleFactorEnabledProperty, typeof(ModernContentControlEx));

            var parent = new StackPanel();
            var presenter = new ContentPresenterEx();

            parent.SetValue(ControlHelper.CharacterSpacingProperty, 24);
            parent.SetValue(ControlHelper.IsTextScaleFactorEnabledProperty, false);
            parent.Children.Add(presenter);

            using var host = new TestWindowHost(parent, width: 120, height: 40);

            Assert.AreEqual(24, presenter.CharacterSpacing);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);

            presenter.CharacterSpacing = 7;
            presenter.IsTextScaleFactorEnabled = true;

            Assert.AreEqual(7, presenter.CharacterSpacing);
            Assert.IsTrue(presenter.IsTextScaleFactorEnabled);
        });
    }

    [TestMethod]
    public void ContentPresenterExPushesSupportedTextPropertiesToDefaultTextBlock()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new ContentPresenterEx
            {
                Width = 120,
                Height = 80,
                Content = "Hello",
                FontFamily = new FontFamily("Courier New"),
                FontSize = 23,
                FontStretch = FontStretches.Condensed,
                FontStyle = FontStyles.Italic,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Blue,
                LineHeight = 37,
                LineStackingStrategy = LineStackingStrategy.MaxHeight,
                MaxLines = 2,
                TextWrapping = TextWrapping.Wrap
            };

            using var host = new TestWindowHost(presenter, width: 140, height: 100);

            var textBlock = FindVisualChild<TextBlock>(presenter)
                ?? throw new AssertFailedException("Expected ContentPresenterEx to generate a default TextBlock.");
            Assert.AreEqual("Courier New", textBlock.FontFamily.Source);
            Assert.AreEqual(23, textBlock.FontSize);
            Assert.AreEqual(FontStretches.Condensed, textBlock.FontStretch);
            Assert.AreEqual(FontStyles.Italic, textBlock.FontStyle);
            Assert.AreEqual(FontWeights.Bold, textBlock.FontWeight);
            Assert.AreSame(Brushes.Blue, textBlock.Foreground);
            Assert.AreEqual(TextWrapping.Wrap, textBlock.TextWrapping);
            Assert.AreEqual(37, textBlock.LineHeight);
            Assert.AreEqual(LineStackingStrategy.MaxHeight, textBlock.LineStackingStrategy);
            Assert.AreEqual(74, textBlock.MaxHeight);
            Assert.IsTrue(textBlock.ClipToBounds);

            presenter.Foreground = Brushes.Green;
            presenter.FontSize = 19;
            presenter.MaxLines = 0;

            Assert.AreSame(Brushes.Green, textBlock.Foreground);
            Assert.AreEqual(19, textBlock.FontSize);
            Assert.AreEqual(double.PositiveInfinity, textBlock.MaxHeight);
            Assert.IsFalse(textBlock.ClipToBounds);
        });
    }

    [TestMethod]
    public void ContentPresenterExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChildRenderClip(new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0),
                Content = CreateRedChildBox()
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void ContentControlExUsesWinUIDefaultAlignmentAndTransitions()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var control = new ModernContentControlEx
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                ContentTransitions = transitions
            };

            Assert.AreEqual(HorizontalAlignment.Left, control.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Top, control.VerticalContentAlignment);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, control.BackgroundSizing);
            Assert.AreSame(backgroundTransition, control.BackgroundTransition);
            Assert.AreEqual(0, control.CharacterSpacing);
            Assert.AreSame(transitions, control.ContentTransitions);
            Assert.IsTrue(control.IsTextScaleFactorEnabled);
        });
    }

    [TestMethod]
    public void ContentControlExTemplateForwardsContentTransitionsAndAlignment()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = CreateButton(40, 20);
            var control = new ModernContentControlEx
            {
                Width = 120,
                Height = 80,
                Background = Brushes.Red,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(5),
                CharacterSpacing = 21,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(10),
                Content = button,
                ContentTransitions = transitions,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                IsTextScaleFactorEnabled = false,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(control, width: 140, height: 100);

            var presenter = FindVisualChild<ContentPresenterEx>(control)
                ?? throw new AssertFailedException("Expected ContentControlEx template to use ContentPresenterEx.");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreSame(backgroundTransition, presenter.BackgroundTransition);
            Assert.AreEqual(21, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreSame(button, presenter.Content);
            Assert.AreEqual(new CornerRadius(3), presenter.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
            Assert.AreSame(button, control.ContentTemplateRoot);
            AssertBoundsRelativeTo(button, control, new Rect(65, 45, 40, 20));
        });
    }

    [TestMethod]
    public void ContentControlExExposesWinUIContentTemplateRoot()
    {
        WpfTestHost.Run(() =>
        {
            var template = (DataTemplate)XamlReader.Parse(
                """
                <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                    <TextBlock Text="{Binding}" />
                </DataTemplate>
                """);
            var control = new ModernContentControlEx
            {
                Width = 120,
                Height = 60,
                Content = "Templated",
                ContentTemplate = template
            };

            Assert.IsNull(control.ContentTemplateRoot);

            using var host = new TestWindowHost(control, width: 140, height: 80);

            var textBlock = control.ContentTemplateRoot as TextBlock
                ?? throw new AssertFailedException("Expected ContentTemplateRoot to expose the generated data-template root.");
            Assert.AreEqual("Templated", textBlock.Text);

            var button = CreateButton(40, 20);
            control.ContentTemplate = null;
            control.Content = button;
            host.UpdateLayout();

            Assert.AreSame(button, control.ContentTemplateRoot);
        });
    }

    [TestMethod]
    public void ContentControlExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ContentControlEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    BackgroundSizing="OuterBorderEdge"
                    CharacterSpacing="21"
                    Padding="2"
                    CornerRadius="3"
                    HorizontalContentAlignment="Right"
                    IsTextScaleFactorEnabled="False"
                    RecognizesAccessKey="True"
                    VerticalContentAlignment="Bottom">
                    <controls:ContentControlEx.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ContentControlEx.BackgroundTransition>
                    <controls:ContentControlEx.ContentTransitions>
                        <animation:TransitionCollection />
                    </controls:ContentControlEx.ContentTransitions>
                    <Button Content="Parsed" />
                </controls:ContentControlEx>
                """;

            var control = (ModernContentControlEx)XamlReader.Parse(xaml);

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, control.BackgroundSizing);
            Assert.IsNotNull(control.BackgroundTransition);
            Assert.AreEqual(21, control.CharacterSpacing);
            Assert.AreEqual(new Thickness(2), control.Padding);
            Assert.AreEqual(new CornerRadius(3), control.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Right, control.HorizontalContentAlignment);
            Assert.IsFalse(control.IsTextScaleFactorEnabled);
            Assert.IsTrue(control.RecognizesAccessKey);
            Assert.IsNotNull(control.ContentTransitions);
            Assert.AreEqual(VerticalAlignment.Bottom, control.VerticalContentAlignment);
            Assert.IsInstanceOfType(control.Content, typeof(Button));
        });
    }

    [TestMethod]
    public void ControlHelperAcceptsWinUIControlTemplateSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new Button();

            ControlHelper.SetBackgroundSizing(button, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetCharacterSpacing(button, 18);
            ControlHelper.SetContentTransitions(button, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(button, false);

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, ControlHelper.GetBackgroundSizing(button));
            Assert.AreEqual(18, ControlHelper.GetCharacterSpacing(button));
            Assert.AreSame(transitions, ControlHelper.GetContentTransitions(button));
            Assert.IsFalse(ControlHelper.GetIsTextScaleFactorEnabled(button));
        });
    }

    [TestMethod]
    public void ButtonTemplateUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                Width = 100,
                Height = 40,
                Content = "Button",
                Foreground = Brushes.Red
            };
            ControlHelper.SetCornerRadius(button, new CornerRadius(6));

            using var host = new TestWindowHost(button, width: 140, height: 80);

            var border = FindTemplateChild<Border>(button, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(button, "ContentPresenter");

            Assert.AreEqual(button.Content, presenter.Content);
            Assert.AreSame(button.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(ControlHelper.GetCornerRadius(button), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(button));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(button));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            Assert.AreEqual(3, button.Template.Triggers.OfType<Trigger>().Count());
        });
    }

    [TestMethod]
    public void AccentButtonStyleUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                Width = 100,
                Height = 40,
                Content = "Accent",
                Foreground = Brushes.Blue,
                Style = (Style)Application.Current.FindResource("AccentButtonStyle")
            };
            ControlHelper.SetCornerRadius(button, new CornerRadius(8));

            using var host = new TestWindowHost(button, width: 140, height: 80);

            var border = FindTemplateChild<Border>(button, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(button, "ContentPresenter");

            Assert.AreEqual(button.Content, presenter.Content);
            Assert.AreSame(button.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(ControlHelper.GetCornerRadius(button), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(button));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(button));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            Assert.AreEqual(3, button.Template.Triggers.OfType<Trigger>().Count());
        });
    }

    [TestMethod]
    public void RepeatButtonTemplateUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var repeatButton = new RepeatButton
            {
                Width = 100,
                Height = 40,
                Content = "Repeat",
                Foreground = Brushes.Blue
            };
            ControlHelper.SetCornerRadius(repeatButton, new CornerRadius(8));

            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);

            var border = FindTemplateChild<Border>(repeatButton, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(repeatButton, "ContentPresenter");

            Assert.AreEqual(repeatButton.Content, presenter.Content);
            Assert.AreSame(repeatButton.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(ControlHelper.GetCornerRadius(repeatButton), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(repeatButton));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(repeatButton));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(repeatButton));
            Assert.AreEqual(3, repeatButton.Template.Triggers.OfType<Trigger>().Count());
        });
    }

    [TestMethod]
    public void ToggleButtonTemplateUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var toggleButton = new ToggleButton
            {
                Width = 100,
                Height = 40,
                Content = "Toggle",
                Foreground = Brushes.Blue
            };
            ControlHelper.SetCornerRadius(toggleButton, new CornerRadius(8));

            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);

            var border = FindTemplateChild<Border>(toggleButton, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

            Assert.AreEqual(toggleButton.Content, presenter.Content);
            Assert.AreSame(toggleButton.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(ControlHelper.GetCornerRadius(toggleButton), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(toggleButton));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(toggleButton));
            Assert.IsFalse(ToggleButtonHelper.GetVisualStateSettersEnabled(toggleButton));
            Assert.AreEqual(7, toggleButton.Template.Triggers.OfType<MultiTrigger>().Count());
        });
    }

    [TestMethod]
    public void HyperlinkButtonTemplateUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var hyperlinkButton = new HyperlinkButton
            {
                Width = 120,
                Height = 40,
                Content = "Link"
            };
            ControlHelper.SetCharacterSpacing(hyperlinkButton, 21);
            ControlHelper.SetContentTransitions(hyperlinkButton, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(hyperlinkButton, false);

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(hyperlinkButton)
                ?? throw new AssertFailedException("Expected HyperlinkButton template to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(hyperlinkButton));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(21, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNotNull(presenter.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), presenter.BackgroundTransition.Duration);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(hyperlinkButton));
            AssertAnimatedIconStateSetters(presenter, "ContentPresenter.(ui:AnimatedIcon.State)");
            AssertAnimatedIconStateTransitions(hyperlinkButton, presenter);
        });
    }

    [TestMethod]
    public void ToolTipTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            var toolTip = new ToolTip
            {
                Width = 30,
                Height = 30,
                Content = "Tip",
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                Padding = new Thickness(0)
            };

            toolTip.ApplyTemplate();
            toolTip.Measure(new Size(30, 30));
            toolTip.Arrange(new Rect(0, 0, 30, 30));
            toolTip.UpdateLayout();

            var border = FindVisualChild<Border>(toolTip)
                ?? throw new AssertFailedException("Expected ToolTip template to use official WPF Border chrome.");
            var presenter = FindVisualChild<ContentPresenter>(toolTip)
                ?? throw new AssertFailedException("Expected ToolTip template to use official WPF ContentPresenter.");

            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(toolTip));
            Assert.IsNull(FindVisualChild<ThemeShadowChrome>(toolTip));
            Assert.AreEqual(new CornerRadius(4), border.CornerRadius);
            Assert.AreSame(toolTip.Background, border.Background);
            Assert.AreSame(toolTip.BorderBrush, border.BorderBrush);
            Assert.AreEqual(toolTip.BorderThickness, border.BorderThickness);
            Assert.IsInstanceOfType(border.Effect, typeof(System.Windows.Media.Effects.DropShadowEffect));
            Assert.AreEqual(new Thickness(0), presenter.Margin);
        });
    }

    [TestMethod]
    public void ContentControlExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var control = new ModernContentControlEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            using var host = new TestWindowHost(control, width: 50, height: 50);
            var presenter = FindVisualChild<ContentPresenterEx>(control)
                ?? throw new AssertFailedException("Expected ContentControlEx template to use ContentPresenterEx.");

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void StackPanelExSupportsSpacingAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 200,
                Height = 120,
                Orientation = Orientation.Vertical,
                Spacing = 10,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(height: 20);
            var second = CreateStretchButton(height: 20);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 220, height: 140);

            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 170, 20));
            AssertBoundsRelativeTo(second, panel, new Rect(15, 45, 170, 20));
        });
    }

    [TestMethod]
    public void StackPanelExSupportsNegativeSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 200,
                Height = 80,
                Orientation = Orientation.Vertical,
                Spacing = -10,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(height: 20);
            var second = CreateStretchButton(height: 20);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 220, height: 100);

            Assert.AreEqual(-10, panel.Spacing);
            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 170, 20));
            AssertBoundsRelativeTo(second, panel, new Rect(15, 25, 170, 20));
        });
    }

    [TestMethod]
    public void StackPanelExAcceptsWinUISnapPointSurface()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                AreScrollSnapPointsRegular = true
            };

            Assert.IsTrue(panel.AreScrollSnapPointsRegular);
            Assert.IsTrue(panel.AreVerticalSnapPointsRegular);
            Assert.IsFalse(panel.AreHorizontalSnapPointsRegular);

            panel.Orientation = Orientation.Horizontal;

            Assert.IsTrue(panel.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(panel.AreVerticalSnapPointsRegular);
        });
    }

    [TestMethod]
    public void StackPanelExComputesWinUISnapPoints()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 140,
                Height = 50,
                Orientation = Orientation.Horizontal,
                AreScrollSnapPointsRegular = true,
                Margin = new Thickness(3, 5, 7, 11)
            };
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));

            using var host = new TestWindowHost(panel, width: 180, height: 90);

            var snapInfo = (IScrollSnapPointsInfo)panel;
            Assert.IsTrue(snapInfo.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(snapInfo.AreVerticalSnapPointsRegular);

            var interval = snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near, out var offset);
            Assert.AreEqual(3.0f, offset, 0.001f);
            Assert.AreEqual(50.0f, interval, 0.001f);

            interval = snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Center, out offset);
            Assert.AreEqual(28.0f, offset, 0.001f);
            Assert.AreEqual(50.0f, interval, 0.001f);

            interval = snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Far, out offset);
            Assert.AreEqual(7.0f, offset, 0.001f);
            Assert.AreEqual(50.0f, interval, 0.001f);

            interval = snapInfo.GetRegularSnapPoints(Orientation.Vertical, SnapPointsAlignment.Near, out offset);
            Assert.AreEqual(0.0f, offset, 0.001f);
            Assert.AreEqual(0.0f, interval, 0.001f);
            Assert.ThrowsException<InvalidOperationException>(() => snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near));

            panel.AreScrollSnapPointsRegular = false;

            Assert.ThrowsException<InvalidOperationException>(() => snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near, out _));
            AssertSnapPoints(new[] { 0.0f, 53.0f }, snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near));
            AssertSnapPoints(new[] { 28.0f, 78.0f }, snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Center));
            AssertSnapPoints(new[] { 53.0f, 103.0f }, snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Far));
            Assert.AreEqual(0, snapInfo.GetIrregularSnapPoints(Orientation.Vertical, SnapPointsAlignment.Near).Count);
        });
    }

    [TestMethod]
    public void StackPanelExRaisesWinUISnapPointChangeEvents()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 160,
                Height = 60,
                Orientation = Orientation.Horizontal,
                AreScrollSnapPointsRegular = true
            };
            var snapInfo = (IScrollSnapPointsInfo)panel;
            int horizontalChanges = 0;
            int verticalChanges = 0;
            snapInfo.HorizontalSnapPointsChanged += (_, __) => horizontalChanges++;
            snapInfo.VerticalSnapPointsChanged += (_, __) => verticalChanges++;

            panel.Children.Add(CreateStretchButton(width: 40, height: 20));

            using var host = new TestWindowHost(panel, width: 180, height: 80);

            Assert.AreEqual(1, horizontalChanges);
            Assert.AreEqual(0, verticalChanges);

            panel.AreScrollSnapPointsRegular = false;
            int beforeChildChange = horizontalChanges;
            panel.Children.Add(CreateStretchButton(width: 30, height: 20));
            host.UpdateLayout();

            Assert.IsTrue(horizontalChanges > beforeChildChange);
            Assert.AreEqual(0, verticalChanges);

            panel.Orientation = Orientation.Vertical;
            host.UpdateLayout();

            Assert.IsTrue(verticalChanges > 0);
            Assert.IsFalse(snapInfo.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(snapInfo.AreVerticalSnapPointsRegular);
        });
    }

    [TestMethod]
    public void StackPanelExHorizontalSpacingSkipsCollapsedChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 160,
                Height = 70,
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(width: 40);
            var collapsed = CreateStretchButton(width: 50);
            collapsed.Visibility = Visibility.Collapsed;
            var second = CreateStretchButton(width: 30);
            panel.Children.Add(first);
            panel.Children.Add(collapsed);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 180, height: 90);

            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 40, 40));
            AssertBoundsRelativeTo(second, panel, new Rect(65, 15, 30, 40));
        });
    }

    [TestMethod]
    public void StackPanelExOrientationChangeReflowsChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 140,
                Height = 100,
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            var first = CreateStretchButton(width: 40, height: 20);
            var second = CreateStretchButton(width: 30, height: 15);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 160, height: 120);

            AssertStackAxisOffsetRelativeTo(first, panel, Orientation.Vertical, 0);
            AssertStackAxisOffsetRelativeTo(second, panel, Orientation.Vertical, 25);

            panel.Orientation = Orientation.Horizontal;
            host.UpdateLayout();

            AssertStackAxisOffsetRelativeTo(first, panel, Orientation.Horizontal, 0);
            AssertStackAxisOffsetRelativeTo(second, panel, Orientation.Horizontal, 45);
        });
    }

    [TestMethod]
    public void StackPanelExDesiredSizeCountsVisibleSpacingAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Orientation = Orientation.Vertical,
                Spacing = 7,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));
            panel.Children.Add(new Button
            {
                Width = 100,
                Height = 80,
                Visibility = Visibility.Collapsed
            });
            panel.Children.Add(CreateStretchButton(width: 30, height: 15));

            panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.AreEqual(80, panel.DesiredSize.Width, 1.0, "Desired width should include max visible child width plus border and padding.");
            Assert.AreEqual(72, panel.DesiredSize.Height, 1.0, "Desired height should include visible children, one spacing gap, border, and padding.");
        });
    }

    [TestMethod]
    public void StackPanelExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:StackPanelEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    AreScrollSnapPointsRegular="True"
                    Spacing="4"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Button Content="Parsed" />
                </controls:StackPanelEx>
                """;

            var panel = (ModernStackPanelEx)XamlReader.Parse(xaml);

            Assert.IsTrue(panel.AreScrollSnapPointsRegular);
            Assert.AreEqual(4, panel.Spacing);
            Assert.AreEqual(new Thickness(2), panel.Padding);
            Assert.AreEqual(new CornerRadius(3), panel.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void StackPanelExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(panel);
        });
    }

    [TestMethod]
    public void StackPanelExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void StackPanelExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            panel.Children.Add(CreateRedChildBox());

            AssertRoundedChildRenderClip(panel);
        });
    }

    [TestMethod]
    public void GridExSupportsSpacingSpansAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 230,
                Height = 130,
                UseLayoutRounding = false,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(5),
                RowSpacing = 10,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            var spanned = CreateStretchButton();

            Grid.SetColumn(second, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(first);
            grid.Children.Add(second);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 250, height: 150);

            AssertBoundsRelativeTo(first, grid, new Rect(7, 7, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(117, 7, 100, 50));
            AssertBoundsRelativeTo(spanned, grid, new Rect(7, 67, 210, 50));
        });
    }

    [TestMethod]
    public void GridExUsesWinUINegativeSpacingLayout()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 190,
                Height = 90,
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            var spanned = CreateStretchButton();

            Grid.SetColumn(second, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(first);
            grid.Children.Add(second);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 210, height: 110);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(90, 0, 100, 50));
            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 40, 190, 50));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingHandlesAutoAndStarTracks()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 190,
                Height = 90,
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var autoCell = CreateLayoutBox(width: 80, height: 40);
            var starCell = CreateLayoutBox(height: 40);
            var spanned = CreateLayoutBox();

            Grid.SetColumn(starCell, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(autoCell);
            grid.Children.Add(starCell);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 210, height: 110);

            AssertBoundsRelativeTo(autoCell, grid, new Rect(0, 0, 80, 40));
            AssertBoundsRelativeTo(starCell, grid, new Rect(70, 0, 120, 40));
            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 30, 190, 60));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingDesiredSizeUsesAutoTracks()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var first = CreateLayoutBox(width: 80, height: 30);
            var second = CreateLayoutBox(width: 70, height: 20);
            Grid.SetColumn(second, 1);
            Grid.SetRow(second, 1);

            grid.Children.Add(first);
            grid.Children.Add(second);

            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.AreEqual(140, grid.DesiredSize.Width, 1.0, "Desired width should subtract the negative column spacing from auto tracks.");
            Assert.AreEqual(40, grid.DesiredSize.Height, 1.0, "Desired height should subtract the negative row spacing from auto tracks.");
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 110,
                Height = 30,
                UseLayoutRounding = false,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var spanned = CreateLayoutBox(width: 110);
            var secondColumnProbe = CreateLayoutBox();
            Grid.SetColumnSpan(spanned, 2);
            Grid.SetColumn(secondColumnProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondColumnProbe);

            using var host = new TestWindowHost(grid, width: 130, height: 50);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 110, 30));
            AssertBoundsRelativeTo(secondColumnProbe, grid, new Rect(50, 0, 60, 30));
        });
    }

    [TestMethod]
    public void GridExPositiveSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 130,
                Height = 30,
                UseLayoutRounding = false,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var spanned = CreateLayoutBox(width: 130);
            var secondColumnProbe = CreateLayoutBox();
            Grid.SetColumnSpan(spanned, 2);
            Grid.SetColumn(secondColumnProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondColumnProbe);

            using var host = new TestWindowHost(grid, width: 150, height: 50);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 130, 30));
            AssertBoundsRelativeTo(secondColumnProbe, grid, new Rect(70, 0, 60, 30));
        });
    }

    [TestMethod]
    public void GridExNegativeRowSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 110,
                UseLayoutRounding = false,
                RowSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var spanned = CreateLayoutBox(height: 110);
            var secondRowProbe = CreateLayoutBox();
            Grid.SetRowSpan(spanned, 2);
            Grid.SetRow(secondRowProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondRowProbe);

            using var host = new TestWindowHost(grid, width: 50, height: 130);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 30, 110));
            AssertBoundsRelativeTo(secondRowProbe, grid, new Rect(0, 50, 30, 60));
        });
    }

    [TestMethod]
    public void GridExPositiveRowSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 130,
                UseLayoutRounding = false,
                RowSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var spanned = CreateLayoutBox(height: 130);
            var secondRowProbe = CreateLayoutBox();
            Grid.SetRowSpan(spanned, 2);
            Grid.SetRow(secondRowProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondRowProbe);

            using var host = new TestWindowHost(grid, width: 50, height: 150);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 30, 130));
            AssertBoundsRelativeTo(secondRowProbe, grid, new Rect(0, 70, 30, 60));
        });
    }

    [TestMethod]
    public void GridExPositiveSpacingHandlesStarSpans()
    {
        WpfTestHost.Run(() =>
        {
            var grid = CreateStarSpanGrid(width: 430, height: 320, spacing: 10);
            var first = CreateLayoutBox();
            var middle = CreateLayoutBox();
            var trailing = CreateLayoutBox();

            Grid.SetColumnSpan(first, 2);
            Grid.SetRow(middle, 1);
            Grid.SetColumn(middle, 1);
            Grid.SetColumnSpan(middle, 2);
            Grid.SetRow(trailing, 2);
            Grid.SetColumn(trailing, 2);
            Grid.SetColumnSpan(trailing, 2);

            grid.Children.Add(first);
            grid.Children.Add(middle);
            grid.Children.Add(trailing);

            using var host = new TestWindowHost(grid, width: 450, height: 340);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 210, 100));
            AssertBoundsRelativeTo(middle, grid, new Rect(110, 110, 210, 100));
            AssertBoundsRelativeTo(trailing, grid, new Rect(220, 220, 210, 100));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingHandlesStarSpans()
    {
        WpfTestHost.Run(() =>
        {
            var grid = CreateStarSpanGrid(width: 370, height: 280, spacing: -10);
            var first = CreateLayoutBox();
            var middle = CreateLayoutBox();
            var trailing = CreateLayoutBox();

            Grid.SetColumnSpan(first, 2);
            Grid.SetRow(middle, 1);
            Grid.SetColumn(middle, 1);
            Grid.SetColumnSpan(middle, 2);
            Grid.SetRow(trailing, 2);
            Grid.SetColumn(trailing, 2);
            Grid.SetColumnSpan(trailing, 2);

            grid.Children.Add(first);
            grid.Children.Add(middle);
            grid.Children.Add(trailing);

            using var host = new TestWindowHost(grid, width: 390, height: 300);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 190, 100));
            AssertBoundsRelativeTo(middle, grid, new Rect(90, 90, 190, 100));
            AssertBoundsRelativeTo(trailing, grid, new Rect(180, 180, 190, 100));
        });
    }

    [TestMethod]
    public void GridExDefinitionChangesInvalidateLayout()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 220,
                Height = 50,
                UseLayoutRounding = false,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            Grid.SetColumn(second, 1);
            grid.Children.Add(first);
            grid.Children.Add(second);

            using var host = new TestWindowHost(grid, width: 240, height: 80);
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(0, 0, 100, 50));

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(110, 0, 100, 50));

            grid.ColumnDefinitions[0].Width = new GridLength(80);
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 80, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(90, 0, 100, 50));
        });
    }

    [TestMethod]
    public void GridExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(grid);
        });
    }

    [TestMethod]
    public void GridExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ModernGridEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void GridExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            grid.Children.Add(CreateRedChildBox());

            AssertRoundedChildRenderClip(grid);
        });
    }

    [TestMethod]
    public void GridExAllowsNegativeSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                RowSpacing = -10,
                ColumnSpacing = -11
            };

            Assert.AreEqual(-10, grid.RowSpacing);
            Assert.AreEqual(-11, grid.ColumnSpacing);
            Assert.ThrowsException<ArgumentException>(() => grid.RowSpacing = double.NaN);
            Assert.ThrowsException<ArgumentException>(() => grid.ColumnSpacing = double.NaN);
        });
    }

    [TestMethod]
    public void GridExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:GridEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    RowSpacing="4"
                    ColumnSpacing="6"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <controls:GridEx.RowDefinitions>
                        <RowDefinition Height="Auto" />
                    </controls:GridEx.RowDefinitions>
                    <controls:GridEx.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                    </controls:GridEx.ColumnDefinitions>
                    <Button Content="Parsed" />
                </controls:GridEx>
                """;

            var grid = (ModernGridEx)XamlReader.Parse(xaml);

            Assert.AreEqual(4, grid.RowSpacing);
            Assert.AreEqual(6, grid.ColumnSpacing);
            Assert.AreEqual(new Thickness(2), grid.Padding);
            Assert.AreEqual(new CornerRadius(3), grid.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, grid.BackgroundSizing);
            Assert.AreEqual(1, grid.Children.Count);
        });
    }

    [TestMethod]
    public void RelativePanelAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernRelativePanel
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(2),
                ChildrenTransitions = childrenTransitions,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(3)
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(Brushes.Blue, panel.BorderBrush);
            Assert.AreEqual(new Thickness(2), panel.BorderThickness);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
            Assert.AreEqual(new CornerRadius(4), panel.CornerRadius);
            Assert.AreEqual(new Thickness(3), panel.Padding);
        });
    }

    [TestMethod]
    public void RelativePanelArrangesWinUIConstraintsAndInvalidatesOnChange()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernRelativePanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = false
            };
            var first = CreateLayoutBox(width: 100, height: 100);
            var second = CreateLayoutBox(width: 100, height: 100);
            var third = CreateLayoutBox(width: 100, height: 100);

            ModernRelativePanel.SetRightOf(second, first);
            ModernRelativePanel.SetRightOf(third, second);
            panel.Children.Add(first);
            panel.Children.Add(second);
            panel.Children.Add(third);

            using var host = new TestWindowHost(panel, width: 400, height: 400);

            AssertBoundsRelativeTo(first, panel, new Rect(0, 0, 100, 100));
            AssertBoundsRelativeTo(second, panel, new Rect(100, 0, 100, 100));
            AssertBoundsRelativeTo(third, panel, new Rect(200, 0, 100, 100));

            ModernRelativePanel.SetRightOf(second, null);
            ModernRelativePanel.SetRightOf(third, null);
            ModernRelativePanel.SetBelow(second, first);
            ModernRelativePanel.SetBelow(third, second);
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, panel, new Rect(0, 0, 100, 100));
            AssertBoundsRelativeTo(second, panel, new Rect(0, 100, 100, 100));
            AssertBoundsRelativeTo(third, panel, new Rect(0, 200, 100, 100));
        });
    }

    [TestMethod]
    public void RelativePanelUsesWinUIBorderChromeForLayout()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernRelativePanel
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(10),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = false
            };
            var first = CreateLayoutBox(width: 100, height: 100);
            var second = CreateLayoutBox(width: 100, height: 100);
            var third = CreateLayoutBox(width: 100, height: 100);

            ModernRelativePanel.SetRightOf(second, first);
            ModernRelativePanel.SetRightOf(third, second);
            panel.Children.Add(first);
            panel.Children.Add(second);
            panel.Children.Add(third);

            using var host = new TestWindowHost(panel, width: 400, height: 400);

            Assert.AreEqual(340, panel.RenderSize.Width, 1.0);
            Assert.AreEqual(140, panel.RenderSize.Height, 1.0);
            AssertBoundsRelativeTo(first, panel, new Rect(20, 20, 100, 100));
            AssertBoundsRelativeTo(second, panel, new Rect(120, 20, 100, 100));
            AssertBoundsRelativeTo(third, panel, new Rect(220, 20, 100, 100));
        });
    }

    [TestMethod]
    public void RelativePanelParsesWinUIConstraintXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:RelativePanel
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    HorizontalAlignment="Left"
                    VerticalAlignment="Top"
                    Padding="5"
                    BorderThickness="1"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Border x:Name="b0" Width="50" Height="20" Background="Transparent" />
                    <Border x:Name="b1" Width="30" Height="20" Background="Transparent" controls:RelativePanel.RightOf="b0" />
                </controls:RelativePanel>
                """;

            var panel = (ModernRelativePanel)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(5), panel.Padding);
            Assert.AreEqual(new Thickness(1), panel.BorderThickness);
            Assert.AreEqual(new CornerRadius(3), panel.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreEqual(2, panel.Children.Count);

            using var host = new TestWindowHost(panel, width: 120, height: 80);

            AssertBoundsRelativeTo((FrameworkElement)panel.Children[0], panel, new Rect(6, 6, 50, 20));
            AssertBoundsRelativeTo((FrameworkElement)panel.Children[1], panel, new Rect(56, 6, 30, 20));
        });
    }

    [TestMethod]
    public void RelativePanelRejectsInvalidWinUIConstraints()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernRelativePanel();
            var first = CreateLayoutBox(width: 100, height: 100);
            var second = CreateLayoutBox(width: 100, height: 100);
            panel.Children.Add(first);
            panel.Children.Add(second);

            Assert.ThrowsException<ArgumentException>(() => ModernRelativePanel.SetRightOf(second, true));

            ModernRelativePanel.SetRightOf(second, "missing");
            Assert.ThrowsException<InvalidOperationException>(() => panel.Measure(new Size(300, 300)));

            ModernRelativePanel.SetRightOf(second, first);
            ModernRelativePanel.SetLeftOf(first, second);
            Assert.ThrowsException<InvalidOperationException>(() => panel.Measure(new Size(300, 300)));
        });
    }

    private static Button CreateButton(double width, double height)
    {
        return new Button
        {
            Width = width,
            Height = height
        };
    }

    private static Button CreateStretchButton(double? width = null, double? height = null)
    {
        return new Button
        {
            Width = width ?? double.NaN,
            Height = height ?? double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static System.Windows.Controls.Border CreateLayoutBox(double? width = null, double? height = null)
    {
        return new System.Windows.Controls.Border
        {
            Width = width ?? double.NaN,
            Height = height ?? double.NaN,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static System.Windows.Controls.Border CreateRedChildBox()
    {
        return new System.Windows.Controls.Border
        {
            Width = 30,
            Height = 30,
            Background = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static ModernGridEx CreateStarSpanGrid(double width, double height, double spacing)
    {
        var grid = new ModernGridEx
        {
            Width = width,
            Height = height,
            UseLayoutRounding = false,
            RowSpacing = spacing,
            ColumnSpacing = spacing
        };

        for (int i = 0; i < 4; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int i = 0; i < 3; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        return grid;
    }

    private static void AssertBoundsRelativeTo(FrameworkElement element, Visual ancestor, Rect expected)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point());
        var actual = new Rect(origin, element.RenderSize);
        Assert.AreEqual(expected.X, actual.X, 1.0, "X");
        Assert.AreEqual(expected.Y, actual.Y, 1.0, "Y");
        Assert.AreEqual(expected.Width, actual.Width, 2.0, "Width");
        Assert.AreEqual(expected.Height, actual.Height, 2.0, "Height");
    }

    private static void AssertTransitionBrush<T>(
        T control,
        Action<T, Brush> setBackground,
        Action<T, BrushTransition> setTransition,
        Action<T> clearTransition,
        Func<T, Brush> getEffectiveBackground)
    {
        var targetBrush = new SolidColorBrush(Colors.Blue);

        setBackground(control, Brushes.Red);
        setTransition(control, new BrushTransition { Duration = TimeSpan.FromSeconds(1) });
        setBackground(control, targetBrush);

        Assert.AreNotSame(targetBrush, getEffectiveBackground(control));

        clearTransition(control);

        Assert.AreSame(targetBrush, getEffectiveBackground(control));
    }

    private static void AssertInheritedTextMetadata(DependencyProperty property, Type ownerType)
    {
        var metadata = (FrameworkPropertyMetadata)property.GetMetadata(ownerType);
        Assert.IsTrue(metadata.AffectsMeasure, $"{ownerType.Name}.{property.Name} should affect measure.");
        Assert.IsTrue(metadata.AffectsRender, $"{ownerType.Name}.{property.Name} should affect render.");
        Assert.IsTrue(metadata.Inherits, $"{ownerType.Name}.{property.Name} should inherit like WinUI text formatting properties.");
    }

    private static void AssertStackAxisOffsetRelativeTo(FrameworkElement element, Visual ancestor, Orientation orientation, double expected)
    {
        var actual = element.TransformToAncestor(ancestor).Transform(new Point());
        Assert.AreEqual(expected, orientation == Orientation.Horizontal ? actual.X : actual.Y, 1.0, orientation.ToString());
    }

    private static void AssertSnapPoints(float[] expected, IReadOnlyList<float> actual)
    {
        Assert.AreEqual(expected.Length, actual.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 0.001f, $"Snap point {i}");
        }
    }

    private static SolidColorBrush AssertSolidColorBrush(Brush brush, Color expectedColor)
    {
        var solidColorBrush = brush as SolidColorBrush
            ?? throw new AssertFailedException("Expected a SolidColorBrush.");
        Assert.AreEqual(expectedColor, solidColorBrush.Color);
        return solidColorBrush;
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "SelectionStates");
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"SelectionStates.{stateName} should set {setterTarget}.");
    }

    private static void AssertAnimatedIconStateSetters(FrameworkElement stateGroupsRoot, string setterTarget)
    {
        AssertAnimatedIconStateSetter(stateGroupsRoot, "PointerOver", setterTarget, "PointerOver");
        AssertAnimatedIconStateSetter(stateGroupsRoot, "Pressed", setterTarget, "Pressed");
        AssertAnimatedIconStateSetter(stateGroupsRoot, "Disabled", setterTarget, "Normal");
    }

    private static void AssertAnimatedIconStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string setterTarget,
        string expectedValue)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        var setter = stateEx.Setters.SingleOrDefault(item => item.Target == setterTarget)
            ?? throw new AssertFailedException($"CommonStates.{stateName} should set {setterTarget}.");

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertAnimatedIconStateTransitions(Control control, DependencyObject stateTarget)
    {
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "PointerOver", false));
        Assert.AreEqual("PointerOver", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "Pressed", false));
        Assert.AreEqual("Pressed", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "Disabled", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "Normal", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static MenuItem CreateMenuItemWithTemplate(string templateResourceId, object header, object? icon, bool isEnabled)
    {
        return new MenuItem
        {
            Header = header,
            Icon = icon,
            IsEnabled = isEnabled,
            Template = FindMenuItemTemplate(templateResourceId)
        };
    }

    private static ControlTemplate FindMenuItemTemplate(string resourceId)
    {
        var key = new ComponentResourceKey(typeof(MenuItem), resourceId);
        return Application.Current.TryFindResource(key) as ControlTemplate
            ?? throw new AssertFailedException($"Expected MenuItem template resource '{resourceId}'.");
    }

    private static Style FindStyleResource(string resourceId)
    {
        return Application.Current.TryFindResource(resourceId) as Style
            ?? throw new AssertFailedException($"Expected style resource '{resourceId}'.");
    }

    private static void AssertMenuTemplatePresenterSlot(MenuItem menuItem, string expectedForegroundResource)
    {
        var contentPresenter = FindTemplateChild<ContentPresenterEx>(menuItem, "ContentPresenter");
        Assert.AreEqual(menuItem.Header, contentPresenter.Content);
        Assert.AreSame(contentPresenter.TryFindResource(expectedForegroundResource), contentPresenter.Foreground);

        var iconContent = FindTemplateChild<ContentPresenterEx>(menuItem, "IconContent");
        Assert.AreEqual(menuItem.Icon, iconContent.Content);
        Assert.AreSame(iconContent.TryFindResource(expectedForegroundResource), iconContent.Foreground);
    }

    private static void AssertCalendarNavigationPresenter(Button button)
    {
        var presenter = FindTemplateChild<ContentPresenterEx>(button, "Text");
        Assert.AreEqual(button.Content, presenter.Content);
        Assert.AreSame(button.Foreground, presenter.Foreground);
        Assert.AreEqual(button.Padding, presenter.Padding);
        Assert.AreEqual(ControlHelper.GetCornerRadius(button), presenter.CornerRadius);
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonBorderBrush"), presenter.BorderBrush);

        Assert.IsTrue(VisualStateManager.GoToState(button, "MouseOver", false));
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonBorderBrushPointerOver"), presenter.BorderBrush);
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonForegroundPointerOver"), presenter.Foreground);

        Assert.IsTrue(VisualStateManager.GoToState(button, "Pressed", false));
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonBorderBrush"), presenter.BorderBrush);
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonForegroundPressed"), presenter.Foreground);

        Assert.IsTrue(VisualStateManager.GoToState(button, "Normal", false));
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonBorderBrush"), presenter.BorderBrush);
        Assert.AreSame(button.Foreground, presenter.Foreground);
    }

    private static void AssertDataGridPresenter(DependencyObject root, object expectedContent, Brush expectedForeground)
    {
        var presenter = FindVisualChild<ContentPresenterEx>(root)
            ?? throw new AssertFailedException($"Expected {root.GetType().Name} template to use ContentPresenterEx.");
        Assert.AreEqual(expectedContent, presenter.Content);
        Assert.AreSame(expectedForeground, presenter.Foreground);
    }

    private static ModernVariableSizedWrapGrid CreateVariableSizedWrapGrid(Orientation orientation, int itemCount)
    {
        var panel = new ModernVariableSizedWrapGrid
        {
            Width = 300,
            Height = 300,
            ItemWidth = 100,
            ItemHeight = 100,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static ModernWrapGrid CreateWrapGrid(Orientation orientation, int itemCount)
    {
        var panel = new ModernWrapGrid
        {
            Width = 300,
            Height = 300,
            ItemWidth = 100,
            ItemHeight = 100,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static ModernItemsStackPanel CreateItemsStackPanel(Orientation orientation, int itemCount)
    {
        var panel = new ModernItemsStackPanel
        {
            Width = 300,
            Height = 300,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static ModernItemsWrapGrid CreateItemsWrapGrid(Orientation orientation, int itemCount)
    {
        var panel = new ModernItemsWrapGrid
        {
            Width = 300,
            Height = 300,
            ItemWidth = 100,
            ItemHeight = 100,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static void AssertItemsStackPanelPositions(ModernItemsStackPanel panel, IReadOnlyList<Point> expectedPositions)
    {
        panel.Measure(new Size(panel.Width, panel.Height));
        panel.Arrange(new Rect(0, 0, panel.Width, panel.Height));
        panel.UpdateLayout();

        Assert.AreEqual(expectedPositions.Count, panel.Children.Count);

        for (int i = 0; i < expectedPositions.Count; i++)
        {
            var actual = ((UIElement)panel.Children[i]).TranslatePoint(new Point(), panel);
            Assert.AreEqual(expectedPositions[i].X, actual.X, 0.1, $"Unexpected X position for item {i}.");
            Assert.AreEqual(expectedPositions[i].Y, actual.Y, 0.1, $"Unexpected Y position for item {i}.");
        }
    }

    private static void AssertVariableSizedWrapGridPositions(
        ModernVariableSizedWrapGrid panel,
        IReadOnlyList<Point> expectedPositions,
        int? expectedArrangedCount = null)
    {
        if (!expectedArrangedCount.HasValue)
        {
            panel.Measure(new Size(panel.Width, panel.Height));
            panel.Arrange(new Rect(0, 0, panel.Width, panel.Height));
            panel.UpdateLayout();
        }

        Assert.AreEqual(300, panel.DesiredSize.Width, 0.1);
        Assert.AreEqual(300, panel.DesiredSize.Height, 0.1);
        if (expectedArrangedCount.HasValue)
        {
            Assert.AreEqual(expectedArrangedCount.Value, expectedPositions.Count);
            Assert.IsTrue(panel.Children.Count >= expectedArrangedCount.Value);
        }
        else
        {
            Assert.AreEqual(expectedPositions.Count, panel.Children.Count);
        }

        for (int i = 0; i < expectedPositions.Count; i++)
        {
            var actual = ((UIElement)panel.Children[i]).TranslatePoint(new Point(), panel);
            Assert.AreEqual(expectedPositions[i].X, actual.X, 0.1, $"Unexpected X position for item {i}.");
            Assert.AreEqual(expectedPositions[i].Y, actual.Y, 0.1, $"Unexpected Y position for item {i}.");
        }
    }

    private static Color RenderBorderEdgePixel(BackgroundSizing backgroundSizing)
    {
        var border = new BorderEx
        {
            Width = 24,
            Height = 24,
            Background = Brushes.Red,
            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)),
            BorderThickness = new Thickness(6),
            BackgroundSizing = backgroundSizing
        };

        return RenderBorderPixel(border, 3, 12, 24, 24);
    }

    private static Color RenderBorderPixel(BorderEx border, int x, int y, int width, int height)
    {
        return RenderElementPixel(border, x, y, width, height);
    }

    private static void AssertOuterChromePixels(FrameworkElement element)
    {
        var roundedCorner = RenderElementPixel(element, 27, 1, 30, 30);
        var straightEdge = RenderElementPixel(element, 1, 15, 30, 30);

        Assert.IsTrue(roundedCorner.A < 30, $"Expected inflated WinUI outer corner to clip the pixel. Pixel={roundedCorner}");
        Assert.IsTrue(straightEdge.R > 200 && straightEdge.A > 200, $"Expected outer edge background under the transparent border. Pixel={straightEdge}");
    }

    private static void AssertRoundedChromeHitTest(FrameworkElement element)
    {
        element.Measure(new Size(30, 30));
        element.Arrange(new Rect(0, 0, 30, 30));
        element.UpdateLayout();

        Assert.IsNull(VisualTreeHelper.HitTest(element, new Point(1, 1)), "Top-left point should be clipped by the rounded chrome.");
        Assert.IsNotNull(VisualTreeHelper.HitTest(element, new Point(15, 15)), "Center point should hit inside the rounded chrome.");
    }

    private static void AssertRoundedChildRenderClip(FrameworkElement element)
    {
        var clippedCorner = RenderElementPixel(element, 1, 1, 30, 30);
        var center = RenderElementPixel(element, 15, 15, 30, 30);

        Assert.IsTrue(clippedCorner.A < 30, $"Expected child content to be clipped out of the rounded corner. Pixel={clippedCorner}");
        Assert.IsTrue(center.R > 200 && center.A > 200, $"Expected child content to render inside the rounded clip. Pixel={center}");
    }

    private static void AssertDynamicRoundedChildClip(FrameworkElement element, Action<CornerRadius> setCornerRadius)
    {
        setCornerRadius(new CornerRadius());
        using var host = new TestWindowHost(element, width: 120, height: 90);

        var squareCorner = RenderCurrentElementPixel(element, 1, 1, 30, 30);
        Assert.IsTrue(squareCorner.R > 200 && squareCorner.A > 200, $"Expected square corner content before radius change. Pixel={squareCorner}");

        setCornerRadius(new CornerRadius(12, 0, 0, 0));
        host.UpdateLayout();

        var clippedCorner = RenderCurrentElementPixel(element, 1, 1, 30, 30);
        Assert.IsTrue(clippedCorner.A < 30, $"Expected rounded corner clip to refresh after CornerRadius change. Pixel={clippedCorner}");
    }

    private static Color RenderElementPixel(FrameworkElement element, int x, int y, int width, int height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        return RenderCurrentElementPixel(element, x, y, width, height);
    }

    private static Color RenderCurrentElementPixel(FrameworkElement element, int x, int y, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        var pixels = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
        return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
    }

    private sealed class TestBorderEx : BorderEx
    {
        public Geometry GetLayoutClipForTest(Size layoutSlotSize)
        {
            return base.GetLayoutClip(layoutSlotSize);
        }
    }
}
