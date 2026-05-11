using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NumberBox;

[TestClass]
public class NumberBoxApiTests
{
    [TestMethod]
    public void VerifyTextAlignmentPropogates()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            var textBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual(TextAlignment.Left, textBox.TextAlignment);

            numberBox.TextAlignment = TextAlignment.Right;
            host.UpdateLayout();

            Assert.AreEqual(TextAlignment.Right, textBox.TextAlignment);
        });
    }

    [TestMethod]
    public void VerifyInputScopePropogates()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            var inputTextBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual(1, inputTextBox.InputScope.Names.Count);
            Assert.AreEqual(InputScopeNameValue.Number, GetInputScopeName(inputTextBox).NameValue);

            var scopeName = new InputScopeName
            {
                NameValue = InputScopeNameValue.CurrencyAmountAndSymbol
            };
            var scope = new InputScope();
            scope.Names.Add(scopeName);

            numberBox.InputScope = scope;
            host.UpdateLayout();

            Assert.AreEqual(1, inputTextBox.InputScope.Names.Count);
            Assert.AreEqual(InputScopeNameValue.CurrencyAmountAndSymbol, GetInputScopeName(inputTextBox).NameValue);
        });
    }

    [TestMethod]
    public void VerifyIsEnabledChangeUpdatesVisualState()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            numberBox.IsEnabled = true;
            host.UpdateLayout();

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(numberBox, 0);
            var commonStatesGroup = VisualStateManager.GetVisualStateGroups(layoutRoot)
                .OfType<VisualStateGroup>()
                .Single(group => group.Name == "CommonStates");

            Assert.AreEqual("Normal", commonStatesGroup.CurrentState.Name);

            numberBox.IsEnabled = false;
            host.UpdateLayout();
            Assert.AreEqual("Disabled", commonStatesGroup.CurrentState.Name);

            numberBox.IsEnabled = true;
            host.UpdateLayout();
            Assert.AreEqual("Normal", commonStatesGroup.CurrentState.Name);
        });
    }

    [TestMethod]
    public void VerifyUIANameBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            var textBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            numberBox.Header = "Some header";
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some header");

            numberBox.Header = new Button();
            AutomationProperties.SetName(numberBox, "Some UIA name");
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name");

            numberBox.Header = new Button();
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name");

            numberBox.Minimum = 0;
            numberBox.Maximum = 10;
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name Minimum0 Maximum10");

            numberBox.Minimum = 50;
            numberBox.Maximum = 100;
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name Minimum50 Maximum100");
        });
    }

    private static T FindTemplatePart<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .Single(element => element.Name == name);
    }

    private static InputScopeName GetInputScopeName(TextBox textBox)
    {
        return textBox.InputScope.Names[0] as InputScopeName
            ?? throw new AssertFailedException("Expected an InputScopeName entry.");
    }

    private static void VerifyUIAName(FrameworkElement element, string expectedName)
    {
        var peer = FrameworkElementAutomationPeer.FromElement(element)
            ?? FrameworkElementAutomationPeer.CreatePeerForElement(element);

        Assert.AreEqual(expectedName, peer.GetName());
    }
}
