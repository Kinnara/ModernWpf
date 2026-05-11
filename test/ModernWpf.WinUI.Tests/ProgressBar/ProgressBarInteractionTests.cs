using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;
using ProgressBar = ModernWpf.Controls.ProgressBar;

namespace ModernWpf.WinUI.Tests.ProgressBars;

[TestClass]
public class ProgressBarInteractionTests
{
    [TestMethod]
    public void ChangeValueTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);
            var changeValueButton = new Button { Content = "ChangeValue" };
            changeValueButton.Click += (sender, args) => progressBar.Value += 25;

            var root = new StackPanel();
            root.Children.Add(progressBar);
            root.Children.Add(changeValueButton);

            using var host = new TestWindowHost(root, width: 320, height: 180);

            var provider = GetRangeValueProvider(progressBar);
            Assert.AreEqual(0.0, provider.Value);

            var oldValue = provider.Value;
            var invokeProvider = (IInvokeProvider)FrameworkElementAutomationPeer
                .CreatePeerForElement(changeValueButton)
                .GetPattern(PatternInterface.Invoke);

            invokeProvider.Invoke();
            host.UpdateLayout();

            var newValue = provider.Value;
            Assert.IsTrue(newValue > oldValue);

            var indicator = FindNamedDescendant<Rectangle>(progressBar, "DeterminateProgressBarIndicator");
            Assert.IsTrue(indicator.Width > 0.0);
        });
    }

    [TestMethod]
    public void UpdateIndicatorWidthTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            progressBar.Value = 50;
            host.UpdateLayout();
            AssertIndicatorWidth(progressBar, 50.0);

            progressBar.Width = 200;
            host.UpdateLayout();
            AssertIndicatorWidth(progressBar, 100.0);

            progressBar.Minimum = 10;
            progressBar.Maximum = 16;
            progressBar.Value = 13;
            host.UpdateLayout();
            AssertIndicatorWidth(progressBar, 100.0);
        });
    }

    [TestMethod]
    public void UpdateMinMaxTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            var provider = GetRangeValueProvider(progressBar);
            var oldMinimum = provider.Minimum;
            var oldMaximum = provider.Maximum;

            progressBar.Minimum = 10;
            progressBar.Maximum = 15;
            host.UpdateLayout();

            Assert.AreNotEqual(oldMinimum, provider.Minimum);
            Assert.AreNotEqual(oldMaximum, provider.Maximum);
            Assert.AreEqual(10.0, provider.Minimum);
            Assert.AreEqual(15.0, provider.Maximum);

            progressBar.Maximum = 5;
            host.UpdateLayout();
            Assert.AreEqual(provider.Minimum, provider.Maximum);

            progressBar.Minimum = 15;
            host.UpdateLayout();
            Assert.AreEqual(provider.Minimum, provider.Value);
            Assert.AreEqual(provider.Minimum, provider.Maximum);

            progressBar.Minimum = 0.1;
            progressBar.Maximum = 1.1;
            progressBar.Value = 0.1;
            host.UpdateLayout();

            var oldValue = provider.Value;
            progressBar.Value += 0.25;
            host.UpdateLayout();

            Assert.IsTrue(provider.Value > oldValue);
        });
    }

    [TestMethod]
    public void PaddingOffsetTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            progressBar.Padding = new Thickness(10, 0, 10, 0);
            progressBar.Value = 100;
            host.UpdateLayout();

            AssertIndicatorWidth(progressBar, 80.0);
        });
    }

    [TestMethod]
    public void IndeterminateProgressBarDoesNotImplementRangeValuePattern()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);
            progressBar.IsIndeterminate = true;

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressBar);
            Assert.IsNotNull(peer);
            Assert.IsNull(peer!.GetPattern(PatternInterface.RangeValue));
        });
    }

    private static ProgressBar CreateProgressBar(double width)
    {
        return new ProgressBar
        {
            Width = width,
            Height = 12,
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };
    }

    private static IRangeValueProvider GetRangeValueProvider(ProgressBar progressBar)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressBar);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.RangeValue) is IRangeValueProvider provider)
        {
            return provider;
        }

        Assert.Fail("ProgressBar should expose IRangeValueProvider when determinate.");
        throw new InvalidOperationException();
    }

    private static void AssertIndicatorWidth(ProgressBar progressBar, double expected)
    {
        var indicator = FindNamedDescendant<Rectangle>(progressBar, "DeterminateProgressBarIndicator");
        Assert.AreEqual(expected, indicator.Width, 0.5);
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
