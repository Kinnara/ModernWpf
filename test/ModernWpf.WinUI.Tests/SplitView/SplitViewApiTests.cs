using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SplitView;

[TestClass]
public class SplitViewApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView();

            Assert.IsFalse(splitView.IsPaneOpen);
            Assert.AreEqual(SplitViewDisplayMode.Overlay, splitView.DisplayMode);
            Assert.AreEqual(SplitViewPanePlacement.Left, splitView.PanePlacement);
            Assert.AreEqual(LightDismissOverlayMode.Auto, splitView.LightDismissOverlayMode);
            Assert.AreEqual(0d, splitView.OpenPaneLength);
            Assert.AreEqual(0d, splitView.CompactPaneLength);
            Assert.IsNull(splitView.Pane);
            Assert.IsNull(splitView.Content);
            Assert.IsNull(splitView.PaneBackground);
            Assert.IsNotNull(splitView.TemplateSettings);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border();
            var pane = new Border();
            var paneBackground = new SolidColorBrush(Colors.Red);
            var splitView = new ModernWpf.Controls.SplitView
            {
                Content = content,
                Pane = pane,
                DisplayMode = SplitViewDisplayMode.CompactInline,
                PanePlacement = SplitViewPanePlacement.Right,
                LightDismissOverlayMode = LightDismissOverlayMode.On,
                OpenPaneLength = 296,
                CompactPaneLength = 48,
                PaneBackground = paneBackground
            };

            Assert.AreSame(content, splitView.Content);
            Assert.AreSame(pane, splitView.Pane);
            Assert.AreEqual(SplitViewDisplayMode.CompactInline, splitView.DisplayMode);
            Assert.AreEqual(SplitViewPanePlacement.Right, splitView.PanePlacement);
            Assert.AreEqual(LightDismissOverlayMode.On, splitView.LightDismissOverlayMode);
            Assert.AreEqual(296d, splitView.OpenPaneLength);
            Assert.AreEqual(48d, splitView.CompactPaneLength);
            Assert.AreSame(paneBackground, splitView.PaneBackground);
        });
    }

    [TestMethod]
    public void TemplateSettingsTrackPaneLengths()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                OpenPaneLength = 296,
                CompactPaneLength = 48
            };

            SplitViewTemplateSettings settings = splitView.TemplateSettings;

            Assert.AreEqual(new GridLength(48), settings.CompactPaneGridLength);
            Assert.AreEqual(-296d, settings.NegativeOpenPaneLength);
            Assert.AreEqual(-248d, settings.NegativeOpenPaneLengthMinusCompactLength);
            Assert.AreEqual(new GridLength(296), settings.OpenPaneGridLength);
            Assert.AreEqual(296d, settings.OpenPaneLength);
            Assert.AreEqual(248d, settings.OpenPaneLengthMinusCompactLength);
        });
    }

    [TestMethod]
    public void TemplateSettingsUseMeasuredPaneLengthWhenOpenPaneLengthIsAuto()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                OpenPaneLength = double.NaN,
                CompactPaneLength = 48,
                Pane = new Border { Width = 137, Height = 24 },
                Content = new Border()
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            host.UpdateLayout();

            SplitViewTemplateSettings settings = splitView.TemplateSettings;
            Assert.AreEqual(137d, settings.OpenPaneLength, 0.01);
            Assert.AreEqual(-137d, settings.NegativeOpenPaneLength, 0.01);
            Assert.AreEqual(89d, settings.OpenPaneLengthMinusCompactLength, 0.01);
            Assert.AreEqual(-89d, settings.NegativeOpenPaneLengthMinusCompactLength, 0.01);
            Assert.AreEqual(new GridLength(137), settings.OpenPaneGridLength);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2SplitViewHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceValue(themeName, "SplitViewOpenPaneThemeLength", 320d);
                AssertThemeResourceValue(themeName, "SplitViewCompactPaneThemeLength", 48d);
                AssertThemeResourceValue(themeName, "SplitViewLeftBorderThemeThickness", new Thickness(0, 0, 1, 0));
                AssertThemeResourceValue(themeName, "SplitViewRightBorderThemeThickness", new Thickness(1, 0, 0, 0));
                AssertThemeResourceReference(
                    themeName,
                    "SplitViewLightDismissOverlayBackground",
                    "SystemControlPageBackgroundMediumAltMediumBrush");
            }
        });
    }

    [TestMethod]
    public void SplitViewStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/SplitView/SplitView.xaml", UriKind.Relative)
            };
            var style = (Style)resources[typeof(ModernWpf.Controls.SplitView)];

            AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "SystemControlForegroundTransparentBrush");
            AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "SplitViewLeftBorderThemeThickness");
            AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
            AssertDynamicResourceSetter(style, ModernWpf.Controls.SplitView.OpenPaneLengthProperty, "SplitViewOpenPaneThemeLength");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.SplitView.CompactPaneLengthProperty, "SplitViewCompactPaneThemeLength");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.SplitView.PaneBackgroundProperty, "SystemControlPageBackgroundChromeLowBrush");
            AssertDynamicResourceSetter(style, ControlEx.CornerRadiusProperty, "SplitViewPaneRootCornerRadius");

            var pane = new Border { Width = 120, Height = 360 };
            var content = new Border { Width = 240, Height = 360 };
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Overlay,
                LightDismissOverlayMode = LightDismissOverlayMode.On,
                IsPaneOpen = true,
                Pane = pane,
                Content = content
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.AreSame(splitView.TryFindResource("SystemControlForegroundTransparentBrush"), splitView.BorderBrush);
            Assert.AreEqual(splitView.TryFindResource("SplitViewLeftBorderThemeThickness"), splitView.BorderThickness);
            Assert.AreEqual(HorizontalAlignment.Stretch, splitView.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, splitView.VerticalContentAlignment);
            Assert.AreEqual(splitView.TryFindResource("SplitViewOpenPaneThemeLength"), splitView.OpenPaneLength);
            Assert.AreEqual(splitView.TryFindResource("SplitViewCompactPaneThemeLength"), splitView.CompactPaneLength);
            Assert.AreSame(splitView.TryFindResource("SystemControlPageBackgroundChromeLowBrush"), splitView.PaneBackground);
            Assert.AreEqual(splitView.TryFindResource("SplitViewPaneRootCornerRadius"), splitView.CornerRadius);

            var templateRoot = (Grid)VisualTreeHelper.GetChild(splitView, 0);
            var columnDefinition1 = FindTemplatePart<ColumnDefinition>(splitView, "ColumnDefinition1");
            var columnDefinition2 = FindTemplatePart<ColumnDefinition>(splitView, "ColumnDefinition2");
            var paneRoot = FindTemplatePart<Border>(splitView, "PaneRoot");
            var paneClipRectangle = FindTemplatePart<RectangleGeometry>(splitView, "PaneClipRectangle");
            var paneClipTransform = FindTemplatePart<TranslateTransform>(splitView, "PaneClipRectangleTransform");
            var paneTransform = FindTemplatePart<TranslateTransform>(splitView, "PaneTransform");
            var contentRoot = FindTemplatePart<Grid>(splitView, "ContentRoot");
            var contentTransform = FindTemplatePart<TranslateTransform>(splitView, "ContentTransform");
            var hcPaneBorder = FindTemplatePart<Rectangle>(splitView, "HCPaneBorder");
            var lightDismissLayer = FindTemplatePart<SplitViewLightDismissLayer>(splitView, "LightDismissLayer");

            Assert.AreSame(splitView.Background, templateRoot.Background);
            Assert.AreEqual(splitView.TemplateSettings.OpenPaneGridLength, columnDefinition1.Width);
            Assert.AreEqual(new GridLength(1, GridUnitType.Star), columnDefinition2.Width);

            Assert.AreSame(splitView.BorderBrush, paneRoot.BorderBrush);
            Assert.AreEqual(splitView.BorderThickness, paneRoot.BorderThickness);
            Assert.AreSame(splitView.PaneBackground, paneRoot.Background);
            Assert.AreEqual(splitView.TemplateSettings.OpenPaneLength, paneRoot.Width);
            Assert.AreEqual(splitView.CornerRadius, paneRoot.CornerRadius);
            Assert.IsTrue(paneRoot.Focusable);
            Assert.AreEqual(KeyboardNavigationMode.Local, KeyboardNavigation.GetTabNavigation(paneRoot));
            Assert.AreSame(paneClipTransform, paneClipRectangle.Transform);
            Assert.AreSame(paneTransform, paneRoot.RenderTransform);

            AssertBrushMatchesResource(splitView, "SystemControlForegroundTransparentBrush", hcPaneBorder.Fill);
            Assert.AreSame(contentTransform, contentRoot.RenderTransform);
            AssertBrushMatchesResource(splitView, "SplitViewLightDismissOverlayBackground", lightDismissLayer.Fill);
        });
    }

    [TestMethod]
    public void PaneOpenCloseEventsFollowTestUiPattern()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView();
            var openingCount = 0;
            var openedCount = 0;
            var closingCount = 0;
            var closedCount = 0;

            splitView.PaneOpening += (sender, args) => openingCount++;
            splitView.PaneOpened += (sender, args) => openedCount++;
            splitView.PaneClosing += (sender, args) => closingCount++;
            splitView.PaneClosed += (sender, args) => closedCount++;

            splitView.IsPaneOpen = true;

            Assert.IsTrue(splitView.IsPaneOpen);
            Assert.AreEqual(1, openingCount);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(0, closingCount);
            Assert.AreEqual(0, closedCount);

            splitView.IsPaneOpen = false;

            Assert.IsFalse(splitView.IsPaneOpen);
            Assert.AreEqual(1, openingCount);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(1, closingCount);
            Assert.AreEqual(1, closedCount);
        });
    }

    [TestMethod]
    public void LightDismissLayerRespectsPaneClosingCancellation()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Overlay,
                IsPaneOpen = true,
                OpenPaneLength = 200,
                Pane = new Border(),
                Content = new Border()
            };

            var cancel = true;
            var closingCount = 0;
            var closedCount = 0;
            splitView.PaneClosing += (sender, args) =>
            {
                closingCount++;
                args.Cancel = cancel;
            };
            splitView.PaneClosed += (sender, args) => closedCount++;

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            var lightDismissLayer = FindTemplatePart<FrameworkElement>(splitView, "LightDismissLayer");
            RaiseMouseLeftButtonUp(lightDismissLayer);

            Assert.IsTrue(splitView.IsPaneOpen);
            Assert.AreEqual(1, closingCount);
            Assert.AreEqual(0, closedCount);

            cancel = false;
            RaiseMouseLeftButtonUp(lightDismissLayer);
            WaitFor(
                () => closedCount == 1,
                "SplitView PaneClosed did not fire after light dismiss.",
                timeoutMilliseconds: 5000);

            Assert.IsFalse(splitView.IsPaneOpen);
            Assert.AreEqual(2, closingCount);
            Assert.AreEqual(1, closedCount);
        });
    }

    [TestMethod]
    public void LightDismissTemplatePartsExposeWinUIAutomationPatterns()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Overlay,
                IsPaneOpen = true,
                OpenPaneLength = 200,
                Pane = new Border(),
                Content = new Border()
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            var paneRoot = FindTemplatePart<SplitViewPaneRoot>(splitView, "PaneRoot");
            var panePeer = FrameworkElementAutomationPeer.CreatePeerForElement(paneRoot);
            Assert.IsNotNull(panePeer);
            Assert.AreEqual("SplitViewPane", panePeer.GetClassName());
            Assert.AreEqual(AutomationControlType.Window, panePeer.GetAutomationControlType());
            Assert.AreEqual("PaneRoot", panePeer.GetAutomationId());
            Assert.IsTrue(panePeer.IsControlElement());
            Assert.IsFalse(panePeer.IsContentElement());

            var windowProvider = panePeer.GetPattern(PatternInterface.Window) as IWindowProvider;
            Assert.IsNotNull(windowProvider);
            Assert.IsTrue(windowProvider!.IsModal);
            Assert.IsTrue(windowProvider.IsTopmost);
            Assert.IsFalse(windowProvider.Maximizable);
            Assert.IsFalse(windowProvider.Minimizable);
            Assert.AreEqual(WindowInteractionState.Running, windowProvider.InteractionState);
            Assert.AreEqual(WindowVisualState.Normal, windowProvider.VisualState);

            var lightDismissLayer = FindTemplatePart<SplitViewLightDismissLayer>(splitView, "LightDismissLayer");
            var dismissPeer = FrameworkElementAutomationPeer.CreatePeerForElement(lightDismissLayer);
            Assert.IsNotNull(dismissPeer);
            Assert.AreEqual("SplitViewLightDismiss", dismissPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.Button, dismissPeer.GetAutomationControlType());
            Assert.AreEqual("Close", dismissPeer.GetName());
            Assert.AreEqual("LightDismiss", dismissPeer.GetAutomationId());
            Assert.IsTrue(dismissPeer.IsControlElement());
            Assert.IsTrue(dismissPeer.IsContentElement());

            var invokeProvider = dismissPeer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            Assert.IsNotNull(invokeProvider);
            invokeProvider!.Invoke();
            WpfTestHost.DoEvents();
            host.UpdateLayout();
            Assert.IsFalse(splitView.IsPaneOpen);

            splitView.DisplayMode = SplitViewDisplayMode.Inline;
            splitView.IsPaneOpen = true;
            WpfTestHost.DoEvents();
            host.UpdateLayout();
            Assert.IsNull(panePeer.GetPattern(PatternInterface.Window));
            Assert.IsNull(dismissPeer.GetPattern(PatternInterface.Invoke));
        });
    }

    [TestMethod]
    public void EscapeClosesLightDismissiblePane()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Overlay,
                IsPaneOpen = true,
                OpenPaneLength = 200,
                Pane = new Border(),
                Content = new Border()
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            RaiseKeyDown(splitView, Key.Escape);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.IsFalse(splitView.IsPaneOpen);
        });
    }

    [TestMethod]
    public void TestUiDisplayModeAndPanePlacementChangesAreApplied()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Inline,
                PanePlacement = SplitViewPanePlacement.Left,
                OpenPaneLength = 296
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);

            splitView.DisplayMode = SplitViewDisplayMode.CompactInline;
            splitView.PanePlacement = SplitViewPanePlacement.Right;
            host.UpdateLayout();

            Assert.AreEqual(SplitViewDisplayMode.CompactInline, splitView.DisplayMode);
            Assert.AreEqual(SplitViewPanePlacement.Right, splitView.PanePlacement);
            Assert.AreEqual(296d, splitView.TemplateSettings.OpenPaneLength);
        });
    }

    [TestMethod]
    public void ClosedCompactStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            VerifyClosedCompactState(SplitViewPanePlacement.Left);
            VerifyClosedCompactState(SplitViewPanePlacement.Right);
        });
    }

    [TestMethod]
    public void OpenInlineLeftStateUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.Inline,
                PanePlacement = SplitViewPanePlacement.Left,
                IsPaneOpen = true,
                OpenPaneLength = 296
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(splitView, 0);
            Assert.AreEqual("OpenInlineLeft", GetCurrentStateName(layoutRoot, "DisplayModeStates"));

            var contentRoot = FindTemplatePart<Grid>(splitView, "ContentRoot");
            var paneRoot = FindTemplatePart<FrameworkElement>(splitView, "PaneRoot");
            var hcPaneBorder = FindTemplatePart<FrameworkElement>(splitView, "HCPaneBorder");
            var paneTransform = FindTemplatePart<TranslateTransform>(splitView, "PaneTransform");
            var contentTransform = FindTemplatePart<TranslateTransform>(splitView, "ContentTransform");
            var paneClipTransform = FindTemplatePart<TranslateTransform>(splitView, "PaneClipRectangleTransform");

            Assert.AreEqual(Visibility.Visible, paneRoot.Visibility);
            Assert.AreEqual(Visibility.Visible, hcPaneBorder.Visibility);
            Assert.AreEqual(1, Grid.GetColumn(contentRoot));
            Assert.AreEqual(1, Grid.GetColumnSpan(contentRoot));
            Assert.AreEqual(1, Grid.GetColumnSpan(paneRoot));
            Assert.AreEqual(0, paneTransform.X);
            Assert.AreEqual(0, contentTransform.X);
            Assert.AreEqual(0, paneClipTransform.X);
        });
    }

    [TestMethod]
    public void OpenCompactOverlayLeftStateUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView
            {
                DisplayMode = SplitViewDisplayMode.CompactOverlay,
                PanePlacement = SplitViewPanePlacement.Left,
                IsPaneOpen = true,
                OpenPaneLength = 296,
                CompactPaneLength = 48
            };

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(splitView, 0);
            Assert.AreEqual("OpenCompactOverlayLeft", GetCurrentStateName(layoutRoot, "DisplayModeStates"));

            var contentRoot = FindTemplatePart<Grid>(splitView, "ContentRoot");
            Assert.AreEqual(1, Grid.GetColumn(contentRoot));
            Assert.AreEqual(1, Grid.GetColumnSpan(contentRoot));
        });
    }

    [TestMethod]
    public void RightInlineTransitionsMatchWinUISourceTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var splitView = new ModernWpf.Controls.SplitView();

            using var host = new TestWindowHost(splitView, width: 640, height: 360);
            host.UpdateLayout();

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(splitView, 0);
            var group = VisualStateManager.GetVisualStateGroups(layoutRoot)
                .OfType<VisualStateGroup>()
                .Single(item => item.Name == "DisplayModeStates");

            var transitions = new HashSet<string>(
                group.Transitions
                    .OfType<VisualTransition>()
                    .Select(transition => $"{transition.From}->{transition.To}"));

            Assert.IsTrue(transitions.Contains("Closed->OpenInlineRight"));
            Assert.IsTrue(transitions.Contains("OpenInlineRight->Closed"));
            Assert.IsTrue(transitions.Contains("ClosedCompactRight->OpenInlineRight"));
            Assert.IsTrue(transitions.Contains("OpenInlineRight->ClosedCompactRight"));
        });
    }

    private static void VerifyClosedCompactState(SplitViewPanePlacement panePlacement)
    {
        var splitView = new ModernWpf.Controls.SplitView
        {
            DisplayMode = SplitViewDisplayMode.CompactInline,
            PanePlacement = panePlacement,
            IsPaneOpen = false,
            OpenPaneLength = 296,
            CompactPaneLength = 48
        };

        using var host = new TestWindowHost(splitView, width: 640, height: 360);
        WpfTestHost.DoEvents();
        host.UpdateLayout();

        var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(splitView, 0);
        var contentRoot = FindTemplatePart<Grid>(splitView, "ContentRoot");
        var paneRoot = FindTemplatePart<FrameworkElement>(splitView, "PaneRoot");
        var columnDefinition1 = FindTemplatePart<ColumnDefinition>(splitView, "ColumnDefinition1");
        var columnDefinition2 = FindTemplatePart<ColumnDefinition>(splitView, "ColumnDefinition2");
        var paneClipTransform = FindTemplatePart<TranslateTransform>(splitView, "PaneClipRectangleTransform");

        if (panePlacement == SplitViewPanePlacement.Left)
        {
            Assert.AreEqual("ClosedCompactLeft", GetCurrentStateName(layoutRoot, "DisplayModeStates"));
            Assert.AreEqual(new GridLength(48), columnDefinition1.Width);
            Assert.AreEqual(1, Grid.GetColumn(contentRoot));
            Assert.AreEqual(1, Grid.GetColumnSpan(contentRoot));
            Assert.AreEqual(Visibility.Visible, paneRoot.Visibility);
            Assert.AreEqual(-248, paneClipTransform.X);
        }
        else
        {
            Assert.AreEqual("ClosedCompactRight", GetCurrentStateName(layoutRoot, "DisplayModeStates"));
            Assert.AreEqual(GridUnitType.Star, columnDefinition1.Width.GridUnitType);
            Assert.AreEqual(new GridLength(48), columnDefinition2.Width);
            Assert.AreEqual(1, Grid.GetColumnSpan(contentRoot));
            Assert.AreEqual(Visibility.Visible, paneRoot.Visibility);
            Assert.AreEqual(2, Grid.GetColumnSpan(paneRoot));
            Assert.AreEqual(HorizontalAlignment.Right, paneRoot.HorizontalAlignment);
            Assert.AreEqual(248, paneClipTransform.X);
        }
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected SplitView template part '{name}'.");
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, object resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertBrushMatchesResource(FrameworkElement element, object resourceKey, Brush actualBrush)
    {
        var expectedBrush = element.TryFindResource(resourceKey);
        Assert.IsInstanceOfType(expectedBrush, typeof(SolidColorBrush), $"{resourceKey} should resolve to a SolidColorBrush.");
        Assert.IsInstanceOfType(actualBrush, typeof(SolidColorBrush), $"{resourceKey} target should be a SolidColorBrush.");

        var expectedSolidBrush = (SolidColorBrush)expectedBrush;
        var actualSolidBrush = (SolidColorBrush)actualBrush;
        Assert.AreEqual(expectedSolidBrush.Color, actualSolidBrush.Color, resourceKey.ToString());
        Assert.AreEqual(expectedSolidBrush.Opacity, actualSolidBrush.Opacity, resourceKey.ToString());
    }

    private static void RaiseMouseLeftButtonUp(UIElement target)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonUpEvent,
            Source = target
        };

        target.RaiseEvent(args);
    }

    private static void RaiseKeyDown(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), Environment.TickCount, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent,
            Source = target
        };

        target.RaiseEvent(args);
    }

    private static void WaitFor(Func<bool> predicate, string failureMessage, int timeoutMilliseconds = 1500)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!predicate() && stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
        {
            WpfTestHost.DoEvents();
        }

        Assert.IsTrue(predicate(), failureMessage);
    }
}
