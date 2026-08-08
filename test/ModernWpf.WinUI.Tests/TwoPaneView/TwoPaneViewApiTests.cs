using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TwoPaneView
{
    [TestClass]
    public class TwoPaneViewApiTests
    {
        [TestMethod]
        public void DefaultsAndSettersMatchCurrentWinUI()
        {
            WpfTestHost.Run(() =>
            {
                var view = new ModernWpf.Controls.TwoPaneView();

                Assert.IsNull(view.Pane1);
                Assert.IsNull(view.Pane2);
                Assert.AreEqual(GridLength.Auto, view.Pane1Length);
                Assert.AreEqual(new GridLength(1, GridUnitType.Star), view.Pane2Length);
                Assert.AreEqual(TwoPaneViewPriority.Pane1, view.PanePriority);
                Assert.AreEqual(TwoPaneViewMode.SinglePane, view.Mode);
                Assert.AreEqual(TwoPaneViewWideModeConfiguration.LeftRight, view.WideModeConfiguration);
                Assert.AreEqual(TwoPaneViewTallModeConfiguration.TopBottom, view.TallModeConfiguration);
                Assert.AreEqual(641d, view.MinWideModeWidth);
                Assert.AreEqual(641d, view.MinTallModeHeight);

                var pane1 = new Border();
                var pane2 = new Border();
                view.Pane1 = pane1;
                view.Pane2 = pane2;
                view.Pane1Length = new GridLength(2, GridUnitType.Star);
                view.Pane2Length = new GridLength(160);
                view.PanePriority = TwoPaneViewPriority.Pane2;
                view.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;
                view.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;
                view.MinWideModeWidth = 500;
                view.MinTallModeHeight = 700;

                Assert.AreSame(pane1, view.Pane1);
                Assert.AreSame(pane2, view.Pane2);
                Assert.AreEqual(new GridLength(2, GridUnitType.Star), view.Pane1Length);
                Assert.AreEqual(new GridLength(160), view.Pane2Length);
                Assert.AreEqual(TwoPaneViewPriority.Pane2, view.PanePriority);
                Assert.AreEqual(TwoPaneViewWideModeConfiguration.RightLeft, view.WideModeConfiguration);
                Assert.AreEqual(TwoPaneViewTallModeConfiguration.BottomTop, view.TallModeConfiguration);
                Assert.AreEqual(500d, view.MinWideModeWidth);
                Assert.AreEqual(700d, view.MinTallModeHeight);

                view.MinWideModeWidth = -1;
                view.MinTallModeHeight = double.NaN;

                Assert.AreEqual(0d, view.MinWideModeWidth);
                Assert.AreEqual(0d, view.MinTallModeHeight);
            });
        }

        [TestMethod]
        public void WideModesTrackThresholdConfigurationAndPaneLengths()
        {
            WpfTestHost.Run(() =>
            {
                var view = new ModernWpf.Controls.TwoPaneView
                {
                    Width = 800,
                    Height = 400,
                    Pane1 = new Border(),
                    Pane2 = new Border(),
                    Pane1Length = new GridLength(240),
                    Pane2Length = new GridLength(1, GridUnitType.Star)
                };

                using var host = new TestWindowHost(view, width: 820, height: 420);
                host.UpdateLayout();

                Assert.AreEqual(TwoPaneViewMode.Wide, view.Mode);
                Assert.AreEqual(new GridLength(240), Part<ColumnDefinition>(view, "PART_ColumnLeft").Width);
                Assert.AreEqual(new GridLength(1, GridUnitType.Star), Part<ColumnDefinition>(view, "PART_ColumnRight").Width);
                Assert.AreEqual(0, Grid.GetColumn(Part<ScrollViewer>(view, "PART_Pane1ScrollViewer")));
                Assert.AreEqual(2, Grid.GetColumn(Part<ScrollViewer>(view, "PART_Pane2ScrollViewer")));

                view.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;
                host.UpdateLayout();

                Assert.AreEqual(TwoPaneViewMode.Wide, view.Mode);
                Assert.AreEqual(new GridLength(1, GridUnitType.Star), Part<ColumnDefinition>(view, "PART_ColumnLeft").Width);
                Assert.AreEqual(new GridLength(240), Part<ColumnDefinition>(view, "PART_ColumnRight").Width);
                Assert.AreEqual(2, Grid.GetColumn(Part<ScrollViewer>(view, "PART_Pane1ScrollViewer")));
                Assert.AreEqual(0, Grid.GetColumn(Part<ScrollViewer>(view, "PART_Pane2ScrollViewer")));

                var exactWidth = view.ActualWidth;
                view.MinWideModeWidth = exactWidth;
                host.UpdateLayout();
                WpfTestHost.DoEvents();
                host.UpdateLayout();

                Assert.AreEqual(exactWidth, view.MinWideModeWidth);
                Assert.AreEqual(TwoPaneViewMode.SinglePane, view.Mode);

                view.MinWideModeWidth = exactWidth - 0.5;
                host.UpdateLayout();
                Assert.AreEqual(TwoPaneViewMode.Wide, view.Mode);
            });
        }

        [TestMethod]
        public void TallModesAndSinglePanePriorityMatchCurrentWinUI()
        {
            WpfTestHost.Run(() =>
            {
                var view = new ModernWpf.Controls.TwoPaneView
                {
                    Width = 400,
                    Height = 800,
                    Pane1 = new Border(),
                    Pane2 = new Border(),
                    Pane1Length = new GridLength(200),
                    Pane2Length = new GridLength(1, GridUnitType.Star)
                };

                using var host = new TestWindowHost(view, width: 420, height: 820);
                host.UpdateLayout();

                Assert.AreEqual(TwoPaneViewMode.Tall, view.Mode);
                Assert.AreEqual(new GridLength(200), Part<RowDefinition>(view, "PART_RowTop").Height);
                Assert.AreEqual(new GridLength(1, GridUnitType.Star), Part<RowDefinition>(view, "PART_RowBottom").Height);
                Assert.AreEqual(0, Grid.GetRow(Part<ScrollViewer>(view, "PART_Pane1ScrollViewer")));
                Assert.AreEqual(2, Grid.GetRow(Part<ScrollViewer>(view, "PART_Pane2ScrollViewer")));

                view.TallModeConfiguration = TwoPaneViewTallModeConfiguration.BottomTop;
                host.UpdateLayout();

                Assert.AreEqual(TwoPaneViewMode.Tall, view.Mode);
                Assert.AreEqual(2, Grid.GetRow(Part<ScrollViewer>(view, "PART_Pane1ScrollViewer")));
                Assert.AreEqual(0, Grid.GetRow(Part<ScrollViewer>(view, "PART_Pane2ScrollViewer")));

                view.TallModeConfiguration = TwoPaneViewTallModeConfiguration.SinglePane;
                view.PanePriority = TwoPaneViewPriority.Pane2;
                host.UpdateLayout();

                Assert.AreEqual(TwoPaneViewMode.SinglePane, view.Mode);
                Assert.AreEqual(Visibility.Collapsed, Part<ScrollViewer>(view, "PART_Pane1ScrollViewer").Visibility);
                Assert.AreEqual(Visibility.Visible, Part<ScrollViewer>(view, "PART_Pane2ScrollViewer").Visibility);
            });
        }

        [TestMethod]
        public void ModeChangedOnlyReportsPublicModeTransitions()
        {
            WpfTestHost.Run(() =>
            {
                var view = new ModernWpf.Controls.TwoPaneView
                {
                    Width = 400,
                    Height = 400,
                    Pane1 = new Border(),
                    Pane2 = new Border()
                };
                var modeChangedCount = 0;
                object lastArgs = new object();
                view.ModeChanged += (sender, args) =>
                {
                    Assert.AreSame(view, sender);
                    modeChangedCount++;
                    lastArgs = args;
                };

                using var host = new TestWindowHost(view, width: 820, height: 420);
                host.UpdateLayout();

                Assert.AreEqual(TwoPaneViewMode.SinglePane, view.Mode);
                Assert.AreEqual(0, modeChangedCount);

                view.Width = 800;
                host.UpdateLayout();
                Assert.AreEqual(TwoPaneViewMode.Wide, view.Mode);
                Assert.AreEqual(1, modeChangedCount);
                Assert.AreSame(view, lastArgs);

                view.WideModeConfiguration = TwoPaneViewWideModeConfiguration.RightLeft;
                host.UpdateLayout();
                Assert.AreEqual(1, modeChangedCount);

                view.Width = 400;
                host.UpdateLayout();
                Assert.AreEqual(TwoPaneViewMode.SinglePane, view.Mode);
                Assert.AreEqual(2, modeChangedCount);
            });
        }

        private static T Part<T>(ModernWpf.Controls.TwoPaneView view, string name)
            where T : DependencyObject
        {
            var part = view.Template.FindName(name, view) as T;
            Assert.IsNotNull(part, $"Expected template part '{name}'.");
            return part;
        }
    }
}
