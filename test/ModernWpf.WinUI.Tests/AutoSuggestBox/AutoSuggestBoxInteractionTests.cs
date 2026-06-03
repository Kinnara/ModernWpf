using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;
using MuxAutoSuggestBox = ModernWpf.Controls.AutoSuggestBox;

namespace ModernWpf.WinUI.Tests.AutoSuggestBox;

[TestClass]
public class AutoSuggestBoxInteractionTests
{
    [TestMethod]
    public void CanSelectSuggestion()
    {
        WpfTestHost.Run(() =>
        {
            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = new List<string> { "lorem", "dolor", "ipsum" },
                Width = 400
            };

            using var host = new TestWindowHost(autoSuggestBox, width: 520, height: 240);

            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");
            Assert.IsTrue(textBox.Focus());

            textBox.Text = "test";
            FlushLayout(host);

            Assert.IsTrue(autoSuggestBox.IsSuggestionListOpen);

            var suggestionsList = FindTemplateChild<AutoSuggestBoxListView>(autoSuggestBox, "SuggestionsList");
            suggestionsList.SelectedItem = "dolor";
            FlushLayout(host);

            Assert.AreEqual("dolor", textBox.Text);
            Assert.AreEqual("dolor", autoSuggestBox.Text);

            RaiseKey(textBox, Keyboard.PreviewKeyDownEvent, Key.Enter);
            FlushLayout(host);

            Assert.IsFalse(autoSuggestBox.IsSuggestionListOpen);
        });
    }

    [TestMethod]
    public void SuggestionItemClickSubmitsAndClosesPopup()
    {
        WpfTestHost.Run(() =>
        {
            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = new List<string> { "lorem", "dolor", "ipsum" },
                Width = 400
            };

            using var host = new TestWindowHost(autoSuggestBox, width: 520, height: 240);

            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");
            Assert.IsTrue(textBox.Focus());

            textBox.Text = "test";
            FlushLayout(host);

            Assert.IsTrue(autoSuggestBox.IsSuggestionListOpen);

            object? chosenSuggestion = null;
            object? submittedSuggestion = null;
            autoSuggestBox.SuggestionChosen += (_, args) => chosenSuggestion = args.SelectedItem;
            autoSuggestBox.QuerySubmitted += (_, args) => submittedSuggestion = args.ChosenSuggestion;

            var suggestionsList = FindTemplateChild<AutoSuggestBoxListView>(autoSuggestBox, "SuggestionsList");
            var item = GetSuggestionItem(suggestionsList, 1);

            suggestionsList.NotifyListItemClicked(item, MouseButton.Left);
            FlushLayout(host);

            Assert.AreEqual("dolor", chosenSuggestion);
            Assert.AreEqual("dolor", submittedSuggestion);
            Assert.IsFalse(autoSuggestBox.IsSuggestionListOpen);
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

    private static AutoSuggestBoxListViewItem GetSuggestionItem(AutoSuggestBoxListView suggestionsList, int index)
    {
        suggestionsList.UpdateLayout();
        WpfTestHost.DoEvents();
        suggestionsList.UpdateLayout();

        return suggestionsList.ItemContainerGenerator.ContainerFromIndex(index) as AutoSuggestBoxListViewItem
            ?? throw new InvalidOperationException($"Could not find suggestion item container {index}.");
    }

    private static void RaiseKey(UIElement element, RoutedEvent routedEvent, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            Environment.TickCount,
            key)
        {
            RoutedEvent = routedEvent
        };

        element.RaiseEvent(args);
    }
}
