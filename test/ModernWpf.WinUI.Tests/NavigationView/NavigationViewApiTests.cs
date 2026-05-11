using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NavigationView;

[TestClass]
public class NavigationViewApiTests
{
    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var footer = new Rectangle
            {
                Height = 40
            };

            var header = new TextBlock
            {
                Text = "Header"
            };

            var paneToggleButtonStyle = new Style();
            var navView = new ModernWpf.Controls.NavigationView();

            Assert.IsTrue(navView.IsPaneOpen);
            Assert.AreEqual(641, navView.CompactModeThresholdWidth);
            Assert.AreEqual(1008, navView.ExpandedModeThresholdWidth);
            Assert.IsNull(navView.PaneFooter);
            Assert.IsNull(navView.Header);
            Assert.IsTrue(navView.IsSettingsVisible);
            Assert.IsTrue(navView.IsPaneToggleButtonVisible);
            Assert.IsTrue(navView.IsTitleBarAutoPaddingEnabled);
            Assert.IsTrue(navView.AlwaysShowHeader);
            Assert.AreEqual(48, navView.CompactPaneLength);
            Assert.AreEqual(320, navView.OpenPaneLength);
            Assert.IsNull(navView.PaneToggleButtonStyle);
            Assert.AreEqual(0, navView.MenuItems.Count);
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Minimal, navView.DisplayMode);
            Assert.AreEqual(string.Empty, navView.PaneTitle);
            Assert.IsFalse(navView.IsBackEnabled);
            Assert.AreEqual(ModernWpf.Controls.NavigationViewBackButtonVisible.Auto, navView.IsBackButtonVisible);

            navView.IsPaneOpen = true;
            navView.CompactModeThresholdWidth = 500;
            navView.ExpandedModeThresholdWidth = 1000;
            navView.PaneFooter = footer;
            navView.Header = header;
            navView.IsSettingsVisible = false;
            navView.IsPaneToggleButtonVisible = false;
            navView.IsTitleBarAutoPaddingEnabled = false;
            navView.AlwaysShowHeader = false;
            navView.CompactPaneLength = 40;
            navView.OpenPaneLength = 300;
            navView.PaneToggleButtonStyle = paneToggleButtonStyle;
            navView.PaneTitle = "ChangedTitle";
            navView.IsBackEnabled = true;
            navView.IsBackButtonVisible = ModernWpf.Controls.NavigationViewBackButtonVisible.Visible;

            Assert.IsTrue(navView.IsPaneOpen);
            Assert.AreEqual(500, navView.CompactModeThresholdWidth);
            Assert.AreEqual(1000, navView.ExpandedModeThresholdWidth);
            Assert.AreSame(footer, navView.PaneFooter);
            Assert.AreSame(header, navView.Header);
            Assert.IsFalse(navView.IsSettingsVisible);
            Assert.IsFalse(navView.IsPaneToggleButtonVisible);
            Assert.IsFalse(navView.IsTitleBarAutoPaddingEnabled);
            Assert.IsFalse(navView.AlwaysShowHeader);
            Assert.AreEqual(40, navView.CompactPaneLength);
            Assert.AreEqual(300, navView.OpenPaneLength);
            Assert.AreSame(paneToggleButtonStyle, navView.PaneToggleButtonStyle);
            Assert.AreEqual("ChangedTitle", navView.PaneTitle);
            Assert.IsTrue(navView.IsBackEnabled);
            Assert.AreEqual(ModernWpf.Controls.NavigationViewBackButtonVisible.Visible, navView.IsBackButtonVisible);

            navView.PaneFooter = null;
            navView.Header = null;
            navView.PaneToggleButtonStyle = null;

            Assert.IsNull(navView.PaneFooter);
            Assert.IsNull(navView.Header);
            Assert.IsNull(navView.PaneToggleButtonStyle);
        });
    }

    [TestMethod]
    public void VerifyValuesCoercion()
    {
        WpfTestHost.Run(() =>
        {
            var navView = new ModernWpf.Controls.NavigationView
            {
                CompactModeThresholdWidth = -1,
                ExpandedModeThresholdWidth = -1,
                CompactPaneLength = -1,
                OpenPaneLength = -1
            };

            Assert.AreEqual(0, navView.CompactModeThresholdWidth);
            Assert.AreEqual(0, navView.ExpandedModeThresholdWidth);
            Assert.AreEqual(0, navView.CompactPaneLength);
            Assert.AreEqual(0, navView.OpenPaneLength);
        });
    }

    [TestMethod]
    public void VerifyPaneProperties()
    {
        WpfTestHost.Run(() =>
        {
            var navView = new ModernWpf.Controls.NavigationView
            {
                IsPaneOpen = false,
                CompactPaneLength = 100.0,
                OpenPaneLength = 200.0
            };

            Assert.IsFalse(navView.IsPaneOpen);
            Assert.AreEqual(100.0, navView.CompactPaneLength);
            Assert.AreEqual(200.0, navView.OpenPaneLength);

            navView.IsPaneOpen = true;
            Assert.IsTrue(navView.IsPaneOpen);
        });
    }
}
