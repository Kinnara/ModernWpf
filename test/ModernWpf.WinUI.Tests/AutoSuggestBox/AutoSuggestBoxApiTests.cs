using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using MuxAutoSuggestBox = ModernWpf.Controls.AutoSuggestBox;

namespace ModernWpf.WinUI.Tests.AutoSuggestBox;

[TestClass]
public class AutoSuggestBoxApiTests
{
    [TestMethod]
    public void VerifyAutoSuggestBoxCornerRadius()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
                Width = 400,
                CornerRadius = new CornerRadius(2),
                MaxHeight = 32
            };

            using var host = new TestWindowHost(autoSuggestBox);

            autoSuggestBox.IsSuggestionListOpen = true;
            FlushLayout(host);

            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");
            AssertCornerRadiusMatchesOpenDirection(
                ControlHelper.GetCornerRadius(textBox),
                new CornerRadius(2, 2, 0, 0),
                new CornerRadius(0, 0, 2, 2));

            var overlayCornerRadius = GetOverlayCornerRadius(autoSuggestBox);
            var suggestionsContainer = FindTemplateChild<Border>(autoSuggestBox, "SuggestionsContainer");
            AssertCornerRadiusMatchesOpenDirection(
                suggestionsContainer.CornerRadius,
                new CornerRadius(0, 0, overlayCornerRadius.BottomRight, overlayCornerRadius.BottomLeft),
                new CornerRadius(overlayCornerRadius.TopRight, overlayCornerRadius.TopLeft, 0, 0));
        });
    }

    private static void FlushLayout(TestWindowHost host)
    {
        host.UpdateLayout();
        WpfTestHost.DoEvents();
        host.UpdateLayout();
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        return control.Template?.FindName(name, control) as T
            ?? throw new InvalidOperationException($"Could not find template child '{name}'.");
    }

    private static CornerRadius GetOverlayCornerRadius(FrameworkElement element)
    {
        return element.TryFindResource("OverlayCornerRadius") is CornerRadius radius
            ? radius
            : default;
    }

    private static void AssertCornerRadiusMatchesOpenDirection(
        CornerRadius actual,
        CornerRadius openDownExpected,
        CornerRadius openUpExpected)
    {
        Assert.IsTrue(
            actual == openDownExpected || actual == openUpExpected,
            $"Expected {openDownExpected} or {openUpExpected}, got {actual}.");
    }
}
