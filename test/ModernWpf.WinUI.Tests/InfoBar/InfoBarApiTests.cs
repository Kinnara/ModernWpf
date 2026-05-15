using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.InfoBar;

[TestClass]
public class InfoBarApiTests
{
    [TestMethod]
    public void InfoBarDefaultsTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar();

            Assert.IsFalse(infoBar.IsOpen);
            Assert.AreEqual(string.Empty, infoBar.Title);
            Assert.AreEqual(string.Empty, infoBar.Message);
            Assert.AreEqual(InfoBarSeverity.Informational, infoBar.Severity);
            Assert.IsNull(infoBar.IconSource);
            Assert.IsTrue(infoBar.IsIconVisible);
            Assert.IsTrue(infoBar.IsClosable);
            Assert.IsNull(infoBar.ActionButton);
            Assert.IsNull(infoBar.Content);
            Assert.IsNull(infoBar.ContentTemplate);
            Assert.IsNotNull(infoBar.TemplateSettings);
        });
    }

    [TestMethod]
    public void InfoBarCloseEventsTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Title = "Update",
                Message = "Restart required"
            };

            var events = new List<string>();
            var cancelClosing = false;
            infoBar.CloseButtonClick += (_, _) => events.Add("CloseButtonClick");
            infoBar.Closing += (_, args) =>
            {
                events.Add($"Closing: {args.Reason}");
                args.Cancel = cancelClosing;
            };
            infoBar.Closed += (_, args) => events.Add($"Closed: {args.Reason}");

            using var host = new TestWindowHost(infoBar, width: 400, height: 120);
            var closeButton = FindNamedDescendant<Button>(infoBar, "CloseButton");

            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "CloseButtonClick", "Closing: CloseButton", "Closed: CloseButton" },
                events);
            Assert.IsFalse(infoBar.IsOpen);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Border>(infoBar, "ContentRoot").Visibility);

            infoBar.IsOpen = true;
            cancelClosing = true;
            events.Clear();

            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "CloseButtonClick", "Closing: CloseButton" },
                events);
            Assert.IsTrue(infoBar.IsOpen);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<Border>(infoBar, "ContentRoot").Visibility);

            cancelClosing = false;
            events.Clear();

            infoBar.IsOpen = false;
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                new[] { "Closing: Programmatic", "Closed: Programmatic" },
                events);
        });
    }

    [TestMethod]
    public void InfoBarIconAndCloseVisibilityTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar { IsOpen = true };
            using var host = new TestWindowHost(infoBar, width: 400, height: 120);

            var standardIconArea = FindNamedDescendant<FrameworkElement>(infoBar, "StandardIconArea");
            var userIconBox = FindNamedDescendant<FrameworkElement>(infoBar, "UserIconBox");
            var closeButton = FindNamedDescendant<Button>(infoBar, "CloseButton");

            Assert.AreEqual(Visibility.Visible, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Collapsed, userIconBox.Visibility);
            Assert.AreEqual(Visibility.Visible, closeButton.Visibility);

            infoBar.IconSource = new SymbolIconSource { Symbol = Symbol.Setting };
            host.UpdateLayout();

            Assert.IsInstanceOfType(infoBar.TemplateSettings.IconElement, typeof(SymbolIcon));
            Assert.AreEqual(Visibility.Collapsed, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Visible, userIconBox.Visibility);

            infoBar.IsIconVisible = false;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, standardIconArea.Visibility);
            Assert.AreEqual(Visibility.Collapsed, userIconBox.Visibility);

            infoBar.IsClosable = false;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, closeButton.Visibility);
        });
    }

    [TestMethod]
    public void InfoBarSeverityAndContentPositionTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Content = new TextBlock { Text = "details" }
            };

            using var host = new TestWindowHost(infoBar, width: 400, height: 140);
            var standardIcon = FindNamedDescendant<TextBlock>(infoBar, "StandardIcon");
            var contentArea = FindNamedDescendant<FrameworkElement>(infoBar, "ContentArea");

            Assert.AreEqual(0, Grid.GetRow(contentArea));
            Assert.AreEqual("\uF13F", standardIcon.Text);

            infoBar.Title = "Title";
            host.UpdateLayout();

            Assert.AreEqual(1, Grid.GetRow(contentArea));

            infoBar.Severity = InfoBarSeverity.Error;
            host.UpdateLayout();

            Assert.AreEqual("\uF13D", standardIcon.Text);
        });
    }

    [TestMethod]
    public void InfoBarTemplateUsesWinUIContentPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            var bannerContent = new TextBlock { Text = "details" };
            var actionButton = new HyperlinkButton { Content = "Learn more" };
            var infoBar = new ModernWpf.Controls.InfoBar
            {
                IsOpen = true,
                Title = "Title",
                Content = bannerContent,
                ActionButton = actionButton
            };

            using var host = new TestWindowHost(infoBar, width: 400, height: 140);

            var contentArea = FindNamedDescendant<ContentPresenterEx>(infoBar, "ContentArea");
            Assert.AreSame(bannerContent, contentArea.Content);
            Assert.AreEqual(1, Grid.GetColumn(contentArea));
            Assert.AreEqual(1, Grid.GetRow(contentArea));
            Assert.AreEqual(VerticalAlignment.Center, contentArea.VerticalAlignment);

            var actionPresenter = FindContentPresenter(infoBar, actionButton);
            Assert.AreEqual(VerticalAlignment.Top, actionPresenter.VerticalAlignment);
            Assert.AreEqual(
                new Thickness(16, 8, 0, 0),
                InfoBarPanel.GetHorizontalOrientationMargin(actionPresenter));
            Assert.AreEqual(
                new Thickness(0, 12, 0, 0),
                InfoBarPanel.GetVerticalOrientationMargin(actionPresenter));
        });
    }

    [TestMethod]
    public void InfoBarAutomationPeerTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBar = new ModernWpf.Controls.InfoBar { IsOpen = true };
            using var host = new TestWindowHost(infoBar, width: 300, height: 100);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(infoBar);

            Assert.IsNotNull(peer);
            Assert.AreEqual(AutomationControlType.StatusBar, peer.GetAutomationControlType());
            Assert.AreEqual(nameof(ModernWpf.Controls.InfoBar), peer.GetClassName());
        });
    }

    [TestMethod]
    public void InfoBarPanelLayoutTest()
    {
        WpfTestHost.Run(() =>
        {
            var first = CreatePanelChild(20, 10);
            var second = CreatePanelChild(30, 10);
            var third = CreatePanelChild(10, 10);
            var panel = new InfoBarPanel
            {
                HorizontalOrientationPadding = new Thickness(1, 2, 3, 4),
                VerticalOrientationPadding = new Thickness(2, 3, 4, 5)
            };

            InfoBarPanel.SetHorizontalOrientationMargin(first, new Thickness(0, 1, 2, 3));
            InfoBarPanel.SetHorizontalOrientationMargin(second, new Thickness(4, 5, 6, 7));
            InfoBarPanel.SetHorizontalOrientationMargin(third, new Thickness(8, 9, 10, 11));
            InfoBarPanel.SetVerticalOrientationMargin(first, new Thickness(0, 1, 0, 2));
            InfoBarPanel.SetVerticalOrientationMargin(second, new Thickness(0, 3, 0, 4));
            InfoBarPanel.SetVerticalOrientationMargin(third, new Thickness(0, 5, 0, 6));

            panel.Children.Add(first);
            panel.Children.Add(second);
            panel.Children.Add(third);

            panel.Measure(new Size(100, 100));
            panel.Arrange(new Rect(0, 0, 100, 100));

            Assert.AreEqual(new Rect(1, 3, 20, 10), LayoutInformation.GetLayoutSlot(first));
            Assert.AreEqual(new Rect(27, 7, 30, 10), LayoutInformation.GetLayoutSlot(second));
            Assert.AreEqual(new Rect(71, 11, 29, 10), LayoutInformation.GetLayoutSlot(third));

            panel.Measure(new Size(40, 100));
            panel.Arrange(new Rect(0, 0, 40, 100));

            Assert.AreEqual(new Rect(2, 3, 20, 10), LayoutInformation.GetLayoutSlot(first));
            Assert.AreEqual(new Rect(2, 18, 30, 10), LayoutInformation.GetLayoutSlot(second));
            Assert.AreEqual(new Rect(2, 37, 10, 10), LayoutInformation.GetLayoutSlot(third));
        });
    }

    private static FrameworkElement CreatePanelChild(double width, double height)
    {
        return new Border
        {
            Width = width,
            Height = height,
            Background = Brushes.Transparent
        };
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private static ContentPresenterEx FindContentPresenter(DependencyObject root, object content)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is ContentPresenterEx presenter && ReferenceEquals(presenter.Content, content))
            {
                return presenter;
            }
        }

        throw new InvalidOperationException("Could not find ContentPresenterEx for the expected content.");
    }
}
