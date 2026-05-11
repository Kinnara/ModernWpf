using System.Collections.Generic;
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

namespace ModernWpf.WinUI.Tests.PipsPager;

[TestClass]
public class PipsPagerApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager();

            Assert.AreEqual(-1, pipsPager.NumberOfPages);
            Assert.AreEqual(0, pipsPager.SelectedPageIndex);
            Assert.AreEqual(5, pipsPager.MaxVisiblePips);
            Assert.AreEqual(Orientation.Horizontal, pipsPager.Orientation);
            Assert.AreEqual(PipsPagerButtonVisibility.Collapsed, pipsPager.PreviousButtonVisibility);
            Assert.AreEqual(PipsPagerButtonVisibility.Collapsed, pipsPager.NextButtonVisibility);
            Assert.IsNull(pipsPager.PreviousButtonStyle);
            Assert.IsNull(pipsPager.NextButtonStyle);
            Assert.IsNull(pipsPager.SelectedPipStyle);
            Assert.IsNull(pipsPager.NormalPipStyle);
            Assert.IsNotNull(pipsPager.TemplateSettings);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var previousButtonStyle = new Style(typeof(Button));
            var nextButtonStyle = new Style(typeof(Button));
            var selectedPipStyle = new Style(typeof(Button));
            var normalPipStyle = new Style(typeof(Button));
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 10,
                SelectedPageIndex = 4,
                MaxVisiblePips = 3,
                Orientation = Orientation.Vertical,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.VisibleOnPointerOver,
                PreviousButtonStyle = previousButtonStyle,
                NextButtonStyle = nextButtonStyle,
                SelectedPipStyle = selectedPipStyle,
                NormalPipStyle = normalPipStyle
            };

            Assert.AreEqual(10, pipsPager.NumberOfPages);
            Assert.AreEqual(4, pipsPager.SelectedPageIndex);
            Assert.AreEqual(3, pipsPager.MaxVisiblePips);
            Assert.AreEqual(Orientation.Vertical, pipsPager.Orientation);
            Assert.AreEqual(PipsPagerButtonVisibility.Visible, pipsPager.PreviousButtonVisibility);
            Assert.AreEqual(PipsPagerButtonVisibility.VisibleOnPointerOver, pipsPager.NextButtonVisibility);
            Assert.AreSame(previousButtonStyle, pipsPager.PreviousButtonStyle);
            Assert.AreSame(nextButtonStyle, pipsPager.NextButtonStyle);
            Assert.AreSame(selectedPipStyle, pipsPager.SelectedPipStyle);
            Assert.AreSame(normalPipStyle, pipsPager.NormalPipStyle);
        });
    }

    [TestMethod]
    public void VerifyAutomationPeerBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 5
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(pipsPager);
            Assert.IsInstanceOfType(peer, typeof(ISelectionProvider));
            var selectionPeer = (ISelectionProvider)peer;

            Assert.IsFalse(selectionPeer.CanSelectMultiple);
            Assert.IsTrue(selectionPeer.IsSelectionRequired);
            Assert.AreEqual(1, selectionPeer.GetSelection().Length);
        });
    }

    [TestMethod]
    public void VerifyPipsPagerButtonUIABehavior()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 5
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var buttons = GetPipButtons(pipsPager);
            Assert.AreEqual(5, buttons.Count);

            for (var i = 0; i < buttons.Count; i++)
            {
                Assert.AreEqual(i + 1, buttons[i].GetValue(AutomationProperties.PositionInSetProperty));
                Assert.AreEqual(5, buttons[i].GetValue(AutomationProperties.SizeOfSetProperty));
            }
        });
    }

    [TestMethod]
    public void VerifyEmptyPagerDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 0
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            Assert.IsNotNull(pipsPager);
            Assert.AreEqual(0, pipsPager.TemplateSettings.PipsPagerItems.Count);
            Assert.AreEqual(0, GetPipButtons(pipsPager).Count);
        });
    }

    [TestMethod]
    public void VerifySelectedIndexChangedEventArgs()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager();
            var newIndex = -2;
            pipsPager.SelectedIndexChanged += (sender, args) => newIndex = sender.SelectedPageIndex;

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            Assert.AreEqual(0, newIndex);

            pipsPager.NumberOfPages = 10;
            Assert.AreEqual(0, newIndex);

            pipsPager.SelectedPageIndex = 9;
            Assert.AreEqual(9, newIndex);

            pipsPager.SelectedPageIndex = 4;
            Assert.AreEqual(4, newIndex);
        });
    }

    [TestMethod]
    public void VisiblePipWindowTracksSelectedPage()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 10,
                MaxVisiblePips = 5,
                SelectedPageIndex = 8
            };

            CollectionAssert.AreEqual(
                new[] { 5, 6, 7, 8, 9 },
                pipsPager.TemplateSettings.PipsPagerItems.ToArray());

            pipsPager.SelectedPageIndex = 1;

            CollectionAssert.AreEqual(
                new[] { 0, 1, 2, 3, 4 },
                pipsPager.TemplateSettings.PipsPagerItems.ToArray());
        });
    }

    [TestMethod]
    public void PipsAndNavigationButtonsChangePage()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.Visible
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var nextButton = GetNamedButton(pipsPager, "Next page");
            var previousButton = GetNamedButton(pipsPager, "Previous page");

            Assert.IsFalse(previousButton.IsEnabled);
            Assert.IsTrue(nextButton.IsEnabled);

            nextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(1, pipsPager.SelectedPageIndex);
            Assert.IsTrue(previousButton.IsEnabled);

            pipsPager.ContainerFromIndex(2).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(2, pipsPager.SelectedPageIndex);
            Assert.IsFalse(nextButton.IsEnabled);
        });
    }

    private static List<Button> GetPipButtons(DependencyObject root)
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
