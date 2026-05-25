using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Testing;
using Mux = ModernWpf.Controls;
using TeachingTipControl = ModernWpf.Controls.TeachingTip;
using WpfShapes = System.Windows.Shapes;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryAutomationHookTests
    {
        public static IEnumerable<object[]> CuratedSampleAutomationIds()
        {
            yield return new object[] { "TeachingTip", "GallerySample_TeachingTip_Root", "GallerySample_TeachingTip_ShowButton" };
            yield return new object[] { "InfoBadge", "GallerySample_InfoBadge_Root", "GallerySample_InfoBadge_InfoBadge" };
            yield return new object[] { "InfoBar", "GallerySample_InfoBar_Root", "GallerySample_InfoBar_InfoBar" };
            yield return new object[] { "ProgressRing", "GallerySample_ProgressRing_Root", "GallerySample_ProgressRing_ProgressRing" };
            yield return new object[] { "PipsPager", "GallerySample_PipsPager_Root", "GallerySample_PipsPager_PipsPager" };
            yield return new object[] { "PullToRefresh", "GallerySample_PullToRefresh_Root", "GallerySample_PullToRefresh_RefreshContainer" };
            yield return new object[] { "SplitView", "GallerySample_SplitView_Root", "GallerySample_SplitView_SplitView" };
            yield return new object[] { "PersonPicture", "GallerySample_PersonPicture_Root", "GallerySample_PersonPicture_PersonPicture" };
            yield return new object[] { "Sound", "GallerySample_Sound_Root", "GallerySample_Sound_ToggleSwitch" };
            yield return new object[] { "MediaPlayerElement", "GallerySample_MediaPlayerElement_Root", "GallerySample_MediaPlayerElement_MediaPlayerElement" };
            yield return new object[] { "MapControl", "GallerySample_MapControl_Root", "GallerySample_MapControl_MapControl" };
            yield return new object[] { "WebView2", "GallerySample_WebView2_Root", "GallerySample_WebView2_WebView2" };
            yield return new object[] { "Acrylic", "GallerySample_Acrylic_Root", "GallerySample_Acrylic_Example1Grid" };
            yield return new object[] { "AnimatedIcon", "GallerySample_AnimatedIcon_Root", "GallerySample_AnimatedIcon_Button" };
            yield return new object[] { "CompactSizing", "GallerySample_CompactSizing_Root", "GallerySample_CompactSizing_FirstName" };
            yield return new object[] { "IconElement", "GallerySample_IconElement_Root", "GallerySample_IconElement_SlicesIcon" };
            yield return new object[] { "Line", "GallerySample_Line_Root", "GallerySample_Line_Line" };
            yield return new object[] { "Shape", "GallerySample_Shape_Root", "GallerySample_Shape_Ellipse" };
            yield return new object[] { "RadialGradientBrush", "GallerySample_RadialGradientBrush_Root", "GallerySample_RadialGradientBrush_Rect" };
            yield return new object[] { "SystemBackdrops", "GallerySample_SystemBackdrops_Root", "GallerySample_SystemBackdrops_ShowWindowButton" };
            yield return new object[] { "SystemBackdropElement", "GallerySample_SystemBackdropElement_Root", "GallerySample_SystemBackdropElement_Button" };
            yield return new object[] { "ThemeShadow", "GallerySample_ThemeShadow_Root", "GallerySample_ThemeShadow_ShadowRect" };
            yield return new object[] { "CreateMultipleWindows", "GallerySample_CreateMultipleWindows_Root", "GallerySample_CreateMultipleWindows_Control1" };
            yield return new object[] { "AppWindow", "GallerySample_AppWindow_Root", "GallerySample_AppWindow_ShowSampleWindow1Button" };
            yield return new object[] { "AppWindowTitleBar", "GallerySample_AppWindowTitleBar_Root", "GallerySample_AppWindowTitleBar_ShowWindowButton" };
            yield return new object[] { "TitleBar", "GallerySample_TitleBar_Root", "GallerySample_TitleBar_TitleBarControl" };
            yield return new object[] { "StoragePickers", "GallerySample_StoragePickers_Root", "GallerySample_StoragePickers_PickSingleFileButton" };
            yield return new object[] { "FlipView", "GallerySample_FlipView_Root", "GallerySample_FlipView_FlipView" };
            yield return new object[] { "ItemsView", "GallerySample_ItemsView_Root", "GallerySample_ItemsView_ItemsView" };
            yield return new object[] { "CalendarDatePicker", "GallerySample_CalendarDatePicker_Root", "GallerySample_CalendarDatePicker_CalendarDatePicker" };
            yield return new object[] { "CalendarView", "GallerySample_CalendarView_Root", "GallerySample_CalendarView_CalendarView" };
            yield return new object[] { "TimePicker", "GallerySample_TimePicker_Root", "GallerySample_TimePicker_TimePicker" };
            yield return new object[] { "GridView", "GallerySample_GridView_Root", "GallerySample_GridView_BasicGridView" };
            yield return new object[] { "ItemsRepeater", "GallerySample_ItemsRepeater_Root", "GallerySample_ItemsRepeater_ItemsRepeater" };
            yield return new object[] { "BreadcrumbBar", "GallerySample_BreadcrumbBar_Root", "GallerySample_BreadcrumbBar_BreadcrumbBar" };
            yield return new object[] { "Pivot", "GallerySample_Pivot_Root", "GallerySample_Pivot_Pivot" };
            yield return new object[] { "SelectorBar", "GallerySample_SelectorBar_Root", "GallerySample_SelectorBar_SelectorBar" };
            yield return new object[] { "TabView", "GallerySample_TabView_Root", "GallerySample_TabView_TabView" };
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
            yield return new object[] { "RichEditBox", "GallerySample_RichEditBox_Root", "GallerySample_RichEditBox_RichEditBox" };
            yield return new object[] { "RichTextBlock", "GallerySample_RichTextBlock_Root", "GallerySample_RichTextBlock_RichTextBlock" };
            yield return new object[] { "MenuBar", "GallerySample_MenuBar_Root", "GallerySample_MenuBar_MenuBar" };
            yield return new object[] { "MenuFlyout", "GallerySample_MenuFlyout_Root", "GallerySample_MenuFlyout_AppBarButton" };
            yield return new object[] { "SwipeControl", "GallerySample_SwipeControl_Root", "GallerySample_SwipeControl_SwipeControl" };
            yield return new object[] { "AppBarButton", "GallerySample_AppBarButton_Root", "GallerySample_AppBarButton_AppBarButton" };
            yield return new object[] { "AppBarSeparator", "GallerySample_AppBarSeparator_Root", "GallerySample_AppBarSeparator_CommandBar" };
            yield return new object[] { "AppBarToggleButton", "GallerySample_AppBarToggleButton_Root", "GallerySample_AppBarToggleButton_AppBarToggleButton" };
            yield return new object[] { "CommandBar", "GallerySample_CommandBar_Root", "GallerySample_CommandBar_CommandBar" };
            yield return new object[] { "CommandBarFlyout", "GallerySample_CommandBarFlyout_Root", "GallerySample_CommandBarFlyout_ShowButton" };
            yield return new object[] { "StandardUICommand", "GallerySample_StandardUICommand_Root", "GallerySample_StandardUICommand_ListView" };
            yield return new object[] { "XamlUICommand", "GallerySample_XamlUICommand_Root", "GallerySample_XamlUICommand_AppBarButton" };
        }

        [TestMethod]
        [DynamicData(nameof(CuratedSampleAutomationIds), DynamicDataSourceType.Method)]
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

                    var pageHeader = FindNamedDescendant<PageHeader>(page, "PageHeader");
                    Assert.IsNotNull(pageHeader, "Item page header is missing.");
                    pageHeader.ApplyTemplate();
                    var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
                    Assert.IsNotNull(titleLabel, "Item page title label is missing.");
                    Assert.AreEqual(page.Title + " Page", AutomationProperties.GetName(titleLabel));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleLabel));
                    Assert.IsTrue(KeyboardNavigation.GetIsTabStop(titleLabel));
                    Assert.AreEqual(0, KeyboardNavigation.GetTabIndex(titleLabel));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId((TextBlock)titleLabel.Content));
                    Assert.IsNotNull(FindByAutomationId(page, "GallerySampleHost"), "Sample host AutomationId is missing.");
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
                    Assert.AreEqual("Yes, empty my cart", ((Button)flyoutPanel.Children[1]).Content);
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
                    Assert.AreEqual("Simple Popup", ((TextBlock)surfacePanel.Children[0]).Text);
                    Assert.AreEqual(16.0, ((TextBlock)surfacePanel.Children[0]).FontSize);
                    Assert.AreEqual("Close", ((Button)surfacePanel.Children[1]).Content);

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

                    ((Button)surfacePanel.Children[1]).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
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

                    var navigationView = (ModernWpf.Controls.NavigationView)FindByAutomationId(page, "GallerySample_NavigationView_NavigationView");
                    Assert.IsNotNull(navigationView);
                    Assert.AreEqual("nvSample5", navigationView.Name);
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
                    Assert.AreEqual(ScrollBarVisibility.Hidden, firstContent.VerticalScrollBarVisibility);

                    var firstItem = (ModernWpf.Controls.NavigationViewItem)navigationView.MenuItems[0];
                    Assert.AreEqual("Menu Item1", firstItem.Content);
                    Assert.AreEqual("SamplePage1", firstItem.Tag);
                    Assert.AreEqual(ModernWpf.Controls.Symbol.Play, ((ModernWpf.Controls.SymbolIcon)firstItem.Icon).Symbol);
                    Assert.AreSame(firstItem, navigationView.SelectedItem);

                    var topNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample6");
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, topNavigationView.PaneDisplayMode);
                    Assert.AreEqual("This is Header Text", topNavigationView.Header);
                    Assert.AreEqual(4, topNavigationView.MenuItems.Count);

                    var adaptiveNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample2");
                    Assert.IsTrue(adaptiveNavigationView.ActualWidth >= adaptiveNavigationView.CompactModeThresholdWidth);
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, adaptiveNavigationView.PaneDisplayMode);
                    Assert.AreEqual(4, adaptiveNavigationView.MenuItems.Count);

                    var tabsNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample7");
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, tabsNavigationView.PaneDisplayMode);
                    Assert.AreEqual(Mux.NavigationViewBackButtonVisible.Collapsed, tabsNavigationView.IsBackButtonVisible);
                    Assert.AreEqual(Mux.NavigationViewSelectionFollowsFocus.Enabled, tabsNavigationView.SelectionFollowsFocus);

                    var boundNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample4");
                    Assert.IsNotNull(boundNavigationView.MenuItemsSource);
                    Assert.IsNotNull(boundNavigationView.MenuItemTemplate);
                    Assert.IsNotNull(boundNavigationView.SelectedItem);

                    var footerNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample9");
                    Assert.IsFalse(footerNavigationView.IsSettingsVisible);
                    Assert.AreEqual(3, footerNavigationView.MenuItems.Count);
                    Assert.AreEqual(3, footerNavigationView.FooterMenuItems.Count);
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Left, footerNavigationView.PaneDisplayMode);
                    var footerTop = FindNamedDescendant<RadioButton>(page, "nvSample9Top");
                    footerTop.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.Top, footerNavigationView.PaneDisplayMode);
                    Assert.IsFalse(footerNavigationView.IsPaneOpen);

                    var hierarchicalNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample8");
                    Assert.AreEqual(3, hierarchicalNavigationView.MenuItems.Count);
                    var accountItem = (Mux.NavigationViewItem)hierarchicalNavigationView.MenuItems[1];
                    var documentOptionsItem = (Mux.NavigationViewItem)hierarchicalNavigationView.MenuItems[2];
                    Assert.AreEqual(2, accountItem.MenuItems.Count);
                    Assert.AreEqual(2, documentOptionsItem.MenuItems.Count);
                    Assert.IsFalse(documentOptionsItem.SelectsOnInvoked);
                    var hierarchicalLeftCompact = FindNamedDescendant<RadioButton>(page, "nvSample8LeftCompact");
                    hierarchicalLeftCompact.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Mux.NavigationViewPaneDisplayMode.LeftCompact, hierarchicalNavigationView.PaneDisplayMode);
                    Assert.IsFalse(hierarchicalNavigationView.IsPaneOpen);

                    var apiNavigationView = FindNamedDescendant<Mux.NavigationView>(page, "nvSample");
                    var samplePage2Item = FindNamedDescendant<Mux.NavigationViewItem>(page, "SamplePage2Item");
                    Assert.AreEqual("Header", apiNavigationView.Header);
                    Assert.AreEqual("Pane Title", apiNavigationView.PaneTitle);
                    Assert.AreEqual(Mux.NavigationViewBackButtonVisible.Visible, apiNavigationView.IsBackButtonVisible);
                    Assert.IsNotNull(apiNavigationView.AutoSuggestBox);

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
                    StringAssert.Contains(page.Examples[2].CSharpCode, "PinkColorCollection");

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

                    Assert.AreEqual("SelectorBar1", selectorBar1.Name);
                    AssertSelectorBarItem(selectorBar1.Items[0], "SelectorBarItemRecent", "Recent", Mux.Symbol.Clock, false);
                    AssertSelectorBarItem(selectorBar1.Items[1], "SelectorBarItemShared", "Shared", Mux.Symbol.Share, false);
                    AssertSelectorBarItem(selectorBar1.Items[2], "SelectorBarItemFavorites", "Favorites", Mux.Symbol.OutlineStar, false);

                    Assert.AreEqual("SelectorBar2", selectorBar2.Name);
                    Assert.AreEqual(5, selectorBar2.Items.Count);
                    Assert.AreSame(selectorBar2.Items[0], selectorBar2.SelectedItem);
                    Assert.AreEqual("SamplePage1", GetFramePageTitle(contentFrame));

                    selectorBar2.SelectedItem = selectorBar2.Items[2];
                    WpfTestHost.DoEvents();
                    Assert.AreSame(selectorBar2.Items[2], selectorBar2.SelectedItem);
                    Assert.AreEqual("SamplePage3", GetFramePageTitle(contentFrame));

                    Assert.AreEqual("SelectorBar3", selectorBar3.Name);
                    Assert.AreEqual(3, selectorBar3.Items.Count);
                    Assert.AreSame(selectorBar3.Items[0], selectorBar3.SelectedItem);
                    Assert.AreEqual(5, CountItems(itemsView3.ItemsSource));

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
        public void PivotSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Pivot"));
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
                    Assert.AreEqual("A basic pivot.", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<Pivot Title=\"EMAIL\">");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<PivotItem Header=\"Unread\">");
                    StringAssert.Contains(page.Examples[0].XamlCode, "urgent emails go here.");

                    var pivot = (TabControl)FindByAutomationId(page, "GallerySample_Pivot_Pivot");
                    Assert.IsNotNull(pivot);
                    Assert.AreEqual("Pivot1", pivot.Name);
                    Assert.AreEqual("EMAIL", ModernWpf.Controls.Primitives.PivotHelper.GetTitle(pivot));
                    Assert.AreEqual(400, pivot.MinHeight);
                    Assert.AreEqual(721, pivot.MaxWidth);
                    Assert.AreSame(pivot.TryFindResource("TabControlPivotStyle"), pivot.Style);
                    Assert.AreEqual(4, pivot.Items.Count);
                    Assert.AreEqual(0, pivot.SelectedIndex);
                    AssertPivotItem((TabItem)pivot.Items[0], "All", "all emails go here.");
                    AssertPivotItem((TabItem)pivot.Items[1], "Unread", "unread emails go here.");
                    AssertPivotItem((TabItem)pivot.Items[2], "Flagged", "flagged emails go here.");
                    AssertPivotItem((TabItem)pivot.Items[3], "Urgent", "urgent emails go here.");
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
        public void TabViewSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("TabView"));
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

                    Assert.AreEqual(10, page.Examples.Count);
                    Assert.AreEqual("A TabView with support for adding, closing, and rearranging tabs", page.Examples[0].HeaderText);
                    Assert.AreEqual("A TabView with TabViewItems defined in markup", page.Examples[1].HeaderText);
                    Assert.AreEqual("A TabView bound to a collection of MyData objects", page.Examples[2].HeaderText);
                    Assert.AreEqual("A TabView with keyboarding support", page.Examples[3].HeaderText);
                    Assert.AreEqual("You can put custom content in TabStripHeader and TabStripFooter", page.Examples[4].HeaderText);
                    Assert.AreEqual("Tab widths can either be equally sized, sized to the content of the tab, or sized to only show the icon when unselected", page.Examples[5].HeaderText);
                    Assert.AreEqual("The close button can be persistent or only visible on hover", page.Examples[6].HeaderText);
                    Assert.AreEqual("TabView with color tab icons", page.Examples[7].HeaderText);
                    Assert.AreEqual("A TabView with accent colored TabStrip background", page.Examples[8].HeaderText);
                    Assert.AreEqual("Complete TabView windowing sample", page.Examples[9].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "TabView_AddButtonClick");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "private TabViewItem CreateNewTab");
                    StringAssert.Contains(page.Examples[3].XamlCode, "KeyboardAccelerator Key=\"Number9\"");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "NewTabKeyboardAccelerator_Invoked");
                    StringAssert.Contains(page.Examples[7].XamlCode, "BitmapIconSource UriSource=\"/Assets/SampleMedia/linux.png\"");
                    StringAssert.Contains(page.Examples[9].XamlCode, "TabViewWindowingSamplePage.xaml");

                    var tabView1Host = (Grid)FindByAutomationId(page, "GallerySample_TabView_TabView");
                    var tabView1 = FindNamedDescendant<TabControl>(page, "TabView1");
                    var tabViewItemsSourceSample = FindNamedDescendant<TabControl>(page, "TabViewItemsSourceSample");
                    var tabView2 = FindNamedDescendant<TabControl>(page, "TabView2");
                    var tabView3 = FindNamedDescendant<TabControl>(page, "TabView3");
                    var tabView4 = FindNamedDescendant<TabControl>(page, "TabView4");
                    var tabViewColorIcons = FindNamedDescendant<TabControl>(page, "TabViewColorIconsSample");
                    var tabViewAccent = FindNamedDescendant<TabControl>(page, "TabViewAccentSample");
                    var widthModeCombo = FindNamedDescendant<ComboBox>(page, "TabWidthBehaviorComboBox");
                    var closeModeCombo = FindNamedDescendant<ComboBox>(page, "TabCloseButtonOverlayModeComboBox");
                    var windowingButton = FindNamedDescendant<Button>(page, "TabViewWindowingButton");
                    Assert.IsNotNull(tabView1Host);
                    Assert.IsNotNull(tabView1);
                    Assert.IsNotNull(tabViewItemsSourceSample);
                    Assert.IsNotNull(tabView2);
                    Assert.IsNotNull(tabView3);
                    Assert.IsNotNull(tabView4);
                    Assert.IsNotNull(tabViewColorIcons);
                    Assert.IsNotNull(tabViewAccent);
                    Assert.IsNotNull(widthModeCombo);
                    Assert.IsNotNull(closeModeCombo);
                    Assert.IsNotNull(windowingButton);

                    Assert.AreEqual("TabView1Host", tabView1Host.Name);
                    Assert.AreEqual(475, tabView1Host.MinHeight);
                    Assert.AreEqual(767, tabView1Host.MaxWidth);
                    Assert.AreEqual("TabView1", tabView1.Name);
                    Assert.AreEqual(475, tabView1.MinHeight);
                    Assert.AreEqual(767, tabView1.MaxWidth);
                    Assert.AreSame(tabView1.TryFindResource("DefaultTabControlStyle"), tabView1.Style);
                    AssertTabItem((TabItem)tabView1.Items[0], "Document 0");
                    AssertTabItem((TabItem)tabView1.Items[1], "Document 1");
                    AssertTabItem((TabItem)tabView1.Items[2], "Document 2");
                    AssertTabViewSamplePage((TabItem)tabView1.Items[0], 3, 4);
                    AssertTabViewSamplePage((TabItem)tabView1.Items[1], 2, 2);
                    AssertTabViewSamplePage((TabItem)tabView1.Items[2], 3, 4);

                    var addButton = FindNamedDescendant<Button>(page, "TabView1AddButton");
                    var closeButton = FindNamedDescendant<Button>(page, "TabView1CloseButton");
                    Assert.IsNotNull(addButton);
                    Assert.IsNotNull(closeButton);
                    addButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(4, tabView1.Items.Count);
                    Assert.AreEqual(3, tabView1.SelectedIndex);
                    closeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(3, tabView1.Items.Count);

                    Assert.AreEqual(3, tabViewItemsSourceSample.Items.Count);
                    var dataAddButton = FindNamedDescendant<Button>(page, "TabViewItemsSourceSampleAddButton");
                    var dataCloseButton = FindNamedDescendant<Button>(page, "TabViewItemsSourceSampleCloseButton");
                    dataAddButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(4, tabViewItemsSourceSample.Items.Count);
                    dataCloseButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(3, tabViewItemsSourceSample.Items.Count);

                    Assert.AreEqual(3, tabView2.Items.Count);
                    Assert.AreEqual("TabWidthBehavior", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(widthModeCombo));
                    widthModeCombo.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(160, ((TabItem)tabView3.Items[0]).Width);
                    widthModeCombo.SelectedIndex = 2;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(48, ((TabItem)tabView3.Items[0]).Width);

                    Assert.AreEqual("TabViewItem CloseButtonOverlayMode", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(closeModeCombo));
                    Assert.AreEqual(1, closeModeCombo.SelectedIndex);
                    Assert.AreEqual("CloseButtonOverlayMode=Always", AutomationProperties.GetHelpText((TabItem)tabView4.Items[0]));
                    Assert.AreEqual(3, tabViewColorIcons.Items.Count);
                    Assert.AreEqual(3, tabViewAccent.Items.Count);
                    Assert.AreEqual("Click here to launch the sample", windowingButton.Content);
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

                    var contentRoot = FindNamedDescendant<Border>(infoBar, "ContentRoot");
                    Assert.IsNotNull(contentRoot);
                    Assert.AreEqual(Visibility.Visible, contentRoot.Visibility);
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
                    Assert.AreEqual("infoBadge1", infoBadge1.Name);
                    Assert.AreEqual(5, infoBadge1.Value);
                    Assert.AreEqual(1.0, infoBadge1.Opacity);
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
        public void PipsPagerSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("PipsPager"));
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
                    Assert.AreEqual("PipsPager integrated with a FlipView", page.Examples[0].HeaderText);
                    Assert.AreEqual("PipsPager with options to change its orientation and button visibility.", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"FlipViewPipsPager\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "NumberOfPages=\"{x:Bind Pictures.Count}\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "Orientation=\"$(Orientation)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "PreviousButtonVisibility=\"$(PrevButton)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "NextButtonVisibility=\"$(NextButton)\"");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var flipViewPipsPagerHost = (Border)FindByAutomationId(page, "GallerySample_PipsPager_PipsPager");
                    var flipViewPipsPager = FindNamedDescendant<Mux.PipsPager>(page, "FlipViewPipsPager");
                    var gallery = FindNamedDescendant<ContentControl>(page, "Gallery");
                    var optionsPipsPager = FindNamedDescendant<Mux.PipsPager>(page, "TestPipsPager2");
                    var orientationComboBox = FindNamedDescendant<ComboBox>(page, "OrientationComboBox");
                    var previousButtonComboBox = FindNamedDescendant<ComboBox>(page, "PrevButtonComboBox");
                    var nextButtonComboBox = FindNamedDescendant<ComboBox>(page, "NextButtonComboBox");
                    Assert.IsNotNull(flipViewPipsPagerHost);
                    Assert.IsNotNull(flipViewPipsPager);
                    Assert.IsNotNull(gallery);
                    Assert.IsNotNull(optionsPipsPager);
                    Assert.IsNotNull(orientationComboBox);
                    Assert.IsNotNull(previousButtonComboBox);
                    Assert.IsNotNull(nextButtonComboBox);

                    Assert.AreEqual("FlipViewPipsPager", flipViewPipsPager.Name);
                    Assert.AreEqual(8, flipViewPipsPager.NumberOfPages);
                    Assert.AreEqual(0, flipViewPipsPager.SelectedPageIndex);
                    Assert.AreEqual(HorizontalAlignment.Center, flipViewPipsPager.HorizontalAlignment);
                    Assert.AreEqual(HorizontalAlignment.Center, flipViewPipsPagerHost.HorizontalAlignment);
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), flipViewPipsPagerHost.Margin);
                    Assert.AreSame(flipViewPipsPager, flipViewPipsPagerHost.Child);
                    Assert.AreEqual("Gallery", gallery.Name);
                    Assert.AreEqual(400.0, gallery.Width);
                    Assert.AreEqual(270.0, gallery.Height);
                    AssertPipsPagerImage(gallery, "LandscapeImage1.jpg");

                    flipViewPipsPager.SelectedPageIndex = 2;
                    WpfTestHost.DoEvents();
                    AssertPipsPagerImage(gallery, "LandscapeImage3.jpg");

                    Assert.AreEqual("TestPipsPager2", optionsPipsPager.Name);
                    Assert.AreEqual(10, optionsPipsPager.NumberOfPages);
                    Assert.AreEqual(Orientation.Horizontal, optionsPipsPager.Orientation);
                    Assert.AreEqual(Mux.PipsPagerButtonVisibility.Visible, optionsPipsPager.PreviousButtonVisibility);
                    Assert.AreEqual(Mux.PipsPagerButtonVisibility.Visible, optionsPipsPager.NextButtonVisibility);
                    AssertPipsPagerComboBox(orientationComboBox, "Orientation", "Horizontal", "Vertical");
                    AssertPipsPagerComboBox(previousButtonComboBox, "Previous Button Visibility", "Visible", "VisibleOnPointerOver", "Collapsed");
                    AssertPipsPagerComboBox(nextButtonComboBox, "Next Button Visibility", "Visible", "VisibleOnPointerOver", "Collapsed");

                    orientationComboBox.SelectedItem = "Vertical";
                    previousButtonComboBox.SelectedItem = "Collapsed";
                    nextButtonComboBox.SelectedItem = "VisibleOnPointerOver";
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(Orientation.Vertical, optionsPipsPager.Orientation);
                    Assert.AreEqual(Mux.PipsPagerButtonVisibility.Collapsed, optionsPipsPager.PreviousButtonVisibility);
                    Assert.AreEqual(Mux.PipsPagerButtonVisibility.VisibleOnPointerOver, optionsPipsPager.NextButtonVisibility);
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
        public void PullToRefreshSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("PullToRefresh"));
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
                    Assert.AreEqual("Basic PullToRefresh", page.Examples[0].HeaderText);
                    Assert.AreEqual("Custom Icon PullToRefresh", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<RefreshContainer x:Name=\"rc\" RefreshRequested=\"rc_RefreshRequested\">");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "RefreshCompletionDeferral.Complete()");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<RefreshVisualizer RefreshStateChanged=\"rv2_RefreshStateChanged\">");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<SymbolIcon Symbol=\"AddFriend\"/>");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "ElementCompositionPreview.GetElementVisual(rv2.Content)");

                    var refreshContainer = (Mux.RefreshContainer)FindByAutomationId(page, "GallerySample_PullToRefresh_RefreshContainer");
                    var listView = FindNamedDescendant<ListView>(page, "lv");
                    var customRefreshContainer = FindNamedDescendant<Mux.RefreshContainer>(page, "rc2");
                    var customListView = FindNamedDescendant<ListView>(page, "lv2");
                    var visualizer = FindNamedDescendant<Mux.RefreshVisualizer>(page, "rv2");
                    Assert.IsNotNull(refreshContainer);
                    Assert.IsNotNull(listView);
                    Assert.IsNotNull(customRefreshContainer);
                    Assert.IsNotNull(customListView);
                    Assert.IsNotNull(visualizer);
                    Assert.IsInstanceOfType(visualizer.Content, typeof(Image));

                    Assert.AreEqual("rc", refreshContainer.Name);
                    Assert.AreEqual(HorizontalAlignment.Center, refreshContainer.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, refreshContainer.VerticalAlignment);
                    Assert.AreEqual("lv", listView.Name);
                    Assert.AreEqual(200.0, listView.Height);
                    Assert.AreEqual(200.0, listView.MinWidth);
                    Assert.AreEqual(9, listView.Items.Count);
                    Assert.AreEqual("AcrylicBrush", listView.Items[0]);

                    refreshContainer.RequestRefresh();
                    WaitFor(() => listView.Items.Count == 10);
                    Assert.AreEqual("NewControl", listView.Items[0]);

                    Assert.AreEqual("rc2", customRefreshContainer.Name);
                    Assert.AreEqual("rv2", visualizer.Name);
                    Assert.AreEqual("lv2", customListView.Name);
                    Assert.AreEqual(8, customListView.Items.Count);
                    Assert.AreEqual("Mike", customListView.Items[0]);

                    customRefreshContainer.RequestRefresh();
                    WaitFor(() => customListView.Items.Count == 9);
                    Assert.AreEqual("New Friend", customListView.Items[0]);
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
        public void FlipViewSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("FlipView"));
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
                    Assert.AreEqual("A simple FlipView with items declared inline.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A FlipView showing bound data with a data template.", page.Examples[1].HeaderText);
                    Assert.AreEqual("Vertical FlipView", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "AutomationProperties.AutomationControlType=\"List\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "ms-appx:///Assets/SampleMedia/cliff.jpg");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ItemsSource=\"{x:Bind Items, Mode=OneWay}\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "ControlInfoDataItem");
                    StringAssert.Contains(page.Examples[2].XamlCode, "VirtualizingStackPanel Orientation=\"Vertical\"");

                    var flipView = (Grid)FindByAutomationId(page, "GallerySample_FlipView_FlipView");
                    var namedFlipView = FindNamedDescendant<Grid>(page, "FlipView1");
                    var flipView1Content = FindNamedDescendant<ContentControl>(page, "FlipView1Content");
                    var flipView1NextButton = FindNamedDescendant<Button>(page, "FlipView1NextButton");
                    var flipView2 = FindNamedDescendant<Grid>(page, "FlipView2");
                    var flipView2Content = FindNamedDescendant<ContentControl>(page, "FlipView2Content");
                    var flipView2NextButton = FindNamedDescendant<Button>(page, "FlipView2NextButton");
                    var flipView3 = FindNamedDescendant<Grid>(page, "FlipView3");
                    var flipView3Content = FindNamedDescendant<ContentControl>(page, "FlipView3Content");
                    var flipView3NextButton = FindNamedDescendant<Button>(page, "FlipView3NextButton");
                    Assert.IsNotNull(flipView);
                    Assert.AreSame(flipView, namedFlipView);
                    Assert.IsNotNull(flipView1Content);
                    Assert.IsNotNull(flipView1NextButton);
                    Assert.IsNotNull(flipView2);
                    Assert.IsNotNull(flipView2Content);
                    Assert.IsNotNull(flipView2NextButton);
                    Assert.IsNotNull(flipView3);
                    Assert.IsNotNull(flipView3Content);
                    Assert.IsNotNull(flipView3NextButton);
                    Assert.AreEqual(400.0, flipView.Width);
                    Assert.AreEqual(270.0, flipView.Height);
                    Assert.AreEqual(400.0, flipView2.Width);
                    Assert.AreEqual(180.0, flipView2.Height);
                    Assert.AreEqual("Cliff", AutomationProperties.GetName(flipView1Content));
                    Assert.AreEqual("Button", AutomationProperties.GetName(flipView2Content));
                    Assert.AreEqual("Cliff", AutomationProperties.GetName(flipView3Content));
                    Assert.AreEqual("Next", AutomationProperties.GetName(flipView1NextButton));
                    Assert.AreEqual("Next", AutomationProperties.GetName(flipView2NextButton));
                    Assert.AreEqual("Down", AutomationProperties.GetName(flipView3NextButton));

                    flipView1NextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, flipView1NextButton));
                    flipView2NextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, flipView2NextButton));
                    flipView3NextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, flipView3NextButton));
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Grapes", AutomationProperties.GetName(flipView1Content));
                    Assert.AreEqual("CalendarView", AutomationProperties.GetName(flipView2Content));
                    Assert.AreEqual("Grapes", AutomationProperties.GetName(flipView3Content));
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
        public void ItemsViewSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ItemsView"));
                var window = new Window
                {
                    Width = 1120,
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

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("Basic ItemsView", page.Examples[0].HeaderText);
                    Assert.AreEqual("ItemsView with swappable layouts", page.Examples[1].HeaderText);
                    Assert.AreEqual("ItemsView item invocation and selection", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Key=\"ImageTemplate\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsItemInvokedEnabled=\"True\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "BasicItemsView_ItemInvoked");
                    StringAssert.Contains(page.Examples[1].XamlCode, "LinedFlowLayout");
                    StringAssert.Contains(page.Examples[1].XamlCode, "MinItemSpacing=\"5\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "SelectionMode=\"$(SelectionMode)\"");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "SwappableSelectionModesItemsView_SelectionChanged");

                    var itemsView = (ListBox)FindByAutomationId(page, "GallerySample_ItemsView_ItemsView");
                    var basicItemsView = FindNamedDescendant<ListBox>(page, "BasicItemsView");
                    var basicOutput = FindNamedDescendant<TextBlock>(page, "tblBasicInvokeOutput");
                    var swappableLayoutsItemsView = FindNamedDescendant<ListBox>(page, "SwappableLayoutsItemsView");
                    var linedFlowOptions = FindNamedDescendant<StackPanel>(page, "spLinedFlowLayoutOptions");
                    var stackOptions = FindNamedDescendant<StackPanel>(page, "spStackLayoutOptions");
                    var uniformGridOptions = FindNamedDescendant<StackPanel>(page, "spUniformGridLayoutOptions");
                    var lineSpacing = FindNamedDescendant<Mux.NumberBox>(page, "nbLineSpacing");
                    var minItemSpacing = FindNamedDescendant<Mux.NumberBox>(page, "nbMinItemSpacing");
                    var smallLineHeight = FindNamedDescendant<RadioButton>(page, "rbSmallLineHeight");
                    var largeLineHeight = FindNamedDescendant<RadioButton>(page, "rbLargeLineHeight");
                    var rowSpacing = FindNamedDescendant<Mux.NumberBox>(page, "nbSpacing");
                    var minColumnSpacing = FindNamedDescendant<Mux.NumberBox>(page, "nbMinColumnSpacing");
                    var minRowSpacing = FindNamedDescendant<Mux.NumberBox>(page, "nbMinRowSpacing");
                    var maximumRowsOrColumns = FindNamedDescendant<Mux.NumberBox>(page, "nbMaximumRowsOrColumns");
                    var selectionItemsView = FindNamedDescendant<ListBox>(page, "SwappableSelectionModesItemsView");
                    var invocationOutput = FindNamedDescendant<TextBlock>(page, "tblInvocationOutput");
                    var selectionOutput = FindNamedDescendant<TextBlock>(page, "tblSelectionOutput");
                    var selectionMode = FindNamedDescendant<ComboBox>(page, "cmbSelectionMode");
                    var invocationEnabled = FindNamedDescendant<CheckBox>(page, "chkIsItemInvokedEnabled");
                    Assert.IsNotNull(itemsView);
                    Assert.AreSame(itemsView, basicItemsView);
                    Assert.IsNotNull(basicOutput);
                    Assert.IsNotNull(swappableLayoutsItemsView);
                    Assert.IsNotNull(linedFlowOptions);
                    Assert.IsNotNull(stackOptions);
                    Assert.IsNotNull(uniformGridOptions);
                    Assert.IsNotNull(lineSpacing);
                    Assert.IsNotNull(minItemSpacing);
                    Assert.IsNotNull(smallLineHeight);
                    Assert.IsNotNull(largeLineHeight);
                    Assert.IsNotNull(rowSpacing);
                    Assert.IsNotNull(minColumnSpacing);
                    Assert.IsNotNull(minRowSpacing);
                    Assert.IsNotNull(maximumRowsOrColumns);
                    Assert.IsNotNull(selectionItemsView);
                    Assert.IsNotNull(invocationOutput);
                    Assert.IsNotNull(selectionOutput);
                    Assert.IsNotNull(selectionMode);
                    Assert.IsNotNull(invocationEnabled);
                    Assert.AreEqual(13, basicItemsView.Items.Count);
                    Assert.AreEqual(220.0, basicItemsView.Width);
                    Assert.AreEqual(400.0, basicItemsView.Height);
                    Assert.AreEqual(Brushes.Transparent, basicItemsView.Background);
                    Assert.AreEqual(Brushes.Transparent, basicItemsView.BorderBrush);
                    Assert.AreEqual(500.0, swappableLayoutsItemsView.Width);
                    Assert.AreEqual(400.0, swappableLayoutsItemsView.Height);
                    Assert.AreEqual(500.0, selectionItemsView.Width);
                    Assert.AreEqual(400.0, selectionItemsView.Height);
                    Assert.AreEqual(SelectionMode.Multiple, selectionItemsView.SelectionMode);
                    Assert.AreEqual(2, selectionMode.SelectedIndex);
                    Assert.AreEqual(5.0, lineSpacing.Value);
                    Assert.AreEqual(5.0, minItemSpacing.Value);
                    Assert.IsTrue(largeLineHeight.IsChecked == true);
                    Assert.AreEqual(5.0, rowSpacing.Value);
                    Assert.AreEqual(5.0, minColumnSpacing.Value);
                    Assert.AreEqual(5.0, minRowSpacing.Value);
                    Assert.AreEqual(3.0, maximumRowsOrColumns.Value);
                    basicItemsView.UpdateLayout();
                    var firstItem = (ListBoxItem)basicItemsView.ItemContainerGenerator.ContainerFromIndex(0);
                    Assert.IsNotNull(firstItem);
                    Assert.AreEqual(200.0, firstItem.Width);
                    Assert.AreEqual(140.0, firstItem.Height);
                    Assert.AreEqual(new Thickness(0), firstItem.Margin);
                    Assert.AreEqual(new Thickness(0), firstItem.Padding);
                    Assert.AreEqual(new Thickness(0), firstItem.BorderThickness);

                    basicItemsView.SelectedIndex = 0;
                    WpfTestHost.DoEvents();
                    var enterArgs = new KeyEventArgs(
                        Keyboard.PrimaryDevice,
                        PresentationSource.FromVisual(basicItemsView),
                        0,
                        Key.Enter)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    basicItemsView.RaiseEvent(enterArgs);
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("You invoked Item 1.", basicOutput.Text);

                    selectionItemsView.SelectedIndex = 0;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("You have selected 1 item(s).", selectionOutput.Text);
                    invocationEnabled.IsChecked = true;
                    WpfTestHost.DoEvents();
                    var selectionEnterArgs = new KeyEventArgs(
                        Keyboard.PrimaryDevice,
                        PresentationSource.FromVisual(selectionItemsView),
                        0,
                        Key.Enter)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    selectionItemsView.RaiseEvent(selectionEnterArgs);
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("You invoked Item 1.", invocationOutput.Text);

                    var layoutOptions = FindDescendants<Mux.RadioButtons>(page)
                        .Find(candidate => string.Equals(candidate.Header as string, "Layout", StringComparison.Ordinal));
                    Assert.IsNotNull(layoutOptions);
                    layoutOptions.SelectedIndex = 2;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Collapsed, linedFlowOptions.Visibility);
                    Assert.AreEqual(Visibility.Visible, stackOptions.Visibility);
                    Assert.AreEqual(Visibility.Collapsed, uniformGridOptions.Visibility);
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
        public void DateAndCalendarExtensionSamplesMatchWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var window = new Window
                {
                    Width = 1120,
                    Height = 820,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                try
                {
                    var calendarDatePickerPage = ShowItemPage(window, "CalendarDatePicker");
                    Assert.AreEqual(1, calendarDatePickerPage.Examples.Count);
                    Assert.AreEqual("CalendarDatePicker with a header and placeholder text.", calendarDatePickerPage.Examples[0].HeaderText);
                    Assert.IsFalse(calendarDatePickerPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(calendarDatePickerPage.Examples[0].XamlCode, "CalendarDatePicker PlaceholderText=\"Pick a date\" Header=\"Calendar\"");
                    var calendarDatePicker = (DatePicker)FindByAutomationId(calendarDatePickerPage, "GallerySample_CalendarDatePicker_CalendarDatePicker");
                    Assert.AreSame(calendarDatePicker, FindNamedDescendant<DatePicker>(calendarDatePickerPage, "CalendarDatePicker1"));
                    Assert.AreEqual("Calendar", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(calendarDatePicker));
                    Assert.AreEqual("Pick a date", ModernWpf.Controls.Primitives.ControlHelper.GetPlaceholderText(calendarDatePicker));

                    var calendarViewPage = ShowItemPage(window, "CalendarView");
                    Assert.AreEqual(1, calendarViewPage.Examples.Count);
                    Assert.AreEqual("A basic calendar view.", calendarViewPage.Examples[0].HeaderText);
                    Assert.IsFalse(calendarViewPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(calendarViewPage.Examples[0].XamlCode, "SelectionMode=\"$(SelectionMode)\"");
                    StringAssert.Contains(calendarViewPage.Examples[0].XamlCode, "CalendarIdentifier=\"$(CalendarIdentifier)\"");
                    var calendarView = (System.Windows.Controls.Calendar)FindByAutomationId(calendarViewPage, "GallerySample_CalendarView_CalendarView");
                    Assert.AreSame(calendarView, FindNamedDescendant<System.Windows.Controls.Calendar>(calendarViewPage, "Control1"));
                    var groupLabel = FindNamedDescendant<CheckBox>(calendarViewPage, "isGroupLabelVisible");
                    var outOfScope = FindNamedDescendant<CheckBox>(calendarViewPage, "isOutOfScopeEnabled");
                    var selectionMode = FindNamedDescendant<ComboBox>(calendarViewPage, "selectionMode");
                    var calendarIdentifier = FindNamedDescendant<ComboBox>(calendarViewPage, "calendarIdentifier");
                    var calendarLanguages = FindNamedDescendant<ComboBox>(calendarViewPage, "calendarLanguages");
                    Assert.IsNotNull(groupLabel);
                    Assert.IsNotNull(outOfScope);
                    Assert.IsNotNull(selectionMode);
                    Assert.IsNotNull(calendarIdentifier);
                    Assert.IsNotNull(calendarLanguages);
                    Assert.AreEqual(CalendarSelectionMode.SingleDate, calendarView.SelectionMode);
                    Assert.AreEqual("Single", selectionMode.SelectedItem);
                    Assert.AreEqual("GregorianCalendar", calendarIdentifier.SelectedItem);
                    Assert.AreEqual(0, calendarLanguages.SelectedIndex);
                    selectionMode.SelectedItem = "Multiple";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(CalendarSelectionMode.MultipleRange, calendarView.SelectionMode);
                    outOfScope.IsChecked = false;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(calendarView.DisplayDateStart.HasValue);
                    Assert.IsTrue(calendarView.DisplayDateEnd.HasValue);

                    var timePickerPage = ShowItemPage(window, "TimePicker");
                    Assert.AreEqual(3, timePickerPage.Examples.Count);
                    Assert.AreEqual("A simple TimePicker.", timePickerPage.Examples[0].HeaderText);
                    Assert.AreEqual("A TimePicker with a header and minute increments specified.", timePickerPage.Examples[1].HeaderText);
                    Assert.AreEqual("A TimePicker using a 24-hour clock, initialized to current time.", timePickerPage.Examples[2].HeaderText);
                    Assert.IsFalse(timePickerPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(timePickerPage.Examples[0].XamlCode, "<TimePicker/>");
                    StringAssert.Contains(timePickerPage.Examples[1].XamlCode, "MinuteIncrement=\"15\"");
                    StringAssert.Contains(timePickerPage.Examples[2].XamlCode, "ClockIdentifier=\"24HourClock\"");
                    var timePicker = (StackPanel)FindByAutomationId(timePickerPage, "GallerySample_TimePicker_TimePicker");
                    Assert.AreSame(timePicker, FindNamedDescendant<StackPanel>(timePickerPage, "TimePicker1"));
                    var timePicker1Hour = FindNamedDescendant<ComboBox>(timePickerPage, "TimePicker1HourComboBox");
                    var timePicker1Minute = FindNamedDescendant<ComboBox>(timePickerPage, "TimePicker1MinuteComboBox");
                    var timePicker1Period = FindNamedDescendant<ComboBox>(timePickerPage, "TimePicker1PeriodComboBox");
                    var timePicker2Minute = FindNamedDescendant<ComboBox>(timePickerPage, "TimePicker2MinuteComboBox");
                    var timePicker3Hour = FindNamedDescendant<ComboBox>(timePickerPage, "TimePicker3HourComboBox");
                    Assert.IsNotNull(timePicker1Hour);
                    Assert.IsNotNull(timePicker1Minute);
                    Assert.IsNotNull(timePicker1Period);
                    Assert.IsNotNull(timePicker2Minute);
                    Assert.IsNotNull(timePicker3Hour);
                    Assert.AreEqual("9", timePicker1Hour.SelectedItem);
                    Assert.AreEqual("30", timePicker1Minute.SelectedItem);
                    Assert.AreEqual("AM", timePicker1Period.SelectedItem);
                    Assert.AreEqual(4, timePicker2Minute.Items.Count);
                    Assert.AreEqual(24, timePicker3Hour.Items.Count);
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
                    var namedBasicGridView = FindNamedDescendant<Mux.GridView>(page, "BasicGridView");
                    var clickOutput0 = FindNamedDescendant<TextBlock>(page, "ClickOutput0");
                    Assert.IsNotNull(basicGridView);
                    Assert.AreSame(basicGridView, namedBasicGridView);
                    Assert.IsNotNull(clickOutput0);
                    Assert.AreEqual(8, basicGridView.Items.Count);
                    Assert.IsTrue(basicGridView.IsItemClickEnabled);
                    Assert.AreEqual(SelectionMode.Single, basicGridView.SelectionMode);
                    Assert.IsNotNull(basicGridView.ItemTemplate);

                    WaitFor(() => basicGridView.ItemContainerGenerator.ContainerFromIndex(0) != null);
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
                    Assert.AreEqual(8, contentGridView.Items.Count);
                    Assert.IsFalse(contentGridView.IsItemClickEnabled);
                    Assert.AreEqual(FlowDirection.LeftToRight, contentGridView.FlowDirection);
                    Assert.AreEqual(SelectionMode.Single, contentGridView.SelectionMode);
                    Assert.IsTrue(contentGridView.IsSelectionEnabled);

                    WaitFor(() => contentGridView.ItemContainerGenerator.ContainerFromIndex(0) != null);
                    itemClickCheckBox.IsChecked = true;
                    itemClickCheckBox.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, itemClickCheckBox));
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
                    StringAssert.Contains(page.Examples[1].XamlCode, "ItemsSource=\"{x:Bind NumberedItems}\"");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "public class MyDataTemplateSelector");
                    StringAssert.Contains(page.Examples[2].XamlCode, "x:Name=\"MixedTypeRepeater\"");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "StringOrIntTemplateSelector");
                    StringAssert.Contains(page.Examples[3].XamlCode, "x:Name=\"outerRepeater\"");
                    StringAssert.Contains(page.Examples[4].XamlCode, "x:Name=\"animatedScrollRepeater\"");
                    StringAssert.Contains(page.Examples[4].CSharpCode, "OnElementPrepared");
                    StringAssert.Contains(page.Examples[5].XamlCode, "x:Name=\"VariedImageSizeRepeater\"");
                    StringAssert.Contains(page.Examples[5].CSharpCode, "public class Recipe");

                    var repeater = (Mux.ItemsRepeater)FindByAutomationId(page, "GallerySample_ItemsRepeater_ItemsRepeater");
                    var namedRepeater = FindNamedDescendant<Mux.ItemsRepeater>(page, "repeater");
                    var addButton = FindNamedDescendant<Button>(page, "AddBtn");
                    var deleteButton = FindNamedDescendant<Button>(page, "DeleteBtn");
                    var horizontalStack = FindNamedDescendant<RadioButton>(page, "HStackBtn");
                    var uniformGrid = FindNamedDescendant<RadioButton>(page, "HGridBtn");
                    Assert.IsNotNull(repeater);
                    Assert.AreSame(repeater, namedRepeater);
                    Assert.IsNotNull(addButton);
                    Assert.IsNotNull(deleteButton);
                    Assert.IsNotNull(horizontalStack);
                    Assert.IsNotNull(uniformGrid);
                    Assert.AreEqual(3, CountItems(repeater.ItemsSource));
                    Assert.IsInstanceOfType(repeater.Layout, typeof(Mux.StackLayout));
                    Assert.AreEqual(Orientation.Vertical, ((Mux.StackLayout)repeater.Layout).Orientation);

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
                    Assert.AreEqual(20, CountItems(animatedRepeater.ItemsSource));
                    Assert.AreEqual(120, CountItems(recipeRepeater.ItemsSource));
                    Assert.IsInstanceOfType(mixedRepeater.Layout, typeof(Mux.UniformGridLayout));
                    Assert.IsInstanceOfType(recipeRepeater.Layout, typeof(Mux.UniformGridLayout));

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
                    StringAssert.Contains(page.Examples[1].XamlCode, "KeyboardAccelerator");
                    StringAssert.Contains(page.Examples[2].XamlCode, "RadioMenuFlyoutItem");

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
                    Assert.AreEqual("Open...", ((MenuItem)simpleFile.Items[1]).Header);
                    var selectedOptionText = FindNamedDescendant<TextBlock>(page, "SelectedOptionText");
                    Assert.IsNotNull(selectedOptionText);
                    ((MenuItem)simpleFile.Items[1]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    Assert.AreEqual("You clicked: Open...", selectedOptionText.Text);

                    var keyboardFile = keyboardMenu.Items[0];
                    var newItem = (MenuItem)keyboardFile.Items[0];
                    Assert.AreEqual("New", newItem.Header);
                    Assert.AreEqual("Ctrl+N", newItem.InputGestureText);

                    var submenuFile = submenuMenu.Items[0];
                    Assert.IsInstanceOfType(submenuFile.Items[0], typeof(MenuItem));
                    Assert.AreEqual(3, ((MenuItem)submenuFile.Items[0]).Items.Count);
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
        public void SwipeControlSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("SwipeControl"));
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

                    Assert.AreEqual(5, page.Examples.Count);
                    Assert.AreEqual("Swipe right to reveal actions", page.Examples[0].HeaderText);
                    Assert.AreEqual("Swipe left to invoke an execute", page.Examples[1].HeaderText);
                    Assert.AreEqual("Custom Swipe in a ListView", page.Examples[2].HeaderText);
                    Assert.AreEqual("Gradient Background", page.Examples[3].HeaderText);
                    Assert.AreEqual("Custom icons", page.Examples[4].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "Accept_ItemInvoked");
                    StringAssert.Contains(page.Examples[1].XamlCode, "BehaviorOnInvoked=\"Close\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "DeleteItem_ItemInvoked");
                    StringAssert.Contains(page.Examples[3].XamlCode, "PurpleGradient");
                    StringAssert.Contains(page.Examples[4].XamlCode, "CoffeeCup.png");

                    var swipeRight = (Mux.SwipeControl)FindByAutomationId(page, "GallerySample_SwipeControl_SwipeControl");
                    Assert.IsNotNull(swipeRight);
                    Assert.AreEqual(2, swipeRight.LeftItems.Count);
                    Assert.AreEqual(Mux.SwipeMode.Reveal, swipeRight.LeftItems.Mode);
                    Assert.AreEqual("Accept", swipeRight.LeftItems[0].Text);
                    Assert.AreEqual("Flag", swipeRight.LeftItems[1].Text);
                    var swipeRightText = (TextBlock)swipeRight.Content;
                    InvokeSwipeItem(swipeRight.LeftItems[0], swipeRight);
                    Assert.AreEqual("Swipe Right - Accepted", swipeRightText.Text);
                    Assert.AreEqual("Cancel", swipeRight.LeftItems[0].Text);
                    InvokeSwipeItem(swipeRight.LeftItems[1], swipeRight);
                    Assert.AreEqual("Swipe Right - Accepted & Flagged", swipeRightText.Text);
                    Assert.AreEqual("Unmark", swipeRight.LeftItems[1].Text);

                    var swipeControls = FindDescendants<Mux.SwipeControl>(page);
                    Assert.IsTrue(swipeControls.Count >= 4);
                    var swipeLeft = swipeControls[1];
                    Assert.AreEqual(Mux.SwipeMode.Execute, swipeLeft.RightItems.Mode);
                    Assert.AreEqual("Archive", swipeLeft.RightItems[0].Text);
                    Assert.AreEqual(Mux.SwipeBehaviorOnInvoked.Close, swipeLeft.RightItems[0].BehaviorOnInvoked);
                    var swipeLeftText = (TextBlock)swipeLeft.Content;
                    InvokeSwipeItem(swipeLeft.RightItems[0], swipeLeft);
                    Assert.AreEqual("Archived - Swipe Left", swipeLeftText.Text);

                    var listView = (ListView)FindByAutomationId(page, "GallerySample_SwipeControl_ListView");
                    Assert.IsNotNull(listView);
                    Assert.AreEqual("lv", listView.Name);
                    Assert.AreEqual(800.0, listView.Width);
                    Assert.AreEqual(300.0, listView.Height);
                    Assert.AreEqual(4, listView.Items.Count);
                    var listSwipe = FindNamedDescendant<Mux.SwipeControl>(listView, "ListViewSwipeContainer");
                    Assert.IsNotNull(listSwipe);
                    Assert.AreEqual("Reply All", listSwipe.LeftItems[0].Text);
                    Assert.AreEqual("Open", listSwipe.LeftItems[1].Text);
                    Assert.AreEqual("Delete", listSwipe.RightItems[0].Text);
                    InvokeSwipeItem(listSwipe.RightItems[0], listSwipe);
                    Assert.AreEqual(3, listView.Items.Count);

                    var gradientSwipe = swipeControls[swipeControls.Count - 2];
                    Assert.AreEqual("Lock", gradientSwipe.RightItems[0].Text);
                    Assert.IsInstanceOfType(gradientSwipe.RightItems[0].Background, typeof(LinearGradientBrush));

                    var customIconSwipe = swipeControls[swipeControls.Count - 1];
                    Assert.AreEqual("Coffee", customIconSwipe.LeftItems[0].Text);
                    var bitmapIconSource = customIconSwipe.LeftItems[0].IconSource as Mux.BitmapIconSource;
                    Assert.IsNotNull(bitmapIconSource);
                    StringAssert.Contains(bitmapIconSource.UriSource.ToString(), "CoffeeCup.png");
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
                    var sortFlyout = sortButton.Flyout as Mux.MenuFlyout;
                    Assert.IsNotNull(sortFlyout);
                    Assert.AreEqual(3, sortFlyout.Items.Count);
                    var ratingItem = (MenuItem)sortFlyout.Items[0];
                    Assert.AreEqual("By rating", ratingItem.Header);
                    Assert.AreEqual("rating", ratingItem.Tag);
                    ratingItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    Assert.AreEqual("Sort by: rating", control1Output.Text);

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
                    Assert.AreEqual("Ctrl+S", ((MenuItem)keyboardFlyout.Items[0]).InputGestureText);
                    Assert.AreEqual("Ctrl+C", ((MenuItem)keyboardFlyout.Items[1]).InputGestureText);
                    Assert.AreEqual("Delete", ((MenuItem)keyboardFlyout.Items[2]).InputGestureText);

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
        public void CommandSamplesMatchWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var standardCommandPage = new ItemPage(GalleryCatalog.FindItem("StandardUICommand"));
                var xamlCommandPage = new ItemPage(GalleryCatalog.FindItem("XamlUICommand"));
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
                    window.Content = standardCommandPage;
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, standardCommandPage.Examples.Count);
                    Assert.AreEqual("Exposing a command in multiple controls using StandardUICommand", standardCommandPage.Examples[0].HeaderText);
                    Assert.IsFalse(standardCommandPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(standardCommandPage.Examples[0].XamlCode, "DeleteSwipeItem");
                    StringAssert.Contains(standardCommandPage.Examples[0].XamlCode, "HoverButton");
                    StringAssert.Contains(standardCommandPage.Examples[0].CSharpCode, "StandardUICommandKind.Delete");

                    var standardRoot = (FrameworkElement)FindByAutomationId(standardCommandPage, "GallerySample_StandardUICommand_Root");
                    var listView = (ListView)FindByAutomationId(standardCommandPage, "GallerySample_StandardUICommand_ListView");
                    var namedListView = FindNamedDescendant<ListView>(standardCommandPage, "ListViewRight");
                    var menu = FindNamedDescendant<Mux.MenuBar>(standardCommandPage, "StandardUICommandMenuBar");
                    Assert.IsNotNull(standardRoot);
                    Assert.AreSame(listView, namedListView);
                    Assert.IsNotNull(menu);
                    var editMenu = (Mux.MenuBarItem)menu.Items[1];
                    var deleteFlyoutItem = (MenuItem)editMenu.Items[0];
                    Assert.AreEqual("DeleteFlyoutItem", deleteFlyoutItem.Name);
                    Assert.IsNotNull(deleteFlyoutItem.Command);
                    Assert.AreEqual("Delete", deleteFlyoutItem.InputGestureText);
                    Assert.AreEqual(500.0, listView.Height);
                    Assert.AreEqual(15, listView.Items.Count);
                    Assert.AreEqual("List item 0", GetCommandListItemText(listView.Items[0]));
                    Assert.IsNotNull(listView.ItemTemplate);
                    var firstContainer = listView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                    Assert.IsNotNull(firstContainer);
                    Assert.IsNotNull(firstContainer.ContextMenu);
                    Assert.AreEqual(1, firstContainer.ContextMenu.Items.Count);

                    listView.SelectedIndex = 1;
                    deleteFlyoutItem.Command.Execute(null);
                    Assert.AreEqual(14, listView.Items.Count);
                    Assert.AreEqual("List item 2", GetCommandListItemText(listView.Items[1]));

                    window.Content = xamlCommandPage;
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(1, xamlCommandPage.Examples.Count);
                    Assert.AreEqual("Creating a reusable command with XamlUICommand", xamlCommandPage.Examples[0].HeaderText);
                    Assert.IsFalse(xamlCommandPage.HasAdditionalSampleSnippets);
                    StringAssert.Contains(xamlCommandPage.Examples[0].XamlCode, "CustomXamlUICommand");
                    StringAssert.Contains(xamlCommandPage.Examples[0].XamlCode, "SymbolIconSource Symbol=\"Favorite\"");
                    StringAssert.Contains(xamlCommandPage.Examples[0].CSharpCode, "You fired the custom command");

                    var customButton = (Mux.AppBarButton)FindByAutomationId(xamlCommandPage, "GallerySample_XamlUICommand_AppBarButton");
                    var namedCustomButton = FindNamedDescendant<Mux.AppBarButton>(xamlCommandPage, "CustomButton");
                    var output = FindNamedDescendant<TextBlock>(xamlCommandPage, "XamlUICommandOutput");
                    Assert.AreSame(customButton, namedCustomButton);
                    Assert.AreEqual("Custom XamlUICommand", customButton.Label);
                    Assert.AreEqual(Mux.Symbol.Favorite, ((Mux.SymbolIcon)customButton.Icon).Symbol);
                    Assert.AreEqual("Ctrl+D", customButton.InputGestureText);
                    Assert.IsNotNull(output);

                    customButton.Command.Execute(null);
                    Assert.AreEqual("You fired the custom command", output.Text);
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
                    var richTextBox = FindNamedDescendant<RichTextBox>(page, "myRichEditBox");
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
                    var richTextBox = FindNamedDescendant<RichTextBox>(page, "myRichEditBox");
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
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.AreEqual("<HyperlinkButton Content=\"Microsoft home page\" NavigateUri=\"https://www.microsoft.com\" $(IsEnabled)/>", page.Examples[0].XamlCode);
                    Assert.AreEqual("<HyperlinkButton Content=\"ToggleButton\" Click=\"HyperlinkButton_Click\"/>", page.Examples[1].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var uriButton = (Mux.HyperlinkButton)FindByAutomationId(page, "GallerySample_HyperlinkButton_HyperlinkButton");
                    var clickButton = (Mux.HyperlinkButton)FindByAutomationId(page, "GallerySample_HyperlinkButton_ClickHyperlinkButton");
                    Assert.IsNotNull(uriButton);
                    Assert.IsNotNull(clickButton);

                    Assert.AreEqual("Control1", uriButton.Name);
                    Assert.AreEqual("Microsoft home page", uriButton.Content);
                    Assert.AreEqual("https://www.microsoft.com/", uriButton.NavigateUri.ToString());
                    Assert.AreEqual("Control2", clickButton.Name);
                    Assert.AreEqual("Go to ToggleButton", clickButton.Content);
                    Assert.IsNull(clickButton.NavigateUri);

                    clickButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, clickButton));
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

                    moreButtonCheck.IsChecked = true;
                    colorSliderCheck.IsChecked = false;
                    colorChannelInputCheck.IsChecked = false;
                    hexInputCheck.IsChecked = false;
                    alphaCheck.IsChecked = true;
                    alphaSliderCheck.IsChecked = false;
                    alphaTextInputCheck.IsChecked = false;
                    shapeRadioButtons.SelectedIndex = 1;
                    colorPicker.Color = Color.FromRgb(51, 102, 204);
                    WpfTestHost.DoEvents();

                    Assert.IsTrue(colorPicker.IsMoreButtonVisible);
                    Assert.IsFalse(colorPicker.IsColorSliderVisible);
                    Assert.IsFalse(colorPicker.IsColorChannelTextInputVisible);
                    Assert.IsFalse(colorPicker.IsHexInputVisible);
                    Assert.IsTrue(colorPicker.IsAlphaEnabled);
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
                    var firstLayout = (Grid)firstExampleRoot.Children[0];
                    var output = (TextBlock)firstLayout.Children[1];
                    Assert.AreEqual(FontWeights.Bold, output.FontWeight);
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), output.Margin);
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
                    Assert.AreEqual(-1, placeholderRating.PlaceholderValue);
                    Assert.AreEqual("slider", slider.Name);
                    Assert.AreEqual(0, slider.Minimum);
                    Assert.AreEqual(5, slider.Maximum);
                    Assert.AreEqual(0.5, slider.SmallChange);
                    Assert.AreEqual(0.5, slider.TickFrequency);

                    slider.Value = 2.5;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(2.5, placeholderRating.PlaceholderValue);
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
                    Assert.AreEqual("Control output", AutomationProperties.GetName(output));
                    Assert.AreEqual(AutomationLiveSetting.Polite, AutomationProperties.GetLiveSetting(output));
                    Assert.AreEqual(string.Empty, output.Text);

                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
                    Assert.AreEqual("Number of clicks: 1", output.Text);
                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
                    Assert.AreEqual("Number of clicks: 2", output.Text);
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
                    Assert.AreEqual("<ToggleButton Content=\"ToggleButton\" Click=\"Button_Click\" $(IsEnabled)/>", page.Examples[0].XamlCode);
                    Assert.IsNull(page.Examples[0].CSharpCode);

                    var button = (ToggleButton)FindByAutomationId(page, "GallerySample_ToggleButton_ToggleButton");
                    var output = FindNamedDescendant<TextBlock>(page, "Control1Output");
                    Assert.IsNotNull(button);
                    Assert.IsNotNull(output);

                    Assert.AreEqual("Toggle1", button.Name);
                    Assert.AreEqual("ToggleButton", button.Content);
                    Assert.AreEqual(false, button.IsChecked);
                    Assert.AreEqual("Control1Output", output.Name);
                    Assert.AreEqual(new Thickness(0, 12, 0, 0), output.Margin);
                    Assert.AreEqual("Off", output.Text);

                    button.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("On", output.Text);

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
                    Assert.AreEqual(string.Empty, simpleToggle.OffContent);
                    Assert.AreEqual(string.Empty, simpleToggle.OnContent);
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
                    Assert.AreEqual(Orientation.Horizontal, exampleRoot.Orientation);
                    Assert.AreEqual(24d, ((FrameworkElement)exampleRoot.Children[1]).Margin.Left);
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
                    Assert.AreEqual("NavLinksList", AutomationProperties.GetAutomationId(navLinksList));
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
                    Assert.AreEqual(196d, compactPaneLength.Width);
                    Assert.AreEqual(24d, compactPaneLength.Minimum);
                    Assert.AreEqual(128d, compactPaneLength.Maximum);

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
                    Assert.AreEqual(Orientation.Horizontal, exampleRoot.Orientation);
                    Assert.AreEqual(24d, ((FrameworkElement)exampleRoot.Children[1]).Margin.Left);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"personPicture\"");
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
                    Assert.AreEqual(0, profileType.SelectedIndex);
                    Assert.AreEqual("Profile Image", profileImageRadio.Content);
                    Assert.AreEqual("Display Name", displayNameRadio.Content);
                    Assert.AreEqual("Initials", initialsRadio.Content);
                    Assert.AreEqual("ProfileImageRadio", AutomationProperties.GetAutomationId(profileImageRadio));
                    Assert.IsNotNull(personPicture.ProfilePicture);
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
        public void SoundSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Sound"));
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
                    Assert.AreEqual("Toggling Sound", page.Examples[0].HeaderText);
                    Assert.AreEqual("Toggling Spatial Audio", page.Examples[1].HeaderText);
                    Assert.AreEqual("Play Specific System Sound", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.IsNull(page.Examples[0].XamlCode);
                    Assert.IsNull(page.Examples[1].XamlCode);
                    Assert.IsNull(page.Examples[2].XamlCode);
                    StringAssert.Contains(page.Examples[0].CSharpCode, "ElementSoundPlayer.State = ElementSoundPlayerState.Off;");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "ElementSoundPlayer.Play(ElementSoundKind.GoBack);");

                    var soundToggle = (Mux.ToggleSwitch)FindByAutomationId(page, "GallerySample_Sound_ToggleSwitch");
                    var spatialAudioBox = FindNamedDescendant<CheckBox>(page, "spatialAudioBox");
                    var focusButton = (Button)FindByAutomationId(page, "Focus");
                    var goBackButton = (Button)FindByAutomationId(page, "GoBack");
                    Assert.IsNotNull(soundToggle);
                    Assert.IsNotNull(spatialAudioBox);
                    Assert.IsNotNull(focusButton);
                    Assert.IsNotNull(goBackButton);

                    Assert.AreEqual("soundToggle", soundToggle.Name);
                    Assert.AreEqual(115d, soundToggle.Width);
                    Assert.AreEqual(0d, soundToggle.MinWidth);
                    Assert.AreEqual("Sound Off", soundToggle.OffContent);
                    Assert.AreEqual("Sound On", soundToggle.OnContent);
                    Assert.IsFalse(soundToggle.IsOn);
                    Assert.AreEqual("Enable Spatial Audio", spatialAudioBox.Content);
                    Assert.IsFalse(spatialAudioBox.IsEnabled);
                    Assert.AreEqual("\u25B6 Focus", focusButton.Content);
                    Assert.AreEqual("0", focusButton.Tag);
                    Assert.AreEqual("Focus", AutomationProperties.GetName(focusButton));
                    Assert.AreEqual("\u25B6 GoBack", goBackButton.Content);
                    Assert.AreEqual("6", goBackButton.Tag);

                    soundToggle.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(spatialAudioBox.IsEnabled);

                    spatialAudioBox.IsChecked = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(true, spatialAudioBox.IsChecked);

                    soundToggle.IsOn = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(spatialAudioBox.IsEnabled);
                    Assert.AreEqual(false, spatialAudioBox.IsChecked);
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
        public void MediaPlayerElementSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("MediaPlayerElement"));
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
                    Assert.AreEqual("A MediaPlayerElement with transport controls.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A MediaPlayerElement that autoplays the video.", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "Source=\"/Assets/SampleMedia/ladybug.wmv\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "AreTransportControlsEnabled=\"True\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "FileOpenPicker");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "Player1.Source = mediaSource;");
                    StringAssert.Contains(page.Examples[1].XamlCode, "Source=\"Assets/SampleMedia/fishes.wmv\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "AutoPlay=\"True\"");
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var firstRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(Orientation.Horizontal, firstRoot.Orientation);
                    Assert.AreEqual(24d, ((FrameworkElement)firstRoot.Children[1]).Margin.Left);

                    var player1 = (FrameworkElement)FindByAutomationId(page, "GallerySample_MediaPlayerElement_MediaPlayerElement");
                    var player2 = (FrameworkElement)FindByAutomationId(page, "GallerySample_MediaPlayerElement_AutoPlayMediaPlayerElement");
                    var openFileButton = FindNamedDescendant<Button>(page, "OpenFileButton");
                    Assert.IsNotNull(player1);
                    Assert.IsNotNull(player2);
                    Assert.IsNotNull(openFileButton);

                    Assert.AreEqual("Player1", player1.Name);
                    Assert.AreEqual(400d, player1.Width);
                    Assert.AreEqual(225d, player1.Height);
                    Assert.AreEqual(400d, player1.MaxWidth);
                    Assert.AreEqual(HorizontalAlignment.Left, player1.HorizontalAlignment);
                    Assert.AreEqual("Assets/SampleMedia/ladybug.wmv", player1.Tag);
                    var player1Poster = (Image)((Grid)player1).Children[0];
                    Assert.AreEqual(400d, player1Poster.Width);
                    Assert.AreEqual(225d, player1Poster.Height);
                    Assert.AreEqual(Stretch.Fill, player1Poster.Stretch);
                    StringAssert.Contains(((BitmapImage)player1Poster.Source).UriSource.ToString(), "ladybug.poster.png");
                    Assert.AreEqual("Open a file", openFileButton.Content);
                    Assert.AreEqual("Open file button", AutomationProperties.GetName(openFileButton));
                    Assert.AreEqual("GallerySample_MediaPlayerElement_OpenFileButton", AutomationProperties.GetAutomationId(openFileButton));

                    Assert.AreEqual("Player2", player2.Name);
                    Assert.AreEqual(400d, player2.Width);
                    Assert.AreEqual(225d, player2.Height);
                    Assert.AreEqual(HorizontalAlignment.Left, player2.HorizontalAlignment);
                    Assert.AreEqual("Assets/SampleMedia/fishes.wmv", player2.Tag);
                    var player2Poster = (Image)((Grid)player2).Children[0];
                    Assert.AreEqual(400d, player2Poster.Width);
                    Assert.AreEqual(225d, player2Poster.Height);
                    Assert.AreEqual(Stretch.Fill, player2Poster.Stretch);
                    StringAssert.Contains(((BitmapImage)player2Poster.Source).UriSource.ToString(), "fishes.poster.png");
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
        public void AcrylicSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Acrylic"));
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
                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("Default in-app acrylic brush.", page.Examples[0].HeaderText);
                    Assert.AreEqual("Custom acrylic in-app brush.", page.Examples[1].HeaderText);
                    Assert.AreEqual("Luminosity with in-app Acrylic.", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.IsNotNull(FindTextBlockByText(page, "Acrylic Brush might fall back to SolidColorbrush in certain scenarios. If you can't see the Acrylic effect, please refer to Acrylic brush adaptability documentation. Acrylic Brush uses in-app acrylic. See SystemBackdrops (Mica/Acrylic) for background acrylic."));
                    StringAssert.Contains(page.Examples[0].XamlCode, "AcrylicInAppFillColorDefaultBrush");
                    StringAssert.Contains(page.Examples[1].XamlCode, "CustomAcrylicInAppBrush");
                    StringAssert.Contains(page.Examples[1].XamlCode, "TintOpacity=\"$(OpacitySlider)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "FallbackColor=\"$(FallbackColor)\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "TintLuminosityOpacity=\"$(TintLuminositySlider)\"");

                    var firstRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    var example1Grid = FindNamedDescendant<Grid>(firstRoot, "Example1Grid");
                    Assert.IsNotNull(example1Grid);
                    Assert.AreEqual("GallerySample_Acrylic_Example1Grid", AutomationProperties.GetAutomationId(example1Grid));
                    Assert.AreEqual(400d, example1Grid.Width);
                    Assert.AreEqual(252d, example1Grid.Height);
                    var defaultAcrylicRect = (WpfShapes.Rectangle)FindByAutomationId(page, "GallerySample_Acrylic_DefaultAcrylicRect");
                    Assert.IsNotNull(defaultAcrylicRect);
                    Assert.AreSame(defaultAcrylicRect, FindNamedDescendant<WpfShapes.Rectangle>(page, "DefaultAcrylicShapeInApp"));
                    Assert.AreEqual(new Thickness(12), defaultAcrylicRect.Margin);
                    Assert.AreEqual(0.72, defaultAcrylicRect.Opacity, 0.001);
                    Assert.IsInstanceOfType(defaultAcrylicRect.Fill, typeof(SolidColorBrush));

                    var customRoot = (GallerySamplePanel)page.Examples[1].ExampleContent;
                    var example3Grid = FindNamedDescendant<Grid>(customRoot, "Example3Grid");
                    Assert.IsNotNull(example3Grid);
                    Assert.AreEqual(2, example3Grid.ColumnDefinitions.Count);
                    Assert.AreEqual(652d, example3Grid.Width);
                    Assert.AreEqual(252d, example3Grid.MinHeight);

                    var customAcrylicRect = FindNamedDescendant<WpfShapes.Rectangle>(page, "CustomAcrylicShapeInApp");
                    Assert.IsNotNull(customAcrylicRect);
                    var customBrush = (SolidColorBrush)customAcrylicRect.Fill;
                    Assert.AreEqual(Colors.Black, customBrush.Color);
                    Assert.AreEqual(0.8, customBrush.Opacity, 0.001);
                    Assert.AreEqual("FallbackColor=#FF008000", customAcrylicRect.Tag);

                    var opacitySlider = FindNamedDescendant<Slider>(page, "OpacitySliderInApp");
                    var colorSelector = FindNamedDescendant<ComboBox>(page, "ColorSelectorInApp");
                    var fallbackSelector = FindNamedDescendant<ComboBox>(page, "FallbackColorSelectorInApp");
                    Assert.IsNotNull(opacitySlider);
                    Assert.IsNotNull(colorSelector);
                    Assert.IsNotNull(fallbackSelector);
                    AssertAcrylicSlider(opacitySlider, "tint opacity", 0.8);
                    AssertAcrylicColorSelector(colorSelector, "tint color", Colors.Black, Colors.Red, Colors.Blue);
                    AssertAcrylicColorSelector(fallbackSelector, "fallback color", Colors.Green, Colors.Yellow);

                    opacitySlider.Value = 0.5;
                    colorSelector.SelectedIndex = 1;
                    fallbackSelector.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(0.5, customBrush.Opacity, 0.001);
                    Assert.AreEqual(Colors.Red, customBrush.Color);
                    Assert.AreEqual("FallbackColor=#FFFFFF00", customAcrylicRect.Tag);

                    var luminosityRoot = (GallerySamplePanel)page.Examples[2].ExampleContent;
                    var example4Grid = FindNamedDescendant<Grid>(luminosityRoot, "Example4Grid");
                    Assert.IsNotNull(example4Grid);
                    Assert.AreEqual(2, example4Grid.ColumnDefinitions.Count);
                    Assert.AreEqual(652d, example4Grid.Width);

                    var luminosityAcrylicRect = FindNamedDescendant<WpfShapes.Rectangle>(page, "CustomAcrylicShapeLumin");
                    Assert.IsNotNull(luminosityAcrylicRect);
                    var luminosityBrush = (SolidColorBrush)luminosityAcrylicRect.Fill;
                    Assert.AreEqual(Colors.SkyBlue, luminosityBrush.Color);
                    Assert.AreEqual(0.64, luminosityBrush.Opacity, 0.001);
                    Assert.AreEqual("TintLuminosityOpacity=0.8", luminosityAcrylicRect.Tag);

                    var opacitySliderLumin = FindNamedDescendant<Slider>(page, "OpacitySliderLumin");
                    var luminositySlider = FindNamedDescendant<Slider>(page, "LuminositySlider");
                    AssertAcrylicSlider(opacitySliderLumin, "tint opacity", 0.8);
                    AssertAcrylicSlider(luminositySlider, "tint luminosity", 0.8);

                    opacitySliderLumin.Value = 0.5;
                    luminositySlider.Value = 0.5;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(0.25, luminosityBrush.Opacity, 0.001);
                    Assert.AreEqual("TintLuminosityOpacity=0.5", luminosityAcrylicRect.Tag);
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
        public void AnimatedIconSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("AnimatedIcon"));
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
                    Assert.AreEqual("Adding AnimatedIcon to a button", page.Examples[0].HeaderText);
                    Assert.AreEqual("Adding AnimatedIcon to a NavigationView", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "PointerEntered=\"Button_PointerEntered\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "animatedvisuals:$(AnimatedVisualSourceKind)");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "AnimatedIcon.SetState(this.SearchAnimatedIcon, \"PointerOver\");");
                    StringAssert.Contains(page.Examples[1].XamlCode, "NavigationViewItem Content = \"Game Settings\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "animatedvisuals:AnimatedSettingsVisualSource");

                    var firstRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, firstRoot.Children.Count);
                    var firstLayout = (Grid)firstRoot.Children[0];
                    Assert.AreEqual(2, firstLayout.ColumnDefinitions.Count);

                    var button = (Button)FindByAutomationId(page, "GallerySample_AnimatedIcon_Button");
                    Assert.IsNotNull(button);
                    Assert.AreEqual(75d, button.Width);
                    Assert.AreEqual("AnimatedIcon Example", AutomationProperties.GetName(button));
                    Assert.AreEqual("AnimatedFindVisualSource", button.Tag);

                    var searchIcon = (Mux.SymbolIcon)button.Content;
                    Assert.AreEqual("SearchAnimatedIcon", searchIcon.Name);
                    Assert.AreEqual(Mux.Symbol.Find, searchIcon.Symbol);
                    Assert.AreEqual("Normal", Mux.AnimatedIcon.GetState(searchIcon));
                    Assert.AreEqual("AnimatedFindVisualSource", searchIcon.DataContext);

                    button.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                    {
                        RoutedEvent = UIElement.MouseEnterEvent
                    });
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("PointerOver", Mux.AnimatedIcon.GetState(searchIcon));

                    button.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                    {
                        RoutedEvent = UIElement.MouseLeaveEvent
                    });
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Normal", Mux.AnimatedIcon.GetState(searchIcon));

                    var sourceSelection = FindNamedDescendant<ComboBox>(page, "AnimatedVisualSourceSelection");
                    Assert.IsNotNull(sourceSelection);
                    Assert.AreEqual("Kind", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(sourceSelection));
                    Assert.AreEqual(340d, sourceSelection.MinWidth);
                    Assert.AreEqual(4, sourceSelection.SelectedIndex);
                    Assert.AreEqual(7, sourceSelection.Items.Count);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "AnimatedBackVisualSource",
                            "AnimatedChevronDownSmallVisualSource",
                            "AnimatedChevronRightDownSmallVisualSource",
                            "AnimatedChevronUpDownSmallVisualSource",
                            "AnimatedFindVisualSource",
                            "AnimatedGlobalNavigationButtonVisualSource",
                            "AnimatedSettingsVisualSource"
                        },
                        sourceSelection.Items.Cast<string>().ToArray());

                    sourceSelection.SelectedIndex = 6;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("AnimatedSettingsVisualSource", button.Tag);
                    Assert.AreEqual("AnimatedSettingsVisualSource", searchIcon.DataContext);

                    var navigationView = (Mux.NavigationView)FindByAutomationId(page, "GallerySample_AnimatedIcon_NavigationView");
                    Assert.IsNotNull(navigationView);
                    Assert.AreEqual("AnimatedIconNavigationView", navigationView.Name);
                    Assert.IsFalse(navigationView.IsSettingsVisible);
                    Assert.AreEqual(1, navigationView.MenuItems.Count);

                    var gameSettingsItem = (Mux.NavigationViewItem)navigationView.MenuItems[0];
                    Assert.AreEqual("GameSettingsItem", gameSettingsItem.Name);
                    Assert.AreEqual("Game Settings", gameSettingsItem.Content);
                    var gameSettingsIcon = (Mux.FontIcon)gameSettingsItem.Icon;
                    Assert.AreEqual("GameSettingsIcon", gameSettingsIcon.Name);
                    Assert.AreEqual("\uE713", gameSettingsIcon.Glyph);
                    Assert.AreEqual("Normal", Mux.AnimatedIcon.GetState(gameSettingsIcon));
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
        public void CompactSizingSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("CompactSizing"));
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
                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("Compact Sizing for controls", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<ResourceDictionary Source=\"ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml\" />");
                    Assert.IsNotNull(FindTextBlockByText(page, "Controls that support compact styling:"));
                    Assert.IsNotNull(FindTextBlockByText(page, "\u2022 TextBox"));
                    Assert.IsNotNull(FindTextBlockByText(page, "\u2022 NavigationView"));

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var layout = (Grid)root.Children[0];
                    Assert.AreEqual(2, layout.ColumnDefinitions.Count);

                    var contentFrame = FindNamedDescendant<ContentControl>(page, "ContentFrame");
                    Assert.IsNotNull(contentFrame);
                    Assert.AreEqual("GallerySample_CompactSizing_ContentFrame", AutomationProperties.GetAutomationId(contentFrame));

                    var header = FindNamedDescendant<TextBlock>(contentFrame, "HeaderBlock");
                    Assert.IsNotNull(header);
                    Assert.AreEqual("Standard Size", header.Text);

                    var firstName = (TextBox)FindByAutomationId(page, "GallerySample_CompactSizing_FirstName");
                    Assert.IsNotNull(firstName);
                    Assert.AreSame(firstName, FindNamedDescendant<TextBox>(page, "firstName"));
                    Assert.AreEqual("First Name:", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(firstName));
                    Assert.AreEqual(34d, firstName.MinHeight);
                    Assert.AreEqual(new Thickness(12, 6, 12, 6), firstName.Padding);
                    Assert.AreEqual(16d, firstName.Margin.Bottom);

                    var lastName = FindNamedDescendant<TextBox>(page, "lastName");
                    var password = FindNamedDescendant<PasswordBox>(page, "password");
                    var confirmPassword = FindNamedDescendant<PasswordBox>(page, "confirmPassword");
                    var chosenDate = FindNamedDescendant<DatePicker>(page, "chosenDate");
                    Assert.IsNotNull(lastName);
                    Assert.IsNotNull(password);
                    Assert.IsNotNull(confirmPassword);
                    Assert.IsNotNull(chosenDate);
                    Assert.AreEqual("Last Name:", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(lastName));
                    Assert.AreEqual("Password:", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(password));
                    Assert.AreEqual("Confirm Password:", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(confirmPassword));
                    Assert.AreEqual("Pick a date", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(chosenDate));

                    var radioButtons = FindNamedDescendant<Mux.RadioButtons>(page, "ControlSizeRadioButtons");
                    var standardRadio = FindNamedDescendant<RadioButton>(page, "StandardSizeRadioButton");
                    var compactRadio = FindNamedDescendant<RadioButton>(page, "CompactSizeRadioButton");
                    Assert.IsNotNull(radioButtons);
                    Assert.AreEqual("Fluent Standard and Compact Sizing", radioButtons.Header);
                    Assert.AreEqual("Standard", standardRadio.Content);
                    Assert.AreEqual("Compact", compactRadio.Content);
                    Assert.AreEqual("StandardSize", standardRadio.Tag);
                    Assert.AreEqual("CompactSize", compactRadio.Tag);
                    Assert.AreEqual(true, standardRadio.IsChecked);

                    firstName.Text = "Ada";
                    lastName.Text = "Lovelace";
                    password.Password = "first-secret";
                    confirmPassword.Password = "first-secret";
                    chosenDate.SelectedDate = new DateTime(2026, 5, 25);
                    compactRadio.IsChecked = true;
                    WpfTestHost.DoEvents();

                    header = FindNamedDescendant<TextBlock>(contentFrame, "HeaderBlock");
                    Assert.AreEqual("Compact Size", header.Text);
                    firstName = (TextBox)FindByAutomationId(page, "GallerySample_CompactSizing_FirstName");
                    lastName = FindNamedDescendant<TextBox>(page, "lastName");
                    password = FindNamedDescendant<PasswordBox>(page, "password");
                    confirmPassword = FindNamedDescendant<PasswordBox>(page, "confirmPassword");
                    chosenDate = FindNamedDescendant<DatePicker>(page, "chosenDate");
                    Assert.AreEqual("Ada", firstName.Text);
                    Assert.AreEqual("Lovelace", lastName.Text);
                    Assert.AreEqual("first-secret", password.Password);
                    Assert.AreEqual("first-secret", confirmPassword.Password);
                    Assert.AreEqual(new DateTime(2026, 5, 25), chosenDate.SelectedDate);
                    Assert.AreEqual(26d, firstName.MinHeight);
                    Assert.AreEqual(new Thickness(8, 3, 8, 3), firstName.Padding);
                    Assert.AreEqual(8d, firstName.Margin.Bottom);

                    firstName.Text = "Grace";
                    standardRadio.IsChecked = true;
                    WpfTestHost.DoEvents();
                    header = FindNamedDescendant<TextBlock>(contentFrame, "HeaderBlock");
                    firstName = (TextBox)FindByAutomationId(page, "GallerySample_CompactSizing_FirstName");
                    Assert.AreEqual("Standard Size", header.Text);
                    Assert.AreEqual("Grace", firstName.Text);
                    Assert.AreEqual(34d, firstName.MinHeight);
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
                    var bitmapLayout = (Grid)bitmapRoot.Children[0];
                    Assert.AreEqual(2, bitmapLayout.ColumnDefinitions.Count);

                    var slicesIcon = (Mux.BitmapIcon)FindByAutomationId(page, "GallerySample_IconElement_SlicesIcon");
                    Assert.IsNotNull(slicesIcon);
                    Assert.AreSame(slicesIcon, FindNamedDescendant<Mux.BitmapIcon>(page, "SlicesIcon"));
                    Assert.AreEqual(50d, slicesIcon.Width);
                    Assert.AreEqual(HorizontalAlignment.Left, slicesIcon.HorizontalAlignment);
                    Assert.IsFalse(slicesIcon.ShowAsMonochrome);
                    StringAssert.Contains(slicesIcon.UriSource.ToString(), "Assets/SampleMedia/Slices.png");

                    var monochromeButton = FindNamedDescendant<CheckBox>(page, "MonochromeButton");
                    Assert.IsNotNull(monochromeButton);
                    Assert.AreEqual("Monochrome", monochromeButton.Content);
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
        public void ShapeSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Shape"));
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
                    Assert.AreEqual("Ellipse", page.Examples[0].HeaderText);
                    Assert.AreEqual("Rectangle", page.Examples[1].HeaderText);
                    Assert.AreEqual("Polygon", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);

                    StringAssert.Contains(page.Examples[0].XamlCode, "<Ellipse Fill=\"SteelBlue\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Height=\"$(Slider1)\" Width=\"$(Slider2)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<Rectangle Fill=\"SteelBlue\" Height=\"$(Slider1)\" Width=\"$(Slider2)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "RadiusY=\"$(Slider4)\" RadiusX=\"$(Slider5)\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "<Polygon Fill=\"SteelBlue\" Points=\"10,100 60,40 200,40 250,100\"");

                    var shapeRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, shapeRoot.Children.Count);
                    var shapeLayout = (Grid)shapeRoot.Children[0];
                    Assert.AreEqual(2, shapeLayout.ColumnDefinitions.Count);

                    var ellipse = (WpfShapes.Ellipse)FindByAutomationId(page, "GallerySample_Shape_Ellipse");
                    Assert.IsNotNull(ellipse);
                    Assert.AreSame(ellipse, FindNamedDescendant<WpfShapes.Ellipse>(page, "EllipseElement"));
                    Assert.AreEqual(100d, ellipse.Width);
                    Assert.AreEqual(100d, ellipse.Height);
                    Assert.AreEqual(Brushes.SteelBlue, ellipse.Fill);
                    Assert.AreEqual(Brushes.Black, ellipse.Stroke);
                    Assert.AreEqual(2d, ellipse.StrokeThickness);

                    var ellipseHeightSlider = AssertLineSlider(page, "slider1", "Height", 100, 150, 100);
                    var ellipseWidthSlider = AssertLineSlider(page, "slider2", "Width", 100, 150, 100);
                    var ellipseStrokeSlider = AssertLineSlider(page, "slider3", "Stroke Thickness", 2, 10, 2);
                    ellipseHeightSlider.Value = 132;
                    ellipseWidthSlider.Value = 144;
                    ellipseStrokeSlider.Value = 6;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(132d, ellipse.Height);
                    Assert.AreEqual(144d, ellipse.Width);
                    Assert.AreEqual(6d, ellipse.StrokeThickness);

                    var rectangle = FindNamedDescendant<WpfShapes.Rectangle>(page, "RectangleElement");
                    Assert.IsNotNull(rectangle);
                    Assert.AreEqual(100d, rectangle.Width);
                    Assert.AreEqual(100d, rectangle.Height);
                    Assert.AreEqual(Brushes.SteelBlue, rectangle.Fill);
                    Assert.AreEqual(Brushes.Black, rectangle.Stroke);
                    Assert.AreEqual(2d, rectangle.StrokeThickness);
                    Assert.AreEqual(0d, rectangle.RadiusX);
                    Assert.AreEqual(0d, rectangle.RadiusY);

                    var rectangleHeightSlider = AssertLineSlider(page, "recSlider1", "Height", 100, 150, 100);
                    var rectangleWidthSlider = AssertLineSlider(page, "recSlider2", "Width", 100, 150, 100);
                    var rectangleStrokeSlider = AssertLineSlider(page, "recSlider3", "Stroke Thickness", 2, 10, 2);
                    var rectangleRadiusYSlider = AssertLineSlider(page, "recSlider4", "Radius Y", 0, 100, 0);
                    var rectangleRadiusXSlider = AssertLineSlider(page, "recSlider5", "Radius X", 0, 100, 0);
                    rectangleHeightSlider.Value = 128;
                    rectangleWidthSlider.Value = 146;
                    rectangleStrokeSlider.Value = 8;
                    rectangleRadiusYSlider.Value = 36;
                    rectangleRadiusXSlider.Value = 48;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(128d, rectangle.Height);
                    Assert.AreEqual(146d, rectangle.Width);
                    Assert.AreEqual(8d, rectangle.StrokeThickness);
                    Assert.AreEqual(36d, rectangle.RadiusY);
                    Assert.AreEqual(48d, rectangle.RadiusX);

                    var polygonDescription = FindTextBlockByText(page, "A polygon is a connected series of lines that form a closed shape.");
                    Assert.IsNotNull(polygonDescription);
                    var polygon = FindNamedDescendant<WpfShapes.Polygon>(page, "PolygonElement");
                    Assert.IsNotNull(polygon);
                    Assert.AreEqual(4, polygon.Points.Count);
                    Assert.AreEqual(new Point(10, 100), polygon.Points[0]);
                    Assert.AreEqual(new Point(60, 40), polygon.Points[1]);
                    Assert.AreEqual(new Point(200, 40), polygon.Points[2]);
                    Assert.AreEqual(new Point(250, 100), polygon.Points[3]);
                    Assert.AreEqual(Brushes.SteelBlue, polygon.Fill);
                    Assert.AreEqual(Brushes.Black, polygon.Stroke);
                    Assert.AreEqual(2d, polygon.StrokeThickness);
                    AssertLineSlider(page, "polySlider1", "Stroke Thickness", 2, 10, 2).Value = 7;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(7d, polygon.StrokeThickness);

                    var polygonToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "ToggleSwitchPoly");
                    Assert.IsNotNull(polygonToggle);
                    Assert.AreEqual("Show points", polygonToggle.Header);
                    var polygonPoint = FindTextBlockByText(page, "Point #1: (10,100)");
                    Assert.IsNotNull(polygonPoint);
                    Assert.AreEqual(Visibility.Collapsed, polygonPoint.Visibility);
                    polygonToggle.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Visible, polygonPoint.Visibility);
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
        public void LineSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Line"));
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
                    Assert.AreEqual("Line", page.Examples[0].HeaderText);
                    Assert.AreEqual("Polyline", page.Examples[1].HeaderText);
                    Assert.AreEqual("Path", page.Examples[2].HeaderText);
                    Assert.AreEqual("GeometryGroup", page.Examples[3].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);

                    StringAssert.Contains(page.Examples[0].XamlCode, "<Line Stroke=\"SteelBlue\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "X1=\"$(Slider1)\" Y1=\"$(Slider2)\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<Polyline Stroke=\"Black\" StrokeThickness=\"$(Slider1)\"");
                    StringAssert.Contains(page.Examples[2].XamlCode, "Data=\"M 10,100 C 100,25 300,250 400,75 H 200\"");
                    StringAssert.Contains(page.Examples[3].XamlCode, "<GeometryGroup FillRule=\"EvenOdd\">");

                    var lineRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, lineRoot.Children.Count);
                    var lineLayout = (Grid)lineRoot.Children[0];
                    Assert.AreEqual(2, lineLayout.ColumnDefinitions.Count);

                    var line = (WpfShapes.Line)FindByAutomationId(page, "GallerySample_Line_Line");
                    Assert.IsNotNull(line);
                    Assert.AreSame(line, FindNamedDescendant<WpfShapes.Line>(page, "LineElement"));
                    Assert.AreEqual(0d, line.X1);
                    Assert.AreEqual(0d, line.Y1);
                    Assert.AreEqual(200d, line.X2);
                    Assert.AreEqual(0d, line.Y2);
                    Assert.AreEqual(5d, line.StrokeThickness);
                    Assert.AreEqual(Brushes.SteelBlue, line.Stroke);
                    Assert.AreEqual(50d, Canvas.GetTop(line));

                    var lineSlider1 = AssertLineSlider(page, "lineSlider1", "Start point X", 0, 100, 0);
                    var lineSlider2 = AssertLineSlider(page, "lineSlider2", "Start point Y", 0, 100, 0);
                    var lineSlider3 = AssertLineSlider(page, "lineSlider3", "End point X", 200, 300, 200);
                    var lineSlider4 = AssertLineSlider(page, "lineSlider4", "End point Y", 0, 100, 0);
                    var lineSlider5 = AssertLineSlider(page, "lineSlider5", "Stroke Thickness", 5, 10, 5);
                    lineSlider1.Value = 12;
                    lineSlider2.Value = 24;
                    lineSlider3.Value = 260;
                    lineSlider4.Value = 72;
                    lineSlider5.Value = 8;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(12d, line.X1);
                    Assert.AreEqual(24d, line.Y1);
                    Assert.AreEqual(260d, line.X2);
                    Assert.AreEqual(72d, line.Y2);
                    Assert.AreEqual(8d, line.StrokeThickness);

                    var polyline = FindNamedDescendant<WpfShapes.Polyline>(page, "PolylineElement");
                    Assert.IsNotNull(polyline);
                    Assert.AreEqual(4, polyline.Points.Count);
                    Assert.AreEqual(new Point(10, 100), polyline.Points[0]);
                    Assert.AreEqual(new Point(60, 40), polyline.Points[1]);
                    Assert.AreEqual(new Point(200, 40), polyline.Points[2]);
                    Assert.AreEqual(new Point(250, 100), polyline.Points[3]);
                    Assert.AreEqual(Brushes.Black, polyline.Stroke);
                    AssertLineSlider(page, "polyLineSlider1", "Stroke Thickness", 2, 10, 2).Value = 7;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(7d, polyline.StrokeThickness);
                    var polylineToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "ToggleSwitch2");
                    Assert.IsNotNull(polylineToggle);
                    Assert.AreEqual("Show points", polylineToggle.Header);
                    var point1 = FindTextBlockByText(page, "Point #1: (10,100)");
                    Assert.IsNotNull(point1);
                    Assert.AreEqual(Visibility.Collapsed, point1.Visibility);
                    polylineToggle.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Visible, point1.Visibility);

                    var path = FindNamedDescendant<WpfShapes.Path>(page, "PathElement");
                    Assert.IsNotNull(path);
                    StringAssert.Contains(path.Data.ToString(CultureInfo.InvariantCulture), "M10,100C100,25");
                    Assert.AreEqual(Brushes.DarkGoldenrod, path.Stroke);
                    AssertLineSlider(page, "pathSlider1", "Stroke Thickness", 2, 10, 2).Value = 6;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(6d, path.StrokeThickness);
                    var pathToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "ToggleSwitch");
                    Assert.IsNotNull(pathToggle);
                    Assert.AreEqual("Show points", pathToggle.Header);
                    var pathPoint = FindTextBlockByText(page, "Point #5: (200,75)");
                    Assert.IsNotNull(pathPoint);
                    Assert.AreEqual(Visibility.Collapsed, pathPoint.Visibility);
                    pathToggle.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(Visibility.Visible, pathPoint.Visibility);

                    var geometryPath = FindNamedDescendant<WpfShapes.Path>(page, "GeometryGroupPath");
                    Assert.IsNotNull(geometryPath);
                    Assert.AreEqual(Brushes.Black, geometryPath.Stroke);
                    Assert.AreEqual(4d, geometryPath.StrokeThickness);
                    Assert.AreEqual(CreateBrushForTest("#CCCCFF").Color, ((SolidColorBrush)geometryPath.Fill).Color);
                    var geometryGroup = (GeometryGroup)geometryPath.Data;
                    Assert.AreEqual(FillRule.EvenOdd, geometryGroup.FillRule);
                    Assert.AreEqual(3, geometryGroup.Children.Count);
                    Assert.IsInstanceOfType(geometryGroup.Children[0], typeof(LineGeometry));
                    var ellipseGeometry = (EllipseGeometry)geometryGroup.Children[1];
                    Assert.AreEqual(new Point(40, 70), ellipseGeometry.Center);
                    Assert.AreEqual(30d, ellipseGeometry.RadiusX);
                    Assert.AreEqual(30d, ellipseGeometry.RadiusY);
                    Assert.IsInstanceOfType(geometryGroup.Children[2], typeof(RectangleGeometry));
                    AssertLineSlider(page, "geogroupslider1", "RadiusX", 30, 40, 30).Value = 38;
                    AssertLineSlider(page, "geogroupslider2", "RadiusY", 30, 50, 30).Value = 44;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(38d, ellipseGeometry.RadiusX);
                    Assert.AreEqual(44d, ellipseGeometry.RadiusY);
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
        public void RadialGradientBrushSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("RadialGradientBrush"));
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
                    Assert.AreEqual("RadialGradientBrush Sample", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<media:RadialGradientBrush");
                    StringAssert.Contains(page.Examples[0].XamlCode, "MappingMode=\"$(MappingMode)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Center=\"$(CenterX),$(CenterY)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "GradientOrigin=\"$(OriginX),$(OriginY)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "SpreadMethod=\"$(SpreadMethod)\"");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var layout = (Grid)root.Children[0];
                    Assert.AreEqual(2, layout.ColumnDefinitions.Count);

                    var rect = (System.Windows.Shapes.Rectangle)FindByAutomationId(page, "GallerySample_RadialGradientBrush_Rect");
                    Assert.IsNotNull(rect);
                    Assert.AreSame(rect, FindNamedDescendant<System.Windows.Shapes.Rectangle>(page, "Rect"));
                    Assert.AreEqual(200d, rect.Width);
                    Assert.AreEqual(200d, rect.Height);

                    var brush = (RadialGradientBrush)rect.Fill;
                    Assert.AreEqual(BrushMappingMode.RelativeToBoundingBox, brush.MappingMode);
                    Assert.AreEqual(GradientSpreadMethod.Pad, brush.SpreadMethod);
                    Assert.AreEqual(new Point(0.5, 0.5), brush.Center);
                    Assert.AreEqual(new Point(0.5, 0.5), brush.GradientOrigin);
                    Assert.AreEqual(0.5d, brush.RadiusX);
                    Assert.AreEqual(0.5d, brush.RadiusY);
                    Assert.AreEqual(2, brush.GradientStops.Count);
                    Assert.AreEqual(Colors.Yellow, brush.GradientStops[0].Color);
                    Assert.AreEqual(0d, brush.GradientStops[0].Offset);
                    Assert.AreEqual(Colors.Blue, brush.GradientStops[1].Color);
                    Assert.AreEqual(1d, brush.GradientStops[1].Offset);

                    var mappingModeComboBox = FindNamedDescendant<ComboBox>(page, "MappingModeComboBox");
                    Assert.IsNotNull(mappingModeComboBox);
                    Assert.AreEqual("MappingMode", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(mappingModeComboBox));
                    Assert.AreEqual(0, mappingModeComboBox.SelectedIndex);
                    Assert.AreEqual("RelativeToBoundingBox", mappingModeComboBox.Items[0]);
                    Assert.AreEqual("Absolute", mappingModeComboBox.Items[1]);

                    var centerXSlider = AssertRadialGradientSlider(page, "CenterXSlider", "Center.X", 1, 0.5, 0.02, 0.05);
                    var centerYSlider = AssertRadialGradientSlider(page, "CenterYSlider", "Center.Y", 1, 0.5, 0.02, 0.05);
                    var radiusXSlider = AssertRadialGradientSlider(page, "RadiusXSlider", "RadiusX", 1, 0.5, 0.02, 0.05);
                    var radiusYSlider = AssertRadialGradientSlider(page, "RadiusYSlider", "RadiusY", 1, 0.5, 0.02, 0.05);
                    var originXSlider = AssertRadialGradientSlider(page, "OriginXSlider", "GradientOrigin.X", 1, 0.5, 0.02, 0.05);
                    var originYSlider = AssertRadialGradientSlider(page, "OriginYSlider", "GradientOrigin.Y", 1, 0.5, 0.02, 0.05);

                    var spreadMethodComboBox = FindNamedDescendant<ComboBox>(page, "SpreadMethodComboBox");
                    Assert.IsNotNull(spreadMethodComboBox);
                    Assert.AreEqual("SpreadMethod", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(spreadMethodComboBox));
                    Assert.AreEqual(0, spreadMethodComboBox.SelectedIndex);
                    Assert.AreEqual("Pad", spreadMethodComboBox.Items[0]);
                    Assert.AreEqual("Reflect", spreadMethodComboBox.Items[1]);
                    Assert.AreEqual("Repeat", spreadMethodComboBox.Items[2]);

                    centerXSlider.Value = 0.25;
                    centerYSlider.Value = 0.75;
                    radiusXSlider.Value = 0.6;
                    radiusYSlider.Value = 0.4;
                    originXSlider.Value = 0.2;
                    originYSlider.Value = 0.8;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(new Point(0.25, 0.75), brush.Center);
                    Assert.AreEqual(new Point(0.2, 0.8), brush.GradientOrigin);
                    Assert.AreEqual(0.6d, brush.RadiusX);
                    Assert.AreEqual(0.4d, brush.RadiusY);

                    spreadMethodComboBox.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(GradientSpreadMethod.Reflect, brush.SpreadMethod);

                    mappingModeComboBox.SelectedIndex = 1;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(BrushMappingMode.Absolute, brush.MappingMode);
                    Assert.AreEqual(new Point(100, 100), brush.Center);
                    Assert.AreEqual(new Point(100, 100), brush.GradientOrigin);
                    Assert.AreEqual(100d, brush.RadiusX);
                    Assert.AreEqual(100d, brush.RadiusY);
                    AssertRadialGradientSlider(page, "CenterXSlider", "Center.X", 200, 100, 4, 10);
                    AssertRadialGradientSlider(page, "CenterYSlider", "Center.Y", 200, 100, 4, 10);
                    AssertRadialGradientSlider(page, "RadiusXSlider", "RadiusX", 200, 100, 4, 10);
                    AssertRadialGradientSlider(page, "RadiusYSlider", "RadiusY", 200, 100, 4, 10);
                    AssertRadialGradientSlider(page, "OriginXSlider", "GradientOrigin.X", 200, 100, 4, 10);
                    AssertRadialGradientSlider(page, "OriginYSlider", "GradientOrigin.Y", 200, 100, 4, 10);
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
                    var layout = (Grid)root.Children[0];
                    Assert.AreEqual(2, layout.ColumnDefinitions.Count);

                    var exampleGrid = FindNamedDescendant<Grid>(page, "Example3Grid");
                    Assert.IsNotNull(exampleGrid);
                    Assert.AreEqual(272d, exampleGrid.MinWidth);
                    Assert.AreEqual(272d, exampleGrid.MinHeight);

                    var shadowCastGrid = FindNamedDescendant<Grid>(page, "ShadowCastGrid");
                    Assert.IsNotNull(shadowCastGrid);
                    Assert.AreSame(shadowCastGrid, exampleGrid.Children[0]);

                    var shadow = FindNamedDescendant<ThemeShadowChrome>(page, "shadow");
                    Assert.IsNotNull(shadow);
                    Assert.AreEqual(32d, shadow.Depth);
                    Assert.AreEqual(32d, shadow.TranslationZ);
                    Assert.AreEqual(new Thickness(36), shadow.Margin);
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

                    slider.Value = 48;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(48d, shadow.Depth);
                    Assert.AreEqual(48d, shadow.TranslationZ);
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
        public void SystemBackdropsSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("SystemBackdrops"));
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
                    Assert.AreEqual("Backdrop types", page.Examples[0].HeaderText);
                    Assert.AreEqual("MicaController", page.Examples[1].HeaderText);
                    Assert.AreEqual("DesktopAcrylicController", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);

                    StringAssert.Contains(page.Examples[0].XamlCode, "<MicaBackdrop/>");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<MicaBackdrop Kind=\"BaseAlt\"/>");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "bool TrySetMicaBackdrop(bool useMicaAlt)");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "SystemBackdrop = micaBackdrop;");
                    Assert.IsNull(page.Examples[1].XamlCode);
                    StringAssert.Contains(page.Examples[1].CSharpCode, "MicaController micaController;");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "micaController.Kind = useMicaAlt ? MicaKind.BaseAlt : MicaKind.Base;");
                    Assert.IsNull(page.Examples[2].XamlCode);
                    StringAssert.Contains(page.Examples[2].CSharpCode, "DesktopAcrylicController acrylicController;");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "acrylicController.Kind = useAcrylicThin ? DesktopAcrylicKind.Thin : DesktopAcrylicKind.Base;");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var firstStack = (StackPanel)root.Children[0];
                    Assert.AreEqual(2, firstStack.Children.Count);
                    var firstText = (TextBlock)firstStack.Children[0];
                    Assert.AreEqual(TextWrapping.Wrap, firstText.TextWrapping);
                    var firstTextContent = new TextRange(firstText.ContentStart, firstText.ContentEnd).Text;
                    StringAssert.Contains(firstTextContent, "A window can use one of the following system backdrops:");
                    StringAssert.Contains(firstTextContent, "Mica vs. Acrylic:");
                    StringAssert.Contains(firstTextContent, "SystemBackdrop");
                    StringAssert.Contains(firstTextContent, "DesktopAcrylicBackdrop");

                    var showWindowButton = (Button)FindByAutomationId(page, "GallerySample_SystemBackdrops_ShowWindowButton");
                    AssertSystemBackdropsShowWindowButton(showWindowButton, "ShowWindowButton");
                    Assert.AreSame(showWindowButton, firstStack.Children[1]);

                    var micaRoot = (GallerySamplePanel)page.Examples[1].ExampleContent;
                    var micaStack = (StackPanel)micaRoot.Children[0];
                    var micaText = (TextBlock)micaStack.Children[0];
                    var micaTextContent = new TextRange(micaText.ContentStart, micaText.ContentEnd).Text;
                    StringAssert.Contains(micaTextContent, "MicaController provides a customizable way to apply the Mica material.");
                    StringAssert.Contains(micaTextContent, "There are 2 kinds of Mica:");
                    AssertSystemBackdropsShowWindowButton((Button)micaStack.Children[1], "MicaControllerShowWindowButton");

                    var acrylicRoot = (GallerySamplePanel)page.Examples[2].ExampleContent;
                    var acrylicStack = (StackPanel)acrylicRoot.Children[0];
                    var acrylicText = (TextBlock)acrylicStack.Children[0];
                    var acrylicTextContent = new TextRange(acrylicText.ContentStart, acrylicText.ContentEnd).Text;
                    StringAssert.Contains(acrylicTextContent, "DesktopAcrylicController provides a customizable way to apply the Desktop Acrylic material.");
                    StringAssert.Contains(acrylicTextContent, "Note: DesktopAcrylicBackdrop always uses the Base kind.");
                    AssertSystemBackdropsShowWindowButton((Button)acrylicStack.Children[1], "DesktopAcrylicControllerShowWindowButton");
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
        public void SystemBackdropElementSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("SystemBackdropElement"));
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
                    Assert.AreEqual("SystemBackdropElement Sample", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<SystemBackdropElement CornerRadius=\"$(CornerRadius)\">");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<DesktopAcrylicBackdrop />");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<Button Content=\"Button\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\"/>");
                    Assert.IsNull(page.Examples[0].CSharpCode);

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var layout = (Grid)root.Children[0];
                    Assert.AreEqual(2, layout.ColumnDefinitions.Count);

                    var example = FindDescendants<Grid>(page)
                        .FirstOrDefault(grid => grid.Width == 300d && grid.Height == 200d && grid.HorizontalAlignment == HorizontalAlignment.Center);
                    Assert.IsNotNull(example);
                    var dynamicBackdropHost = FindNamedDescendant<Border>(page, "DynamicBackdropHost");
                    Assert.IsNotNull(dynamicBackdropHost);
                    Assert.AreEqual(new CornerRadius(8), dynamicBackdropHost.CornerRadius);

                    var button = (Button)FindByAutomationId(page, "GallerySample_SystemBackdropElement_Button");
                    Assert.IsNotNull(button);
                    Assert.AreEqual("Click Me", button.Content);
                    Assert.AreEqual(HorizontalAlignment.Center, button.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, button.VerticalAlignment);

                    var backdropTypeComboBox = FindNamedDescendant<ComboBox>(page, "BackdropTypeComboBox");
                    Assert.IsNotNull(backdropTypeComboBox);
                    Assert.AreEqual("Backdrop Type", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(backdropTypeComboBox));
                    Assert.AreEqual(180d, backdropTypeComboBox.Width);
                    Assert.AreEqual(0, backdropTypeComboBox.SelectedIndex);
                    AssertSystemBackdropElementComboBoxItem(backdropTypeComboBox, 0, "Acrylic", "Acrylic");
                    AssertSystemBackdropElementComboBoxItem(backdropTypeComboBox, 1, "Mica", "Mica");
                    AssertSystemBackdropElementComboBoxItem(backdropTypeComboBox, 2, "Mica Alt", "MicaAlt");

                    var cornerRadiusSlider = FindNamedDescendant<Slider>(page, "CornerRadiusSlider");
                    Assert.IsNotNull(cornerRadiusSlider);
                    Assert.AreEqual("Corner radius", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(cornerRadiusSlider));
                    Assert.AreEqual(0d, cornerRadiusSlider.Minimum);
                    Assert.AreEqual(50d, cornerRadiusSlider.Maximum);
                    Assert.AreEqual(1d, cornerRadiusSlider.TickFrequency);
                    Assert.AreEqual(8d, cornerRadiusSlider.Value);

                    cornerRadiusSlider.Value = 24;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(new CornerRadius(24), dynamicBackdropHost.CornerRadius);

                    backdropTypeComboBox.SelectedIndex = 2;
                    WpfTestHost.DoEvents();
                    var brush = dynamicBackdropHost.Background as SolidColorBrush;
                    Assert.IsNotNull(brush);
                    Assert.AreEqual((Color)ColorConverter.ConvertFromString("#E9EEF5"), brush.Color);
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
        public void AppWindowSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("AppWindow"));
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
                    Assert.AreEqual("Creating and customizing an AppWindow from a Window instance", page.Examples[0].HeaderText);
                    Assert.AreEqual("Centering AppWindow on the screen using the available display area", page.Examples[1].HeaderText);
                    Assert.AreEqual("AppWindow with OverlapedPresenter", page.Examples[2].HeaderText);
                    Assert.AreEqual("Setting the minimum and maximum width / height on an AppWindow using OverlappedPresenter", page.Examples[3].HeaderText);
                    Assert.AreEqual("Modal window with OverlappedPresenter using AppWindow", page.Examples[4].HeaderText);
                    Assert.AreEqual("AppWindow with FullScreenPresenter", page.Examples[5].HeaderText);
                    Assert.AreEqual("AppWindow with CompactOverlayPresenter", page.Examples[6].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "x:Name=\"Hide\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "AppWindow.Title = \"$(WindowTitle)\";");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "AppWindow.Resize(new Windows.Graphics.SizeInt32($(Width), $(Height)));");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "OverlappedPresenter presenter = OverlappedPresenter.Create();");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "presenter.SetBorderAndTitleBar($(HasBorder), $(HasTitleBar));");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "presenter.PreferredMinimumWidth = MinWidth;");
                    StringAssert.Contains(page.Examples[4].CSharpCode, "OverlappedPresenter presenter = OverlappedPresenter.CreateForDialog();");
                    StringAssert.Contains(page.Examples[5].CSharpCode, "AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);");
                    StringAssert.Contains(page.Examples[6].CSharpCode, "CompactOverlayPresenter presenter = CompactOverlayPresenter.Create();");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var showSampleWindow1Button = (Button)FindByAutomationId(page, "GallerySample_AppWindow_ShowSampleWindow1Button");
                    Assert.IsNotNull(showSampleWindow1Button);
                    Assert.AreEqual("ShowSampleWindow1Button", showSampleWindow1Button.Name);
                    Assert.AreEqual("Show window", showSampleWindow1Button.Content);
                    Assert.AreEqual("Show window", AutomationProperties.GetName(showSampleWindow1Button));
                    Assert.AreEqual("GallerySample_AppWindow_ShowSampleWindow1Button", AutomationProperties.GetAutomationId(showSampleWindow1Button));

                    var titleTextBox = FindNamedDescendant<TextBox>(page, "WindowTitle");
                    Assert.IsNotNull(titleTextBox);
                    Assert.AreEqual("Window title", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(titleTextBox));
                    Assert.AreEqual("This is a title", titleTextBox.Text);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "WindowWidth"), "Width", 200, 1000, 800);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "WindowHeight"), "Height", 200, 700, 500);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "XPoint"), "X", 0, 800, 50);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "YPoint"), "Y", 0, 300, 50);

                    var centeredButton = FindNamedDescendant<Button>(page, "ShowSampleWindow2Button");
                    Assert.IsNotNull(centeredButton);
                    Assert.AreEqual("Show centered sample window", centeredButton.Content);

                    var overlappedWarning = FindDescendants<Mux.InfoBar>(page).FirstOrDefault(infoBar => string.Equals(infoBar.Title, "Warning", StringComparison.Ordinal));
                    Assert.IsNotNull(overlappedWarning);
                    Assert.IsTrue(overlappedWarning.IsOpen);
                    Assert.IsFalse(overlappedWarning.IsClosable);
                    Assert.AreEqual(Mux.InfoBarSeverity.Warning, overlappedWarning.Severity);
                    StringAssert.Contains(overlappedWarning.Message, "OverlappedPresenter");
                    var hasBorder = FindNamedDescendant<Mux.ToggleSwitch>(page, "HasBorder");
                    var hasTitleBar = FindNamedDescendant<Mux.ToggleSwitch>(page, "HasTitleBar");
                    AssertAppWindowToggleSwitch(FindNamedDescendant<Mux.ToggleSwitch>(page, "IsAlwaysOnTop"), "IsAlwaysOnTop", false);
                    AssertAppWindowToggleSwitch(FindNamedDescendant<Mux.ToggleSwitch>(page, "IsMaximizable"), "IsMaximizable", true);
                    AssertAppWindowToggleSwitch(FindNamedDescendant<Mux.ToggleSwitch>(page, "IsMinimizable"), "IsMinimizable", true);
                    AssertAppWindowToggleSwitch(FindNamedDescendant<Mux.ToggleSwitch>(page, "IsResizable"), "IsResizable", true);
                    AssertAppWindowToggleSwitch(hasBorder, "HasBorder", true);
                    AssertAppWindowToggleSwitch(hasTitleBar, "HasTitleBar", true);
                    hasBorder.IsOn = false;
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(hasTitleBar.IsOn);
                    hasTitleBar.IsOn = true;
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(hasBorder.IsOn);

                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "MinWidthBox"), "PreferredMinimumWidth", 0, double.PositiveInfinity, 400);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "MinHeightBox"), "PreferredMinimumHeight", 0, double.PositiveInfinity, 400);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "MaxWidthBox"), "PreferredMaximumWidth", 0, double.PositiveInfinity, 1000);
                    AssertAppWindowNumberBox(FindNamedDescendant<Mux.NumberBox>(page, "MaxHeightBox"), "PreferredMaximumHeight", 0, double.PositiveInfinity, 1000);
                    Assert.AreEqual("Show modal window", FindNamedDescendant<Button>(page, "ShowSampleWindow5Button").Content);
                    Assert.AreEqual("Show window (Fullscreen mode)", FindNamedDescendant<Button>(page, "ShowSampleWindow6Button").Content);

                    var initialSize = FindNamedDescendant<ComboBox>(page, "InitialSize");
                    Assert.IsNotNull(initialSize);
                    Assert.AreEqual("InitialSize", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(initialSize));
                    Assert.AreEqual(150d, initialSize.Width);
                    Assert.AreEqual(0, initialSize.SelectedIndex);
                    Assert.AreEqual("Small", initialSize.Items[0]);
                    Assert.AreEqual("Medium", initialSize.Items[1]);
                    Assert.AreEqual("Large", initialSize.Items[2]);
                    var initialSizeDescription = FindNamedDescendant<TextBlock>(page, "InitialSizeDescription");
                    Assert.IsNotNull(initialSizeDescription);
                    Assert.AreEqual(250d, initialSizeDescription.Width);
                    Assert.AreEqual("Small: Window size is approximately 5% of the display's work area.", initialSizeDescription.Text);
                    initialSize.SelectedIndex = 2;
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Large: Window size is approximately 25% of the display's work area.", initialSizeDescription.Text);
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
        public void CreateMultipleWindowsSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("CreateMultipleWindows"));
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
                    Assert.AreEqual("Create single threaded Multiple Top level Windows(MTW).", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.IsNull(page.Examples[0].XamlCode);
                    StringAssert.Contains(page.Examples[0].CSharpCode, "ExtendsContentIntoTitleBar = true");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "SystemBackdrop = new MicaBackdrop()");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "Text = \"New child window!\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "childWindow.AppWindow.ResizeClient(new SizeInt32(500, 500));");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "childWindow.Activate();");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);

                    var button = (Button)FindByAutomationId(page, "GallerySample_CreateMultipleWindows_Control1");
                    Assert.IsNotNull(button);
                    Assert.AreSame(button, root.Children[0]);
                    Assert.AreEqual("Control1", button.Name);
                    Assert.AreEqual("Create new Window", button.Content);
                    Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
                    Assert.AreEqual("Create new Window", AutomationProperties.GetName(button));
                    Assert.AreEqual("GallerySample_CreateMultipleWindows_Control1", AutomationProperties.GetAutomationId(button));
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
        public void AppWindowTitleBarSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("AppWindowTitleBar"));
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
                    Assert.AreEqual(new Thickness(0, 8, 0, 0), intro.Margin);
                    Assert.AreEqual(TextWrapping.Wrap, intro.TextWrapping);
                    Assert.AreEqual(
                        "For the default title bar and basic scenarios, use the TitleBar control.",
                        new TextRange(intro.ContentStart, intro.ContentEnd).Text.Trim());

                    Assert.AreEqual(3, page.Examples.Count);
                    Assert.AreEqual("AppWindowTitleBar color customization", page.Examples[0].HeaderText);
                    Assert.AreEqual("Extending content into the AppWindowTitleBar area", page.Examples[1].HeaderText);
                    Assert.AreEqual("AppWindowTitleBar preferred theme and height options", page.Examples[2].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    Assert.IsNull(page.Examples[0].XamlCode);
                    Assert.IsNull(page.Examples[1].XamlCode);
                    Assert.IsNull(page.Examples[2].XamlCode);
                    StringAssert.Contains(page.Examples[0].CSharpCode, "AppWindow.TitleBar.BackgroundColor = ColorHelper.FromArgb($(BackgroundColor));");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "AppWindow.TitleBar.ButtonPressedForegroundColor = ColorHelper.FromArgb($(ButtonPressedForegroundColor));");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "AppWindow.TitleBar.ExtendsContentIntoTitleBar = $(ExtendsContentIntoTitleBar);");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "AppWindow.TitleBar.HeightOption = TitleBarHeightOption.$(TitleBarHeightOption);");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "AppWindow.TitleBar.PreferredTheme = TitleBarTheme.$(PreferredTheme);");

                    var colorRoot = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(2, colorRoot.Children.Count);
                    var showWindowButton = (Button)FindByAutomationId(page, "GallerySample_AppWindowTitleBar_ShowWindowButton");
                    Assert.IsNotNull(showWindowButton);
                    Assert.AreSame(showWindowButton, colorRoot.Children[0]);
                    Assert.AreEqual("ShowWindowButton", showWindowButton.Name);
                    Assert.AreEqual("Show window", showWindowButton.Content);
                    Assert.AreEqual("Show window", AutomationProperties.GetName(showWindowButton));

                    var options = (Grid)colorRoot.Children[1];
                    Assert.AreEqual(3, options.ColumnDefinitions.Count);
                    AssertTitleBarColorSelector(page, "Background", "BackgroundColor", "#FFF2F6FA");
                    AssertTitleBarColorSelector(page, "Foreground", "ForegroundColor", "#FF1E2933");
                    AssertTitleBarColorSelector(page, "ButtonBackground", "ButtonBackgroundColor", "#FF3B82F6");
                    AssertTitleBarColorSelector(page, "ButtonForeground", "ButtonForegroundColor", "#FFFFFFFF");
                    AssertTitleBarColorSelector(page, "ButtonHoverBackground", "ButtonHoverBackgroundColor", "#FF2563EB");
                    AssertTitleBarColorSelector(page, "ButtonHoverForeground", "ButtonHoverForegroundColor", "#FFFFFFFF");
                    AssertTitleBarColorSelector(page, "InactiveBackground", "InactiveBackgroundColor", "#FFE5EAF0");
                    AssertTitleBarColorSelector(page, "InactiveForeground", "InactiveForegroundColor", "#FF6B7280");
                    AssertTitleBarColorSelector(page, "ButtonInactiveBackground", "ButtonInactiveBackgroundColor", "#FFCBD5E1");
                    AssertTitleBarColorSelector(page, "ButtonInactiveForeground", "ButtonInactiveForegroundColor", "#FF475569");
                    AssertTitleBarColorSelector(page, "ButtonPressedBackground", "ButtonPressedBackgroundColor", "#FF1D4ED8");
                    AssertTitleBarColorSelector(page, "ButtonPressedForeground", "ButtonPressedForegroundColor", "#FFFFFFFF");

                    var extendRoot = (GallerySamplePanel)page.Examples[1].ExampleContent;
                    Assert.AreEqual(2, extendRoot.Children.Count);
                    var showExtendButton = FindNamedDescendant<Button>(page, "ShowExtendButton");
                    Assert.IsNotNull(showExtendButton);
                    Assert.AreEqual("Show window", showExtendButton.Content);
                    var extendContentCheckBox = FindNamedDescendant<CheckBox>(page, "ExtendContentCheckBox");
                    Assert.IsNotNull(extendContentCheckBox);
                    Assert.AreEqual("Extend content into title bar", extendContentCheckBox.Content);
                    Assert.AreEqual(true, extendContentCheckBox.IsChecked);
                    Assert.AreEqual(new Thickness(0, 0, 0, 12), extendContentCheckBox.Margin);
                    var heightComboBox = FindNamedDescendant<ComboBox>(page, "HeightComboBox");
                    Assert.IsNotNull(heightComboBox);
                    Assert.AreEqual(200d, heightComboBox.Width);
                    Assert.AreEqual("TitleBarHeightOption", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(heightComboBox));
                    Assert.AreEqual(0, heightComboBox.SelectedIndex);
                    Assert.AreEqual("Standard", heightComboBox.Items[0]);
                    Assert.AreEqual("Tall", heightComboBox.Items[1]);

                    var themeRoot = (GallerySamplePanel)page.Examples[2].ExampleContent;
                    Assert.AreEqual(2, themeRoot.Children.Count);
                    var showThemeHeightButton = FindNamedDescendant<Button>(page, "ShowThemeHeightButton");
                    Assert.IsNotNull(showThemeHeightButton);
                    Assert.AreEqual("Show window", showThemeHeightButton.Content);
                    var themeComboBox = FindNamedDescendant<ComboBox>(page, "ThemeComboBox");
                    Assert.IsNotNull(themeComboBox);
                    Assert.AreEqual(200d, themeComboBox.Width);
                    Assert.AreEqual("TitleBarTheme", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(themeComboBox));
                    Assert.AreEqual(1, themeComboBox.SelectedIndex);
                    Assert.AreEqual("UseDefaultAppMode", themeComboBox.Items[0]);
                    Assert.AreEqual("Light", themeComboBox.Items[1]);
                    Assert.AreEqual("Dark", themeComboBox.Items[2]);
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
                        "For full title bar customization without using the TitleBar control, see the AppWindowTitleBar sample",
                        new TextRange(intro.ContentStart, intro.ContentEnd).Text.Trim());

                    Assert.AreEqual(2, page.Examples.Count);
                    Assert.AreEqual("TitleBar configuration", page.Examples[0].HeaderText);
                    Assert.AreEqual("End to end TitleBar sample", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<TitleBar");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Title=\"$(Title)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Subtitle=\"$(Subtitle)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsBackButtonVisible=\"$(BackButtonVisibility)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "IsPaneToggleButtonVisible=\"$(PaneToggleVisibility)\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "<ImageIconSource ImageSource=\"/Assets/Tiles/GalleryIcon.ico\" />");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    StringAssert.Contains(page.Examples[1].XamlCode, "x:Name=\"titleBar\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "PaneToggleRequested=\"TitleBar_PaneToggleRequested\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<NavigationView");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "this.ExtendsContentIntoTitleBar = true;");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "this.SetTitleBar(titleBar);");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var titleBarControl = (ContentControl)FindByAutomationId(page, "GallerySample_TitleBar_TitleBarControl");
                    Assert.IsNotNull(titleBarControl);
                    Assert.AreEqual("TitleBarControl", titleBarControl.Name);
                    Assert.AreEqual(470d, titleBarControl.Width);
                    Assert.AreEqual(48d, titleBarControl.Height);
                    Assert.AreEqual("TitleBarControl", AutomationProperties.GetName(titleBarControl));

                    var titleBox = FindNamedDescendant<TextBox>(page, "TitleBox");
                    Assert.IsNotNull(titleBox);
                    Assert.AreEqual("Title", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(titleBox));
                    Assert.AreEqual("WinUI Gallery", titleBox.Text);
                    var subtitleBox = FindNamedDescendant<TextBox>(page, "SubtitleBox");
                    Assert.IsNotNull(subtitleBox);
                    Assert.AreEqual("Subtitle", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(subtitleBox));
                    Assert.AreEqual("Preview", subtitleBox.Text);
                    var backButtonToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "BackButtonToggle");
                    Assert.IsNotNull(backButtonToggle);
                    Assert.AreEqual("IsBackButtonVisible", backButtonToggle.Header);
                    Assert.IsFalse(backButtonToggle.IsOn);
                    var paneToggle = FindNamedDescendant<Mux.ToggleSwitch>(page, "PaneToggle");
                    Assert.IsNotNull(paneToggle);
                    Assert.AreEqual("IsPaneToggleButtonVisible", paneToggle.Header);
                    Assert.IsFalse(paneToggle.IsOn);

                    var titleText = FindNamedDescendant<TextBlock>(page, "TitleText");
                    var subtitleText = FindNamedDescendant<TextBlock>(page, "SubtitleText");
                    Assert.IsNotNull(titleText);
                    Assert.IsNotNull(subtitleText);
                    Assert.AreEqual("WinUI Gallery", titleText.Text);
                    Assert.AreEqual("Preview", subtitleText.Text);

                    var endToEndRoot = (GallerySamplePanel)page.Examples[1].ExampleContent;
                    Assert.AreEqual(1, endToEndRoot.Children.Count);
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
        public void StoragePickersSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("StoragePickers"));
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
                    var intro = page.IntroContent as Mux.InfoBar;
                    Assert.IsNotNull(intro);
                    Assert.IsTrue(intro.IsOpen);
                    Assert.IsFalse(intro.IsClosable);
                    Assert.AreEqual(new Thickness(0, 8, 0, 0), intro.Margin);
                    StringAssert.Contains(intro.Message, "The picker reopens in the last selected location and view.");
                    StringAssert.Contains(intro.Message, "SuggestedStartLocation");
                    StringAssert.Contains(intro.Message, "ViewMode");

                    Assert.AreEqual(4, page.Examples.Count);
                    Assert.AreEqual("Pick single file", page.Examples[0].HeaderText);
                    Assert.AreEqual("Pick multiple files", page.Examples[1].HeaderText);
                    Assert.AreEqual("Save file", page.Examples[2].HeaderText);
                    Assert.AreEqual("Pick folder", page.Examples[3].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "PickSingleFileButton");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);$(FileType)");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "picker.CommitButtonText = \"$(CommitButtonText)\";");
                    StringAssert.Contains(page.Examples[1].XamlCode, "PickMultipleFilesButton");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "var files = await picker.PickMultipleFilesAsync();");
                    StringAssert.Contains(page.Examples[2].XamlCode, "FileContentTextBox");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "var picker = new FileSavePicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "picker.SuggestedFolder = \"$(SuggestedFolder)\";");
                    StringAssert.Contains(page.Examples[3].XamlCode, "PickFolderButton");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "var picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);");

                    var pickSingleButton = (Button)FindByAutomationId(page, "GallerySample_StoragePickers_PickSingleFileButton");
                    Assert.IsNotNull(pickSingleButton);
                    Assert.AreEqual("PickSingleFileButton", pickSingleButton.Name);
                    Assert.AreEqual("Pick a single file", pickSingleButton.Content);
                    Assert.AreEqual("Pick a single file", AutomationProperties.GetName(pickSingleButton));
                    var pickedSingleText = FindNamedDescendant<TextBlock>(page, "PickedSingleFileTextBlock");
                    Assert.IsNotNull(pickedSingleText);
                    Assert.AreEqual("No file picked", pickedSingleText.Text);

                    var fileTypeComboBox1 = FindNamedDescendant<ComboBox>(page, "FileTypeComboBox1");
                    AssertStoragePickerComboBox(fileTypeComboBox1, "File type", 200, "All Files (*)", "Text Files (*.txt)", "Images (*.jpg, *.png)");
                    var commitButtonTextTextBox = FindNamedDescendant<TextBox>(page, "CommitButtonTextTextBox");
                    Assert.IsNotNull(commitButtonTextTextBox);
                    Assert.AreEqual("Commit button text", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(commitButtonTextTextBox));
                    Assert.AreEqual("Pick File", commitButtonTextTextBox.Text);
                    AssertStoragePickerComboBox(FindNamedDescendant<ComboBox>(page, "PickerLocationComboBox1"), "Suggested start location", 200, "DocumentsLibrary");
                    AssertStoragePickerComboBox(FindNamedDescendant<ComboBox>(page, "PickerViewModeComboBox1"), "View mode", 200, "List", "Thumbnail");

                    var pickMultipleButton = FindNamedDescendant<Button>(page, "PickMultipleFilesButton");
                    Assert.IsNotNull(pickMultipleButton);
                    Assert.AreEqual("Pick multiple files", pickMultipleButton.Content);
                    Assert.AreEqual("No files picked", FindNamedDescendant<TextBlock>(page, "PickedMultipleFilesTextBlock").Text);
                    Assert.AreEqual("Pick Files", FindNamedDescendant<TextBox>(page, "CommitButtonTextTextBox2").Text);

                    var fileContentTextBox = FindNamedDescendant<TextBox>(page, "FileContentTextBox");
                    Assert.IsNotNull(fileContentTextBox);
                    Assert.AreEqual("File content", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(fileContentTextBox));
                    Assert.AreEqual(500d, fileContentTextBox.Width);
                    Assert.AreEqual(200d, fileContentTextBox.Height);
                    Assert.AreEqual("Hello, WinUI!", fileContentTextBox.Text);
                    Assert.IsTrue(fileContentTextBox.AcceptsReturn);
                    Assert.AreEqual("Save a file", FindNamedDescendant<Button>(page, "SaveFileButton").Content);
                    Assert.AreEqual("No file saved", FindNamedDescendant<TextBlock>(page, "SavedFileTextBlock").Text);
                    Assert.AreEqual("Text Files (*.txt)", FindNamedDescendant<CheckBox>(page, "TxtCheckBox").Content);
                    Assert.AreEqual("JSON Files (*.json)", FindNamedDescendant<CheckBox>(page, "JsonCheckBox").Content);
                    Assert.AreEqual("XML Files (*.xml)", FindNamedDescendant<CheckBox>(page, "XmlCheckBox").Content);
                    AssertStoragePickerComboBox(FindNamedDescendant<ComboBox>(page, "DefaultExtensionComboBox"), "Default extension", 200, ".txt", ".json", ".xml");
                    Assert.AreEqual("NewDocument", FindNamedDescendant<TextBox>(page, "SuggestedFileNameTextBox").Text);
                    Assert.AreEqual("Save File", FindNamedDescendant<TextBox>(page, "CommitButtonTextTextBox3").Text);
                    Assert.AreEqual("Select folder", AutomationProperties.GetName(FindNamedDescendant<Button>(page, "SelectSuggestedFolderButton")));

                    var pickFolderButton = FindNamedDescendant<Button>(page, "PickFolderButton");
                    Assert.IsNotNull(pickFolderButton);
                    Assert.AreEqual("Pick a folder", pickFolderButton.Content);
                    Assert.AreEqual("No folder picked", FindNamedDescendant<TextBlock>(page, "PickedFolderTextBlock").Text);
                    Assert.AreEqual("Pick Folder", FindNamedDescendant<TextBox>(page, "CommitButtonTextTextBox4").Text);
                    AssertStoragePickerComboBox(FindNamedDescendant<ComboBox>(page, "PickerViewModeComboBox3"), "View mode", 200, "List", "Thumbnail");
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
        public void WebView2SampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("WebView2"));
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
                    Assert.AreEqual("A simple WebView2 ", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<WebView2 x:Name=\"MyWebView2\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Source=\"https://learn.microsoft.com/windows/apps/winui/winui3/\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "HorizontalAlignment=\"Stretch\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "Grid.Row=\"1\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "MinHeight=\"200\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "MinWidth=\"200\"");
                    Assert.IsNull(page.Examples[0].CSharpCode);

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(1, root.Children.Count);
                    var layout = (Grid)root.Children[0];
                    Assert.AreEqual(2, layout.RowDefinitions.Count);
                    Assert.AreEqual(GridLength.Auto, layout.RowDefinitions[0].Height);
                    Assert.AreEqual(GridUnitType.Star, layout.RowDefinitions[1].Height.GridUnitType);

                    var description = (TextBlock)layout.Children[0];
                    Assert.AreEqual(new Thickness(0, 0, 0, 12), description.Margin);
                    Assert.AreEqual(TextWrapping.Wrap, description.TextWrapping);
                    Assert.AreEqual("WebView2 is powered by the Chromium engine.", description.Text);

                    var browser = (Border)FindByAutomationId(page, "GallerySample_WebView2_WebView2");
                    Assert.IsNotNull(browser);
                    Assert.AreEqual("MyWebView2", browser.Name);
                    Assert.AreEqual(200d, browser.MinWidth);
                    Assert.AreEqual(200d, browser.MinHeight);
                    Assert.AreEqual(HorizontalAlignment.Stretch, browser.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Stretch, browser.VerticalAlignment);
                    Assert.AreEqual("https://learn.microsoft.com/windows/apps/winui/winui3/", browser.Tag);
                    Assert.AreEqual("MyWebView2", AutomationProperties.GetName(browser));
                    Assert.AreEqual("GallerySample_WebView2_WebView2", AutomationProperties.GetAutomationId(browser));
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
        public void MapControlSampleMatchesWinUIGalleryExample()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("MapControl"));
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

                    var intro = (StackPanel)page.IntroContent;
                    Assert.IsNotNull(intro);
                    Assert.AreEqual(2, intro.Children.Count);
                    var instructions = (TextBlock)intro.Children[0];
                    Assert.AreEqual(12d, instructions.Margin.Bottom);
                    Assert.AreEqual("Follow instructions here to obtain your MapServiceToken.", new TextRange(instructions.ContentStart, instructions.ContentEnd).Text.Trim());
                    var mapImage = (Image)intro.Children[1];
                    Assert.AreEqual(320d, mapImage.Height);
                    StringAssert.Contains(((BitmapImage)mapImage.Source).UriSource.ToString(), "MapExample.png");

                    Assert.AreEqual(1, page.Examples.Count);
                    Assert.AreEqual("Showing a pin on the map", page.Examples[0].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<MapControl x:Name=\"map1\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "MapServiceToken=\"MapServiceToken\"");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "map1.Center = centerPoint;");
                    StringAssert.Contains(page.Examples[0].CSharpCode, "map1.Layers.Add(LandmarksLayer);");

                    var root = (GallerySamplePanel)page.Examples[0].ExampleContent;
                    Assert.AreEqual(2, root.Children.Count);
                    var tokenRow = (StackPanel)root.Children[0];
                    Assert.AreEqual(Orientation.Horizontal, tokenRow.Orientation);
                    Assert.AreEqual(12d, tokenRow.Margin.Bottom);

                    var token = FindNamedDescendant<PasswordBox>(page, "MapToken");
                    var map = (FrameworkElement)FindByAutomationId(page, "GallerySample_MapControl_MapControl");
                    Assert.IsNotNull(token);
                    Assert.IsNotNull(map);

                    Assert.AreEqual(200d, token.MinWidth);
                    Assert.AreEqual("Map service token", AutomationProperties.GetName(token));
                    Assert.AreEqual("GallerySample_MapControl_MapToken", AutomationProperties.GetAutomationId(token));
                    Assert.AreEqual("Map service token", ControlHelper.GetPlaceholderText(token));

                    var setTokenButton = (Button)tokenRow.Children[1];
                    Assert.AreEqual("Set token", setTokenButton.Content);
                    Assert.AreEqual(8d, setTokenButton.Margin.Left);

                    Assert.AreEqual("map1", map.Name);
                    Assert.AreEqual(400d, map.Height);
                    Assert.AreEqual(400d, map.MinWidth);
                    Assert.AreEqual(HorizontalAlignment.Stretch, map.HorizontalAlignment);
                    Assert.AreEqual("Map", AutomationProperties.GetName(map));
                    Assert.AreEqual("Center=0,0; ZoomLevel=1; Pin=-30.034647,-51.217659", map.Tag);
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
        public void RichEditBoxSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("RichEditBox"));
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

                    Assert.AreEqual(5, page.Examples.Count);
                    Assert.AreEqual("A simple text editor using RichEditBox.", page.Examples[0].HeaderText);
                    Assert.AreEqual("Customizing RichEditBox's CommandBarFlyout - Adding 'Share'", page.Examples[1].HeaderText);
                    Assert.AreEqual("A custom editor with RichEditBox.", page.Examples[2].HeaderText);
                    Assert.AreEqual("Rich edit box in math mode", page.Examples[3].HeaderText);
                    Assert.AreEqual("Working with MathML in RichEditBox", page.Examples[4].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "simple text editor");
                    StringAssert.Contains(page.Examples[1].XamlCode, "REBCustom");
                    StringAssert.Contains(page.Examples[1].CSharpCode, "StandardUICommandKind.Share");
                    StringAssert.Contains(page.Examples[2].XamlCode, "fontColorButton");
                    StringAssert.Contains(page.Examples[2].CSharpCode, "OpenButton_Click");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "SetMathMode");
                    StringAssert.Contains(page.Examples[4].XamlCode, "mathEditor2");
                    StringAssert.Contains(page.Examples[4].CSharpCode, "SetMathmlFormulaBtn_Click");

                    var simple = (RichTextBox)FindByAutomationId(page, "GallerySample_RichEditBox_RichEditBox");
                    var customMenu = FindNamedDescendant<RichTextBox>(page, "REBCustom");
                    var openFileButton = FindNamedDescendant<Button>(page, "openFileButton");
                    var italicButton = FindNamedDescendant<Button>(page, "italicButton");
                    var fontColorButton = FindNamedDescendant<Mux.DropDownButton>(page, "fontColorButton");
                    var findBox = FindNamedDescendant<TextBox>(page, "findBox");
                    var mathEditor = FindNamedDescendant<RichTextBox>(page, "MathEditor");
                    var mathEditor2 = FindNamedDescendant<RichTextBox>(page, "mathEditor2");
                    var mathmlPresenter = FindNamedDescendant<TextBox>(page, "MathmlPresenter");
                    var setFormulaButton = FindNamedDescendant<Button>(page, "SetMathmlFormulaBtn");
                    Assert.IsNotNull(simple);
                    Assert.IsNotNull(customMenu);
                    Assert.IsNotNull(openFileButton);
                    Assert.IsNotNull(italicButton);
                    Assert.IsNotNull(fontColorButton);
                    Assert.IsNotNull(findBox);
                    Assert.IsNotNull(mathEditor);
                    Assert.IsNotNull(mathEditor2);
                    Assert.IsNotNull(mathmlPresenter);
                    Assert.IsNotNull(setFormulaButton);

                    Assert.AreEqual("editor", simple.Name);
                    Assert.AreEqual("simple text editor", AutomationProperties.GetName(simple));
                    Assert.AreEqual("editor with custom menu", AutomationProperties.GetName(customMenu));
                    Assert.AreEqual("Open file", AutomationProperties.GetName(openFileButton));
                    Assert.AreEqual("Italic", AutomationProperties.GetName(italicButton));
                    Assert.AreEqual("Font color", AutomationProperties.GetName(fontColorButton));
                    Assert.AreEqual("Enter search text", ControlHelper.GetPlaceholderText(findBox));
                    Assert.AreEqual(16, mathEditor.FontSize);
                    Assert.AreEqual(16, mathEditor2.FontSize);
                    Assert.AreEqual("<!-- No MathML content -->", mathmlPresenter.Text);

                    setFormulaButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    StringAssert.Contains(mathmlPresenter.Text, "<mml:math");
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
        public void RichTextBlockSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("RichTextBlock"));
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
                    Assert.AreEqual("A simple RichTextBlock.", page.Examples[0].HeaderText);
                    Assert.AreEqual("A RichTextBlock with a custom selection highlight color.", page.Examples[1].HeaderText);
                    Assert.AreEqual("A RichTextBlock with overflow.", page.Examples[2].HeaderText);
                    Assert.AreEqual("RichTextBlock with custom TextHighlighting", page.Examples[3].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "<Paragraph>I am a RichTextBlock.</Paragraph>");
                    StringAssert.Contains(page.Examples[1].XamlCode, "SelectionHighlightColor=\"Green\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "formatted text");
                    StringAssert.Contains(page.Examples[2].XamlCode, "firstOverflowContainer");
                    StringAssert.Contains(page.Examples[3].XamlCode, "TextHighlightingRichTextBlock");
                    StringAssert.Contains(page.Examples[3].CSharpCode, "TextHighlighter");

                    var simple = (TextBlock)FindByAutomationId(page, "GallerySample_RichTextBlock_RichTextBlock");
                    var selection = FindNamedDescendant<TextBlock>(page, "SelectionHighlightRichTextBlock");
                    var firstOverflow = FindNamedDescendant<TextBlock>(page, "firstOverflowContainer");
                    var secondOverflow = FindNamedDescendant<TextBlock>(page, "secondOverflowContainer");
                    var highlighted = FindNamedDescendant<TextBlock>(page, "TextHighlightingRichTextBlock");
                    Assert.IsNotNull(simple);
                    Assert.IsNotNull(selection);
                    Assert.IsNotNull(firstOverflow);
                    Assert.IsNotNull(secondOverflow);
                    Assert.IsNotNull(highlighted);

                    Assert.AreEqual("SimpleRichTextBlock", simple.Name);
                    Assert.AreEqual("I am a RichTextBlock.", simple.Text);
                    Assert.AreEqual(TextWrapping.Wrap, selection.TextWrapping);
                    Assert.AreEqual(TextAlignment.Justify, firstOverflow.TextAlignment);
                    Assert.AreEqual(TextAlignment.Justify, secondOverflow.TextAlignment);
                    StringAssert.Contains(GetInlineText(selection), "RichTextBlock provides a rich text display container");
                    StringAssert.Contains(GetInlineText(highlighted), "Lorem ipsum dolor sit amet");
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        private static string GetInlineText(TextBlock textBlock)
        {
            var builder = new StringBuilder();
            AppendInlineText(builder, textBlock.Inlines);
            return builder.ToString();
        }

        private static void AppendInlineText(StringBuilder builder, InlineCollection inlines)
        {
            foreach (Inline inline in inlines)
            {
                AppendInlineText(builder, inline);
            }
        }

        private static void AppendInlineText(StringBuilder builder, Inline inline)
        {
            switch (inline)
            {
                case Run run:
                    builder.Append(run.Text);
                    break;
                case LineBreak:
                    builder.AppendLine();
                    break;
                case Span span:
                    AppendInlineText(builder, span.Inlines);
                    break;
            }
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
                    StringAssert.Contains(page.Examples[0].XamlCode, "DefaultLabelPosition=\"Right\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "MultipleButtonsSecondaryCommands");

                    var commandBar = (Mux.CommandBar)FindByAutomationId(page, "GallerySample_CommandBar_CommandBar");
                    Assert.IsNotNull(commandBar);
                    Assert.AreEqual(Mux.CommandBarDefaultLabelPosition.Right, commandBar.DefaultLabelPosition);
                    Assert.AreEqual(HorizontalAlignment.Left, commandBar.HorizontalAlignment);
                    Assert.IsFalse(commandBar.IsOpen);
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

                    var selectedOptionText = FindNamedDescendant<TextBlock>(page, "SelectedOptionText");
                    Assert.IsNotNull(selectedOptionText);
                    addButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual("You clicked: Add", selectedOptionText.Text);

                    var openButton = FindButtonByContent(page, "Open command bar");
                    var closeButton = FindButtonByContent(page, "Close command bar");
                    var addSecondaryCommandsButton = FindButtonByContent(page, "Add secondary commands");
                    var removeSecondaryCommandsButton = FindButtonByContent(page, "Remove secondary commands");
                    Assert.IsNotNull(openButton);
                    Assert.IsNotNull(closeButton);
                    Assert.IsNotNull(addSecondaryCommandsButton);
                    Assert.IsNotNull(removeSecondaryCommandsButton);

                    openButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.IsTrue(commandBar.IsOpen);
                    closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.IsFalse(commandBar.IsOpen);

                    addSecondaryCommandsButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    Assert.AreEqual(6, commandBar.SecondaryCommands.Count);
                    Assert.AreEqual("Button 1", ((Mux.AppBarButton)commandBar.SecondaryCommands[1]).Label);
                    Assert.AreEqual("Ctrl+N", ((Mux.AppBarButton)commandBar.SecondaryCommands[1]).InputGestureText);
                    Assert.IsInstanceOfType(commandBar.SecondaryCommands[3], typeof(Mux.AppBarSeparator));
                    Assert.AreEqual("Button 4", ((Mux.AppBarButton)commandBar.SecondaryCommands[5]).Label);

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
                    var rootArtifact = Path.Combine(artifactDirectory, "GallerySample_InfoBar_Root.png");
                    Assert.IsTrue(File.Exists(infoBarArtifact), infoBarArtifact + " was not written.");
                    Assert.IsTrue(File.Exists(rootArtifact), rootArtifact + " was not written.");
                    Assert.IsTrue(new FileInfo(infoBarArtifact).Length > 0);
                    Assert.IsTrue(new FileInfo(rootArtifact).Length > 0);
                    Assert.IsTrue(HasVisibleRgbPixels(infoBarArtifact), infoBarArtifact + " has no visible RGB content.");
                    Assert.IsTrue(HasVisibleRgbPixels(rootArtifact), rootArtifact + " has no visible RGB content.");
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
        public void UserDashboardWritesNamedContentRootGridVisualArtifact()
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

                    var contentRootArtifact = Path.Combine(artifactDirectory, "ContentRootGrid.png");
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
                    var openContentArtifact = Path.Combine(artifactDirectory, "ContentRootGrid.png");
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

        private static void InvokeSwipeItem(Mux.SwipeItem item, Mux.SwipeControl swipeControl)
        {
            var invokeMethod = typeof(Mux.SwipeItem).GetMethod(
                "Invoke",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(invokeMethod);
            invokeMethod.Invoke(item, new object[] { swipeControl });
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

        private static void AssertAcrylicSlider(Slider slider, string automationName, double value)
        {
            Assert.IsNotNull(slider);
            Assert.AreEqual(automationName, AutomationProperties.GetName(slider));
            Assert.AreEqual(200d, slider.Width);
            Assert.AreEqual(HorizontalAlignment.Left, slider.HorizontalAlignment);
            Assert.AreEqual(0d, slider.Minimum);
            Assert.AreEqual(1d, slider.Maximum);
            Assert.AreEqual(0.001, slider.SmallChange, 0.0001);
            Assert.AreEqual(0.001, slider.TickFrequency, 0.0001);
            Assert.AreEqual(value, slider.Value, 0.001);
        }

        private static void AssertAcrylicColorSelector(ComboBox comboBox, string automationName, params Color[] colors)
        {
            Assert.IsNotNull(comboBox);
            Assert.AreEqual(automationName, AutomationProperties.GetName(comboBox));
            Assert.AreEqual(0, comboBox.SelectedIndex);
            Assert.AreEqual(colors.Length, comboBox.Items.Count);

            for (var i = 0; i < colors.Length; i++)
            {
                var item = (ComboBoxItem)comboBox.Items[i];
                Assert.AreEqual(colors[i], (Color)item.Tag);

                var content = (StackPanel)item.Content;
                Assert.AreEqual(Orientation.Horizontal, content.Orientation);
                Assert.AreEqual(colors[i].ToString(), AutomationProperties.GetName(content));

                var swatch = (WpfShapes.Rectangle)content.Children[0];
                Assert.AreEqual(20d, swatch.Width);
                Assert.AreEqual(20d, swatch.Height);
                Assert.AreEqual(colors[i], ((SolidColorBrush)swatch.Fill).Color);
                Assert.AreEqual(colors[i].ToString(), ((TextBlock)content.Children[1]).Text);
            }
        }

        private static void AssertPipsPagerImage(ContentControl gallery, string fileName)
        {
            var image = gallery.Content as Image;
            Assert.IsNotNull(image);
            var bitmapImage = image.Source as BitmapImage;
            Assert.IsNotNull(bitmapImage);
            StringAssert.Contains(bitmapImage.UriSource.ToString(), fileName);
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

        private static Slider AssertRadialGradientSlider(DependencyObject root, string name, string header, double maximum, double value, double tickFrequency, double smallChange)
        {
            var slider = FindNamedDescendant<Slider>(root, name);
            Assert.IsNotNull(slider);
            Assert.AreEqual(header, ModernWpf.Controls.Primitives.ControlHelper.GetHeader(slider));
            Assert.AreEqual(maximum, slider.Maximum);
            Assert.AreEqual(value, slider.Value);
            Assert.AreEqual(tickFrequency, slider.TickFrequency);
            Assert.AreEqual(smallChange, slider.SmallChange);
            return slider;
        }

        private static Slider AssertLineSlider(DependencyObject root, string name, string header, double minimum, double maximum, double value)
        {
            var slider = FindNamedDescendant<Slider>(root, name);
            Assert.IsNotNull(slider);
            Assert.AreEqual(header, ModernWpf.Controls.Primitives.ControlHelper.GetHeader(slider));
            Assert.AreEqual(minimum, slider.Minimum);
            Assert.AreEqual(maximum, slider.Maximum);
            Assert.AreEqual(value, slider.Value);
            Assert.AreEqual(1d, slider.SmallChange);
            Assert.AreEqual(0.5d, slider.TickFrequency);
            return slider;
        }

        private static SolidColorBrush CreateBrushForTest(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }

        private static void AssertStoragePickerComboBox(ComboBox comboBox, string header, double width, params string[] expectedItems)
        {
            Assert.IsNotNull(comboBox);
            Assert.AreEqual(header, ModernWpf.Controls.Primitives.ControlHelper.GetHeader(comboBox));
            Assert.AreEqual(width, comboBox.Width);
            Assert.AreEqual(0, comboBox.SelectedIndex);
            Assert.IsTrue(comboBox.Items.Count >= expectedItems.Length);
            for (var i = 0; i < expectedItems.Length; i++)
            {
                var item = comboBox.Items[i] as ComboBoxItem;
                Assert.AreEqual(expectedItems[i], item == null ? comboBox.Items[i] : item.Content);
            }
        }

        private static void AssertSystemBackdropElementComboBoxItem(ComboBox comboBox, int index, string content, string tag)
        {
            var item = comboBox.Items[index] as ComboBoxItem;
            Assert.IsNotNull(item);
            Assert.AreEqual(content, item.Content);
            Assert.AreEqual(tag, item.Tag);
        }

        private static void AssertSystemBackdropsShowWindowButton(Button button, string elementName)
        {
            Assert.IsNotNull(button);
            Assert.AreEqual(elementName, button.Name);
            Assert.AreEqual("Show window", button.Content);
            Assert.AreEqual(new Thickness(0, 10, 0, 0), button.Margin);
            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
            Assert.AreEqual("Show window", AutomationProperties.GetName(button));
            Assert.AreEqual("GallerySample_SystemBackdrops_" + elementName, AutomationProperties.GetAutomationId(button));
        }

        private static void AssertAppWindowNumberBox(Mux.NumberBox numberBox, string header, double minimum, double maximum, double value)
        {
            Assert.IsNotNull(numberBox);
            Assert.AreEqual(header, numberBox.Header);
            Assert.AreEqual(minimum, numberBox.Minimum);
            Assert.AreEqual(maximum, numberBox.Maximum);
            Assert.AreEqual(value, numberBox.Value);
            Assert.AreEqual(10d, numberBox.SmallChange);
            Assert.AreEqual(100d, numberBox.LargeChange);
            Assert.AreEqual(Mux.NumberBoxSpinButtonPlacementMode.Inline, numberBox.SpinButtonPlacementMode);
        }

        private static void AssertAppWindowToggleSwitch(Mux.ToggleSwitch toggleSwitch, string header, bool isOn)
        {
            Assert.IsNotNull(toggleSwitch);
            Assert.AreEqual(header, toggleSwitch.Header);
            Assert.AreEqual(isOn, toggleSwitch.IsOn);
            Assert.AreEqual("true", toggleSwitch.OnContent);
            Assert.AreEqual("false", toggleSwitch.OffContent);
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

        private static void AssertPivotItem(TabItem item, string header, string text)
        {
            Assert.IsNotNull(item);
            Assert.AreEqual(header, item.Header);
            Assert.AreSame(item.TryFindResource("TabItemPivotStyle"), item.Style);
            var textBlock = item.Content as TextBlock;
            Assert.IsNotNull(textBlock);
            Assert.AreEqual(text, textBlock.Text);
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

        private static void AssertTabViewSamplePage(TabItem item, int expectedColumnCount, int expectedRowCount)
        {
            var scrollViewer = item.Content as ScrollViewer;
            Assert.IsNotNull(scrollViewer);
            var grid = scrollViewer.Content as Grid;
            Assert.IsNotNull(grid);
            Assert.AreEqual(expectedColumnCount, grid.ColumnDefinitions.Count);
            Assert.AreEqual(expectedRowCount, grid.RowDefinitions.Count);
        }

        private static string GetFramePageTitle(Frame frame)
        {
            var page = frame.Content as Page;
            Assert.IsNotNull(page);
            var border = page.Content as Border;
            Assert.IsNotNull(border);
            var textBlock = border.Child as TextBlock;
            Assert.IsNotNull(textBlock);
            return textBlock.Text;
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

        private static void AssertPipsPagerComboBox(ComboBox comboBox, string header, params string[] expectedItems)
        {
            Assert.AreEqual(header, ModernWpf.Controls.Primitives.ControlHelper.GetHeader(comboBox));
            Assert.AreEqual(220.0, comboBox.Width);
            Assert.AreEqual(new Thickness(0, 0, 0, 12), comboBox.Margin);
            Assert.AreEqual(expectedItems[0], comboBox.SelectedItem);
            Assert.AreEqual(expectedItems.Length, comboBox.Items.Count);
            for (var i = 0; i < expectedItems.Length; i++)
            {
                Assert.AreEqual(expectedItems[i], comboBox.Items[i]);
            }
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
            Assert.AreEqual(expectedNames.Length, expectedCount);
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
    }
}
