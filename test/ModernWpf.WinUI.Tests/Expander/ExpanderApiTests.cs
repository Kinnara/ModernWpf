using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using WpfExpander = System.Windows.Controls.Expander;

namespace ModernWpf.WinUI.Tests.Expander;

[TestClass]
public class ExpanderApiTests
{
    [TestMethod]
    public void ExpanderAutomationPeerTest()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var firstLine = new TextBlock
            {
                Text = "This expander is expanded by default.",
                Margin = new Thickness(0, 0, 0, 4)
            };
            AutomationProperties.SetName(firstLine, "test");

            var secondLine = new TextBlock
            {
                Text = "This is the second line of text."
            };

            var headerText = new StackPanel
            {
                Margin = new Thickness(0, 14, 0, 16)
            };
            headerText.Children.Add(firstLine);
            headerText.Children.Add(secondLine);

            var toggleSwitch = new ToggleSwitch();
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            header.Children.Add(headerText);
            Grid.SetColumn(toggleSwitch, 1);
            header.Children.Add(toggleSwitch);

            var contentButton = new Button { Content = "Content" };
            AutomationProperties.SetAutomationId(contentButton, "ExpandedExpanderContent");

            var expander = new WpfExpander
            {
                Header = header,
                Content = contentButton,
                IsExpanded = true,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(expander, "ExpandedExpander");

            using var host = new TestWindowHost(expander, width: 500, height: 300);

            Assert.AreEqual("ExpandedExpander", AutomationProperties.GetName(expander));
            Assert.IsTrue(IsContentElement(firstLine));
            Assert.IsTrue(IsContentElement(secondLine));
            Assert.IsTrue(IsControlElement(toggleSwitch));
            Assert.IsTrue(IsControlElement(contentButton));
            Assert.IsTrue(contentButton.IsVisible);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(expander);
            Assert.IsNotNull(peer);
            Assert.AreEqual("Expander", peer!.GetClassName());

            expander.IsExpanded = false;
            host.UpdateLayout();

            Assert.IsFalse(contentButton.IsVisible, "Collapsed Expander content should not be visible to UI automation.");
        });
    }

    private static bool IsContentElement(FrameworkElement element)
    {
        return FrameworkElementAutomationPeer.CreatePeerForElement(element)?.IsContentElement() == true;
    }

    private static bool IsControlElement(FrameworkElement element)
    {
        return FrameworkElementAutomationPeer.CreatePeerForElement(element)?.IsControlElement() == true;
    }
}
