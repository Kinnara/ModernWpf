using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SplitView;

[TestClass]
public class SplitViewApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView();

            Assert.IsFalse(splitView.IsPaneOpen);
            Assert.AreEqual(SplitViewDisplayMode.Overlay, splitView.DisplayMode);
            Assert.AreEqual(SplitViewPanePlacement.Left, splitView.PanePlacement);
            Assert.AreEqual(LightDismissOverlayMode.Auto, splitView.LightDismissOverlayMode);
            Assert.AreEqual(0d, splitView.OpenPaneLength);
            Assert.AreEqual(0d, splitView.CompactPaneLength);
            Assert.IsNull(splitView.Pane);
            Assert.IsNull(splitView.Content);
            Assert.IsNull(splitView.PaneBackground);
            Assert.IsNotNull(splitView.TemplateSettings);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border();
            var pane = new Border();
            var paneBackground = new SolidColorBrush(Colors.Red);
            var splitView = new ModernWpf.Controls.SplitView
            {
                Content = content,
                Pane = pane,
                DisplayMode = SplitViewDisplayMode.CompactInline,
                PanePlacement = SplitViewPanePlacement.Right,
                LightDismissOverlayMode = LightDismissOverlayMode.On,
                OpenPaneLength = 296,
                CompactPaneLength = 48,
                PaneBackground = paneBackground
            };

            Assert.AreSame(content, splitView.Content);
            Assert.AreSame(pane, splitView.Pane);
            Assert.AreEqual(SplitViewDisplayMode.CompactInline, splitView.DisplayMode);
            Assert.AreEqual(SplitViewPanePlacement.Right, splitView.PanePlacement);
            Assert.AreEqual(LightDismissOverlayMode.On, splitView.LightDismissOverlayMode);
            Assert.AreEqual(296d, splitView.OpenPaneLength);
            Assert.AreEqual(48d, splitView.CompactPaneLength);
            Assert.AreSame(paneBackground, splitView.PaneBackground);
        });
    }

    [TestMethod]
    public void TemplateSettingsTrackPaneLengths()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                OpenPaneLength = 296,
                CompactPaneLength = 48
            };

            SplitViewTemplateSettings settings = splitView.TemplateSettings;

            Assert.AreEqual(new GridLength(48), settings.CompactPaneGridLength);
            Assert.AreEqual(-296d, settings.NegativeOpenPaneLength);
            Assert.AreEqual(-248d, settings.NegativeOpenPaneLengthMinusCompactLength);
            Assert.AreEqual(new GridLength(296), settings.OpenPaneGridLength);
            Assert.AreEqual(296d, settings.OpenPaneLength);
            Assert.AreEqual(248d, settings.OpenPaneLengthMinusCompactLength);
        });
    }

    [TestMethod]
    public void PaneOpenCloseEventsFollowTestUiPattern()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView();
            var openingCount = 0;
            var openedCount = 0;
            var closingCount = 0;
            var closedCount = 0;

            splitView.PaneOpening += (sender, args) => openingCount++;
            splitView.PaneOpened += (sender, args) => openedCount++;
            splitView.PaneClosing += (sender, args) => closingCount++;
            splitView.PaneClosed += (sender, args) => closedCount++;

            splitView.IsPaneOpen = true;

            Assert.IsTrue(splitView.IsPaneOpen);
            Assert.AreEqual(1, openingCount);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(0, closingCount);
            Assert.AreEqual(0, closedCount);

            splitView.IsPaneOpen = false;

            Assert.IsFalse(splitView.IsPaneOpen);
            Assert.AreEqual(1, openingCount);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(1, closingCount);
            Assert.AreEqual(1, closedCount);
        });
    }

    [TestMethod]
    public void TestUiDisplayModeAndPanePlacementChangesAreApplied()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Inline,
                PanePlacement = SplitViewPanePlacement.Left,
                OpenPaneLength = 296
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);

            splitView.DisplayMode = SplitViewDisplayMode.CompactInline;
            splitView.PanePlacement = SplitViewPanePlacement.Right;
            host.UpdateLayout();

            Assert.AreEqual(SplitViewDisplayMode.CompactInline, splitView.DisplayMode);
            Assert.AreEqual(SplitViewPanePlacement.Right, splitView.PanePlacement);
            Assert.AreEqual(296d, splitView.TemplateSettings.OpenPaneLength);
        });
    }
}
