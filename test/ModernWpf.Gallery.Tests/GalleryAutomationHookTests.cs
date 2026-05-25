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
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.ViewModels;
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
            yield return new object[] { "AnnotatedScrollBar", "GallerySample_AnnotatedScrollBar_Root", "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar" };
            yield return new object[] { "PullToRefresh", "GallerySample_PullToRefresh_Root", "GallerySample_PullToRefresh_RefreshContainer" };
            yield return new object[] { "SplitView", "GallerySample_SplitView_Root", "GallerySample_SplitView_SplitView" };
            yield return new object[] { "PersonPicture", "GallerySample_PersonPicture_Root", "GallerySample_PersonPicture_PersonPicture" };
            yield return new object[] { "ParallaxView", "GallerySample_ParallaxView_Root", "GallerySample_ParallaxView_ParallaxView" };
            yield return new object[] { "IconElement", "GallerySample_IconElement_Root", "GallerySample_IconElement_SlicesIcon" };
            yield return new object[] { "ThemeShadow", "GallerySample_ThemeShadow_Root", "GallerySample_ThemeShadow_ShadowRect" };
            yield return new object[] { "TitleBar", "GallerySample_TitleBar_Root", "GallerySample_TitleBar_TitleBarControl" };
            yield return new object[] { "GridView", "GallerySample_GridView_Root", "GallerySample_GridView_BasicGridView" };
            yield return new object[] { "ItemsRepeater", "GallerySample_ItemsRepeater_Root", "GallerySample_ItemsRepeater_ItemsRepeater" };
            yield return new object[] { "BreadcrumbBar", "GallerySample_BreadcrumbBar_Root", "GallerySample_BreadcrumbBar_BreadcrumbBar" };
            yield return new object[] { "Pivot", "GallerySample_Pivot_Root", "GallerySample_Pivot_Pivot" };
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
            yield return new object[] { "SwipeControl", "GallerySample_SwipeControl_Root", "GallerySample_SwipeControl_SwipeControl" };
            yield return new object[] { "AppBarButton", "GallerySample_AppBarButton_Root", "GallerySample_AppBarButton_AppBarButton" };
            yield return new object[] { "AppBarSeparator", "GallerySample_AppBarSeparator_Root", "GallerySample_AppBarSeparator_CommandBar" };
            yield return new object[] { "AppBarToggleButton", "GallerySample_AppBarToggleButton_Root", "GallerySample_AppBarToggleButton_AppBarToggleButton" };
            yield return new object[] { "CommandBar", "GallerySample_CommandBar_Root", "GallerySample_CommandBar_CommandBar" };
            yield return new object[] { "CommandBarFlyout", "GallerySample_CommandBarFlyout_Root", "GallerySample_CommandBarFlyout_ShowButton" };
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

                    var scrollViewer = (ScrollViewer)FindByAutomationId(page, "GallerySample_AnnotatedScrollBar_ScrollView");
                    var annotatedScrollBar = (Mux.AnnotatedScrollBar)FindByAutomationId(page, "GallerySample_AnnotatedScrollBar_AnnotatedScrollBar");
                    var itemsRepeater = FindNamedDescendant<WrapPanel>(page, "itemsRepeater");
                    var heightSlider = FindNamedDescendant<Slider>(page, "AnnotatedScrollBarMaxHeightSlider");

                    Assert.IsNotNull(scrollViewer);
                    Assert.IsNotNull(annotatedScrollBar);
                    Assert.IsNotNull(itemsRepeater);
                    Assert.IsNotNull(heightSlider);

                    Assert.AreEqual("scrollView", scrollViewer.Name);
                    Assert.AreEqual(800.0, scrollViewer.MaxWidth);
                    Assert.AreEqual(500.0, scrollViewer.MaxHeight);
                    Assert.AreEqual(Brushes.LightGray, scrollViewer.Background);
                    Assert.AreEqual(ScrollBarVisibility.Hidden, scrollViewer.VerticalScrollBarVisibility);
                    Assert.AreEqual(ScrollBarVisibility.Disabled, scrollViewer.HorizontalScrollBarVisibility);
                    Assert.AreSame(itemsRepeater, scrollViewer.Content);

                    Assert.AreEqual("annotatedScrollBar", annotatedScrollBar.Name);
                    Assert.AreEqual(500.0, annotatedScrollBar.MaxHeight);
                    Assert.AreEqual(new Thickness(4, 0, 48, 0), annotatedScrollBar.Margin);
                    Assert.AreEqual(HorizontalAlignment.Right, annotatedScrollBar.HorizontalAlignment);
                    Assert.IsTrue(annotatedScrollBar.ScrollController.CanScroll);

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
                    Assert.IsTrue(renderedLabelTexts.Count >= 3, string.Join("; ", labelDiagnostics));

                    var thumb = FindNamedDescendant<Border>(annotatedScrollBar, "PART_VerticalThumb");
                    Assert.IsNotNull(thumb);
                    Assert.IsTrue(thumb.ActualWidth > 0);
                    Assert.IsTrue(thumb.ActualHeight > 0);

                    Assert.AreEqual("AnnotatedScrollBar maximum height:", ModernWpf.Controls.Primitives.ControlHelper.GetHeader(heightSlider));
                    Assert.AreEqual(100.0, heightSlider.Minimum);
                    Assert.AreEqual(500.0, heightSlider.Maximum);
                    Assert.AreEqual(500.0, heightSlider.Value);

                    heightSlider.Value = 250;
                    WpfTestHost.DoEvents();
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
        public void ParallaxViewSampleMatchesWinUIGalleryExamples()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ParallaxView"));
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
                    Assert.AreEqual("Parallax on a ListView", page.Examples[0].HeaderText);
                    Assert.AreEqual("Parallax with a ScrollView", page.Examples[1].HeaderText);
                    Assert.IsFalse(page.HasAdditionalSampleSnippets);
                    StringAssert.Contains(page.Examples[0].XamlCode, "Source=\"{Binding ElementName=listView}\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "VerticalShift=\"500\"");
                    StringAssert.Contains(page.Examples[0].XamlCode, "AutomationProperties.Name=\"all samples\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<ScrollView x:Name=\"scrollView\" Width=\"150\"");
                    StringAssert.Contains(page.Examples[1].XamlCode, "<Rectangle Fill=\"AliceBlue\" Height=\"150\"/>");
                    Assert.IsNull(page.Examples[0].CSharpCode);
                    Assert.IsNull(page.Examples[1].CSharpCode);

                    var sampleRoot = FindByAutomationId(page, "GallerySample_ParallaxView_Root") as UIElement;
                    Assert.IsNotNull(sampleRoot);
                    var sampleRootPeer = UIElementAutomationPeer.CreatePeerForElement(sampleRoot);
                    Assert.IsNotNull(sampleRootPeer);
                    Assert.IsTrue(sampleRootPeer.IsControlElement());
                    Assert.AreEqual(AutomationControlType.Group, sampleRootPeer.GetAutomationControlType());

                    var parallaxView = (Mux.ParallaxView)FindByAutomationId(page, "GallerySample_ParallaxView_ParallaxView");
                    var listView = FindNamedDescendant<ListView>(page, "listView");
                    Assert.IsNotNull(parallaxView);
                    Assert.AreEqual("parallaxView", parallaxView.Name);
                    Assert.AreEqual(500.0, parallaxView.VerticalShift);
                    Assert.AreSame(listView, parallaxView.Source);
                    Assert.IsInstanceOfType(parallaxView.Child, typeof(Image));
                    StringAssert.Contains(((BitmapImage)((Image)parallaxView.Child).Source).UriSource.ToString(), "cliff.jpg");

                    Assert.IsNotNull(listView);
                    Assert.AreEqual("all samples", AutomationProperties.GetName(listView));
                    Assert.AreEqual(475.0, listView.Height);
                    Assert.AreEqual(new Thickness(0, 76, 0, 0), listView.Margin);
                    Assert.AreEqual(Colors.Transparent, ((SolidColorBrush)listView.Background).Color);
                    var overlay = FindNamedDescendant<Border>(page, "ParallaxOverlay");
                    Assert.IsNotNull(overlay);
                    Assert.AreEqual(Color.FromArgb(0x80, 0x00, 0x00, 0x00), ((SolidColorBrush)overlay.Background).Color);
                    var itemTitles = listView.Items.Cast<string>().ToArray();
                    Assert.IsTrue(itemTitles.Length > 60);
                    CollectionAssert.AreEqual(itemTitles.OrderBy(title => title).ToArray(), itemTitles);
                    CollectionAssert.Contains(itemTitles, "NavigationView");
                    CollectionAssert.Contains(itemTitles, "ParallaxView");

                    var headerTexts = FindDescendants<TextBlock>(page).Select(textBlock => textBlock.Text).ToArray();
                    CollectionAssert.Contains(headerTexts, "Scroll the list to see parallaxing of image");
                    CollectionAssert.Contains(headerTexts, "Scroll the rectangles to see parallaxing of image");

                    var scrollView = FindNamedDescendant<ScrollViewer>(page, "scrollView");
                    Assert.IsNotNull(scrollView);
                    Assert.AreEqual(150.0, scrollView.Width);
                    Assert.AreEqual(551.0, scrollView.Height);
                    Assert.AreEqual(HorizontalAlignment.Left, scrollView.HorizontalAlignment);
                    Assert.AreEqual(ScrollBarVisibility.Disabled, scrollView.HorizontalScrollBarVisibility);
                    Assert.AreEqual(ScrollBarVisibility.Auto, scrollView.VerticalScrollBarVisibility);
                    var secondParallaxView = FindDescendants<Mux.ParallaxView>(page).Single(view => ReferenceEquals(view.Source, scrollView));
                    Assert.AreEqual(500.0, secondParallaxView.VerticalShift);
                    Assert.IsInstanceOfType(secondParallaxView.Child, typeof(Image));
                    StringAssert.Contains(((BitmapImage)((Image)secondParallaxView.Child).Source).UriSource.ToString(), "cliff.jpg");

                    var rectangles = ((StackPanel)scrollView.Content).Children.OfType<WpfShapes.Rectangle>().ToArray();
                    Assert.AreEqual(19, rectangles.Length);
                    Assert.AreEqual(150.0, rectangles[0].Height);
                    Assert.AreEqual(Colors.AliceBlue, ((SolidColorBrush)rectangles[0].Fill).Color);
                    Assert.AreEqual(Colors.Cyan, ((SolidColorBrush)rectangles[18].Fill).Color);
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
                    Assert.AreEqual("AutoSuggestBox", listView.Items[0]);

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
                        "Use the TitleBar control and ModernWpf title bar attached properties for WPF title bar customization.",
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
        public void ShellNavigationRootWritesRenderedVisualArtifacts()
        {
            WpfTestHost.Run(() =>
            {
                var artifactDirectory = Path.Combine(Path.GetTempPath(), "ModernWpfGalleryTests", Path.GetRandomFileName());
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--visual-artifact-dir", artifactDirectory }));

                var page = new NavigationRootPage();
                page.DataContext = new { ViewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings) };
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
                    var contentRootArtifact = Path.Combine(artifactDirectory, "ContentRootGrid.png");

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
