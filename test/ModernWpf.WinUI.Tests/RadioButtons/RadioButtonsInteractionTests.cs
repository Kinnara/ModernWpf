using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RadioButtons;

[TestClass]
public class RadioButtonsInteractionTests
{
    [TestMethod]
    public void SelectionTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = new ModernWpf.Controls.RadioButtons();
            for (var i = 0; i < 5; i++)
            {
                radioButtons.Items.Add(new RadioButton { Content = $"Radio Button {i}" });
            }

            using var host = new TestWindowHost(radioButtons, width: 320, height: 240);

            radioButtons.SelectedIndex = 1;
            host.UpdateLayout();

            var item1 = GetRadioButton(radioButtons, 1);
            Assert.AreEqual(1, radioButtons.SelectedIndex);
            Assert.AreSame(item1, radioButtons.SelectedItem);
            Assert.AreEqual(true, item1.IsChecked);

            var item3 = GetRadioButton(radioButtons, 3);
            item3.IsChecked = true;
            WpfTestHost.DoEvents();

            Assert.AreEqual(3, radioButtons.SelectedIndex);
            Assert.AreSame(item3, radioButtons.SelectedItem);
            Assert.AreEqual(true, item3.IsChecked);
            Assert.AreEqual(false, item1.IsChecked);
        });
    }

    [TestMethod]
    public void SelectByItem()
    {
        WpfTestHost.Run(() =>
        {
            var item0 = new RadioButton { Content = "Radio Button 0" };
            var item1 = new RadioButton { Content = "Radio Button 1" };
            var item2 = new RadioButton { Content = "Radio Button 2" };
            var item3 = new RadioButton { Content = "Radio Button 3" };
            var item4 = new RadioButton { Content = "Radio Button 4" };
            var items = new List<RadioButton> { item0, item1, item2, item3, item4 };

            var radioButtons = new ModernWpf.Controls.RadioButtons
            {
                ItemsSource = items
            };

            using var host = new TestWindowHost(radioButtons, width: 320, height: 240);

            radioButtons.SelectedItem = item1;
            host.UpdateLayout();

            Assert.AreEqual(1, radioButtons.SelectedIndex);
            Assert.AreSame(item1, radioButtons.SelectedItem);
            Assert.AreEqual(true, item1.IsChecked);

            radioButtons.SelectedItem = item3;
            host.UpdateLayout();

            Assert.AreEqual(3, radioButtons.SelectedIndex);
            Assert.AreSame(item3, radioButtons.SelectedItem);
            Assert.AreEqual(true, item3.IsChecked);
            Assert.AreEqual(false, item1.IsChecked);
        });
    }

    private static RadioButton GetRadioButton(ModernWpf.Controls.RadioButtons radioButtons, int index)
    {
        var radioButton = radioButtons.ContainerFromIndex(index) as RadioButton;
        Assert.IsNotNull(radioButton);
        return radioButton!;
    }
}
