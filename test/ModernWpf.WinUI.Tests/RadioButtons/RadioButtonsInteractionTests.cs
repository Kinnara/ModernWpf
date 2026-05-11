using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

    [TestMethod]
    public void BasicKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            using var host = new TestWindowHost(radioButtons, width: 320, height: 360);

            SelectItem(radioButtons, 3);
            AssertSelectedFocusedIndex(radioButtons, 3);

            RaiseKey(GetRadioButton(radioButtons, 3), Key.Down);
            AssertSelectedFocusedIndex(radioButtons, 4);

            RaiseKey(GetRadioButton(radioButtons, 4), Key.Up);
            AssertSelectedFocusedIndex(radioButtons, 3);

            RaiseKey(GetRadioButton(radioButtons, 3), Key.Left);
            AssertSelectedFocusedIndex(radioButtons, 3);

            RaiseKey(GetRadioButton(radioButtons, 3), Key.Right);
            AssertSelectedFocusedIndex(radioButtons, 3);

            SelectItem(radioButtons, 0);
            AssertSelectedFocusedIndex(radioButtons, 0);

            RaiseKey(GetRadioButton(radioButtons, 0), Key.Up);
            AssertSelectedFocusedIndex(radioButtons, 0);

            SelectItem(radioButtons, 9);
            AssertSelectedFocusedIndex(radioButtons, 9);

            RaiseKey(GetRadioButton(radioButtons, 9), Key.Down);
            AssertSelectedFocusedIndex(radioButtons, 9);
        });
    }

    [TestMethod]
    public void MultiColumnKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            radioButtons.MaxColumns = 3;
            using var host = new TestWindowHost(radioButtons, width: 520, height: 360);

            SelectItem(radioButtons, 3);
            AssertSelectedFocusedIndex(radioButtons, 3);

            PressKeyAndAssert(radioButtons, Key.Down, 4);
            PressKeyAndAssert(radioButtons, Key.Up, 3);
            PressKeyAndAssert(radioButtons, Key.Left, 3);
            PressKeyAndAssert(radioButtons, Key.Right, 6);
            PressKeyAndAssert(radioButtons, Key.Right, 9);
            PressKeyAndAssert(radioButtons, Key.Right, 9);
            PressKeyAndAssert(radioButtons, Key.Left, 6);
            PressKeyAndAssert(radioButtons, Key.Left, 2);
            PressKeyAndAssert(radioButtons, Key.Left, 2);

            SelectItem(radioButtons, 0);
            AssertSelectedFocusedIndex(radioButtons, 0);

            PressKeyAndAssert(radioButtons, Key.Up, 0);
            PressKeyAndAssert(radioButtons, Key.Left, 0);
            PressKeyAndAssert(radioButtons, Key.Right, 4);

            SelectItem(radioButtons, 9);
            AssertSelectedFocusedIndex(radioButtons, 9);

            PressKeyAndAssert(radioButtons, Key.Down, 9);
            PressKeyAndAssert(radioButtons, Key.Right, 9);
            PressKeyAndAssert(radioButtons, Key.Left, 6);
        });
    }

    [TestMethod]
    public void SingleRowKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(3);
            radioButtons.MaxColumns = 3;
            using var host = new TestWindowHost(radioButtons, width: 520, height: 180);

            SelectItem(radioButtons, 0);
            AssertSelectedFocusedIndex(radioButtons, 0);

            PressKeyAndAssert(radioButtons, Key.Right, 1);
            PressKeyAndAssert(radioButtons, Key.Right, 2);
            PressKeyAndAssert(radioButtons, Key.Left, 1);
            PressKeyAndAssert(radioButtons, Key.Left, 0);
            PressKeyAndAssert(radioButtons, Key.Down, 1);
            PressKeyAndAssert(radioButtons, Key.Down, 2);
            PressKeyAndAssert(radioButtons, Key.Up, 1);
            PressKeyAndAssert(radioButtons, Key.Up, 0);
        });
    }

    private static ModernWpf.Controls.RadioButtons CreateRadioButtons(int itemCount)
    {
        var radioButtons = new ModernWpf.Controls.RadioButtons();
        for (var i = 0; i < itemCount; i++)
        {
            radioButtons.Items.Add(new RadioButton { Content = $"Radio Button {i}" });
        }

        return radioButtons;
    }

    private static void SelectItem(ModernWpf.Controls.RadioButtons radioButtons, int index)
    {
        var item = GetRadioButton(radioButtons, index);
        item.Focus();
        item.IsChecked = true;
        WpfTestHost.DoEvents();
    }

    private static void AssertSelectedFocusedIndex(ModernWpf.Controls.RadioButtons radioButtons, int index)
    {
        var item = GetRadioButton(radioButtons, index);
        Assert.AreEqual(index, radioButtons.SelectedIndex);
        Assert.AreSame(item, radioButtons.SelectedItem);
        Assert.AreEqual(true, item.IsChecked);
        Assert.IsTrue(item.IsKeyboardFocused, $"Expected item {index} to have keyboard focus.");
    }

    private static void PressKeyAndAssert(ModernWpf.Controls.RadioButtons radioButtons, Key key, int expectedIndex)
    {
        RaiseKey(GetRadioButton(radioButtons, radioButtons.SelectedIndex), key);
        AssertSelectedFocusedIndex(radioButtons, expectedIndex);
    }

    private static void RaiseKey(UIElement element, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            System.Environment.TickCount,
            key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        element.RaiseEvent(args);
        WpfTestHost.DoEvents();
    }

    private static RadioButton GetRadioButton(ModernWpf.Controls.RadioButtons radioButtons, int index)
    {
        var radioButton = radioButtons.ContainerFromIndex(index) as RadioButton;
        Assert.IsNotNull(radioButton);
        return radioButton!;
    }
}
