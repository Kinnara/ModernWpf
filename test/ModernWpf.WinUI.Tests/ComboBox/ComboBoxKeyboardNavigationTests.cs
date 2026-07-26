using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ComboBox;

[TestClass]
public class ComboBoxKeyboardNavigationTests
{
    [TestMethod]
    public void EditableComboBoxUsesTextBoxAsOnlyTabStopAndLeavesOnBackwardOrControlTab()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var previous = new TextBox();
            var comboBox = new System.Windows.Controls.ComboBox
            {
                IsEditable = true
            };
            var root = new StackPanel();
            root.Children.Add(previous);
            root.Children.Add(comboBox);

            using var host = new TestWindowHost(root, width: 320, height: 160);
            host.UpdateLayout();

            var editableTextBox = VisualTreeTestHelper.FindDescendant<TextBox>(comboBox)
                ?? throw new AssertFailedException("Expected editable ComboBox TextBox.");

            Assert.IsFalse(comboBox.IsTabStop);
            Assert.IsTrue(editableTextBox.IsTabStop);

            Assert.IsTrue(editableTextBox.Focus());
            WpfTestHost.DoEvents();
            Assert.AreSame(editableTextBox, Keyboard.FocusedElement);

            Assert.IsTrue(editableTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)));
            WpfTestHost.DoEvents();
            Assert.AreSame(previous, Keyboard.FocusedElement);

            Assert.IsTrue(editableTextBox.Focus());
            WpfTestHost.DoEvents();
            Assert.IsTrue(NavigateWithControlModifier(
                editableTextBox,
                new TraversalRequest(FocusNavigationDirection.Next)));
            WpfTestHost.DoEvents();
            Assert.AreSame(previous, Keyboard.FocusedElement);
        });
    }

    private static bool NavigateWithControlModifier(
        DependencyObject currentElement,
        TraversalRequest request)
    {
        var currentProperty = typeof(KeyboardNavigation).GetProperty(
            "Current",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("KeyboardNavigation.Current was not found.");
        var navigation = currentProperty.GetValue(null)
            ?? throw new InvalidOperationException("KeyboardNavigation.Current returned null.");
        var navigate = typeof(KeyboardNavigation).GetMethod(
            "Navigate",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types:
            [
                typeof(DependencyObject),
                typeof(TraversalRequest),
                typeof(ModifierKeys),
                typeof(bool)
            ],
            modifiers: null)
            ?? throw new InvalidOperationException("KeyboardNavigation.Navigate was not found.");

        return (bool)navigate.Invoke(
            navigation,
            [currentElement, request, ModifierKeys.Control, true])!;
    }
}
