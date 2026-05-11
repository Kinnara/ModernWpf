using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.DropDownButton;

[TestClass]
public class DropDownButtonInteractionTests
{
    [TestMethod]
    public void AccessibilityTest()
    {
        WpfTestHost.Run(() =>
        {
            var dropDownButton = new ModernWpf.Controls.DropDownButton
            {
                Content = "TestDropDownButton",
                Width = 180,
                Height = 36
            };
            var root = new Grid { Width = 400, Height = 240 };
            root.Children.Add(dropDownButton);

            using var host = new TestWindowHost(root, width: 400, height: 240);
            var firstFlyout = CreateCountingFlyout("TestFlyout");
            dropDownButton.Flyout = firstFlyout.Flyout;

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(dropDownButton);
            Assert.IsNotNull(peer);
            Assert.AreEqual(nameof(ModernWpf.Controls.DropDownButton), peer.GetClassName());

            if (peer.GetPattern(PatternInterface.ExpandCollapse) is not IExpandCollapseProvider provider)
            {
                Assert.Fail("DropDownButton should expose IExpandCollapseProvider.");
                return;
            }

            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);

            provider.Expand();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, firstFlyout.OpenedCount);
            Assert.AreEqual(0, firstFlyout.ClosedCount);
            Assert.IsTrue(firstFlyout.Flyout.IsOpen);
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, firstFlyout.OpenedCount);
            Assert.AreEqual(1, firstFlyout.ClosedCount);
            Assert.IsFalse(firstFlyout.Flyout.IsOpen);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);

            var secondFlyout = CreateCountingFlyout("ReplacementFlyout");
            dropDownButton.Flyout = secondFlyout.Flyout;

            provider.Expand();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, firstFlyout.OpenedCount);
            Assert.AreEqual(1, firstFlyout.ClosedCount);
            Assert.AreEqual(1, secondFlyout.OpenedCount);
            Assert.AreEqual(0, secondFlyout.ClosedCount);
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, secondFlyout.OpenedCount);
            Assert.AreEqual(1, secondFlyout.ClosedCount);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);
        });
    }

    private static CountingFlyout CreateCountingFlyout(string name)
    {
        var flyout = new Flyout
        {
            Content = new TextBlock
            {
                Name = name,
                Text = name,
                MinWidth = 120,
                MinHeight = 32
            }
        };
        var countingFlyout = new CountingFlyout(flyout);
        flyout.Opened += (sender, args) => countingFlyout.OpenedCount++;
        flyout.Closed += (sender, args) => countingFlyout.ClosedCount++;
        return countingFlyout;
    }

    private sealed class CountingFlyout
    {
        public CountingFlyout(Flyout flyout)
        {
            Flyout = flyout;
        }

        public Flyout Flyout { get; }

        public int OpenedCount { get; set; }

        public int ClosedCount { get; set; }
    }
}
