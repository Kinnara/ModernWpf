using System;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SplitButton;

[TestClass]
public class SplitButtonApiTests
{
    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = new Flyout();
            var command = new TestCommand();
            const int parameter = 0;

            var splitButton = new ModernWpf.Controls.SplitButton();
            Assert.IsNotNull(splitButton);

            Assert.IsNull(splitButton.Flyout);
            Assert.IsNull(splitButton.Command);
            Assert.IsNull(splitButton.CommandParameter);

            splitButton.Flyout = flyout;
            splitButton.Command = command;
            splitButton.CommandParameter = parameter;

            WpfTestHost.DoEvents();

            Assert.AreSame(flyout, splitButton.Flyout);
            Assert.AreSame(command, splitButton.Command);
            Assert.AreEqual(parameter, splitButton.CommandParameter);
        });
    }

    [TestMethod]
    public void VerifyIsCheckedProperty()
    {
        WpfTestHost.Run(() =>
        {
            var toggleSplitButton = new ToggleSplitButton();

            Assert.IsFalse(toggleSplitButton.IsChecked, "ToggleSplitButton is not unchecked");

            toggleSplitButton.SetValue(ToggleSplitButton.IsCheckedProperty, true);

            Assert.IsTrue((bool)toggleSplitButton.GetValue(ToggleSplitButton.IsCheckedProperty), "ToggleSplitButton is not checked");
        });
    }

#pragma warning disable CS0067
    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
        }
    }
#pragma warning restore CS0067
}
