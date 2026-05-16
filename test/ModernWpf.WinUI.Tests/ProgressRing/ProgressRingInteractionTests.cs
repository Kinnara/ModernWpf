using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;
using ProgressRingControl = ModernWpf.Controls.ProgressRing;

namespace ModernWpf.WinUI.Tests.ProgressRing;

[TestClass]
public class ProgressRingInteractionTests
{
    [TestMethod]
    public void ChangeStateTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = CreateProgressRing();

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);

            AssertCurrentState(progressRing, "Active");
            AssertRingVisibility(progressRing, Visibility.Visible);
            AssertRingOpacity(progressRing, 1.0);

            progressRing.IsIndeterminate = false;
            host.UpdateLayout();

            AssertCurrentState(progressRing, "DeterminateActive");
            AssertRingVisibility(progressRing, Visibility.Visible);
            AssertRingOpacity(progressRing, 1.0);
            Assert.IsNotNull(GetRangeValueProvider(progressRing));

            progressRing.IsActive = false;
            host.UpdateLayout();

            AssertCurrentState(progressRing, "Inactive");
            AssertRingVisibility(progressRing, Visibility.Collapsed);
            AssertRingOpacity(progressRing, 0.0);
            Assert.IsNotNull(GetRangeValueProvider(progressRing));
        });
    }

    [TestMethod]
    public void ChangeValueTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = CreateProgressRing();
            progressRing.IsIndeterminate = false;

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);

            var provider = GetRangeValueProvider(progressRing);
            Assert.AreEqual(0.0, provider.Value);

            var oldValue = provider.Value;

            progressRing.Value += 25;
            host.UpdateLayout();

            Assert.IsTrue(provider.Value > oldValue);
            Assert.AreEqual(25.0, provider.Value);
        });
    }

    [TestMethod]
    public void UpdateMinMaxTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = CreateProgressRing();
            progressRing.IsIndeterminate = false;

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);

            var provider = GetRangeValueProvider(progressRing);
            var oldMinimum = provider.Minimum;
            var oldMaximum = provider.Maximum;

            progressRing.Minimum = 10;
            progressRing.Maximum = 15;
            host.UpdateLayout();

            Assert.AreNotEqual(oldMinimum, provider.Minimum);
            Assert.AreNotEqual(oldMaximum, provider.Maximum);
            Assert.AreEqual(10.0, provider.Minimum);
            Assert.AreEqual(15.0, provider.Maximum);

            progressRing.Maximum = 5;
            host.UpdateLayout();
            Assert.AreEqual(provider.Minimum, provider.Maximum);

            progressRing.Minimum = 15;
            host.UpdateLayout();
            Assert.AreEqual(provider.Minimum, provider.Value);
            Assert.AreEqual(provider.Minimum, provider.Maximum);

            progressRing.Minimum = 0.1;
            progressRing.Maximum = 1.1;
            progressRing.Value = 0.1;
            host.UpdateLayout();

            var oldValue = provider.Value;
            progressRing.Value += 0.25;
            host.UpdateLayout();

            Assert.IsTrue(provider.Value > oldValue);
        });
    }

    [TestMethod]
    public void VerifyIndeterminateProgressRingDoesNotImplementRangeValuePattern()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = CreateProgressRing();

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
            Assert.IsNotNull(peer);
            Assert.IsTrue(peer!.IsControlElement());
            Assert.IsNull(peer.GetPattern(PatternInterface.RangeValue));
        });
    }

    private static ProgressRingControl CreateProgressRing()
    {
        return new ProgressRingControl
        {
            Width = 48,
            Height = 48
        };
    }

    private static void AssertCurrentState(ProgressRingControl progressRing, string expectedStateName)
    {
        var activeStatesGroup = GetActiveStatesGroup(progressRing);
        Assert.IsNotNull(activeStatesGroup.CurrentState);
        Assert.AreEqual(expectedStateName, activeStatesGroup.CurrentState.Name);
    }

    private static VisualStateGroup GetActiveStatesGroup(ProgressRingControl progressRing)
    {
        var ring = FindNamedDescendant<Border>(progressRing, "Ring");
        return VisualStateManager.GetVisualStateGroups(ring)
            .OfType<VisualStateGroup>()
            .First(group => group.Name == "ActiveStates");
    }

    private static void AssertRingVisibility(ProgressRingControl progressRing, Visibility expectedVisibility)
    {
        var ring = FindNamedDescendant<Border>(progressRing, "Ring");
        Assert.AreEqual(expectedVisibility, ring.Visibility);
    }

    private static void AssertRingOpacity(ProgressRingControl progressRing, double expectedOpacity)
    {
        var ring = FindNamedDescendant<Border>(progressRing, "Ring");
        Assert.AreEqual(expectedOpacity, ring.Opacity);
    }

    private static IRangeValueProvider GetRangeValueProvider(ProgressRingControl progressRing)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.RangeValue) is IRangeValueProvider provider)
        {
            return provider;
        }

        Assert.Fail("ProgressRing should expose IRangeValueProvider when determinate.");
        throw new InvalidOperationException();
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
}
