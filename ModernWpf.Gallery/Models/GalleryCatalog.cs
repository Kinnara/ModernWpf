using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernWpf.Gallery.Models
{
    internal static class GalleryCatalog
    {
        private const string ControlImagePath = "pack://application:,,,/Assets/ControlImages/";

        private static readonly IReadOnlyList<GalleryItem> CatalogItems = CreateItems();
        private static readonly IReadOnlyList<GalleryGroup> DisplayGroups = CreateDisplayGroups();

        public static IReadOnlyList<GalleryGroup> Groups
        {
            get { return DisplayGroups; }
        }

        public static IReadOnlyList<GalleryGroup> OverviewGroups
        {
            get { return DisplayGroups.Where(IsOverviewGroup).ToArray(); }
        }

        private static IReadOnlyList<GalleryGroup> SourceGroups
        {
            get { return GalleryCatalogData.Groups; }
        }

        public static IReadOnlyList<GalleryItem> Items
        {
            get { return CatalogItems; }
        }

        public static IReadOnlyList<GalleryItem> AllControlsItems
        {
            get { return CatalogItems.Where(IsAllControlsItem).ToArray(); }
        }

        public static IReadOnlyList<GalleryItem> NewOrUpdatedItems
        {
            get { return Items.Where(item => item.IsNew || item.IsUpdated).Take(16).ToArray(); }
        }

        public static GalleryGroup FindGroup(string uniqueId)
        {
            return Groups.FirstOrDefault(group => string.Equals(group.UniqueId, uniqueId, StringComparison.OrdinalIgnoreCase))
                ?? SourceGroups.FirstOrDefault(group => string.Equals(group.UniqueId, uniqueId, StringComparison.OrdinalIgnoreCase));
        }

        public static GalleryGroup FindDisplayGroupForItem(string uniqueId)
        {
            return Groups.FirstOrDefault(group => group.Items.Any(item => string.Equals(item.UniqueId, uniqueId, StringComparison.OrdinalIgnoreCase)));
        }

        public static GalleryItem FindItem(string uniqueId)
        {
            uniqueId = NormalizeItemLookupId(uniqueId);
            return Items.FirstOrDefault(item => string.Equals(item.UniqueId, uniqueId, StringComparison.OrdinalIgnoreCase));
        }

        public static IReadOnlyList<GalleryItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Items;
            }

            var tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return Items
                .Where(item => tokens.All(token => item.Matches(token)))
                .OrderByDescending(item => item.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .ThenBy(item => item.Title)
                .ToArray();
        }

        private static IReadOnlyList<GalleryItem> CreateItems()
        {
            var sourceItems = GalleryCatalogData.Items;
            var wpfItems = CreateWpfGalleryItems();
            return wpfItems
                .Concat(sourceItems.Where(sourceItem => wpfItems.All(wpfItem => !string.Equals(sourceItem.UniqueId, wpfItem.UniqueId, StringComparison.OrdinalIgnoreCase))))
                .ToArray();
        }

        private static string NormalizeItemLookupId(string uniqueId)
        {
            if (string.Equals(uniqueId, "File and Folder Dialogs", StringComparison.OrdinalIgnoreCase))
            {
                return "FileAndFolderDialogs";
            }

            if (string.Equals(uniqueId, "Colors", StringComparison.OrdinalIgnoreCase))
            {
                return "Color";
            }

            if (string.Equals(uniqueId, "Icons", StringComparison.OrdinalIgnoreCase))
            {
                return "Iconography";
            }

            if (string.Equals(uniqueId, "User Dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return "UserDashboard";
            }

            return uniqueId;
        }

        private static bool IsOverviewGroup(GalleryGroup group)
        {
            return !string.Equals(group.UniqueId, "DesignGuidance", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(group.UniqueId, "Samples", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(group.UniqueId, "Media", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAllControlsItem(GalleryItem item)
        {
            return !string.Equals(item.GroupId, "Samples", StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<GalleryItem> CreateWpfGalleryItems()
        {
            return new[]
            {
                CreateWpfItem(
                    "DesignGuidance",
                    "Color",
                    "Colors",
                    "Guide showing how to use colors in your app.",
                    "ColorPaletteResources.png",
                    "Guide showing how to use colors in your app",
                    "",
                    new string[0],
                    new[] { "Typography", "Spacing", "Geometry" }),
                CreateWpfItem(
                    "DesignGuidance",
                    "Typography",
                    "Typography",
                    "Guide showing how to use typography in your app.",
                    "TextBlock.png",
                    "Guide showing how to use typography in your app",
                    "",
                    new string[0],
                    new[] { "Color", "Spacing" }),
                CreateWpfItem(
                    "DesignGuidance",
                    "Spacing",
                    "Spacing",
                    "Guide showing how to use spacing in your app.",
                    "CompactSizing.png",
                    "Guide showing how to use spacing in your app",
                    "",
                    new string[0],
                    new[] { "Typography", "Geometry" },
                    isNew: true),
                CreateWpfItem(
                    "DesignGuidance",
                    "Geometry",
                    "Geometry",
                    "Corner radius standards for your app.",
                    "Border.png",
                    "Geometry describes the shape, size and position of UI elements on screen.",
                    "",
                    new string[0],
                    new[] { "Spacing", "Color" },
                    isNew: true),
                CreateWpfItem(
                    "DesignGuidance",
                    "Iconography",
                    "Icons",
                    "Guide showing how to use icons in your app.",
                    "IconElement.png",
                    "Guide showing how to use icons in your application.",
                    "",
                    new string[0],
                    new[] { "Typography", "Color" },
                    isUpdated: true),
                CreateWpfItem(
                    "Samples",
                    "UserDashboard",
                    "User Dashboard",
                    "A dashboard for a user to view and interact with.",
                    "PersonPicture.png",
                    "A dashboard for viewing, editing, adding, and removing users.",
                    "",
                    new string[0],
                    new[] { "ListView", "TextBox", "DatePicker", "Slider", "CheckBox" }),
                CreateWpfItem(
                    "BasicInput",
                    "Button",
                    "Button",
                    "A control that responds to user input and raises a Click event.",
                    "Button.png",
                    "Button responds to user interaction and raises Click for commands or immediate actions.",
                    "System.Windows.Controls.Button",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "ButtonBase", "Button" },
                    new[] { "RepeatButton", "ToggleButton", "HyperlinkButton" }),
                CreateWpfItem(
                    "BasicInput",
                    "CheckBox",
                    "CheckBox",
                    "A control that a user can select or clear.",
                    "Checkbox.png",
                    "CheckBox lets users choose between checked, unchecked, and optional indeterminate states.",
                    "System.Windows.Controls.CheckBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "ButtonBase", "ToggleButton", "CheckBox" },
                    new[] { "RadioButton", "ToggleSwitch" }),
                CreateWpfItem(
                    "BasicInput",
                    "ComboBox",
                    "ComboBox",
                    "A drop-down list of items a user can select from.",
                    "Checkbox.png",
                    "ComboBox displays a selected item and opens a drop-down list for choosing another item.",
                    "System.Windows.Controls.ComboBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "Selector", "ComboBox" },
                    new[] { "ListBox", "AutoSuggestBox" }),
                CreateWpfItem(
                    "BasicInput",
                    "RadioButton",
                    "RadioButton",
                    "A control that allows a user to select a single option from a group of options.",
                    "RadioButton.png",
                    "RadioButton represents one choice in a mutually exclusive set of options.",
                    "System.Windows.Controls.RadioButton",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "ButtonBase", "ToggleButton", "RadioButton" },
                    new[] { "CheckBox", "ToggleButton" }),
                CreateWpfItem(
                    "BasicInput",
                    "Slider",
                    "Slider",
                    "A control that lets the user select from a range of values by moving a Thumb control along a track.",
                    "Slider.png",
                    "Slider lets users choose a numeric value from a continuous or stepped range.",
                    "System.Windows.Controls.Slider",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "RangeBase", "Slider" },
                    new[] { "NumberBox" }),
                CreateWpfItem(
                    "Collections",
                    "DataGrid",
                    "DataGrid",
                    "The DataGrid control presents data in a customizable table of rows and columns.",
                    "DataGrid.png",
                    "DataGrid presents data in a customizable table with columns, rows, selection, sorting, and editing behavior.",
                    "System.Windows.Controls.DataGrid",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "MultiSelector", "DataGrid" },
                    new[] { "ListView", "GridView" },
                    isUpdated: true),
                CreateWpfItem(
                    "Collections",
                    "ListBox",
                    "ListBox",
                    "A control that presents an inline list of items that the user can select from.",
                    "ListBox.png",
                    "ListBox presents selectable items in a list.",
                    "System.Windows.Controls.ListBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "Selector", "ListBox" },
                    new[] { "ListView", "ComboBox" }),
                CreateWpfItem(
                    "Collections",
                    "ListView",
                    "ListView",
                    "A control that presents a collection of items in a vertical list.",
                    "ListView.png",
                    "ListView displays a collection of data items using templates or GridView columns.",
                    "System.Windows.Controls.ListView",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "Selector", "ListBox", "ListView" },
                    new[] { "ListBox", "DataGrid", "GridView" }),
                CreateWpfItem(
                    "Collections",
                    "TreeView",
                    "TreeView",
                    "The TreeView control is a hierarchical list pattern with expanding and collapsing nodes that contain nested items.",
                    "TreeView.png",
                    "TreeView displays hierarchical data with expandable and collapsible nodes.",
                    "System.Windows.Controls.TreeView",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "TreeView" },
                    new[] { "ListView" }),
                CreateWpfItem(
                    "DateAndCalendar",
                    "Calendar",
                    "Calendar",
                    "A control that presents a calendar for a user to choose a date from.",
                    "CalendarView.png",
                    "The WPF Calendar control lets users select dates directly from a month view.",
                    "System.Windows.Controls.Calendar",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "Calendar" },
                    new[] { "DatePicker", "CalendarView" }),
                CreateWpfItem(
                    "DateAndCalendar",
                    "DatePicker",
                    "DatePicker",
                    "A control that lets a user pick a date value.",
                    "DatePicker.png",
                    "DatePicker combines a text field and calendar drop-down for entering a date.",
                    "System.Windows.Controls.DatePicker",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "DatePicker" },
                    new[] { "Calendar", "CalendarDatePicker" }),
                CreateWpfItem(
                    "Layout",
                    "Expander",
                    "Expander",
                    "A container with a header that can be expanded to show a body with more content.",
                    "Expander.png",
                    "Expander reveals or hides content below a header.",
                    "System.Windows.Controls.Expander",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "HeaderedContentControl", "Expander" },
                    new[] { "GroupBox" }),
                CreateWpfItem(
                    "Layout",
                    "Grid",
                    "Grid",
                    "A layout panel that arranges child elements in rows and columns.",
                    "Grid.png",
                    "Grid positions child elements in a flexible row and column layout.",
                    "System.Windows.Controls.Grid",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Panel", "Grid" },
                    new[] { "GridSplitter", "StackPanel", "Canvas" },
                    isNew: true),
                CreateWpfItem(
                    "Layout",
                    "ResizeGrip",
                    "ResizeGrip",
                    "A control that enables users to resize a Window.",
                    "ResizeGrip.png",
                    "ResizeGrip gives users a recognizable handle for resizing a host surface.",
                    "System.Windows.Controls.Primitives.ResizeGrip",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ResizeGrip" },
                    new[] { "GridSplitter" },
                    isNew: true),
                CreateWpfItem(
                    "Layout",
                    "GridSplitter",
                    "GridSplitter",
                    "The GridSplitter redistributes space between columns or rows of a Grid control.",
                    "GridSplitter.png",
                    "GridSplitter redistributes space between Grid rows or columns at runtime.",
                    "System.Windows.Controls.GridSplitter",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "Thumb", "GridSplitter" },
                    new[] { "Grid", "ResizeGrip" },
                    isNew: true),
                CreateWpfItem(
                    "Layout",
                    "GroupBox",
                    "GroupBox",
                    "A control that visually groups controls together while maintaining layout and accessibility.",
                    "GroupBox.png",
                    "GroupBox provides a labeled boundary for related settings or controls while preserving normal WPF layout behavior.",
                    "System.Windows.Controls.GroupBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "HeaderedContentControl", "GroupBox" },
                    new[] { "Border", "Expander" },
                    isNew: true),
                CreateWpfItem(
                    "Layout",
                    "StackPanel",
                    "StackPanel",
                    "A layout panel that arranges child elements into a single line, either horizontally or vertically.",
                    "StackPanel.png",
                    "StackPanel arranges child elements in a single horizontal or vertical line.",
                    "System.Windows.Controls.StackPanel",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Panel", "StackPanel" },
                    new[] { "Grid", "Canvas" },
                    isNew: true),
                CreateWpfItem(
                    "Layout",
                    "Border",
                    "Border",
                    "A decorator that draws a border, background, or both around another element.",
                    "Border.png",
                    "Border draws a background and border around a single child element.",
                    "System.Windows.Controls.Border",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Decorator", "Border" },
                    new[] { "Grid", "GroupBox" },
                    isNew: true),
                CreateWpfItem(
                    "Media",
                    "Canvas",
                    "Canvas",
                    "A layout panel that positions child elements by explicit coordinates.",
                    "Canvas.png",
                    "",
                    "System.Windows.Controls.Canvas",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Panel", "Canvas" },
                    new[] { "Grid", "StackPanel" }),
                CreateWpfItem(
                    "Media",
                    "Image",
                    "Image",
                    "A control that displays image content.",
                    "Image.png",
                    "",
                    "System.Windows.Controls.Image",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Image" },
                    new[] { "Canvas" }),
                CreateWpfItem(
                    "Navigation",
                    "Frame",
                    "Frame",
                    "A navigation control that allows displaying different Page content within an application.",
                    "MenuBar.png",
                    "Frame hosts navigable WPF Page instances and maintains a navigation journal.",
                    "System.Windows.Controls.Frame",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "Frame" },
                    new[] { "NavigationView", "PageTransition" },
                    isNew: true),
                CreateWpfItem(
                    "Navigation",
                    "NavigationWindow",
                    "NavigationWindow",
                    "A control that supports navigation between pages, similar to a web browser.",
                    "CreateMultipleWindows.png",
                    "NavigationWindow hosts Page content in its own window and provides browser-style navigation chrome.",
                    "System.Windows.Navigation.NavigationWindow",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "ContentControl", "Window", "NavigationWindow" },
                    new[] { "Frame", "CreateMultipleWindows" },
                    isNew: true),
                CreateWpfItem(
                    "Navigation",
                    "Menu",
                    "Menu",
                    "A classic menu, allowing the display of MenuItems containing MenuFlyoutItems.",
                    "Pivot.png",
                    "Menu provides top-level commands through MenuItem children, separators, and access keys.",
                    "System.Windows.Controls.Menu",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "MenuBase", "Menu" },
                    new[] { "MenuBar", "MenuFlyout" }),
                CreateWpfItem(
                    "Navigation",
                    "TabControl",
                    "TabControl",
                    "A control that displays a collection of tabs.",
                    "TabView.png",
                    "TabControl presents a set of TabItem pages with one active selection.",
                    "System.Windows.Controls.TabControl",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "Selector", "TabControl" },
                    new[] { "TabView", "Pivot" }),
                CreateWpfItem(
                    "StatusAndInfo",
                    "ProgressBar",
                    "ProgressBar",
                    "Shows the apps progress on a task, or that the app is performing ongoing work that doesn't block user interaction.",
                    "ProgressBar.png",
                    "ProgressBar communicates determinate or indeterminate progress.",
                    "System.Windows.Controls.ProgressBar",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "RangeBase", "ProgressBar" },
                    new[] { "ProgressRing" }),
                CreateWpfItem(
                    "StatusAndInfo",
                    "ToolTip",
                    "ToolTip",
                    "Displays information for an element in a pop-up window.",
                    "ToolTip.png",
                    "ToolTip displays contextual information when a user hovers over or focuses an element.",
                    "System.Windows.Controls.ToolTip",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "ToolTip" },
                    new[] { "TeachingTip" }),
                CreateWpfItem(
                    "Text",
                    "Label",
                    "Label",
                    "Caption of an item.",
                    "Button.png",
                    "",
                    "System.Windows.Controls.Label",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "Label" },
                    new[] { "TextBlock", "TextBox" }),
                CreateWpfItem(
                    "Text",
                    "TextBox",
                    "TextBox",
                    "A single-line or multi-line plain text field.",
                    "TextBox.png",
                    "",
                    "System.Windows.Controls.TextBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "TextBoxBase", "TextBox" },
                    new[] { "PasswordBox", "RichTextEdit" },
                    isUpdated: true),
                CreateWpfItem(
                    "Text",
                    "TextBlock",
                    "TextBlock",
                    "A lightweight control for displaying small amounts of text.",
                    "TextBlock.png",
                    "",
                    "System.Windows.Controls.TextBlock",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "TextBlock" },
                    new[] { "Label", "TextBox" }),
                CreateWpfItem(
                    "Text",
                    "RichTextEdit",
                    "RichTextEdit",
                    "A control that displays formatted text, hyperlinks, inline images, and other rich content.",
                    "RichEditBox.png",
                    "",
                    "System.Windows.Controls.RichTextBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "RichTextBox" },
                    new[] { "RichEditBox", "TextBox", "TextBlock" }),
                CreateWpfItem(
                    "Text",
                    "PasswordBox",
                    "PasswordBox",
                    "A control for entering passwords.",
                    "PasswordBox.png",
                    "",
                    "System.Windows.Controls.PasswordBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "PasswordBox" },
                    new[] { "TextBox" }),
                CreateWpfItem(
                    "Text",
                    "Hyperlink",
                    "Hyperlink",
                    "A control to navigate to another document, webpage, or section within the same page.",
                    "HyperlinkButton.png",
                    "Hyperlink appears inside flow or text content and raises navigation events for links.",
                    "System.Windows.Documents.Hyperlink",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "ContentElement", "FrameworkContentElement", "TextElement", "Inline", "Span", "Hyperlink" },
                    new[] { "HyperlinkButton", "TextBlock" }),
                CreateWpfItem(
                    "System",
                    "FileAndFolderDialogs",
                    "File and Folder Dialogs",
                    "File and folder picker dialogs for opening and saving files.",
                    "FilePicker.png",
                    "WPF apps use Microsoft.Win32 dialogs for common file and save picker workflows.",
                    "Microsoft.Win32.OpenFileDialog",
                    new[] { "Object", "CommonDialog", "FileDialog", "OpenFileDialog" },
                    new[] { "StoragePickers" },
                    isNew: true),
                CreateWpfItem(
                    "System",
                    "MessageBox",
                    "MessageBox",
                    "Display messages and get user responses through dialog boxes.",
                    "ContentDialog.png",
                    "MessageBox displays simple modal prompts through the WPF windowing stack.",
                    "System.Windows.MessageBox",
                    new[] { "MessageBox" },
                    new[] { "ContentDialog" },
                    isNew: true),
                CreateWpfItem(
                    "System",
                    "Clipboard",
                    "Clipboard",
                    "Access and manipulate data stored in the system clipboard.",
                    "Clipboard.png",
                    "Clipboard stores data that users and apps can copy, cut, and paste.",
                    "System.Windows.Clipboard",
                    new[] { "Clipboard" },
                    new string[0],
                    isNew: true)
            };
        }

        private static GalleryItem CreateWpfItem(
            string groupId,
            string uniqueId,
            string title,
            string subtitle,
            string imageFileName,
            string description,
            string apiNamespace,
            IReadOnlyList<string> baseClasses,
            IReadOnlyList<string> relatedControlIds,
            bool isNew = false,
            bool isUpdated = false)
        {
            return new GalleryItem(
                groupId,
                uniqueId,
                title,
                subtitle,
                ControlImagePath + imageFileName,
                description,
                apiNamespace,
                isNew,
                isUpdated,
                baseClasses,
                Array.Empty<GalleryDocLink>(),
                relatedControlIds);
        }

        private static IReadOnlyList<GalleryGroup> CreateDisplayGroups()
        {
            return new[]
            {
                CreateGroup(
                    "DesignGuidance",
                    "Design Guidance",
                    "Guide showing how to use colors, typography, spacing, geometry, and icons in your app.",
                    "pack://application:,,,/Assets/ControlImages/ColorPaletteResources.png",
                    new[]
                    {
                        "Color",
                        "Typography",
                        "Spacing",
                        "Geometry",
                        "Iconography"
                    },
                    "Design guidelines on how to use colors, typography, and icons in your app."),
                CreateGroup(
                    "Samples",
                    "Samples",
                    "User-facing app samples that combine WPF controls into realistic screens.",
                    "pack://application:,,,/Assets/ControlImages/PersonPicture.png",
                    new[]
                    {
                        "UserDashboard"
                    },
                    "Sample pages for common scenarios"),
                CreateGroup(
                    "BasicInput",
                    "Basic Input",
                    "Button, CheckBox, ComboBox, RadioButton, Slider",
                    "pack://application:,,,/Assets/ControlImages/Button.png",
                    new[]
                    {
                        "Button",
                        "CheckBox",
                        "ComboBox",
                        "RadioButton",
                        "Slider"
                    },
                    "Controls for getting user input"),
                CreateGroup(
                    "Collections",
                    "Collections",
                    "DataGrid, ListBox, ListView, TreeView",
                    "pack://application:,,,/Assets/ControlImages/DataGrid.png",
                    new[]
                    {
                        "DataGrid",
                        "ListBox",
                        "ListView",
                        "TreeView"
                    },
                    "Controls for collection presentation"),
                CreateGroup(
                    "DateAndCalendar",
                    "Date & Calendar",
                    "Calendar, DatePicker",
                    "pack://application:,,,/Assets/ControlImages/CalendarView.png",
                    new[]
                    {
                        "Calendar",
                        "DatePicker"
                    },
                    "Controls for date and calendar"),
                CreateGroup(
                    "Layout",
                    "Layout",
                    "Expander,Grid, ResizeGrip, GridSplitter, GroupBox, StackPanel, Border",
                    "pack://application:,,,/Assets/ControlImages/Expander.png",
                    new[]
                    {
                        "Expander",
                        "Grid",
                        "ResizeGrip",
                        "GridSplitter",
                        "GroupBox",
                        "StackPanel",
                        "Border"
                    },
                    "Controls for layouting"),
                CreateGroup(
                    "Media",
                    "Media Controls",
                    "Canvas, Image",
                    "pack://application:,,,/Assets/ControlImages/Image.png",
                    new[]
                    {
                        "Canvas",
                        "Image"
                    },
                    "Controls for media presentation"),
                CreateGroup(
                    "Navigation",
                    "Navigation",
                    "Menu, TabControl, Frame, NavigationWindow",
                    "pack://application:,,,/Assets/ControlImages/Pivot.png",
                    new[]
                    {
                        "Menu",
                        "TabControl",
                        "Frame",
                        "NavigationWindow"
                    },
                    "Controls for navigation and actions"),
                CreateGroup(
                    "StatusAndInfo",
                    "Status & Info",
                    "ProgressBar, ToolTip",
                    "pack://application:,,,/Assets/ControlImages/ProgressBar.png",
                    new[]
                    {
                        "ProgressBar",
                        "ToolTip"
                    },
                    "Controls to show progress and extra information"),
                CreateGroup(
                    "Text",
                    "Text",
                    "Label, TextBox, TextBlock, RichTextEdit, PasswordBox",
                    "pack://application:,,,/Assets/ControlImages/TextBlock.png",
                    new[]
                    {
                        "Label",
                        "TextBox",
                        "TextBlock",
                        "RichTextEdit",
                        "PasswordBox",
                        "Hyperlink"
                    },
                    "Controls for displaying and editing text"),
                CreateGroup(
                    "System",
                    "System",
                    "File and Folder Dialogs, MessageBox, Clipboard",
                    "pack://application:,,,/Assets/ControlImages/FilePicker.png",
                    new[]
                    {
                        "FileAndFolderDialogs",
                        "MessageBox",
                        "Clipboard"
                    },
                    "System-level controls and dialogs"),
                CreateGroup(
                    "ModernWpfControls",
                    "ModernWpf controls",
                    "WinUI-style controls and patterns implemented or adapted for WPF.",
                    "pack://application:,,,/Assets/HomeHeaderTiles/Header-WinUI.png",
                    new[]
                    {
                        "NavigationView",
                        "InfoBar",
                        "NumberBox",
                        "AutoSuggestBox",
                        "ContentDialog",
                        "TeachingTip",
                        "CommandBar",
                        "CommandBarFlyout",
                        "AppBarButton",
                        "AppBarToggleButton",
                        "AppBarSeparator",
                        "DropDownButton",
                        "SplitButton",
                        "ToggleSplitButton",
                        "MenuBar",
                        "MenuFlyout",
                        "ItemsRepeater",
                        "PipsPager",
                        "RatingControl",
                        "ToggleSwitch",
                        "ColorPicker",
                        "HyperlinkButton",
                        "CalendarDatePicker",
                        "ProgressRing",
                        "InfoBadge",
                        "Flyout",
                        "Pivot",
                        "TabView",
                        "RichEditBox",
                        "RichTextBlock",
                        "SplitView",
                        "ScrollViewer",
                        "AnnotatedScrollBar",
                        "PersonPicture",
                        "IconElement",
                        "ThemeShadow",
                        "TitleBar"
                    }),
                CreateGroup(
                    "PlatformAndPatterns",
                    "Platform & patterns",
                    "Windowing, shell, media, motion, system integration, and compatibility samples.",
                    "pack://application:,,,/Assets/HomeHeaderTiles/Header-Store.light.png",
                    new[]
                    {
                        "AppWindow",
                        "AppWindowTitleBar",
                        "CreateMultipleWindows",
                        "AppNotification",
                        "BadgeNotificationManager",
                        "JumpList",
                        "WebView2",
                        "Sound",
                        "MediaPlayerElement",
                        "MapControl",
                        "SystemBackdrops",
                        "SystemBackdropElement",
                        "XamlCompInterop",
                        "StoragePickers",
                        "ConnectedAnimation",
                        "EasingFunction",
                        "ImplicitTransition",
                        "PageTransition",
                        "ThemeTransition",
                        "ParallaxView",
                        "StandardUICommand",
                        "XamlUICommand"
                    })
            };
        }

        private static GalleryGroup CreateGroup(string uniqueId, string title, string subtitle, string imagePath, IReadOnlyList<string> itemIds, string pageDescription = null)
        {
            var items = itemIds
                .Select(FindItem)
                .Where(item => item != null)
                .ToArray();

            return new GalleryGroup(uniqueId, title, subtitle, imagePath, false, items, pageDescription);
        }
    }

    public sealed class GalleryGroup
    {
        public GalleryGroup(string uniqueId, string title, string subtitle, string imagePath, bool isSpecialSection, IReadOnlyList<GalleryItem> items, string pageDescription = null)
        {
            UniqueId = uniqueId;
            Title = title;
            Subtitle = subtitle;
            PageDescription = pageDescription ?? subtitle;
            ImagePath = GalleryAssetUri.Normalize(imagePath);
            IsSpecialSection = isSpecialSection;
            Items = items ?? Array.Empty<GalleryItem>();
        }

        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Description
        {
            get { return Subtitle; }
        }

        public string PageDescription { get; }
        public string ImagePath { get; }
        public Uri ImageSource
        {
            get { return string.IsNullOrEmpty(ImagePath) ? null : new Uri(ImagePath, UriKind.Absolute); }
        }

        public bool IsSpecialSection { get; }
        public IReadOnlyList<GalleryItem> Items { get; }

        public override string ToString()
        {
            return Title;
        }
    }

    public sealed class GalleryItem
    {
        public GalleryItem(
            string groupId,
            string uniqueId,
            string title,
            string subtitle,
            string imagePath,
            string description,
            string apiNamespace,
            bool isNew,
            bool isUpdated,
            IReadOnlyList<string> baseClasses,
            IReadOnlyList<GalleryDocLink> docs,
            IReadOnlyList<string> relatedControlIds)
        {
            GroupId = groupId;
            UniqueId = uniqueId;
            Title = title;
            Subtitle = subtitle;
            ImagePath = GalleryAssetUri.Normalize(imagePath);
            Description = description;
            ApiNamespace = apiNamespace;
            IsNew = isNew;
            IsUpdated = isUpdated;
            BaseClasses = baseClasses ?? Array.Empty<string>();
            Docs = docs ?? Array.Empty<GalleryDocLink>();
            RelatedControlIds = relatedControlIds ?? Array.Empty<string>();
        }

        public string GroupId { get; }
        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string ImagePath { get; }
        public Uri ImageSource
        {
            get { return string.IsNullOrEmpty(ImagePath) ? null : new Uri(ImagePath, UriKind.Absolute); }
        }

        public string Description { get; }
        public string ApiNamespace { get; }
        public bool IsNew { get; }
        public bool IsUpdated { get; }
        public IReadOnlyList<string> BaseClasses { get; }
        public IReadOnlyList<GalleryDocLink> Docs { get; }
        public IReadOnlyList<string> RelatedControlIds { get; }

        public string Badge
        {
            get
            {
                if (IsNew)
                {
                    return "New";
                }

                return IsUpdated ? "Updated" : string.Empty;
            }
        }

        public bool HasBadge
        {
            get { return !string.IsNullOrEmpty(Badge); }
        }

        public string BaseClassText
        {
            get { return BaseClasses.Count == 0 ? string.Empty : string.Join(" > ", BaseClasses); }
        }

        public string GroupTitle
        {
            get
            {
                var group = GalleryCatalog.FindGroup(GroupId);
                return group == null ? string.Empty : group.Title;
            }
        }

        public bool Matches(string token)
        {
            return Contains(Title, token) ||
                Contains(Subtitle, token) ||
                Contains(Description, token) ||
                Contains(UniqueId, token) ||
                Contains(ApiNamespace, token);
        }

        public override string ToString()
        {
            return Title;
        }

        private static bool Contains(string value, string token)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal static class GalleryAssetUri
    {
        private const string ApplicationPackPrefix = "pack://application:,,,/";
        private const string GalleryPackPrefix = "pack://application:,,,/ModernWpf.Gallery;component/";
        private const string MsAppxPrefix = "ms-appx:///";

        public static string Normalize(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return uri;
            }

            if (uri.StartsWith(GalleryPackPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return uri;
            }

            if (uri.StartsWith(MsAppxPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return GalleryPackPrefix + uri.Substring(MsAppxPrefix.Length);
            }

            if (uri.StartsWith(ApplicationPackPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var path = uri.Substring(ApplicationPackPrefix.Length);
                if (path.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return GalleryPackPrefix + path;
                }
            }

            return uri;
        }
    }

    public sealed class GalleryDocLink
    {
        public GalleryDocLink(string title, string uri)
        {
            Title = title;
            Uri = uri;
        }

        public string Title { get; }
        public string Uri { get; }

        public override string ToString()
        {
            return Title;
        }
    }

    public sealed class SampleSnippet
    {
        public SampleSnippet(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string Title { get; }
        public string Text { get; }
    }
}
