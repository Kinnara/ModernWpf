using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RadioButtons;

[TestClass]
public class RadioButtonsApiTests
{
    [TestMethod]
    public void VerifyHeaderPresenterMatchesWinUITemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var headerTemplate = (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <TextBlock Text='{Binding}'/>
                </DataTemplate>");
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                Header = "RadioButtons header",
                HeaderTemplate = headerTemplate,
                ItemsSource = new List<string> { "Option 1", "Option 2" }
            };

            using var host = new TestWindowHost(radioButtons);
            host.UpdateLayout();

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(radioButtons, "HeaderContentPresenter");
            Assert.AreEqual("RadioButtons header", headerPresenter.Content);
            Assert.AreSame(headerTemplate, headerPresenter.ContentTemplate);
            AssertBrushEquals((Brush)headerPresenter.TryFindResource("RadioButtonsHeaderForeground"), headerPresenter.Foreground);

            radioButtons.IsEnabled = false;
            host.UpdateLayout();

            AssertBrushEquals((Brush)headerPresenter.TryFindResource("RadioButtonsHeaderForegroundDisabled"), headerPresenter.Foreground);
        });
    }

    [TestMethod]
    public void VerifyCustomItemTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "Option 1", "Option 2" },
                ItemTemplate = (DataTemplate)XamlReader.Parse(
                    @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                        <TextBlock Text='{Binding}'/>
                    </DataTemplate>")
            };

            var radioButtons2 = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = new List<string> { "Option 1", "Option 2" },
                ItemTemplate = (DataTemplate)XamlReader.Parse(
                    @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                        <RadioButton Foreground='Blue'>
                            <TextBlock Text='{Binding}'/>
                        </RadioButton>
                    </DataTemplate>")
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(radioButtons);
            stackPanel.Children.Add(radioButtons2);

            using var host = new TestWindowHost(stackPanel);

            var radioButton1 = radioButtons.ContainerFromIndex(0) as RadioButton;
            var radioButton2 = radioButtons2.ContainerFromIndex(0) as RadioButton;

            Assert.IsNotNull(radioButton1, "Our custom ItemTemplate should have been wrapped in a RadioButton.");
            Assert.IsNotNull(radioButton2, "Our custom ItemTemplate should have been wrapped in a RadioButton.");
            Assert.IsFalse(IsBlue(radioButton1!.Foreground), "Default foreground color of the RadioButton should not have been [blue].");
            Assert.IsTrue(IsBlue(radioButton2!.Foreground), "The foreground color of the RadioButton should have been [blue].");
        });
    }

    [TestMethod]
    public void VerifyIsEnabledChangeUpdatesVisualState()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                IsEnabled = true
            };

            using var host = new TestWindowHost(radioButtons);

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(radioButtons, 0);
            var commonStatesGroup = VisualStateManager.GetVisualStateGroups(layoutRoot)
                .OfType<VisualStateGroup>()
                .Single(group => group.Name == "CommonStates");

            Assert.AreEqual("Normal", commonStatesGroup.CurrentState.Name);

            radioButtons.IsEnabled = false;
            host.UpdateLayout();
            Assert.AreEqual("Disabled", commonStatesGroup.CurrentState.Name);

            radioButtons.IsEnabled = true;
            host.UpdateLayout();
            Assert.AreEqual("Normal", commonStatesGroup.CurrentState.Name);
        });
    }

    private static bool IsBlue(Brush brush)
    {
        return brush is SolidColorBrush solidColorBrush && solidColorBrush.Color == Colors.Blue;
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
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

        throw new AssertFailedException($"Could not find descendant named '{name}'.");
    }
}
