using System.Windows.Automation.Peers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ProgressRing;

[TestClass]
public class ProgressRingInteractionTests
{
    [TestMethod]
    public void VerifyIndeterminateProgressRingDoesNotImplementRangeValuePattern()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = new ModernWpf.Controls.ProgressRing
            {
                IsActive = true,
                Width = 48,
                Height = 48
            };

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
            Assert.IsNotNull(peer);
            Assert.IsTrue(peer!.IsControlElement());
            Assert.IsNull(peer.GetPattern(PatternInterface.RangeValue));
        });
    }
}
