using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using ModernContentControlEx = ModernWpf.Controls.ContentControlEx;
using ModernGridEx = ModernWpf.Controls.GridEx;
using ModernStackPanelEx = ModernWpf.Controls.StackPanelEx;

namespace ModernWpf.WinUI.Tests.LayoutCompatibility;

[TestClass]
public class LayoutCompatibilityApiTests
{
    [TestMethod]
    public void BorderExAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) },
                ChildTransitions = new ModernWpf.Media.Animation.TransitionCollection()
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, border.BackgroundSizing);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), border.BackgroundTransition.Duration);
            Assert.IsNotNull(border.ChildTransitions);
        });
    }

    [TestMethod]
    public void CoreTextInputDescriptionPresentersUseWinUIPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var controls = new Control[]
            {
                new TextBox(),
                new PasswordBox(),
                new RichTextBox(),
                new DatePicker()
            };

            foreach (var control in controls)
            {
                ControlHelper.SetDescription(control, control.GetType().Name + " description");
            }

            using var host = new TestWindowHost(new StackPanel { Children = { controls[0], controls[1], controls[2], controls[3] } });
            host.UpdateLayout();

            foreach (var control in controls)
            {
                var descriptionPresenter = FindTemplateChild<ContentPresenterEx>(control, "DescriptionPresenter");
                Assert.AreEqual(ControlHelper.GetDescription(control), descriptionPresenter.Content);
                Assert.AreEqual(Visibility.Visible, descriptionPresenter.Visibility);
                Assert.AreSame(
                    descriptionPresenter.TryFindResource("SystemControlDescriptionTextForegroundBrush"),
                    descriptionPresenter.Foreground);
            }
        });
    }

    [TestMethod]
    public void CoreItemTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var listBoxItem = new ListBoxItem
            {
                Content = "ListBox content",
                IsEnabled = false
            };
            var listViewItem = new System.Windows.Controls.ListViewItem
            {
                Content = "ListView content",
                IsSelected = true
            };
            var header = new GridViewColumnHeader
            {
                Content = "Header content"
            };

            using var host = new TestWindowHost(new StackPanel { Children = { listBoxItem, listViewItem, header } });
            host.UpdateLayout();

            var listBoxPresenter = FindTemplateChild<ContentPresenterEx>(listBoxItem, "ContentPresenter");
            Assert.AreEqual(listBoxItem.Content, listBoxPresenter.Content);
            Assert.AreSame(listBoxPresenter.TryFindResource("ListBoxItemForegroundDisabled"), listBoxPresenter.Foreground);

            var listViewPresenter = FindTemplateChild<ContentPresenterEx>(listViewItem, "ContentPresenter");
            Assert.AreEqual(listViewItem.Content, listViewPresenter.Content);
            Assert.AreSame(listViewPresenter.TryFindResource("ListViewItemForegroundSelected"), listViewPresenter.Foreground);

            var headerPresenter = FindVisualChild<ContentPresenterEx>(header)
                ?? throw new AssertFailedException("Expected GridViewColumnHeader template to use ContentPresenterEx.");
            Assert.AreEqual(header.Content, headerPresenter.Content);
        });
    }

    [TestMethod]
    public void CoreMenuItemTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var topLevelItem = CreateMenuItemWithTemplate("TopLevelItemTemplateKey", "File", null, isEnabled: true);
            var topLevelHeader = CreateMenuItemWithTemplate("TopLevelHeaderTemplateKey", "Edit", null, isEnabled: true);
            var submenuItem = CreateMenuItemWithTemplate("SubmenuItemTemplateKey", "Open", new TextBlock { Text = "Icon" }, isEnabled: false);
            var submenuHeader = CreateMenuItemWithTemplate("SubmenuHeaderTemplateKey", "More", new TextBlock { Text = "Icon" }, isEnabled: false);

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { topLevelItem, topLevelHeader, submenuItem, submenuHeader }
            });
            host.UpdateLayout();

            Assert.AreEqual(topLevelItem.Header, FindVisualChild<ContentPresenterEx>(topLevelItem)?.Content);
            Assert.AreEqual(topLevelHeader.Header, FindVisualChild<ContentPresenterEx>(topLevelHeader)?.Content);

            AssertMenuTemplatePresenterSlot(
                submenuItem,
                expectedForegroundResource: "MenuFlyoutItemForegroundDisabled");
            AssertMenuTemplatePresenterSlot(
                submenuHeader,
                expectedForegroundResource: "MenuFlyoutSubItemForegroundDisabled");
        });
    }

    [TestMethod]
    public void CoreTabControlTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var tabItem = new TabItem
            {
                Header = "Tab Header",
                Content = "Tab Content"
            };
            var tabControl = new TabControl
            {
                Width = 320,
                Height = 160
            };
            TabControlHelper.SetTabStripHeader(tabControl, "Strip Header");
            TabControlHelper.SetTabStripFooter(tabControl, "Strip Footer");
            tabControl.Items.Add(tabItem);

            using var host = new TestWindowHost(tabControl, width: 380, height: 220);
            host.UpdateLayout();

            var itemPresenter = FindTemplateChild<ContentPresenterEx>(tabItem, "ContentPresenter");
            Assert.AreEqual(tabItem.Header, itemPresenter.Content);
            Assert.AreSame(itemPresenter.TryFindResource("TabViewItemHeaderForegroundSelected"), itemPresenter.Foreground);

            var headerPresenter = FindTemplateChild<ContentPresenterEx>(tabControl, "HeaderContentPresenter");
            Assert.AreEqual(TabControlHelper.GetTabStripHeader(tabControl), headerPresenter.Content);

            var footerPresenter = FindTemplateChild<ContentPresenterEx>(tabControl, "FooterContentPresenter");
            Assert.AreEqual(TabControlHelper.GetTabStripFooter(tabControl), footerPresenter.Content);

            var selectedContentHost = FindTemplateChild<ContentPresenterEx>(tabControl, "PART_SelectedContentHost");
            Assert.AreEqual(tabItem.Content, selectedContentHost.Content);
        });
    }

    [TestMethod]
    public void CorePivotTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var pivotItem = new TabItem
            {
                Header = "Pivot Header",
                Content = "Pivot Content"
            };
            var pivot = new TabControl
            {
                Style = FindStyleResource("TabControlPivotStyle"),
                Width = 320,
                Height = 160
            };
            PivotHelper.SetTitle(pivot, "Pivot Title");
            PivotHelper.SetLeftHeader(pivot, "Left Header");
            PivotHelper.SetRightHeader(pivot, "Right Header");
            pivot.Items.Add(pivotItem);

            using var host = new TestWindowHost(pivot, width: 380, height: 220);
            host.UpdateLayout();

            var itemPresenter = FindTemplateChild<ContentPresenterEx>(pivotItem, "ContentPresenter");
            Assert.AreEqual(pivotItem.Header, itemPresenter.Content);
            Assert.AreSame(itemPresenter.TryFindResource("PivotHeaderItemForegroundSelected"), itemPresenter.Foreground);

            var titleControl = FindTemplateChild<ContentControl>(pivot, "TitleContentControl");
            var titlePresenter = FindVisualChild<ContentPresenterEx>(titleControl)
                ?? throw new AssertFailedException("Expected Pivot title template to use ContentPresenterEx.");
            Assert.AreEqual(PivotHelper.GetTitle(pivot), titlePresenter.Content);

            var leftHeader = FindTemplateChild<ContentPresenterEx>(pivot, "LeftHeaderPresenter");
            Assert.AreEqual(PivotHelper.GetLeftHeader(pivot), leftHeader.Content);

            var rightHeader = FindTemplateChild<ContentPresenterEx>(pivot, "RightHeaderPresenter");
            Assert.AreEqual(PivotHelper.GetRightHeader(pivot), rightHeader.Content);

            var selectedContentHost = FindTemplateChild<ContentPresenterEx>(pivot, "PART_SelectedContentHost");
            Assert.AreEqual(pivotItem.Content, selectedContentHost.Content);
        });
    }

    [TestMethod]
    public void CoreResidualTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = new RadioButton
            {
                Content = "Radio content",
                Foreground = Brushes.Red
            };
            var listViewHeaderItem = new ListViewHeaderItem
            {
                Content = "List header",
                Foreground = Brushes.Blue
            };
            var titleBarButton = new TitleBarButton
            {
                Content = "X",
                Foreground = Brushes.Green,
                IsActive = true
            };

            var hostPanel = new StackPanel();
            hostPanel.Children.Add(radioButton);
            hostPanel.Children.Add(listViewHeaderItem);
            hostPanel.Children.Add(titleBarButton);

            using var host = new TestWindowHost(hostPanel, width: 320, height: 180);
            host.UpdateLayout();

            var radioPresenter = FindTemplateChild<ContentPresenterEx>(radioButton, "ContentPresenter");
            Assert.AreEqual(radioButton.Content, radioPresenter.Content);
            Assert.AreSame(radioButton.Foreground, radioPresenter.Foreground);

            var headerPresenter = FindTemplateChild<ContentPresenterEx>(listViewHeaderItem, "ContentPresenter");
            Assert.AreEqual(listViewHeaderItem.Content, headerPresenter.Content);
            Assert.AreSame(listViewHeaderItem.Foreground, headerPresenter.Foreground);

            var titlePresenter = FindTemplateChild<ContentPresenterEx>(titleBarButton, "Content");
            Assert.AreEqual(titleBarButton.Content, titlePresenter.Content);
            Assert.AreSame(titleBarButton.Foreground, titlePresenter.Foreground);
            Assert.AreEqual(titleBarButton.FontSize, titlePresenter.FontSize);
        });
    }

    [TestMethod]
    public void SimpleShellTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var page = new ModernWpf.Controls.Page
            {
                Content = "Page content",
                Foreground = Brushes.Red
            };
            var frame = new ModernWpf.Controls.Frame();
            var groupBox = new GroupBox
            {
                Header = "Group header",
                Content = "Group content",
                Foreground = Brushes.Blue
            };
            var label = new Label
            {
                Content = "Label content",
                Foreground = Brushes.Green
            };
            var statusBarItem = new StatusBarItem
            {
                Content = "Status content",
                Foreground = Brushes.Orange
            };
            var expander = new System.Windows.Controls.Expander
            {
                Header = "Expander header",
                Content = "Expander content",
                Foreground = Brushes.Purple,
                IsExpanded = true
            };

            var hostPanel = new StackPanel();
            hostPanel.Children.Add(frame);
            hostPanel.Children.Add(groupBox);
            hostPanel.Children.Add(label);
            hostPanel.Children.Add(statusBarItem);
            hostPanel.Children.Add(expander);

            using var pageHost = new TestWindowHost(page, width: 240, height: 120);
            using var host = new TestWindowHost(hostPanel, width: 360, height: 320);
            pageHost.UpdateLayout();
            host.UpdateLayout();

            var pagePresenter = FindVisualChild<ContentPresenterEx>(page)
                ?? throw new AssertFailedException("Expected Page template to use ContentPresenterEx.");
            Assert.AreEqual(page.Content, pagePresenter.Content);
            Assert.AreSame(page.Foreground, pagePresenter.Foreground);

            Assert.IsInstanceOfType(FindTemplateChild<ContentPresenterEx>(frame, "FirstContentPresenter"), typeof(ContentPresenterEx));
            Assert.IsInstanceOfType(FindTemplateChild<ContentPresenterEx>(frame, "SecondContentPresenter"), typeof(ContentPresenterEx));

            var groupPresenters = VisualTreeTestHelper.EnumerateDescendants(groupBox)
                .OfType<ContentPresenterEx>()
                .ToArray();
            Assert.IsTrue(groupPresenters.Any(presenter => Equals(groupBox.Header, presenter.Content)));
            Assert.IsTrue(groupPresenters.Any(presenter => Equals(groupBox.Content, presenter.Content)));

            var labelPresenter = FindVisualChild<ContentPresenterEx>(label)
                ?? throw new AssertFailedException("Expected Label template to use ContentPresenterEx.");
            Assert.AreEqual(label.Content, labelPresenter.Content);
            Assert.AreSame(label.Foreground, labelPresenter.Foreground);

            var statusPresenter = FindVisualChild<ContentPresenterEx>(statusBarItem)
                ?? throw new AssertFailedException("Expected StatusBarItem template to use ContentPresenterEx.");
            Assert.AreEqual(statusBarItem.Content, statusPresenter.Content);
            Assert.AreSame(statusBarItem.Foreground, statusPresenter.Foreground);

            var expandSite = FindTemplateChild<ContentPresenterEx>(expander, "ExpandSite");
            Assert.AreEqual(expander.Content, expandSite.Content);
            Assert.AreSame(expander.Foreground, expandSite.Foreground);

            var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            Assert.IsTrue(
                VisualTreeTestHelper.EnumerateDescendants(headerSite)
                    .OfType<ContentPresenterEx>()
                    .Any(presenter => Equals(expander.Header, presenter.Content)),
                "Expected Expander header template to use ContentPresenterEx.");
        });
    }

    [TestMethod]
    public void CalendarNavigationButtonsUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var calendar = new Calendar();
            using var host = new TestWindowHost(calendar, width: 360, height: 320);
            host.UpdateLayout();

            var calendarItem = FindTemplateChild<CalendarItem>(calendar, "PART_CalendarItem");
            AssertCalendarNavigationPresenter(FindTemplateChild<Button>(calendarItem, "PART_HeaderButton"));
            AssertCalendarNavigationPresenter(FindTemplateChild<Button>(calendarItem, "PART_PreviousButton"));
            AssertCalendarNavigationPresenter(FindTemplateChild<Button>(calendarItem, "PART_NextButton"));
        });
    }

    [TestMethod]
    public void DataGridWpfSpecificTemplatesUseModernPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var cell = new DataGridCell
            {
                Style = FindStyleResource("DataGridCellExpanded"),
                Content = "Cell content",
                Foreground = Brushes.Red
            };
            var columnHeader = new DataGridColumnHeader
            {
                Style = FindStyleResource("DefaultDataGridColumnHeaderStyle"),
                Content = "Column header",
                Foreground = Brushes.Blue
            };
            var rowHeader = new DataGridRowHeader
            {
                Style = FindStyleResource("DefaultDataGridRowHeaderStyle"),
                Content = "Row header",
                Foreground = Brushes.Green
            };
            var groupHeader = new ToggleButton
            {
                Style = FindStyleResource("DataGridRowGroupHeaderStyle"),
                Content = "Group header",
                Foreground = Brushes.Purple
            };

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { cell, columnHeader, rowHeader, groupHeader }
            }, width: 360, height: 220);
            host.UpdateLayout();

            AssertDataGridPresenter(cell, cell.Content, cell.Foreground);
            AssertDataGridPresenter(columnHeader, columnHeader.Content, columnHeader.Foreground);
            AssertDataGridPresenter(rowHeader, rowHeader.Content, rowHeader.Foreground);
            AssertDataGridPresenter(groupHeader, groupHeader.Content, groupHeader.Foreground);
        });
    }

    [TestMethod]
    public void BorderExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:BorderEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Button Content="Parsed" />
                </controls:BorderEx>
                """;

            var border = (BorderEx)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(2), border.Padding);
            Assert.AreEqual(new CornerRadius(3), border.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, border.BackgroundSizing);
            Assert.IsInstanceOfType(border.Child, typeof(Button));
        });
    }

    [TestMethod]
    public void BorderExOuterBackgroundSizingPaintsBehindBorder()
    {
        WpfTestHost.Run(() =>
        {
            var inner = RenderBorderEdgePixel(BackgroundSizing.InnerBorderEdge);
            var outer = RenderBorderEdgePixel(BackgroundSizing.OuterBorderEdge);

            Assert.IsTrue(outer.R > inner.R + 40, $"Expected outer edge red channel above inner edge. Inner={inner}, Outer={outer}");
            Assert.IsTrue(outer.A > inner.A + 40, $"Expected outer edge alpha above inner edge. Inner={inner}, Outer={outer}");
        });
    }

    [TestMethod]
    public void BorderExOuterBackgroundSizingInflatesOuterCornerByHalfBorder()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            var roundedCorner = RenderBorderPixel(border, 27, 1, 30, 30);
            var straightEdge = RenderBorderPixel(border, 1, 15, 30, 30);

            Assert.IsTrue(roundedCorner.A < 30, $"Expected inflated WinUI outer corner to clip the pixel. Pixel={roundedCorner}");
            Assert.IsTrue(straightEdge.R > 200 && straightEdge.A > 200, $"Expected outer edge background under the transparent border. Pixel={straightEdge}");
        });
    }

    [TestMethod]
    public void BorderExLayoutClipUsesNonUniformCornerRadius()
    {
        WpfTestHost.Run(() =>
        {
            var border = new TestBorderEx
            {
                Width = 24,
                Height = 24,
                ClipToBounds = true,
                CornerRadius = new CornerRadius(0, 12, 0, 0)
            };
            border.Measure(new Size(24, 24));
            border.Arrange(new Rect(0, 0, 24, 24));
            border.UpdateLayout();

            var clip = border.GetLayoutClipForTest(new Size(24, 24));

            Assert.IsNotNull(clip);
            Assert.IsTrue(clip.FillContains(new Point(1, 1)), "Top-left corner should remain square.");
            Assert.IsFalse(clip.FillContains(new Point(23, 1)), "Top-right corner should be clipped by the non-uniform radius.");
            Assert.IsTrue(clip.FillContains(new Point(12, 12)), "Center should remain inside the clip.");
        });
    }

    [TestMethod]
    public void RoundedLayoutClipPreservesBaseLayoutClip()
    {
        WpfTestHost.Run(() =>
        {
            var baseClip = new RectangleGeometry(new Rect(0, 0, 12, 24));

            var clip = LayoutChromeHelper.CreateRoundedLayoutClip(
                new Size(24, 24),
                new CornerRadius(12),
                baseClip);

            Assert.IsNotNull(clip);
            Assert.IsTrue(clip.FillContains(new Point(6, 12)), "Point inside both clips should remain visible.");
            Assert.IsFalse(clip.FillContains(new Point(18, 12)), "Point outside the base layout clip should be clipped.");
            Assert.IsFalse(clip.FillContains(new Point(1, 1)), "Point outside the rounded corner should be clipped.");
        });
    }

    [TestMethod]
    public void BorderExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new BorderEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void BorderExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChildRenderClip(new BorderEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0),
                Child = CreateRedChildBox()
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExOffsetsContentByChrome()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var presenter = new ContentPresenterEx
            {
                Width = 120,
                Height = 80,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10),
                Content = button
            };

            using var host = new TestWindowHost(presenter, width: 140, height: 100);

            AssertBoundsRelativeTo(button, presenter, new Rect(15, 15, 90, 50));
        });
    }

    [TestMethod]
    public void ContentPresenterExAlignsContentInsideChrome()
    {
        WpfTestHost.Run(() =>
        {
            var button = CreateButton(40, 20);
            var presenter = new ContentPresenterEx
            {
                Width = 140,
                Height = 100,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Content = button
            };

            using var host = new TestWindowHost(presenter, width: 160, height: 120);

            Assert.AreEqual(HorizontalAlignment.Stretch, new ContentPresenterEx().HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, new ContentPresenterEx().VerticalContentAlignment);
            AssertBoundsRelativeTo(button, presenter, new Rect(85, 65, 40, 20));
        });
    }

    [TestMethod]
    public void ContentPresenterExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ContentPresenterEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge"
                    CharacterSpacing="15"
                    HorizontalContentAlignment="Right"
                    IsTextScaleFactorEnabled="False"
                    LineHeight="37"
                    LineStackingStrategy="MaxHeight"
                    MaxLines="2"
                    OpticalMarginAlignment="TrimSideBearings"
                    TextLineBounds="TrimToBaseline"
                    TextWrapping="Wrap"
                    VerticalContentAlignment="Bottom">
                    <Button Content="Parsed" />
                </controls:ContentPresenterEx>
                """;

            var presenter = (ContentPresenterEx)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(2), presenter.Padding);
            Assert.AreEqual(new CornerRadius(3), presenter.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(15, presenter.CharacterSpacing);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(37, presenter.LineHeight);
            Assert.AreEqual(LineStackingStrategy.MaxHeight, presenter.LineStackingStrategy);
            Assert.AreEqual(2, presenter.MaxLines);
            Assert.AreEqual(ModernWpf.OpticalMarginAlignment.TrimSideBearings, presenter.OpticalMarginAlignment);
            Assert.AreEqual(ModernWpf.TextLineBounds.TrimToBaseline, presenter.TextLineBounds);
            Assert.AreEqual(TextWrapping.Wrap, presenter.TextWrapping);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
            Assert.IsInstanceOfType(presenter.Content, typeof(Button));
        });
    }

    [TestMethod]
    public void ContentPresenterExPushesSupportedTextPropertiesToDefaultTextBlock()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new ContentPresenterEx
            {
                Width = 120,
                Height = 80,
                Content = "Hello",
                LineHeight = 37,
                LineStackingStrategy = LineStackingStrategy.MaxHeight,
                MaxLines = 2,
                TextWrapping = TextWrapping.Wrap
            };

            using var host = new TestWindowHost(presenter, width: 140, height: 100);

            var textBlock = FindVisualChild<TextBlock>(presenter)
                ?? throw new AssertFailedException("Expected ContentPresenterEx to generate a default TextBlock.");
            Assert.AreEqual(TextWrapping.Wrap, textBlock.TextWrapping);
            Assert.AreEqual(37, textBlock.LineHeight);
            Assert.AreEqual(LineStackingStrategy.MaxHeight, textBlock.LineStackingStrategy);
            Assert.AreEqual(74, textBlock.MaxHeight);
            Assert.IsTrue(textBlock.ClipToBounds);

            presenter.MaxLines = 0;

            Assert.AreEqual(double.PositiveInfinity, textBlock.MaxHeight);
            Assert.IsFalse(textBlock.ClipToBounds);
        });
    }

    [TestMethod]
    public void ContentPresenterExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChildRenderClip(new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0),
                Content = CreateRedChildBox()
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void ContentControlExUsesWinUIDefaultAlignmentAndTransitions()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var control = new ModernContentControlEx
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                ContentTransitions = transitions
            };

            Assert.AreEqual(HorizontalAlignment.Left, control.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Top, control.VerticalContentAlignment);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, control.BackgroundSizing);
            Assert.AreSame(backgroundTransition, control.BackgroundTransition);
            Assert.AreEqual(0, control.CharacterSpacing);
            Assert.AreSame(transitions, control.ContentTransitions);
            Assert.IsTrue(control.IsTextScaleFactorEnabled);
        });
    }

    [TestMethod]
    public void ContentControlExTemplateForwardsContentTransitionsAndAlignment()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = CreateButton(40, 20);
            var control = new ModernContentControlEx
            {
                Width = 120,
                Height = 80,
                Background = Brushes.Red,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(5),
                CharacterSpacing = 21,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(10),
                Content = button,
                ContentTransitions = transitions,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                IsTextScaleFactorEnabled = false,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(control, width: 140, height: 100);

            var presenter = FindVisualChild<ContentPresenterEx>(control)
                ?? throw new AssertFailedException("Expected ContentControlEx template to use ContentPresenterEx.");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreSame(backgroundTransition, presenter.BackgroundTransition);
            Assert.AreEqual(21, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreSame(button, presenter.Content);
            Assert.AreEqual(new CornerRadius(3), presenter.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
            AssertBoundsRelativeTo(button, control, new Rect(65, 45, 40, 20));
        });
    }

    [TestMethod]
    public void ContentControlExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ContentControlEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    BackgroundSizing="OuterBorderEdge"
                    CharacterSpacing="21"
                    Padding="2"
                    CornerRadius="3"
                    HorizontalContentAlignment="Right"
                    IsTextScaleFactorEnabled="False"
                    RecognizesAccessKey="True"
                    VerticalContentAlignment="Bottom">
                    <controls:ContentControlEx.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ContentControlEx.BackgroundTransition>
                    <controls:ContentControlEx.ContentTransitions>
                        <animation:TransitionCollection />
                    </controls:ContentControlEx.ContentTransitions>
                    <Button Content="Parsed" />
                </controls:ContentControlEx>
                """;

            var control = (ModernContentControlEx)XamlReader.Parse(xaml);

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, control.BackgroundSizing);
            Assert.IsNotNull(control.BackgroundTransition);
            Assert.AreEqual(21, control.CharacterSpacing);
            Assert.AreEqual(new Thickness(2), control.Padding);
            Assert.AreEqual(new CornerRadius(3), control.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Right, control.HorizontalContentAlignment);
            Assert.IsFalse(control.IsTextScaleFactorEnabled);
            Assert.IsTrue(control.RecognizesAccessKey);
            Assert.IsNotNull(control.ContentTransitions);
            Assert.AreEqual(VerticalAlignment.Bottom, control.VerticalContentAlignment);
            Assert.IsInstanceOfType(control.Content, typeof(Button));
        });
    }

    [TestMethod]
    public void ControlHelperAcceptsWinUIControlTemplateSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new Button();

            ControlHelper.SetBackgroundSizing(button, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetCharacterSpacing(button, 18);
            ControlHelper.SetContentTransitions(button, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(button, false);

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, ControlHelper.GetBackgroundSizing(button));
            Assert.AreEqual(18, ControlHelper.GetCharacterSpacing(button));
            Assert.AreSame(transitions, ControlHelper.GetContentTransitions(button));
            Assert.IsFalse(ControlHelper.GetIsTextScaleFactorEnabled(button));
        });
    }

    [TestMethod]
    public void ButtonTemplateForwardsControlHelperLayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new Button
            {
                Width = 100,
                Height = 40,
                Content = "Button"
            };
            ControlHelper.SetBackgroundSizing(button, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetCharacterSpacing(button, 18);
            ControlHelper.SetContentTransitions(button, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(button, false);

            using var host = new TestWindowHost(button, width: 140, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(button)
                ?? throw new AssertFailedException("Expected Button template to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(button));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(18, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNotNull(presenter.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), presenter.BackgroundTransition.Duration);
        });
    }

    [TestMethod]
    public void AccentButtonStyleUsesOuterBackgroundSizing()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                Width = 100,
                Height = 40,
                Content = "Accent",
                Style = (Style)Application.Current.FindResource("AccentButtonStyle")
            };

            using var host = new TestWindowHost(button, width: 140, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(button)
                ?? throw new AssertFailedException("Expected AccentButtonStyle to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(button));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
        });
    }

    [TestMethod]
    public void RepeatButtonTemplateUsesContentPresenterExDirectly()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var repeatButton = new RepeatButton
            {
                Width = 100,
                Height = 40,
                Content = "Repeat"
            };
            ControlHelper.SetBackgroundSizing(repeatButton, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetCharacterSpacing(repeatButton, 19);
            ControlHelper.SetContentTransitions(repeatButton, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(repeatButton, false);

            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(repeatButton)
                ?? throw new AssertFailedException("Expected RepeatButton template to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(repeatButton));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(19, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNotNull(presenter.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), presenter.BackgroundTransition.Duration);
        });
    }

    [TestMethod]
    public void ToggleButtonCheckedStateUsesOuterBackgroundSizing()
    {
        WpfTestHost.Run(() =>
        {
            var toggleButton = new ToggleButton
            {
                Width = 100,
                Height = 40,
                Content = "Toggle",
                IsChecked = true
            };

            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(toggleButton)
                ?? throw new AssertFailedException("Expected ToggleButton template to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(toggleButton));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
        });
    }

    [TestMethod]
    public void HyperlinkButtonTemplateUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var hyperlinkButton = new HyperlinkButton
            {
                Width = 120,
                Height = 40,
                Content = "Link"
            };
            ControlHelper.SetCharacterSpacing(hyperlinkButton, 21);
            ControlHelper.SetContentTransitions(hyperlinkButton, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(hyperlinkButton, false);

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(hyperlinkButton)
                ?? throw new AssertFailedException("Expected HyperlinkButton template to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(hyperlinkButton));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(21, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNotNull(presenter.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), presenter.BackgroundTransition.Duration);
        });
    }

    [TestMethod]
    public void ToolTipTemplateUsesContentPresenterExChromeSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var toolTip = new ToolTip
            {
                Width = 30,
                Height = 30,
                Content = "Tip",
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                Padding = new Thickness(0)
            };
            ControlHelper.SetBackgroundSizing(toolTip, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetCharacterSpacing(toolTip, 16);
            ControlHelper.SetContentTransitions(toolTip, transitions);
            ControlHelper.SetCornerRadius(toolTip, new CornerRadius(0, 4, 0, 0));
            ControlHelper.SetIsTextScaleFactorEnabled(toolTip, false);

            toolTip.ApplyTemplate();
            toolTip.Measure(new Size(30, 30));
            toolTip.Arrange(new Rect(0, 0, 30, 30));
            toolTip.UpdateLayout();

            var presenter = FindVisualChild<ContentPresenterEx>(toolTip)
                ?? throw new AssertFailedException("Expected ToolTip template to use ContentPresenterEx as the chrome presenter.");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(16, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(new CornerRadius(0, 4, 0, 0), presenter.CornerRadius);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(new Thickness(0), presenter.Padding);
            Assert.AreEqual(TextWrapping.Wrap, presenter.TextWrapping);

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void ContentControlExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var control = new ModernContentControlEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            using var host = new TestWindowHost(control, width: 50, height: 50);
            var presenter = FindVisualChild<ContentPresenterEx>(control)
                ?? throw new AssertFailedException("Expected ContentControlEx template to use ContentPresenterEx.");

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void StackPanelExSupportsSpacingAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 200,
                Height = 120,
                Orientation = Orientation.Vertical,
                Spacing = 10,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(height: 20);
            var second = CreateStretchButton(height: 20);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 220, height: 140);

            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 170, 20));
            AssertBoundsRelativeTo(second, panel, new Rect(15, 45, 170, 20));
        });
    }

    [TestMethod]
    public void StackPanelExSupportsNegativeSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 200,
                Height = 80,
                Orientation = Orientation.Vertical,
                Spacing = -10,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(height: 20);
            var second = CreateStretchButton(height: 20);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 220, height: 100);

            Assert.AreEqual(-10, panel.Spacing);
            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 170, 20));
            AssertBoundsRelativeTo(second, panel, new Rect(15, 25, 170, 20));
        });
    }

    [TestMethod]
    public void StackPanelExAcceptsWinUISnapPointSurface()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                AreScrollSnapPointsRegular = true
            };

            Assert.IsTrue(panel.AreScrollSnapPointsRegular);
            Assert.IsTrue(panel.AreVerticalSnapPointsRegular);
            Assert.IsFalse(panel.AreHorizontalSnapPointsRegular);

            panel.Orientation = Orientation.Horizontal;

            Assert.IsTrue(panel.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(panel.AreVerticalSnapPointsRegular);
        });
    }

    [TestMethod]
    public void StackPanelExHorizontalSpacingSkipsCollapsedChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 160,
                Height = 70,
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(width: 40);
            var collapsed = CreateStretchButton(width: 50);
            collapsed.Visibility = Visibility.Collapsed;
            var second = CreateStretchButton(width: 30);
            panel.Children.Add(first);
            panel.Children.Add(collapsed);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 180, height: 90);

            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 40, 40));
            AssertBoundsRelativeTo(second, panel, new Rect(65, 15, 30, 40));
        });
    }

    [TestMethod]
    public void StackPanelExOrientationChangeReflowsChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 140,
                Height = 100,
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            var first = CreateStretchButton(width: 40, height: 20);
            var second = CreateStretchButton(width: 30, height: 15);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 160, height: 120);

            AssertStackAxisOffsetRelativeTo(first, panel, Orientation.Vertical, 0);
            AssertStackAxisOffsetRelativeTo(second, panel, Orientation.Vertical, 25);

            panel.Orientation = Orientation.Horizontal;
            host.UpdateLayout();

            AssertStackAxisOffsetRelativeTo(first, panel, Orientation.Horizontal, 0);
            AssertStackAxisOffsetRelativeTo(second, panel, Orientation.Horizontal, 45);
        });
    }

    [TestMethod]
    public void StackPanelExDesiredSizeCountsVisibleSpacingAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Orientation = Orientation.Vertical,
                Spacing = 7,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));
            panel.Children.Add(new Button
            {
                Width = 100,
                Height = 80,
                Visibility = Visibility.Collapsed
            });
            panel.Children.Add(CreateStretchButton(width: 30, height: 15));

            panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.AreEqual(80, panel.DesiredSize.Width, 1.0, "Desired width should include max visible child width plus border and padding.");
            Assert.AreEqual(72, panel.DesiredSize.Height, 1.0, "Desired height should include visible children, one spacing gap, border, and padding.");
        });
    }

    [TestMethod]
    public void StackPanelExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:StackPanelEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    AreScrollSnapPointsRegular="True"
                    Spacing="4"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Button Content="Parsed" />
                </controls:StackPanelEx>
                """;

            var panel = (ModernStackPanelEx)XamlReader.Parse(xaml);

            Assert.IsTrue(panel.AreScrollSnapPointsRegular);
            Assert.AreEqual(4, panel.Spacing);
            Assert.AreEqual(new Thickness(2), panel.Padding);
            Assert.AreEqual(new CornerRadius(3), panel.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void StackPanelExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(panel);
        });
    }

    [TestMethod]
    public void StackPanelExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void StackPanelExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            panel.Children.Add(CreateRedChildBox());

            AssertRoundedChildRenderClip(panel);
        });
    }

    [TestMethod]
    public void GridExSupportsSpacingSpansAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 230,
                Height = 130,
                UseLayoutRounding = false,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(5),
                RowSpacing = 10,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            var spanned = CreateStretchButton();

            Grid.SetColumn(second, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(first);
            grid.Children.Add(second);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 250, height: 150);

            AssertBoundsRelativeTo(first, grid, new Rect(7, 7, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(117, 7, 100, 50));
            AssertBoundsRelativeTo(spanned, grid, new Rect(7, 67, 210, 50));
        });
    }

    [TestMethod]
    public void GridExUsesWinUINegativeSpacingLayout()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 190,
                Height = 90,
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            var spanned = CreateStretchButton();

            Grid.SetColumn(second, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(first);
            grid.Children.Add(second);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 210, height: 110);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(90, 0, 100, 50));
            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 40, 190, 50));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingHandlesAutoAndStarTracks()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 190,
                Height = 90,
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var autoCell = CreateLayoutBox(width: 80, height: 40);
            var starCell = CreateLayoutBox(height: 40);
            var spanned = CreateLayoutBox();

            Grid.SetColumn(starCell, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(autoCell);
            grid.Children.Add(starCell);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 210, height: 110);

            AssertBoundsRelativeTo(autoCell, grid, new Rect(0, 0, 80, 40));
            AssertBoundsRelativeTo(starCell, grid, new Rect(70, 0, 120, 40));
            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 30, 190, 60));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingDesiredSizeUsesAutoTracks()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var first = CreateLayoutBox(width: 80, height: 30);
            var second = CreateLayoutBox(width: 70, height: 20);
            Grid.SetColumn(second, 1);
            Grid.SetRow(second, 1);

            grid.Children.Add(first);
            grid.Children.Add(second);

            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.AreEqual(140, grid.DesiredSize.Width, 1.0, "Desired width should subtract the negative column spacing from auto tracks.");
            Assert.AreEqual(40, grid.DesiredSize.Height, 1.0, "Desired height should subtract the negative row spacing from auto tracks.");
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 110,
                Height = 30,
                UseLayoutRounding = false,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var spanned = CreateLayoutBox(width: 110);
            var secondColumnProbe = CreateLayoutBox();
            Grid.SetColumnSpan(spanned, 2);
            Grid.SetColumn(secondColumnProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondColumnProbe);

            using var host = new TestWindowHost(grid, width: 130, height: 50);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 110, 30));
            AssertBoundsRelativeTo(secondColumnProbe, grid, new Rect(50, 0, 60, 30));
        });
    }

    [TestMethod]
    public void GridExPositiveSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 130,
                Height = 30,
                UseLayoutRounding = false,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var spanned = CreateLayoutBox(width: 130);
            var secondColumnProbe = CreateLayoutBox();
            Grid.SetColumnSpan(spanned, 2);
            Grid.SetColumn(secondColumnProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondColumnProbe);

            using var host = new TestWindowHost(grid, width: 150, height: 50);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 130, 30));
            AssertBoundsRelativeTo(secondColumnProbe, grid, new Rect(70, 0, 60, 30));
        });
    }

    [TestMethod]
    public void GridExNegativeRowSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 110,
                UseLayoutRounding = false,
                RowSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var spanned = CreateLayoutBox(height: 110);
            var secondRowProbe = CreateLayoutBox();
            Grid.SetRowSpan(spanned, 2);
            Grid.SetRow(secondRowProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondRowProbe);

            using var host = new TestWindowHost(grid, width: 50, height: 130);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 30, 110));
            AssertBoundsRelativeTo(secondRowProbe, grid, new Rect(0, 50, 30, 60));
        });
    }

    [TestMethod]
    public void GridExPositiveRowSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 130,
                UseLayoutRounding = false,
                RowSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var spanned = CreateLayoutBox(height: 130);
            var secondRowProbe = CreateLayoutBox();
            Grid.SetRowSpan(spanned, 2);
            Grid.SetRow(secondRowProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondRowProbe);

            using var host = new TestWindowHost(grid, width: 50, height: 150);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 30, 130));
            AssertBoundsRelativeTo(secondRowProbe, grid, new Rect(0, 70, 30, 60));
        });
    }

    [TestMethod]
    public void GridExPositiveSpacingHandlesStarSpans()
    {
        WpfTestHost.Run(() =>
        {
            var grid = CreateStarSpanGrid(width: 430, height: 320, spacing: 10);
            var first = CreateLayoutBox();
            var middle = CreateLayoutBox();
            var trailing = CreateLayoutBox();

            Grid.SetColumnSpan(first, 2);
            Grid.SetRow(middle, 1);
            Grid.SetColumn(middle, 1);
            Grid.SetColumnSpan(middle, 2);
            Grid.SetRow(trailing, 2);
            Grid.SetColumn(trailing, 2);
            Grid.SetColumnSpan(trailing, 2);

            grid.Children.Add(first);
            grid.Children.Add(middle);
            grid.Children.Add(trailing);

            using var host = new TestWindowHost(grid, width: 450, height: 340);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 210, 100));
            AssertBoundsRelativeTo(middle, grid, new Rect(110, 110, 210, 100));
            AssertBoundsRelativeTo(trailing, grid, new Rect(220, 220, 210, 100));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingHandlesStarSpans()
    {
        WpfTestHost.Run(() =>
        {
            var grid = CreateStarSpanGrid(width: 370, height: 280, spacing: -10);
            var first = CreateLayoutBox();
            var middle = CreateLayoutBox();
            var trailing = CreateLayoutBox();

            Grid.SetColumnSpan(first, 2);
            Grid.SetRow(middle, 1);
            Grid.SetColumn(middle, 1);
            Grid.SetColumnSpan(middle, 2);
            Grid.SetRow(trailing, 2);
            Grid.SetColumn(trailing, 2);
            Grid.SetColumnSpan(trailing, 2);

            grid.Children.Add(first);
            grid.Children.Add(middle);
            grid.Children.Add(trailing);

            using var host = new TestWindowHost(grid, width: 390, height: 300);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 190, 100));
            AssertBoundsRelativeTo(middle, grid, new Rect(90, 90, 190, 100));
            AssertBoundsRelativeTo(trailing, grid, new Rect(180, 180, 190, 100));
        });
    }

    [TestMethod]
    public void GridExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(grid);
        });
    }

    [TestMethod]
    public void GridExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ModernGridEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void GridExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            grid.Children.Add(CreateRedChildBox());

            AssertRoundedChildRenderClip(grid);
        });
    }

    [TestMethod]
    public void GridExAllowsNegativeSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                RowSpacing = -10,
                ColumnSpacing = -11
            };

            Assert.AreEqual(-10, grid.RowSpacing);
            Assert.AreEqual(-11, grid.ColumnSpacing);
            Assert.ThrowsException<ArgumentException>(() => grid.RowSpacing = double.NaN);
            Assert.ThrowsException<ArgumentException>(() => grid.ColumnSpacing = double.NaN);
        });
    }

    [TestMethod]
    public void GridExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:GridEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    RowSpacing="4"
                    ColumnSpacing="6"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <controls:GridEx.RowDefinitions>
                        <RowDefinition Height="Auto" />
                    </controls:GridEx.RowDefinitions>
                    <controls:GridEx.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                    </controls:GridEx.ColumnDefinitions>
                    <Button Content="Parsed" />
                </controls:GridEx>
                """;

            var grid = (ModernGridEx)XamlReader.Parse(xaml);

            Assert.AreEqual(4, grid.RowSpacing);
            Assert.AreEqual(6, grid.ColumnSpacing);
            Assert.AreEqual(new Thickness(2), grid.Padding);
            Assert.AreEqual(new CornerRadius(3), grid.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, grid.BackgroundSizing);
            Assert.AreEqual(1, grid.Children.Count);
        });
    }

    private static Button CreateButton(double width, double height)
    {
        return new Button
        {
            Width = width,
            Height = height
        };
    }

    private static Button CreateStretchButton(double? width = null, double? height = null)
    {
        return new Button
        {
            Width = width ?? double.NaN,
            Height = height ?? double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static System.Windows.Controls.Border CreateLayoutBox(double? width = null, double? height = null)
    {
        return new System.Windows.Controls.Border
        {
            Width = width ?? double.NaN,
            Height = height ?? double.NaN,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static System.Windows.Controls.Border CreateRedChildBox()
    {
        return new System.Windows.Controls.Border
        {
            Width = 30,
            Height = 30,
            Background = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static ModernGridEx CreateStarSpanGrid(double width, double height, double spacing)
    {
        var grid = new ModernGridEx
        {
            Width = width,
            Height = height,
            UseLayoutRounding = false,
            RowSpacing = spacing,
            ColumnSpacing = spacing
        };

        for (int i = 0; i < 4; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int i = 0; i < 3; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        return grid;
    }

    private static void AssertBoundsRelativeTo(FrameworkElement element, Visual ancestor, Rect expected)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point());
        var actual = new Rect(origin, element.RenderSize);
        Assert.AreEqual(expected.X, actual.X, 1.0, "X");
        Assert.AreEqual(expected.Y, actual.Y, 1.0, "Y");
        Assert.AreEqual(expected.Width, actual.Width, 2.0, "Width");
        Assert.AreEqual(expected.Height, actual.Height, 2.0, "Height");
    }

    private static void AssertStackAxisOffsetRelativeTo(FrameworkElement element, Visual ancestor, Orientation orientation, double expected)
    {
        var actual = element.TransformToAncestor(ancestor).Transform(new Point());
        Assert.AreEqual(expected, orientation == Orientation.Horizontal ? actual.X : actual.Y, 1.0, orientation.ToString());
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static MenuItem CreateMenuItemWithTemplate(string templateResourceId, object header, object? icon, bool isEnabled)
    {
        return new MenuItem
        {
            Header = header,
            Icon = icon,
            IsEnabled = isEnabled,
            Template = FindMenuItemTemplate(templateResourceId)
        };
    }

    private static ControlTemplate FindMenuItemTemplate(string resourceId)
    {
        var key = new ComponentResourceKey(typeof(MenuItem), resourceId);
        return Application.Current.TryFindResource(key) as ControlTemplate
            ?? throw new AssertFailedException($"Expected MenuItem template resource '{resourceId}'.");
    }

    private static Style FindStyleResource(string resourceId)
    {
        return Application.Current.TryFindResource(resourceId) as Style
            ?? throw new AssertFailedException($"Expected style resource '{resourceId}'.");
    }

    private static void AssertMenuTemplatePresenterSlot(MenuItem menuItem, string expectedForegroundResource)
    {
        var contentPresenter = FindTemplateChild<ContentPresenterEx>(menuItem, "ContentPresenter");
        Assert.AreEqual(menuItem.Header, contentPresenter.Content);
        Assert.AreSame(contentPresenter.TryFindResource(expectedForegroundResource), contentPresenter.Foreground);

        var iconContent = FindTemplateChild<ContentPresenterEx>(menuItem, "IconContent");
        Assert.AreEqual(menuItem.Icon, iconContent.Content);
        Assert.AreSame(iconContent.TryFindResource(expectedForegroundResource), iconContent.Foreground);
    }

    private static void AssertCalendarNavigationPresenter(Button button)
    {
        var presenter = FindTemplateChild<ContentPresenterEx>(button, "Text");
        Assert.AreEqual(button.Content, presenter.Content);
        Assert.AreSame(button.Foreground, presenter.Foreground);
        Assert.AreEqual(button.Padding, presenter.Padding);
        Assert.AreEqual(ControlHelper.GetCornerRadius(button), presenter.CornerRadius);
        Assert.AreSame(presenter.TryFindResource("CalendarViewNavigationButtonBorderBrush"), presenter.BorderBrush);
    }

    private static void AssertDataGridPresenter(DependencyObject root, object expectedContent, Brush expectedForeground)
    {
        var presenter = FindVisualChild<ContentPresenterEx>(root)
            ?? throw new AssertFailedException($"Expected {root.GetType().Name} template to use ContentPresenterEx.");
        Assert.AreEqual(expectedContent, presenter.Content);
        Assert.AreSame(expectedForeground, presenter.Foreground);
    }

    private static Color RenderBorderEdgePixel(BackgroundSizing backgroundSizing)
    {
        var border = new BorderEx
        {
            Width = 24,
            Height = 24,
            Background = Brushes.Red,
            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)),
            BorderThickness = new Thickness(6),
            BackgroundSizing = backgroundSizing
        };

        return RenderBorderPixel(border, 3, 12, 24, 24);
    }

    private static Color RenderBorderPixel(BorderEx border, int x, int y, int width, int height)
    {
        return RenderElementPixel(border, x, y, width, height);
    }

    private static void AssertOuterChromePixels(FrameworkElement element)
    {
        var roundedCorner = RenderElementPixel(element, 27, 1, 30, 30);
        var straightEdge = RenderElementPixel(element, 1, 15, 30, 30);

        Assert.IsTrue(roundedCorner.A < 30, $"Expected inflated WinUI outer corner to clip the pixel. Pixel={roundedCorner}");
        Assert.IsTrue(straightEdge.R > 200 && straightEdge.A > 200, $"Expected outer edge background under the transparent border. Pixel={straightEdge}");
    }

    private static void AssertRoundedChromeHitTest(FrameworkElement element)
    {
        element.Measure(new Size(30, 30));
        element.Arrange(new Rect(0, 0, 30, 30));
        element.UpdateLayout();

        Assert.IsNull(VisualTreeHelper.HitTest(element, new Point(1, 1)), "Top-left point should be clipped by the rounded chrome.");
        Assert.IsNotNull(VisualTreeHelper.HitTest(element, new Point(15, 15)), "Center point should hit inside the rounded chrome.");
    }

    private static void AssertRoundedChildRenderClip(FrameworkElement element)
    {
        var clippedCorner = RenderElementPixel(element, 1, 1, 30, 30);
        var center = RenderElementPixel(element, 15, 15, 30, 30);

        Assert.IsTrue(clippedCorner.A < 30, $"Expected child content to be clipped out of the rounded corner. Pixel={clippedCorner}");
        Assert.IsTrue(center.R > 200 && center.A > 200, $"Expected child content to render inside the rounded clip. Pixel={center}");
    }

    private static Color RenderElementPixel(FrameworkElement element, int x, int y, int width, int height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        var pixels = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
        return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
    }

    private sealed class TestBorderEx : BorderEx
    {
        public Geometry GetLayoutClipForTest(Size layoutSlotSize)
        {
            return base.GetLayoutClip(layoutSlotSize);
        }
    }
}
