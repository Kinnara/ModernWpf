using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.FlyoutTests;

[TestClass]
public class FlyoutPresenterApiTests
{
    [TestMethod]
    public void FlyoutPresenterAcceptsWinUIContentPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new TransitionCollection();
            var presenter = new FlyoutPresenter
            {
                ContentTransitions = transitions,
                CornerRadius = new CornerRadius(4)
            };

            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(new CornerRadius(4), presenter.CornerRadius);
        });
    }

    [TestMethod]
    public void FlyoutPresenterTemplateUsesWinUIContentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border { Width = 80, Height = 24 };
            var transitions = new TransitionCollection();
            var background = new SolidColorBrush(Colors.Red);
            var borderBrush = new SolidColorBrush(Colors.Blue);
            var presenter = new FlyoutPresenter
            {
                Content = content,
                Background = background,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1, 2, 3, 4),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(6, 7, 8, 9),
                ContentTransitions = transitions,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(presenter, width: 240, height: 160);

            var chrome = VisualTreeTestHelper.FindDescendant<BorderEx>(presenter)
                ?? throw new AssertFailedException("Expected FlyoutPresenter template to use BorderEx for WinUI chrome.");
            var contentPresenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(presenter)
                ?? throw new AssertFailedException("Expected FlyoutPresenter template to use ContentPresenterEx for its content slot.");

            Assert.AreSame(background, chrome.Background);
            Assert.AreSame(borderBrush, chrome.BorderBrush);
            Assert.AreEqual(new Thickness(1, 2, 3, 4), chrome.BorderThickness);
            Assert.AreEqual(new CornerRadius(5), chrome.CornerRadius);
            Assert.AreEqual(BackgroundSizing.InnerBorderEdge, chrome.BackgroundSizing);

            Assert.AreSame(content, contentPresenter.Content);
            Assert.AreEqual(new Thickness(6, 7, 8, 9), contentPresenter.Margin);
            Assert.AreSame(transitions, contentPresenter.ContentTransitions);
            Assert.AreEqual(HorizontalAlignment.Right, contentPresenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, contentPresenter.VerticalAlignment);
        });
    }
}
