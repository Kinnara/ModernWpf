using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;
using ProgressBar = ModernWpf.Controls.ProgressBar;

namespace ModernWpf.WinUI.Tests.ProgressBars;

[TestClass]
public class ProgressBarApiTests
{
    [TestMethod]
    public void ResourceOverridability()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new Grid();
            grid.Resources["ProgressBarTrackHeight"] = 3.0;

            var overriddenProgressBar = new ProgressBar();
            grid.Children.Add(overriddenProgressBar);

            var defaultProgressBar = new ProgressBar();
            var root = new StackPanel();
            root.Children.Add(grid);
            root.Children.Add(defaultProgressBar);

            using var host = new TestWindowHost(root);

            var overriddenTrack = FindNamedDescendant<Rectangle>(overriddenProgressBar, "ProgressBarTrack");
            Assert.AreEqual(3.0, overriddenTrack.Height);

            var defaultTrack = FindNamedDescendant<Rectangle>(defaultProgressBar, "ProgressBarTrack");
            Assert.AreEqual(1.0, defaultTrack.Height);
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
}
