using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using MuxAutoSuggestBox = ModernWpf.Controls.AutoSuggestBox;

namespace ModernWpf.WinUI.Tests.AutoSuggestBox;

[TestClass]
public class AutoSuggestBoxApiTests
{
    [TestMethod]
    public void VerifyAutoSuggestBoxDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var autoSuggestBox = new MuxAutoSuggestBox();

            using var host = new TestWindowHost(autoSuggestBox, width: 400, height: 120);
            host.UpdateLayout();

            Assert.AreEqual(VerticalAlignment.Stretch, autoSuggestBox.VerticalAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, autoSuggestBox.VerticalContentAlignment);
            Assert.AreEqual(HorizontalAlignment.Stretch, autoSuggestBox.HorizontalContentAlignment);
            Assert.IsNotNull(autoSuggestBox.TextBoxStyle);
            Assert.IsTrue(autoSuggestBox.AutoMaximizeSuggestionArea);
            Assert.AreEqual(LightDismissOverlayMode.Auto, autoSuggestBox.LightDismissOverlayMode);
            Assert.AreEqual(ControlHeaderPlacement.Top, autoSuggestBox.HeaderPlacement);

            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");
            Assert.AreSame(autoSuggestBox.TextBoxStyle, textBox.Style);

            AssertThemeResourceReference("Light", "AutoSuggestBoxSuggestionsListBackground", "AcrylicBackgroundFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "AutoSuggestBoxSuggestionsListBackground", "AcrylicBackgroundFillColorDefaultBrush");
            AssertThemeResourceReference("Light", "AutoSuggestBoxSuggestionsListBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertThemeResourceReference("Dark", "AutoSuggestBoxSuggestionsListBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertThemeResourceReference("HighContrast", "AutoSuggestBoxSuggestionsListBackground", "SystemControlBackgroundChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "AutoSuggestBoxSuggestionsListBorderBrush", "SystemControlTransientBorderBrush");
            AssertThemeResourceReference("Light", "AutoSuggestBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            AssertThemeResourceReference("Dark", "AutoSuggestBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            AssertThemeResourceReference("HighContrast", "AutoSuggestBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            AssertThemeResourceValue("Light", "AutoSuggestBoxIconFontSize", 12d);
            AssertThemeResourceValue("Dark", "AutoSuggestBoxIconFontSize", 12d);
            AssertThemeResourceValue("HighContrast", "AutoSuggestBoxIconFontSize", 12d);
        });
    }

    [TestMethod]
    public void TextChangedArgsUseSourceCounterSemantics()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = new List<string> { "alpha" },
                Width = 400
            };

            using var host = new TestWindowHost(autoSuggestBox, width: 460, height: 120);
            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");

            AutoSuggestBoxTextChangedEventArgs? firstArgs = null;
            AutoSuggestBoxTextChangedEventArgs? secondArgs = null;

            autoSuggestBox.TextChanged += (_, args) =>
            {
                if (firstArgs == null)
                {
                    firstArgs = args;
                }
                else
                {
                    secondArgs = args;
                }
            };

            textBox.Text = "a";
            WaitFor(() => firstArgs != null, "First source-delayed TextChanged event did not fire.");
            var first = firstArgs!;
            Assert.AreEqual(AutoSuggestionBoxTextChangeReason.UserInput, first.Reason);
            Assert.IsTrue(first.CheckCurrent());

            textBox.Text = "ab";
            WaitFor(() => secondArgs != null, "Second source-delayed TextChanged event did not fire.");
            var second = secondArgs!;
            Assert.IsFalse(first.CheckCurrent());
            Assert.IsTrue(second.CheckCurrent());
        });
    }

    [TestMethod]
    public void AutomationPeerInvokesProgrammaticSubmitQuery()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = new List<string> { "alpha", "beta" },
                Text = "alpha",
                Width = 400
            };

            using var host = new TestWindowHost(autoSuggestBox, width: 460, height: 160);
            autoSuggestBox.IsSuggestionListOpen = true;
            FlushLayout(host);

            var suggestionsList = FindTemplateChild<AutoSuggestBoxListView>(autoSuggestBox, "SuggestionsList");
            suggestionsList.SelectedIndex = 1;
            FlushLayout(host);

            AutoSuggestBoxQuerySubmittedEventArgs? submitted = null;
            autoSuggestBox.QuerySubmitted += (_, args) => submitted = args;

            var peer = UIElementAutomationPeer.CreatePeerForElement(autoSuggestBox);
            Assert.IsNotNull(peer);
            Assert.AreEqual("AutoSuggestBox", peer!.GetClassName());
            Assert.AreEqual(AutomationControlType.Group, peer.GetAutomationControlType());

            var invokeProvider = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)!;
            invokeProvider.Invoke();
            FlushLayout(host);

            Assert.IsNotNull(submitted);
            var args = submitted!;
            Assert.AreEqual("beta", args.QueryText);
            Assert.IsNull(args.ChosenSuggestion);
            Assert.IsFalse(autoSuggestBox.IsSuggestionListOpen);
            Assert.AreEqual(-1, suggestionsList.SelectedIndex);
        });
    }

    [TestMethod]
    public void VerifyAutoSuggestBoxQueryButtonUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var queryIcon = new SymbolIcon(Symbol.Find);
            var autoSuggestBox = new MuxAutoSuggestBox
            {
                Description = "Search description",
                QueryIcon = queryIcon,
                Width = 400
            };

            using var host = new TestWindowHost(autoSuggestBox, width: 460, height: 120);
            host.UpdateLayout();

            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");
            var queryButton = FindTemplateChild<Button>(textBox, "QueryButton");
            ControlHelper.SetBackgroundSizing(queryButton, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetContentTransitions(queryButton, transitions);

            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(queryButton)
                ?? throw new AssertFailedException("Expected AutoSuggestBox query button template to use ContentPresenterEx.");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreSame(queryIcon, presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(queryButton.Padding, presenter.Padding);
            Assert.AreEqual(ControlHelper.GetCornerRadius(queryButton), presenter.CornerRadius);
            Assert.AreEqual(12d, presenter.FontSize);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(queryButton));
            AssertAnimatedIconStateSetter(presenter, "PointerOver", "PointerOver");
            AssertAnimatedIconStateSetter(presenter, "Pressed", "Pressed");
            Assert.AreEqual("Normal", AnimatedIcon.GetState(presenter));
            Assert.IsTrue(VisualStateManager.GoToState(queryButton, "PointerOver", false));
            Assert.AreEqual("PointerOver", AnimatedIcon.GetState(presenter));
            Assert.IsTrue(VisualStateManager.GoToState(queryButton, "Pressed", false));
            Assert.AreEqual("Pressed", AnimatedIcon.GetState(presenter));
            Assert.IsTrue(VisualStateManager.GoToState(queryButton, "Disabled", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(presenter));
            Assert.IsTrue(VisualStateManager.GoToState(queryButton, "Normal", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(presenter));

            var descriptionPresenter = FindTemplateChild<ContentPresenterEx>(textBox, "DescriptionPresenter");
            Assert.AreEqual("Search description", descriptionPresenter.Content);
            Assert.AreEqual(Visibility.Visible, descriptionPresenter.Visibility);
            Assert.AreSame(
                descriptionPresenter.TryFindResource("SystemControlDescriptionTextForegroundBrush"),
                descriptionPresenter.Foreground);
        });
    }

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

    private static void WaitFor(Func<bool> predicate, string failureMessage, int timeoutMilliseconds = 1500)
    {
        var deadline = Environment.TickCount + timeoutMilliseconds;

        while (Environment.TickCount < deadline)
        {
            WpfTestHost.DoEvents();
            if (predicate())
            {
                return;
            }

            Thread.Sleep(10);
        }

        Assert.Fail(failureMessage);
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        return control.Template?.FindName(name, control) as T
            ?? throw new InvalidOperationException($"Could not find template child '{name}'.");
    }

    private static void AssertAnimatedIconStateSetter(FrameworkElement stateGroupsRoot, string stateName, string expectedValue)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var setter = state.Setters.Single(item => item.Target == "ContentPresenter.(local:AnimatedIcon.State)");

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
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
