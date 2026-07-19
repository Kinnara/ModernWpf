using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class StatusInfoSampleFactory
    {
        private const string InfoBarLongMessage = "A long essential app message for your users to be informed of, acknowledge, or take action on. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin dapibus dolor vitae justo rutrum, ut lobortis nibh mattis. Aenean id elit commodo, semper felis nec.";

        private const string InfoBarExample1Xaml =
@"<InfoBar
    IsOpen=""True""
    Severity=""Informational""
    Title=""Title""
    Message=""Essential app message for your users to be informed of, acknowledge, or take action on."" />";

        private const string InfoBarExample2Xaml =
@"<InfoBar
    IsOpen=""True""
    Title=""Title""
    Message=""A long essential app message..."">
</InfoBar>";

        private const string InfoBarExample3Xaml =
@"<InfoBar
    IsOpen=""True""
    IsIconVisible=""True""
    IsClosable=""True""
    Title=""Title""
    Message=""Essential app message for your users to be informed of, acknowledge, or take action on."" />";

        private const string InfoBadgeNavigationViewXaml =
@"<NavigationViewItem x:Name=""InboxPage"" Content=""Inbox"" Icon=""Mail"" AutomationProperties.Name=""Inbox, 5 notifications"">
    <NavigationViewItem.InfoBadge>
        <InfoBadge x:Name=""infoBadge1"" Value=""5"" Opacity=""{x:Bind InfoBadgeOpacity, Mode=OneWay}""/>
    </NavigationViewItem.InfoBadge>
</NavigationViewItem>";

        private const string InfoBadgeStylesXaml =
@"<StackPanel Orientation=""Horizontal"" Spacing=""20"" HorizontalAlignment=""Center"">
    <InfoBadge x:Name=""infoBadge2"" Style=""{StaticResource $(Style)IconInfoBadgeStyle}"" HorizontalAlignment=""Right""/>
    <InfoBadge x:Name=""infoBadge3"" Style=""{StaticResource $(Style)ValueInfoBadgeStyle}"" HorizontalAlignment=""Right"" Value=""10"" />
    <InfoBadge x:Name=""infoBadge4"" Style=""{StaticResource $(Style)DotInfoBadgeStyle}"" VerticalAlignment=""Center""/>
</StackPanel>";

        private const string InfoBadgeInsideControlXaml =
@"<Button Padding=""0"" Width=""200"" Height=""60"" ToolTipService.ToolTip=""Refresh required""
        HorizontalAlignment=""Center"" HorizontalContentAlignment=""Stretch"" VerticalContentAlignment=""Stretch"">
    <Grid HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch"" Width=""Auto"" Height=""Auto"">
        <SymbolIcon Symbol=""Sync"" HorizontalAlignment=""Center""/>
        <InfoBadge Background=""#C42B1C"" HorizontalAlignment=""Right"" VerticalAlignment=""Top"">
            <InfoBadge.IconSource>
                <FontIconSource FontFamily=""{StaticResource SymbolThemeFontFamily}"" Glyph=""&#xF13C;"" />
            </InfoBadge.IconSource>
        </InfoBadge>
    </Grid>
</Button>";

        private const string InfoBadgeDynamicValueXaml =
@"<InfoBadge Value=""{Binding ElementName=ValueNumberBox, Path=Value, Mode=TwoWay}"" />
<NumberBox x:Name=""ValueNumberBox"" Header=""InfoBadge Value"" Value=""1"" Minimum=""-1""
    SpinButtonPlacementMode=""Inline"" ValueChanged=""ValueNumberBox_ValueChanged"" />";

        private const string InfoBadgeDynamicValueCSharp =
@"private void ValueNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
{
    if((int)args.NewValue >= -1)
    {
        DynamicInfoBadge.Value = (int)args.NewValue;
    }
}";

        private const string ProgressRingIndeterminateXaml =
@"<ProgressRing IsActive=""$(IsActive)"" $(Background)/>";

        private const string ProgressRingDeterminateXaml =
@"<ProgressRing Width=""60"" Height=""60"" Value=""$(DeterminateProgressValue)""
              IsIndeterminate=""False""
              $(Background)/>";

        private const string ProgressBarIndeterminateXaml =
@"<ProgressBar Width=""130"" IsIndeterminate=""True"" ShowPaused=""$(ShowPaused)"" ShowError=""$(ShowError)"" />";

        private const string ProgressBarDeterminateXaml =
@"<ProgressBar Width=""130"" Value=""$(DeterminateProgressValue)"" />";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "InfoBadge":
                    return CreateInfoBadgeExamples();
                case "InfoBar":
                    return CreateInfoBarExamples();
                case "ProgressRing":
                    return CreateProgressRingExamples();
                case "WinUIProgressBar":
                    return CreateProgressBarExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "InfoBadge":
                    return CreateInfoBadgeSample();
                case "InfoBar":
                    return CreateInfoBarSample();
                case "ProgressRing":
                    return CreateProgressRingSample();
                case "WinUIProgressBar":
                    return CreateProgressBarSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateInfoBadgeSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("InfoBadge"));
            panel.Children.Add(CreateNavigationViewInfoBadgeExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateInfoBadgeExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "InfoBadge embedded in NavigationView ",
                    CreateNavigationViewInfoBadgeExampleContent(assignRootAutomationId: true),
                    InfoBadgeNavigationViewXaml,
                    null),
                new GalleryExample(
                    "Different InfoBadge Styles",
                    CreateInfoBadgeStylesExampleContent(),
                    InfoBadgeStylesXaml,
                    null),
                new GalleryExample(
                    "Placing an InfoBadge Inside Another Control",
                    CreateInfoBadgeInsideControlExampleContent(),
                    InfoBadgeInsideControlXaml,
                    null),
                new GalleryExample(
                    "InfoBadge with Dynamic Value",
                    CreateDynamicInfoBadgeExampleContent(),
                    InfoBadgeDynamicValueXaml,
                    InfoBadgeDynamicValueCSharp)
            };
        }

        private static GallerySamplePanel CreateNavigationViewInfoBadgeExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("InfoBadge"));
            }

            var navigationView = new Mux.NavigationView
            {
                Name = "nvSample1",
                Width = 560,
                Height = 300,
                PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Left,
                IsPaneOpen = true,
                Content = new Frame { Name = "contentFrame" }
            };
            GalleryAutomation.WithAutomationId(navigationView, GalleryAutomation.SampleElementId("InfoBadge", "NavigationView"));
            var infoBadge = new Mux.InfoBadge
            {
                Name = "infoBadge1",
                Opacity = 1,
                Value = 5
            };
            GalleryAutomation.WithAutomationId(infoBadge, GalleryAutomation.SampleElementId("InfoBadge", "InfoBadge"));

            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Home",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Home)
            });
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Account",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Contact)
            });
            var inboxItem = new Mux.NavigationViewItem
            {
                Name = "InboxPage",
                Content = "Inbox",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Mail),
                InfoBadge = infoBadge
            };
            AutomationProperties.SetName(inboxItem, "Inbox, 5 notifications");
            navigationView.MenuItems.Add(inboxItem);

            var toggle = new Mux.ToggleSwitch
            {
                Name = "ToggleInfoBadgeOpacity",
                Header = "InfoBadge Opacity",
                IsOn = true
            };
            toggle.Toggled += delegate
            {
                infoBadge.Opacity = toggle.IsOn ? 1.0 : 0.0;
            };

            var displayMode = new ComboBox
            {
                Name = "NavigationViewDisplayMode"
            };
            displayMode.Items.Add("LeftExpanded");
            displayMode.Items.Add("LeftCompact");
            displayMode.Items.Add("Top");
            displayMode.SelectionChanged += delegate
            {
                switch (displayMode.SelectedItem as string)
                {
                    case "LeftCompact":
                        navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.LeftCompact;
                        navigationView.IsPaneOpen = false;
                        break;
                    case "Top":
                        navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Top;
                        navigationView.IsPaneOpen = true;
                        break;
                    default:
                        navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Left;
                        navigationView.IsPaneOpen = true;
                        break;
                }
            };
            displayMode.SelectedItem = "LeftExpanded";

            root.Children.Add(CreateInfoBadgeExampleLayout(
                navigationView,
                CreateInfoBadgeOptionsPanel(toggle, CreateOptionBlock("Display Mode", displayMode))));
            return root;
        }

        private static GallerySamplePanel CreateInfoBadgeStylesExampleContent()
        {
            var root = new GallerySamplePanel();
            var infoBadge2 = new Mux.InfoBadge
            {
                Name = "infoBadge2",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var infoBadge3 = new Mux.InfoBadge
            {
                Name = "infoBadge3",
                HorizontalAlignment = HorizontalAlignment.Right,
                Value = 10
            };
            var infoBadge4 = new Mux.InfoBadge
            {
                Name = "infoBadge4",
                VerticalAlignment = VerticalAlignment.Center
            };
            SetInfoBadgeStyles("Attention", infoBadge2, infoBadge3, infoBadge4);

            var badges = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Horizontal
            };
            badges.Children.Add(infoBadge2);
            badges.Children.Add(infoBadge3);
            badges.Children.Add(infoBadge4);
            infoBadge2.Margin = new Thickness(0, 0, 20, 0);
            infoBadge3.Margin = new Thickness(0, 0, 20, 0);

            var styleCombo = new ComboBox
            {
                Name = "InfoBadgeStyleComboBox"
            };
            styleCombo.Items.Add("Attention");
            styleCombo.Items.Add("Informational");
            styleCombo.Items.Add("Success");
            styleCombo.Items.Add("Critical");
            styleCombo.SelectionChanged += delegate
            {
                SetInfoBadgeStyles(styleCombo.SelectedItem as string, infoBadge2, infoBadge3, infoBadge4);
            };
            styleCombo.SelectedItem = "Attention";

            root.Children.Add(CreateInfoBadgeExampleLayout(
                badges,
                CreateInfoBadgeOptionsPanel(CreateOptionBlock("Styles", styleCombo))));
            return root;
        }

        private static GallerySamplePanel CreateInfoBadgeInsideControlExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "Example3Button",
                Width = 200,
                Height = 60,
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                ToolTip = "Refresh required"
            };
            AutomationProperties.SetName(button, "Example3Button");

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            grid.Children.Add(new Mux.SymbolIcon(Mux.Symbol.Sync)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            grid.Children.Add(new Mux.InfoBadge
            {
                Name = "Example3InfoBadge",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = CreateBrush("#C42B1C"),
                IconSource = new Mux.FontIconSource { Glyph = "\uF13C" }
            });
            button.Content = grid;
            root.Children.Add(button);
            return root;
        }

        private static GallerySamplePanel CreateDynamicInfoBadgeExampleContent()
        {
            var root = new GallerySamplePanel();
            var dynamicInfoBadge = new Mux.InfoBadge
            {
                Name = "DynamicInfoBadge",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var valueNumberBox = new Mux.NumberBox
            {
                Name = "ValueNumberBox",
                Header = "InfoBadge Value",
                Minimum = -1,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline,
                Value = 1
            };
            valueNumberBox.ValueChanged += delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
            {
                if ((int)args.NewValue >= -1)
                {
                    dynamicInfoBadge.Value = (int)args.NewValue;
                }
            };
            dynamicInfoBadge.Value = 1;

            root.Children.Add(CreateInfoBadgeExampleLayout(
                dynamicInfoBadge,
                CreateInfoBadgeOptionsPanel(valueNumberBox)));
            return root;
        }

        private static Grid CreateInfoBadgeExampleLayout(UIElement sample, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.Children.Add(sample);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreateInfoBadgeOptionsPanel(params UIElement[] children)
        {
            var panel = new StackPanel
            {
                Width = 160
            };
            foreach (var child in children)
            {
                panel.Children.Add(child);
            }

            return panel;
        }

        private static void SetInfoBadgeStyles(
            string style,
            Mux.InfoBadge iconBadge,
            Mux.InfoBadge valueBadge,
            Mux.InfoBadge dotBadge)
        {
            var stylePrefix = string.IsNullOrEmpty(style) ? "Attention" : style;
            ApplyInfoBadgeBackground(iconBadge, stylePrefix);
            ApplyInfoBadgeBackground(valueBadge, stylePrefix);
            ApplyInfoBadgeBackground(dotBadge, stylePrefix);

            iconBadge.Padding = new Thickness(0);
            switch (stylePrefix)
            {
                case "Informational":
                    iconBadge.Padding = new Thickness(0, 4, 0, 2);
                    iconBadge.IconSource = new Mux.FontIconSource { Glyph = "\uF13F" };
                    break;
                case "Success":
                    iconBadge.IconSource = new Mux.SymbolIconSource { Symbol = Mux.Symbol.Accept };
                    break;
                case "Critical":
                    iconBadge.IconSource = new Mux.SymbolIconSource { Symbol = Mux.Symbol.Cancel };
                    break;
                default:
                    iconBadge.Padding = new Thickness(0, 4, 0, 2);
                    iconBadge.IconSource = new Mux.FontIconSource { Glyph = "\uEA38" };
                    break;
            }
        }

        private static void ApplyInfoBadgeBackground(Mux.InfoBadge badge, string stylePrefix)
        {
            switch (stylePrefix)
            {
                case "Informational":
                    badge.SetResourceReference(Control.BackgroundProperty, "SystemFillColorSolidNeutralBrush");
                    break;
                case "Success":
                    badge.SetResourceReference(Control.BackgroundProperty, "SystemFillColorSuccessBrush");
                    break;
                case "Critical":
                    badge.SetResourceReference(Control.BackgroundProperty, "SystemFillColorCriticalBrush");
                    break;
                default:
                    badge.SetResourceReference(Control.BackgroundProperty, "SystemFillColorAttentionBrush");
                    break;
            }
        }

        private static UIElement CreateInfoBarSample()
        {
            var panel = CreateSamplePanel("InfoBar presents inline app status without blocking the current task.");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("InfoBar"));
            panel.Children.Add(CreateSeverityInfoBarExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateInfoBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A closable InfoBar with options to change its Severity.",
                    CreateSeverityInfoBarExampleContent(assignRootAutomationId: true),
                    InfoBarExample1Xaml,
                    null),
                new GalleryExample(
                    "A closable InfoBar with a long or short message and various buttons",
                    CreateMessageInfoBarExampleContent(),
                    InfoBarExample2Xaml,
                    null),
                new GalleryExample(
                    "A closable InfoBar with options to display the close button and icon",
                    CreateIconAndCloseInfoBarExampleContent(),
                    InfoBarExample3Xaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateInfoBarExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("InfoBar"));
            }

            return root;
        }

        private static GallerySamplePanel CreateSeverityInfoBarExampleContent(bool assignRootAutomationId)
        {
            var root = CreateInfoBarExampleRoot(assignRootAutomationId);
            var infoBar = new Mux.InfoBar
            {
                IsOpen = true,
                Severity = Mux.InfoBarSeverity.Informational,
                Title = "Title",
                Message = "Essential app message for your users to be informed of, acknowledge, or take action on.",
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            GalleryAutomation.WithAutomationId(infoBar, GalleryAutomation.SampleElementId("InfoBar", "InfoBar"));

            var isOpen = CreateOptionCheckBox("Is Open", isChecked: true);
            isOpen.Name = "InfoBarIsOpenCheckBox1";
            GalleryAutomation.WithAutomationId(isOpen, GalleryAutomation.SampleElementId("InfoBar", "IsOpenCheckBox"));
            isOpen.Checked += delegate { infoBar.IsOpen = true; };
            isOpen.Unchecked += delegate { infoBar.IsOpen = false; };

            var severity = CreateOptionComboBox("InfoBarSeverityComboBox");
            severity.Items.Add("Informational");
            severity.Items.Add("Success");
            severity.Items.Add("Warning");
            severity.Items.Add("Error");
            severity.SelectionChanged += delegate
            {
                var selectedSeverity = severity.SelectedItem as string;
                switch (selectedSeverity)
                {
                    case "Error":
                        infoBar.Severity = Mux.InfoBarSeverity.Error;
                        break;
                    case "Warning":
                        infoBar.Severity = Mux.InfoBarSeverity.Warning;
                        break;
                    case "Success":
                        infoBar.Severity = Mux.InfoBarSeverity.Success;
                        break;
                    default:
                        infoBar.Severity = Mux.InfoBarSeverity.Informational;
                        break;
                }
            };
            severity.SelectedItem = "Informational";

            var options = CreateOptionsPanel();
            options.Children.Add(isOpen);
            options.Children.Add(CreateOptionBlock("Severity", severity));

            root.Children.Add(CreateInfoBarExampleLayout(infoBar, options));
            return root;
        }

        private static GallerySamplePanel CreateMessageInfoBarExampleContent()
        {
            var root = CreateInfoBarExampleRoot(assignRootAutomationId: false);
            var infoBar = new Mux.InfoBar
            {
                IsOpen = true,
                Title = "Title",
                Message = InfoBarLongMessage,
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            GalleryAutomation.WithAutomationId(infoBar, GalleryAutomation.SampleElementId("InfoBar", "LongMessageInfoBar"));

            var isOpen = CreateOptionCheckBox("Is Open", isChecked: true);
            isOpen.Name = "InfoBarIsOpenCheckBox2";
            isOpen.Checked += delegate { infoBar.IsOpen = true; };
            isOpen.Unchecked += delegate { infoBar.IsOpen = false; };

            var messageLength = CreateOptionComboBox("InfoBarMessageComboBox");
            messageLength.Items.Add(new ComboBoxItem { Content = "Short" });
            messageLength.Items.Add(new ComboBoxItem { Content = "Long" });
            messageLength.SelectionChanged += delegate
            {
                infoBar.Message = messageLength.SelectedIndex == 0
                    ? "A short essential app message."
                    : InfoBarLongMessage;
            };
            messageLength.SelectedIndex = 1;

            var actionButton = CreateOptionComboBox("InfoBarActionButtonComboBox");
            actionButton.Items.Add(new ComboBoxItem { Content = "None" });
            actionButton.Items.Add(new ComboBoxItem { Content = "Button" });
            actionButton.Items.Add(new ComboBoxItem { Content = "Hyperlink" });
            actionButton.SelectionChanged += delegate
            {
                if (actionButton.SelectedIndex == 1)
                {
                    infoBar.ActionButton = new Button { Content = "Action" };
                }
                else if (actionButton.SelectedIndex == 2)
                {
                    infoBar.ActionButton = new Mux.HyperlinkButton
                    {
                        Content = "Informational link",
                        NavigateUri = new Uri("http://www.microsoft.com/")
                    };
                }
                else
                {
                    infoBar.ActionButton = null;
                }
            };
            actionButton.SelectedIndex = 0;

            var options = CreateOptionsPanel();
            options.Children.Add(isOpen);
            options.Children.Add(CreateOptionBlock("Message Length", messageLength));
            options.Children.Add(CreateOptionBlock("Action Button", actionButton));

            root.Children.Add(CreateInfoBarExampleLayout(infoBar, options));
            return root;
        }

        private static GallerySamplePanel CreateIconAndCloseInfoBarExampleContent()
        {
            var root = CreateInfoBarExampleRoot(assignRootAutomationId: false);
            var infoBar = new Mux.InfoBar
            {
                IsOpen = true,
                IsIconVisible = true,
                IsClosable = true,
                Title = "Title",
                Message = "Essential app message for your users to be informed of, acknowledge, or take action on.",
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            GalleryAutomation.WithAutomationId(infoBar, GalleryAutomation.SampleElementId("InfoBar", "IconAndCloseInfoBar"));

            var isOpen = CreateOptionCheckBox("Is Open", isChecked: true);
            isOpen.Name = "InfoBarIsOpenCheckBox3";
            isOpen.Checked += delegate { infoBar.IsOpen = true; };
            isOpen.Unchecked += delegate { infoBar.IsOpen = false; };

            var isIconVisible = CreateOptionCheckBox("Is Icon Visible", isChecked: true);
            isIconVisible.Name = "InfoBarIsIconVisibleCheckBox";
            isIconVisible.Checked += delegate { infoBar.IsIconVisible = true; };
            isIconVisible.Unchecked += delegate { infoBar.IsIconVisible = false; };

            var isClosable = CreateOptionCheckBox("Is Closable", isChecked: true);
            isClosable.Name = "InfoBarIsClosableCheckBox";
            isClosable.Checked += delegate { infoBar.IsClosable = true; };
            isClosable.Unchecked += delegate { infoBar.IsClosable = false; };

            var options = CreateOptionsPanel();
            options.Children.Add(isOpen);
            options.Children.Add(isIconVisible);
            options.Children.Add(isClosable);

            root.Children.Add(CreateInfoBarExampleLayout(infoBar, options));
            return root;
        }

        private static Grid CreateInfoBarExampleLayout(UIElement infoBar, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(560) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            layout.Children.Add(infoBar);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreateOptionsPanel()
        {
            return new StackPanel
            {
                Width = 150,
                VerticalAlignment = VerticalAlignment.Top
            };
        }

        private static CheckBox CreateOptionCheckBox(string content, bool isChecked)
        {
            return new CheckBox
            {
                Content = content,
                IsChecked = isChecked,
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        private static StackPanel CreateOptionBlock(string label, Control control)
        {
            var block = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 8)
            };
            block.Children.Add(new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 0, 4)
            });
            block.Children.Add(control);
            return block;
        }

        private static ComboBox CreateOptionComboBox(string name)
        {
            return new ComboBox
            {
                Name = name,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
        }

        private static UIElement CreateProgressRingSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ProgressRing"));
            panel.Children.Add(CreateIndeterminateProgressRingExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static UIElement CreateProgressBarSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("WinUIProgressBar"));
            panel.Children.Add(CreateIndeterminateProgressBarExampleContent(assignRootAutomationId: false, out _));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateProgressBarExamples()
        {
            var indeterminate = CreateIndeterminateProgressBarExampleContent(assignRootAutomationId: true, out var options);
            return new[]
            {
                new GalleryExample(
                    "An indeterminate progress bar.",
                    indeterminate,
                    ProgressBarIndeterminateXaml,
                    null,
                    options),
                new GalleryExample(
                    "A determinate progress bar.",
                    CreateDeterminateProgressBarExampleContent(),
                    ProgressBarDeterminateXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateIndeterminateProgressBarExampleContent(
            bool assignRootAutomationId,
            out Mux.RadioButtons options)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("WinUIProgressBar"));
            }

            var progressBar = new Mux.ProgressBar
            {
                Name = "ProgressBar1",
                Width = 130,
                Margin = new Thickness(10, 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                IsIndeterminate = true
            };
            GalleryAutomation.WithAutomationId(progressBar, GalleryAutomation.SampleElementId("WinUIProgressBar", "IndeterminateProgressBar"));

            var radioButtons = new Mux.RadioButtons
            {
                Name = "ProgressStateRadioButtons",
                Header = "Progress state"
            };
            radioButtons.Items.Add(new RadioButton { Name = "RunningRB", Content = "Running", IsChecked = true });
            radioButtons.Items.Add(new RadioButton { Name = "PausedRB", Content = "Paused" });
            radioButtons.Items.Add(new RadioButton { Name = "ErrorRB", Content = "Error" });
            radioButtons.SelectionChanged += delegate
            {
                progressBar.ShowPaused = radioButtons.SelectedIndex == 1;
                progressBar.ShowError = radioButtons.SelectedIndex == 2;
            };
            radioButtons.SelectedIndex = 0;
            options = radioButtons;

            root.Children.Add(progressBar);
            return root;
        }

        private static GallerySamplePanel CreateDeterminateProgressBarExampleContent()
        {
            var root = new GallerySamplePanel();
            var progressBar = new Mux.ProgressBar
            {
                Name = "ProgressBar2",
                Width = 130
            };
            AutomationProperties.SetName(progressBar, "Determinate ProgressBar example");
            GalleryAutomation.WithAutomationId(progressBar, GalleryAutomation.SampleElementId("WinUIProgressBar", "DeterminateProgressBar"));

            var output = new TextBlock
            {
                Name = "Control2Output",
                Width = 60,
                TextAlignment = TextAlignment.Center
            };
            var label = new TextBlock
            {
                Name = "ProgressLabel",
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = "Progress"
            };
            var progressValue = new Mux.NumberBox
            {
                Name = "ProgressValue",
                Minimum = 0,
                Maximum = 100,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline,
                Value = 0
            };
            AutomationProperties.SetName(progressValue, "NumberBox controlling ProgressBar2 value");
            AutomationProperties.SetLabeledBy(progressValue, label);
            progressValue.ValueChanged += delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
            {
                if (!double.IsNaN(sender.Value))
                {
                    progressBar.Value = sender.Value;
                }
                else
                {
                    sender.Value = 0;
                }
            };

            var sample = new StackPanel
            {
                Name = "Control2",
                Orientation = Orientation.Horizontal
            };
            sample.Children.Add(progressBar);
            sample.Children.Add(output);
            sample.Children.Add(label);
            sample.Children.Add(progressValue);
            root.Children.Add(sample);
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateProgressRingExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "An indeterminate progress ring.",
                    CreateIndeterminateProgressRingExampleContent(assignRootAutomationId: true),
                    ProgressRingIndeterminateXaml,
                    null),
                new GalleryExample(
                    "A determinate progress ring.",
                    CreateDeterminateProgressRingExampleContent(),
                    ProgressRingDeterminateXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateIndeterminateProgressRingExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ProgressRing"));
            }

            var progressRing = new Mux.ProgressRing
            {
                Name = "ProgressRing1",
                Width = 60,
                Height = 60,
                Margin = new Thickness(10, 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                IsActive = true
            };
            AutomationProperties.SetName(progressRing, "Progress image");
            GalleryAutomation.WithAutomationId(progressRing, GalleryAutomation.SampleElementId("ProgressRing", "ProgressRing"));

            var progressHost = CreateProgressRingBackgroundHost("ProgressRing1BackgroundHost", progressRing);
            var toggle = new Mux.ToggleSwitch
            {
                Name = "ProgressToggle",
                Header = "Progress Options",
                IsOn = true,
                OffContent = "Do work",
                OnContent = "Working"
            };
            AutomationProperties.SetName(toggle, "Progress Options");
            GalleryAutomation.WithAutomationId(toggle, GalleryAutomation.SampleElementId("ProgressRing", "ProgressToggle"));
            toggle.Toggled += delegate { progressRing.IsActive = toggle.IsOn; };

            var background = CreateBackgroundComboBox("BackgroundComboBox1");
            background.SelectionChanged += delegate
            {
                ApplyProgressRingBackground(progressHost, background.SelectedItem as string);
            };

            root.Children.Add(CreateProgressRingExampleLayout(
                progressHost,
                CreateOptionsPanel(toggle, CreateOptionBlock("Background color", background))));
            return root;
        }

        private static GallerySamplePanel CreateDeterminateProgressRingExampleContent()
        {
            var root = new GallerySamplePanel();
            var progressRing = new Mux.ProgressRing
            {
                Name = "ProgressRing2",
                Width = 60,
                Height = 60,
                Margin = new Thickness(0, 0, 60, 0),
                IsIndeterminate = false
            };
            AutomationProperties.SetName(progressRing, "Progress image");
            GalleryAutomation.WithAutomationId(progressRing, GalleryAutomation.SampleElementId("ProgressRing", "DeterminateProgressRing"));

            var progressValue = new Mux.NumberBox
            {
                Name = "ProgressValue",
                MinWidth = 120,
                VerticalAlignment = VerticalAlignment.Center,
                Header = "Progress",
                Minimum = 0,
                Maximum = 100,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline,
                Value = 0
            };
            AutomationProperties.SetName(progressValue, "Progress amount");
            progressValue.ValueChanged += delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
            {
                if (!double.IsNaN(sender.Value))
                {
                    progressRing.Value = sender.Value;
                }
                else
                {
                    sender.Value = 0;
                    progressRing.Value = 0;
                }
            };

            var sample = new StackPanel
            {
                Name = "Control2",
                Orientation = Orientation.Horizontal
            };
            var progressHost = CreateProgressRingBackgroundHost("ProgressRing2BackgroundHost", progressRing);
            sample.Children.Add(progressHost);
            sample.Children.Add(progressValue);

            var background = CreateBackgroundComboBox("BackgroundComboBox2");
            background.SelectionChanged += delegate
            {
                ApplyProgressRingBackground(progressHost, background.SelectedItem as string);
            };

            root.Children.Add(CreateProgressRingExampleLayout(
                sample,
                CreateOptionsPanel(CreateOptionBlock("Background color", background))));
            return root;
        }

        private static Border CreateProgressRingBackgroundHost(string name, UIElement child)
        {
            return new Border
            {
                Name = name,
                Background = Brushes.Transparent,
                Child = child
            };
        }

        private static Grid CreateProgressRingExampleLayout(UIElement sample, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.Children.Add(sample);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreateOptionsPanel(params UIElement[] children)
        {
            var panel = new StackPanel();
            foreach (var child in children)
            {
                panel.Children.Add(child);
            }

            return panel;
        }

        private static ComboBox CreateBackgroundComboBox(string name)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                Width = 200
            };
            comboBox.Items.Add("Transparent");
            comboBox.Items.Add("LightGray");
            return comboBox;
        }

        private static void ApplyProgressRingBackground(Border host, string colorName)
        {
            switch (colorName)
            {
                case "Transparent":
                    host.Background = Brushes.Transparent;
                    break;
                case "LightGray":
                    host.Background = Brushes.LightGray;
                    break;
            }
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}
