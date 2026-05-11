using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.PagerControl;

[TestClass]
public class PagerControlApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl();

            Assert.AreEqual(PagerControlDisplayMode.Auto, pager.DisplayMode);
            Assert.AreEqual(0, pager.NumberOfPages);
            Assert.AreEqual(0, pager.SelectedPageIndex);
            Assert.AreEqual(PagerControlButtonVisibility.Visible, pager.FirstButtonVisibility);
            Assert.AreEqual(PagerControlButtonVisibility.Visible, pager.PreviousButtonVisibility);
            Assert.AreEqual(PagerControlButtonVisibility.Visible, pager.NextButtonVisibility);
            Assert.AreEqual(PagerControlButtonVisibility.Visible, pager.LastButtonVisibility);
            Assert.IsTrue(pager.ButtonPanelAlwaysShowFirstLastPageIndex);
            Assert.AreEqual(string.Empty, pager.PrefixText);
            Assert.AreEqual(string.Empty, pager.SuffixText);
            Assert.IsNotNull(pager.TemplateSettings);
        });
    }

    [TestMethod]
    public void VerifyAutomationPeerBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl
            {
                NumberOfPages = 5
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(pager);
            Assert.IsInstanceOfType(peer, typeof(ISelectionProvider));
            var selectionPeer = (ISelectionProvider)peer;

            Assert.IsFalse(selectionPeer.CanSelectMultiple);
            Assert.IsTrue(selectionPeer.IsSelectionRequired);
            Assert.AreEqual(1, selectionPeer.GetSelection().Length);
        });
    }

    [TestMethod]
    public void VerifyNumberPanelButtonUIABehavior()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl
            {
                NumberOfPages = 5,
                DisplayMode = PagerControlDisplayMode.ButtonPanel
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            var buttons = GetPageButtons(pager);
            Assert.AreEqual(5, buttons.Count);

            for (var i = 0; i < buttons.Count; i++)
            {
                Assert.AreEqual(i + 1, buttons[i].GetValue(AutomationProperties.PositionInSetProperty));
                Assert.AreEqual(5, buttons[i].GetValue(AutomationProperties.SizeOfSetProperty));
            }
        });
    }

    [TestMethod]
    public void VerifyComboBoxItemsListNormal()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl
            {
                NumberOfPages = 5,
                DisplayMode = PagerControlDisplayMode.ComboBox
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            AssertPages(pager, 5);

            pager.NumberOfPages = 100;

            AssertPages(pager, 100);
        });
    }

    [TestMethod]
    public void VerifyComboBoxItemsInfiniteItems()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl
            {
                NumberOfPages = 5,
                DisplayMode = PagerControlDisplayMode.ComboBox
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            pager.NumberOfPages = -1;

            AssertPages(pager, 100);

            pager.NumberOfPages = 200;
            pager.NumberOfPages = -1;

            AssertPages(pager, 200);
        });
    }

    [TestMethod]
    public void VerifyEmptyPagerDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl();

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            Assert.IsNotNull(pager);
            Assert.AreEqual(0, pager.TemplateSettings.Pages.Count);
            Assert.AreEqual(0, pager.TemplateSettings.NumberPanelItems.Count);
        });
    }

    [TestMethod]
    public void VerifySelectedIndexChangedEventArgs()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl();
            var previousIndex = -2;
            var newIndex = -2;
            pager.SelectedIndexChanged += (sender, args) =>
            {
                previousIndex = args.PreviousPageIndex;
                newIndex = args.NewPageIndex;
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            Assert.AreEqual(-1, previousIndex);
            Assert.AreEqual(0, newIndex);

            pager.NumberOfPages = 10;

            Assert.AreEqual(-1, previousIndex);
            Assert.AreEqual(0, newIndex);

            pager.SelectedPageIndex = 9;

            Assert.AreEqual(0, previousIndex);
            Assert.AreEqual(9, newIndex);

            pager.SelectedPageIndex = 4;

            Assert.AreEqual(9, previousIndex);
            Assert.AreEqual(4, newIndex);
        });
    }

    [TestMethod]
    public void NavigationButtonsChangePage()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl
            {
                NumberOfPages = 3
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            var nextButton = GetNamedButton(pager, "Next page");
            var previousButton = GetNamedButton(pager, "Previous page");

            Assert.IsFalse(previousButton.IsEnabled);
            Assert.IsTrue(nextButton.IsEnabled);

            nextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(1, pager.SelectedPageIndex);
            Assert.IsTrue(previousButton.IsEnabled);

            pager.ContainerFromPageIndex(2).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(2, pager.SelectedPageIndex);
            Assert.IsFalse(nextButton.IsEnabled);
        });
    }

    private static void AssertPages(ModernWpf.Controls.PagerControl pager, int expectedCount)
    {
        Assert.AreEqual(expectedCount, pager.TemplateSettings.Pages.Count);
        for (var i = 0; i < expectedCount; i++)
        {
            Assert.AreEqual(i + 1, pager.TemplateSettings.Pages[i]);
        }
    }

    private static System.Collections.Generic.List<Button> GetPageButtons(DependencyObject root)
    {
        return VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<Button>()
            .Where(button => button.Tag is int)
            .OrderBy(button => (int)button.Tag)
            .ToList();
    }

    private static Button GetNamedButton(DependencyObject root, string name)
    {
        var button = VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<Button>()
            .FirstOrDefault(candidate => AutomationProperties.GetName(candidate) == name);

        if (button == null)
        {
            Assert.Fail($"Could not find button named '{name}'.");
            throw new AssertFailedException();
        }

        return button;
    }
}
