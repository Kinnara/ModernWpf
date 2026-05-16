using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TwoPaneView;

[TestClass]
public class TwoPaneViewApiTests
{
    private const double DefaultMinWideModeWidth = 641.0;
    private const double DefaultMinTallModeHeight = 641.0;

    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var pane1 = new Grid();
            var pane2 = new Grid();
            var twoPaneView = new ModernWpf.Controls.TwoPaneView();

            Assert.IsNull(twoPaneView.Pane1);
            Assert.IsNull(twoPaneView.Pane2);
            Assert.AreEqual(GridLength.Auto, twoPaneView.Pane1Length);
            Assert.AreEqual(new GridLength(1, GridUnitType.Star), twoPaneView.Pane2Length);
            Assert.AreEqual(TwoPaneViewPriority.Pane1, twoPaneView.PanePriority);
            Assert.AreEqual(TwoPaneViewMode.SinglePane, twoPaneView.Mode);
            Assert.AreEqual(TwoPaneViewWideModeConfiguration.LeftRight, twoPaneView.WideModeConfiguration);
            Assert.AreEqual(TwoPaneViewTallModeConfiguration.TopBottom, twoPaneView.TallModeConfiguration);
            Assert.AreEqual(DefaultMinWideModeWidth, twoPaneView.MinWideModeWidth);
            Assert.AreEqual(DefaultMinTallModeHeight, twoPaneView.MinTallModeHeight);

            twoPaneView.Pane1 = pane1;
            twoPaneView.Pane2 = pane2;
            twoPaneView.Pane1Length = new GridLength(2, GridUnitType.Star);
            twoPaneView.Pane2Length = new GridLength(3, GridUnitType.Star);
            twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
            twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;
            twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;
            twoPaneView.MinWideModeWidth = 500;
            twoPaneView.MinTallModeHeight = 700;

            Assert.AreSame(pane1, twoPaneView.Pane1);
            Assert.AreSame(pane2, twoPaneView.Pane2);
            Assert.AreEqual(new GridLength(2, GridUnitType.Star), twoPaneView.Pane1Length);
            Assert.AreEqual(new GridLength(3, GridUnitType.Star), twoPaneView.Pane2Length);
            Assert.AreEqual(TwoPaneViewPriority.Pane2, twoPaneView.PanePriority);
            Assert.AreEqual(TwoPaneViewWideModeConfiguration.RightLeft, twoPaneView.WideModeConfiguration);
            Assert.AreEqual(TwoPaneViewTallModeConfiguration.BottomTop, twoPaneView.TallModeConfiguration);
            Assert.AreEqual(500, twoPaneView.MinWideModeWidth);
            Assert.AreEqual(700, twoPaneView.MinTallModeHeight);

            twoPaneView.MinWideModeWidth = -1;
            twoPaneView.MinTallModeHeight = -1;

            Assert.AreEqual(0, twoPaneView.MinWideModeWidth);
            Assert.AreEqual(0, twoPaneView.MinTallModeHeight);

            twoPaneView.Pane1 = null;
            twoPaneView.Pane2 = null;

            Assert.IsNull(twoPaneView.Pane1);
            Assert.IsNull(twoPaneView.Pane2);
        });
    }

    [TestMethod]
    public void TwoPaneViewLayoutModesTest()
    {
        WpfTestHost.Run(() =>
        {
            var modeChanges = 0;
            var twoPaneView = new ModernWpf.Controls.TwoPaneView
            {
                Pane1 = new Border { Width = 40, Height = 40 },
                Pane2 = new Border { Width = 40, Height = 40 },
                Pane1Length = new GridLength(2, GridUnitType.Star),
                Pane2Length = new GridLength(3, GridUnitType.Star),
                Width = 900,
                Height = 720
            };
            twoPaneView.ModeChanged += (_, _) => modeChanges++;

            using var host = new TestWindowHost(twoPaneView, width: 1024, height: 768);

            var pane1ScrollViewer = FindNamedDescendant<FrameworkElement>(twoPaneView, "PART_Pane1ScrollViewer");
            var pane2ScrollViewer = FindNamedDescendant<FrameworkElement>(twoPaneView, "PART_Pane2ScrollViewer");
            var columnLeft = FindNamedTemplatePart<ColumnDefinition>(twoPaneView, "PART_ColumnLeft");
            var columnRight = FindNamedTemplatePart<ColumnDefinition>(twoPaneView, "PART_ColumnRight");
            var rowTop = FindNamedTemplatePart<RowDefinition>(twoPaneView, "PART_RowTop");
            var rowBottom = FindNamedTemplatePart<RowDefinition>(twoPaneView, "PART_RowBottom");

            Assert.AreEqual(TwoPaneViewMode.Wide, twoPaneView.Mode);
            AssertModeState(twoPaneView, "ViewMode_LeftRight");
            Assert.AreEqual(0, Grid.GetColumn(pane1ScrollViewer));
            Assert.AreEqual(2, Grid.GetColumn(pane2ScrollViewer));
            Assert.AreEqual(new GridLength(2, GridUnitType.Star), columnLeft.Width);
            Assert.AreEqual(new GridLength(3, GridUnitType.Star), columnRight.Width);
            Assert.IsTrue(modeChanges >= 1);

            twoPaneView.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;
            host.UpdateLayout();

            Assert.AreEqual(TwoPaneViewMode.Wide, twoPaneView.Mode);
            AssertModeState(twoPaneView, "ViewMode_RightLeft");
            Assert.AreEqual(2, Grid.GetColumn(pane1ScrollViewer));
            Assert.AreEqual(0, Grid.GetColumn(pane2ScrollViewer));
            Assert.AreEqual(new GridLength(3, GridUnitType.Star), columnLeft.Width);
            Assert.AreEqual(new GridLength(2, GridUnitType.Star), columnRight.Width);

            twoPaneView.MinWideModeWidth = 1000;
            twoPaneView.MinTallModeHeight = 500;
            host.UpdateLayout();

            Assert.AreEqual(TwoPaneViewMode.Tall, twoPaneView.Mode);
            AssertModeState(twoPaneView, "ViewMode_TopBottom");
            Assert.AreEqual(0, Grid.GetRow(pane1ScrollViewer));
            Assert.AreEqual(2, Grid.GetRow(pane2ScrollViewer));
            Assert.AreEqual(new GridLength(2, GridUnitType.Star), rowTop.Height);
            Assert.AreEqual(new GridLength(3, GridUnitType.Star), rowBottom.Height);

            twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;
            host.UpdateLayout();

            Assert.AreEqual(TwoPaneViewMode.Tall, twoPaneView.Mode);
            AssertModeState(twoPaneView, "ViewMode_BottomTop");
            Assert.AreEqual(2, Grid.GetRow(pane1ScrollViewer));
            Assert.AreEqual(0, Grid.GetRow(pane2ScrollViewer));
            Assert.AreEqual(new GridLength(3, GridUnitType.Star), rowTop.Height);
            Assert.AreEqual(new GridLength(2, GridUnitType.Star), rowBottom.Height);

            twoPaneView.TallModeConfiguration = TwoPaneViewTallModeConfiguration.SinglePane;
            twoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
            host.UpdateLayout();

            Assert.AreEqual(TwoPaneViewMode.SinglePane, twoPaneView.Mode);
            AssertModeState(twoPaneView, "ViewMode_TwoOnly");
            Assert.AreEqual(Visibility.Collapsed, pane1ScrollViewer.Visibility);
            Assert.AreEqual(Visibility.Visible, pane2ScrollViewer.Visibility);
            Assert.AreEqual(0, Grid.GetColumn(pane2ScrollViewer));
            Assert.AreEqual(0, Grid.GetRow(pane2ScrollViewer));
        });
    }

    private static void AssertModeState(ModernWpf.Controls.TwoPaneView twoPaneView, string expectedStateName)
    {
        var rootGrid = FindNamedDescendant<FrameworkElement>(twoPaneView, "RootGrid");
        var modeStates = VisualStateManager.GetVisualStateGroups(rootGrid)
            .OfType<VisualStateGroup>()
            .Single(group => group.Name == "ModeStates");
        Assert.IsNotNull(modeStates.CurrentState);
        Assert.AreEqual(expectedStateName, modeStates.CurrentState.Name);
    }

    [TestMethod]
    public void TwoPaneViewTemplateUsesWinUIPaneHostSlots()
    {
        WpfTestHost.Run(() =>
        {
            var pane1 = new Grid();
            var pane2 = new TextBlock { Text = "Pane 2" };
            var twoPaneView = new ModernWpf.Controls.TwoPaneView
            {
                Pane1 = pane1,
                Pane2 = pane2
            };

            using var host = new TestWindowHost(twoPaneView, width: 720, height: 720);

            var pane1ScrollViewer = FindNamedDescendant<ScrollViewer>(twoPaneView, "PART_Pane1ScrollViewer");
            var pane2ScrollViewer = FindNamedDescendant<ScrollViewer>(twoPaneView, "PART_Pane2ScrollViewer");

            var pane1Host = AssertPaneHost(pane1ScrollViewer);
            var pane2Host = AssertPaneHost(pane2ScrollViewer);

            Assert.AreSame(pane1, pane1Host.Content);
            Assert.AreSame(pane2, pane2Host.Content);
        });
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

    private static T FindNamedTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        var part = control.Template.FindName(name, control) as T;
        if (part == null)
        {
            throw new InvalidOperationException($"Could not find template part named '{name}'.");
        }

        return part;
    }

    private static ContentPresenterEx AssertPaneHost(ScrollViewer scrollViewer)
    {
        if (scrollViewer.Content is ContentPresenterEx paneHost)
        {
            return paneHost;
        }

        throw new AssertFailedException("Expected TwoPaneView pane ScrollViewer to host content through ContentPresenterEx.");
    }
}
