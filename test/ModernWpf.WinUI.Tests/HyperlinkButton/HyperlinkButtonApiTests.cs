using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.HyperlinkButtonTests;

[TestClass]
public class HyperlinkButtonApiTests
{
    [TestMethod]
    public void VerifyWinUI3ApiSurfaceAndDefaults()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var resources = (ResourceDictionary)Application.LoadComponent(
                new Uri("/ModernWpf.Controls;component/HyperlinkButton/HyperlinkButton.xaml", UriKind.Relative));

            var hyperlinkButton = new ModernWpf.Controls.HyperlinkButton
            {
                Content = "Link"
            };

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            Assert.IsNull(hyperlinkButton.NavigateUri);
            Assert.IsNull(typeof(ModernWpf.Controls.HyperlinkButton).GetProperty("TargetName"));
            Assert.IsNull(typeof(ModernWpf.Controls.HyperlinkButton).GetField("TargetNameProperty", BindingFlags.Public | BindingFlags.Static));
            Assert.IsInstanceOfType(resources["DefaultHyperlinkButtonStyle"], typeof(Style));
            Assert.AreEqual(hyperlinkButton.TryFindResource("ButtonPadding"), hyperlinkButton.Padding);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(hyperlinkButton));
        });
    }

    [TestMethod]
    public void VerifyWinUI3TemplateStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var hyperlinkButton = new ModernWpf.Controls.HyperlinkButton
            {
                Content = "Link"
            };

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(hyperlinkButton)
                ?? throw new AssertFailedException("Expected HyperlinkButton template to use ContentPresenterEx.");

            AssertStateSetter(presenter, "CommonStates", "PointerOver",
                "ContentPresenter.(ui:AnimatedIcon.State)",
                "ContentPresenter.Foreground",
                "ContentPresenter.Background",
                "ContentPresenter.BorderBrush");
            AssertStateSetter(presenter, "CommonStates", "Pressed",
                "ContentPresenter.(ui:AnimatedIcon.State)",
                "ContentPresenter.Foreground",
                "ContentPresenter.Background",
                "ContentPresenter.BorderBrush");
            AssertStateSetter(presenter, "CommonStates", "Disabled",
                "ContentPresenter.(ui:AnimatedIcon.State)",
                "ContentPresenter.Foreground",
                "ContentPresenter.Background",
                "ContentPresenter.BorderBrush");

            Assert.IsTrue(VisualStateManager.GoToState(hyperlinkButton, "PointerOver", false));
            Assert.AreEqual(hyperlinkButton.TryFindResource("HyperlinkButtonForegroundPointerOver"), presenter.Foreground);
            Assert.AreEqual(hyperlinkButton.TryFindResource("HyperlinkButtonBackgroundPointerOver"), presenter.Background);
            Assert.AreEqual(hyperlinkButton.TryFindResource("HyperlinkButtonBorderBrushPointerOver"), presenter.BorderBrush);
        });
    }

    [TestMethod]
    public void VerifyWinUI3AutomationPeerInvoke()
    {
        WpfTestHost.Run(() =>
        {
            var hyperlinkButton = new ModernWpf.Controls.HyperlinkButton
            {
                Content = "Link"
            };
            var clickCount = 0;
            hyperlinkButton.Click += (sender, args) => clickCount++;

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(hyperlinkButton);
            Assert.AreEqual("Hyperlink", peer.GetClassName());
            Assert.AreEqual(AutomationControlType.Hyperlink, peer.GetAutomationControlType());

            var invokeProvider = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
            invokeProvider.Invoke();

            Assert.AreEqual(1, clickCount);
        });
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        foreach (var child in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (child is T typedChild)
            {
                return typedChild;
            }
        }

        return null;
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] expectedTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .FirstOrDefault(candidate => candidate.Name == groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = group!.States
            .OfType<VisualState>()
            .FirstOrDefault(candidate => candidate.Name == stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (var expectedTarget in expectedTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.OfType<VisualStateSetter>().Any(setter => setter.Target == expectedTarget),
                $"Expected visual state '{groupName}.{stateName}' to contain setter '{expectedTarget}'.");
        }
    }
}
