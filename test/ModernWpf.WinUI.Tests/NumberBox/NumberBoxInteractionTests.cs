using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NumberBox;

[TestClass]
public class NumberBoxInteractionTests
{
    [TestMethod]
    public void UpDownTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var upButton = FindTemplatePart<RepeatButton>(numberBox, "UpSpinButton");
            var downButton = FindTemplatePart<RepeatButton>(numberBox, "DownSpinButton");

            Click(upButton);
            Assert.AreEqual(1.0, numberBox.Value);

            Click(downButton);
            Assert.AreEqual(0.0, numberBox.Value);

            numberBox.SmallChange = 5;
            Click(upButton);
            Assert.AreEqual(5.0, numberBox.Value);

            numberBox.Value = 100;
            numberBox.IsWrapEnabled = true;
            host.UpdateLayout();

            Click(upButton);
            Assert.AreEqual(0.0, numberBox.Value);

            Click(downButton);
            Assert.AreEqual(100.0, numberBox.Value);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            inputBox.Text = "54";
            Click(upButton);
            Assert.AreEqual(59.0, numberBox.Value);
        });
    }

    [TestMethod]
    public void UpDownEnabledTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.Value = double.NaN;

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var upButton = FindTemplatePart<RepeatButton>(numberBox, "UpSpinButton");
            var downButton = FindTemplatePart<RepeatButton>(numberBox, "DownSpinButton");

            Assert.IsFalse(upButton.IsEnabled);
            Assert.IsFalse(downButton.IsEnabled);

            numberBox.Value = 0;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsFalse(downButton.IsEnabled);

            numberBox.Value = 100;
            host.UpdateLayout();
            Assert.IsFalse(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.IsWrapEnabled = true;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.IsWrapEnabled = false;
            numberBox.Maximum = 200;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.ValidationMode = NumberBoxValidationMode.Disabled;
            numberBox.Value = 0;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.Value = double.NaN;
            host.UpdateLayout();
            Assert.IsFalse(upButton.IsEnabled);
            Assert.IsFalse(downButton.IsEnabled);
        });
    }

    [TestMethod]
    public void ValueTextTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            var provider = GetRangeValueProvider(numberBox);

            provider.SetValue(10);
            host.UpdateLayout();
            Assert.AreEqual("10", inputBox.Text);

            numberBox.Text = "15";
            host.UpdateLayout();
            Assert.AreEqual(15.0, provider.Value);
            Assert.AreEqual("15", inputBox.Text);

            numberBox.Text = " 15 ";
            host.UpdateLayout();
            Assert.AreEqual("15", inputBox.Text);
        });
    }

    [TestMethod]
    public void MinMaxTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var provider = GetRangeValueProvider(numberBox);
            Assert.AreEqual(0.0, provider.Minimum);
            Assert.AreEqual(100.0, provider.Maximum);

            provider.SetValue(10);
            provider.SetValue(-1);
            host.UpdateLayout();
            Assert.AreEqual(0.0, provider.Value);

            numberBox.Text = "123";
            host.UpdateLayout();
            Assert.AreEqual(100.0, provider.Value);

            numberBox.Maximum = 90;
            host.UpdateLayout();
            Assert.AreEqual(90.0, provider.Value);

            numberBox.Minimum = 200;
            host.UpdateLayout();
            Assert.AreEqual(200.0, provider.Minimum);
            Assert.AreEqual(200.0, provider.Maximum);
            Assert.AreEqual(200.0, provider.Value);

            numberBox.Maximum = 150;
            host.UpdateLayout();
            Assert.AreEqual(150.0, provider.Minimum);
            Assert.AreEqual(150.0, provider.Maximum);
            Assert.AreEqual(150.0, provider.Value);
        });
    }

    [TestMethod]
    public void ValidationDisabledTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.ValidationMode = NumberBoxValidationMode.Disabled;

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var provider = GetRangeValueProvider(numberBox);

            provider.SetValue(-10);
            host.UpdateLayout();
            Assert.AreEqual(-10.0, provider.Value);

            provider.SetValue(150);
            host.UpdateLayout();
            Assert.AreEqual(150.0, provider.Value);
        });
    }

    private static ModernWpf.Controls.NumberBox CreateNumberBox()
    {
        return new ModernWpf.Controls.NumberBox
        {
            Width = 180,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };
    }

    private static IRangeValueProvider GetRangeValueProvider(ModernWpf.Controls.NumberBox numberBox)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(numberBox);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.RangeValue) is IRangeValueProvider provider)
        {
            return provider;
        }

        Assert.Fail("NumberBox should expose IRangeValueProvider.");
        throw new InvalidOperationException();
    }

    private static void Click(RepeatButton button)
    {
        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        WpfTestHost.DoEvents();
    }

    private static T FindTemplatePart<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .Single(element => element.Name == name);
    }
}
