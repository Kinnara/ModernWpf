using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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

            var controlsResources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/AutoSuggestBox/AutoSuggestBox.xaml", UriKind.Relative)
            };
            var sharedResources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf;component/Styles/AutoSuggestBox.xaml", UriKind.Relative)
            };

            AssertResource(sharedResources, "AutoSuggestBoxTopHeaderMargin", new Thickness(0, 0, 0, 8));
            AssertResource(sharedResources, "AutoSuggestBoxInnerButtonMargin", new Thickness(1, 3, 1, 3));
            AssertResource(sharedResources, "AutoSuggestBoxDeleteButtonMargin", new Thickness(0, 4, 0, 4));
            AssertResource(sharedResources, "AutoSuggestBoxQueryButtonPadding", new Thickness(3, 2, 3, 2));
            AssertResource(sharedResources, "AutoSuggestBoxLeftButtonMargin", 3d);
            AssertResource(sharedResources, "AutoSuggestBoxRightButtonMargin", 4d);

            var textBoxStyle = (Style)sharedResources["AutoSuggestBoxTextBoxStyle"];
            AssertSetterValue(textBoxStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(textBoxStyle, FrameworkElement.MinWidthProperty, "TextControlThemeMinWidth");
            AssertDynamicResourceSetter(textBoxStyle, FrameworkElement.MinHeightProperty, "TextControlThemeMinHeight");
            AssertDynamicResourceSetter(textBoxStyle, Control.ForegroundProperty, "TextControlForeground");
            AssertDynamicResourceSetter(textBoxStyle, Control.BackgroundProperty, "TextControlBackground");
            AssertDynamicResourceSetter(textBoxStyle, Control.BorderBrushProperty, "TextControlBorderBrush");
            AssertDynamicResourceSetter(textBoxStyle, TextBoxBase.SelectionBrushProperty, "TextControlSelectionHighlightColor");
            AssertDynamicResourceSetter(textBoxStyle, Control.BorderThicknessProperty, "TextControlBorderThemeThickness");
            AssertDynamicResourceSetter(textBoxStyle, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(textBoxStyle, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertSetterValue(textBoxStyle, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            AssertSetterValue(textBoxStyle, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            AssertSetterValue(textBoxStyle, ScrollViewer.IsDeferredScrollingEnabledProperty, false);
            AssertDynamicResourceSetter(textBoxStyle, Control.PaddingProperty, "TextControlThemePadding");
            AssertSetterValue(textBoxStyle, KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
            AssertSetterValue(textBoxStyle, Control.FocusVisualStyleProperty, null);
            AssertSetterValue(textBoxStyle, UIElement.AllowDropProperty, true);
            AssertDynamicResourceSetter(textBoxStyle, Control.ContextMenuProperty, "TextControlContextMenu");
            AssertDynamicResourceSetter(textBoxStyle, Validation.ErrorTemplateProperty, "TextControlValidationErrorTemplate");
            Assert.IsInstanceOfType(FindSetter(textBoxStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var style = (Style)controlsResources[typeof(MuxAutoSuggestBox)];
            AssertSetterValue(style, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
            AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(style, Control.IsTabStopProperty, false);
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TextControlForeground");
            AssertDynamicResourceSetter(style, Control.BackgroundProperty, "TextControlBackground");
            AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "TextControlBorderBrush");
            AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "TextControlBorderThemeThickness");
            AssertDynamicResourceSetter(style, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(style, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertDynamicResourceSetter(style, MuxAutoSuggestBox.TextBoxStyleProperty, "AutoSuggestBoxTextBoxStyle");
            AssertDynamicResourceSetter(style, MuxAutoSuggestBox.UseSystemFocusVisualsProperty, "IsApplicationFocusVisualKindReveal");
            AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(style, MuxAutoSuggestBox.CornerRadiusProperty, "ControlCornerRadius");
            AssertDynamicResourceSetter(style, ItemsControl.ItemContainerStyleProperty, typeof(System.Windows.Controls.ListViewItem));
            Assert.IsInstanceOfType(FindSetter(style, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var suggestions = new List<string> { "alpha", "beta" };
            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = suggestions,
                Style = style,
                Width = 400
            };
            autoSuggestBox.Resources.MergedDictionaries.Add(controlsResources);
            autoSuggestBox.Resources.MergedDictionaries.Add(sharedResources);

            using var host = new TestWindowHost(autoSuggestBox, width: 400, height: 120);
            host.UpdateLayout();

            Assert.AreSame(style, autoSuggestBox.Style);
            Assert.AreEqual(VerticalAlignment.Stretch, autoSuggestBox.VerticalAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, autoSuggestBox.VerticalContentAlignment);
            Assert.AreEqual(HorizontalAlignment.Stretch, autoSuggestBox.HorizontalContentAlignment);
            Assert.IsFalse(autoSuggestBox.IsTabStop);
            AssertBrushEquals((Brush)autoSuggestBox.TryFindResource("TextControlForeground"), autoSuggestBox.Foreground);
            AssertBrushEquals((Brush)autoSuggestBox.TryFindResource("TextControlBackground"), autoSuggestBox.Background);
            AssertBrushEquals((Brush)autoSuggestBox.TryFindResource("TextControlBorderBrush"), autoSuggestBox.BorderBrush);
            Assert.AreEqual(autoSuggestBox.TryFindResource("TextControlBorderThemeThickness"), autoSuggestBox.BorderThickness);
            Assert.AreEqual(
                ((FontFamily)autoSuggestBox.TryFindResource("ContentControlThemeFontFamily")).Source,
                autoSuggestBox.FontFamily.Source);
            Assert.AreEqual(autoSuggestBox.TryFindResource("ControlContentThemeFontSize"), autoSuggestBox.FontSize);
            Assert.IsNotNull(autoSuggestBox.TextBoxStyle);
            Assert.IsTrue(autoSuggestBox.AutoMaximizeSuggestionArea);
            Assert.AreEqual(LightDismissOverlayMode.Auto, autoSuggestBox.LightDismissOverlayMode);
            Assert.AreEqual(ControlHeaderPlacement.Top, autoSuggestBox.HeaderPlacement);
            Assert.AreSame(autoSuggestBox.TryFindResource("AutoSuggestBoxTextBoxStyle"), autoSuggestBox.TextBoxStyle);
            Assert.AreEqual(autoSuggestBox.TryFindResource("IsApplicationFocusVisualKindReveal"), autoSuggestBox.UseSystemFocusVisuals);
            Assert.AreSame(autoSuggestBox.TryFindResource(SystemParameters.FocusVisualStyleKey), autoSuggestBox.FocusVisualStyle);
            Assert.AreEqual(autoSuggestBox.TryFindResource("ControlCornerRadius"), autoSuggestBox.CornerRadius);
            Assert.AreSame(autoSuggestBox.TryFindResource(typeof(System.Windows.Controls.ListViewItem)), autoSuggestBox.ItemContainerStyle);
            AssertGlobalResourceValue(autoSuggestBox, "AutoSuggestBoxTopHeaderMargin", new Thickness(0, 0, 0, 8));
            AssertGlobalResourceValue(autoSuggestBox, "AutoSuggestBoxInnerButtonMargin", new Thickness(1, 3, 1, 3));
            AssertGlobalResourceValue(autoSuggestBox, "AutoSuggestBoxDeleteButtonMargin", new Thickness(0, 4, 0, 4));
            AssertGlobalResourceValue(autoSuggestBox, "AutoSuggestBoxQueryButtonPadding", new Thickness(3, 2, 3, 2));
            AssertGlobalResourceValue(autoSuggestBox, "AutoSuggestBoxLeftButtonMargin", 3d);
            AssertGlobalResourceValue(autoSuggestBox, "AutoSuggestBoxRightButtonMargin", 4d);

            var textBox = FindTemplateChild<TextBox>(autoSuggestBox, "TextBox");
            Assert.AreSame(autoSuggestBox.TextBoxStyle, textBox.Style);
            Assert.AreEqual(textBox.TryFindResource("TextControlThemeMinWidth"), textBox.MinWidth);
            Assert.AreEqual(textBox.TryFindResource("TextControlThemeMinHeight"), textBox.MinHeight);
            AssertBrushEquals(autoSuggestBox.Foreground, textBox.Foreground);
            AssertBrushEquals(autoSuggestBox.Background, textBox.Background);
            AssertBrushEquals(autoSuggestBox.BorderBrush, textBox.BorderBrush);
            AssertBrushEquals((Brush)textBox.TryFindResource("TextControlSelectionHighlightColor"), textBox.SelectionBrush);
            Assert.AreEqual(autoSuggestBox.BorderThickness, textBox.BorderThickness);
            Assert.AreEqual(autoSuggestBox.FontSize, textBox.FontSize);
            Assert.AreEqual(autoSuggestBox.FontFamily.Source, textBox.FontFamily.Source);
            Assert.AreEqual(autoSuggestBox.VerticalContentAlignment, textBox.VerticalContentAlignment);
            Assert.AreEqual(autoSuggestBox.HorizontalContentAlignment, textBox.HorizontalContentAlignment);
            Assert.AreEqual(new Thickness(0), textBox.Margin);
            Assert.AreEqual(textBox.TryFindResource("TextControlThemePadding"), textBox.Padding);
            Assert.AreSame(textBox.TryFindResource("TextControlContextMenu"), textBox.ContextMenu);
            Assert.AreSame(textBox.TryFindResource("TextControlValidationErrorTemplate"), Validation.GetErrorTemplate(textBox));

            var deleteButton = FindTemplateChild<Button>(textBox, "DeleteButton");
            deleteButton.ApplyTemplate();
            var deleteButtonChrome = FindNamedDescendant<Border>(deleteButton, "ButtonLayoutGrid");
            var deleteButtonGlyph = FindNamedDescendant<FontIconFallback>(deleteButton, "GlyphElement");
            Assert.AreEqual(32d, deleteButton.Width);
            Assert.AreEqual(VerticalAlignment.Stretch, deleteButton.VerticalAlignment);
            Assert.AreEqual(textBox.BorderThickness, deleteButton.BorderThickness);
            Assert.AreEqual(textBox.TryFindResource("HelperButtonThemePadding"), deleteButton.Padding);
            Assert.IsFalse(deleteButton.Focusable);
            AssertBrushEquals((Brush)deleteButton.TryFindResource("TextControlButtonBackground"), deleteButtonChrome.Background);
            AssertBrushEquals((Brush)deleteButton.TryFindResource("TextControlButtonBorderBrush"), deleteButtonChrome.BorderBrush);
            Assert.AreEqual(deleteButton.BorderThickness, deleteButtonChrome.BorderThickness);
            Assert.AreEqual(deleteButton.TryFindResource("AutoSuggestBoxDeleteButtonMargin"), deleteButtonChrome.Margin);
            Assert.AreEqual(deleteButton.TryFindResource("AutoSuggestBoxIconFontSize"), deleteButtonGlyph.FontSize);
            Assert.AreEqual(((FontFamily)deleteButton.TryFindResource("SymbolThemeFontFamily")).Source, deleteButtonGlyph.FontFamily.Source);

            var queryButton = FindTemplateChild<Button>(textBox, "QueryButton");
            queryButton.ApplyTemplate();
            var queryButtonPresenter = FindNamedDescendant<ContentPresenterEx>(queryButton, "ContentPresenter");
            Assert.AreEqual(32d, queryButton.Width);
            Assert.AreEqual(28d, queryButton.Height);
            Assert.AreEqual(new Thickness(2, 0, 0, 0), queryButton.Margin);
            Assert.AreEqual(VerticalAlignment.Stretch, queryButton.VerticalAlignment);
            Assert.AreEqual(textBox.BorderThickness, queryButton.BorderThickness);
            Assert.AreEqual(textBox.TryFindResource("HelperButtonThemePadding"), queryButton.Padding);
            Assert.IsFalse(queryButton.Focusable);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(queryButton));
            AssertBrushEquals((Brush)queryButton.TryFindResource("TextControlButtonBackground"), queryButtonPresenter.Background);
            AssertBrushEquals((Brush)queryButton.TryFindResource("TextControlButtonBorderBrush"), queryButtonPresenter.BorderBrush);
            Assert.AreEqual(queryButton.BorderThickness, queryButtonPresenter.BorderThickness);
            Assert.AreEqual(queryButton.TryFindResource("AutoSuggestBoxInnerButtonMargin"), queryButtonPresenter.Margin);
            Assert.AreEqual(queryButton.TryFindResource("AutoSuggestBoxIconFontSize"), queryButtonPresenter.FontSize);
            Assert.AreEqual(queryButton.Padding, queryButtonPresenter.Padding);

            var popup = FindTemplateChild<Popup>(autoSuggestBox, "SuggestionsPopup");
            Assert.IsTrue(popup.AllowsTransparency);
            Assert.AreEqual(PlacementMode.Bottom, popup.Placement);

            var shadowChrome = popup.Child as ThemeShadowChrome
                ?? throw new AssertFailedException("Expected AutoSuggestBox suggestions popup child to be ThemeShadowChrome.");
            Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, shadowChrome.WindowedPopupInsetMode);
            Assert.AreEqual(autoSuggestBox.MaxSuggestionListHeight, shadowChrome.MaxHeight);

            var suggestionsContainer = FindTemplateChild<Border>(autoSuggestBox, "SuggestionsContainer");
            Assert.AreSame(suggestionsContainer, shadowChrome.Child);
            Assert.AreEqual(autoSuggestBox.TryFindResource("AutoSuggestListMargin"), suggestionsContainer.Padding);
            Assert.AreEqual(autoSuggestBox.TryFindResource("AutoSuggestListBorderThemeThickness"), suggestionsContainer.BorderThickness);
            AssertBrushEquals((Brush)autoSuggestBox.TryFindResource("AutoSuggestBoxSuggestionsListBorderBrush"), suggestionsContainer.BorderBrush);
            AssertBrushEquals((Brush)autoSuggestBox.TryFindResource("AutoSuggestBoxSuggestionsListBackground"), suggestionsContainer.Background);
            Assert.AreEqual(autoSuggestBox.TryFindResource("OverlayCornerRadius"), suggestionsContainer.CornerRadius);

            var suggestionsList = FindTemplateChild<AutoSuggestBoxListView>(autoSuggestBox, "SuggestionsList");
            Assert.AreSame(autoSuggestBox.TryFindResource(typeof(System.Windows.Controls.ListView)), suggestionsList.Style);
            Assert.AreEqual(autoSuggestBox.DisplayMemberPath, suggestionsList.DisplayMemberPath);
            Assert.AreEqual(autoSuggestBox.TextMemberPath, suggestionsList.SelectedValuePath);
            Assert.IsTrue(suggestionsList.IsItemClickEnabled);
            Assert.AreSame(autoSuggestBox.ItemContainerStyle, suggestionsList.ItemContainerStyle);
            Assert.AreSame(suggestions, suggestionsList.ItemsSource);
            Assert.AreEqual(autoSuggestBox.TryFindResource("AutoSuggestListMaxHeight"), suggestionsList.MaxHeight);
            Assert.AreEqual(autoSuggestBox.TryFindResource("AutoSuggestListPadding"), suggestionsList.Margin);
            Assert.AreEqual(ScrollBarVisibility.Disabled, ScrollViewer.GetHorizontalScrollBarVisibility(suggestionsList));

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceValue(themeName, "AutoSuggestListMaxHeight", 374d);
                AssertThemeResourceValue(themeName, "AutoSuggestListBorderOpacity", 0d);
                AssertThemeResourceValue(themeName, "AutoSuggestListBorderThemeThickness", new Thickness(1));
                AssertThemeResourceValue(themeName, "AutoSuggestListMargin", new Thickness(0, 2, 0, 2));
                AssertThemeResourceValue(themeName, "AutoSuggestListPadding", new Thickness(-1, 0, -1, 0));
                AssertThemeResourceValue(themeName, "AutoSuggestBoxIconFontSize", 12d);
            }

            AssertThemeResourceValue("Light", "AutoSuggestListViewItemMargin", new Thickness(10, 11, 0, 13));
            AssertThemeResourceValue("Dark", "AutoSuggestListViewItemMargin", new Thickness(12, 11, 0, 13));
            AssertThemeResourceValue("HighContrast", "AutoSuggestListViewItemMargin", new Thickness(10, 11, 0, 13));
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "AutoSuggestBoxSuggestionsListBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "AutoSuggestBoxSuggestionsListBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "AutoSuggestBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            }

            AssertThemeResourceReference("HighContrast", "AutoSuggestBoxSuggestionsListBackground", "SystemControlBackgroundChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "AutoSuggestBoxSuggestionsListBorderBrush", "SystemControlTransientBorderBrush");
            AssertThemeResourceReference("HighContrast", "AutoSuggestBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
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
    public void SuggestionListItemClickUsesWinUISourceEventBeforeSelectionOrder()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var suggestionsList = CreateSuggestionList("alpha", "beta");
            suggestionsList.SelectionMode = SelectionMode.Multiple;

            using var host = new TestWindowHost(suggestionsList, width: 240, height: 120);
            FlushLayout(host);

            object? clickedItem = null;
            var selectedCountDuringItemClick = -1;
            suggestionsList.ItemClick += (_, args) =>
            {
                clickedItem = args.ClickedItem;
                selectedCountDuringItemClick = suggestionsList.SelectedItems.Count;
            };

            suggestionsList.NotifyListItemClicked(GetSuggestionItem(suggestionsList, 0), MouseButton.Left);

            Assert.AreEqual("alpha", clickedItem);
            Assert.AreEqual(0, selectedCountDuringItemClick);
            CollectionAssert.AreEqual(new[] { "alpha" }, SelectedStrings(suggestionsList));
        });
    }

    [TestMethod]
    public void SuggestionListPrimarySelectionUsesWinUISourceSelectionModes()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var multipleList = CreateSuggestionList("alpha", "beta", "gamma");
            multipleList.SelectionMode = SelectionMode.Multiple;

            using (var host = new TestWindowHost(multipleList, width: 240, height: 140))
            {
                FlushLayout(host);

                multipleList.NotifyListItemClicked(GetSuggestionItem(multipleList, 0), MouseButton.Left);
                multipleList.NotifyListItemClicked(GetSuggestionItem(multipleList, 1), MouseButton.Left);
                CollectionAssert.AreEqual(new[] { "alpha", "beta" }, SelectedStrings(multipleList));

                multipleList.NotifyListItemClicked(GetSuggestionItem(multipleList, 0), MouseButton.Left);
                CollectionAssert.AreEqual(new[] { "beta" }, SelectedStrings(multipleList));
            }

            var extendedList = CreateSuggestionList("alpha", "beta", "gamma");
            extendedList.SelectionMode = SelectionMode.Extended;

            using (var host = new TestWindowHost(extendedList, width: 240, height: 140))
            {
                FlushLayout(host);

                extendedList.NotifyListItemClicked(GetSuggestionItem(extendedList, 0), MouseButton.Left);
                CollectionAssert.AreEqual(new[] { "alpha" }, SelectedStrings(extendedList));

                extendedList.NotifyListItemClicked(GetSuggestionItem(extendedList, 1), MouseButton.Left);
                CollectionAssert.AreEqual(new[] { "beta" }, SelectedStrings(extendedList));
            }
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
            Assert.AreEqual(((CornerRadius)queryButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), presenter.CornerRadius);
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
    public void SuggestionsPopupUsesSourceThemeShadow()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var autoSuggestBox = new MuxAutoSuggestBox
            {
                ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
                Width = 400
            };

            using var host = new TestWindowHost(autoSuggestBox, width: 460, height: 160);
            host.UpdateLayout();

            var popup = FindTemplateChild<Popup>(autoSuggestBox, "SuggestionsPopup");
            var shadowChrome = popup.Child as ThemeShadowChrome
                ?? throw new AssertFailedException("Expected AutoSuggestBox suggestions popup child to be ThemeShadowChrome.");
            var suggestionsContainer = FindTemplateChild<Border>(autoSuggestBox, "SuggestionsContainer");

            Assert.AreSame(suggestionsContainer, shadowChrome.Child);
            Assert.AreEqual(32.0, shadowChrome.Depth);
            Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, shadowChrome.WindowedPopupInsetMode);
            Assert.AreEqual(new Thickness(10, 2, 10, 18), shadowChrome.PopupShadowPadding);
            Assert.AreEqual(suggestionsContainer.CornerRadius, shadowChrome.CornerRadius);

            var cornerRadiusBinding = BindingOperations.GetBinding(shadowChrome, ThemeShadowChrome.CornerRadiusProperty);
            Assert.IsNotNull(cornerRadiusBinding);
            Assert.AreEqual("SuggestionsContainer", cornerRadiusBinding!.ElementName);
            Assert.AreEqual("CornerRadius", cornerRadiusBinding.Path.Path);
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
                ((CornerRadius)textBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)),
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

    private static AutoSuggestBoxListView CreateSuggestionList(params string[] items)
    {
        var suggestionsList = new AutoSuggestBoxListView
        {
            IsItemClickEnabled = true,
            Width = 200,
            Height = 120
        };

        foreach (var item in items)
        {
            suggestionsList.Items.Add(item);
        }

        return suggestionsList;
    }

    private static AutoSuggestBoxListViewItem GetSuggestionItem(AutoSuggestBoxListView suggestionsList, int index)
    {
        suggestionsList.UpdateLayout();
        WpfTestHost.DoEvents();
        suggestionsList.UpdateLayout();

        return suggestionsList.ItemContainerGenerator.ContainerFromIndex(index) as AutoSuggestBoxListViewItem
            ?? throw new InvalidOperationException($"Could not find suggestion item container {index}.");
    }

    private static string[] SelectedStrings(AutoSuggestBoxListView suggestionsList)
    {
        return suggestionsList.SelectedItems.Cast<string>().ToArray();
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

    private static void AssertResource(ResourceDictionary resources, string key, object expectedValue)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expectedValue, resources[key], $"Unexpected value for '{key}'.");
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object? expectedValue)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");

        var dynamicResource = setter!.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var current = style; current != null; current = current.BasedOn)
        {
            var setter = current.Setters
                .OfType<Setter>()
                .SingleOrDefault(item => item.Property == property);

            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertGlobalResourceValue<T>(FrameworkElement element, string resourceKey, T expectedValue)
    {
        var resource = element.TryFindResource(resourceKey);
        Assert.IsNotNull(resource, $"Missing global resource {resourceKey}.");
        Assert.AreEqual(expectedValue, resource);
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

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
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
