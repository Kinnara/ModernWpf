using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
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
    public void FocusComingFromAnotherRepeaterTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons1 = CreateRadioButtons(4);
            var radioButtons2 = CreateRadioButtons(4);
            var panel = new StackPanel();
            panel.Children.Add(radioButtons1);
            panel.Children.Add(radioButtons2);

            using var host = new TestWindowHost(panel, width: 320, height: 360);

            SelectItem(radioButtons1, 1);
            SelectItem(radioButtons2, 2);

            AssertSelectedFocusedIndex(radioButtons2, 2);

            GetRadioButton(radioButtons1, 0).Focus();
            WpfTestHost.DoEvents();
            AssertSelectedFocusedIndex(radioButtons1, 1);

            GetRadioButton(radioButtons2, 0).Focus();
            WpfTestHost.DoEvents();
            AssertSelectedFocusedIndex(radioButtons2, 2);
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
            var radioButtons = CreateRadioButtons(10, compactContent: true);
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

    [TestMethod]
    public void DisabledItemsKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            radioButtons.MaxColumns = 3;
            using var host = new TestWindowHost(radioButtons, width: 520, height: 420);

            InsertDisabledRadioButton(radioButtons, 10);
            SelectItemByLabel(radioButtons, 7);
            AssertSelectedFocusedIndex(radioButtons, 7);

            PressKeyAndAssert(radioButtons, Key.Right, 9);
            PressKeyAndAssert(radioButtons, Key.Left, 5);
            PressKeyAndAssert(radioButtons, Key.Down, 6);
            PressKeyAndAssert(radioButtons, Key.Right, 9);

            InsertDisabledRadioButton(radioButtons, 6);
            InsertDisabledRadioButton(radioButtons, 6);

            SelectItemByLabel(radioButtons, 1);
            AssertSelectedFocusedIndex(radioButtons, 1);
            PressKeyAndAssert(radioButtons, Key.Right, 10);

            SelectItemByLabel(radioButtons, 2);
            AssertSelectedFocusedIndex(radioButtons, 2);
            PressKeyAndAssert(radioButtons, Key.Right, 11);

            SelectItemByLabel(radioButtons, 5);
            PressKeyAndAssert(radioButtons, Key.Up, 4);
            PressKeyAndAssert(radioButtons, Key.Down, 5);
            PressKeyAndAssert(radioButtons, Key.Down, 8);

            SelectItemByLabel(radioButtons, 8);
            AssertSelectedFocusedIndex(radioButtons, 10);
            PressKeyAndAssert(radioButtons, Key.Left, 1);

            SelectItemByLabel(radioButtons, 9);
            AssertSelectedFocusedIndex(radioButtons, 11);
            PressKeyAndAssert(radioButtons, Key.Left, 2);
        });
    }

    [TestMethod]
    public void DisabledItemsAtTopOfColumnKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            radioButtons.MaxColumns = 3;
            using var host = new TestWindowHost(radioButtons, width: 520, height: 420);

            InsertDisabledRadioButton(radioButtons, 5);
            InsertDisabledRadioButton(radioButtons, 5);
            InsertDisabledRadioButton(radioButtons, 5);

            SelectItemByLabel(radioButtons, 0);
            AssertSelectedFocusedIndex(radioButtons, 0);
            PressKeyAndAssert(radioButtons, Key.Right, 9);

            SelectItemByLabel(radioButtons, 1);
            AssertSelectedFocusedIndex(radioButtons, 1);
            PressKeyAndAssert(radioButtons, Key.Right, 10);

            SelectItemByLabel(radioButtons, 2);
            AssertSelectedFocusedIndex(radioButtons, 2);
            PressKeyAndAssert(radioButtons, Key.Right, 11);

            SelectItemByLabel(radioButtons, 3);
            AssertSelectedFocusedIndex(radioButtons, 3);
            PressKeyAndAssert(radioButtons, Key.Right, 8);

            SelectItemByLabel(radioButtons, 4);
            AssertSelectedFocusedIndex(radioButtons, 4);
            PressKeyAndAssert(radioButtons, Key.Right, 8);

            SelectItemByLabel(radioButtons, 6);
            AssertSelectedFocusedIndex(radioButtons, 9);
            PressKeyAndAssert(radioButtons, Key.Left, 0);

            SelectItemByLabel(radioButtons, 7);
            AssertSelectedFocusedIndex(radioButtons, 10);
            PressKeyAndAssert(radioButtons, Key.Left, 1);

            SelectItemByLabel(radioButtons, 8);
            AssertSelectedFocusedIndex(radioButtons, 11);
            PressKeyAndAssert(radioButtons, Key.Left, 2);

            SelectItemByLabel(radioButtons, 9);
            AssertSelectedFocusedIndex(radioButtons, 12);
            PressKeyAndAssert(radioButtons, Key.Left, 8);
        });
    }

    [TestMethod]
    public void InsertedCheckedRadioButtonGetsSelection()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            using var host = new TestWindowHost(radioButtons, width: 320, height: 360);

            SelectItem(radioButtons, 3);
            Assert.AreEqual(3, radioButtons.SelectedIndex);

            InsertEnabledRadioButton(radioButtons, 6, isChecked: true);
            Assert.AreEqual(6, radioButtons.SelectedIndex);
            Assert.AreSame(GetRadioButton(radioButtons, 6), radioButtons.SelectedItem);
        });
    }

    [TestMethod]
    public void ColumnsTest()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            using var host = new TestWindowHost(radioButtons, width: 2600, height: 1000);
            ModernWpf.Controls.RadioButtonsTestHooks.SetTestHooksEnabled(radioButtons, true);
            radioButtons.MaxColumns = 2;
            host.UpdateLayout();

            SetNumberOfColumns(radioButtons, host, 1);
            AssertLayoutData(radioButtons, rows: 10, columns: 1, largerColumns: 0);

            SetNumberOfColumns(radioButtons, host, 3);
            AssertLayoutData(radioButtons, rows: 3, columns: 3, largerColumns: 1);

            SetNumberOfColumns(radioButtons, host, 5);
            AssertLayoutData(radioButtons, rows: 2, columns: 5, largerColumns: 0);

            SetNumberOfColumns(radioButtons, host, 7);
            AssertLayoutData(radioButtons, rows: 1, columns: 7, largerColumns: 3);

            SetNumberOfColumns(radioButtons, host, 10);
            AssertLayoutData(radioButtons, rows: 1, columns: 10, largerColumns: 0);

            SetNumberOfColumns(radioButtons, host, 20);
            AssertLayoutData(radioButtons, rows: 1, columns: 10, largerColumns: 0);

            SetNumberOfItems(radioButtons, 77, compactContent: true);
            host.UpdateLayout();
            AssertLayoutData(radioButtons, rows: 3, columns: 20, largerColumns: 17);

            ModernWpf.Controls.RadioButtonsTestHooks.SetTestHooksEnabled(radioButtons, false);
        });
    }

    [TestMethod]
    public void UIAProperties()
    {
        WpfTestHost.Run(() =>
        {
            var radioButtons = CreateRadioButtons(10);
            using var host = new TestWindowHost(radioButtons, width: 520, height: 420);

            SelectItem(radioButtons, 1);
            AssertSelectedAutomationPosition(radioButtons, positionInSet: 2, sizeOfSet: 10);

            radioButtons.MaxColumns = 3;
            host.UpdateLayout();
            AssertSelectedAutomationPosition(radioButtons, positionInSet: 2, sizeOfSet: 10);

            InsertEnabledRadioButton(radioButtons, 0);
            host.UpdateLayout();
            Assert.AreEqual(2, radioButtons.SelectedIndex);
            AssertSelectedAutomationPosition(radioButtons, positionInSet: 3, sizeOfSet: 11);

            InsertEnabledRadioButton(radioButtons, 10);
            host.UpdateLayout();
            Assert.AreEqual(2, radioButtons.SelectedIndex);
            AssertSelectedAutomationPosition(radioButtons, positionInSet: 3, sizeOfSet: 12);

            SelectItem(radioButtons, 10);
            AssertSelectedAutomationPosition(radioButtons, positionInSet: 11, sizeOfSet: 12);

            SetNumberOfItems(radioButtons, 17);
            host.UpdateLayout();
            SelectItem(radioButtons, 16);
            AssertSelectedAutomationPosition(radioButtons, positionInSet: 17, sizeOfSet: 17);
        });
    }

    private static ModernWpf.Controls.RadioButtons CreateRadioButtons(int itemCount, bool compactContent = false)
    {
        var radioButtons = new ModernWpf.Controls.RadioButtons();
        for (var i = 0; i < itemCount; i++)
        {
            radioButtons.Items.Add(new RadioButton { Content = GetRadioButtonContent(i, compactContent) });
        }

        return radioButtons;
    }

    private static void InsertDisabledRadioButton(ModernWpf.Controls.RadioButtons radioButtons, int index)
    {
        InsertRadioButton(radioButtons, index, isEnabled: false);
    }

    private static void InsertEnabledRadioButton(
        ModernWpf.Controls.RadioButtons radioButtons,
        int index,
        bool isChecked = false)
    {
        InsertRadioButton(radioButtons, index, isEnabled: true, isChecked);
    }

    private static void InsertRadioButton(
        ModernWpf.Controls.RadioButtons radioButtons,
        int index,
        bool isEnabled,
        bool isChecked = false)
    {
        radioButtons.Items.Insert(index, new RadioButton
        {
            Content = "Custom",
            IsEnabled = isEnabled,
            IsChecked = isChecked
        });
        WpfTestHost.DoEvents();
    }

    private static void SelectItem(ModernWpf.Controls.RadioButtons radioButtons, int index)
    {
        var item = GetRadioButton(radioButtons, index);
        item.Focus();
        item.IsChecked = true;
        WpfTestHost.DoEvents();
    }

    private static void SelectItemByLabel(ModernWpf.Controls.RadioButtons radioButtons, int labelIndex)
    {
        var item = radioButtons.Items
            .OfType<RadioButton>()
            .Single(radioButton => Equals(radioButton.Content, $"Radio Button {labelIndex}"));

        item.Focus();
        item.IsChecked = true;
        WpfTestHost.DoEvents();
    }

    private static void SetNumberOfColumns(
        ModernWpf.Controls.RadioButtons radioButtons,
        TestWindowHost host,
        int columns)
    {
        radioButtons.MaxColumns = columns;
        host.UpdateLayout();
    }

    private static void SetNumberOfItems(
        ModernWpf.Controls.RadioButtons radioButtons,
        int itemCount,
        bool compactContent = false)
    {
        radioButtons.Items.Clear();
        for (var i = 0; i < itemCount; i++)
        {
            radioButtons.Items.Add(new RadioButton { Content = GetRadioButtonContent(i, compactContent) });
        }
        WpfTestHost.DoEvents();
    }

    private static string GetRadioButtonContent(int index, bool compactContent)
    {
        return compactContent ? index.ToString() : $"Radio Button {index}";
    }

    private static void AssertLayoutData(
        ModernWpf.Controls.RadioButtons radioButtons,
        int rows,
        int columns,
        int largerColumns)
    {
        Assert.AreEqual(rows, ModernWpf.Controls.RadioButtonsTestHooks.GetRows(radioButtons));
        Assert.AreEqual(columns, ModernWpf.Controls.RadioButtonsTestHooks.GetColumns(radioButtons));
        Assert.AreEqual(largerColumns, ModernWpf.Controls.RadioButtonsTestHooks.GetLargerColumns(radioButtons));
    }

    private static void AssertSelectedAutomationPosition(
        ModernWpf.Controls.RadioButtons radioButtons,
        int positionInSet,
        int sizeOfSet)
    {
        var item = GetRadioButton(radioButtons, radioButtons.SelectedIndex);
        Assert.AreEqual(positionInSet, item.GetValue(AutomationProperties.PositionInSetProperty));
        Assert.AreEqual(sizeOfSet, item.GetValue(AutomationProperties.SizeOfSetProperty));
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
