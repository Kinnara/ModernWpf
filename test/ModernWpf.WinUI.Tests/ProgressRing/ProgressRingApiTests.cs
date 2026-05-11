using System.Windows.Automation.Peers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ProgressRing;

[TestClass]
public class ProgressRingApiTests
{
    [TestMethod]
    public void VerifyDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = new ModernWpf.Controls.ProgressRing();

            Assert.IsTrue(progressRing.IsActive);
            Assert.IsTrue(progressRing.IsIndeterminate);
            Assert.AreEqual(0.0, progressRing.Minimum);
            Assert.AreEqual(100.0, progressRing.Maximum);
            Assert.AreEqual(0.0, progressRing.Value);
        });
    }

    [TestMethod]
    public void VerifyAccessibilityView()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = new ModernWpf.Controls.ProgressRing
            {
                IsActive = true
            };

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
            Assert.IsTrue(peer.IsControlElement());

            progressRing.IsActive = false;
            Assert.IsFalse(peer.IsControlElement());
        });
    }
}
