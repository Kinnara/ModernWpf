using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.ViewModels;
using Mux = ModernWpf.Controls;
using TeachingTipControl = ModernWpf.Controls.TeachingTip;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryAutomationHookTests
    {
        [TestMethod]
        public void GalleryAutomationIdsAreReservedForCuratedSampleDiagnostics()
        {
            WpfTestHost.Run(() =>
            {
                var sampleRoot = new Border();
                GalleryAutomation.WithAutomationId(sampleRoot, GalleryAutomation.SampleRootId("Example"));
                Assert.AreEqual("GallerySample_Example_Root", AutomationProperties.GetAutomationId(sampleRoot));

                var sampleElement = new Button();
                GalleryAutomation.WithAutomationId(sampleElement, GalleryAutomation.SampleElementId("Example", "Button"));
                Assert.AreEqual("GallerySample_Example_Button", AutomationProperties.GetAutomationId(sampleElement));

                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.WithAutomationId(new Border(), "ExampleButton"));
                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.WithAutomationId(new Border(), string.Empty));
                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.WithAutomationId(new Border(), "GallerySample_Example"));
                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.WithAutomationId(new Border(), "GallerySample_Example_Button_Extra"));
                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.WithAutomationId(new Border(), "GallerySample_Example Button"));
                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.SampleRootId("Bad_Name"));
                Assert.ThrowsExactly<ArgumentException>(() =>
                    GalleryAutomation.SampleElementId("Example", string.Empty));
            });
        }

        public static IEnumerable<object[]> CuratedSampleAutomationIds()
        {
            yield return new object[] { "TeachingTip", "GallerySample_TeachingTip_Root", "GallerySample_TeachingTip_ShowButton" };
            yield return new object[] { "Button", "GallerySample_Button_Root", "GallerySample_Button_PrimaryButton" };
            yield return new object[] { "CheckBox", "GallerySample_CheckBox_Root", "GallerySample_CheckBox_CheckBox" };
            yield return new object[] { "ComboBox", "GallerySample_ComboBox_Root", "GallerySample_ComboBox_ComboBox" };
            yield return new object[] { "RadioButton", "GallerySample_RadioButton_Root", "GallerySample_RadioButton_RadioButton" };
            yield return new object[] { "Slider", "GallerySample_Slider_Root", "GallerySample_Slider_Slider" };
            yield return new object[] { "InfoBadge", "GallerySample_InfoBadge_Root", "GallerySample_InfoBadge_InfoBadge" };
            yield return new object[] { "InfoBar", "GallerySample_InfoBar_Root", "GallerySample_InfoBar_InfoBar" };
            yield return new object[] { "ProgressRing", "GallerySample_ProgressRing_Root", "GallerySample_ProgressRing_ProgressRing" };
            yield return new object[] { "WinUIProgressBar", "GallerySample_WinUIProgressBar_Root", "GallerySample_WinUIProgressBar_DeterminateProgressBar" };
            yield return new object[] { "AnnotatedScrollBar", "GallerySample_AnnotatedScrollBar_Root", "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar" };
            yield return new object[] { "SplitView", "GallerySample_SplitView_Root", "GallerySample_SplitView_SplitView" };
            yield return new object[] { "PersonPicture", "GallerySample_PersonPicture_Root", "GallerySample_PersonPicture_PersonPicture" };
            yield return new object[] { "IconElement", "GallerySample_IconElement_Root", "GallerySample_IconElement_SlicesIcon" };
            yield return new object[] { "ThemeShadow", "GallerySample_ThemeShadow_Root", "GallerySample_ThemeShadow_ShadowRect" };
            yield return new object[] { "TitleBar", "GallerySample_TitleBar_Root", "GallerySample_TitleBar_TitleBarControl" };
            yield return new object[] { "GridView", "GallerySample_GridView_Root", "GallerySample_GridView_BasicGridView" };
            yield return new object[] { "ItemsRepeater", "GallerySample_ItemsRepeater_Root", "GallerySample_ItemsRepeater_ItemsRepeater" };
            yield return new object[] { "BreadcrumbBar", "GallerySample_BreadcrumbBar_Root", "GallerySample_BreadcrumbBar_BreadcrumbBar" };
            yield return new object[] { "SelectorBar", "GallerySample_SelectorBar_Root", "GallerySample_SelectorBar_SelectorBar" };
            yield return new object[] { "NavigationView", "GallerySample_NavigationView_Root", "GallerySample_NavigationView_NavigationView" };
            yield return new object[] { "ContentDialog", "GallerySample_ContentDialog_Root", "GallerySample_ContentDialog_ShowButton" };
            yield return new object[] { "Flyout", "GallerySample_Flyout_Root", "GallerySample_Flyout_Button" };
            yield return new object[] { "Popup", "GallerySample_Popup_Root", "GallerySample_Popup_Button" };
            yield return new object[] { "ColorPicker", "GallerySample_ColorPicker_Root", "GallerySample_ColorPicker_ColorPicker" };
            yield return new object[] { "HyperlinkButton", "GallerySample_HyperlinkButton_Root", "GallerySample_HyperlinkButton_HyperlinkButton" };
            yield return new object[] { "RatingControl", "GallerySample_RatingControl_Root", "GallerySample_RatingControl_RatingControl" };
            yield return new object[] { "RepeatButton", "GallerySample_RepeatButton_Root", "GallerySample_RepeatButton_RepeatButton" };
            yield return new object[] { "ToggleButton", "GallerySample_ToggleButton_Root", "GallerySample_ToggleButton_ToggleButton" };
            yield return new object[] { "DropDownButton", "GallerySample_DropDownButton_Root", "GallerySample_DropDownButton_DropDownButton" };
            yield return new object[] { "SplitButton", "GallerySample_SplitButton_Root", "GallerySample_SplitButton_SplitButton" };
            yield return new object[] { "ToggleSplitButton", "GallerySample_ToggleSplitButton_Root", "GallerySample_ToggleSplitButton_ToggleSplitButton" };
            yield return new object[] { "ToggleSwitch", "GallerySample_ToggleSwitch_Root", "GallerySample_ToggleSwitch_ToggleSwitch" };
            yield return new object[] { "NumberBox", "GallerySample_NumberBox_Root", "GallerySample_NumberBox_SpinButtonNumberBox" };
            yield return new object[] { "AutoSuggestBox", "GallerySample_AutoSuggestBox_Root", "GallerySample_AutoSuggestBox_AutoSuggestBox" };
            yield return new object[] { "MenuBar", "GallerySample_MenuBar_Root", "GallerySample_MenuBar_MenuBar" };
            yield return new object[] { "MenuFlyout", "GallerySample_MenuFlyout_Root", "GallerySample_MenuFlyout_AppBarButton" };
            yield return new object[] { "AppBarButton", "GallerySample_AppBarButton_Root", "GallerySample_AppBarButton_AppBarButton" };
            yield return new object[] { "AppBarSeparator", "GallerySample_AppBarSeparator_Root", "GallerySample_AppBarSeparator_CommandBar" };
            yield return new object[] { "AppBarToggleButton", "GallerySample_AppBarToggleButton_Root", "GallerySample_AppBarToggleButton_AppBarToggleButton" };
            yield return new object[] { "CommandBar", "GallerySample_CommandBar_Root", "GallerySample_CommandBar_CommandBar" };
            yield return new object[] { "CommandBarFlyout", "GallerySample_CommandBarFlyout_Root", "GallerySample_CommandBarFlyout_ShowButton" };
        }

        public static IEnumerable<object[]> WinUIPortedControlExampleCounts()
        {
            yield return new object[] { "NavigationView", 8 };
            yield return new object[] { "InfoBar", 3 };
            yield return new object[] { "NumberBox", 3 };
            yield return new object[] { "AutoSuggestBox", 2 };
            yield return new object[] { "ContentDialog", 2 };
            yield return new object[] { "TeachingTip", 3 };
            yield return new object[] { "CommandBar", 1 };
            yield return new object[] { "CommandBarFlyout", 1 };
            yield return new object[] { "AppBarButton", 6 };
            yield return new object[] { "AppBarToggleButton", 4 };
            yield return new object[] { "AppBarSeparator", 1 };
            yield return new object[] { "DropDownButton", 2 };
            yield return new object[] { "SplitButton", 2 };
            yield return new object[] { "ToggleSplitButton", 1 };
            yield return new object[] { "RepeatButton", 1 };
            yield return new object[] { "ToggleButton", 1 };
            yield return new object[] { "MenuBar", 3 };
            yield return new object[] { "MenuFlyout", 7 };
            yield return new object[] { "ItemsRepeater", 6 };
            yield return new object[] { "RatingControl", 2 };
            yield return new object[] { "ToggleSwitch", 2 };
            yield return new object[] { "ColorPicker", 1 };
            yield return new object[] { "HyperlinkButton", 2 };
            yield return new object[] { "ProgressRing", 2 };
            yield return new object[] { "WinUIProgressBar", 2 };
            yield return new object[] { "InfoBadge", 4 };
            yield return new object[] { "Flyout", 1 };
            yield return new object[] { "Popup", 1 };
            yield return new object[] { "BreadcrumbBar", 2 };
            yield return new object[] { "SelectorBar", 3 };
            yield return new object[] { "SplitView", 1 };
            yield return new object[] { "AnnotatedScrollBar", 1 };
            yield return new object[] { "GridView", 3 };
            yield return new object[] { "PersonPicture", 1 };
            yield return new object[] { "IconElement", 6 };
            yield return new object[] { "ThemeShadow", 1 };
            yield return new object[] { "TitleBar", 2 };
        }

        public static IEnumerable<object[]> WinUIPortedControlExampleSupplementalContent()
        {
            yield return new object[] { "TeachingTip", "NNN" };
            yield return new object[] { "ColorPicker", "P" };
            yield return new object[] { "HyperlinkButton", "PN" };
            yield return new object[] { "RatingControl", "BP" };
            yield return new object[] { "RepeatButton", "P" };
            yield return new object[] { "ToggleButton", "B" };
            yield return new object[] { "DropDownButton", "NN" };
            yield return new object[] { "SplitButton", "PN" };
            yield return new object[] { "ToggleSplitButton", "P" };
            yield return new object[] { "ToggleSwitch", "NN" };
            yield return new object[] { "NumberBox", "NPN" };
            yield return new object[] { "AutoSuggestBox", "NN" };
            yield return new object[] { "SplitView", "P" };
            yield return new object[] { "PersonPicture", "P" };
            yield return new object[] { "IconElement", "PNNNNN" };
            yield return new object[] { "ThemeShadow", "P" };
            yield return new object[] { "TitleBar", "PN" };
            yield return new object[] { "InfoBadge", "PPNP" };
            yield return new object[] { "InfoBar", "PPP" };
            yield return new object[] { "ProgressRing", "PP" };
            yield return new object[] { "WinUIProgressBar", "PN" };
            yield return new object[] { "AnnotatedScrollBar", "P" };
            yield return new object[] { "GridView", "NPP" };
            yield return new object[] { "ItemsRepeater", "PPNNNN" };
            yield return new object[] { "BreadcrumbBar", "NP" };
            yield return new object[] { "SelectorBar", "NNN" };
            yield return new object[] { "NavigationView", "NNNNNPPP" };
            yield return new object[] { "ContentDialog", "NN" };
            yield return new object[] { "Flyout", "N" };
            yield return new object[] { "Popup", "P" };
            yield return new object[] { "MenuBar", "NNN" };
            yield return new object[] { "MenuFlyout", "NNNNNNN" };
            yield return new object[] { "AppBarButton", "NNNNNN" };
            yield return new object[] { "AppBarSeparator", "N" };
            yield return new object[] { "AppBarToggleButton", "NNNN" };
            yield return new object[] { "CommandBar", "P" };
            yield return new object[] { "CommandBarFlyout", "N" };
        }

        [TestMethod]
        [DynamicData(nameof(WinUIPortedControlExampleSupplementalContent))]
        public void EveryWinUIPortedControlExampleUsesTheSourceOutputAndOptionsSlots(string uniqueId, string expectedShape)
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
                var actualShape = new string(page.Examples.Select(example =>
                {
                    var hasOutput = example.OutputContent != null;
                    var hasOptions = example.OptionsContent != null;
                    return hasOutput && hasOptions ? 'B' : hasOutput ? 'O' : hasOptions ? 'P' : 'N';
                }).ToArray());

                Assert.AreEqual(
                    expectedShape,
                    actualShape,
                    uniqueId + " supplemental content differs from the current WinUI Gallery ControlExample structure (N=none, O=output, P=options, B=both)." );
            });
        }

        [TestMethod]
        [DynamicData(nameof(WinUIPortedControlExampleCounts))]
        public void EveryWinUIPortedControlExampleExposesStableVisualArtifactIdentity(string uniqueId, int expectedCount)
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(expectedCount, page.Examples.Count, uniqueId + " source example count changed.");
                    Assert.AreEqual(
                        expectedCount,
                        FindDescendants<ControlExample>(page).Count(),
                        uniqueId + " did not render every GalleryExample through ControlExample.");

                    for (var index = 0; index < expectedCount; index++)
                    {
                        var expectedAutomationId = "GallerySample_" + uniqueId + "_Example" + (index + 1).ToString(CultureInfo.InvariantCulture);
                        Assert.AreEqual(expectedAutomationId, page.Examples[index].AutomationId);

                        var example = FindByAutomationId(page, expectedAutomationId) as ControlExample;
                        Assert.IsNotNull(example, expectedAutomationId + " is missing.");
                        Assert.AreEqual(page.Examples[index].HeaderText, AutomationProperties.GetName(example));
                        Assert.IsTrue(example.ActualWidth > 0, expectedAutomationId + " rendered with zero width.");
                        Assert.IsTrue(example.ActualHeight > 0, expectedAutomationId + " rendered with zero height.");
                    }
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void SliderSampleStartsAtReferenceValue()
        {
            WpfTestHost.Run(() =>
            {
                var viewModel = new ModernWpf.Gallery.Pages.WpfGallery.BasicInput.SliderPageViewModel();
                var page = new ModernWpf.Gallery.Pages.WpfGallery.BasicInput.SliderPage(viewModel);
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };
                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    WaitForRendering();

                    var slider = FindNamedDescendant<Slider>(page, "SimpleSlider");

                    Assert.AreEqual(0, viewModel.SimpleSliderValue);
                    Assert.AreEqual(0.0, slider.Value);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        [DynamicData(nameof(CuratedSampleAutomationIds))]
        public void CuratedSamplesExposeStableAutomationIds(string uniqueId, string rootAutomationId, string primaryAutomationId)
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var pageHeader = FindDescendants<PageHeader>(page)
                        .Single(header => header.IsVisible);
                    Assert.IsNotNull(pageHeader, "Item page header is missing.");
                    pageHeader.ApplyTemplate();
                    var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
                    Assert.IsNotNull(titleLabel, "Item page title label is missing.");
                    Assert.AreEqual(page.Title + " Page", AutomationProperties.GetName(titleLabel));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleLabel));
                    Assert.IsTrue(KeyboardNavigation.GetIsTabStop(titleLabel));
                    Assert.AreEqual(0, KeyboardNavigation.GetTabIndex(titleLabel));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId((TextBlock)titleLabel.Content));
                    Assert.IsNull(
                        FindByAutomationId(page, "GallerySampleHost"),
                        "The generic ItemPage wrapper should not expose a local-only sample host AutomationId.");
                    var sampleRoot = FindByAutomationId(page, rootAutomationId) as UIElement;
                    Assert.IsNotNull(sampleRoot, rootAutomationId + " is missing.");
                    var sampleRootPeer = UIElementAutomationPeer.CreatePeerForElement(sampleRoot);
                    Assert.IsNotNull(sampleRootPeer, rootAutomationId + " has no automation peer.");
                    Assert.IsTrue(sampleRootPeer.IsControlElement(), rootAutomationId + " is not exposed in the UI Automation control view.");
                    Assert.AreEqual(AutomationControlType.Group, sampleRootPeer.GetAutomationControlType());
                    Assert.IsNotNull(FindByAutomationId(page, primaryAutomationId), primaryAutomationId + " is missing.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void TeachingTipSampleButtonOpensTip()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("TeachingTip"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("Show a targeted TeachingTip on a button.", page.Examples[0].HeaderText);
                    Assert.AreEqual("Show a non-targeted TeachingTip with buttons.", page.Examples[1].HeaderText);
                    Assert.AreEqual("Show a targeted TeachingTip with hero content on a button.", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "TestButton1TeachingTip");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ActionButtonContent=\"Action button\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "TeachingTip.HeroContent");

                    var button = (ButtonBase)FindByAutomationId(page, "GallerySample_TeachingTip_ShowButton");
                    var teachingTip = (TeachingTipControl)FindByAutomationId(page, "GallerySample_TeachingTip_TeachingTip");
                    var nonTargetedButton = (ButtonBase)FindByAutomationId(page, "GallerySample_TeachingTip_NonTargetedShowButton");
                    var nonTargetedTeachingTip = (TeachingTipControl)FindByAutomationId(page, "GallerySample_TeachingTip_NonTargetedTeachingTip");
                    var heroButton = (ButtonBase)FindByAutomationId(page, "GallerySample_TeachingTip_HeroShowButton");
                    var heroTeachingTip = (TeachingTipControl)FindByAutomationId(page, "GallerySample_TeachingTip_HeroTeachingTip");

                    Assert.IsNotNull(button);
                    Assert.IsNotNull(teachingTip);
                    Assert.IsNotNull(nonTargetedButton);
                    Assert.IsNotNull(nonTargetedTeachingTip);
                    Assert.IsNotNull(heroButton);
                    Assert.IsNotNull(heroTeachingTip);
                    Assert.AreSame(DependencyProperty.UnsetValue, ((Control)button).ReadLocalValue(Control.PaddingProperty));
                    Assert.AreEqual(48.0, teachingTip.TryFindResource("TeachingTipMinWidth"));
                    Assert.IsTrue(nonTargetedTeachingTip.IsLightDismissEnabled);
                    Assert.AreEqual("Action button", nonTargetedTeachingTip.ActionButtonContent);
                    Assert.AreEqual("Close button", nonTargetedTeachingTip.CloseButtonContent);
                    Assert.AreEqual(new Thickness(20), nonTargetedTeachingTip.PlacementMargin);
                    Assert.AreEqual(ModernWpf.Controls.TeachingTipPlacementMode.Auto, nonTargetedTeachingTip.PreferredPlacement);
                    Assert.AreEqual(ModernWpf.Controls.TeachingTipPlacementMode.Bottom, heroTeachingTip.PreferredPlacement);
                    Assert.IsInstanceOfType(heroTeachingTip.HeroContent, typeof(Image));
                    Assert.AreEqual("Sunset", AutomationProperties.GetName(heroTeachingTip.HeroContent));

                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();

                    Assert.IsTrue(teachingTip.IsOpen);
                    teachingTip.IsOpen = false;
                    WpfTestHost.DoEvents();

                    var buttonPeer = UIElementAutomationPeer.CreatePeerForElement(button) as IInvokeProvider;
                    Assert.IsNotNull(buttonPeer);
                    buttonPeer.Invoke();
                    WpfTestHost.DoEvents();

                    Assert.IsTrue(teachingTip.IsOpen);
                    var popupRoot = teachingTip.Template.FindName("TailOcclusionGrid", teachingTip) as FrameworkElement;
                    Assert.IsNotNull(popupRoot);
                    Assert.AreEqual(48.0, popupRoot.MinWidth);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ContentDialogSampleMatchesWinUIGalleryFirstExampleButton()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ContentDialog"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A basic content dialog with content.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A content dialog without a default button.", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].CSharpCode, "DefaultButton = ContentDialogButton.Primary");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "DefaultButton = ContentDialogButton.None");

                    var button = (Button)FindByAutomationId(page, "GallerySample_ContentDialog_ShowButton");
                    var noDefaultButton = (Button)FindByAutomationId(page, "GallerySample_ContentDialog_ShowNoDefaultButton");
                    Assert.IsNotNull(button);
                    Assert.IsNotNull(noDefaultButton);
                    Assert.AreEqual("Show dialog", button.Content);
                    Assert.AreEqual("Show dialog without default button", noDefaultButton.Content);
                    Assert.AreSame(DependencyProperty.UnsetValue, button.ReadLocalValue(Control.PaddingProperty));
                    Assert.AreSame(DependencyProperty.UnsetValue, noDefaultButton.ReadLocalValue(Control.PaddingProperty));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void FlyoutSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Flyout"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("A button with a flyout", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<Button Content=\"Empty cart\">");
                    StringAssert.Contains(page.Examples[0].XamlCode, "DeleteConfirmation_Click");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Yes, empty my cart");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "this.Control1.Flyout is Flyout f");

                    var root = (FrameworkElement)FindByAutomationId(page, "GallerySample_Flyout_Root");
                    var button = (Button)FindByAutomationId(page, "GallerySample_Flyout_Button");
                    Assert.IsNotNull(root);
                    Assert.IsNotNull(button);
                    Assert.AreEqual("Control1", button.Name);
                    Assert.AreEqual("Empty cart", button.Content);
                    var buttonPeer = new ButtonAutomationPeer(button);
                    Assert.AreEqual(AutomationControlType.Button, buttonPeer.GetAutomationControlType());
                    Assert.AreEqual("Empty cart", buttonPeer.GetName());
                    Assert.IsInstanceOfType(buttonPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));

                    var sharedFlyout = root.Resources["SharedFlyout"] as Mux.Flyout;
                    Assert.IsNotNull(sharedFlyout);
                    var sharedPanel = sharedFlyout.Content as StackPanel;
                    Assert.IsNotNull(sharedPanel);
                    Assert.AreEqual(1, sharedPanel.Children.Count);
                    Assert.AreEqual("This Flyout is shared.", ((TextBlock)sharedPanel.Children[0]).Text);

                    var flyout = Mux.FlyoutService.GetFlyout(button) as Mux.Flyout;
                    Assert.IsNotNull(flyout);
                    var flyoutPanel = flyout.Content as StackPanel;
                    Assert.IsNotNull(flyoutPanel);
                    Assert.AreEqual(2, flyoutPanel.Children.Count);
                    var flyoutText = (TextBlock)flyoutPanel.Children[0];
                    Assert.AreEqual("All items will be removed. Do you want to continue?", flyoutText.Text);
                    Assert.AreEqual(new Thickness(0, 0, 0, 12), flyoutText.Margin);
                    var confirmButton = (Button)flyoutPanel.Children[1];
                    Assert.AreEqual("Yes, empty my cart", confirmButton.Content);

                    var flyoutTextPeer = new TextBlockAutomationPeer(flyoutText);
                    Assert.AreEqual(AutomationControlType.Text, flyoutTextPeer.GetAutomationControlType());
                    Assert.AreEqual(flyoutText.Text, flyoutTextPeer.GetName());
                    var confirmButtonPeer = new ButtonAutomationPeer(confirmButton);
                    Assert.AreEqual(AutomationControlType.Button, confirmButtonPeer.GetAutomationControlType());
                    Assert.AreEqual("Yes, empty my cart", confirmButtonPeer.GetName());
                    Assert.IsInstanceOfType(confirmButtonPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));

                    flyout.ShowAt(button);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(flyout.IsOpen);
                    confirmButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(flyout.IsOpen);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void PopupSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Popup"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("Popup with Offset Positioning", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"StandardPopup\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "ShowPopupOffsetClicked");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsLightDismissEnabled=\"$(IsLightDismissEnabled)\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }");

                    var popupOptions = (StackPanel)page.Examples[0].OptionsContent;
                    Assert.AreEqual(3, popupOptions.Children.Count);
                    Assert.AreEqual(new Thickness(0, 0, 0, 8), ((FrameworkElement)popupOptions.Children[0]).Margin);
                    Assert.AreEqual(new Thickness(0, 0, 0, 8), ((FrameworkElement)popupOptions.Children[1]).Margin);
                    Assert.AreEqual(new Thickness(), ((FrameworkElement)popupOptions.Children[2]).Margin);

                    var root = (FrameworkElement)FindByAutomationId(page, "GallerySample_Popup_Root");
                    var showButton = (Button)FindByAutomationId(page, "GallerySample_Popup_Button");
                    var output = FindNamedDescendant<Grid>(page, "Output");
                    var popup = FindNamedDescendant<Popup>(page, "StandardPopup");
                    var lightDismiss = FindNamedDescendant<Mux.ToggleSwitch>(page, "IsLightDismissEnabledToggleSwitch");
                    var verticalOffset = FindNamedDescendant<Mux.NumberBox>(page, "VerticalOffset");
                    var horizontalOffset = FindNamedDescendant<Mux.NumberBox>(page, "HorizontalOffset");
                    Assert.IsNotNull(root);
                    Assert.IsNotNull(showButton);
                    Assert.IsNotNull(output);
                    Assert.IsNotNull(popup);
                    Assert.IsNotNull(lightDismiss);
                    Assert.IsNotNull(verticalOffset);
                    Assert.IsNotNull(horizontalOffset);

                    Assert.AreEqual("Show Popup (using Offset)", showButton.Content);
                    Assert.AreEqual(HorizontalAlignment.Left, output.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Top, output.VerticalAlignment);
                    Assert.AreSame(showButton, popup.PlacementTarget);
                    Assert.AreEqual(PlacementMode.Bottom, popup.Placement);
                    Assert.IsTrue(popup.AllowsTransparency);
                    Assert.IsFalse(popup.StaysOpen);
                    Assert.AreEqual(200.0, popup.HorizontalOffset);
                    Assert.AreEqual(0.0, popup.VerticalOffset);

                    Assert.AreEqual("IsLightDismissEnabled", lightDismiss.Header);
                    Assert.AreEqual("True", lightDismiss.OnContent);
                    Assert.AreEqual("False", lightDismiss.OffContent);
                    Assert.IsTrue(lightDismiss.IsOn);
                    Assert.IsTrue(lightDismiss.IsEnabled);

                    AssertPopupOffsetNumberBox(verticalOffset, "VerticalOffset", -100, 100, 0);
                    AssertPopupOffsetNumberBox(horizontalOffset, "HorizontalOffset", -100, 500, 200);

                    var surface = popup.Child as Border;
                    Assert.IsNotNull(surface);
                    Assert.AreEqual(240.0, surface.MinWidth);
                    Assert.AreEqual(new Thickness(16), surface.Padding);
                    Assert.AreEqual(new Thickness(1), surface.BorderThickness);
                    var surfacePanel = surface.Child as StackPanel;
                    Assert.IsNotNull(surfacePanel);
                    Assert.AreEqual(2, surfacePanel.Children.Count);
                    var heading = (TextBlock)surfacePanel.Children[0];
                    var closeButton = (Button)surfacePanel.Children[1];
                    Assert.AreEqual("Simple Popup", heading.Text);
                    Assert.AreEqual(16.0, heading.FontSize);
                    Assert.AreEqual(22.0, heading.MinHeight);
                    Assert.AreEqual("Close", closeButton.Content);

                    var headingPeer = new TextBlockAutomationPeer(heading);
                    Assert.AreEqual("Simple Popup", headingPeer.GetName());
                    Assert.AreEqual(AutomationControlType.Text, headingPeer.GetAutomationControlType());
                    var closePeer = new ButtonAutomationPeer(closeButton);
                    Assert.AreEqual("Close", closePeer.GetName());
                    Assert.AreEqual(AutomationControlType.Button, closePeer.GetAutomationControlType());
                    Assert.IsInstanceOfType(closePeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));

                    verticalOffset.Value = 25;
                    horizontalOffset.Value = 175;
                    lightDismiss.IsOn = false;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(25.0, popup.VerticalOffset);
                    Assert.AreEqual(175.0, popup.HorizontalOffset);
                    Assert.IsTrue(popup.StaysOpen);

                    showButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(popup.IsOpen);
                    Assert.IsFalse(lightDismiss.IsEnabled);

                    closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(popup.IsOpen);
                    Assert.IsTrue(lightDismiss.IsEnabled);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void NavigationViewSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("NavigationView"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(8, page.Examples.Count);
                    Assert.AreEqual("NavigationView with default PaneDisplayMode", page.Examples[0].HeaderText);
                    Assert.AreEqual("NavigationView with PaneDisplayMode set to Top", page.Examples[1].HeaderText);
                    Assert.AreEqual("NavigationView that switches pane orientation based on window width", page.Examples[2].HeaderText);
                    Assert.AreEqual("Tying selection and focus - Tabs", page.Examples[3].HeaderText);
                    Assert.AreEqual("Data binding", page.Examples[4].HeaderText);
                    Assert.AreEqual("NavigationView with Footer Menu Items", page.Examples[5].HeaderText);
                    Assert.AreEqual("Hierarchical NavigationView", page.Examples[6].HeaderText);
                    Assert.AreEqual("API in action", page.Examples[7].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<NavigationView x:Name=\"nvSample\">");
                    StringAssert.Contains(page.Examples[1].XamlCode, "PaneDisplayMode=\"Top\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "AdaptiveTrigger");
                    StringAssert.Contains(page.Examples[3].XamlCode, "SelectionFollowsFocus=\"Enabled\"");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "FrameNavigationOptions");
                    StringAssert.Contains(page.Examples[4].XamlCode, "MenuItemsSource");
                    StringAssert.Contains(page.Examples[4].CSharpCode, "ObservableCollection<CategoryBase>");
                    StringAssert.Contains(page.Examples[5].XamlCode, "FooterMenuItems");
                    StringAssert.Contains(page.Examples[6].XamlCode, "SelectsOnInvoked=\"False\"");
                    StringAssert.Contains(page.Examples[7].XamlCode, "PaneFooter");

                    var navigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_NavigationView",
                        "nvSample5",
                        745.0,
                        460.0);
                    var topNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_TopNavigationView",
                        "nvSample6",
                        745.0,
                        460.0);
                    var adaptiveNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_AdaptiveNavigationView",
                        "nvSample2",
                        745.0,
                        460.0);
                    var tabsNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_TabsNavigationView",
                        "nvSample7",
                        745.0,
                        460.0);
                    var boundNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_DataBindingNavigationView",
                        "nvSample4",
                        745.0,
                        460.0);
                    var footerNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_FooterNavigationView",
                        "nvSample9",
                        592.0,
                        460.0);
                    var hierarchicalNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_HierarchicalNavigationView",
                        "nvSample8",
                        565.0,
                        460.0);
                    var apiNavigationView = AssertNavigationViewSampleArtifact(
                        page,
                        "GallerySample_NavigationView_ApiNavigationView",
                        "nvSample",
                        458.0,
                        540.0);

                    Assert.AreEqual(745.0, navigationView.Width);
                    Assert.AreEqual(460.0, navigationView.Height);
                    Assert.AreEqual(HorizontalAlignment.Left, navigationView.HorizontalAlignment);
                    Assert.AreEqual("Sample Page 1", navigationView.Header);
                    Assert.AreEqual(ModernWpf.Controls.NavigationViewBackButtonVisible.Auto, navigationView.IsBackButtonVisible);
                    Assert.IsFalse(navigationView.IsTitleBarAutoPaddingEnabled);
                    Assert.AreEqual(0.0, navigationView.TemplateSettings.TopPadding);
                    Assert.AreEqual(ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto, navigationView.PaneDisplayMode);
                    Assert.AreEqual(4, navigationView.MenuItems.Count);
                    var contentFrame5 = FindNamedDescendant<Frame>(page, "contentFrame5");
                    Assert.AreSame(contentFrame5, navigationView.Content);
                    var firstContent = contentFrame5.Content as ScrollViewer;
                    Assert.IsNotNull(firstContent);
                    Assert.AreEqual(ScrollBarVisibility.Auto, firstContent.VerticalScrollBarVisibility);
                    var bodyText = FindDescendants<TextBlock>(firstContent)
                        .Single(textBlock => textBlock.Text.StartsWith("Lorem ipsum", StringComparison.Ordinal));
                    Assert.AreEqual(new Thickness(0, 1, 0, 0), bodyText.Margin);
                    Assert.AreSame(page.TryFindResource("BodyTextBlockStyle"), bodyText.Style);
                    Assert.AreSame(page.TryFindResource("TextFillColorPrimaryBrush"), bodyText.Foreground);

                    var firstItem = (ModernWpf.Controls.NavigationViewItem)navigationView.MenuItems[0];
                    Assert.AreEqual("Menu Item1", firstItem.Content);
                    Assert.AreEqual("SamplePage1", firstItem.Tag);
                    Assert.AreEqual(ModernWpf.Controls.Symbol.Play, ((ModernWpf.Controls.SymbolIcon)firstItem.Icon).Symbol);
                    Assert.AreSame(firstItem, navigationView.SelectedItem);
                    AssertClosedCompactNavigationViewGeometry(
                        navigationView,
                        "Menu Item1",
                        "The default NavigationView sample");
                    var secondItem = (ModernWpf.Controls.NavigationViewItem)navigationView.MenuItems[1];
                    Assert.AreEqual("Menu Item2", secondItem.Content);
                    Assert.AreEqual("SamplePage2", secondItem.Tag);
                    Assert.AreEqual(ModernWpf.Controls.Symbol.Save, ((ModernWpf.Controls.SymbolIcon)secondItem.Icon).Symbol);

                    navigationView.SelectedItem = secondItem;
                    WpfTestHost.DoEvents();
                    Assert.AreSame(secondItem, navigationView.SelectedItem);
                    Assert.AreEqual("Sample Page 2", navigationView.Header);
                    Assert.AreSame(contentFrame5, navigationView.Content);

                    navigationView.SelectedItem = firstItem;
                    WpfTestHost.DoEvents();
                    Assert.AreSame(firstItem, navigationView.SelectedItem);
                    Assert.AreEqual("Sample Page 1", navigationView.Header);

                    var leftSelectionIndicator = FindNamedDescendant<System.Windows.Shapes.Rectangle>(firstItem, "SelectionIndicator");
                    var leftSelectionIndicatorBounds = GetRelativeBounds(leftSelectionIndicator, firstItem);
                    var leftSelectionIndicatorBoundsInNavigationView = GetRelativeBounds(leftSelectionIndicator, navigationView);
                    Assert.AreEqual(
                        4.0,
                        leftSelectionIndicatorBounds.Left,
                        0.01,
                        "The default Gallery sample must keep the WinUI left selection indicator at the item background origin.");
                    Assert.AreEqual(
                        4.0,
                        leftSelectionIndicatorBoundsInNavigationView.Left,
                        0.01,
                        "The default Gallery sample must match the WinUI selection indicator's horizontal origin.");

                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, topNavigationView.PaneDisplayMode);
                    Assert.AreEqual("This is Header Text", topNavigationView.Header);
                    Assert.AreEqual(4, topNavigationView.MenuItems.Count);
                    AssertTopNavigationViewGeometry(
                        topNavigationView,
                        "Menu Item1",
                        "The Top NavigationView sample");

                    Assert.IsTrue(adaptiveNavigationView.ActualWidth >= adaptiveNavigationView.CompactModeThresholdWidth);
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, adaptiveNavigationView.PaneDisplayMode);
                    Assert.AreEqual(4, adaptiveNavigationView.MenuItems.Count);
                    AssertTopNavigationViewGeometry(
                        adaptiveNavigationView,
                        "Menu Item1",
                        "The adaptive NavigationView sample at Gallery width");

                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, tabsNavigationView.PaneDisplayMode);
                    Assert.AreEqual(Mux.NavigationViewBackButtonVisible.Collapsed, tabsNavigationView.IsBackButtonVisible);
                    Assert.AreEqual(Mux.NavigationViewSelectionFollowsFocus.Enabled, tabsNavigationView.SelectionFollowsFocus);
                    AssertTopNavigationViewGeometry(
                        tabsNavigationView,
                        "Item1",
                        "The tabs NavigationView sample");

                    Assert.IsNotNull(boundNavigationView.MenuItemsSource);
                    Assert.IsNotNull(boundNavigationView.MenuItemTemplate);
                    Assert.IsNotNull(boundNavigationView.SelectedItem);
                    Assert.AreEqual("Sample Page 1", boundNavigationView.Header);

                    var boundCategories = ((System.Collections.IEnumerable)boundNavigationView.MenuItemsSource)
                        .Cast<object>()
                        .ToArray();
                    boundNavigationView.SelectedItem = boundCategories[1];
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Sample Page 2", boundNavigationView.Header);
                    boundNavigationView.SelectedItem = boundCategories[0];
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Sample Page 1", boundNavigationView.Header);

                    boundNavigationView.BringIntoView();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    var boundFirstItem = FindDescendants<Mux.NavigationViewItem>(boundNavigationView)
                        .Single(item => Equals(item.Content, "Category 1"));
                    var boundPaneContentGrid = FindNamedDescendant<Border>(boundNavigationView, "PaneContentGrid");
                    var boundItemContentPresenter = FindNamedDescendant<Mux.ContentPresenterEx>(
                        boundFirstItem,
                        "ContentPresenter");
                    var boundSettingsItem = (Mux.NavigationViewItem)boundNavigationView.SettingsItem;
                    var boundSettingsContentPresenter = FindNamedDescendant<Mux.ContentPresenterEx>(
                        boundSettingsItem,
                        "ContentPresenter");
                    var boundPaneBounds = GetRelativeBounds(boundPaneContentGrid, boundNavigationView);
                    var boundItemBounds = GetRelativeBounds(boundFirstItem, boundNavigationView);
                    var boundContentPresenterBounds = GetRelativeBounds(boundItemContentPresenter, boundNavigationView);
                    var boundSettingsContentPresenterBounds = GetRelativeBounds(
                        boundSettingsContentPresenter,
                        boundNavigationView);
                    var boundRootGrid = FindNamedDescendant<Grid>(boundNavigationView, "RootGrid");
                    var boundStateGroups = VisualStateManager.GetVisualStateGroups(boundRootGrid)
                        .Cast<VisualStateGroup>()
                        .ToArray();
                    var boundPaneState = boundStateGroups
                        .Single(group => group.Name == "PaneStateGroup")
                        .CurrentState;
                    var boundListSizeState = boundStateGroups
                        .Single(group => group.Name == "PaneStateListSizeGroup")
                        .CurrentState;

                    Assert.IsFalse(boundNavigationView.IsPaneOpen);
                    Assert.AreEqual(Mux.NavigationViewDisplayMode.Compact, boundNavigationView.DisplayMode);
                    Assert.AreEqual("ClosedCompact", boundPaneState?.Name);
                    Assert.AreEqual("ListSizeCompact", boundListSizeState?.Name);
                    Assert.AreEqual(
                        boundNavigationView.CompactPaneLength,
                        boundPaneBounds.Width,
                        0.01,
                        "A lazily realized data-bound sample must use the compact pane width while closed.");
                    Assert.AreEqual(
                        boundNavigationView.CompactPaneLength - 1.0,
                        boundItemBounds.Width,
                        0.01,
                        "Data-bound menu items must be constrained by the compact pane.");
                    Assert.AreEqual(
                        0.0,
                        boundContentPresenterBounds.Width,
                        0.01,
                        "The compact item must not leave any label width visible beside its icon.");
                    Assert.AreEqual(
                        0.0,
                        boundSettingsContentPresenterBounds.Width,
                        0.01,
                        "The compact Settings item must not leave any label width visible beside its icon.");

                    Assert.IsFalse(footerNavigationView.IsSettingsVisible);
                    Assert.AreEqual(3, footerNavigationView.MenuItems.Count);
                    Assert.AreEqual(3, footerNavigationView.FooterMenuItems.Count);
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Left, footerNavigationView.PaneDisplayMode);
                    AssertExpandedLeftNavigationViewGeometry(
                        footerNavigationView,
                        "Browse",
                        "The footer NavigationView sample",
                        expectSelection: true);
                    var trackOrderItem = (Mux.NavigationViewItem)footerNavigationView.MenuItems[1];
                    footerNavigationView.SelectedItem = trackOrderItem;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Sample Page 2", footerNavigationView.Header);
                    footerNavigationView.SelectedItem = footerNavigationView.MenuItems[0];
                    WpfTestHost.DoEvents();
                    var footerTop = FindNamedDescendant<RadioButton>(page, "nvSample9Top");
                    footerTop.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, footerNavigationView.PaneDisplayMode);
                    Assert.IsFalse(footerNavigationView.IsPaneOpen);
                    AssertTopNavigationViewGeometry(
                        footerNavigationView,
                        "Browse",
                        "The footer NavigationView sample in Top mode");

                    Assert.AreEqual(3, hierarchicalNavigationView.MenuItems.Count);
                    var accountItem = (Mux.NavigationViewItem)hierarchicalNavigationView.MenuItems[1];
                    var documentOptionsItem = (Mux.NavigationViewItem)hierarchicalNavigationView.MenuItems[2];
                    Assert.AreEqual(2, accountItem.MenuItems.Count);
                    Assert.AreEqual(2, documentOptionsItem.MenuItems.Count);
                    Assert.IsFalse(documentOptionsItem.SelectsOnInvoked);
                    Assert.AreEqual("Sample Page 1", hierarchicalNavigationView.Header);
                    hierarchicalNavigationView.SelectedItem = accountItem;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Sample Page 2", hierarchicalNavigationView.Header);
                    hierarchicalNavigationView.SelectedItem = hierarchicalNavigationView.MenuItems[0];
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Sample Page 1", hierarchicalNavigationView.Header);
                    AssertExpandedLeftNavigationViewGeometry(
                        hierarchicalNavigationView,
                        "Home",
                        "The hierarchical NavigationView sample",
                        expectSelection: true);
                    var hierarchicalTop = FindNamedDescendant<RadioButton>(page, "nvSample8Top");
                    hierarchicalTop.IsChecked = true;
                    WpfTestHost.DoEvents();
                    AssertTopNavigationViewGeometry(
                        hierarchicalNavigationView,
                        "Home",
                        "The hierarchical NavigationView sample in Top mode");
                    var hierarchicalLeftCompact = FindNamedDescendant<RadioButton>(page, "nvSample8LeftCompact");
                    hierarchicalLeftCompact.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.LeftCompact, hierarchicalNavigationView.PaneDisplayMode);
                    Assert.IsFalse(hierarchicalNavigationView.IsPaneOpen);
                    AssertClosedCompactNavigationViewGeometry(
                        hierarchicalNavigationView,
                        "Home",
                        "The hierarchical NavigationView sample in LeftCompact mode");

                    var samplePage2Item = FindNamedDescendant<Mux.NavigationViewItem>(page, "SamplePage2Item");
                    Assert.AreEqual("Header", apiNavigationView.Header);
                    Assert.AreEqual("Pane Title", apiNavigationView.PaneTitle);
                    Assert.AreEqual(Mux.NavigationViewBackButtonVisible.Visible, apiNavigationView.IsBackButtonVisible);
                    Assert.IsNotNull(apiNavigationView.AutoSuggestBox);
                    var apiContentFrame = FindNamedDescendant<Frame>(apiNavigationView, "contentFrame");
                    Assert.IsNull(apiContentFrame.Content, "The API sample should match WinUI's initially empty Frame.");
                    var paneTitleTextBlock = FindNamedDescendant<TextBlock>(apiNavigationView, "PaneTitleTextBlock");
                    Assert.AreEqual(
                        ((FontFamily)page.TryFindResource("ContentControlThemeFontFamily")).Source,
                        paneTitleTextBlock.FontFamily.Source,
                        "PaneTitle must not inherit the toggle button's symbol font.");
                    AssertExpandedLeftNavigationViewGeometry(
                        apiNavigationView,
                        "Menu Item1",
                        "The API NavigationView sample",
                        expectSelection: false);

                    var headerText = FindNamedDescendant<TextBox>(page, "headerText");
                    headerText.Text = "Updated Header";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Updated Header", apiNavigationView.Header);

                    var autoSuggestCheck = FindNamedDescendant<CheckBox>(page, "autoSuggestCheck");
                    autoSuggestCheck.IsChecked = false;
                    autoSuggestCheck.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsNull(apiNavigationView.AutoSuggestBox);

                    var paneHyperlink = FindNamedDescendant<Mux.HyperlinkButton>(page, "PaneHyperlink");
                    var paneCustomContentCheck = FindNamedDescendant<CheckBox>(page, "panemc_Check");
                    paneCustomContentCheck.IsChecked = true;
                    paneCustomContentCheck.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Visible, paneHyperlink.Visibility);

                    var footerStackPanel = FindNamedDescendant<StackPanel>(page, "FooterStackPanel");
                    var paneFooterCheck = FindNamedDescendant<CheckBox>(page, "paneFooterCheck");
                    paneFooterCheck.IsChecked = true;
                    paneFooterCheck.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Visible, footerStackPanel.Visibility);

                    var apiTop = FindNamedDescendant<RadioButton>(page, "nvSampleTop");
                    apiTop.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, apiNavigationView.PaneDisplayMode);
                    Assert.AreEqual(Orientation.Horizontal, footerStackPanel.Orientation);
                    apiNavigationView.SelectedItem = apiNavigationView.MenuItems[0];
                    WpfTestHost.DoEvents();
                    AssertTopNavigationViewGeometry(
                        apiNavigationView,
                        "Menu Item1",
                        "The API NavigationView sample in Top mode");
                    Assert.IsNotNull(apiContentFrame.Content, "Selecting an API sample item should populate its Frame.");

                    var selectionFollowsFocus = FindNamedDescendant<CheckBox>(page, "sffCheck");
                    selectionFollowsFocus.IsChecked = true;
                    selectionFollowsFocus.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewSelectionFollowsFocus.Enabled, apiNavigationView.SelectionFollowsFocus);

                    var suppressSelection = FindNamedDescendant<CheckBox>(page, "suppressselectionCheck_Checked");
                    suppressSelection.IsChecked = true;
                    suppressSelection.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(samplePage2Item.SelectsOnInvoked);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void BreadcrumbBarSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("BreadcrumbBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A BreadcrumbBar control", page.Examples[0].HeaderText);
                    Assert.AreEqual("BreadCrumbBar Control with Custom DataTemplate", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<BreadcrumbBar x:Name=\"BreadcrumbBar1\"/>");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "BreadcrumbBar1.ItemsSource = new string[]");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<BreadcrumbBar x:Name=\"BreadcrumbBar2\">");
                    StringAssert.Contains(page.Examples[1].XamlCode, "BreadcrumbBarItem Content=\"{Binding}\"");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "ObservableCollection<Folder>");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "BreadcrumbBar2_ItemClicked");

                    var breadcrumbBar1 = (Mux.BreadcrumbBar)FindByAutomationId(page, "GallerySample_BreadcrumbBar_BreadcrumbBar");
                    var breadcrumbBar2 = FindNamedDescendant<Mux.BreadcrumbBar>(page, "BreadcrumbBar2");
                    var resetSampleButton = FindNamedDescendant<Button>(page, "ResetSampleBtn");
                    Assert.IsNotNull(breadcrumbBar1);
                    Assert.IsNotNull(breadcrumbBar2);
                    Assert.IsNotNull(resetSampleButton);

                    Assert.AreEqual("BreadcrumbBar1", breadcrumbBar1.Name);
                    AssertBreadcrumbItems(
                        breadcrumbBar1.ItemsSource,
                        "Home",
                        "Documents",
                        "Design",
                        "Northwind",
                        "Images",
                        "Folder1",
                        "Folder2",
                        "Folder3");
                    Assert.IsNotNull(FindTextBlockByText(breadcrumbBar1, "Home"));
                    Assert.IsNotNull(FindTextBlockByText(breadcrumbBar1, "Folder3"));

                    Assert.AreEqual("BreadcrumbBar2", breadcrumbBar2.Name);
                    Assert.IsNotNull(breadcrumbBar2.ItemTemplate);
                    AssertBreadcrumbItems(breadcrumbBar2.ItemsSource, "Home", "Folder1", "Folder2", "Folder3");
                    Assert.IsNotNull(FindTextBlockByText(breadcrumbBar2, "Home"));
                    Assert.IsNotNull(FindTextBlockByText(breadcrumbBar2, "Folder3"));
                    Assert.AreEqual("Reset sample", resetSampleButton.Content);

                    var folders = breadcrumbBar2.ItemsSource as System.Collections.IList;
                    Assert.IsNotNull(folders);
                    folders.RemoveAt(3);
                    folders.RemoveAt(2);
                    WpfTestHost.DoEvents();
                    AssertBreadcrumbItems(breadcrumbBar2.ItemsSource, "Home", "Folder1");

                    resetSampleButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    AssertBreadcrumbItems(breadcrumbBar2.ItemsSource, "Home", "Folder1", "Folder2", "Folder3");
                    Assert.IsNotNull(FindTextBlockByText(breadcrumbBar2, "Folder3"));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void SelectorBarSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("SelectorBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("A Basic SelectorBar", page.Examples[0].HeaderText);
                    Assert.AreEqual("SelectorBar with Frame Slide Transitions", page.Examples[1].HeaderText);
                    Assert.AreEqual("SelectorBar Displaying Different Collections Using ItemsView", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "SelectorBarItemRecent");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Icon=\"Clock\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ContentFrame");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "SelectorBar2_SelectionChanged");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "SlideNavigationTransitionInfo");
                    StringAssert.Contains(page.Examples[2].XamlCode, "ItemsView3");
                    StringAssert.Contains(page.Examples[2].XamlCode, "UniformGridLayout");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "PinkColorCollection");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "ItemsView3.ItemsSource");

                    var selectorBar1 = (Mux.SelectorBar)FindByAutomationId(page, "GallerySample_SelectorBar_SelectorBar");
                    var selectorBar2 = FindNamedDescendant<Mux.SelectorBar>(page, "SelectorBar2");
                    var contentFrame = FindNamedDescendant<Frame>(page, "ContentFrame");
                    var selectorBar3 = FindNamedDescendant<Mux.SelectorBar>(page, "SelectorBar3");
                    var itemsView3 = FindNamedDescendant<ItemsControl>(page, "ItemsView3");
                    Assert.IsNotNull(selectorBar1);
                    Assert.IsNotNull(selectorBar2);
                    Assert.IsNotNull(contentFrame);
                    Assert.IsNotNull(selectorBar3);
                    Assert.IsNotNull(itemsView3);
                    Assert.AreEqual(new Thickness(0), itemsView3.Margin);

                    Assert.AreEqual("SelectorBar1", selectorBar1.Name);
                    Assert.AreEqual(new Thickness(0), selectorBar1.BorderThickness);
                    var selectorBarWidth = Math.Round(selectorBar1.ActualWidth);
                    Assert.IsTrue(
                        selectorBarWidth >= 283.0 && selectorBarWidth <= 284.0,
                        "Unexpected SelectorBar visual width: " + selectorBarWidth);
                    Assert.AreEqual(48.0, Math.Round(selectorBar1.ActualHeight));
                    AssertSelectorBarItem(selectorBar1.Items[0], "SelectorBarItemRecent", "Recent", Mux.Symbol.Clock, false);
                    AssertSelectorBarItem(selectorBar1.Items[1], "SelectorBarItemShared", "Shared", Mux.Symbol.Share, false);
                    AssertSelectorBarItem(selectorBar1.Items[2], "SelectorBarItemFavorites", "Favorites", Mux.Symbol.OutlineStar, false);
                    AssertSelectorBarItemUsesVisibleGalleryTemplate(selectorBar1.Items[1]);
                    RaiseSelectorBarItemClick(selectorBar1.Items[1]);
                    WpfTestHost.DoEvents();
                    Assert.AreSame(selectorBar1.Items[1], selectorBar1.SelectedItem);
                    Assert.IsTrue(selectorBar1.Items[1].IsSelected);

                    Assert.AreEqual("SelectorBar2", selectorBar2.Name);
                    Assert.AreEqual(5, selectorBar2.Items.Count);
                    Assert.AreSame(selectorBar2.Items[0], selectorBar2.SelectedItem);
                    Assert.AreEqual("SamplePage1", GetFramePageTitle(contentFrame));
                    AssertSelectorBarSamplePage1Layout(contentFrame);

                    selectorBar2.SelectedItem = selectorBar2.Items[2];
                    WpfTestHost.DoEvents();
                    Assert.AreSame(selectorBar2.Items[2], selectorBar2.SelectedItem);
                    Assert.AreEqual("SamplePage3", GetFramePageTitle(contentFrame));
                    AssertSelectorBarSamplePage3Layout(contentFrame);

                    Assert.AreEqual("SelectorBar3", selectorBar3.Name);
                    Assert.AreEqual(3, selectorBar3.Items.Count);
                    Assert.AreSame(selectorBar3.Items[0], selectorBar3.SelectedItem);
                    Assert.AreEqual(5, CountItems(itemsView3.ItemsSource));
                    var colorItems = FindDescendants<Border>(itemsView3)
                        .Where(border => border.Width == 112d && border.Height == 82d)
                        .ToList();
                    Assert.AreEqual(5, colorItems.Count);
                    Assert.IsTrue(colorItems.All(border => border.CornerRadius == new CornerRadius(4)));

                    selectorBar3.SelectedItem = selectorBar3.Items[1];
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(7, CountItems(itemsView3.ItemsSource));

                    selectorBar3.SelectedItem = selectorBar3.Items[2];
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(4, CountItems(itemsView3.ItemsSource));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void InfoBarSampleUsesVisibleOpenInfoBarTemplate()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("InfoBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("A closable InfoBar with options to change its Severity.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A closable InfoBar with a long or short message and various buttons", page.Examples[1].HeaderText);
                    Assert.AreEqual("A closable InfoBar with options to display the close button and icon", page.Examples[2].HeaderText);
                    StringAssert.Contains(page.Examples[0].XamlCode, "Severity=\"Informational\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "A long essential app message...");
                    StringAssert.Contains(page.Examples[2].XamlCode, "IsIconVisible=\"True\"");

                    var infoBar = (ModernWpf.Controls.InfoBar)FindByAutomationId(page, "GallerySample_InfoBar_InfoBar");
                    var longMessageInfoBar = (ModernWpf.Controls.InfoBar)FindByAutomationId(page, "GallerySample_InfoBar_LongMessageInfoBar");
                    var iconAndCloseInfoBar = (ModernWpf.Controls.InfoBar)FindByAutomationId(page, "GallerySample_InfoBar_IconAndCloseInfoBar");
                    Assert.IsNotNull(infoBar);
                    Assert.IsNotNull(longMessageInfoBar);
                    Assert.IsNotNull(iconAndCloseInfoBar);
                    Assert.IsTrue(infoBar.IsOpen);
                    Assert.IsTrue(longMessageInfoBar.IsOpen);
                    Assert.IsTrue(iconAndCloseInfoBar.IsOpen);
                    Assert.AreEqual(560.0, infoBar.Width);
                    Assert.AreEqual(ModernWpf.Controls.InfoBarSeverity.Informational, infoBar.Severity);
                    Assert.IsTrue(iconAndCloseInfoBar.IsIconVisible);
                    Assert.IsTrue(iconAndCloseInfoBar.IsClosable);

                    var severityComboBox = FindNamedDescendant<ComboBox>(page, "InfoBarSeverityComboBox");
                    var messageComboBox = FindNamedDescendant<ComboBox>(page, "InfoBarMessageComboBox");
                    var actionButtonComboBox = FindNamedDescendant<ComboBox>(page, "InfoBarActionButtonComboBox");
                    Assert.IsNotNull(severityComboBox);
                    Assert.IsNotNull(messageComboBox);
                    Assert.IsNotNull(actionButtonComboBox);
                    Assert.AreEqual("Informational", severityComboBox.SelectedItem);
                    Assert.AreEqual(1, messageComboBox.SelectedIndex);
                    Assert.AreEqual(0, actionButtonComboBox.SelectedIndex);
                    Assert.AreEqual(new Thickness(), severityComboBox.Margin);
                    Assert.AreEqual(new Thickness(), messageComboBox.Margin);
                    Assert.AreEqual(new Thickness(), actionButtonComboBox.Margin);
                    Assert.AreEqual(60d, ((StackPanel)severityComboBox.Parent).MinHeight);
                    Assert.AreEqual(60d, ((StackPanel)messageComboBox.Parent).MinHeight);
                    Assert.AreEqual(60d, ((StackPanel)actionButtonComboBox.Parent).MinHeight);

                    foreach (var checkBox in new[]
                    {
                        FindNamedDescendant<CheckBox>(page, "InfoBarIsOpenCheckBox1"),
                        FindNamedDescendant<CheckBox>(page, "InfoBarIsOpenCheckBox2"),
                        FindNamedDescendant<CheckBox>(page, "InfoBarIsOpenCheckBox3"),
                        FindNamedDescendant<CheckBox>(page, "InfoBarIsIconVisibleCheckBox"),
                        FindNamedDescendant<CheckBox>(page, "InfoBarIsClosableCheckBox")
                    })
                    {
                        Assert.IsNotNull(checkBox);
                        Assert.AreEqual(new Thickness(), checkBox.Margin);
                    }

                    severityComboBox.SelectedItem = "Error";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(ModernWpf.Controls.InfoBarSeverity.Error, infoBar.Severity);

                    messageComboBox.SelectedIndex = 0;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("A short essential app message.", longMessageInfoBar.Message);

                    actionButtonComboBox.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType(longMessageInfoBar.ActionButton, typeof(Button));
                    Assert.AreEqual("Action", longMessageInfoBar.ActionButton.Content);

                    actionButtonComboBox.SelectedIndex = 2;
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType(longMessageInfoBar.ActionButton, typeof(ModernWpf.Controls.HyperlinkButton));
                    Assert.AreEqual("Project information", longMessageInfoBar.ActionButton.Content);
                    Assert.AreEqual(new Uri(GalleryBranding.RepositoryUrl), ((ModernWpf.Controls.HyperlinkButton)longMessageInfoBar.ActionButton).NavigateUri);

                    actionButtonComboBox.SelectedIndex = 0;
                    WpfTestHost.DoEvents();
                    Assert.IsNull(longMessageInfoBar.ActionButton);

                    var isOpenCheckBox = FindNamedDescendant<CheckBox>(page, "InfoBarIsOpenCheckBox1");
                    var isIconVisibleCheckBox = FindNamedDescendant<CheckBox>(page, "InfoBarIsIconVisibleCheckBox");
                    var isClosableCheckBox = FindNamedDescendant<CheckBox>(page, "InfoBarIsClosableCheckBox");
                    isOpenCheckBox.IsChecked = false;
                    isIconVisibleCheckBox.IsChecked = false;
                    isClosableCheckBox.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(infoBar.IsOpen);
                    Assert.IsFalse(iconAndCloseInfoBar.IsIconVisible);
                    Assert.IsFalse(iconAndCloseInfoBar.IsClosable);

                    var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
                    Assert.IsNotNull(contentRoot);
                    Assert.AreEqual(Visibility.Collapsed, contentRoot.Visibility);
                    Assert.IsNotNull(FindNamedDescendant<TextBlock>(infoBar, "Title"));
                    Assert.IsNotNull(FindNamedDescendant<TextBlock>(infoBar, "Message"));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void InfoBadgeSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("InfoBadge"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(4, page.Examples.Count);
                    Assert.AreEqual("InfoBadge embedded in NavigationView ", page.Examples[0].HeaderText);
                    Assert.AreEqual("Different InfoBadge Styles", page.Examples[1].HeaderText);
                    Assert.AreEqual("Placing an InfoBadge Inside Another Control", page.Examples[2].HeaderText);
                    Assert.AreEqual("InfoBadge with Dynamic Value", page.Examples[3].HeaderText);
                    Assert.IsTrue(page.Examples.All(example =>
                        example.HorizontalContentAlignment == HorizontalAlignment.Stretch &&
                        example.VerticalContentAlignment == VerticalAlignment.Top));
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "Inbox, 5 notifications");
                    StringAssert.Contains(page.Examples[0].XamlCode, "infoBadge1");
                    StringAssert.Contains(page.Examples[1].XamlCode, "$(Style)IconInfoBadgeStyle");
                    StringAssert.Contains(page.Examples[2].XamlCode, "ToolTipService.ToolTip=\"Refresh required\"");
                    StringAssert.Contains(page.Examples[3].XamlCode, "ValueNumberBox");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "DynamicInfoBadge.Value = (int)args.NewValue;");

                    var navigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample1");
                    var inboxItem = FindNamedDescendant<Mux.NavigationViewItem>(page, "InboxPage");
                    var infoBadge1 = (Mux.InfoBadge)FindByAutomationId(page, "GallerySample_InfoBadge_InfoBadge");
                    var toggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "ToggleInfoBadgeOpacity");
                    var displayMode = FindNamedDescendant<ComboBox>(page, "NavigationViewDisplayMode");
                    var contentFrame = FindNamedDescendant<Frame>(page, "contentFrame");
                    Assert.IsNotNull(navigationView);
                    Assert.IsNotNull(inboxItem);
                    Assert.IsNotNull(infoBadge1);
                    Assert.IsNotNull(toggle);
                    Assert.IsNotNull(displayMode);
                    Assert.IsNotNull(contentFrame);
                    Assert.AreEqual(300.0, navigationView.Height);
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Left, navigationView.PaneDisplayMode);
                    Assert.IsTrue(navigationView.IsPaneOpen);
                    Assert.AreEqual(3, navigationView.MenuItems.Count);
                    Assert.AreEqual("Inbox", inboxItem.Content);
                    Assert.AreEqual("Inbox, 5 notifications", AutomationProperties.GetName(inboxItem));
                    Assert.AreSame(infoBadge1, inboxItem.InfoBadge);
                    Assert.IsNull(UIElementAutomationPeer.CreatePeerForElement(infoBadge1));
                    Assert.AreEqual("infoBadge1", infoBadge1.Name);
                    Assert.AreEqual(5, infoBadge1.Value);
                    Assert.AreEqual(1.0, infoBadge1.Opacity);
                    AssertNavigationViewInfoBadgeSampleRendered(navigationView, inboxItem, infoBadge1);
                    Assert.AreEqual("InfoBadge Opacity", toggle.Header);
                    Assert.IsTrue(toggle.IsOn);
                    Assert.AreEqual("LeftExpanded", displayMode.SelectedItem);
                    Assert.AreEqual("LeftExpanded", displayMode.Items[0]);
                    Assert.AreEqual("LeftCompact", displayMode.Items[1]);
                    Assert.AreEqual("Top", displayMode.Items[2]);

                    toggle.IsOn = false;
                    displayMode.SelectedItem = "LeftCompact";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(0.0, infoBadge1.Opacity);
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.LeftCompact, navigationView.PaneDisplayMode);
                    Assert.IsFalse(navigationView.IsPaneOpen);

                    displayMode.SelectedItem = "Top";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode);
                    Assert.IsTrue(navigationView.IsPaneOpen);

                    var infoBadge2 = FindNamedDescendant<Mux.InfoBadge>(page, "infoBadge2");
                    var infoBadge3 = FindNamedDescendant<Mux.InfoBadge>(page, "infoBadge3");
                    var infoBadge4 = FindNamedDescendant<Mux.InfoBadge>(page, "infoBadge4");
                    var styleCombo = FindNamedDescendant<ComboBox>(page, "InfoBadgeStyleComboBox");
                    Assert.IsNotNull(infoBadge2);
                    Assert.IsNotNull(infoBadge3);
                    Assert.IsNotNull(infoBadge4);
                    Assert.IsNotNull(styleCombo);
                    Assert.AreEqual(HorizontalAlignment.Right, infoBadge2.HorizontalAlignment);
                    Assert.AreEqual(HorizontalAlignment.Right, infoBadge3.HorizontalAlignment);
                    Assert.AreEqual(10, infoBadge3.Value);
                    Assert.AreEqual(VerticalAlignment.Center, infoBadge4.VerticalAlignment);
                    Assert.AreEqual("Attention", styleCombo.SelectedItem);
                    Assert.AreEqual("\uEA38", ((Mux.FontIconSource)infoBadge2.IconSource).Glyph);
                    Assert.AreEqual(new Thickness(0, 4, 0, 2), infoBadge2.Padding);

                    styleCombo.SelectedItem = "Critical";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.Symbol.Cancel, ((Mux.SymbolIconSource)infoBadge2.IconSource).Symbol);
                    Assert.AreEqual(new Thickness(0), infoBadge2.Padding);

                    var exampleButton = FindNamedDescendant<Button>(page, "Example3Button");
                    var exampleButtonBadge = FindNamedDescendant<Mux.InfoBadge>(page, "Example3InfoBadge");
                    Assert.IsNotNull(exampleButton);
                    Assert.IsNotNull(exampleButtonBadge);
                    Assert.AreEqual(200.0, exampleButton.Width);
                    Assert.AreEqual(60.0, exampleButton.Height);
                    Assert.AreEqual(new Thickness(0), exampleButton.Padding);
                    Assert.AreEqual(HorizontalAlignment.Center, exampleButton.HorizontalAlignment);
                    Assert.AreEqual(HorizontalAlignment.Stretch, exampleButton.HorizontalContentAlignment);
                    Assert.AreEqual(VerticalAlignment.Stretch, exampleButton.VerticalContentAlignment);
                    Assert.AreEqual("Example3Button", AutomationProperties.GetName(exampleButton));
                    Assert.AreEqual("Refresh required", exampleButton.ToolTip);
                    Assert.AreEqual(HorizontalAlignment.Right, exampleButtonBadge.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Top, exampleButtonBadge.VerticalAlignment);
                    Assert.AreEqual(Color.FromRgb(0xC4, 0x2B, 0x1C), ((SolidColorBrush)exampleButtonBadge.Background).Color);
                    Assert.AreEqual("\uF13C", ((Mux.FontIconSource)exampleButtonBadge.IconSource).Glyph);

                    var dynamicInfoBadge = FindNamedDescendant<Mux.InfoBadge>(page, "DynamicInfoBadge");
                    var valueNumberBox = FindNamedDescendant<Mux.NumberBox>(page, "ValueNumberBox");
                    Assert.IsNotNull(dynamicInfoBadge);
                    Assert.IsNotNull(valueNumberBox);
                    Assert.AreEqual(HorizontalAlignment.Center, dynamicInfoBadge.HorizontalAlignment);
                    Assert.AreEqual(1, dynamicInfoBadge.Value);
                    Assert.AreEqual("InfoBadge Value", valueNumberBox.Header);
                    Assert.AreEqual(-1.0, valueNumberBox.Minimum);
                    Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Inline, valueNumberBox.SpinButtonPlacementMode);
                    Assert.AreEqual(1.0, valueNumberBox.Value);

                    valueNumberBox.Value = 12;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(12, dynamicInfoBadge.Value);
                    valueNumberBox.Value = -1;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(-1, dynamicInfoBadge.Value);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ProgressRingSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ProgressRing"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("An indeterminate progress ring.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A determinate progress ring.", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsActive=\"$(IsActive)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "$(Background)");
                    StringAssert.Contains(page.Examples[1].XamlCode, "Value=\"$(DeterminateProgressValue)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "IsIndeterminate=\"False\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "$(Background)");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var progressRing1 = (Mux.ProgressRing)FindByAutomationId(page, "GallerySample_ProgressRing_ProgressRing");
                    var progressRing2 = FindNamedDescendant<Mux.ProgressRing>(page, "ProgressRing2");
                    var progressToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "ProgressToggle");
                    var backgroundComboBox1 = FindNamedDescendant<ComboBox>(page, "BackgroundComboBox1");
                    var backgroundComboBox2 = FindNamedDescendant<ComboBox>(page, "BackgroundComboBox2");
                    var progressValue = FindNamedDescendant<Mux.NumberBox>(page, "ProgressValue");
                    var control2 = FindNamedDescendant<StackPanel>(page, "Control2");
                    var progressRing1Host = FindNamedDescendant<Border>(page, "ProgressRing1BackgroundHost");
                    var progressRing2Host = FindNamedDescendant<Border>(page, "ProgressRing2BackgroundHost");
                    Assert.IsNotNull(progressRing1);
                    Assert.IsNotNull(progressRing2);
                    Assert.IsNotNull(progressToggle);
                    Assert.IsNotNull(backgroundComboBox1);
                    Assert.IsNotNull(backgroundComboBox2);
                    Assert.IsNotNull(progressValue);
                    Assert.IsNotNull(control2);
                    Assert.IsNotNull(progressRing1Host);
                    Assert.IsNotNull(progressRing2Host);

                    Assert.AreEqual("ProgressRing1", progressRing1.Name);
                    Assert.AreEqual(60.0, progressRing1.Width);
                    Assert.AreEqual(60.0, progressRing1.Height);
                    Assert.AreEqual(new Thickness(10, 10, 0, 0), progressRing1.Margin);
                    Assert.AreEqual(VerticalAlignment.Top, progressRing1.VerticalAlignment);
                    Assert.IsTrue(progressRing1.IsActive);
                    Assert.AreEqual("Progress image", AutomationProperties.GetName(progressRing1));
                    Assert.AreEqual("Progress Options", AutomationProperties.GetName(progressToggle));
                    Assert.AreEqual("Progress Options", progressToggle.Header);
                    Assert.IsTrue(progressToggle.IsOn);
                    Assert.AreEqual("Do work", progressToggle.OffContent);
                    Assert.AreEqual("Working", progressToggle.OnContent);
                    Assert.AreEqual(200.0, backgroundComboBox1.Width);
                    Assert.AreEqual(
                        "Pick a color",
                        ModernWpf.Controls.Primitives.ControlHelper.GetPlaceholderText(backgroundComboBox1));
                    Assert.AreEqual("Transparent", backgroundComboBox1.Items[0]);
                    Assert.AreEqual("LightGray", backgroundComboBox1.Items[1]);
                    Assert.AreEqual(Brushes.Transparent, progressRing1Host.Background);

                    Assert.AreEqual("Control2", control2.Name);
                    Assert.AreEqual(Orientation.Horizontal, control2.Orientation);
                    Assert.AreEqual("ProgressRing2", progressRing2.Name);
                    Assert.AreEqual(60.0, progressRing2.Width);
                    Assert.AreEqual(60.0, progressRing2.Height);
                    Assert.AreEqual(new Thickness(0, 0, 60, 0), progressRing2.Margin);
                    Assert.IsFalse(progressRing2.IsIndeterminate);
                    Assert.AreEqual(0.0, progressRing2.Value);
                    Assert.AreEqual("Progress image", AutomationProperties.GetName(progressRing2));
                    Assert.AreEqual("ProgressValue", progressValue.Name);
                    Assert.AreEqual(120.0, progressValue.MinWidth);
                    Assert.AreEqual(VerticalAlignment.Center, progressValue.VerticalAlignment);
                    Assert.AreEqual("Progress", progressValue.Header);
                    Assert.AreEqual("Progress amount", AutomationProperties.GetName(progressValue));
                    Assert.AreEqual(0.0, progressValue.Minimum);
                    Assert.AreEqual(100.0, progressValue.Maximum);
                    Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Inline, progressValue.SpinButtonPlacementMode);
                    Assert.AreEqual(0.0, progressValue.Value);
                    Assert.AreEqual(200.0, backgroundComboBox2.Width);
                    Assert.AreEqual(
                        "Pick a color",
                        ModernWpf.Controls.Primitives.ControlHelper.GetPlaceholderText(backgroundComboBox2));
                    Assert.AreEqual("Transparent", backgroundComboBox2.Items[0]);
                    Assert.AreEqual("LightGray", backgroundComboBox2.Items[1]);
                    Assert.AreEqual(Brushes.Transparent, progressRing2Host.Background);

                    progressToggle.IsOn = false;
                    backgroundComboBox1.SelectedItem = "LightGray";
                    backgroundComboBox2.SelectedItem = "LightGray";
                    progressValue.Value = 42;
                    WpfTestHost.DoEvents();

                    Assert.IsFalse(progressRing1.IsActive);
                    Assert.AreEqual(Brushes.LightGray, progressRing1Host.Background);
                    Assert.AreEqual(Brushes.LightGray, progressRing2Host.Background);
                    Assert.AreEqual(42.0, progressRing2.Value);

                    progressValue.Value = double.NaN;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(0.0, progressValue.Value);
                    Assert.AreEqual(0.0, progressRing2.Value);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void WinUIProgressBarSampleMatchesCurrentWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var stockItem = GalleryCatalog.FindItem("ProgressBar");
                var item = GalleryCatalog.FindItem("WinUIProgressBar");
                Assert.IsNotNull(stockItem);
                Assert.IsNotNull(item);
                Assert.AreEqual("System.Windows.Controls.ProgressBar", stockItem.ApiNamespace);
                Assert.AreEqual("ProgressBar (ModernWPF)", item.Title);
                Assert.AreEqual("ModernWpf.Controls", item.ApiNamespace);

                var page = new ItemPage(item);
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("An indeterminate progress bar.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A determinate progress bar.", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.AreEqual(
                        "<ProgressBar Width=\"130\" IsIndeterminate=\"True\" ShowPaused=\"$(ShowPaused)\" ShowError=\"$(ShowError)\" />",
                        page.Examples[0].XamlCode);
                    Assert.AreEqual(
                        "<ProgressBar Width=\"130\" Value=\"$(DeterminateProgressValue)\" />",
                        page.Examples[1].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var sampleRoot = (GallerySamplePanel)FindByAutomationId(page, "GallerySample_WinUIProgressBar_Root");
                    var indeterminate = (Mux.ProgressBar)FindByAutomationId(page, "GallerySample_WinUIProgressBar_IndeterminateProgressBar");
                    var determinate = (Mux.ProgressBar)FindByAutomationId(page, "GallerySample_WinUIProgressBar_DeterminateProgressBar");
                    var radioButtons = FindNamedDescendant<Mux.RadioButtons>(page, "ProgressStateRadioButtons");
                    var progressValue = FindNamedDescendant<Mux.NumberBox>(page, "ProgressValue");
                    var label = FindNamedDescendant<TextBlock>(page, "ProgressLabel");
                    var output = FindNamedDescendant<TextBlock>(page, "Control2Output");
                    var control2 = FindNamedDescendant<StackPanel>(page, "Control2");

                    Assert.IsNotNull(sampleRoot);
                    Assert.IsNotNull(indeterminate);
                    Assert.IsNotNull(determinate);
                    Assert.IsNotNull(radioButtons);
                    Assert.IsNotNull(progressValue);
                    Assert.IsNotNull(label);
                    Assert.IsNotNull(output);
                    Assert.IsNotNull(control2);

                    Assert.AreEqual(130.0, indeterminate.Width);
                    Assert.AreEqual(new Thickness(10, 10, 0, 0), indeterminate.Margin);
                    Assert.AreEqual(VerticalAlignment.Top, indeterminate.VerticalAlignment);
                    Assert.IsTrue(indeterminate.IsIndeterminate);
                    Assert.AreEqual("Progress state", radioButtons.Header);
                    Assert.AreEqual(0, radioButtons.SelectedIndex);
                    Assert.AreEqual("Running", ((RadioButton)radioButtons.Items[0]).Content);
                    Assert.AreEqual("Paused", ((RadioButton)radioButtons.Items[1]).Content);
                    Assert.AreEqual("Error", ((RadioButton)radioButtons.Items[2]).Content);

                    Assert.AreEqual("Control2", control2.Name);
                    Assert.AreEqual(Orientation.Horizontal, control2.Orientation);
                    Assert.AreEqual(130.0, determinate.Width);
                    Assert.AreEqual("Determinate ProgressBar example", AutomationProperties.GetName(determinate));
                    Assert.AreEqual(60.0, output.Width);
                    Assert.AreEqual("Progress", label.Text);
                    Assert.AreSame(label, AutomationProperties.GetLabeledBy(progressValue));
                    Assert.AreEqual("NumberBox controlling ProgressBar2 value", AutomationProperties.GetName(progressValue));
                    Assert.AreEqual(0.0, progressValue.Minimum);
                    Assert.AreEqual(100.0, progressValue.Maximum);
                    Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Inline, progressValue.SpinButtonPlacementMode);

                    radioButtons.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(indeterminate.ShowPaused);
                    Assert.IsFalse(indeterminate.ShowError);

                    radioButtons.SelectedIndex = 2;
                    progressValue.Value = 65;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(indeterminate.ShowPaused);
                    Assert.IsTrue(indeterminate.ShowError);
                    Assert.AreEqual(65.0, determinate.Value);

                    progressValue.Value = double.NaN;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(0.0, progressValue.Value);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void AnnotatedScrollBarSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("AnnotatedScrollBar"));
                var window = new Window
                {
                    Width = 1180,
                    Height = 820,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("AnnotatedScrollBar linked to a ScrollView.", page.Examples[0].HeaderText);
                    Assert.AreEqual(448d, page.Examples[0].OptionsMaxWidth);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "ScrollView x:Name=\"scrollView\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "AnnotatedScrollBar x:Name=\"annotatedScrollBar\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "scrollView.ScrollPresenter.VerticalScrollController = annotatedScrollBar.ScrollController;");

                    var sampleRoot = FindByAutomationId(page, "GallerySample_AnnotatedScrollBar_Root") as UIElement;
                    Assert.IsNotNull(sampleRoot);
                    var sampleRootPeer = UIElementAutomationPeer.CreatePeerForElement(sampleRoot);
                    Assert.IsNotNull(sampleRootPeer);
                    Assert.IsTrue(sampleRootPeer.IsControlElement());
                    Assert.AreEqual(AutomationControlType.Group, sampleRootPeer.GetAutomationControlType());

                    var scrollViewer = (ScrollViewer)FindByAutomationId(page, "GallerySample_AnnotatedScrollBar_ScrollViewer");
                    var annotatedScrollBar = (Mux.AnnotatedScrollBar)FindByAutomationId(page, "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar");
                    var itemsRepeater = FindNamedDescendant<WrapPanel>(page, "itemsRepeater");
                    var heightSlider = FindNamedDescendant<Slider>(page, "AnnotatedScrollBarMaxHeightSlider");

                    Assert.IsNotNull(scrollViewer);
                    Assert.IsNotNull(annotatedScrollBar);
                    Assert.IsNotNull(itemsRepeater);
                    Assert.IsNotNull(heightSlider);

                    Assert.AreEqual("scrollViewer", scrollViewer.Name);
                    Assert.AreEqual(124.0, scrollViewer.Width);
                    Assert.AreEqual(new Thickness(12, 0, 0, 0), scrollViewer.Margin);
                    Assert.AreEqual(800.0, scrollViewer.MaxWidth);
                    Assert.AreEqual(500.0, scrollViewer.MaxHeight);
                    Assert.AreEqual(Brushes.LightGray, scrollViewer.Background);
                    Assert.AreEqual(ScrollBarVisibility.Hidden, scrollViewer.VerticalScrollBarVisibility);
                    Assert.AreEqual(ScrollBarVisibility.Disabled, scrollViewer.HorizontalScrollBarVisibility);
                    Assert.AreSame(itemsRepeater, scrollViewer.Content);
                    Assert.AreEqual(Brushes.LightGray, itemsRepeater.Background);

                    Assert.AreEqual("annotatedScrollBar", annotatedScrollBar.Name);
                    Assert.IsNull(UIElementAutomationPeer.CreatePeerForElement(annotatedScrollBar));
                    Assert.AreEqual(500.0, annotatedScrollBar.MaxHeight);
                    Assert.AreEqual(new Thickness(4, 0, 48, 0), annotatedScrollBar.Margin);
                    Assert.AreEqual(HorizontalAlignment.Right, annotatedScrollBar.HorizontalAlignment);
                    Assert.IsTrue(annotatedScrollBar.ScrollController.CanScroll);

                    var incrementButton = FindNamedDescendant<RepeatButton>(annotatedScrollBar, "PART_VerticalIncrementRepeatButton");
                    Assert.IsNotNull(incrementButton);
                    var incrementPresenter = FindDescendants<Mux.ContentPresenterEx>(incrementButton).Single();
                    Assert.AreSame(incrementButton.Background, incrementPresenter.Background);
                    Assert.AreSame(annotatedScrollBar.TryFindResource("SubtleFillColorTransparentBrush"), incrementPresenter.Background);
                    Assert.AreEqual(Colors.Transparent, ((SolidColorBrush)incrementPresenter.Background).Color);
                    Assert.AreEqual(annotatedScrollBar.TryFindResource("ButtonPadding"), incrementButton.Padding);

                    Assert.AreEqual(new Thickness(2), itemsRepeater.Margin);
                    Assert.AreEqual(250, itemsRepeater.Children.Count);
                    AssertAnnotatedColorItem(itemsRepeater, 0, Colors.Azure);
                    AssertAnnotatedColorItem(itemsRepeater, 31, Colors.Azure);
                    AssertAnnotatedColorItem(itemsRepeater, 32, Colors.Crimson);
                    AssertAnnotatedColorItem(itemsRepeater, 81, Colors.Crimson);
                    AssertAnnotatedColorItem(itemsRepeater, 82, Colors.Cyan);
                    AssertAnnotatedColorItem(itemsRepeater, 89, Colors.Cyan);
                    AssertAnnotatedColorItem(itemsRepeater, 90, Colors.Fuchsia);
                    AssertAnnotatedColorItem(itemsRepeater, 159, Colors.Fuchsia);
                    AssertAnnotatedColorItem(itemsRepeater, 160, Colors.Gold);
                    AssertAnnotatedColorItem(itemsRepeater, 249, Colors.Gold);

                    Assert.AreEqual(5, annotatedScrollBar.Labels.Count);
                    var itemsPerRow = Math.Max((int)(itemsRepeater.ActualWidth / 120), 1);
                    AssertAnnotatedLabel(annotatedScrollBar.Labels[0], "Azure", 0, itemsPerRow);
                    AssertAnnotatedLabel(annotatedScrollBar.Labels[1], "Crimson", 32, itemsPerRow);
                    AssertAnnotatedLabel(annotatedScrollBar.Labels[2], "Cyan", 82, itemsPerRow);
                    AssertAnnotatedLabel(annotatedScrollBar.Labels[3], "Fuchsia", 90, itemsPerRow);
                    AssertAnnotatedLabel(annotatedScrollBar.Labels[4], "Gold", 160, itemsPerRow);

                    var labelDiagnostics = FindDescendants<TextBlock>(annotatedScrollBar)
                        .Where(textBlock => !string.IsNullOrEmpty(textBlock.Text))
                        .Select(textBlock => string.Format(
                            "{0}:{1}:IsVisible={2}:Actual={3}x{4}",
                            textBlock.Text,
                            textBlock.Visibility,
                            textBlock.IsVisible,
                            textBlock.ActualWidth,
                            textBlock.ActualHeight))
                        .ToList();
                    labelDiagnostics.AddRange(FindDescendants<ContentPresenter>(annotatedScrollBar)
                        .Where(presenter => presenter.Content is Mux.AnnotatedScrollBarLabel)
                        .Select(presenter =>
                        {
                            var label = (Mux.AnnotatedScrollBarLabel)presenter.Content;
                            return string.Format(
                                "Container({0}):{1}:Top={2}:Actual={3}x{4}",
                                label.Content,
                                presenter.Visibility,
                                presenter.Margin.Top,
                                presenter.ActualWidth,
                                presenter.ActualHeight);
                        }));
                    var allLabelTexts = FindDescendants<TextBlock>(annotatedScrollBar)
                        .Select(textBlock => textBlock.Text)
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList();
                    CollectionAssert.Contains(allLabelTexts, "Azure");
                    CollectionAssert.Contains(allLabelTexts, "Crimson");
                    CollectionAssert.Contains(allLabelTexts, "Cyan");
                    CollectionAssert.Contains(allLabelTexts, "Fuchsia");
                    CollectionAssert.Contains(allLabelTexts, "Gold");

                    var renderedLabelTexts = FindDescendants<TextBlock>(annotatedScrollBar)
                        .Where(textBlock => textBlock.IsVisible && textBlock.Visibility == Visibility.Visible)
                        .Select(textBlock => textBlock.Text)
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList();
                    Assert.IsTrue(renderedLabelTexts.Contains("Azure"), string.Join("; ", labelDiagnostics));
                    Assert.IsTrue(renderedLabelTexts.Contains("Crimson"), string.Join("; ", labelDiagnostics));
                    Assert.IsTrue(renderedLabelTexts.Contains("Cyan"), string.Join("; ", labelDiagnostics));
                    Assert.IsTrue(renderedLabelTexts.Contains("Fuchsia"), string.Join("; ", labelDiagnostics));
                    Assert.IsTrue(renderedLabelTexts.Contains("Gold"), string.Join("; ", labelDiagnostics));

                    var thumb = FindNamedDescendant<Border>(annotatedScrollBar, "PART_VerticalThumb");
                    Assert.IsNotNull(thumb);
                    Assert.IsTrue(thumb.ActualWidth > 0);
                    Assert.IsTrue(thumb.ActualHeight > 0);
                    Assert.AreSame(annotatedScrollBar.TryFindResource("AccentFillColorDefaultBrush"), thumb.Background);

                    Assert.AreEqual("AnnotatedScrollBar maximum height:", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(heightSlider));
                    Assert.IsTrue(
                        FindDescendants<TextBlock>(page).Any(textBlock =>
                            textBlock.IsVisible &&
                            textBlock.Text == "AnnotatedScrollBar maximum height:"),
                        "Expected the AnnotatedScrollBar maximum height header to be visible.");
                    Assert.AreEqual(100.0, heightSlider.Minimum);
                    Assert.AreEqual(500.0, heightSlider.Maximum);
                    Assert.AreEqual(500.0, heightSlider.Value);
                    Assert.AreEqual(new Thickness(0), heightSlider.Margin);
                    var heightSliderHost = heightSlider.Parent as StackPanel;
                    Assert.IsNotNull(heightSliderHost);
                    Assert.AreEqual(new Thickness(0, 10, 0, 0), heightSliderHost.Margin);
                    var optionsGrid = heightSliderHost.Parent as Grid;
                    Assert.IsNotNull(optionsGrid);
                    var optionsExplanation = optionsGrid.Children
                        .OfType<TextBlock>()
                        .Single(textBlock => textBlock.Text.StartsWith("Changing the AnnotatedScrollBar height", StringComparison.Ordinal));
                    Assert.AreEqual(TextWrapping.NoWrap, optionsExplanation.TextWrapping);
                    Assert.AreSame(page.Examples[0].OptionsContent, optionsGrid);
                    var annotatedCard = FindDescendants<ControlExample>(page).Single();
                    var optionsColumn = (ColumnDefinition)annotatedCard.Template.FindName("OptionsColumn", annotatedCard);
                    Assert.AreEqual(448d, optionsColumn.MaxWidth);
                    Assert.IsTrue(
                        optionsExplanation.ActualWidth >= 395d,
                        "The full WinUI explanation line must fit without clipping; actual " + optionsExplanation.ActualWidth);
                    Assert.IsTrue(heightSlider.IsSelectionRangeEnabled);
                    Assert.AreEqual(100.0, heightSlider.SelectionStart);
                    Assert.AreEqual(500.0, heightSlider.SelectionEnd);
                    var valueFill = FindNamedDescendant<Border>(heightSlider, "PART_SelectionRange");
                    Assert.IsNotNull(valueFill);
                    Assert.IsTrue(valueFill.ActualWidth >= 390d, "The maximum-value slider fill must reach its visible thumb.");

                    heightSlider.Value = 250;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(250.0, heightSlider.SelectionEnd);
                    Assert.AreEqual(250.0, annotatedScrollBar.MaxHeight);
                    Assert.AreEqual(5, annotatedScrollBar.Labels.Count);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void GridViewSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("GridView"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("Basic GridView with Simple DataTemplate", page.Examples[0].HeaderText);
                    Assert.AreEqual("GridView with Layout Customization", page.Examples[1].HeaderText);
                    Assert.AreEqual("Content inside of a GridView.", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"BasicGridView\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "ItemClick=\"BasicGridView_ItemClick\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "CustomDataObject class definition");
                    StringAssert.Contains(page.Examples[1].XamlCode, "x:Name=\"StyledGrid\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "MaxItemsWrapGrid");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ImageOverlayTemplate");
                    StringAssert.Contains(page.Examples[2].XamlCode, "x:Name=\"ContentGridView\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "CanDragItems=\"$(CanDragItems)\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "ContentGridView_SelectionChanged");

                    var basicGridView = (Mux.GridView)FindByAutomationId(page, "GallerySample_GridView_BasicGridView");
                    var automatedClickOutput0 = (TextBlock)FindByAutomationId(page, "GallerySample_GridView_ClickOutput0");
                    var namedBasicGridView = FindNamedDescendant<Mux.GridView>(page, "BasicGridView");
                    var clickOutput0 = FindNamedDescendant<TextBlock>(page, "ClickOutput0");
                    Assert.IsNotNull(basicGridView);
                    Assert.IsNotNull(automatedClickOutput0);
                    Assert.AreSame(basicGridView, namedBasicGridView);
                    Assert.IsNotNull(clickOutput0);
                    Assert.AreSame(clickOutput0, automatedClickOutput0);
                    Assert.AreEqual(8, basicGridView.Items.Count);
                    Assert.IsTrue(basicGridView.IsItemClickEnabled);
                    Assert.AreEqual(SelectionMode.Single, basicGridView.SelectionMode);
                    Assert.IsNotNull(basicGridView.ItemTemplate);

                    WaitFor(() => basicGridView.ItemContainerGenerator.ContainerFromIndex(0) != null);
                    var basicImage = FindDescendants<Image>(basicGridView).FirstOrDefault();
                    Assert.IsNotNull(basicImage);
                    Assert.AreEqual(BitmapScalingMode.HighQuality, RenderOptions.GetBitmapScalingMode(basicImage));
                    Assert.AreEqual("Item 1", AutomationProperties.GetName(basicImage));

                    var basicGridPeer = FrameworkElementAutomationPeer.CreatePeerForElement(basicGridView);
                    var basicGridItemPeer = basicGridPeer.GetChildren().First();
                    Assert.AreEqual("GridView", basicGridPeer.GetClassName());
                    Assert.AreEqual(AutomationControlType.List, basicGridPeer.GetAutomationControlType());
                    Assert.AreEqual("GridViewItem", basicGridItemPeer.GetClassName());
                    Assert.AreEqual(AutomationControlType.ListItem, basicGridItemPeer.GetAutomationControlType());
                    Assert.IsNotNull(basicGridItemPeer.GetPattern(PatternInterface.Invoke));

                    InvokeListViewBaseItemClick(
                        basicGridView,
                        (Mux.ListViewBaseItem)basicGridView.ItemContainerGenerator.ContainerFromIndex(0));
                    Assert.AreEqual("You clicked Item 1.", clickOutput0.Text);

                    var styledGrid = FindNamedDescendant<Mux.GridView>(page, "StyledGrid");
                    var maxItemsWrapGrid = FindNamedDescendant<Mux.ItemsWrapGrid>(page, "MaxItemsWrapGrid");
                    var columnSpace = FindNamedDescendant<Mux.NumberBox>(page, "ColumnSpace");
                    var rowSpace = FindNamedDescendant<Mux.NumberBox>(page, "RowSpace");
                    var wrapItemCount = FindNamedDescendant<Mux.NumberBox>(page, "WrapItemCount");
                    Assert.IsNotNull(styledGrid);
                    Assert.IsNotNull(maxItemsWrapGrid);
                    Assert.IsNotNull(columnSpace);
                    Assert.IsNotNull(rowSpace);
                    Assert.IsNotNull(wrapItemCount);
                    Assert.AreEqual(8, styledGrid.Items.Count);
                    Assert.AreEqual(3, maxItemsWrapGrid.MaximumRowsOrColumns);
                    Assert.AreEqual("Space between columns", AutomationProperties.GetName(columnSpace));
                    Assert.AreEqual("Space between rows", AutomationProperties.GetName(rowSpace));
                    Assert.AreEqual("Maximum number of items before wrapping", AutomationProperties.GetName(wrapItemCount));

                    WaitFor(() => styledGrid.ItemContainerGenerator.ContainerFromIndex(0) != null);
                    columnSpace.Value = 9;
                    rowSpace.Value = 7;
                    wrapItemCount.Value = 4;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(4, maxItemsWrapGrid.MaximumRowsOrColumns);
                    Assert.AreEqual(
                        new Thickness(9, 7, 9, 7),
                        ((Mux.GridViewItem)styledGrid.ItemContainerGenerator.ContainerFromIndex(0)).Margin);

                    var contentGridView = FindNamedDescendant<Mux.GridView>(page, "ContentGridView");
                    var clickOutput = FindNamedDescendant<TextBlock>(page, "ClickOutput");
                    var selectionOutput = FindNamedDescendant<TextBlock>(page, "SelectionOutput");
                    var control2 = FindNamedDescendant<StackPanel>(page, "Control2");
                    var itemClickCheckBox = FindNamedDescendant<CheckBox>(page, "ItemClickCheckBox");
                    var dropCheckBox = FindNamedDescendant<CheckBox>(page, "DropCheckBox");
                    var selectionModeComboBox = FindNamedDescendant<ComboBox>(page, "SelectionModeComboBox");
                    ToggleButton reverseFlowButton = null;
                    foreach (var button in FindDescendants<ToggleButton>(page))
                    {
                        if (string.Equals(button.Content as string, "Reverse FlowDirection", StringComparison.Ordinal))
                        {
                            reverseFlowButton = button;
                            break;
                        }
                    }
                    Assert.IsNotNull(contentGridView);
                    Assert.IsNotNull(clickOutput);
                    Assert.IsNotNull(selectionOutput);
                    Assert.IsNotNull(control2);
                    Assert.IsNotNull(itemClickCheckBox);
                    Assert.IsNotNull(dropCheckBox);
                    Assert.IsNotNull(selectionModeComboBox);
                    Assert.IsNotNull(reverseFlowButton);
                    Assert.IsTrue(double.IsNaN(control2.Width));
                    Assert.AreEqual(new Thickness(), selectionModeComboBox.Margin);
                    Assert.AreEqual(HorizontalAlignment.Left, selectionModeComboBox.HorizontalAlignment);
                    var selectionModeBlock = (StackPanel)selectionModeComboBox.Parent;
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), selectionModeBlock.Margin);
                    Assert.AreEqual(60d, selectionModeBlock.MinHeight);
                    Assert.AreEqual(HorizontalAlignment.Left, selectionModeBlock.HorizontalAlignment);
                    var propertyDescriptionBlocks = control2.Children.OfType<TextBlock>()
                        .Where(textBlock => textBlock.MaxWidth == 150)
                        .ToList();
                    Assert.AreEqual(2, propertyDescriptionBlocks.Count);
                    Assert.IsTrue(propertyDescriptionBlocks.All(textBlock => textBlock.Margin == new Thickness()));
                    Assert.IsTrue(propertyDescriptionBlocks.All(textBlock =>
                        textBlock.Inlines.OfType<LineBreak>().Count() == 1));
                    Assert.AreEqual(8, contentGridView.Items.Count);
                    Assert.AreEqual(double.PositiveInfinity, contentGridView.MaxHeight);
                    Assert.IsFalse(contentGridView.IsItemClickEnabled);
                    Assert.AreEqual(FlowDirection.LeftToRight, contentGridView.FlowDirection);
                    Assert.AreEqual(SelectionMode.Single, contentGridView.SelectionMode);
                    Assert.IsTrue(contentGridView.IsSelectionEnabled);

                    WaitFor(() => contentGridView.ItemContainerGenerator.ContainerFromIndex(0) != null);
                    var contentGridPeer = FrameworkElementAutomationPeer.CreatePeerForElement(contentGridView);
                    var contentGridItemPeer = contentGridPeer.GetChildren().First();
                    Assert.IsNull(contentGridItemPeer.GetPattern(PatternInterface.Invoke));

                    itemClickCheckBox.IsChecked = true;
                    itemClickCheckBox.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, itemClickCheckBox));
                    Assert.IsNotNull(contentGridItemPeer.GetPattern(PatternInterface.Invoke));
                    InvokeListViewBaseItemClick(
                        contentGridView,
                        (Mux.ListViewBaseItem)contentGridView.ItemContainerGenerator.ContainerFromIndex(0));
                    Assert.AreEqual("You clicked Item 1.", clickOutput.Text);

                    contentGridView.SelectedIndex = 0;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("You have selected 1 item(s).", selectionOutput.Text);

                    reverseFlowButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, reverseFlowButton));
                    Assert.AreEqual(FlowDirection.RightToLeft, contentGridView.FlowDirection);

                    dropCheckBox.IsChecked = true;
                    dropCheckBox.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, dropCheckBox));
                    Assert.IsTrue(contentGridView.AllowDrop);

                    selectionModeComboBox.SelectedItem = "Multiple";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(SelectionMode.Multiple, contentGridView.SelectionMode);
                    Assert.IsTrue(contentGridView.IsSelectionEnabled);

                    selectionModeComboBox.SelectedItem = "None";
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(contentGridView.IsSelectionEnabled);
                    Assert.AreEqual(string.Empty, selectionOutput.Text);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ItemsRepeaterSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ItemsRepeater"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(6, page.Examples.Count);
                    Assert.AreEqual("Basic, non-interactive items laid out by ItemsRepeater", page.Examples[0].HeaderText);
                    Assert.AreEqual("Virtualizing, scrollable list of items laid out by ItemsRepeater", page.Examples[1].HeaderText);
                    Assert.AreEqual("ItemsRepeater with mixed-type collection", page.Examples[2].HeaderText);
                    Assert.AreEqual("Laying out nested ItemsRepeaters", page.Examples[3].HeaderText);
                    Assert.AreEqual("Animated Scrolling and Content Display", page.Examples[4].HeaderText);
                    Assert.AreEqual("Virtualized, Content-Heavy Layout with Filtering and Sorting", page.Examples[5].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "ItemsSource=\"{x:Bind BarItems}\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "public class Bar");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ItemsSource=\"{Binding NumberedItems}\"");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "public class MyDataTemplateSelector");
                    StringAssert.Contains(page.Examples[2].XamlCode, "x:Name=\"MixedTypeRepeater\"");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "StringOrIntTemplateSelector");
                    StringAssert.Contains(page.Examples[3].XamlCode, "x:Name=\"outerRepeater\"");
                    StringAssert.Contains(page.Examples[4].XamlCode, "x:Name=\"animatedScrollRepeater\"");
                    StringAssert.Contains(page.Examples[4].CSharpCode, "OnElementPrepared");
                    StringAssert.Contains(page.Examples[4].CSharpCode, "ModernWpf.Controls.ItemsRepeater");
                    StringAssert.Contains(page.Examples[5].XamlCode, "x:Name=\"VariedImageSizeRepeater\"");
                    StringAssert.Contains(page.Examples[5].CSharpCode, "public class Recipe");
                    Assert.IsFalse(page.Examples[5].CSharpCode.Contains("Microsoft.UI.Xaml", StringComparison.Ordinal));

                    var repeater = (Mux.ItemsRepeater)FindByAutomationId(page, "GallerySample_ItemsRepeater_ItemsRepeater");
                    var namedRepeater = FindNamedDescendant<Mux.ItemsRepeater>(page, "repeater");
                    var addButton = FindNamedDescendant<Button>(page, "AddBtn");
                    var deleteButton = FindNamedDescendant<Button>(page, "DeleteBtn");
                    var horizontalStack = FindNamedDescendant<RadioButton>(page, "HStackBtn");
                    var uniformGrid = FindNamedDescendant<RadioButton>(page, "HGridBtn");
                    var basicOptions = (StackPanel)page.Examples[0].OptionsContent;
                    var virtualizingOptions = (StackPanel)page.Examples[1].OptionsContent;
                    Assert.IsNotNull(repeater);
                    Assert.AreSame(repeater, namedRepeater);
                    Assert.IsNotNull(addButton);
                    Assert.IsNotNull(deleteButton);
                    Assert.IsNotNull(horizontalStack);
                    Assert.IsNotNull(uniformGrid);
                    Assert.IsTrue(double.IsNaN(basicOptions.Width));
                    Assert.IsTrue(double.IsNaN(virtualizingOptions.Width));
                    Assert.AreEqual(new Thickness(0, 0, 0, 12), addButton.Margin);
                    Assert.AreEqual(new Thickness(0, 0, 0, 12), deleteButton.Margin);
                    Assert.AreEqual(HorizontalAlignment.Stretch, page.Examples[1].HorizontalContentAlignment);
                    Assert.AreEqual(VerticalAlignment.Top, page.Examples[1].VerticalContentAlignment);
                    var layoutRadioButtons = basicOptions.Children.OfType<Mux.RadioButtons>().Single();
                    Assert.AreEqual("Layout", layoutRadioButtons.Header);
                    Assert.AreEqual(3, layoutRadioButtons.Items.Count);
                    var virtualizingLayoutRadioButtons = virtualizingOptions.Children.OfType<Mux.RadioButtons>().Single();
                    Assert.AreEqual(1, virtualizingLayoutRadioButtons.SelectedIndex);
                    Assert.AreEqual(2, virtualizingLayoutRadioButtons.Items.Count);
                    Assert.AreEqual(3, CountItems(repeater.ItemsSource));
                    Assert.IsInstanceOfType(repeater.Layout, typeof(Mux.StackLayout));
                    Assert.AreEqual(Orientation.Vertical, ((Mux.StackLayout)repeater.Layout).Orientation);
                    var firstBar = repeater.TryGetElement(0) as Border ??
                        FindDescendants<Border>(repeater.TryGetElement(0)).First(border => border.Background != null);
                    var expectedLowChrome = (SolidColorBrush)Application.Current.TryFindResource("SystemControlPageBackgroundChromeLowBrush");
                    Assert.IsNotNull(firstBar);
                    Assert.IsNotNull(expectedLowChrome);
                    Assert.AreEqual(expectedLowChrome.Color, ((SolidColorBrush)firstBar.Background).Color);

                    addButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, addButton));
                    Assert.AreEqual(4, CountItems(repeater.ItemsSource));
                    deleteButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, deleteButton));
                    Assert.AreEqual(3, CountItems(repeater.ItemsSource));

                    horizontalStack.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType(repeater.Layout, typeof(Mux.StackLayout));
                    Assert.AreEqual(Orientation.Horizontal, ((Mux.StackLayout)repeater.Layout).Orientation);
                    uniformGrid.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType(repeater.Layout, typeof(Mux.UniformGridLayout));

                    var repeater2 = FindNamedDescendant<Mux.ItemsRepeater>(page, "repeater2");
                    var mixedRepeater = FindNamedDescendant<Mux.ItemsRepeater>(page, "MixedTypeRepeater");
                    var outerRepeater = FindNamedDescendant<Mux.ItemsRepeater>(page, "outerRepeater");
                    var animatedRepeater = FindNamedDescendant<Mux.ItemsRepeater>(page, "animatedScrollRepeater");
                    var colorRectangle = FindNamedDescendant<System.Windows.Shapes.Rectangle>(page, "colorRectangle");
                    var recipeRepeater = FindNamedDescendant<Mux.ItemsRepeater>(page, "VariedImageSizeRepeater");
                    var filterRecipes = FindNamedDescendant<TextBox>(page, "FilterRecipes");
                    Assert.IsNotNull(repeater2);
                    Assert.IsNotNull(mixedRepeater);
                    Assert.IsNotNull(outerRepeater);
                    Assert.IsNotNull(animatedRepeater);
                    Assert.IsNotNull(colorRectangle);
                    Assert.IsNotNull(recipeRepeater);
                    Assert.IsNotNull(filterRecipes);
                    Assert.AreEqual(500, CountItems(repeater2.ItemsSource));
                    Assert.AreEqual(9, CountItems(mixedRepeater.ItemsSource));
                    Assert.AreEqual(4, CountItems(outerRepeater.ItemsSource));
                    Assert.AreEqual(0d, ((Mux.StackLayout)outerRepeater.Layout).Spacing);
                    Assert.AreEqual(20, CountItems(animatedRepeater.ItemsSource));
                    Assert.AreEqual(120, CountItems(recipeRepeater.ItemsSource));
                    Assert.AreEqual("ActivityFeedLayout", repeater2.Layout.GetType().Name);
                    Assert.IsInstanceOfType(mixedRepeater.Layout, typeof(Mux.UniformGridLayout));
                    Assert.AreEqual("VariedImageSizeLayout", recipeRepeater.Layout.GetType().Name);
                    Assert.AreEqual(
                        200d,
                        (double)recipeRepeater.Layout.GetType().GetProperty("Width").GetValue(recipeRepeater.Layout));

                    var virtualizingScrollViewer = (ScrollViewer)FindByAutomationId(
                        page,
                        "GallerySample_ItemsRepeater_VirtualizingScrollViewer");
                    Assert.IsNotNull(virtualizingScrollViewer);
                    virtualizingScrollViewer.BringIntoView();
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();
                    WaitFor(() => repeater2.TryGetElement(5) != null);

                    var activityItem0 = (FrameworkElement)repeater2.TryGetElement(0);
                    var activityItem1 = (FrameworkElement)repeater2.TryGetElement(1);
                    var activityItem2 = (FrameworkElement)repeater2.TryGetElement(2);
                    var activityItem3 = (FrameworkElement)repeater2.TryGetElement(3);
                    var activityItem4 = (FrameworkElement)repeater2.TryGetElement(4);
                    var activityItem5 = (FrameworkElement)repeater2.TryGetElement(5);
                    var activityBounds0 = GetRelativeBounds(activityItem0, repeater2);
                    var activityBounds1 = GetRelativeBounds(activityItem1, repeater2);
                    var activityBounds2 = GetRelativeBounds(activityItem2, repeater2);
                    var activityBounds3 = GetRelativeBounds(activityItem3, repeater2);
                    var activityBounds4 = GetRelativeBounds(activityItem4, repeater2);
                    var activityBounds5 = GetRelativeBounds(activityItem5, repeater2);

                    Assert.AreEqual(108d, activityBounds0.Height, 0.01);
                    Assert.AreEqual(activityBounds0.Width, activityBounds1.Width, 0.01);
                    Assert.AreEqual((activityBounds0.Width * 2) + 12d, activityBounds2.Width, 1.01);
                    Assert.AreEqual(activityBounds0.Right + 12d, activityBounds1.Left, 1.01);
                    Assert.AreEqual(activityBounds1.Right + 12d, activityBounds2.Left, 1.01);
                    Assert.AreEqual(activityBounds2.Bottom + 12d, activityBounds3.Top, 0.01);
                    Assert.AreEqual((activityBounds4.Width * 2) + 12d, activityBounds3.Width, 1.01);
                    Assert.AreEqual(activityBounds4.Width, activityBounds5.Width, 0.01);

                    var animatedScrollViewer = FindNamedDescendant<ScrollViewer>(page, "Animated_ScrollViewer");
                    Assert.IsNotNull(animatedScrollViewer);
                    animatedScrollViewer.BringIntoView();
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();
                    WaitFor(() => animatedRepeater.TryGetElement(4) != null);

                    var realizedAnimatedElements = Enumerable.Range(0, 20)
                        .Select(animatedRepeater.TryGetElement)
                        .OfType<FrameworkElement>()
                        .ToList();
                    Assert.IsTrue(
                        realizedAnimatedElements.Count >= 5,
                        $"The 175 px animated viewport realized only {realizedAnimatedElements.Count} color buttons.");
                    Assert.IsTrue(realizedAnimatedElements.All(element => element.Margin == new Thickness()));
                    var animatedScales = realizedAnimatedElements
                        .Select(element => element.RenderTransform as ScaleTransform)
                        .Where(transform => transform != null)
                        .Select(transform => transform.ScaleX)
                        .ToList();
                    Assert.AreEqual(realizedAnimatedElements.Count, animatedScales.Count);
                    Assert.IsTrue(
                        animatedScales.Max() - animatedScales.Min() > 0.1,
                        "Animated repeater items should scale by their distance from the viewport center.");

                    var recipeTracker = FindNamedDescendant<Mux.ItemsRepeaterScrollHost>(page, "tracker");
                    Assert.IsNotNull(recipeTracker);
                    recipeTracker.BringIntoView();
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();
                    WaitFor(() => recipeRepeater.TryGetElement(2) != null);
                    var realizedRecipes = Enumerable.Range(0, 6)
                        .Select(recipeRepeater.TryGetElement)
                        .OfType<FrameworkElement>()
                        .ToList();
                    Assert.IsTrue(realizedRecipes.Count >= 3);
                    Assert.IsTrue(
                        realizedRecipes.All(recipe => Math.Abs(LayoutInformation.GetLayoutSlot(recipe).Width - 200d) < 0.01),
                        "The custom varied-size layout should assign each recipe a 200 px layout slot.");
                    Assert.IsTrue(
                        realizedRecipes.Select(recipe => Math.Round(recipe.ActualHeight, 2)).Distinct().Count() > 1,
                        "The content-heavy sample should preserve varied recipe heights.");

                    var firstCategory = outerRepeater.TryGetElement(0);
                    Assert.IsNotNull(firstCategory);
                    var innerRepeater = FindNamedDescendant<Mux.ItemsRepeater>(firstCategory, "innerRepeater");
                    Assert.IsNotNull(innerRepeater);
                    Assert.AreEqual(0d, ((Mux.StackLayout)innerRepeater.Layout).Spacing);
                    WaitFor(() => innerRepeater.TryGetElement(0) != null);
                    var firstNestedItem = innerRepeater.TryGetElement(0);
                    var apricotsText = FindDescendants<TextBlock>(firstNestedItem)
                        .Single(textBlock => textBlock.Text == "Apricots");
                    var nestedItemGrid = firstNestedItem as Grid ?? FindDescendants<Grid>(firstNestedItem)
                        .First(grid => grid.Background != null);
                    Assert.IsTrue(
                        apricotsText.ActualWidth >= 60,
                        $"The nested ItemsRepeater clipped 'Apricots' to {apricotsText.ActualWidth:0.##} px.");
                    Assert.IsTrue(
                        nestedItemGrid.ActualWidth >= apricotsText.ActualWidth,
                        $"The nested item background is {nestedItemGrid.ActualWidth:0.##} px wide while its text is {apricotsText.ActualWidth:0.##} px wide " +
                        $"(item type {firstNestedItem.GetType().Name}, item width {firstNestedItem.RenderSize.Width:0.##}, text desired width {apricotsText.DesiredSize.Width:0.##}).");

                    filterRecipes.Text = "Garlic";
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(CountItems(recipeRepeater.ItemsSource) > 0);
                    Assert.IsTrue(CountItems(recipeRepeater.ItemsSource) < 120);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void MenuBarSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("MenuBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("A simple MenuBar", page.Examples[0].HeaderText);
                    Assert.AreEqual("MenuBar with keyboard accelerators", page.Examples[1].HeaderText);
                    Assert.AreEqual("MenuBar with submenus, separators, and radio items", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<MenuBar>");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Text=\"Open...\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "KeyboardAccelerator");
                    StringAssert.Contains(page.Examples[1].XamlCode, "Text=\"Open...\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "RadioMenuFlyoutItem");
                    StringAssert.Contains(page.Examples[2].XamlCode, "Text=\"Other Formats...\"");

                    var simpleMenu = (Mux.MenuBar)FindByAutomationId(page, "GallerySample_MenuBar_MenuBar");
                    var keyboardMenu = (Mux.MenuBar)FindByAutomationId(page, "GallerySample_MenuBar_KeyboardAcceleratorsMenuBar");
                    var submenuMenu = (Mux.MenuBar)FindByAutomationId(page, "GallerySample_MenuBar_SubmenusMenuBar");
                    Assert.IsNotNull(simpleMenu);
                    Assert.IsNotNull(keyboardMenu);
                    Assert.IsNotNull(submenuMenu);
                    Assert.AreEqual(HorizontalAlignment.Left, simpleMenu.HorizontalAlignment);
                    Assert.AreEqual(158.0, simpleMenu.MinWidth);

                    Assert.AreEqual(3, simpleMenu.Items.Count);
                    Assert.AreEqual("File", simpleMenu.Items[0].Title);
                    Assert.AreEqual("Edit", simpleMenu.Items[1].Title);
                    Assert.AreEqual("Help", simpleMenu.Items[2].Title);

                    var simpleFile = simpleMenu.Items[0];
                    Assert.AreEqual(4, simpleFile.Items.Count);
                    Assert.AreEqual("Open", ((MenuItem)simpleFile.Items[1]).Header);
                    var menuPeer = new ModernWpf.Automation.Peers.MenuBarAutomationPeer(simpleMenu);
                    Assert.AreEqual(AutomationControlType.MenuBar, menuPeer.GetAutomationControlType());
                    Assert.AreEqual("MenuBar", menuPeer.GetClassName());
                    var filePeer = new ModernWpf.Automation.Peers.MenuBarItemAutomationPeer(simpleFile);
                    Assert.AreEqual(AutomationControlType.MenuItem, filePeer.GetAutomationControlType());
                    Assert.AreEqual("File", filePeer.GetName());
                    Assert.IsInstanceOfType(filePeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
                    var fileExpandCollapse = filePeer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
                    Assert.IsNotNull(fileExpandCollapse);
                    Assert.AreEqual(ExpandCollapseState.Collapsed, fileExpandCollapse.ExpandCollapseState);
                    fileExpandCollapse.Expand();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(ExpandCollapseState.Expanded, fileExpandCollapse.ExpandCollapseState);

                    var newItemPeer = new MenuItemAutomationPeer((MenuItem)simpleFile.Items[0]);
                    Assert.AreEqual(AutomationControlType.MenuItem, newItemPeer.GetAutomationControlType());
                    Assert.AreEqual("New", newItemPeer.GetName());
                    Assert.IsInstanceOfType(newItemPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
                    fileExpandCollapse.Collapse();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(ExpandCollapseState.Collapsed, fileExpandCollapse.ExpandCollapseState);
                    var selectedOptionText = FindNamedDescendant<TextBlock>(page, "SelectedOptionText");
                    Assert.IsNotNull(selectedOptionText);
                    ((MenuItem)simpleFile.Items[1]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    Assert.AreEqual("You clicked: Open", selectedOptionText.Text);
                    var outputPeer = new TextBlockAutomationPeer(selectedOptionText);
                    Assert.AreEqual(AutomationControlType.Text, outputPeer.GetAutomationControlType());
                    Assert.AreEqual("You clicked: Open", outputPeer.GetName());

                    var keyboardFile = keyboardMenu.Items[0];
                    var newItem = (MenuItem)keyboardFile.Items[0];
                    Assert.AreEqual("New", newItem.Header);
                    Assert.AreEqual("Ctrl+N", newItem.InputGestureText);

                    var submenuFile = submenuMenu.Items[0];
                    Assert.IsInstanceOfType(submenuFile.Items[0], typeof(MenuItem));
                    Assert.AreEqual(3, ((MenuItem)submenuFile.Items[0]).Items.Count);
                    Assert.AreEqual("Other Formats", ((MenuItem)((MenuItem)submenuFile.Items[0]).Items[2]).Header);
                    Assert.AreEqual("Open", ((MenuItem)submenuFile.Items[1]).Header);
                    Assert.IsInstanceOfType(submenuFile.Items[3], typeof(Separator));

                    var viewMenu = submenuMenu.Items[2];
                    Assert.AreEqual("View", viewMenu.Title);
                    var landscape = (Mux.RadioMenuItem)viewMenu.Items[2];
                    var portrait = (Mux.RadioMenuItem)viewMenu.Items[3];
                    var mediumIcons = (Mux.RadioMenuItem)viewMenu.Items[6];
                    Assert.AreEqual("OrientationGroup", landscape.GroupName);
                    Assert.IsFalse(landscape.IsChecked);
                    Assert.IsTrue(portrait.IsChecked);
                    Assert.AreEqual("SizeGroup", mediumIcons.GroupName);
                    Assert.IsTrue(mediumIcons.IsChecked);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void MenuFlyoutSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("MenuFlyout"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(7, page.Examples.Count);
                    Assert.AreEqual("An AppBarButton with a MenuFlyout.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A MenuFlyout with ToggleMenuFlyoutItems and MenuFlyoutSeparator.", page.Examples[1].HeaderText);
                    Assert.AreEqual("A MenuFlyout with cascading menus.", page.Examples[2].HeaderText);
                    Assert.AreEqual("A MenuFlyout with SplitMenuFlyoutItems.", page.Examples[3].HeaderText);
                    Assert.AreEqual("A MenuFlyout with icons.", page.Examples[4].HeaderText);
                    Assert.AreEqual("A MenuFlyout with icons and Keyboard Accelerators.", page.Examples[5].HeaderText);
                    Assert.AreEqual("A MenuFlyout with RadioMenuFlyoutItems", page.Examples[6].HeaderText);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "AppbarbuttonMenuflyout.txt",
                            "MenuflyoutCascadingMenus.txt",
                            "MenuflyoutIcons.txt",
                            "MenuflyoutIconsKeyboardAccelerators.txt",
                            "MenuflyoutRadiomenuflyoutitems.txt",
                            "MenuflyoutSplitmenuflyoutitems.txt",
                            "MenuflyoutTogglemenuflyoutitemsMenuflyoutseparator.txt"
                        },
                        page.SampleSnippets.Select(snippet => snippet.Title).ToArray());
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "MenuFlyoutItem Text=\"By rating\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ToggleMenuFlyoutItem Text=\"Repeat\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "MenuFlyoutSubItem Text=\"Send to\"");
                    StringAssert.Contains(page.Examples[3].XamlCode, "SplitMenuFlyoutItem Text=\"Save\"");
                    StringAssert.Contains(page.Examples[4].XamlCode, "MenuFlyoutItem Text=\"Copy\" Icon=\"Copy\"");
                    StringAssert.Contains(page.Examples[5].XamlCode, "KeyboardAccelerator Key=\"Delete\"");
                    StringAssert.Contains(page.Examples[6].XamlCode, "RadioMenuFlyoutItem Text=\"Portrait\"");

                    var sortButton = (Mux.AppBarButton)FindByAutomationId(page, "GallerySample_MenuFlyout_AppBarButton");
                    var control1 = FindNamedDescendant<StackPanel>(page, "Control1");
                    var control1Output = FindNamedDescendant<TextBlock>(page, "Control1Output");
                    Assert.IsNotNull(sortButton);
                    Assert.IsNotNull(control1);
                    Assert.IsNotNull(control1Output);
                    Assert.AreEqual(Mux.Symbol.Sort, ((Mux.SymbolIcon)sortButton.Icon).Symbol);
                    Assert.IsTrue(sortButton.IsCompact);
                    Assert.AreEqual("Sort", AutomationProperties.GetName(sortButton));
                    Assert.AreEqual(new Thickness(8, 0, 0, 0), control1Output.Margin);
                    Assert.AreEqual(VerticalAlignment.Center, control1Output.VerticalAlignment);
                    var sortFlyout = sortButton.Flyout as Mux.MenuFlyout;
                    Assert.IsNotNull(sortFlyout);
                    Assert.AreEqual(3, sortFlyout.Items.Count);
                    var ratingItem = (MenuItem)sortFlyout.Items[0];
                    Assert.AreEqual("By rating", ratingItem.Header);
                    Assert.AreEqual("rating", ratingItem.Tag);
                    var sortButtonPeer = new ModernWpf.Automation.Peers.AppBarButtonAutomationPeer(sortButton);
                    Assert.AreEqual(AutomationControlType.Button, sortButtonPeer.GetAutomationControlType());
                    Assert.AreEqual("Sort", sortButtonPeer.GetName());
                    Assert.IsInstanceOfType(sortButtonPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
                    var sortExpandCollapse = sortButtonPeer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
                    Assert.IsNotNull(sortExpandCollapse);
                    Assert.AreEqual(ExpandCollapseState.Collapsed, sortExpandCollapse.ExpandCollapseState);
                    sortExpandCollapse.Expand();
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(sortFlyout.IsOpen);
                    Assert.AreEqual(ExpandCollapseState.Expanded, sortExpandCollapse.ExpandCollapseState);

                    var ratingItemPeer = new MenuItemAutomationPeer(ratingItem);
                    Assert.AreEqual(AutomationControlType.MenuItem, ratingItemPeer.GetAutomationControlType());
                    Assert.AreEqual("By rating", ratingItemPeer.GetName());
                    Assert.IsInstanceOfType(ratingItemPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
                    sortExpandCollapse.Collapse();
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(sortFlyout.IsOpen);
                    Assert.AreEqual(ExpandCollapseState.Collapsed, sortExpandCollapse.ExpandCollapseState);
                    ratingItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    Assert.AreEqual("Sort by: rating", control1Output.Text);
                    var control1OutputPeer = new TextBlockAutomationPeer(control1Output);
                    Assert.AreEqual(AutomationControlType.Text, control1OutputPeer.GetAutomationControlType());
                    Assert.AreEqual("Sort by: rating", control1OutputPeer.GetName());

                    var control2 = FindNamedDescendant<Button>(page, "Control2");
                    var toggleFlyout = (Mux.MenuFlyout)Mux.FlyoutService.GetFlyout(control2);
                    Assert.AreEqual(4, toggleFlyout.Items.Count);
                    Assert.IsInstanceOfType(toggleFlyout.Items[1], typeof(Separator));
                    var repeat = (MenuItem)toggleFlyout.Items[2];
                    var shuffle = (MenuItem)toggleFlyout.Items[3];
                    Assert.AreEqual("RepeatToggleMenuFlyoutItem", repeat.Name);
                    Assert.IsTrue(repeat.IsCheckable);
                    Assert.IsTrue(repeat.IsChecked);
                    Assert.AreEqual("ShuffleToggleMenuFlyoutItem", shuffle.Name);
                    Assert.IsTrue(shuffle.IsChecked);
                    var repeatPeer = new MenuItemAutomationPeer(repeat);
                    Assert.AreEqual(AutomationControlType.MenuItem, repeatPeer.GetAutomationControlType());
                    Assert.AreEqual("Repeat", repeatPeer.GetName());
                    Assert.IsInstanceOfType(repeatPeer.GetPattern(PatternInterface.Toggle), typeof(IToggleProvider));

                    var control3 = FindNamedDescendant<Button>(page, "Control3");
                    var cascadingFlyout = (Mux.MenuFlyout)Mux.FlyoutService.GetFlyout(control3);
                    var sendTo = (MenuItem)cascadingFlyout.Items[1];
                    Assert.AreEqual("Send to", sendTo.Header);
                    Assert.AreEqual(3, sendTo.Items.Count);
                    var compressedFile = (MenuItem)sendTo.Items[2];
                    Assert.AreEqual("Compressed file", compressedFile.Header);
                    Assert.AreEqual(3, compressedFile.Items.Count);

                    var control3b = FindNamedDescendant<StackPanel>(page, "Control3b");
                    var splitButton = (Button)control3b.Children[0];
                    var splitOutput = FindNamedDescendant<TextBlock>(page, "Control3bOutput");
                    var splitFlyout = (Mux.MenuFlyout)Mux.FlyoutService.GetFlyout(splitButton);
                    var saveSplitItem = (MenuItem)splitFlyout.Items[0];
                    Assert.AreEqual("SaveSplitItem", saveSplitItem.Name);
                    Assert.AreEqual("Save", saveSplitItem.Header);
                    Assert.AreEqual(3, saveSplitItem.Items.Count);
                    Assert.AreEqual(new Thickness(8, 0, 0, 0), splitOutput.Margin);
                    Assert.AreEqual(VerticalAlignment.Center, splitOutput.VerticalAlignment);
                    ((MenuItem)saveSplitItem.Items[1]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    Assert.AreEqual("Clicked: Save as .pdf", splitOutput.Text);

                    var control4 = FindNamedDescendant<Button>(page, "Control4");
                    var iconsFlyout = (Mux.MenuFlyout)Mux.FlyoutService.GetFlyout(control4);
                    Assert.AreEqual("Share", ((MenuItem)iconsFlyout.Items[0]).Header);
                    Assert.AreEqual("\uE72D", ((Mux.FontIcon)((MenuItem)iconsFlyout.Items[0]).Icon).Glyph);
                    Assert.AreEqual(Mux.Symbol.Copy, ((Mux.SymbolIcon)((MenuItem)iconsFlyout.Items[1]).Icon).Symbol);
                    Assert.AreEqual(Mux.Symbol.Delete, ((Mux.SymbolIcon)((MenuItem)iconsFlyout.Items[2]).Icon).Symbol);

                    var control5 = FindNamedDescendant<Button>(page, "Control5");
                    var keyboardFlyout = (Mux.MenuFlyout)Mux.FlyoutService.GetFlyout(control5);
                    Assert.AreEqual("Segoe UI", control5.FontFamily.Source);
                    Assert.AreEqual("Ctrl+S", ((MenuItem)keyboardFlyout.Items[0]).InputGestureText);
                    Assert.AreEqual("Ctrl+C", ((MenuItem)keyboardFlyout.Items[1]).InputGestureText);
                    Assert.AreEqual("Delete", ((MenuItem)keyboardFlyout.Items[2]).InputGestureText);
                    Assert.AreEqual("Segoe UI", ((MenuItem)keyboardFlyout.Items[0]).FontFamily.Source);
                    Assert.AreEqual("Consolas", ((MenuItem)keyboardFlyout.Items[1]).FontFamily.Source);
                    Assert.AreEqual("Segoe UI", ((MenuItem)keyboardFlyout.Items[2]).FontFamily.Source);

                    var control6 = FindNamedDescendant<Button>(page, "Control6");
                    var radioFlyout = (Mux.MenuFlyout)Mux.FlyoutService.GetFlyout(control6);
                    var landscape = (Mux.RadioMenuItem)radioFlyout.Items[0];
                    var portrait = (Mux.RadioMenuItem)radioFlyout.Items[1];
                    var mediumIcons = (Mux.RadioMenuItem)radioFlyout.Items[4];
                    Assert.AreEqual("OrientationGroup", landscape.GroupName);
                    Assert.IsFalse(landscape.IsChecked);
                    Assert.IsTrue(portrait.IsChecked);
                    Assert.AreEqual("SizeGroup", mediumIcons.GroupName);
                    Assert.IsTrue(mediumIcons.IsChecked);

                    var landscapePeer = new MenuItemAutomationPeer(landscape);
                    var portraitPeer = new MenuItemAutomationPeer(portrait);
                    Assert.AreEqual(AutomationControlType.MenuItem, landscapePeer.GetAutomationControlType());
                    Assert.AreEqual("Landscape", landscapePeer.GetName());
                    Assert.AreEqual(AutomationControlType.MenuItem, portraitPeer.GetAutomationControlType());
                    Assert.AreEqual("Portrait", portraitPeer.GetName());
                    var landscapeToggle = landscapePeer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
                    var portraitToggle = portraitPeer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
                    Assert.IsNotNull(landscapeToggle);
                    Assert.IsNotNull(portraitToggle);
                    Assert.AreEqual(ToggleState.Off, landscapeToggle!.ToggleState);
                    Assert.AreEqual(ToggleState.On, portraitToggle!.ToggleState);

                    landscapeToggle.Toggle();
                    Assert.AreEqual(ToggleState.On, landscapeToggle.ToggleState);
                    Assert.AreEqual(ToggleState.Off, portraitToggle.ToggleState);

                    landscapeToggle.Toggle();
                    Assert.AreEqual(ToggleState.On, landscapeToggle.ToggleState);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void AppBarControlsMatchWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var appBarButtonPage = new ItemPage(GalleryCatalog.FindItem("AppBarButton"));
                var appBarSeparatorPage = new ItemPage(GalleryCatalog.FindItem("AppBarSeparator"));
                var appBarToggleButtonPage = new ItemPage(GalleryCatalog.FindItem("AppBarToggleButton"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                try
                {
                    window.Content = appBarButtonPage;
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(6, appBarButtonPage.Examples.Count);
                    Assert.AreEqual("An AppBarButton with a symbol icon.", appBarButtonPage.Examples[0].HeaderText);
                    Assert.AreEqual("An AppBarButton with a bitmap icon.", appBarButtonPage.Examples[1].HeaderText);
                    Assert.AreEqual("An AppBarButton with a font icon.", appBarButtonPage.Examples[2].HeaderText);
                    Assert.AreEqual("An AppBarButton with a path icon.", appBarButtonPage.Examples[3].HeaderText);
                    Assert.AreEqual("An AppBarButton with a KeyboardAccelerator", appBarButtonPage.Examples[4].HeaderText);
                    Assert.AreEqual("An AppBarButton that opens a Flyout containing an input control.", appBarButtonPage.Examples[5].HeaderText);
                    Assert.IsFalse(appBarButtonPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(appBarButtonPage.Examples[0].XamlCode, "Icon=\"Like\"");
                    StringAssert.Contains(appBarButtonPage.Examples[1].XamlCode, "BitmapIcon UriSource=\"ms-appx:///Assets/SampleMedia/Slices2.png\"");
                    StringAssert.Contains(appBarButtonPage.Examples[2].XamlCode, "FontFamily=\"Candara\"");
                    StringAssert.Contains(appBarButtonPage.Examples[3].XamlCode, "PathIcon Data=\"F1 M 20,20L 24,10L 24,24L 5,24\"");
                    StringAssert.Contains(appBarButtonPage.Examples[4].XamlCode, "KeyboardAccelerator Modifiers=\"Control\" Key=\"S\"");
                    StringAssert.Contains(appBarButtonPage.Examples[5].XamlCode, "PlaceholderText=\"Input text here\"");
                    StringAssert.Contains(appBarButtonPage.Examples[5].XamlCode, "<Flyout>");
                    StringAssert.Contains(appBarButtonPage.Examples[5].XamlCode, "</Flyout>");
                    StringAssert.Contains(appBarButtonPage.Examples[5].XamlCode, "</AppBarButton.Flyout>");
                    Assert.IsFalse(appBarButtonPage.Examples[5].XamlCode.Contains("<Flyout/>", StringComparison.Ordinal));

                    var symbolButton = (Mux.AppBarButton)FindByAutomationId(appBarButtonPage, "GallerySample_AppBarButton_AppBarButton");
                    var bitmapButton = FindNamedDescendant<Mux.AppBarButton>(appBarButtonPage, "Button2");
                    var fontButton = FindNamedDescendant<Mux.AppBarButton>(appBarButtonPage, "Button3");
                    var pathButton = FindNamedDescendant<Mux.AppBarButton>(appBarButtonPage, "Button4");
                    var acceleratorButton = FindNamedDescendant<Mux.AppBarButton>(appBarButtonPage, "Button5");
                    var flyoutButton = FindNamedDescendant<Mux.AppBarButton>(appBarButtonPage, "Button6");
                    Assert.IsNotNull(symbolButton);
                    Assert.IsNotNull(bitmapButton);
                    Assert.IsNotNull(fontButton);
                    Assert.IsNotNull(pathButton);
                    Assert.IsNotNull(acceleratorButton);
                    Assert.IsNotNull(flyoutButton);

                    Assert.AreEqual("Button1", symbolButton.Name);
                    Assert.AreEqual("SymbolIcon", symbolButton.Label);
                    Assert.AreEqual(Mux.Symbol.Like, ((Mux.SymbolIcon)symbolButton.Icon).Symbol);
                    var output1 = FindNamedDescendant<TextBlock>(appBarButtonPage, "Control1Output");
                    symbolButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("You clicked: Button1", output1.Text);

                    Assert.AreEqual("BitmapIcon", bitmapButton.Label);
                    StringAssert.Contains(((Mux.BitmapIcon)bitmapButton.Icon).UriSource.ToString(), "Slices2.png");
                    Assert.AreEqual("FontIcon", fontButton.Label);
                    Assert.AreEqual("Candara", ((Mux.FontIcon)fontButton.Icon).FontFamily.Source);
                    Assert.AreEqual("\u03A3", ((Mux.FontIcon)fontButton.Icon).Glyph);
                    Assert.AreEqual("PathIcon", pathButton.Label);
                    Assert.IsInstanceOfType(pathButton.Content, typeof(Viewbox));
                    var pathViewbox = (Viewbox)pathButton.Content;
                    Assert.AreEqual(20d, pathViewbox.Width);
                    Assert.AreEqual(20d, pathViewbox.Height);
                    Assert.IsInstanceOfType(pathViewbox.Child, typeof(Mux.PathIcon));
                    Assert.IsTrue(pathViewbox.Child.RenderSize.Width > 0);
                    Assert.IsTrue(pathViewbox.Child.RenderSize.Height > 0);
                    Assert.AreEqual("Save", acceleratorButton.Label);
                    Assert.AreEqual("Ctrl+S", acceleratorButton.InputGestureText);
                    Assert.AreEqual("Edit", flyoutButton.Label);
                    var flyout = flyoutButton.Flyout as Mux.Flyout;
                    Assert.IsNotNull(flyout);
                    var flyoutTextBox = flyout.Content as TextBox;
                    Assert.IsNotNull(flyoutTextBox);
                    Assert.AreEqual(240.0, flyoutTextBox.MinWidth);
                    Assert.AreEqual("Input text here", ModernWpf.Controls.Primitives.ControlHelper.GetPlaceholderText(flyoutTextBox));

                    window.Content = appBarSeparatorPage;
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, appBarSeparatorPage.Examples.Count);
                    Assert.AreEqual("AppBarButtons separated by AppBarSeparators.", appBarSeparatorPage.Examples[0].HeaderText);
                    Assert.IsFalse(appBarSeparatorPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(appBarSeparatorPage.Examples[0].XamlCode, "AppBarButton Icon=\"AttachCamera\"");
                    StringAssert.Contains(appBarSeparatorPage.Examples[0].XamlCode, "<AppBarSeparator />");

                    var commandBar = (Mux.CommandBar)FindByAutomationId(appBarSeparatorPage, "GallerySample_AppBarSeparator_CommandBar");
                    Assert.IsNotNull(commandBar);
                    Assert.AreEqual("Control1", commandBar.Name);
                    Assert.AreEqual(Mux.CommandBarDefaultLabelPosition.Collapsed, commandBar.DefaultLabelPosition);
                    Assert.AreEqual(Mux.CommandBarOverflowButtonVisibility.Visible, commandBar.OverflowButtonVisibility);
                    Assert.AreEqual(6, commandBar.PrimaryCommands.Count);
                    AssertAppBarButton(commandBar.PrimaryCommands[0], Mux.Symbol.AttachCamera, "Attach Camera");
                    Assert.IsInstanceOfType(commandBar.PrimaryCommands[1], typeof(Mux.AppBarSeparator));
                    AssertAppBarButton(commandBar.PrimaryCommands[2], Mux.Symbol.Like, "Like");
                    AssertAppBarButton(commandBar.PrimaryCommands[3], Mux.Symbol.Dislike, "Dislike");
                    Assert.IsInstanceOfType(commandBar.PrimaryCommands[4], typeof(Mux.AppBarSeparator));
                    AssertAppBarButton(commandBar.PrimaryCommands[5], Mux.Symbol.Orientation, "Orientation");

                    window.Content = appBarToggleButtonPage;
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(4, appBarToggleButtonPage.Examples.Count);
                    Assert.AreEqual("An AppBarToggleButton with a symbol icon.", appBarToggleButtonPage.Examples[0].HeaderText);
                    Assert.AreEqual("An AppBarToggleButton with a bitmap icon.", appBarToggleButtonPage.Examples[1].HeaderText);
                    Assert.AreEqual("An AppBarToggleButton with a font icon.", appBarToggleButtonPage.Examples[2].HeaderText);
                    Assert.AreEqual("A three-state AppBarToggleButton with a path icon.", appBarToggleButtonPage.Examples[3].HeaderText);
                    Assert.IsFalse(appBarToggleButtonPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(appBarToggleButtonPage.Examples[0].XamlCode, "Icon=\"Shuffle\"");
                    StringAssert.Contains(appBarToggleButtonPage.Examples[3].XamlCode, "IsThreeState=\"True\"");

                    var symbolToggleButton = (Mux.AppBarToggleButton)FindByAutomationId(appBarToggleButtonPage, "GallerySample_AppBarToggleButton_AppBarToggleButton");
                    var bitmapToggleButton = FindNamedDescendant<Mux.AppBarToggleButton>(appBarToggleButtonPage, "Button2");
                    var fontToggleButton = FindNamedDescendant<Mux.AppBarToggleButton>(appBarToggleButtonPage, "Button3");
                    var pathToggleButton = FindNamedDescendant<Mux.AppBarToggleButton>(appBarToggleButtonPage, "Button4");
                    Assert.IsNotNull(symbolToggleButton);
                    Assert.IsNotNull(bitmapToggleButton);
                    Assert.IsNotNull(fontToggleButton);
                    Assert.IsNotNull(pathToggleButton);
                    Assert.AreEqual(Mux.Symbol.Shuffle, ((Mux.SymbolIcon)symbolToggleButton.Icon).Symbol);
                    Assert.AreEqual("BitmapIcon", bitmapToggleButton.Label);
                    StringAssert.Contains(((Mux.BitmapIcon)bitmapToggleButton.Icon).UriSource.ToString(), "Slices2.png");
                    Assert.AreEqual("\u03A3", ((Mux.FontIcon)fontToggleButton.Icon).Glyph);
                    Assert.IsTrue(pathToggleButton.IsThreeState);
                    Assert.IsInstanceOfType(pathToggleButton.Content, typeof(Viewbox));
                    var pathToggleViewbox = (Viewbox)pathToggleButton.Content;
                    Assert.AreEqual(20d, pathToggleViewbox.Width);
                    Assert.AreEqual(20d, pathToggleViewbox.Height);
                    Assert.IsInstanceOfType(pathToggleViewbox.Child, typeof(Mux.PathIcon));
                    Assert.IsTrue(pathToggleViewbox.Child.RenderSize.Width > 0);
                    Assert.IsTrue(pathToggleViewbox.Child.RenderSize.Height > 0);

                    var toggleOutput = FindNamedDescendant<TextBlock>(appBarToggleButtonPage, "Control1Output");
                    symbolToggleButton.IsChecked = true;
                    symbolToggleButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("IsChecked = True", toggleOutput.Text);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void DropDownButtonSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("DropDownButton"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("Simple DropDownButton", page.Examples[0].HeaderText);
                    Assert.AreEqual("DropDownButton with Icons", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<DropDownButton Content=\"Email\">");
                    StringAssert.Contains(page.Examples[1].XamlCode, "AutomationProperties.Name=\"Email\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "Glyph=\"&#xE715;\"");

                    var simpleButton = (Mux.DropDownButton)FindByAutomationId(page, "GallerySample_DropDownButton_DropDownButton");
                    var iconButton = (Mux.DropDownButton)FindByAutomationId(page, "GallerySample_DropDownButton_IconDropDownButton");
                    Assert.IsNotNull(simpleButton);
                    Assert.IsNotNull(iconButton);
                    Assert.AreEqual("Email", simpleButton.Content);
                    Assert.AreEqual("Email", AutomationProperties.GetName(iconButton));
                    Assert.AreEqual("\uE715", ((Mux.FontIcon)iconButton.Content).Glyph);
                    AssertDropDownButtonShowsChevron(simpleButton);
                    AssertDropDownButtonShowsChevron(iconButton);

                    AssertEmailDropDownFlyout(simpleButton.Flyout, includeIcons: false);
                    AssertEmailDropDownFlyout(iconButton.Flyout, includeIcons: true);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        private static void AssertDropDownButtonShowsChevron(Mux.DropDownButton button)
        {
            var contentPresenter = FindDescendants<Mux.ContentPresenterEx>(button)
                .SingleOrDefault(item => item.Name == "ContentPresenter");
            var chevron = FindDescendants<Mux.FontIconFallback>(button)
                .SingleOrDefault(item => item.Name == "ChevronIcon");
            Assert.IsNotNull(contentPresenter);
            Assert.IsNotNull(chevron);

            var contentBounds = contentPresenter.TransformToAncestor(button).TransformBounds(new Rect(contentPresenter.RenderSize));
            var chevronBounds = chevron.TransformToAncestor(button).TransformBounds(new Rect(chevron.RenderSize));
            Assert.IsTrue(chevronBounds.Width > 0, "DropDownButton chevron should have positive layout width.");
            Assert.IsTrue(chevronBounds.Left > contentBounds.Left, "DropDownButton chevron should be laid out to the right of its content.");
            Assert.IsTrue(
                chevronBounds.Right <= button.ActualWidth,
                $"DropDownButton chevron should fit inside the button bounds. ButtonWidth={button.ActualWidth}; Chevron={chevronBounds}");
        }

        [TestMethod]
        public void SplitButtonSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("SplitButton"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A SplitButton controlling text color in a RichEditBox", page.Examples[0].HeaderText);
                    Assert.AreEqual("A SplitButton with text", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "myColorButton");
                    StringAssert.Contains(page.Examples[0].XamlCode, "CurrentColor");
                    StringAssert.Contains(page.Examples[1].XamlCode, "Choose color");

                    var colorButton = (Mux.SplitButton)FindByAutomationId(page, "GallerySample_SplitButton_SplitButton");
                    var textButton = (Mux.SplitButton)FindByAutomationId(page, "GallerySample_SplitButton_TextSplitButton");
                    var richTextBox = FindNamedDescendant<RichTextBox>(page, "myRichTextBox");
                    Assert.IsNotNull(colorButton);
                    Assert.IsNotNull(textButton);
                    Assert.IsNotNull(richTextBox);

                    Assert.AreEqual("Font color", AutomationProperties.GetName(colorButton));
                    Assert.AreEqual(0.0, colorButton.MinWidth);
                    Assert.AreEqual(0.0, colorButton.MinHeight);
                    Assert.AreEqual(new Thickness(0), colorButton.Padding);
                    Assert.AreEqual(VerticalAlignment.Top, colorButton.VerticalAlignment);
                    var currentColor = (Border)colorButton.Content;
                    Assert.AreEqual("CurrentColor", currentColor.Name);
                    Assert.AreEqual(32.0, currentColor.Width);
                    Assert.AreEqual(32.0, currentColor.Height);
                    Assert.AreEqual(new CornerRadius(4, 0, 0, 4), currentColor.CornerRadius);
                    Assert.AreEqual(Colors.Green, ((SolidColorBrush)currentColor.Background).Color);
                    AssertColorSwatchFlyout(colorButton.Flyout, expectedCount: 8, includeBlack: false);

                    Assert.AreEqual("Choose color", textButton.Content);
                    Assert.AreEqual("Font color with text", AutomationProperties.GetName(textButton));
                    Assert.AreEqual(0.0, textButton.MinWidth);
                    Assert.AreEqual(0.0, textButton.MinHeight);
                    Assert.AreEqual(new Thickness(5), textButton.Padding);
                    Assert.AreEqual(VerticalAlignment.Top, textButton.VerticalAlignment);
                    Assert.AreEqual(HorizontalAlignment.Left, textButton.HorizontalAlignment);
                    AssertColorSwatchFlyout(textButton.Flyout, expectedCount: 9, includeBlack: true);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ToggleSplitButtonSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ToggleSplitButton"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("Using ToggleSplitButton to control bulleted list functionality in RichEditBox", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "myListButton");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Symbol=\"List\"");

                    var toggleSplitButton = (Mux.ToggleSplitButton)FindByAutomationId(page, "GallerySample_ToggleSplitButton_ToggleSplitButton");
                    var symbolIcon = FindNamedDescendant<Mux.SymbolIcon>(page, "mySymbolIcon");
                    var richTextBox = FindNamedDescendant<RichTextBox>(page, "myRichTextBox");
                    Assert.IsNotNull(toggleSplitButton);
                    Assert.IsNotNull(symbolIcon);
                    Assert.IsNotNull(richTextBox);

                    Assert.AreEqual("Bullets", AutomationProperties.GetName(toggleSplitButton));
                    Assert.AreEqual(VerticalAlignment.Top, toggleSplitButton.VerticalAlignment);
                    Assert.AreSame(symbolIcon, toggleSplitButton.Content);
                    Assert.AreEqual(Mux.Symbol.List, symbolIcon.Symbol);
                    Assert.AreEqual(240.0, richTextBox.Width);
                    Assert.AreEqual(96.0, richTextBox.MinHeight);
                    Assert.AreEqual("Text entry", AutomationProperties.GetName(richTextBox));

                    var flyoutPanel = AssertListMarkerFlyout(toggleSplitButton.Flyout);
                    var romanButton = (Button)flyoutPanel.Children[1];
                    romanButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, romanButton));
                    Assert.IsTrue(toggleSplitButton.IsChecked);
                    Assert.AreEqual(Mux.Symbol.Bullets, symbolIcon.Symbol);
                    Assert.AreEqual("Roman Numerals", AutomationProperties.GetName(toggleSplitButton));
                    var list = richTextBox.Document.Blocks.OfType<List>().Single();
                    Assert.AreEqual(TextMarkerStyle.UpperRoman, list.MarkerStyle);

                    toggleSplitButton.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(richTextBox.Document.Blocks.OfType<List>().Any());
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void HyperlinkButtonSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("HyperlinkButton"));
                GalleryItem requestedItem = null;
                page.ItemRequested = item => requestedItem = item;
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A hyperlink button that navigates to a URI.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A hyperlink button that handles a Click event.", page.Examples[1].HeaderText);
                    Assert.AreEqual(new Thickness(0, 16, 0, 0), page.Examples[1].Margin);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.AreEqual("<HyperlinkButton Content=\"ModernWPF repository\" NavigateUri=\"https://github.com/Kinnara/ModernWpf\" $(IsEnabled)/>", page.Examples[0].XamlCode);
                    Assert.AreEqual("<HyperlinkButton Content=\"ToggleButton\" Click=\"HyperlinkButton_Click\"/>", page.Examples[1].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);
                    Assert.IsNotNull(page.Examples[0].OptionsContent);
                    Assert.IsNull(page.Examples[1].OptionsContent);

                    var uriButton = (Mux.HyperlinkButton)FindByAutomationId(page, "GallerySample_HyperlinkButton_HyperlinkButton");
                    var clickButton = (Mux.HyperlinkButton)FindByAutomationId(page, "GallerySample_HyperlinkButton_ClickHyperlinkButton");
                    Assert.IsNotNull(uriButton);
                    Assert.IsNotNull(clickButton);

                    Assert.AreEqual("Control1", uriButton.Name);
                    Assert.AreEqual("ModernWPF repository", uriButton.Content);
                    Assert.AreEqual(GalleryBranding.RepositoryUrl, uriButton.NavigateUri.ToString());
                    Assert.AreEqual("Control2", clickButton.Name);
                    Assert.AreEqual("Go to ToggleButton", clickButton.Content);
                    Assert.IsNull(clickButton.NavigateUri);

                    var uriPeer = new HyperlinkButtonAutomationPeer(uriButton);
                    var clickPeer = new HyperlinkButtonAutomationPeer(clickButton);
                    Assert.AreEqual(AutomationControlType.Hyperlink, uriPeer.GetAutomationControlType());
                    Assert.AreEqual(AutomationControlType.Hyperlink, clickPeer.GetAutomationControlType());
                    Assert.AreEqual("Hyperlink", uriPeer.GetClassName());
                    Assert.AreEqual("Hyperlink", clickPeer.GetClassName());
                    Assert.AreEqual("ModernWPF repository", uriPeer.GetName());
                    Assert.AreEqual("Go to ToggleButton", clickPeer.GetName());
                    Assert.IsInstanceOfType(uriPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
                    Assert.IsInstanceOfType(clickPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));

                    var options = (StackPanel)page.Examples[0].OptionsContent;
                    var disableControl = (CheckBox)options.Children[0];
                    Assert.AreEqual("DisableControl1", disableControl.Name);
                    Assert.AreEqual("Disable hyperlink button", disableControl.Content);
                    Assert.AreEqual(false, disableControl.IsChecked);
                    var disablePeer = new CheckBoxAutomationPeer(disableControl);
                    Assert.AreEqual(AutomationControlType.CheckBox, disablePeer.GetAutomationControlType());
                    Assert.AreEqual("Disable hyperlink button", disablePeer.GetName());
                    Assert.IsInstanceOfType(disablePeer.GetPattern(PatternInterface.Toggle), typeof(IToggleProvider));
                    disableControl.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(uriButton.IsEnabled);
                    Assert.ThrowsExactly<ElementNotEnabledException>(() =>
                        ((IInvokeProvider)uriPeer.GetPattern(PatternInterface.Invoke)).Invoke());
                    disableControl.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(uriButton.IsEnabled);

                    ((IInvokeProvider)clickPeer.GetPattern(PatternInterface.Invoke)).Invoke();
                    WpfTestHost.DoEvents();
                    Assert.IsNotNull(requestedItem);
                    Assert.AreEqual("ToggleButton", requestedItem.UniqueId);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ColorPickerSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ColorPicker"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("ColorPicker Properties.", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "ColorSpectrumShape=\"$(ColorSpectrumShape)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsMoreButtonVisible=\"$(IsMoreButtonVisible)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsColorSliderVisible=\"$(IsColorSliderVisible)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsColorChannelTextInputVisible=\"$(IsColorChannelTextInputVisible)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsHexInputVisible=\"$(IsHexInputVisible)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsAlphaEnabled=\"$(IsAlphaEnabled)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsAlphaSliderVisible=\"$(IsAlphaSliderVisible)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsAlphaTextInputVisible=\"$(IsAlphaTextInputVisible)\"");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNotNull(page.Examples[0].OptionsContent);

                    var colorPicker = (Mux.ColorPicker)FindByAutomationId(page, "GallerySample_ColorPicker_ColorPicker");
                    var moreButtonCheck = FindNamedDescendant<CheckBox>(page, "moreBtn");
                    var colorSliderCheck = FindNamedDescendant<CheckBox>(page, "colorSlider");
                    var colorChannelInputCheck = FindNamedDescendant<CheckBox>(page, "colorChannelInput");
                    var hexInputCheck = FindNamedDescendant<CheckBox>(page, "hexInput");
                    var alphaCheck = FindNamedDescendant<CheckBox>(page, "alpha");
                    var alphaSliderCheck = FindNamedDescendant<CheckBox>(page, "alphaSlider");
                    var alphaTextInputCheck = FindNamedDescendant<CheckBox>(page, "alphaTextInput");
                    var shapeRadioButtons = FindNamedDescendant<Mux.RadioButtons>(page, "ColorSpectrumShapeRadioButtons");
                    var previewRect = FindNamedDescendant<System.Windows.Shapes.Rectangle>(page, "previewRect");
                    var colorSpectrum = FindNamedDescendant<ColorSpectrum>(colorPicker, "ColorSpectrum");
                    Assert.IsNotNull(colorPicker);
                    Assert.IsNotNull(moreButtonCheck);
                    Assert.IsNotNull(colorSliderCheck);
                    Assert.IsNotNull(colorChannelInputCheck);
                    Assert.IsNotNull(hexInputCheck);
                    Assert.IsNotNull(alphaCheck);
                    Assert.IsNotNull(alphaSliderCheck);
                    Assert.IsNotNull(alphaTextInputCheck);
                    Assert.IsNotNull(shapeRadioButtons);
                    Assert.IsNotNull(previewRect);
                    Assert.IsNotNull(colorSpectrum);
                    Assert.AreSame(page.Examples[0].OptionsContent, moreButtonCheck.Parent);

                    var moreButtonPeer = new CheckBoxAutomationPeer(moreButtonCheck);
                    var alphaPeer = new CheckBoxAutomationPeer(alphaCheck);
                    var shapePeer = new RadioButtonsAutomationPeer(shapeRadioButtons);
                    var spectrumPeer = UIElementAutomationPeer.CreatePeerForElement(colorSpectrum);
                    Assert.AreEqual(AutomationControlType.CheckBox, moreButtonPeer.GetAutomationControlType());
                    Assert.AreEqual("IsMoreButtonVisible", moreButtonPeer.GetName());
                    Assert.IsInstanceOfType(moreButtonPeer.GetPattern(PatternInterface.Toggle), typeof(IToggleProvider));
                    Assert.AreEqual(AutomationControlType.CheckBox, alphaPeer.GetAutomationControlType());
                    Assert.AreEqual("Alpha Enabled", alphaPeer.GetName());
                    Assert.IsInstanceOfType(alphaPeer.GetPattern(PatternInterface.Toggle), typeof(IToggleProvider));
                    Assert.AreEqual(AutomationControlType.Group, shapePeer.GetAutomationControlType());
                    Assert.AreEqual("Colorspectrum shape", shapePeer.GetName());
                    Assert.AreEqual(AutomationControlType.Slider, spectrumPeer.GetAutomationControlType());
                    Assert.AreEqual("Color picker", spectrumPeer.GetName());
                    Assert.IsInstanceOfType(spectrumPeer.GetPattern(PatternInterface.Value), typeof(IValueProvider));

                    Assert.AreEqual("colorPicker", colorPicker.Name);
                    Assert.IsFalse(colorPicker.IsMoreButtonVisible);
                    Assert.IsTrue(colorPicker.IsColorSliderVisible);
                    Assert.IsTrue(colorPicker.IsColorChannelTextInputVisible);
                    Assert.IsTrue(colorPicker.IsHexInputVisible);
                    Assert.IsFalse(colorPicker.IsAlphaEnabled);
                    Assert.IsTrue(colorPicker.IsAlphaSliderVisible);
                    Assert.IsTrue(colorPicker.IsAlphaTextInputVisible);
                    Assert.AreEqual(Mux.ColorSpectrumShape.Box, colorPicker.ColorSpectrumShape);
                    Assert.AreEqual("Colorspectrum shape", shapeRadioButtons.Header);
                    Assert.AreEqual(0, shapeRadioButtons.SelectedIndex);
                    Assert.AreEqual(2, shapeRadioButtons.Items.Count);
                    Assert.AreEqual("Box", shapeRadioButtons.Items[0]);
                    Assert.AreEqual("Ring", shapeRadioButtons.Items[1]);
                    Assert.AreEqual(250.0, ((FrameworkElement)moreButtonCheck.Parent).Width);
                    Assert.AreEqual(new Thickness(0, -5, 0, 0), ((FrameworkElement)moreButtonCheck.Parent).Margin);
                    Assert.IsFalse(alphaSliderCheck.IsEnabled);
                    Assert.IsFalse(alphaTextInputCheck.IsEnabled);
                    Assert.AreEqual(100.0, previewRect.Height);
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), previewRect.Margin);
                    Assert.AreEqual(1.0, previewRect.StrokeThickness);
                    Assert.AreEqual(colorPicker.Color, ((SolidColorBrush)previewRect.Fill).Color);
                    AssertColorPickerTextInputLayoutMatchesReference(colorPicker);

                    ((IToggleProvider)moreButtonPeer.GetPattern(PatternInterface.Toggle)).Toggle();
                    colorSliderCheck.IsChecked = false;
                    colorChannelInputCheck.IsChecked = false;
                    hexInputCheck.IsChecked = false;
                    ((IToggleProvider)alphaPeer.GetPattern(PatternInterface.Toggle)).Toggle();
                    alphaSliderCheck.IsChecked = false;
                    alphaTextInputCheck.IsChecked = false;
                    shapeRadioButtons.SelectedIndex = 1;
                    colorPicker.Color = Color.FromRgb(51, 102, 204);
                    WpfTestHost.DoEvents();

                    Assert.IsTrue(colorPicker.IsMoreButtonVisible);
                    Assert.AreEqual(ToggleState.On, ((IToggleProvider)moreButtonPeer.GetPattern(PatternInterface.Toggle)).ToggleState);
                    Assert.IsFalse(colorPicker.IsColorSliderVisible);
                    Assert.IsFalse(colorPicker.IsColorChannelTextInputVisible);
                    Assert.IsFalse(colorPicker.IsHexInputVisible);
                    Assert.IsTrue(colorPicker.IsAlphaEnabled);
                    Assert.AreEqual(ToggleState.On, ((IToggleProvider)alphaPeer.GetPattern(PatternInterface.Toggle)).ToggleState);
                    Assert.IsTrue(alphaSliderCheck.IsEnabled);
                    Assert.IsTrue(alphaTextInputCheck.IsEnabled);
                    Assert.IsFalse(colorPicker.IsAlphaSliderVisible);
                    Assert.IsFalse(colorPicker.IsAlphaTextInputVisible);
                    Assert.AreEqual(Mux.ColorSpectrumShape.Ring, colorPicker.ColorSpectrumShape);
                    Assert.AreEqual(Color.FromRgb(51, 102, 204), ((SolidColorBrush)previewRect.Fill).Color);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void RatingControlSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("RatingControl"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A simple RatingControl", page.Examples[0].HeaderText);
                    Assert.AreEqual("PlaceholderValue of RatingControl", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.IsNotNull(page.Examples[0].OutputContent);
                    Assert.IsNotNull(page.Examples[0].OptionsContent);
                    Assert.IsNull(page.Examples[1].OutputContent);
                    Assert.IsNotNull(page.Examples[1].OptionsContent);
                    StringAssert.Contains(page.Examples[0].XamlCode, "AutomationProperties.Name=\"Simple RatingControl\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsClearEnabled=\"$(IsClearEnabled)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsReadOnly=\"$(IsReadOnly)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Caption=\"$(Caption)\"");
                    Assert.AreEqual("<RatingControl AutomationProperties.Name=\"RatingControl with placeholder\" PlaceholderValue=\"$(Slider)\" />", page.Examples[1].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var rating = (Mux.RatingControl)FindByAutomationId(page, "GallerySample_RatingControl_RatingControl");
                    var placeholderRating = (Mux.RatingControl)FindByAutomationId(page, "GallerySample_RatingControl_PlaceholderRatingControl");
                    var clearEnabledCheck = FindNamedDescendant<CheckBox>(page, "clearEnabledCheck");
                    var readOnlyCheck = FindNamedDescendant<CheckBox>(page, "readOnlyCheck");
                    var slider = FindNamedDescendant<Slider>(page, "slider");
                    Assert.IsNotNull(rating);
                    Assert.IsNotNull(placeholderRating);
                    Assert.IsNotNull(clearEnabledCheck);
                    Assert.IsNotNull(readOnlyCheck);
                    Assert.IsNotNull(slider);

                    Assert.AreEqual("RatingControl1", rating.Name);
                    Assert.AreEqual(HorizontalAlignment.Left, rating.HorizontalAlignment);
                    Assert.AreEqual("Simple RatingControl", AutomationProperties.GetName(rating));
                    Assert.AreEqual("312 ratings", rating.Caption);
                    Assert.IsFalse(rating.IsClearEnabled);
                    Assert.IsFalse(rating.IsReadOnly);

                    var firstExampleRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    var ratingStack = (StackPanel)firstExampleRoot.Children[0];
                    var output = (TextBlock)page.Examples[0].OutputContent;
                    Assert.AreSame(rating, ratingStack.Children[0]);
                    Assert.AreEqual(FontWeights.Bold, output.FontWeight);
                    Assert.AreEqual(new Thickness(0), output.Margin);
                    Assert.AreEqual(rating.Value.ToString(CultureInfo.InvariantCulture), output.Text);

                    clearEnabledCheck.IsChecked = true;
                    readOnlyCheck.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(rating.IsClearEnabled);
                    Assert.IsTrue(rating.IsReadOnly);

                    clearEnabledCheck.IsChecked = false;
                    readOnlyCheck.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(rating.IsClearEnabled);
                    Assert.IsFalse(rating.IsReadOnly);

                    rating.Value = 3;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Your rating", rating.Caption);
                    Assert.AreEqual("3", output.Text);

                    Assert.AreEqual("RatingControl2", placeholderRating.Name);
                    Assert.AreEqual(HorizontalAlignment.Left, placeholderRating.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Top, placeholderRating.VerticalAlignment);
                    Assert.AreEqual("RatingControl with placeholder", AutomationProperties.GetName(placeholderRating));
                    Assert.AreEqual(0, placeholderRating.PlaceholderValue);
                    Assert.AreEqual("slider", slider.Name);
                    Assert.AreEqual(0, slider.Minimum);
                    Assert.AreEqual(5, slider.Maximum);
                    Assert.AreEqual(0.5, slider.SmallChange);
                    Assert.AreEqual(0.5, slider.TickFrequency);
                    Assert.IsTrue(slider.IsSelectionRangeEnabled);
                    Assert.AreEqual(0d, slider.SelectionStart);
                    Assert.AreEqual(0d, slider.SelectionEnd);

                    slider.Value = 2.5;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(2.5, placeholderRating.PlaceholderValue);
                    Assert.AreEqual(2.5, slider.SelectionEnd);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void RepeatButtonSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("RepeatButton"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("A simple RepeatButton with text content.", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.AreEqual("<RepeatButton Content=\"Click and hold\" Click=\"RepeatButton_Click\" $(IsEnabled)/>", page.Examples[0].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);

                    var button = (RepeatButton)FindByAutomationId(page, "GallerySample_RepeatButton_RepeatButton");
                    var output = FindNamedDescendant<TextBlock>(page, "Control1Output");
                    Assert.IsNotNull(button);
                    Assert.IsNotNull(output);

                    Assert.AreEqual("Control1", button.Name);
                    Assert.AreEqual("Click and hold", button.Content);
                    Assert.AreEqual("Control1Output", output.Name);
                    Assert.AreEqual(new Thickness(8, 0, 0, 0), output.Margin);
                    Assert.AreEqual(VerticalAlignment.Center, output.VerticalAlignment);
                    Assert.AreEqual("GallerySample_RepeatButton_Output", AutomationProperties.GetAutomationId(output));
                    Assert.AreEqual("Control output", AutomationProperties.GetName(output));
                    Assert.AreEqual(AutomationLiveSetting.Polite, AutomationProperties.GetLiveSetting(output));
                    Assert.AreEqual(string.Empty, output.Text);

                    var buttonPeer = new RepeatButtonAutomationPeer(button);
                    Assert.AreEqual("Click and hold", buttonPeer.GetName());
                    Assert.AreEqual(AutomationControlType.Button, buttonPeer.GetAutomationControlType());
                    Assert.IsInstanceOfType(buttonPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
                    var outputPeer = new TextBlockAutomationPeer(output);
                    Assert.AreEqual("Control output", outputPeer.GetName());
                    Assert.AreEqual(AutomationControlType.Text, outputPeer.GetAutomationControlType());

                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
                    Assert.AreEqual("Number of clicks: 1", output.Text);
                    Assert.AreEqual("Number of clicks: 1", AutomationProperties.GetHelpText(output));
                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
                    Assert.AreEqual("Number of clicks: 2", output.Text);
                    Assert.AreEqual("Number of clicks: 2", AutomationProperties.GetHelpText(output));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ToggleButtonSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ToggleButton"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("A simple ToggleButton with text content.", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.IsNotNull(page.Examples[0].OutputContent);
                    Assert.IsNotNull(page.Examples[0].OptionsContent);
                    Assert.AreEqual("<ToggleButton Content=\"ToggleButton\" Click=\"Button_Click\" $(IsEnabled)/>", page.Examples[0].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);

                    var button = (ToggleButton)FindByAutomationId(page, "GallerySample_ToggleButton_ToggleButton");
                    var output = FindNamedDescendant<TextBlock>(page, "Control1Output");
                    var disableToggle = FindNamedDescendant<CheckBox>(page, "DisableToggle1");
                    Assert.IsNotNull(button);
                    Assert.IsNotNull(output);
                    Assert.IsNotNull(disableToggle);
                    Assert.AreSame(output, page.Examples[0].OutputContent);
                    Assert.AreSame(disableToggle, ((StackPanel)page.Examples[0].OptionsContent).Children[0]);

                    Assert.AreEqual("Toggle1", button.Name);
                    Assert.AreEqual("ToggleButton", button.Content);
                    Assert.AreEqual(false, button.IsChecked);
                    Assert.AreEqual("Control1Output", output.Name);
                    Assert.AreEqual(new Thickness(0), output.Margin);
                    Assert.AreEqual("GallerySample_ToggleButton_Output", AutomationProperties.GetAutomationId(output));
                    Assert.AreEqual("Off", output.Text);
                    Assert.AreEqual("Disable ToggleButton", disableToggle.Content);

                    disableToggle.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(button.IsEnabled);
                    disableToggle.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(button.IsEnabled);

                    var buttonPeer = new ToggleButtonAutomationPeer(button);
                    Assert.AreEqual("ToggleButton", buttonPeer.GetName());
                    Assert.AreEqual(AutomationControlType.Button, buttonPeer.GetAutomationControlType());
                    Assert.IsInstanceOfType(buttonPeer.GetPattern(PatternInterface.Toggle), typeof(IToggleProvider));
                    var outputPeer = new TextBlockAutomationPeer(output);
                    Assert.AreEqual("Off", outputPeer.GetName());
                    Assert.AreEqual(AutomationControlType.Text, outputPeer.GetAutomationControlType());

                    button.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("On", output.Text);
                    Assert.AreEqual("On", outputPeer.GetName());

                    button.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Off", output.Text);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ToggleSwitchSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ToggleSwitch"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A simple ToggleSwitch.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A ToggleSwitch with custom header and content.", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.AreEqual("<ToggleSwitch AutomationProperties.Name=\"simple ToggleSwitch\"/>", page.Examples[0].XamlCode);
                    StringAssert.Contains(page.Examples[1].XamlCode, "Header=\"Toggle work\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "OffContent=\"Do work\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "OnContent=\"Working\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ProgressRing");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var simpleToggle = (Mux.ToggleSwitch)FindByAutomationId(page, "GallerySample_ToggleSwitch_ToggleSwitch");
                    var workToggle = (Mux.ToggleSwitch)FindByAutomationId(page, "GallerySample_ToggleSwitch_WorkToggleSwitch");
                    var progressRing = FindNamedDescendant<Mux.ProgressRing>(page, "ToggleSwitchProgressRing");
                    Assert.IsNotNull(simpleToggle);
                    Assert.IsNotNull(workToggle);
                    Assert.IsNotNull(progressRing);

                    Assert.AreEqual("simple ToggleSwitch", AutomationProperties.GetName(simpleToggle));
                    Assert.AreEqual(72.0, simpleToggle.Width);
                    Assert.AreEqual(0.0, simpleToggle.MinWidth);
                    Assert.IsFalse(simpleToggle.IsOn);
                    Assert.AreEqual("Off", simpleToggle.OffContent);
                    Assert.AreEqual("On", simpleToggle.OnContent);
                    Assert.AreSame(
                        DependencyProperty.UnsetValue,
                        simpleToggle.ReadLocalValue(Mux.ToggleSwitch.OffContentProperty));
                    Assert.AreSame(
                        DependencyProperty.UnsetValue,
                        simpleToggle.ReadLocalValue(Mux.ToggleSwitch.OnContentProperty));
                    Assert.AreEqual("ToggleSwitch2", workToggle.Name);
                    Assert.AreEqual("Toggle work", workToggle.Header);
                    Assert.IsTrue(workToggle.IsOn);
                    Assert.AreEqual("Do work", workToggle.OffContent);
                    Assert.AreEqual("Working", workToggle.OnContent);
                    Assert.AreEqual(32.0, progressRing.Width);
                    Assert.IsTrue(progressRing.IsActive);

                    workToggle.IsOn = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(progressRing.IsActive);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void AutoSuggestBoxSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("AutoSuggestBox"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("A basic autosuggest box.", page.Examples[0].HeaderText);
                    Assert.AreEqual("An AutoSuggestBox that provides a SearchBox experience", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "AutomationProperties.Name=\"Basic AutoSuggestBox\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "private List<string> Cats");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "No results found");
                    StringAssert.Contains(page.Examples[1].XamlCode, "PlaceholderText=\"Type a control name\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "QueryIcon=\"Find\"");
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var basicBox = (Mux.AutoSuggestBox)FindByAutomationId(page, "GallerySample_AutoSuggestBox_AutoSuggestBox");
                    var searchBox = (Mux.AutoSuggestBox)FindByAutomationId(page, "GallerySample_AutoSuggestBox_SearchBox");
                    var suggestionOutput = FindNamedDescendant<TextBlock>(page, "SuggestionOutput");
                    var controlDetails = FindNamedDescendant<Grid>(page, "ControlDetails");
                    var controlImage = FindNamedDescendant<Image>(page, "ControlImage");
                    var controlTitle = FindNamedDescendant<TextBlock>(page, "ControlTitle");
                    var controlSubtitle = FindNamedDescendant<TextBlock>(page, "ControlSubtitle");
                    Assert.IsNotNull(basicBox);
                    Assert.IsNotNull(searchBox);
                    Assert.IsNotNull(suggestionOutput);
                    Assert.IsNotNull(controlDetails);
                    Assert.IsNotNull(controlImage);
                    Assert.IsNotNull(controlTitle);
                    Assert.IsNotNull(controlSubtitle);

                    Assert.AreEqual("Control1", basicBox.Name);
                    Assert.AreEqual(300.0, basicBox.Width);
                    Assert.AreEqual("Basic AutoSuggestBox", AutomationProperties.GetName(basicBox));
                    Assert.AreEqual("SuggestionOutput", suggestionOutput.Name);

                    Assert.AreEqual("Control2", searchBox.Name);
                    Assert.AreEqual(300.0, searchBox.Width);
                    Assert.AreEqual(HorizontalAlignment.Left, searchBox.HorizontalAlignment);
                    Assert.AreEqual("Type a control name", searchBox.PlaceholderText);
                    Assert.IsInstanceOfType(searchBox.QueryIcon, typeof(Mux.SymbolIcon));
                    Assert.AreEqual(Mux.Symbol.Find, ((Mux.SymbolIcon)searchBox.QueryIcon).Symbol);
                    Assert.AreEqual(Visibility.Collapsed, controlDetails.Visibility);
                    Assert.AreEqual(75.0, controlImage.Height);
                    Assert.AreEqual(TextWrapping.Wrap, controlSubtitle.TextWrapping);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void NumberBoxSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("NumberBox"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("A NumberBox that evaluates expressions.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A NumberBox with a spin button.", page.Examples[1].HeaderText);
                    Assert.AreEqual("A formatted NumberBox that rounds to the nearest 0.25.", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "AcceptsExpression=\"True\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "NumberBoxSpinButtonPlacementExample");
                    StringAssert.Contains(page.Examples[2].XamlCode, "FormattedNumberBox");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "Increment = 0.25");

                    var expressionBox = (Mux.NumberBox)FindByAutomationId(page, "GallerySample_NumberBox_ExpressionNumberBox");
                    var spinButtonBox = (Mux.NumberBox)FindByAutomationId(page, "GallerySample_NumberBox_SpinButtonNumberBox");
                    var formattedBox = (Mux.NumberBox)FindByAutomationId(page, "GallerySample_NumberBox_FormattedNumberBox");
                    var placementGroup = FindNamedDescendant<Mux.RadioButtons>(page, "SpinButtonPlacementGroup");
                    Assert.IsNotNull(expressionBox);
                    Assert.IsNotNull(spinButtonBox);
                    Assert.IsNotNull(formattedBox);
                    Assert.IsNotNull(placementGroup);

                    Assert.AreEqual("Enter an expression:", expressionBox.Header);
                    Assert.AreEqual(124.0, expressionBox.Width);
                    Assert.AreEqual("1 + 2^2", expressionBox.PlaceholderText);
                    Assert.IsTrue(expressionBox.AcceptsExpression);
                    Assert.IsTrue(double.IsNaN(expressionBox.Value));

                    Assert.AreEqual("NumberBoxSpinButtonPlacementExample", spinButtonBox.Name);
                    Assert.AreEqual(132.0, spinButtonBox.Width);
                    Assert.AreEqual(HorizontalAlignment.Left, spinButtonBox.HorizontalAlignment);
                    Assert.AreEqual("NumberBox with spin button", AutomationProperties.GetName(spinButtonBox));
                    Assert.AreEqual("Enter an integer:", spinButtonBox.Header);
                    Assert.AreEqual(10.0, spinButtonBox.Value);
                    Assert.AreEqual(10.0, spinButtonBox.SmallChange);
                    Assert.AreEqual(100.0, spinButtonBox.LargeChange);
                    Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Inline, spinButtonBox.SpinButtonPlacementMode);
                    Assert.AreEqual("SpinButton placement", placementGroup.Header);
                    Assert.AreEqual(0, placementGroup.SelectedIndex);
                    Assert.AreEqual("Inline", placementGroup.Items[0]);
                    Assert.AreEqual("Compact", placementGroup.Items[1]);
                    placementGroup.SelectedIndex = 1;
                    Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Compact, spinButtonBox.SpinButtonPlacementMode);

                    Assert.AreEqual("FormattedNumberBox", formattedBox.Name);
                    Assert.AreEqual(137.0, formattedBox.Width);
                    Assert.AreEqual("Enter a dollar amount:", formattedBox.Header);
                    Assert.AreEqual("0.00", formattedBox.PlaceholderText);
                    Assert.IsNotNull(formattedBox.NumberFormatter);
                    Assert.AreEqual("1.25", formattedBox.NumberFormatter.FormatDouble(1.13));
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void SplitViewSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("SplitView"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("A basic SplitView.", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    var exampleRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, exampleRoot.Children.Count);
                    Assert.IsInstanceOfType(page.Examples[0].OptionsContent, typeof(StackPanel));
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"splitView\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "PaneBackground=\"$(PaneBackground)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "NavLinksList");

                    var splitView = (Mux.SplitView)FindByAutomationId(page, "GallerySample_SplitView_SplitView");
                    var paneHeader = FindNamedDescendant<TextBlock>(page, "PaneHeader");
                    var navLinksList = FindNamedDescendant<ListView>(page, "NavLinksList");
                    var content = FindNamedDescendant<TextBlock>(page, "content");
                    var togglePaneButton = FindNamedDescendant<ToggleButton>(page, "togglePaneButton");
                    var displayMode = FindNamedDescendant<ComboBox>(page, "displayModeCombobox");
                    var paneBackground = FindNamedDescendant<ComboBox>(page, "paneBackgroundCombobox");
                    var openPaneLength = FindNamedDescendant<Slider>(page, "openPaneLengthSlider");
                    var compactPaneLength = FindNamedDescendant<Slider>(page, "compactPaneLengthSlider");
                    var placement = FindToggleSwitchByHeader(page, "Placement");
                    Assert.IsNotNull(splitView);
                    Assert.IsNotNull(paneHeader);
                    Assert.IsNotNull(navLinksList);
                    Assert.IsNotNull(content);
                    Assert.IsNotNull(togglePaneButton);
                    Assert.IsNotNull(displayMode);
                    Assert.IsNotNull(paneBackground);
                    Assert.IsNotNull(openPaneLength);
                    Assert.IsNotNull(compactPaneLength);
                    Assert.IsNotNull(placement);

                    Assert.AreEqual("splitView", splitView.Name);
                    Assert.IsTrue(splitView.IsPaneOpen);
                    Assert.AreEqual(Mux.SplitViewDisplayMode.Inline, splitView.DisplayMode);
                    Assert.AreEqual(Mux.SplitViewPanePlacement.Left, splitView.PanePlacement);
                    Assert.AreEqual(256d, splitView.OpenPaneLength);
                    Assert.AreEqual(48d, splitView.CompactPaneLength);
                    Assert.AreEqual("PANE CONTENT", paneHeader.Text);
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(navLinksList));
                    Assert.AreEqual(4, navLinksList.Items.Count);
                    Assert.AreEqual("IsPaneOpen", togglePaneButton.Content);
                    Assert.IsTrue(togglePaneButton.IsChecked.GetValueOrDefault());
                    Assert.AreEqual(196d, displayMode.Width);
                    Assert.AreEqual(4d, displayMode.Margin.Top);
                    Assert.AreEqual(4, displayMode.Items.Count);
                    Assert.AreEqual("Inline", displayMode.SelectedItem);
                    Assert.AreEqual(196d, paneBackground.Width);
                    Assert.AreEqual(4, paneBackground.Items.Count);
                    Assert.AreEqual("SystemControlBackgroundChromeMediumLowBrush", paneBackground.SelectedItem);
                    Assert.AreEqual(196d, openPaneLength.Width);
                    Assert.AreEqual(128d, openPaneLength.Minimum);
                    Assert.AreEqual(500d, openPaneLength.Maximum);
                    Assert.IsTrue(openPaneLength.IsSelectionRangeEnabled);
                    Assert.AreEqual(openPaneLength.Minimum, openPaneLength.SelectionStart);
                    Assert.AreEqual(openPaneLength.Value, openPaneLength.SelectionEnd);
                    Assert.AreEqual(196d, compactPaneLength.Width);
                    Assert.AreEqual(24d, compactPaneLength.Minimum);
                    Assert.AreEqual(128d, compactPaneLength.Maximum);
                    Assert.AreEqual(new Thickness(0, 4, 0, 0), ((StackPanel)compactPaneLength.Parent).Margin);
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), placement.Margin);
                    Assert.IsTrue(compactPaneLength.IsSelectionRangeEnabled);
                    Assert.AreEqual(compactPaneLength.Minimum, compactPaneLength.SelectionStart);
                    Assert.AreEqual(compactPaneLength.Value, compactPaneLength.SelectionEnd);

                    navLinksList.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Globe Page", content.Text);

                    placement.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.SplitViewPanePlacement.Right, splitView.PanePlacement);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void PersonPictureSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("PersonPicture"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("Select different looks for the person picture.", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    var exampleRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, exampleRoot.Children.Count);
                    Assert.IsInstanceOfType(page.Examples[0].OptionsContent, typeof(StackPanel));
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"personPicture\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Height=\"300\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "ProfileImageRadio");
                    StringAssert.Contains(page.Examples[0].XamlCode, "$(ProfilePicture)$(DisplayName)$(Initials)");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "personPicture.ProfilePicture");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "DisplayName = \"Jane Doe\"");

                    var personPicture = (Mux.PersonPicture)FindByAutomationId(page, "GallerySample_PersonPicture_PersonPicture");
                    var profileType = FindRadioButtonsByHeader(page, "Profile type");
                    var profileImageRadio = FindNamedDescendant<RadioButton>(page, "ProfileImageRadio");
                    var displayNameRadio = FindNamedDescendant<RadioButton>(page, "DisplayNameRadio");
                    var initialsRadio = FindNamedDescendant<RadioButton>(page, "InitialsRadio");
                    Assert.IsNotNull(personPicture);
                    Assert.IsNotNull(profileType);
                    Assert.IsNotNull(profileImageRadio);
                    Assert.IsNotNull(displayNameRadio);
                    Assert.IsNotNull(initialsRadio);

                    Assert.AreEqual("personPicture", personPicture.Name);
                    Assert.AreEqual(96d, personPicture.Width);
                    Assert.AreEqual(96d, personPicture.Height);
                    Assert.AreEqual(96d, personPicture.ActualWidth);
                    Assert.AreEqual(96d, personPicture.ActualHeight);
                    Assert.AreEqual(HorizontalAlignment.Left, personPicture.HorizontalAlignment);
                    Assert.AreEqual(0, profileType.SelectedIndex);
                    Assert.AreEqual("Profile Image", profileImageRadio.Content);
                    Assert.AreEqual("Display Name", displayNameRadio.Content);
                    Assert.AreEqual("Initials", initialsRadio.Content);
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(profileImageRadio));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(displayNameRadio));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(initialsRadio));
                    Assert.IsNotNull(personPicture.ProfilePicture);
                    var profileBitmap = personPicture.ProfilePicture as BitmapImage;
                    Assert.IsNotNull(profileBitmap);
                    Assert.AreEqual(
                        "pack://application:,,,/ModernWpf.Gallery;component/Assets/SampleMedia/shoulder-tap-static-payload.png",
                        profileBitmap.UriSource.AbsoluteUri);
                    Assert.AreEqual(string.Empty, personPicture.DisplayName);
                    Assert.AreEqual(string.Empty, personPicture.Initials);

                    profileType.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.IsNull(personPicture.ProfilePicture);
                    Assert.AreEqual("Jane Doe", personPicture.DisplayName);
                    Assert.AreEqual(string.Empty, personPicture.Initials);

                    profileType.SelectedIndex = 2;
                    WpfTestHost.DoEvents();
                    Assert.IsNull(personPicture.ProfilePicture);
                    Assert.AreEqual(string.Empty, personPicture.DisplayName);
                    Assert.AreEqual("SB", personPicture.Initials);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void IconElementSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("IconElement"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(6, page.Examples.Count);
                    Assert.AreEqual("A BitmapIcon with a multicolor bitmap image", page.Examples[0].HeaderText);
                    Assert.AreEqual("A FontIcon using a glyph from a specific font family in a button", page.Examples[1].HeaderText);
                    Assert.AreEqual("A ImageIcon using a bitmap image in a button", page.Examples[2].HeaderText);
                    Assert.AreEqual("A ImageIcon using a SVG image in a button", page.Examples[3].HeaderText);
                    Assert.AreEqual("A PathIcon in a button", page.Examples[4].HeaderText);
                    Assert.AreEqual("A SymbolIcon in a button", page.Examples[5].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);

                    StringAssert.Contains(page.Examples[0].XamlCode, "<BitmapIcon x:Name=\"SlicesIcon\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "ShowAsMonochrome=\"$(ShowAsMonochrome)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<FontIcon FontFamily=\"Segoe MDL2 Assets\" Glyph=\"&#xE790;\"/>");
                    StringAssert.Contains(page.Examples[2].XamlCode, "<ImageIcon Source=\"/Assets/SampleMedia/slices.png\"/>");
                    StringAssert.Contains(page.Examples[3].XamlCode, "libre-camera-panorama.svg");
                    StringAssert.Contains(page.Examples[4].XamlCode, "PathIcon Data=\"F1 M 16,12 20,2L 20,16 1,16\"");
                    StringAssert.Contains(page.Examples[5].XamlCode, "<SymbolIcon Symbol=\"Accept\"/>");

                    var bitmapRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, bitmapRoot.Children.Count);
                    Assert.IsInstanceOfType(bitmapRoot.Children[0], typeof(StackPanel));
                    var bitmapExampleStack = (StackPanel)bitmapRoot.Children[0];
                    var bitmapDescription = (TextBlock)bitmapExampleStack.Children[0];
                    Assert.AreSame(bitmapDescription.FindResource("BodyTextBlockStyle"), bitmapDescription.Style);
                    var bitmapOptions = (CheckBox)page.Examples[0].OptionsContent;
                    Assert.IsNotNull(bitmapOptions);

                    var slicesIcon = (Mux.BitmapIcon)FindByAutomationId(page, "GallerySample_IconElement_SlicesIcon");
                    Assert.IsNotNull(slicesIcon);
                    Assert.AreSame(slicesIcon, FindNamedDescendant<Mux.BitmapIcon>(page, "SlicesIcon"));
                    Assert.AreEqual(50d, slicesIcon.Width);
                    Assert.AreEqual(HorizontalAlignment.Left, slicesIcon.HorizontalAlignment);
                    Assert.IsFalse(slicesIcon.ShowAsMonochrome);
                    StringAssert.Contains(slicesIcon.UriSource.ToString(), "Assets/SampleMedia/Slices.png");

                    var monochromeButton = FindNamedDescendant<CheckBox>(page, "MonochromeButton");
                    Assert.IsNotNull(monochromeButton);
                    Assert.AreSame(bitmapOptions, monochromeButton);
                    Assert.AreEqual("Monochrome", monochromeButton.Content);
                    Assert.AreEqual(
                        "GallerySample_IconElement_MonochromeButton",
                        AutomationProperties.GetAutomationId(monochromeButton));
                    Assert.AreEqual(false, monochromeButton.IsChecked);
                    monochromeButton.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(slicesIcon.ShowAsMonochrome);
                    monochromeButton.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(slicesIcon.ShowAsMonochrome);

                    var fontButton = (Button)FindByAutomationId(page, "GallerySample_IconElement_ExampleButton1");
                    Assert.IsNotNull(fontButton);
                    Assert.AreSame(fontButton, FindNamedDescendant<Button>(page, "ExampleButton1"));
                    Assert.AreEqual("ExampleButton1", AutomationProperties.GetName(fontButton));
                    var fontIcon = (Mux.FontIcon)fontButton.Content;
                    Assert.AreEqual("Segoe MDL2 Assets", fontIcon.FontFamily.Source);
                    Assert.AreEqual("\uE790", fontIcon.Glyph);

                    var imageButton = (Button)FindByAutomationId(page, "GallerySample_IconElement_ImageExample1");
                    Assert.IsNotNull(imageButton);
                    Assert.AreEqual(100d, imageButton.Width);
                    Assert.AreEqual("ImageExample1", AutomationProperties.GetName(imageButton));
                    var imageIcon = (Mux.ImageIcon)imageButton.Content;
                    StringAssert.Contains(((BitmapImage)imageIcon.Source).UriSource.ToString(), "Assets/SampleMedia/Slices.png");

                    var svgButton = (Button)FindByAutomationId(page, "GallerySample_IconElement_ImageExample2");
                    Assert.IsNotNull(svgButton);
                    Assert.AreEqual("ImageExample2", AutomationProperties.GetName(svgButton));
                    var svgIcon = (Mux.ImageIcon)svgButton.Content;
                    Assert.AreEqual(50d, svgIcon.Width);
                    Assert.IsInstanceOfType(svgIcon.Source, typeof(DrawingImage));
                    var svgDrawing = (DrawingGroup)((DrawingImage)svgIcon.Source).Drawing;
                    Assert.AreEqual(2, svgDrawing.Children.Count);
                    var svgContent = (DrawingGroup)svgDrawing.Children[1];
                    Assert.AreEqual(new Matrix(0.3, 0, 0, 0.3, -0.2, -0.5), ((MatrixTransform)svgContent.Transform).Matrix);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            Color.FromRgb(0xF5, 0x7C, 0x00),
                            Color.FromRgb(0x94, 0x2A, 0x09),
                            Color.FromRgb(0xBF, 0x36, 0x0C),
                            Color.FromRgb(0xE6, 0x51, 0x00),
                            Color.FromRgb(0xFF, 0xF9, 0xC4)
                        },
                        svgContent.Children
                            .OfType<GeometryDrawing>()
                            .Select(child => ((SolidColorBrush)child.Brush).Color)
                            .ToArray());

                    var pathButton = (Button)FindByAutomationId(page, "GallerySample_IconElement_Example1Button");
                    Assert.IsNotNull(pathButton);
                    Assert.AreEqual("Example1Button", AutomationProperties.GetName(pathButton));
                    var pathIcon = (Mux.PathIcon)pathButton.Content;
                    Assert.AreEqual(HorizontalAlignment.Center, pathIcon.HorizontalAlignment);
                    StringAssert.Contains(pathIcon.Data.ToString(CultureInfo.InvariantCulture), "M16,12L20,2");

                    var acceptButton = (Button)FindByAutomationId(page, "GallerySample_IconElement_AcceptButton");
                    Assert.IsNotNull(acceptButton);
                    Assert.AreEqual("AcceptButton", AutomationProperties.GetName(acceptButton));
                    var acceptStack = (StackPanel)acceptButton.Content;
                    var symbolIcon = (Mux.SymbolIcon)acceptStack.Children[0];
                    Assert.AreEqual(Mux.Symbol.Accept, symbolIcon.Symbol);
                    Assert.AreEqual("Accept", ((TextBlock)acceptStack.Children[1]).Text);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ThemeShadowSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ThemeShadow"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("ThemeShadow applied to a Border", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"ShadowCastGrid\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"ShadowRect\" Translation=\"0,0,$(TranslationSlider)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<ThemeShadow x:Name=\"shadow\"/>");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "shadow.Receivers.Add(ShadowCastGrid);");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    Assert.IsInstanceOfType(root.Children[0], typeof(Mux.GridEx));
                    var optionsPanel = (StackPanel)page.Examples[0].OptionsContent;
                    Assert.AreEqual(2, optionsPanel.Children.Count);
                    var sliderHeader = (TextBlock)optionsPanel.Children[0];
                    Assert.AreEqual("Z-translation", sliderHeader.Text);
                    Assert.AreEqual(new Thickness(0, 0, 0, 10), sliderHeader.Margin);

                    var exampleGrid = FindNamedDescendant<Mux.GridEx>(page, "Example3Grid");
                    Assert.IsNotNull(exampleGrid);
                    Assert.AreSame(exampleGrid, FindByAutomationId(page, "GallerySample_ThemeShadow_Example3Grid"));
                    Assert.AreEqual(new Thickness(36), exampleGrid.Padding);
                    Assert.AreEqual(272d, exampleGrid.MinWidth);
                    Assert.AreEqual(272d, exampleGrid.MinHeight);

                    var shadowCastGrid = FindNamedDescendant<Grid>(page, "ShadowCastGrid");
                    Assert.IsNotNull(shadowCastGrid);
                    Assert.AreSame(shadowCastGrid, exampleGrid.Children[0]);
                    Assert.AreSame(shadowCastGrid, FindByAutomationId(page, "GallerySample_ThemeShadow_ShadowCastGrid"));
                    Assert.AreEqual(new Thickness(), shadowCastGrid.Margin);

                    var shadow = FindNamedDescendant<ThemeShadowChrome>(page, "shadow");
                    Assert.IsNotNull(shadow);
                    Assert.AreSame(shadow, FindByAutomationId(page, "GallerySample_ThemeShadow_ShadowChrome"));
                    Assert.AreEqual(32d, shadow.Depth);
                    Assert.AreEqual(32d, shadow.TranslationZ);
                    Assert.AreEqual(new Thickness(), shadow.Margin);
                    Assert.IsFalse(shadow.ReservesShadowSpace);
                    Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Default, shadow.WindowedPopupInsetMode);
                    Assert.AreEqual(HorizontalAlignment.Left, shadow.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Top, shadow.VerticalAlignment);

                    var shadowRect = (Border)FindByAutomationId(page, "GallerySample_ThemeShadow_ShadowRect");
                    Assert.IsNotNull(shadowRect);
                    Assert.AreSame(shadowRect, shadow.Child);
                    Assert.AreEqual("ShadowRect", shadowRect.Name);
                    Assert.AreEqual(200d, shadowRect.Width);
                    Assert.AreEqual(200d, shadowRect.Height);
                    Assert.IsNotNull(shadowRect.Background);

                    var slider = FindNamedDescendant<Slider>(page, "TranslationSliderInApp");
                    Assert.IsNotNull(slider);
                    Assert.AreEqual("shadow intensity", AutomationProperties.GetName(slider));
                    Assert.AreEqual("Z-translation", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(slider));
                    Assert.AreEqual(200d, slider.Width);
                    Assert.AreEqual(0d, slider.Minimum);
                    Assert.AreEqual(64d, slider.Maximum);
                    Assert.AreEqual(1d, slider.SmallChange);
                    Assert.AreEqual(1d, slider.TickFrequency);
                    Assert.AreEqual(32d, slider.Value);
                    Assert.IsTrue(slider.IsSelectionRangeEnabled);
                    Assert.AreEqual(0d, slider.SelectionStart);
                    Assert.AreEqual(32d, slider.SelectionEnd);
                    var selectionRange = FindNamedDescendant<Border>(slider, "PART_SelectionRange");
                    Assert.IsNotNull(selectionRange);
                    Assert.AreEqual(Visibility.Visible, selectionRange.Visibility);
                    var sliderTrack = FindNamedDescendant<Track>(slider, "PART_Track");
                    var selectionRangeCanvas = (Canvas)selectionRange.Parent;
                    Assert.IsTrue(
                        selectionRange.ActualWidth > 80d,
                        "ThemeShadow's value fill must be visibly rendered; range actual/declared width " +
                        selectionRange.ActualWidth + "/" + selectionRange.Width +
                        ", left " + Canvas.GetLeft(selectionRange) +
                        ", parent " + selectionRange.Parent.GetType().FullName +
                        ", canvas " + selectionRangeCanvas.ActualWidth + "x" + selectionRangeCanvas.ActualHeight +
                        ", track/thumb " + sliderTrack.ActualWidth + "/" + sliderTrack.Thumb.ActualWidth +
                        ", start/end " + slider.SelectionStart + "/" + slider.SelectionEnd);

                    var beforeRootBounds = GetRelativeBounds(root, page);
                    var beforeGridBounds = GetRelativeBounds(exampleGrid, page);
                    var beforeReceiverBounds = GetRelativeBounds(shadowCastGrid, exampleGrid);
                    var beforeShadowBounds = GetRelativeBounds(shadow, exampleGrid);
                    var beforeRectBounds = GetRelativeBounds(shadowRect, exampleGrid);
                    var beforeSliderBounds = GetRelativeBounds(slider, page);
                    var beforeRenderedSample = RenderElementBitmap(root);
                    var beforeRenderedCardBounds = MeasureRenderedColorBounds(beforeRenderedSample, color => color.R > 245 && color.G > 245 && color.B > 245);
                    AssertRectNear(
                        new Rect(36, 36, Math.Max(0, exampleGrid.ActualWidth - 72), Math.Max(0, exampleGrid.ActualHeight - 72)),
                        beforeReceiverBounds,
                        0.5,
                        "ThemeShadow receiver should preserve the WinUI sample's 36px padded content layout.");
                    AssertRectNear(new Rect(36, 36, 200, 200), beforeShadowBounds, 0.5, "ThemeShadow chrome should preserve the WinUI sample's 36px caster layout.");
                    AssertRectNear(new Rect(36, 36, 200, 200), beforeRectBounds, 0.5, "ThemeShadow caster should preserve the WinUI sample's 36px layout.");

                    var maxRenderedDelta = 0d;
                    foreach (var depth in new[] { 0d, 16d, 32d, 48d, 64d })
                    {
                        slider.Value = depth;
                        WpfTestHost.DoEvents();
                        window.UpdateLayout();
                        WpfTestHost.DoEvents();
                        WaitForRendering();

                        var afterRootBounds = GetRelativeBounds(root, page);
                        var afterGridBounds = GetRelativeBounds(exampleGrid, page);
                        var afterReceiverBounds = GetRelativeBounds(shadowCastGrid, exampleGrid);
                        var afterShadowBounds = GetRelativeBounds(shadow, exampleGrid);
                        var afterRectBounds = GetRelativeBounds(shadowRect, exampleGrid);
                        var afterSliderBounds = GetRelativeBounds(slider, page);
                        var afterRenderedSample = RenderElementBitmap(root);
                        var afterRenderedCardBounds = MeasureRenderedColorBounds(afterRenderedSample, color => color.R > 245 && color.G > 245 && color.B > 245);
                        maxRenderedDelta = Math.Max(maxRenderedDelta, CompareRenderedMeanDelta(beforeRenderedSample, afterRenderedSample));

                        Assert.AreEqual(depth, shadow.Depth);
                        Assert.AreEqual(depth, shadow.TranslationZ);
                        AssertRectNear(beforeRootBounds, afterRootBounds, 0.5, $"Changing ThemeShadow depth to {depth} should not move the sample root.");
                        AssertRectNear(beforeGridBounds, afterGridBounds, 0.5, $"Changing ThemeShadow depth to {depth} should not move the example grid.");
                        AssertRectNear(beforeReceiverBounds, afterReceiverBounds, 0.5, $"Changing ThemeShadow depth to {depth} should not move the receiver grid.");
                        AssertRectNear(beforeShadowBounds, afterShadowBounds, 0.5, $"Changing ThemeShadow depth to {depth} should not move the shadow chrome.");
                        AssertRectNear(beforeRectBounds, afterRectBounds, 0.5, $"Changing ThemeShadow depth to {depth} should not move the sample card.");
                        AssertRectNear(beforeSliderBounds, afterSliderBounds, 0.5, $"Changing ThemeShadow depth to {depth} should not move the options slider.");
                        Assert.AreEqual(beforeRenderedCardBounds, afterRenderedCardBounds, $"Changing ThemeShadow depth to {depth} should not move the rendered card pixels.");
                    }

                    Assert.IsTrue(maxRenderedDelta > 0.1, $"Changing ThemeShadow depth should visibly redraw the sample shadow. Delta={maxRenderedDelta}.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void TitleBarSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("TitleBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };
                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.IsTrue(page.ShowIntroContent);
                    var intro = page.IntroContent as TextBlock;
                    Assert.IsNotNull(intro);
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), intro.Margin);
                    Assert.AreEqual(TextWrapping.Wrap, intro.TextWrapping);
                    Assert.AreEqual(
                        "Use the examples below to configure a ModernWPF TitleBar or integrate one with NavigationView.",
                        new TextRange(intro.ContentStart, intro.ContentEnd).Text.Trim());

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("TitleBar configuration", page.Examples[0].HeaderText);
                    Assert.AreEqual("End to end TitleBar sample", page.Examples[1].HeaderText);
                    Assert.AreEqual(HorizontalAlignment.Stretch, page.Examples[0].HorizontalContentAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, page.Examples[0].VerticalContentAlignment);
                    Assert.AreEqual(HorizontalAlignment.Stretch, page.Examples[1].HorizontalContentAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, page.Examples[1].VerticalContentAlignment);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<TitleBar");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Title=\"$(Title)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Subtitle=\"$(Subtitle)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsBackButtonVisible=\"$(BackButtonVisibility)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsPaneToggleButtonVisible=\"$(PaneToggleVisibility)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<SymbolIconSource Symbol=\"Library\" />");
                    Assert.IsFalse(page.Examples[0].XamlCode.Contains("TitleBarContentHorizontalAlignment"));
                    StringAssert.Contains(page.Examples[0].XamlCode, "Width=\"360\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "PlaceholderText=\"Search..\"");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    StringAssert.Contains(page.Examples[1].XamlCode, "x:Name=\"titleBar\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "PaneToggleRequested=\"TitleBar_PaneToggleRequested\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<NavigationView");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "this.ExtendsContentIntoTitleBar = true;");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "this.SetTitleBar(titleBar);");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    Assert.AreEqual(VerticalAlignment.Center, root.VerticalAlignment);
                    var titleBarControl = (ContentControl)FindByAutomationId(page, "GallerySample_TitleBar_TitleBarControl");
                    Assert.IsNotNull(titleBarControl);
                    Assert.AreEqual("TitleBarControl", titleBarControl.Name);
                    Assert.AreEqual(470d, titleBarControl.Width);
                    Assert.AreEqual(48d, titleBarControl.Height);
                    Assert.AreEqual("TitleBarControl", AutomationProperties.GetName(titleBarControl));
                    var configurationCard = FindDescendants<ControlExample>(page)
                        .Single(example => example.HeaderText == "TitleBar configuration");
                    Assert.IsTrue(configurationCard.UseWinUIGalleryLayout);
                    var configurationDisplay = (Border)configurationCard.Template.FindName("ExampleDisplayBorder", configurationCard);
                    var titleBarBoundsInDisplay = GetRelativeBounds(titleBarControl, configurationDisplay);
                    Assert.AreEqual(
                        configurationDisplay.ActualHeight / 2d,
                        titleBarBoundsInDisplay.Top + titleBarBoundsInDisplay.Height / 2d,
                        1d,
                        "The TitleBar preview must remain vertically centered beside its taller options panel.");
                    var titleBarSurface = FindNamedDescendant<Border>(titleBarControl, "TitleBarSurface");
                    Assert.IsNotNull(titleBarSurface);
                    Assert.AreEqual(new Thickness(-1), titleBarSurface.Margin);
                    Assert.AreEqual(new Thickness(1), titleBarSurface.BorderThickness);
                    Assert.AreEqual(new CornerRadius(4), titleBarSurface.CornerRadius);
                    var titleBarIcon = FindNamedDescendant<Mux.SymbolIcon>(titleBarControl, "TitleBarIcon");
                    Assert.IsNotNull(titleBarIcon);
                    Assert.AreEqual(16d, titleBarIcon.Width);
                    Assert.AreEqual(16d, titleBarIcon.Height);
                    Assert.AreEqual(new Thickness(14, 0, 16, 0), titleBarIcon.Margin);
                    Assert.AreEqual(Mux.Symbol.Library, titleBarIcon.Symbol);
                    var titleBarSearchBox = (Mux.AutoSuggestBox)FindByAutomationId(page, "GallerySample_TitleBar_SearchBox");
                    Assert.IsNotNull(titleBarSearchBox);
                    Assert.AreEqual(186d, titleBarSearchBox.Width);
                    Assert.AreEqual(new Thickness(0, 0, 16, 0), titleBarSearchBox.Margin);
                    Assert.AreEqual("Search..", titleBarSearchBox.PlaceholderText);
                    var rightHeader = FindNamedDescendant<Mux.PersonPicture>(titleBarControl, "TitleBarRightHeader");
                    Assert.IsNotNull(rightHeader);
                    Assert.AreEqual(30d, rightHeader.Width);
                    Assert.AreEqual(30d, rightHeader.Height);
                    Assert.AreEqual(new Thickness(0, 0, 16, 0), rightHeader.Margin);

                    var titleBox = FindNamedDescendant<TextBox>(page, "TitleBox");
                    Assert.IsNotNull(titleBox);
                    Assert.AreEqual("Title", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(titleBox));
                    Assert.AreEqual(GalleryBranding.DisplayName, titleBox.Text);
                    var titleHeader = FindNamedDescendant<TextBlock>(page, "TitleHeader");
                    Assert.IsNotNull(titleHeader);
                    Assert.AreEqual("Title", titleHeader.Text);
                    Assert.AreEqual(new Thickness(0, 0, 0, 8), titleHeader.Margin);
                    Assert.IsTrue(titleHeader.ActualWidth > 0);
                    Assert.IsTrue(titleHeader.ActualHeight > 0);
                    var subtitleBox = FindNamedDescendant<TextBox>(page, "SubtitleBox");
                    Assert.IsNotNull(subtitleBox);
                    Assert.AreEqual("Subtitle", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(subtitleBox));
                    Assert.AreEqual("Preview", subtitleBox.Text);
                    Assert.AreEqual(new Thickness(), subtitleBox.Margin);
                    var subtitleHeader = FindNamedDescendant<TextBlock>(page, "SubtitleHeader");
                    Assert.IsNotNull(subtitleHeader);
                    Assert.AreEqual("Subtitle", subtitleHeader.Text);
                    Assert.AreEqual(new Thickness(0, 12, 0, 8), subtitleHeader.Margin);
                    Assert.IsTrue(subtitleHeader.ActualWidth > 0);
                    Assert.IsTrue(subtitleHeader.ActualHeight > 0);
                    var backButtonToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "BackButtonToggle");
                    Assert.IsNotNull(backButtonToggle);
                    Assert.AreEqual("IsBackButtonVisible", backButtonToggle.Header);
                    Assert.IsFalse(backButtonToggle.IsOn);
                    var paneToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "PaneToggle");
                    Assert.IsNotNull(paneToggle);
                    Assert.AreEqual("IsPaneToggleButtonVisible", paneToggle.Header);
                    Assert.IsFalse(paneToggle.IsOn);
                    Assert.AreEqual(280d, ((FrameworkElement)titleHeader.Parent).ActualHeight, 0.01);
                    var previewBackButton = (Button)FindByAutomationId(page, "GallerySample_TitleBar_BackButton");
                    Assert.IsNotNull(previewBackButton);
                    Assert.AreEqual(Visibility.Collapsed, previewBackButton.Visibility);
                    var previewPaneButton = (Button)FindByAutomationId(page, "GallerySample_TitleBar_PaneToggleButton");
                    Assert.IsNotNull(previewPaneButton);
                    Assert.AreEqual(Visibility.Collapsed, previewPaneButton.Visibility);

                    var titleText = FindNamedDescendant<TextBlock>(page, "TitleText");
                    var subtitleText = FindNamedDescendant<TextBlock>(page, "SubtitleText");
                    Assert.IsNotNull(titleText);
                    Assert.IsNotNull(subtitleText);
                    Assert.AreEqual(GalleryBranding.DisplayName, titleText.Text);
                    Assert.AreEqual("Preview", subtitleText.Text);
                    backButtonToggle.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Visible, previewBackButton.Visibility);

                    var endToEndRoot = (GallerySamplePanel)page.Examples[1].ExampleContent;
                    Assert.AreEqual(1, endToEndRoot.Children.Count);
                    var endToEndStack = (StackPanel)endToEndRoot.Children[0];
                    Assert.AreEqual(560d, endToEndStack.MaxWidth);
                    Assert.AreEqual(HorizontalAlignment.Center, endToEndStack.HorizontalAlignment);
                    var endToEndCard = FindDescendants<ControlExample>(page)
                        .Single(example => example.HeaderText == "End to end TitleBar sample");
                    var endToEndDisplay = (Border)endToEndCard.Template.FindName("ExampleDisplayBorder", endToEndCard);
                    var endToEndBoundsInDisplay = GetRelativeBounds(endToEndStack, endToEndDisplay);
                    Assert.AreEqual(
                        endToEndDisplay.ActualWidth / 2d,
                        endToEndBoundsInDisplay.Left + endToEndBoundsInDisplay.Width / 2d,
                        1d,
                        "The end-to-end TitleBar content must remain horizontally centered like the WinUI Gallery sample.");
                    var showWindowButton = FindButtonByContent(endToEndRoot, "Show window");
                    Assert.IsNotNull(showWindowButton);
                    Assert.AreSame(showWindowButton.TryFindResource("AccentButtonStyle"), showWindowButton.Style);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void CommandBarSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("CommandBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("A command bar with labels on the side free floating in a page", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.AreEqual("CommandBarLabelsSide.txt", page.SampleSnippets[0].Title);
                    Assert.AreEqual(page.SampleSnippets[0].Text, page.Examples[0].XamlCode);
                    StringAssert.Contains(page.Examples[0].XamlCode, "DefaultLabelPosition=\"Right\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "MultipleButtonsSecondaryCommands");

                    var commandBar = (Mux.CommandBar)FindByAutomationId(page, "GallerySample_CommandBar_CommandBar");
                    Assert.IsNotNull(commandBar);
                    Assert.AreEqual(Mux.CommandBarDefaultLabelPosition.Right, commandBar.DefaultLabelPosition);
                    Assert.AreEqual(HorizontalAlignment.Left, commandBar.HorizontalAlignment);
                    Assert.IsFalse(commandBar.IsOpen);
                    Assert.IsFalse(commandBar.IsSticky);
                    Assert.AreEqual(3, commandBar.PrimaryCommands.Count);
                    Assert.AreEqual(1, commandBar.SecondaryCommands.Count);

                    var addButton = (Mux.AppBarButton)commandBar.PrimaryCommands[0];
                    var editButton = (Mux.AppBarButton)commandBar.PrimaryCommands[1];
                    var shareButton = (Mux.AppBarButton)commandBar.PrimaryCommands[2];
                    var settingsButton = (Mux.AppBarButton)commandBar.SecondaryCommands[0];
                    Assert.AreEqual("Add", addButton.Label);
                    Assert.AreEqual("Edit", editButton.Label);
                    Assert.AreEqual("Share", shareButton.Label);
                    Assert.AreEqual("Settings", settingsButton.Label);
                    Assert.AreEqual("Ctrl+A", addButton.InputGestureText);
                    Assert.AreEqual("Ctrl+E", editButton.InputGestureText);
                    Assert.AreEqual("F4", shareButton.InputGestureText);
                    Assert.AreEqual("Ctrl+I", settingsButton.InputGestureText);

                    var addPeer = new AppBarButtonAutomationPeer(addButton);
                    Assert.AreEqual(AutomationControlType.Button, addPeer.GetAutomationControlType());
                    Assert.AreEqual("Add", addPeer.GetName());
                    Assert.AreEqual("AppBarButton", addPeer.GetClassName());
                    Assert.AreEqual("Ctrl+A", addPeer.GetAcceleratorKey());
                    Assert.IsNotNull(addPeer.GetPattern(PatternInterface.Invoke));

                    var selectedOptionText = FindNamedDescendant<TextBlock>(page, "SelectedOptionText");
                    Assert.IsNotNull(selectedOptionText);
                    addButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("You clicked: Add", selectedOptionText.Text);
                    var outputPeer = new TextBlockAutomationPeer(selectedOptionText);
                    Assert.AreEqual("You clicked: Add", outputPeer.GetName());

                    var openButton = FindButtonByContent(page, "Open command bar");
                    var closeButton = FindButtonByContent(page, "Close command bar");
                    var addSecondaryCommandsButton = FindButtonByContent(page, "Add secondary commands");
                    var removeSecondaryCommandsButton = FindButtonByContent(page, "Remove secondary commands");
                    Assert.IsNotNull(openButton);
                    Assert.IsNotNull(closeButton);
                    Assert.IsNotNull(addSecondaryCommandsButton);
                    Assert.IsNotNull(removeSecondaryCommandsButton);
                    var commandBarOptions = removeSecondaryCommandsButton.Parent as StackPanel;
                    Assert.IsNotNull(commandBarOptions);
                    Assert.IsTrue(double.IsNaN(commandBarOptions.Width));
                    Assert.IsTrue(removeSecondaryCommandsButton.ActualWidth >= removeSecondaryCommandsButton.DesiredSize.Width - 0.5);

                    openButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.IsTrue(commandBar.IsOpen);
                    Assert.IsTrue(commandBar.IsSticky);
                    var settingsPeer = new AppBarButtonAutomationPeer(settingsButton);
                    Assert.AreEqual(AutomationControlType.Button, settingsPeer.GetAutomationControlType());
                    Assert.AreEqual("Settings", settingsPeer.GetName());
                    Assert.AreEqual("Ctrl+I", settingsPeer.GetAcceleratorKey());
                    Assert.IsNotNull(settingsPeer.GetPattern(PatternInterface.Invoke));
                    closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.IsFalse(commandBar.IsOpen);
                    Assert.IsFalse(commandBar.IsSticky);

                    addSecondaryCommandsButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual(6, commandBar.SecondaryCommands.Count);
                    Assert.AreEqual("Button 1", ((Mux.AppBarButton)commandBar.SecondaryCommands[1]).Label);
                    Assert.AreEqual("Ctrl+N", ((Mux.AppBarButton)commandBar.SecondaryCommands[1]).InputGestureText);
                    Assert.IsInstanceOfType(commandBar.SecondaryCommands[3], typeof(Mux.AppBarSeparator));
                    Assert.AreEqual("Button 4", ((Mux.AppBarButton)commandBar.SecondaryCommands[5]).Label);
                    Assert.AreEqual("Ctrl+Subtract", ((Mux.AppBarButton)commandBar.SecondaryCommands[4]).InputGestureText);
                    Assert.AreEqual("Ctrl+Add", ((Mux.AppBarButton)commandBar.SecondaryCommands[5]).InputGestureText);

                    selectedOptionText.Text = "unchanged";
                    ((Mux.AppBarButton)commandBar.SecondaryCommands[1]).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("unchanged", selectedOptionText.Text);

                    removeSecondaryCommandsButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual(1, commandBar.SecondaryCommands.Count);
                    Assert.AreSame(settingsButton, commandBar.SecondaryCommands[0]);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void CommandBarFlyoutSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("CommandBarFlyout"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("CommandBarFlyout for commands on an in-app object", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "CommandBarFlyout1");
                    StringAssert.Contains(page.Examples[0].XamlCode, "AutomationProperties.Name=\"mountain\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "FlyoutShowMode.Transient");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "ShowMenu(false)");

                    var root = (FrameworkElement)FindByAutomationId(page, "GallerySample_CommandBarFlyout_Root");
                    var button = (Button)FindByAutomationId(page, "GallerySample_CommandBarFlyout_ShowButton");
                    var image = FindNamedDescendant<Image>(page, "Image1");
                    var selectedOptionText = FindNamedDescendant<TextBlock>(page, "SelectedOptionText");
                    Assert.IsNotNull(root);
                    Assert.IsNotNull(button);
                    Assert.IsNotNull(image);
                    Assert.IsNotNull(selectedOptionText);
                    Assert.AreEqual("mountain", AutomationProperties.GetName(button));
                    Assert.AreEqual(new Thickness(0), button.Padding);
                    Assert.AreEqual(new Thickness(0, 12, 0, 12), button.Margin);
                    Assert.AreEqual(300.0, image.Height);
                    Assert.AreEqual(BitmapScalingMode.HighQuality, RenderOptions.GetBitmapScalingMode(image));
                    StringAssert.Contains(((BitmapImage)image.Source).UriSource.ToString(), "Assets/SampleMedia/rainier.jpg");

                    var flyout = (Mux.CommandBarFlyout)root.Resources["CommandBarFlyout1"];
                    Assert.IsNotNull(flyout);
                    Assert.AreEqual(ModernWpf.Controls.Primitives.FlyoutPlacementMode.Right, flyout.Placement);
                    Assert.AreEqual(3, flyout.PrimaryCommands.Count);
                    Assert.AreEqual(2, flyout.SecondaryCommands.Count);

                    var shareButton = (Mux.AppBarButton)flyout.PrimaryCommands[0];
                    var saveButton = (Mux.AppBarButton)flyout.PrimaryCommands[1];
                    var deleteButton = (Mux.AppBarButton)flyout.PrimaryCommands[2];
                    var resizeButton = (Mux.AppBarButton)flyout.SecondaryCommands[0];
                    var moveButton = (Mux.AppBarButton)flyout.SecondaryCommands[1];
                    Assert.AreEqual("Share", shareButton.Label);
                    Assert.AreEqual("Save", saveButton.Label);
                    Assert.AreEqual("Delete", deleteButton.Label);
                    Assert.AreEqual("Resize", resizeButton.Label);
                    Assert.AreEqual("Move", moveButton.Label);
                    Assert.AreEqual(Mux.Symbol.Share, ((Mux.SymbolIcon)shareButton.Icon).Symbol);
                    Assert.AreEqual(Mux.Symbol.Save, ((Mux.SymbolIcon)saveButton.Icon).Symbol);
                    Assert.AreEqual(Mux.Symbol.Delete, ((Mux.SymbolIcon)deleteButton.Icon).Symbol);
                    Assert.AreEqual("Share", shareButton.ToolTip);
                    Assert.AreEqual("Save", saveButton.ToolTip);
                    Assert.AreEqual("Delete", deleteButton.ToolTip);

                    shareButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("You clicked: Share", selectedOptionText.Text);
                    resizeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("You clicked: Resize", selectedOptionText.Text);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void InfoBarSampleWritesRenderedVisualArtifacts()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var page = new ItemPage(GalleryCatalog.FindItem("InfoBar"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(page);

                    var infoBarArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_InfoBar.png");
                    var infoBarBoundsArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_InfoBar.bounds.txt");
                    var rootArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_Root.png");
                    var rootBoundsArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_Root.bounds.txt");
                    Assert.IsTrue(File.Exists(infoBarArtifact), infoBarArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(infoBarBoundsArtifact), infoBarBoundsArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(rootArtifact), rootArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(rootBoundsArtifact), rootBoundsArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(infoBarArtifact).Length > 0);
                    Assert.IsTrue(new FileInfo(infoBarBoundsArtifact).Length > 0);
                    Assert.IsTrue(new FileInfo(rootArtifact).Length > 0);
                    Assert.IsTrue(new FileInfo(rootBoundsArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(infoBarArtifact), infoBarArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbPixels(rootArtifact), rootArtifact + " has no visible RGB content.");

                    var infoBarBounds = File.ReadAllText(infoBarBoundsArtifact).Split(',');
                    Assert.AreEqual(4, infoBarBounds.Length);
                    Assert.IsTrue(double.Parse(infoBarBounds[2], CultureInfo.InvariantCulture) > 0);
                    Assert.IsTrue(double.Parse(infoBarBounds[3], CultureInfo.InvariantCulture) > 0);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ContentHostWritesRenderedVisualArtifact()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var contentHost = new ContentControl
                {
                    Width = 360,
                    Height = 220,
                    Content = new Border
                    {
                        Background = Brushes.White,
                        Child = new TextBlock
                        {
                            Margin = new Thickness(24),
                            Foreground = Brushes.Black,
                            Text = "Visual audit content"
                        }
                    }
                };
                AutomationProperties.SetAutomationId(contentHost, "GalleryContentHost");

                var window = new Window
                {
                    Width = 420,
                    Height = 280,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = contentHost
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(contentHost);

                    var contentHostArtifact = Path.Combine(artifactDirectory, "GalleryContentHost.png");
                    Assert.IsTrue(File.Exists(contentHostArtifact), contentHostArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(contentHostArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(contentHostArtifact), contentHostArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void VisualArtifactsStabilizeIndeterminateProgressBarAnimation()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var progressBar = new ProgressBar
                {
                    Width = 240,
                    IsIndeterminate = true
                };

                var contentHost = new Grid
                {
                    Width = 320,
                    Height = 80,
                    Background = Brushes.White
                };
                AutomationProperties.SetAutomationId(contentHost, "GalleryContentHost");
                contentHost.Children.Add(progressBar);

                var window = new Window
                {
                    Width = 360,
                    Height = 120,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = contentHost
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(contentHost);

                    var animation = progressBar.Template.FindName("Animation", progressBar) as FrameworkElement;
                    Assert.IsNotNull(animation);
                    Assert.AreEqual(new Point(0, 0.5), animation.RenderTransformOrigin);

                    var scale = FindScaleTransform(animation.RenderTransform);
                    Assert.IsNotNull(scale);
                    Assert.AreEqual(0.25, scale.ScaleX, 0.001);

                    var contentHostArtifact = Path.Combine(artifactDirectory, "GalleryContentHost.png");
                    Assert.IsTrue(File.Exists(contentHostArtifact), contentHostArtifact + " was not written.");
                    Assert.IsTrue(HasVisibleRgbPixels(contentHostArtifact), contentHostArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void VisualArtifactsCanPreserveIndeterminateProgressRingAnimation()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--preserve-animated-visuals", "--visual-artifact-dir", artifactDirectory }));

                var progressRing = new Mux.ProgressRing
                {
                    Width = 48,
                    Height = 48,
                    IsActive = true,
                    IsIndeterminate = true
                };

                var contentHost = new Grid
                {
                    Width = 120,
                    Height = 120,
                    Background = Brushes.White
                };
                AutomationProperties.SetAutomationId(contentHost, "GalleryContentHost");
                contentHost.Children.Add(progressRing);

                var window = new Window
                {
                    Width = 160,
                    Height = 160,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = contentHost
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(contentHost);

                    progressRing.ApplyTemplate();
                    var ring = progressRing.Template.FindName("Ring", progressRing) as ProgressRingIndicator;
                    Assert.IsNotNull(ring);

                    var valueSource = DependencyPropertyHelper.GetValueSource(ring, ProgressRingIndicator.IndeterminateStartAngleProperty);
                    Assert.IsTrue(valueSource.IsAnimated, "ProgressRing animation should stay active when --preserve-animated-visuals is set.");

                    var contentHostArtifact = Path.Combine(artifactDirectory, "GalleryContentHost.png");
                    Assert.IsTrue(File.Exists(contentHostArtifact), contentHostArtifact + " was not written.");
                    Assert.IsTrue(HasVisibleRgbPixels(contentHostArtifact), contentHostArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void TopLevelPagesWriteContentRootPaneVisualArtifacts()
        {
            WpfTestHost.Run(() =>
            {
                var cases = new[]
                {
                    Tuple.Create<Page, string>(new DashboardPage(), "HomeContentRootPane.png"),
                    Tuple.Create<Page, string>(new AllSamplesPage(), "AllControlsContentRootPane.png"),
                    Tuple.Create<Page, string>(new SettingsPage(), "SettingsContentRootPane.png")
                };

                foreach (var testCase in cases)
                {
                    var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                    GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                    var page = testCase.Item1;
                    var window = new Window
                    {
                        Width = 1180,
                        Height = 820,
                        Left = -32000,
                        Top = -32000,
                        ShowInTaskbar = false,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Content = page
                    };

                    try
                    {
                        window.Show();
                        WpfTestHost.DoEvents();
                        window.UpdateLayout();
                        WpfTestHost.DoEvents();

                        GalleryDiagnostics.WriteVisualArtifacts(page);

                        var contentRootArtifact = Path.Combine(artifactDirectory, testCase.Item2);
                        Assert.IsTrue(File.Exists(contentRootArtifact), contentRootArtifact + " was not written.");
                        Assert.IsTrue(new FileInfo(contentRootArtifact).Length > 0);
                        Assert.IsTrue(HasVisibleRgbPixels(contentRootArtifact), contentRootArtifact + " has no visible RGB content.");
                    }
                    finally
                    {
                        window.Content = null;
                        window.Close();
                        GalleryDiagnostics.ResetForTests();
                        if (Directory.Exists(artifactDirectory))
                        {
                            Directory.Delete(artifactDirectory, recursive: true);
                        }
                        WpfTestHost.DoEvents();
                    }
                }
            });
        }

        [TestMethod]
        public void ShellNavigationRootWritesRenderedVisualArtifacts()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var page = new NavigationRootPage();
                page.DataContext = new { ViewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings, page.GoForward) };
                var window = new Window
                {
                    Width = 1180,
                    Height = 820,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };
                AutomationProperties.SetAutomationId(window, "ModernWpfGalleryMainWindow");

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    page.NavigateTo("category/Navigation");
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var windowArtifact = Path.Combine(artifactDirectory, "ModernWpfGalleryMainWindow.png");
                    var shellRootArtifact = Path.Combine(artifactDirectory, "GalleryNavigationRoot.png");
                    var navigationArtifact = Path.Combine(artifactDirectory, "GalleryNavigationView.png");
                    var contentRootArtifact = Path.Combine(artifactDirectory, "ContentPagePane.png");

                    Assert.IsTrue(File.Exists(windowArtifact), windowArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(shellRootArtifact), shellRootArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(navigationArtifact), navigationArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(contentRootArtifact), contentRootArtifact + " was not written.");
                    Assert.IsTrue(HasVisibleRgbPixels(windowArtifact), windowArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbPixels(shellRootArtifact), shellRootArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbPixels(navigationArtifact), navigationArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbPixels(contentRootArtifact), contentRootArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void FrameContentHostWritesDescendantVisualArtifacts()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var frame = new Frame
                {
                    Width = 900,
                    Height = 700,
                    Content = new ItemPage(GalleryCatalog.FindItem("InfoBar")),
                    NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
                };
                AutomationProperties.SetAutomationId(frame, "GalleryContentHost");

                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = frame
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(frame);

                    var contentHostArtifact = Path.Combine(artifactDirectory, "GalleryContentHost.png");
                    var infoBarArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_InfoBar.png");
                    Assert.IsFalse(File.Exists(contentHostArtifact), "Frame host surfaces are intentionally skipped.");
                    Assert.IsTrue(File.Exists(infoBarArtifact), infoBarArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(infoBarArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(infoBarArtifact), infoBarArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbVariation(infoBarArtifact), infoBarArtifact + " has no visible RGB variation.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void GalleryVisualScrollHookReachesTheLastControlExampleAfterArtifactCapture()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));
                var page = new ItemPage(GalleryCatalog.FindItem("InfoBadge"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var scrollViewer = (ScrollViewer)FindByAutomationId(page, "GalleryItemPageScrollViewer");
                    Assert.IsNotNull(scrollViewer);
                    var scrollPresenter = (ScrollContentPresenter)scrollViewer.Template.FindName("PART_ScrollContentPresenter", scrollViewer);
                    Assert.AreSame(scrollViewer, scrollPresenter.ScrollOwner, "The item-page ScrollViewer must own its template presenter.");
                    Assert.IsTrue(scrollPresenter.ViewportHeight > 0);
                    Assert.IsTrue(scrollPresenter.ExtentHeight > scrollPresenter.ViewportHeight);

                    GalleryDiagnostics.WriteVisualArtifacts(page);
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var firstExample = (FrameworkElement)FindByAutomationId(page, "GallerySample_InfoBadge_Example1");
                    var lastExample = (FrameworkElement)FindByAutomationId(page, "GallerySample_InfoBadge_Example4");
                    Assert.IsTrue(GalleryDiagnostics.BringVisualArtifactIntoView(page, "GallerySample_InfoBadge_Example4"));
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    var lastPosition = lastExample.TransformToAncestor(scrollPresenter).Transform(new Point());
                    Assert.IsTrue(scrollPresenter.VerticalOffset > 0, "The last example did not move the pixel-scrolling presenter.");
                    Assert.IsTrue(lastPosition.Y >= -0.01, "The last example scrolled above the viewport.");
                    Assert.IsTrue(lastPosition.Y < scrollPresenter.ActualHeight, "The last example remained below the viewport.");
                    Assert.IsTrue(
                        lastPosition.Y + Math.Min(lastExample.ActualHeight, scrollPresenter.ActualHeight) <= scrollPresenter.ActualHeight + 0.01,
                        "The last example was not fully exposed within the available viewport.");

                    Assert.IsTrue(GalleryDiagnostics.BringVisualArtifactIntoView(page, "GallerySample_InfoBadge_Example1"));
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    var firstPosition = firstExample.TransformToAncestor(scrollPresenter).Transform(new Point());
                    Assert.AreEqual(
                        16d,
                        scrollPresenter.VerticalOffset,
                        0.01,
                        "Returning to the first example should consume its intentional 16 px top margin.");
                    Assert.AreEqual(
                        0d,
                        firstPosition.Y,
                        0.01,
                        "Returning to the first example should align its rendered top edge with the viewport.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void VisualTestStatusFileWritesRouteAndReadyState()
        {
            var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
            GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

            try
            {
                GalleryDiagnostics.RecordRoute("item/InfoBar");
                GalleryDiagnostics.SetReadyState("Ready:item/InfoBar");
                GalleryDiagnostics.WriteStatusFile();

                var statusPath = Path.Combine(artifactDirectory, GalleryDiagnostics.StatusFileName);
                Assert.IsTrue(File.Exists(statusPath), statusPath + " was not written.");
                var lines = File.ReadAllLines(statusPath);
                Assert.IsTrue(lines.Length >= 3);
                Assert.AreEqual("item/InfoBar", lines[0]);
                Assert.AreEqual("Ready:item/InfoBar", lines[1]);
                Assert.AreEqual(string.Empty, System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(lines[2])));
            }
            finally
            {
                GalleryDiagnostics.ResetForTests();
                if (Directory.Exists(artifactDirectory))
                {
                    Directory.Delete(artifactDirectory, recursive: true);
                }
            }
        }

        [TestMethod]
        public void VisualArtifactsIgnoreMalformedGallerySampleAutomationIds()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var validSample = new Border
                {
                    Width = 24,
                    Height = 24,
                    Background = Brushes.Red
                };
                GalleryAutomation.WithAutomationId(validSample, GalleryAutomation.SampleElementId("Example", "Button"));

                var malformedSample = new Border
                {
                    Name = "GallerySample_Example",
                    Width = 24,
                    Height = 24,
                    Background = Brushes.Blue
                };
                AutomationProperties.SetAutomationId(malformedSample, "GallerySample_Example");

                var root = new StackPanel();
                root.Children.Add(validSample);
                root.Children.Add(malformedSample);

                var window = new Window
                {
                    Width = 128,
                    Height = 128,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = root
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(root);

                    var validArtifact = Path.Combine(artifactDirectory, "GallerySample_Example_Button.png");
                    var malformedArtifact = Path.Combine(artifactDirectory, "GallerySample_Example.png");
                    Assert.IsTrue(File.Exists(validArtifact), validArtifact + " was not written.");
                    Assert.IsTrue(HasVisibleRgbPixels(validArtifact), validArtifact + " has no visible RGB content.");
                    Assert.IsFalse(File.Exists(malformedArtifact), malformedArtifact + " should not be written.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }

                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void VisualArtifactsIgnoreLegacyContentRootGridAutomationId()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var legacyRoot = new Border
                {
                    Width = 24,
                    Height = 24,
                    Background = Brushes.Blue
                };
                AutomationProperties.SetAutomationId(legacyRoot, "ContentRootGrid");

                var window = new Window
                {
                    Width = 128,
                    Height = 128,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = legacyRoot
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(legacyRoot);

                    var legacyArtifact = Path.Combine(artifactDirectory, "ContentRootGrid.png");
                    Assert.IsFalse(File.Exists(legacyArtifact), legacyArtifact + " should not be written.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }

                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void DirectWpfPageWritesContentPagePaneVisualArtifact()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var page = new ItemPage(GalleryCatalog.FindItem("TextBlock"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(page);

                    var contentPageArtifact = Path.Combine(artifactDirectory, "ContentPagePane.png");
                    Assert.IsTrue(File.Exists(contentPageArtifact), contentPageArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(contentPageArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(contentPageArtifact), contentPageArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void UserDashboardWritesItemPageRootVisualArtifact()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var page = new ItemPage(GalleryCatalog.FindItem("UserDashboard"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    GalleryDiagnostics.WriteVisualArtifacts(page);

                    var itemPageRootArtifact = Path.Combine(artifactDirectory, "GalleryItemPageRoot.png");
                    Assert.IsTrue(File.Exists(itemPageRootArtifact), itemPageRootArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(itemPageRootArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(itemPageRootArtifact), itemPageRootArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void TeachingTipInteractionModeWritesOpenContentArtifact()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--open-interactions", "--visual-artifact-dir", artifactDirectory }));

                var page = new ItemPage(GalleryCatalog.FindItem("TeachingTip"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var teachingTip = (TeachingTipControl)FindByAutomationId(page, "GallerySample_TeachingTip_TeachingTip");
                    Assert.IsNotNull(teachingTip);

                    GalleryDiagnostics.PrepareInteractiveVisualState(page);
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    GalleryDiagnostics.WriteVisualArtifacts(page);

                    Assert.IsTrue(teachingTip.IsOpen);
                    var openContentArtifact = Path.Combine(artifactDirectory, "GalleryItemPageRoot.png");
                    Assert.IsTrue(File.Exists(openContentArtifact), openContentArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(openContentArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(openContentArtifact), openContentArtifact + " has no visible RGB content.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    if (Directory.Exists(artifactDirectory))
                    {
                        Directory.Delete(artifactDirectory, recursive: true);
                    }
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void WpfToolTipInteractionModeOpensTooltip()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--open-interactions" }));
                var page = new ItemPage(GalleryCatalog.FindItem("ToolTip"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var button = (Button)FindByAutomationName(page, "TooltipButton");
                    Assert.IsNotNull(button);
                    var initialToolTip = ToolTipService.GetToolTip(button) as ToolTip;
                    Assert.IsNotNull(initialToolTip);
                    Assert.AreEqual("Simple ToolTip", initialToolTip.Content);

                    GalleryDiagnostics.PrepareInteractiveVisualState(page);
                    WpfTestHost.DoEvents();

                    var toolTip = ToolTipService.GetToolTip(button) as ToolTip;
                    Assert.IsNotNull(toolTip);
                    Assert.AreEqual("Simple ToolTip", toolTip.Content);
                    Assert.AreSame(button, toolTip.PlacementTarget);
                    Assert.IsTrue(toolTip.IsOpen);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void RichTextEditInteractionModeDoesNotPopulateDocumentText()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--open-interactions" }));
                var page = new ItemPage(GalleryCatalog.FindItem("RichTextEdit"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var richTextBox = (RichTextBox)FindByAutomationName(page, "simple rich text editor");
                    Assert.IsNotNull(richTextBox);

                    GalleryDiagnostics.PrepareInteractiveVisualState(page);
                    WpfTestHost.DoEvents();

                    var text = new TextRange(
                        richTextBox.Document.ContentStart,
                        richTextBox.Document.ContentEnd).Text;
                    Assert.IsFalse(
                        text.Contains("ModernWpf rich text", StringComparison.Ordinal),
                        "RichTextEdit must be exercised through recorder input, not diagnostic-prepared text.");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void RichTextEditAcceptsTextCompositionInput()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                var page = new ItemPage(GalleryCatalog.FindItem("RichTextEdit"));
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var richTextBox = (RichTextBox)FindByAutomationName(page, "simple rich text editor");
                    Assert.IsNotNull(richTextBox);
                    Assert.AreSame(DependencyProperty.UnsetValue, richTextBox.ReadLocalValue(FrameworkElement.MinHeightProperty));
                    Assert.IsTrue(double.IsNaN(richTextBox.Height));
                    Assert.IsTrue(
                        richTextBox.ActualHeight > 24d && richTextBox.ActualHeight < 60d,
                        "RichTextEdit should keep the official WPF one-line RichTextBox layout instead of a recorder-inflated editor.");

                    richTextBox.Focus();
                    WpfTestHost.DoEvents();

                    var composition = new TextComposition(
                        InputManager.Current,
                        richTextBox,
                        "ModernWpf rich text");
                    TextCompositionManager.StartComposition(composition);
                    WpfTestHost.DoEvents();

                    var text = new TextRange(
                        richTextBox.Document.ContentStart,
                        richTextBox.Document.ContentEnd).Text;
                    StringAssert.Contains(text, "ModernWpf rich text");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                    WpfTestHost.DoEvents();
                }
            });
        }

        private static void WaitFor(Func<bool> condition)
        {
            var deadline = DateTime.UtcNow.AddSeconds(3);
            while (!condition() && DateTime.UtcNow < deadline)
            {
                WpfTestHost.DoEvents();
                System.Threading.Thread.Sleep(25);
            }

            WpfTestHost.DoEvents();
            Assert.IsTrue(condition());
        }

        private static ItemPage ShowItemPage(Window window, string uniqueId)
        {
            var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            window.Content = page;
            if (!window.IsVisible)
            {
                window.Show();
            }

            WpfTestHost.DoEvents();
            window.UpdateLayout();
            WpfTestHost.DoEvents();
            return page;
        }

        private static Rect GetRelativeBounds(FrameworkElement element, UIElement ancestor)
        {
            var origin = element.TranslatePoint(new Point(), ancestor);
            return new Rect(origin, element.RenderSize);
        }

        private static void AssertRectNear(Rect expected, Rect actual, double tolerance, string message)
        {
            Assert.AreEqual(expected.X, actual.X, tolerance, message + " X");
            Assert.AreEqual(expected.Y, actual.Y, tolerance, message + " Y");
            Assert.AreEqual(expected.Width, actual.Width, tolerance, message + " Width");
            Assert.AreEqual(expected.Height, actual.Height, tolerance, message + " Height");
        }

        private static BitmapSource RenderElementBitmap(FrameworkElement element)
        {
            element.UpdateLayout();

            var width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
            var height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));
            var drawingVisual = new DrawingVisual();
            var visualBrush = new VisualBrush(element)
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Stretch = Stretch.None,
                Viewbox = new Rect(0, 0, width, height),
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, width, height),
                ViewportUnits = BrushMappingMode.Absolute
            };

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    element.TryFindResource("SolidBackgroundFillColorBaseBrush") as Brush
                        ?? new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3)),
                    null,
                    new Rect(0, 0, width, height));
                drawingContext.DrawRectangle(
                    visualBrush,
                    null,
                    new Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            return bitmap;
        }

        private static void WaitForRendering()
        {
            var frame = new DispatcherFrame();
            var rendered = false;
            EventHandler renderingHandler = (_, _) =>
            {
                rendered = true;
                frame.Continue = false;
            };
            var timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            timer.Tick += (_, _) => frame.Continue = false;

            try
            {
                CompositionTarget.Rendering += renderingHandler;
                timer.Start();
                Dispatcher.PushFrame(frame);
            }
            finally
            {
                timer.Stop();
                CompositionTarget.Rendering -= renderingHandler;
            }

            Assert.IsTrue(rendered, "Timed out waiting for a WPF render tick.");
        }

        private static Int32Rect MeasureRenderedColorBounds(BitmapSource source, Func<Color, bool> predicate)
        {
            BitmapSource bitmap = source.Format == PixelFormats.Bgra32
                ? source
                : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            var stride = bitmap.PixelWidth * 4;
            var pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            var minX = bitmap.PixelWidth;
            var minY = bitmap.PixelHeight;
            var maxX = -1;
            var maxY = -1;

            for (var y = 0; y < bitmap.PixelHeight; y++)
            {
                var row = y * stride;
                for (var x = 0; x < bitmap.PixelWidth; x++)
                {
                    var index = row + (x * 4);
                    var color = Color.FromArgb(pixels[index + 3], pixels[index + 2], pixels[index + 1], pixels[index]);
                    if (predicate(color))
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            return maxX < minX || maxY < minY
                ? Int32Rect.Empty
                : new Int32Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static double CompareRenderedMeanDelta(BitmapSource first, BitmapSource second)
        {
            BitmapSource firstBitmap = first.Format == PixelFormats.Bgra32
                ? first
                : new FormatConvertedBitmap(first, PixelFormats.Bgra32, null, 0);
            BitmapSource secondBitmap = second.Format == PixelFormats.Bgra32
                ? second
                : new FormatConvertedBitmap(second, PixelFormats.Bgra32, null, 0);

            Assert.AreEqual(firstBitmap.PixelWidth, secondBitmap.PixelWidth);
            Assert.AreEqual(firstBitmap.PixelHeight, secondBitmap.PixelHeight);

            var stride = firstBitmap.PixelWidth * 4;
            var firstPixels = new byte[stride * firstBitmap.PixelHeight];
            var secondPixels = new byte[stride * secondBitmap.PixelHeight];
            firstBitmap.CopyPixels(firstPixels, stride, 0);
            secondBitmap.CopyPixels(secondPixels, stride, 0);

            long sum = 0;
            for (var i = 0; i < firstPixels.Length; i += 4)
            {
                sum += Math.Abs(firstPixels[i] - secondPixels[i]);
                sum += Math.Abs(firstPixels[i + 1] - secondPixels[i + 1]);
                sum += Math.Abs(firstPixels[i + 2] - secondPixels[i + 2]);
            }

            return sum / (double)(firstBitmap.PixelWidth * firstBitmap.PixelHeight * 3);
        }

        private static DependencyObject FindByAutomationId(DependencyObject root, string automationId)
        {
            if (root == null)
            {
                return null;
            }

            var element = root as UIElement;
            if (element != null && AutomationProperties.GetAutomationId(element) == automationId)
            {
                return root;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindByAutomationId(VisualTreeHelper.GetChild(root, i), automationId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static DependencyObject FindByAutomationName(DependencyObject root, string automationName)
        {
            if (root == null)
            {
                return null;
            }

            var element = root as UIElement;
            if (element != null && AutomationProperties.GetName(element) == automationName)
            {
                return root;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindByAutomationName(VisualTreeHelper.GetChild(root, i), automationName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Mux.ToggleSwitch FindToggleSwitchByHeader(DependencyObject root, string header)
        {
            var toggles = FindDescendants<Mux.ToggleSwitch>(root);
            foreach (var toggle in toggles)
            {
                if (Equals(toggle.Header, header))
                {
                    return toggle;
                }
            }

            return null;
        }

        private static Mux.RadioButtons FindRadioButtonsByHeader(DependencyObject root, string header)
        {
            var radioButtons = FindDescendants<Mux.RadioButtons>(root);
            foreach (var radioButtonGroup in radioButtons)
            {
                if (Equals(radioButtonGroup.Header, header))
                {
                    return radioButtonGroup;
                }
            }

            return null;
        }

        private static bool HasVisibleRgbPixels(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                BitmapSource bitmap = frame.Format == PixelFormats.Bgra32
                    ? frame
                    : new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
                var stride = bitmap.PixelWidth * 4;
                var pixels = new byte[stride * bitmap.PixelHeight];
                bitmap.CopyPixels(pixels, stride, 0);

                for (var i = 0; i < pixels.Length; i += 4)
                {
                    var blue = pixels[i];
                    var green = pixels[i + 1];
                    var red = pixels[i + 2];
                    var alpha = pixels[i + 3];
                    if (alpha > 16 && red + green + blue > 36)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasVisibleRgbVariation(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];
                BitmapSource bitmap = frame.Format == PixelFormats.Bgra32
                    ? frame
                    : new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
                var stride = bitmap.PixelWidth * 4;
                var pixels = new byte[stride * bitmap.PixelHeight];
                bitmap.CopyPixels(pixels, stride, 0);
                var minLuma = 255;
                var maxLuma = 0;

                for (var i = 0; i < pixels.Length; i += 4)
                {
                    var blue = pixels[i];
                    var green = pixels[i + 1];
                    var red = pixels[i + 2];
                    var alpha = pixels[i + 3];
                    if (alpha <= 16)
                    {
                        continue;
                    }

                    var luma = (red * 299 + green * 587 + blue * 114) / 1000;
                    minLuma = Math.Min(minLuma, luma);
                    maxLuma = Math.Max(maxLuma, luma);
                    if (maxLuma - minLuma > 32)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static T FindNamedDescendant<T>(DependencyObject root, string name)
            where T : FrameworkElement
        {
            if (root == null)
            {
                return null;
            }

            var element = root as T;
            if (element != null && element.Name == name)
            {
                return element;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindNamedDescendant<T>(VisualTreeHelper.GetChild(root, i), name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static List<T> FindDescendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            var results = new List<T>();
            AddDescendants(root, results);
            return results;
        }

        private static void AddDescendants<T>(DependencyObject root, List<T> results)
            where T : DependencyObject
        {
            if (root == null)
            {
                return;
            }

            if (root is T item)
            {
                results.Add(item);
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                AddDescendants(VisualTreeHelper.GetChild(root, i), results);
            }
        }

        private static void InvokeListViewBaseItemClick(Mux.ListViewBase listViewBase, Mux.ListViewBaseItem item)
        {
            var invokeMethod = typeof(Mux.ListViewBase).GetMethod(
                "NotifyListItemClicked",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new[] { typeof(Mux.ListViewBaseItem) },
                null);
            Assert.IsNotNull(invokeMethod);
            invokeMethod.Invoke(listViewBase, new object[] { item });
        }

        private static Button FindButtonByContent(DependencyObject root, string content)
        {
            if (root == null)
            {
                return null;
            }

            var button = root as Button;
            if (button != null && string.Equals(button.Content as string, content, StringComparison.Ordinal))
            {
                return button;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindButtonByContent(VisualTreeHelper.GetChild(root, i), content);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static TextBlock FindTextBlockByText(DependencyObject root, string text)
        {
            if (root == null)
            {
                return null;
            }

            var textBlock = root as TextBlock;
            if (textBlock != null && string.Equals(textBlock.Text, text, StringComparison.Ordinal))
            {
                return textBlock;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindTextBlockByText(VisualTreeHelper.GetChild(root, i), text);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void AssertBreadcrumbItems(object itemsSource, params string[] expectedItems)
        {
            var enumerable = itemsSource as System.Collections.IEnumerable;
            Assert.IsNotNull(enumerable);

            var actualItems = new List<string>();
            foreach (var item in enumerable)
            {
                actualItems.Add(GetBreadcrumbItemName(item));
            }

            CollectionAssert.AreEqual(expectedItems, actualItems);
        }

        private static string GetBreadcrumbItemName(object item)
        {
            var text = item as string;
            if (text != null)
            {
                return text;
            }

            var nameProperty = item.GetType().GetProperty("Name");
            Assert.IsNotNull(nameProperty);
            return (string)nameProperty.GetValue(item, null);
        }

        private static void AssertTitleBarColorSelector(DependencyObject root, string name, string automationName, string color)
        {
            var selector = FindNamedDescendant<Button>(root, name);
            Assert.IsNotNull(selector, name + " color selector is missing.");
            Assert.AreEqual(48d, selector.Width);
            Assert.AreEqual(32d, selector.Height);
            Assert.AreEqual(color, selector.Tag);
            Assert.AreEqual(automationName, AutomationProperties.GetName(selector));
        }

        private static void AssertSelectorBarItem(Mux.SelectorBarItem item, string name, string text, Mux.Symbol? symbol, bool isSelected)
        {
            Assert.IsNotNull(item);
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(text, item.Text);
            Assert.AreEqual(isSelected, item.IsSelected);
            if (symbol.HasValue)
            {
                var symbolIcon = item.Icon as Mux.SymbolIcon;
                Assert.IsNotNull(symbolIcon);
                Assert.AreEqual(symbol.Value, symbolIcon.Symbol);
            }
            else
            {
                Assert.IsNull(item.Icon);
            }
        }

        private static void AssertSelectorBarItemUsesVisibleGalleryTemplate(Mux.SelectorBarItem item)
        {
            Assert.AreEqual(
                BaseValueSource.Local,
                DependencyPropertyHelper.GetValueSource(item, Control.TemplateProperty).BaseValueSource,
                "Gallery SelectorBar samples should keep their visible adapted item template.");
            Assert.IsInstanceOfType(item.Foreground, typeof(SolidColorBrush));
            Assert.IsTrue(((SolidColorBrush)item.Foreground).Color.A > 0, "Gallery SelectorBar foreground should be visible.");
            item.ApplyTemplate();
            item.UpdateLayout();

            var itemPeer = FrameworkElementAutomationPeer.CreatePeerForElement(item);
            Assert.IsNotNull(itemPeer);
            Assert.IsNotNull(itemPeer.GetPattern(PatternInterface.SelectionItem));

            var iconVisual = FindDescendants<ContentPresenter>(item).FirstOrDefault(presenter => ReferenceEquals(presenter.Content, item.Icon));
            var textVisual = FindDescendants<TextBlock>(item).FirstOrDefault(textBlock => textBlock.Text == item.Text);
            var selectionVisual = item.Template.FindName("SelectionPill", item) as System.Windows.Shapes.Rectangle;
            Assert.IsNotNull(iconVisual, "SelectorBar item icon presenter should be rendered.");
            Assert.IsNotNull(textVisual, "SelectorBar item text visual should be rendered.");
            Assert.IsNotNull(selectionVisual, "SelectorBar item selection pill should be present.");
            Assert.IsTrue(textVisual.ActualWidth > 0, "SelectorBar text visual should be rendered.");
            Assert.IsTrue(iconVisual.ActualWidth > 0, "SelectorBar icon visual should be rendered.");
            Assert.IsTrue(selectionVisual.ActualWidth > 0, "SelectorBar selection visual should be measured.");
            Assert.IsInstanceOfType(textVisual.RenderTransform, typeof(TranslateTransform));
            Assert.AreEqual(-1.0, ((TranslateTransform)textVisual.RenderTransform).Y);
        }

        private static void AssertNavigationViewInfoBadgeSampleRendered(
            Mux.NavigationView navigationView,
            Mux.NavigationViewItem inboxItem,
            Mux.InfoBadge infoBadge)
        {
            Assert.IsTrue(navigationView.ActualWidth > 0, "InfoBadge sample NavigationView should be measured.");
            var navigationDiagnostics = GetNavigationViewLayoutDiagnostics(navigationView);
            AssertRenderedInside(navigationView, inboxItem, "Inbox NavigationViewItem", navigationDiagnostics);
            AssertRenderedInside(navigationView, infoBadge, "Inbox InfoBadge", navigationDiagnostics);

            var rootGrid = FindNamedDescendant<Mux.GridEx>(infoBadge, "RootGrid");
            Assert.IsNotNull(rootGrid, "InfoBadge should render through its source-shaped RootGrid template part.");
            Assert.AreEqual(new CornerRadius(infoBadge.ActualHeight / 2), infoBadge.TemplateSettings.InfoBadgeCornerRadius);
            Assert.AreEqual(infoBadge.TemplateSettings.InfoBadgeCornerRadius, rootGrid.CornerRadius);

            var cornerPixel = RenderDescendantPixel(infoBadge, infoBadge, 0, 0);
            var centerPixel = RenderDescendantPixel(
                infoBadge,
                infoBadge,
                (int)(infoBadge.ActualWidth / 2),
                (int)(infoBadge.ActualHeight / 2));
            Assert.IsTrue(
                cornerPixel.A < 30 || (cornerPixel.R > 180 && cornerPixel.G > 180 && cornerPixel.B > 180),
                $"InfoBadge's first NavigationView-hosted frame should leave the circular corner transparent. Pixel={cornerPixel}");
            Assert.IsTrue(
                centerPixel.B > centerPixel.R + 60 && centerPixel.B > centerPixel.G + 10,
                $"InfoBadge's circular center should retain the accent fill. Pixel={centerPixel}");

            var visibleMenuTexts = FindDescendants<TextBlock>(navigationView)
                .Where(textBlock => textBlock.IsVisible && textBlock.ActualWidth > 0 && textBlock.ActualHeight > 0)
                .Select(textBlock => textBlock.Text)
                .ToList();

            CollectionAssert.Contains(visibleMenuTexts, "Home", string.Join(", ", visibleMenuTexts));
            CollectionAssert.Contains(visibleMenuTexts, "Account", string.Join(", ", visibleMenuTexts));
            CollectionAssert.Contains(visibleMenuTexts, "Inbox", string.Join(", ", visibleMenuTexts));
        }

        private static Color RenderDescendantPixel(
            FrameworkElement ancestor,
            FrameworkElement descendant,
            int offsetX,
            int offsetY)
        {
            ancestor.UpdateLayout();
            var width = (int)Math.Ceiling(ancestor.ActualWidth);
            var height = (int)Math.Ceiling(ancestor.ActualHeight);
            var origin = descendant.TranslatePoint(new Point(), ancestor);
            var x = Math.Max(0, Math.Min(width - 1, (int)Math.Floor(origin.X) + offsetX));
            var y = Math.Max(0, Math.Min(height - 1, (int)Math.Floor(origin.Y) + offsetY));

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(ancestor);
            var pixels = new byte[4];
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
            return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
        }

        private static void AssertColorPickerTextInputLayoutMatchesReference(Mux.ColorPicker colorPicker)
        {
            var colorRepresentationComboBox = FindNamedDescendant<ComboBox>(colorPicker, "ColorRepresentationComboBox");
            var hexTextBox = FindNamedDescendant<TextBox>(colorPicker, "HexTextBox");
            var redTextBox = FindNamedDescendant<TextBox>(colorPicker, "RedTextBox");
            var greenTextBox = FindNamedDescendant<TextBox>(colorPicker, "GreenTextBox");
            var blueTextBox = FindNamedDescendant<TextBox>(colorPicker, "BlueTextBox");
            var redLabel = FindNamedDescendant<TextBlock>(colorPicker, "RedLabel");
            var greenLabel = FindNamedDescendant<TextBlock>(colorPicker, "GreenLabel");
            var blueLabel = FindNamedDescendant<TextBlock>(colorPicker, "BlueLabel");

            Assert.IsNotNull(colorRepresentationComboBox);
            Assert.IsNotNull(hexTextBox);
            Assert.IsNotNull(redTextBox);
            Assert.IsNotNull(greenTextBox);
            Assert.IsNotNull(blueTextBox);
            Assert.IsNotNull(redLabel);
            Assert.IsNotNull(greenLabel);
            Assert.IsNotNull(blueLabel);

            var comboBounds = AssertRenderedInside(colorPicker, colorRepresentationComboBox, "ColorPicker representation ComboBox");
            var hexBounds = AssertRenderedInside(colorPicker, hexTextBox, "ColorPicker hex TextBox");
            Assert.IsTrue(comboBounds.Left < hexBounds.Left, $"ColorPicker combo should render left of the hex input. Combo={comboBounds}; Hex={hexBounds}.");
            Assert.IsTrue(Math.Abs(comboBounds.Top - hexBounds.Top) <= 4.0, $"ColorPicker combo and hex input should share a row. Combo={comboBounds}; Hex={hexBounds}.");

            var redBounds = AssertColorPickerChannelRow(colorPicker, redTextBox, redLabel, "Red");
            var greenBounds = AssertColorPickerChannelRow(colorPicker, greenTextBox, greenLabel, "Green");
            var blueBounds = AssertColorPickerChannelRow(colorPicker, blueTextBox, blueLabel, "Blue");

            Assert.IsTrue(greenBounds.Top > redBounds.Bottom, $"Green input should stack below Red. Red={redBounds}; Green={greenBounds}.");
            Assert.IsTrue(blueBounds.Top > greenBounds.Bottom, $"Blue input should stack below Green. Green={greenBounds}; Blue={blueBounds}.");
        }

        private static Rect AssertColorPickerChannelRow(FrameworkElement ancestor, TextBox textBox, TextBlock label, string channelName)
        {
            var textBoxBounds = AssertRenderedInside(ancestor, textBox, $"ColorPicker {channelName} TextBox");
            var labelBounds = AssertRenderedInside(ancestor, label, $"ColorPicker {channelName} label");
            Assert.IsTrue(textBoxBounds.Left < labelBounds.Left, $"ColorPicker {channelName} label should render to the right of its input. Input={textBoxBounds}; Label={labelBounds}.");

            var textBoxCenterY = (textBoxBounds.Top + textBoxBounds.Bottom) / 2.0;
            var labelCenterY = (labelBounds.Top + labelBounds.Bottom) / 2.0;
            Assert.IsTrue(Math.Abs(textBoxCenterY - labelCenterY) <= 6.0, $"ColorPicker {channelName} label should align with its input row. Input={textBoxBounds}; Label={labelBounds}.");
            return textBoxBounds;
        }

        private static Rect AssertRenderedInside(FrameworkElement ancestor, FrameworkElement element, string description, string ancestorDiagnostics = null)
        {
            Assert.IsTrue(element.IsVisible, description + " should be visible.");
            Assert.IsTrue(element.ActualWidth > 0, $"{description} should have positive width. Actual={element.ActualWidth}x{element.ActualHeight}. {ancestorDiagnostics}");
            Assert.IsTrue(element.ActualHeight > 0, $"{description} should have positive height. Actual={element.ActualWidth}x{element.ActualHeight}. {ancestorDiagnostics}");

            var bounds = element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));
            Assert.IsTrue(
                bounds.Right > 0 && bounds.Bottom > 0 && bounds.Left < ancestor.ActualWidth && bounds.Top < ancestor.ActualHeight,
                $"{description} should render inside the NavigationView. Bounds={bounds}; Ancestor={ancestor.ActualWidth}x{ancestor.ActualHeight}.");
            return bounds;
        }

        private static string GetNavigationViewLayoutDiagnostics(Mux.NavigationView navigationView)
        {
            var paneContentGrid = FindNamedDescendant<FrameworkElement>(navigationView, "PaneContentGrid");
            var menuItemsHost = FindNamedDescendant<FrameworkElement>(navigationView, "MenuItemsHost");
            var rootSplitView = FindNamedDescendant<FrameworkElement>(navigationView, "RootSplitView");
            return string.Format(
                CultureInfo.InvariantCulture,
                "NavigationView Actual={0:0.##}x{1:0.##}; TemplateOpenPaneLength={2:0.##}; OpenPaneLength={3:0.##}; IsPaneOpen={4}; DisplayMode={5}; PaneDisplayMode={6}; RootSplitView={7}; PaneContentGrid={8}; MenuItemsHost={9}",
                navigationView.ActualWidth,
                navigationView.ActualHeight,
                navigationView.TemplateSettings.OpenPaneLength,
                navigationView.OpenPaneLength,
                navigationView.IsPaneOpen,
                navigationView.DisplayMode,
                navigationView.PaneDisplayMode,
                FormatActualSize(rootSplitView),
                FormatActualSize(paneContentGrid),
                FormatActualSize(menuItemsHost));
        }

        private static Mux.NavigationView AssertNavigationViewSampleArtifact(
            DependencyObject page,
            string automationId,
            string expectedName,
            double expectedWidth,
            double expectedHeight)
        {
            var navigationView = FindByAutomationId(page, automationId) as Mux.NavigationView;
            Assert.IsNotNull(navigationView, automationId + " should identify a NavigationView sample artifact.");

            navigationView.BringIntoView();
            WpfTestHost.DoEvents();
            navigationView.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreEqual(automationId, AutomationProperties.GetAutomationId(navigationView));
            Assert.AreEqual(expectedName, navigationView.Name);
            Assert.IsTrue(navigationView.IsLoaded, automationId + " should be loaded before visual validation.");
            Assert.IsTrue(navigationView.IsVisible, automationId + " should be visible before visual validation.");
            Assert.AreEqual(expectedWidth, navigationView.ActualWidth, 0.01, automationId + " should render at the WinUI Gallery sample width.");
            Assert.AreEqual(expectedHeight, navigationView.ActualHeight, 0.01, automationId + " should render at the Gallery sample height.");
            return navigationView;
        }

        private static void AssertTopNavigationViewGeometry(
            Mux.NavigationView navigationView,
            string selectedContent,
            string sampleDescription)
        {
            navigationView.BringIntoView();
            WpfTestHost.DoEvents();
            navigationView.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, navigationView.PaneDisplayMode, sampleDescription);
            Assert.IsFalse(navigationView.IsPaneOpen, sampleDescription + " should not retain a left pane in Top mode.");

            var selectedItem = FindDescendants<Mux.NavigationViewItem>(navigationView)
                .Single(item => Equals(item.Content, selectedContent));
            Assert.AreSame(selectedItem, navigationView.SelectedItem, sampleDescription + " should keep its first item selected.");
            Assert.IsTrue(selectedItem.IsSelected, sampleDescription + " should render the selected item state.");

            var topNavGrid = FindNamedDescendant<Grid>(navigationView, "TopNavGrid");
            var itemPresenter = FindNamedDescendant<Mux.Primitives.NavigationViewItemPresenter>(
                selectedItem,
                "NavigationViewItemPresenter");
            var selectionIndicator = FindNamedDescendant<System.Windows.Shapes.Rectangle>(
                selectedItem,
                "SelectionIndicator");
            var itemText = FindDescendants<TextBlock>(selectedItem)
                .Single(textBlock => textBlock.Text == selectedContent);
            var itemPresenterBounds = GetRelativeBounds(itemPresenter, topNavGrid);
            var selectionIndicatorBounds = GetRelativeBounds(selectionIndicator, topNavGrid);
            var itemTextBounds = GetRelativeBounds(itemText, topNavGrid);

            Assert.AreEqual(48.0, itemPresenterBounds.Height, 0.01, sampleDescription + " items should fill the Top strip.");
            Assert.AreEqual(16.0, selectionIndicatorBounds.Width, 0.01, sampleDescription + " should use the WinUI indicator width.");
            Assert.AreEqual(3.0, selectionIndicatorBounds.Height, 0.01, sampleDescription + " should use the WinUI indicator height.");
            Assert.AreEqual(39.0, selectionIndicatorBounds.Top, 0.01, sampleDescription + " should put the indicator at the bottom of the Top strip.");
            Assert.AreEqual(1.0, selectionIndicator.Opacity, 0.01, sampleDescription + " should show its selected-item indicator.");
            Assert.AreEqual(
                itemPresenterBounds.Left + (itemPresenterBounds.Width / 2.0),
                selectionIndicatorBounds.Left + (selectionIndicatorBounds.Width / 2.0),
                0.51,
                sampleDescription + " should center the indicator beneath its item.");
            Assert.IsTrue(
                itemTextBounds.Bottom < selectionIndicatorBounds.Top,
                $"{sampleDescription} should not overlap its label and indicator. Text={itemTextBounds}; Indicator={selectionIndicatorBounds}.");
        }

        private static void AssertExpandedLeftNavigationViewGeometry(
            Mux.NavigationView navigationView,
            string firstItemContent,
            string sampleDescription,
            bool expectSelection)
        {
            navigationView.BringIntoView();
            WpfTestHost.DoEvents();
            navigationView.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Left, navigationView.PaneDisplayMode, sampleDescription);
            Assert.AreEqual(Mux.NavigationViewDisplayMode.Expanded, navigationView.DisplayMode, sampleDescription);
            Assert.IsTrue(navigationView.IsPaneOpen, sampleDescription + " should start with its Left pane open.");
            AssertNavigationViewVisualState(navigationView, "PaneStateGroup", "NotClosedCompact", sampleDescription);
            AssertNavigationViewVisualState(navigationView, "PaneStateListSizeGroup", "ListSizeFull", sampleDescription);

            var firstItem = FindDescendants<Mux.NavigationViewItem>(navigationView)
                .Single(item => Equals(item.Content, firstItemContent));
            var paneContentGrid = FindNamedDescendant<Border>(navigationView, "PaneContentGrid");
            var contentPresenter = FindNamedDescendant<Mux.ContentPresenterEx>(firstItem, "ContentPresenter");
            var paneBounds = GetRelativeBounds(paneContentGrid, navigationView);
            var itemBounds = GetRelativeBounds(firstItem, navigationView);
            var contentBounds = GetRelativeBounds(contentPresenter, navigationView);

            Assert.AreEqual(navigationView.OpenPaneLength, paneBounds.Width, 0.01, sampleDescription + " should use the full pane width.");
            Assert.AreEqual(navigationView.OpenPaneLength - 1.0, itemBounds.Width, 0.01, sampleDescription + " should size menu items to the pane.");
            Assert.IsTrue(contentBounds.Width > 0, sampleDescription + " should show its menu labels in the open pane.");
            Assert.IsTrue(contentBounds.Right <= paneBounds.Right + 0.01, sampleDescription + " menu labels should remain inside the pane.");

            if (expectSelection)
            {
                Assert.AreSame(firstItem, navigationView.SelectedItem, sampleDescription + " should start with its first item selected.");
                var selectionIndicator = FindNamedDescendant<System.Windows.Shapes.Rectangle>(firstItem, "SelectionIndicator");
                var selectionIndicatorBounds = GetRelativeBounds(selectionIndicator, firstItem);
                Assert.AreEqual(4.0, selectionIndicatorBounds.Left, 0.01, sampleDescription + " should use the WinUI left indicator origin.");
            }
        }

        private static void AssertClosedCompactNavigationViewGeometry(
            Mux.NavigationView navigationView,
            string firstItemContent,
            string sampleDescription)
        {
            navigationView.BringIntoView();
            WpfTestHost.DoEvents();
            navigationView.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.AreEqual(Mux.NavigationViewDisplayMode.Compact, navigationView.DisplayMode, sampleDescription);
            Assert.IsFalse(navigationView.IsPaneOpen, sampleDescription + " should keep its compact pane closed.");
            AssertNavigationViewVisualState(navigationView, "PaneStateGroup", "ClosedCompact", sampleDescription);
            AssertNavigationViewVisualState(navigationView, "PaneStateListSizeGroup", "ListSizeCompact", sampleDescription);

            var firstItem = FindDescendants<Mux.NavigationViewItem>(navigationView)
                .Single(item => Equals(item.Content, firstItemContent));
            var paneContentGrid = FindNamedDescendant<Border>(navigationView, "PaneContentGrid");
            var contentPresenter = FindNamedDescendant<Mux.ContentPresenterEx>(firstItem, "ContentPresenter");
            var paneBounds = GetRelativeBounds(paneContentGrid, navigationView);
            var itemBounds = GetRelativeBounds(firstItem, navigationView);
            var contentBounds = GetRelativeBounds(contentPresenter, navigationView);

            Assert.AreEqual(navigationView.CompactPaneLength, paneBounds.Width, 0.01, sampleDescription + " should use the compact pane width.");
            Assert.AreEqual(navigationView.CompactPaneLength - 1.0, itemBounds.Width, 0.01, sampleDescription + " should constrain compact menu items.");
            Assert.AreEqual(0.0, contentBounds.Width, 0.01, sampleDescription + " should not leak menu-label pixels from its compact pane.");
        }

        private static void AssertNavigationViewVisualState(
            Mux.NavigationView navigationView,
            string groupName,
            string expectedState,
            string sampleDescription)
        {
            var rootGrid = FindNamedDescendant<Grid>(navigationView, "RootGrid");
            var group = VisualStateManager.GetVisualStateGroups(rootGrid)
                .Cast<VisualStateGroup>()
                .Single(stateGroup => stateGroup.Name == groupName);
            Assert.AreEqual(expectedState, group.CurrentState?.Name, sampleDescription + " has the wrong " + groupName + " state.");
        }

        private static string FormatActualSize(FrameworkElement element)
        {
            if (element == null)
            {
                return "<missing>";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.##}x{1:0.##}",
                element.ActualWidth,
                element.ActualHeight);
        }

        private static void RaiseSelectorBarItemClick(Mux.SelectorBarItem item)
        {
            item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = item
            });
            item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                Source = item
            });
        }

        private static void AssertTabItem(TabItem item, string header)
        {
            Assert.IsNotNull(item);
            if (item.Header is string headerText)
            {
                Assert.AreEqual(header, headerText);
            }
            else
            {
                Assert.AreEqual(header, AutomationProperties.GetName(item));
            }
            Assert.AreSame(item.TryFindResource("DefaultTabItemStyle"), item.Style);
        }


        private static string GetFramePageTitle(Frame frame)
        {
            var page = frame.Content as Page;
            Assert.IsNotNull(page);
            return page.Title;
        }

        private static void AssertSelectorBarSamplePage1Layout(Frame frame)
        {
            var page = frame.Content as Page;
            Assert.IsNotNull(page);
            var scrollViewer = page.Content as ScrollViewer;
            Assert.IsNotNull(scrollViewer);
            var grid = scrollViewer.Content as Grid;
            Assert.IsNotNull(grid);
            Assert.AreEqual(3, grid.ColumnDefinitions.Count);
            Assert.AreEqual(4, grid.RowDefinitions.Count);

            var sourceElement = FindNamedDescendant<Grid>(page, "SourceElement");
            Assert.IsNotNull(sourceElement);
            Assert.AreEqual(250d, sourceElement.MinWidth);
            Assert.AreEqual(2, Grid.GetRowSpan(sourceElement));
            Assert.AreEqual(5, grid.Children.OfType<Grid>().Count(child => child.MinHeight == 150d));
        }

        private static void AssertSelectorBarSamplePage3Layout(Frame frame)
        {
            var page = frame.Content as Page;
            Assert.IsNotNull(page);
            var scrollViewer = page.Content as ScrollViewer;
            Assert.IsNotNull(scrollViewer);
            var grid = scrollViewer.Content as Grid;
            Assert.IsNotNull(grid);
            Assert.AreEqual(3, grid.ColumnDefinitions.Count);
            Assert.AreEqual(new GridLength(2, GridUnitType.Star), grid.ColumnDefinitions[0].Width);
            Assert.AreEqual(5, grid.Children.OfType<Grid>().Count(child => child.MinHeight == 150d));
        }

        private static int CountItems(object itemsSource)
        {
            var enumerable = itemsSource as System.Collections.IEnumerable;
            Assert.IsNotNull(enumerable);

            var count = 0;
            foreach (var item in enumerable)
            {
                count++;
            }

            return count;
        }

        private static void AssertAnnotatedColorItem(WrapPanel itemsRepeater, int index, Color expectedColor)
        {
            var item = itemsRepeater.Children[index] as Border;
            Assert.IsNotNull(item);
            Assert.AreEqual(112.0, item.Width);
            Assert.AreEqual(82.0, item.Height);
            Assert.AreEqual(new Thickness(4), item.Margin);
            Assert.AreEqual(new CornerRadius(4), item.CornerRadius);
            Assert.AreEqual(expectedColor, ((SolidColorBrush)item.Background).Color);
        }

        private static void AssertAnnotatedLabel(Mux.AnnotatedScrollBarLabel label, string content, int itemIndex, int itemsPerRow)
        {
            Assert.IsNotNull(label);
            Assert.AreEqual(content, label.Content);
            Assert.AreEqual(90 * (itemIndex / itemsPerRow), label.ScrollOffset);
        }

        private static void AssertPopupOffsetNumberBox(
            Mux.NumberBox numberBox,
            string header,
            double minimum,
            double maximum,
            double value)
        {
            Assert.AreEqual(header, numberBox.Header);
            Assert.AreEqual(minimum, numberBox.Minimum);
            Assert.AreEqual(maximum, numberBox.Maximum);
            Assert.AreEqual(value, numberBox.Value);
            Assert.AreEqual(10.0, numberBox.SmallChange);
            Assert.AreEqual(100.0, numberBox.LargeChange);
            Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Inline, numberBox.SpinButtonPlacementMode);
        }

        private static StackPanel AssertListMarkerFlyout(ModernWpf.Controls.Primitives.FlyoutBase flyoutBase)
        {
            var flyout = flyoutBase as Mux.Flyout;
            Assert.IsNotNull(flyout);
            Assert.AreEqual(ModernWpf.Controls.Primitives.FlyoutPlacementMode.Bottom, flyout.Placement);

            var panel = flyout.Content as StackPanel;
            Assert.IsNotNull(panel);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.Children.Count);

            AssertListMarkerButton((Button)panel.Children[0], "Bulleted list", Mux.Symbol.List);
            AssertListMarkerButton((Button)panel.Children[1], "Roman numerals list", Mux.Symbol.Bullets);
            return panel;
        }

        private static void AssertListMarkerButton(Button button, string automationName, Mux.Symbol symbol)
        {
            Assert.IsNotNull(button);
            Assert.AreEqual(automationName, AutomationProperties.GetName(button));
            Assert.AreEqual(new Thickness(4), button.Padding);
            Assert.AreEqual(0.0, button.MinWidth);
            Assert.AreEqual(0.0, button.MinHeight);
            Assert.AreEqual(new Thickness(6), button.Margin);
            Assert.AreEqual(symbol, ((Mux.SymbolIcon)button.Content).Symbol);
        }

        private static Mux.VariableSizedWrapGrid AssertColorSwatchFlyout(ModernWpf.Controls.Primitives.FlyoutBase flyoutBase, int expectedCount, bool includeBlack)
        {
            var flyout = flyoutBase as Mux.Flyout;
            Assert.IsNotNull(flyout);
            Assert.AreEqual(ModernWpf.Controls.Primitives.FlyoutPlacementMode.Bottom, flyout.Placement);

            var grid = flyout.Content as Mux.VariableSizedWrapGrid;
            Assert.IsNotNull(grid);
            Assert.AreEqual(3, grid.MaximumRowsOrColumns);
            Assert.AreEqual(Orientation.Horizontal, grid.Orientation);
            Assert.AreEqual(expectedCount, grid.Children.Count);

            var expectedNames = includeBlack
                ? new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray", "Black" }
                : new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray" };
            Assert.AreEqual(expectedCount, expectedNames.Length);
            for (var i = 0; i < expectedNames.Length; i++)
            {
                var button = grid.Children[i] as Button;
                Assert.IsNotNull(button);
                Assert.AreEqual(expectedNames[i], AutomationProperties.GetName(button));
                Assert.AreEqual(new Thickness(0), button.Padding);
                Assert.AreEqual(0.0, button.MinWidth);
                Assert.AreEqual(0.0, button.MinHeight);
                Assert.AreEqual(new Thickness(6), button.Margin);

                var swatch = button.Content as System.Windows.Shapes.Rectangle;
                Assert.IsNotNull(swatch);
                Assert.AreEqual(32.0, swatch.Width);
                Assert.AreEqual(32.0, swatch.Height);
                Assert.AreEqual(4.0, swatch.RadiusX);
                Assert.AreEqual(4.0, swatch.RadiusY);
                Assert.AreEqual((Color)ColorConverter.ConvertFromString(expectedNames[i]), ((SolidColorBrush)swatch.Fill).Color);
            }

            return grid;
        }

        private static void AssertEmailDropDownFlyout(ModernWpf.Controls.Primitives.FlyoutBase flyoutBase, bool includeIcons)
        {
            var flyout = flyoutBase as Mux.MenuFlyout;
            Assert.IsNotNull(flyout);
            Assert.AreEqual(ModernWpf.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft, flyout.Placement);
            Assert.AreEqual(3, flyout.Items.Count);

            var send = (MenuItem)flyout.Items[0];
            var reply = (MenuItem)flyout.Items[1];
            var replyAll = (MenuItem)flyout.Items[2];
            Assert.AreEqual("Send", send.Header);
            Assert.AreEqual("Reply", reply.Header);
            Assert.AreEqual("Reply All", replyAll.Header);

            if (includeIcons)
            {
                Assert.AreEqual("\uE725", ((Mux.FontIcon)send.Icon).Glyph);
                Assert.AreEqual("\uE8CA", ((Mux.FontIcon)reply.Icon).Glyph);
                Assert.AreEqual("\uE8C2", ((Mux.FontIcon)replyAll.Icon).Glyph);
            }
            else
            {
                Assert.IsNull(send.Icon);
                Assert.IsNull(reply.Icon);
                Assert.IsNull(replyAll.Icon);
            }
        }

        private static string GetCommandListItemText(object item)
        {
            var property = item.GetType().GetProperty("Text");
            Assert.IsNotNull(property);
            return (string)property.GetValue(item, null);
        }

        private static void AssertAppBarButton(object command, Mux.Symbol symbol, string label)
        {
            var button = command as Mux.AppBarButton;
            Assert.IsNotNull(button);
            Assert.AreEqual(label, button.Label);
            Assert.AreEqual(symbol, ((Mux.SymbolIcon)button.Icon).Symbol);
        }

        private static ScaleTransform FindScaleTransform(Transform transform)
        {
            var scale = transform as ScaleTransform;
            if (scale != null)
            {
                return scale;
            }

            var group = transform as TransformGroup;
            return group?.Children.OfType<ScaleTransform>().FirstOrDefault();
        }
    }
}
