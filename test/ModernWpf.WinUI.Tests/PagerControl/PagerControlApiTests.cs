using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
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

    [TestMethod]
    public void NavigationButtonStatesUseVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            var pager = new ModernWpf.Controls.PagerControl
            {
                NumberOfPages = 3,
                FirstButtonVisibility = PagerControlButtonVisibility.HiddenOnEdge,
                PreviousButtonVisibility = PagerControlButtonVisibility.HiddenOnEdge,
                NextButtonVisibility = PagerControlButtonVisibility.HiddenOnEdge,
                LastButtonVisibility = PagerControlButtonVisibility.HiddenOnEdge
            };

            using var host = new TestWindowHost(pager, width: 360, height: 120);

            var rootPanel = GetTemplateChild<FrameworkElement>(pager, "PART_RootPanel");
            var firstButton = GetTemplateChild<Button>(pager, "PART_FirstButton");
            var previousButton = GetTemplateChild<Button>(pager, "PART_PreviousButton");
            var nextButton = GetTemplateChild<Button>(pager, "PART_NextButton");
            var lastButton = GetTemplateChild<Button>(pager, "PART_LastButton");

            AssertStateSetter(rootPanel, "FirstPageButtonVisibilityStates", "FirstPageButtonCollapsed", "PART_FirstButton.Visibility");
            AssertStateSetter(rootPanel, "FirstPageButtonVisibilityStates", "FirstPageButtonHidden", "PART_FirstButton.Opacity");
            AssertStateSetter(rootPanel, "FirstPageButtonVisibilityStates", "FirstPageButtonHidden", "PART_FirstButton.IsEnabled");
            AssertStateSetter(rootPanel, "FirstPageButtonIsEnabledStates", "FirstPageButtonDisabled", "PART_FirstButton.IsEnabled");
            AssertStateSetter(rootPanel, "PreviousPageButtonVisibilityStates", "PreviousPageButtonCollapsed", "PART_PreviousButton.Visibility");
            AssertStateSetter(rootPanel, "PreviousPageButtonVisibilityStates", "PreviousPageButtonHidden", "PART_PreviousButton.Opacity");
            AssertStateSetter(rootPanel, "PreviousPageButtonVisibilityStates", "PreviousPageButtonHidden", "PART_PreviousButton.IsEnabled");
            AssertStateSetter(rootPanel, "PreviousPageButtonIsEnabledStates", "PreviousPageButtonDisabled", "PART_PreviousButton.IsEnabled");
            AssertStateSetter(rootPanel, "NextPageButtonVisibilityStates", "NextPageButtonCollapsed", "PART_NextButton.Visibility");
            AssertStateSetter(rootPanel, "NextPageButtonIsEnabledStates", "NextPageButtonHidden", "PART_NextButton.Opacity");
            AssertStateSetter(rootPanel, "NextPageButtonIsEnabledStates", "NextPageButtonHidden", "PART_NextButton.IsEnabled");
            AssertStateSetter(rootPanel, "NextPageButtonIsEnabledStates", "NextPageButtonDisabled", "PART_NextButton.IsEnabled");
            AssertStateSetter(rootPanel, "LastPageButtonStates", "LastPageButtonCollapsed", "PART_LastButton.Visibility");
            AssertStateSetter(rootPanel, "LastPageButtonIsEnabledStates", "LastPageButtonHidden", "PART_LastButton.Opacity");
            AssertStateSetter(rootPanel, "LastPageButtonIsEnabledStates", "LastPageButtonHidden", "PART_LastButton.IsEnabled");
            AssertStateSetter(rootPanel, "LastPageButtonIsEnabledStates", "LastPageButtonDisabled", "PART_LastButton.IsEnabled");

            Assert.AreEqual(0.0, firstButton.Opacity);
            Assert.AreEqual(0.0, previousButton.Opacity);
            Assert.IsFalse(firstButton.IsEnabled);
            Assert.IsFalse(previousButton.IsEnabled);
            Assert.AreEqual(1.0, nextButton.Opacity);
            Assert.AreEqual(1.0, lastButton.Opacity);
            Assert.IsTrue(nextButton.IsEnabled);
            Assert.IsTrue(lastButton.IsEnabled);

            pager.SelectedPageIndex = 2;
            host.UpdateLayout();

            Assert.AreEqual(1.0, firstButton.Opacity);
            Assert.AreEqual(1.0, previousButton.Opacity);
            Assert.IsTrue(firstButton.IsEnabled);
            Assert.IsTrue(previousButton.IsEnabled);
            Assert.AreEqual(0.0, nextButton.Opacity);
            Assert.AreEqual(0.0, lastButton.Opacity);
            Assert.IsFalse(nextButton.IsEnabled);
            Assert.IsFalse(lastButton.IsEnabled);
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

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        var child = control.Template.FindName(name, control) as T;
        Assert.IsNotNull(child, $"Missing template part '{name}'.");
        return child!;
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"{groupName}.{stateName} should set {setterTarget}.");
    }
}
