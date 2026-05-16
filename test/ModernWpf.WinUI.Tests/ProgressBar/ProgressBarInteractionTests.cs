using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
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
    public void ChangeStateTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            AssertCurrentState(progressBar, "Determinate");

            progressBar.ShowPaused = true;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "Paused");

            progressBar.IsIndeterminate = true;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "IndeterminatePaused");

            progressBar.ShowPaused = false;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "Indeterminate");

            progressBar.ShowError = true;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "IndeterminateError");

            progressBar.IsIndeterminate = false;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "Error");
        });
    }

    [TestMethod]
    public void UpdatingErrorStateUsesNestedVisualStateSetter()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            var state = GetCommonStatesGroup(progressBar).States
                .OfType<VisualState>()
                .Single(candidate => candidate.Name == "UpdatingError");
            Assert.IsInstanceOfType(state, typeof(VisualStateEx));

            var stateEx = (VisualStateEx)state;
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == "DeterminateProgressBarIndicator.(Shape.Fill).(SolidColorBrush.Color)"),
                "UpdatingError should use the WinUI nested brush-color setter path.");

            var indicator = FindNamedDescendant<Rectangle>(progressBar, "DeterminateProgressBarIndicator");
            var initialColor = ((SolidColorBrush)indicator.Fill).Color;
            var expectedErrorColor = (Color)progressBar.TryFindResource("ProgressBarErrorForegroundColor");

            Assert.IsTrue(VisualStateManager.GoToState(progressBar, "UpdatingError", false));
            AssertSolidColorBrush(indicator.Fill, expectedErrorColor);

            Assert.IsTrue(VisualStateManager.GoToState(progressBar, "Determinate", false));
            AssertSolidColorBrush(indicator.Fill, initialColor);
        });
    }

    [TestMethod]
    public void RetemplateUpdateIndicatorWidthTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);
            progressBar.Template = CreateRetemplate();

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
    public void RetemplateChangeStateTest()
    {
        WpfTestHost.Run(() =>
        {
            var progressBar = CreateProgressBar(width: 100);
            progressBar.Template = CreateRetemplate();

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);

            AssertCurrentState(progressBar, "Determinate");

            progressBar.ShowPaused = true;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "Paused");

            progressBar.ShowPaused = false;
            progressBar.IsIndeterminate = true;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "Indeterminate");

            progressBar.IsIndeterminate = false;
            progressBar.ShowError = true;
            host.UpdateLayout();
            AssertCurrentState(progressBar, "Error");
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

    private static void AssertCurrentState(ProgressBar progressBar, string expectedStateName)
    {
        var commonStatesGroup = GetCommonStatesGroup(progressBar);
        Assert.IsNotNull(commonStatesGroup.CurrentState);
        Assert.AreEqual(expectedStateName, commonStatesGroup.CurrentState.Name);
    }

    private static VisualStateGroup GetCommonStatesGroup(ProgressBar progressBar)
    {
        var layoutRoot = FindNamedDescendant<Grid>(progressBar, "LayoutRoot");
        return VisualStateManager.GetVisualStateGroups(layoutRoot)
            .OfType<VisualStateGroup>()
            .First(group => group.Name == "CommonStates");
    }

    private static void AssertIndicatorWidth(ProgressBar progressBar, double expected)
    {
        var indicator = FindNamedDescendant<Rectangle>(progressBar, "DeterminateProgressBarIndicator");
        Assert.AreEqual(expected, indicator.Width, 0.5);
    }

    private static void AssertSolidColorBrush(Brush brush, Color expectedColor)
    {
        var solidColorBrush = brush as SolidColorBrush
            ?? throw new AssertFailedException("Expected a SolidColorBrush.");
        Assert.AreEqual(expectedColor, solidColorBrush.Color);
    }

    private static ControlTemplate CreateRetemplate()
    {
        const string templateXaml =
            """
            <ControlTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                TargetType="{x:Type controls:ProgressBar}">
                <Grid x:Name="LayoutRoot">
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name="CommonStates">
                            <VisualState x:Name="Determinate" />
                            <VisualState x:Name="Updating" />
                            <VisualState x:Name="UpdatingError" />
                            <VisualState x:Name="Error" />
                            <VisualState x:Name="Paused" />
                            <VisualState x:Name="Indeterminate" />
                            <VisualState x:Name="IndeterminateError" />
                            <VisualState x:Name="IndeterminatePaused" />
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>
                    <Rectangle
                        x:Name="DeterminateProgressBarIndicator"
                        Fill="{TemplateBinding Foreground}"
                        HorizontalAlignment="Left" />
                    <Rectangle
                        x:Name="IndeterminateProgressBarIndicator"
                        Fill="{TemplateBinding Foreground}"
                        HorizontalAlignment="Left"
                        Opacity="0" />
                    <Rectangle
                        x:Name="IndeterminateProgressBarIndicator2"
                        Fill="{TemplateBinding Foreground}"
                        HorizontalAlignment="Left"
                        Opacity="0" />
                </Grid>
            </ControlTemplate>
            """;

        return (ControlTemplate)XamlReader.Parse(templateXaml);
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
