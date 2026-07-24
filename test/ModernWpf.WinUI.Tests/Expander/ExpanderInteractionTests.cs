using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using WpfExpander = System.Windows.Controls.Expander;

namespace ModernWpf.WinUI.Tests.Expander;

[TestClass]
public class ExpanderInteractionTests
{
    [TestMethod]
    public void ExpandCollapseAutomationTests()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var expander = new WpfExpander
            {
                Header = "ExpandedExpander",
                Content = new Button { Content = "Content" },
                IsExpanded = true,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            using var host = new TestWindowHost(expander, width: 400, height: 240);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(expander);
            Assert.IsNotNull(peer);
            Assert.AreEqual("Expander", peer!.GetClassName());

            if (peer.GetPattern(PatternInterface.ExpandCollapse) is not IExpandCollapseProvider provider)
            {
                Assert.Fail("Expander should expose IExpandCollapseProvider.");
                return;
            }

            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            host.UpdateLayout();
            Assert.IsFalse(expander.IsExpanded);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);

            provider.Expand();
            host.UpdateLayout();
            Assert.IsTrue(expander.IsExpanded);
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);
        });
    }
}
