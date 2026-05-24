using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Testing;
using Mux = ModernWpf.Controls;
using TeachingTipControl = ModernWpf.Controls.TeachingTip;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryAutomationHookTests
    {
        public static IEnumerable<object[]> CuratedSampleAutomationIds()
        {
            yield return new object[] { "TeachingTip", "GallerySample_TeachingTip_Root", "GallerySample_TeachingTip_ShowButton" };
            yield return new object[] { "InfoBar", "GallerySample_InfoBar_Root", "GallerySample_InfoBar_InfoBar" };
            yield return new object[] { "NavigationView", "GallerySample_NavigationView_Root", "GallerySample_NavigationView_NavigationView" };
            yield return new object[] { "ContentDialog", "GallerySample_ContentDialog_Root", "GallerySample_ContentDialog_ShowButton" };
            yield return new object[] { "DropDownButton", "GallerySample_DropDownButton_Root", "GallerySample_DropDownButton_DropDownButton" };
            yield return new object[] { "MenuBar", "GallerySample_MenuBar_Root", "GallerySample_MenuBar_MenuBar" };
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
        public void NavigationViewSampleMatchesWinUIGalleryFirstExampleShape()
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

                    var navigationView = (ModernWpf.Controls.NavigationView)FindByAutomationId(page, "GallerySample_NavigationView_NavigationView");
                    Assert.IsNotNull(navigationView);
                    Assert.AreEqual(745.0, navigationView.Width);
                    Assert.AreEqual(460.0, navigationView.Height);
                    Assert.AreEqual(HorizontalAlignment.Left, navigationView.HorizontalAlignment);
                    Assert.AreEqual("Sample Page 1", navigationView.Header);
                    Assert.AreEqual(ModernWpf.Controls.NavigationViewBackButtonVisible.Auto, navigationView.IsBackButtonVisible);
                    Assert.IsFalse(navigationView.IsTitleBarAutoPaddingEnabled);
                    Assert.AreEqual(0.0, navigationView.TemplateSettings.TopPadding);
                    Assert.AreEqual(ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto, navigationView.PaneDisplayMode);
                    Assert.AreEqual(4, navigationView.MenuItems.Count);
                    Assert.AreEqual(ScrollBarVisibility.Hidden, ((ScrollViewer)navigationView.Content).VerticalScrollBarVisibility);

                    var firstItem = (ModernWpf.Controls.NavigationViewItem)navigationView.MenuItems[0];
                    Assert.AreEqual("Menu Item1", firstItem.Content);
                    Assert.AreEqual("SamplePage1", firstItem.Tag);
                    Assert.AreEqual(ModernWpf.Controls.Symbol.Play, ((ModernWpf.Controls.SymbolIcon)firstItem.Icon).Symbol);
                    Assert.AreSame(firstItem, navigationView.SelectedItem);
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
    }
}
