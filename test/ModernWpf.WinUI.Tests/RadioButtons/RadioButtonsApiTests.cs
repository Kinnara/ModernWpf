using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RadioButtons;

[TestClass]
public class RadioButtonsApiTests
{
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
}
