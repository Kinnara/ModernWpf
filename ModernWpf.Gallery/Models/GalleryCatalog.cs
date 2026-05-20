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

        private static IReadOnlyList<GalleryGroup> SourceGroups
        {
            get { return GalleryCatalogData.Groups; }
        }

        public static IReadOnlyList<GalleryItem> Items
        {
            get { return CatalogItems; }
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
            return sourceItems
                .Concat(wpfItems.Where(wpfItem => sourceItems.All(sourceItem => !string.Equals(sourceItem.UniqueId, wpfItem.UniqueId, StringComparison.OrdinalIgnoreCase))))
                .ToArray();
        }

        private static string NormalizeItemLookupId(string uniqueId)
        {
            if (string.Equals(uniqueId, "File and Folder Dialogs", StringComparison.OrdinalIgnoreCase))
            {
                return "FileAndFolderDialogs";
            }

            return uniqueId;
        }

        private static IReadOnlyList<GalleryItem> CreateWpfGalleryItems()
        {
            return new[]
            {
                CreateWpfItem(
                    "DateAndTime",
                    "Calendar",
                    "Calendar",
                    "A control that presents a calendar for choosing one or more dates.",
                    "CalendarView.png",
                    "The WPF Calendar control lets users select dates directly from a month view.",
                    "System.Windows.Controls.Calendar",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "Calendar" },
                    new[] { "DatePicker", "CalendarView" }),
                CreateWpfItem(
                    "Collections",
                    "DataGrid",
                    "DataGrid",
                    "A tabular data control for displaying and editing rows and columns.",
                    "GridView.png",
                    "DataGrid presents data in a customizable table with columns, rows, selection, sorting, and editing behavior.",
                    "System.Windows.Controls.DataGrid",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "MultiSelector", "DataGrid" },
                    new[] { "ListView", "GridView" }),
                CreateWpfItem(
                    "Layout",
                    "GroupBox",
                    "GroupBox",
                    "A container that visually groups related controls under a header.",
                    "Border.png",
                    "GroupBox provides a labeled boundary for related settings or controls while preserving normal WPF layout behavior.",
                    "System.Windows.Controls.GroupBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "HeaderedContentControl", "GroupBox" },
                    new[] { "Border", "Expander" }),
                CreateWpfItem(
                    "Layout",
                    "GridSplitter",
                    "GridSplitter",
                    "A control that lets users resize rows or columns in a Grid.",
                    "Grid.png",
                    "GridSplitter redistributes space between Grid rows or columns at runtime.",
                    "System.Windows.Controls.GridSplitter",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "Thumb", "GridSplitter" },
                    new[] { "Grid", "ResizeGrip" }),
                CreateWpfItem(
                    "Layout",
                    "ResizeGrip",
                    "ResizeGrip",
                    "A small resize affordance typically shown at the corner of a resizable window.",
                    "Placeholder.png",
                    "ResizeGrip gives users a recognizable handle for resizing a host surface.",
                    "System.Windows.Controls.Primitives.ResizeGrip",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ResizeGrip" },
                    new[] { "GridSplitter" }),
                CreateWpfItem(
                    "Text",
                    "Label",
                    "Label",
                    "A text label that can target another control and expose access keys.",
                    "TextBlock.png",
                    "Label identifies another element and can move keyboard focus to that target when the access key is pressed.",
                    "System.Windows.Controls.Label",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "Label" },
                    new[] { "TextBlock", "TextBox" }),
                CreateWpfItem(
                    "Text",
                    "Hyperlink",
                    "Hyperlink",
                    "An inline text element that responds to navigation requests.",
                    "HyperlinkButton.png",
                    "Hyperlink appears inside flow or text content and raises navigation events for links.",
                    "System.Windows.Documents.Hyperlink",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "ContentElement", "FrameworkContentElement", "TextElement", "Inline", "Span", "Hyperlink" },
                    new[] { "HyperlinkButton", "TextBlock" }),
                CreateWpfItem(
                    "Text",
                    "RichTextEdit",
                    "RichTextEdit",
                    "A rich text editor for formatted text content.",
                    "RichEditBox.png",
                    "WPF uses RichTextBox for editable formatted documents with paragraphs, inline formatting, and flow content.",
                    "System.Windows.Controls.RichTextBox",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "RichTextBox" },
                    new[] { "RichEditBox", "TextBox", "TextBlock" }),
                CreateWpfItem(
                    "System",
                    "MessageBox",
                    "MessageBox",
                    "A system dialog for short modal messages and choices.",
                    "ContentDialog.png",
                    "MessageBox displays simple modal prompts through the WPF windowing stack.",
                    "System.Windows.MessageBox",
                    new[] { "MessageBox" },
                    new[] { "ContentDialog" }),
                CreateWpfItem(
                    "System",
                    "FileAndFolderDialogs",
                    "File and Folder Dialogs",
                    "Common system dialogs for choosing files, save paths, and folders.",
                    "FilePicker.png",
                    "WPF apps use Microsoft.Win32 dialogs for common file and save picker workflows.",
                    "Microsoft.Win32.OpenFileDialog",
                    new[] { "Object", "CommonDialog", "FileDialog", "OpenFileDialog" },
                    new[] { "StoragePickers" }),
                CreateWpfItem(
                    "Navigation",
                    "Frame",
                    "Frame",
                    "A navigation host that displays Page content.",
                    "NavigationView.png",
                    "Frame hosts navigable WPF Page instances and maintains a navigation journal.",
                    "System.Windows.Controls.Frame",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ContentControl", "Frame" },
                    new[] { "NavigationView", "PageTransition" }),
                CreateWpfItem(
                    "Navigation",
                    "NavigationWindow",
                    "NavigationWindow",
                    "A top-level window with built-in page navigation.",
                    "AppWindow.png",
                    "NavigationWindow hosts Page content in its own window and provides browser-style navigation chrome.",
                    "System.Windows.Navigation.NavigationWindow",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "ContentControl", "Window", "NavigationWindow" },
                    new[] { "Frame", "CreateMultipleWindows" }),
                CreateWpfItem(
                    "Navigation",
                    "Menu",
                    "Menu",
                    "A classic WPF menu with nested commands and keyboard access.",
                    "MenuBar.png",
                    "Menu provides top-level commands through MenuItem children, separators, and access keys.",
                    "System.Windows.Controls.Menu",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "MenuBase", "Menu" },
                    new[] { "MenuBar", "MenuFlyout" }),
                CreateWpfItem(
                    "Navigation",
                    "TabControl",
                    "TabControl",
                    "A selector that switches between multiple tabbed content pages.",
                    "TabView.png",
                    "TabControl presents a set of TabItem pages with one active selection.",
                    "System.Windows.Controls.TabControl",
                    new[] { "Object", "DispatcherObject", "DependencyObject", "Visual", "UIElement", "FrameworkElement", "Control", "ItemsControl", "Selector", "TabControl" },
                    new[] { "TabView", "Pivot" })
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
            IReadOnlyList<string> relatedControlIds)
        {
            return new GalleryItem(
                groupId,
                uniqueId,
                title,
                subtitle,
                ControlImagePath + imageFileName,
                description,
                apiNamespace,
                false,
                false,
                baseClasses,
                Array.Empty<GalleryDocLink>(),
                relatedControlIds);
        }

        private static IReadOnlyList<GalleryGroup> CreateDisplayGroups()
        {
            return new[]
            {
                CreateGroup(
                    "WpfGalleryControls",
                    "WPF Gallery",
                    "Stock WPF controls and platform features shown with the Fluent WPF theme.",
                    "pack://application:,,,/Assets/HomeHeaderTiles/Header-WindowsDesign.png",
                    new[]
                    {
                        "Button",
                        "CheckBox",
                        "ComboBox",
                        "RadioButton",
                        "Slider",
                        "DatePicker",
                        "Calendar",
                        "ListBox",
                        "ListView",
                        "DataGrid",
                        "GridView",
                        "TreeView",
                        "Label",
                        "Hyperlink",
                        "TextBlock",
                        "TextBox",
                        "PasswordBox",
                        "RichTextEdit",
                        "Border",
                        "Canvas",
                        "Expander",
                        "Grid",
                        "GridSplitter",
                        "GroupBox",
                        "ResizeGrip",
                        "StackPanel",
                        "Image",
                        "Frame",
                        "NavigationWindow",
                        "Menu",
                        "TabControl",
                        "ProgressBar",
                        "ToolTip",
                        "MessageBox",
                        "FileAndFolderDialogs",
                        "Clipboard"
                    }),
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
                    "DesignGuidance",
                    "Design guidance",
                    "Colors, typography, spacing, iconography, accessibility, and XAML fundamentals for modern WPF apps.",
                    "pack://application:,,,/Assets/HomeHeaderTiles/Header-Toolkit.png",
                    new[]
                    {
                        "XamlResources",
                        "XamlStyles",
                        "Binding",
                        "Templates",
                        "CustomUserControls",
                        "Color",
                        "Geometry",
                        "Iconography",
                        "Spacing",
                        "Typography",
                        "AccessibilityColorContrast",
                        "AccessibilityKeyboard",
                        "AccessibilityScreenReader"
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

        private static GalleryGroup CreateGroup(string uniqueId, string title, string subtitle, string imagePath, IReadOnlyList<string> itemIds)
        {
            var items = itemIds
                .Select(FindItem)
                .Where(item => item != null)
                .ToArray();

            return new GalleryGroup(uniqueId, title, subtitle, imagePath, false, items);
        }
    }

    public sealed class GalleryGroup
    {
        public GalleryGroup(string uniqueId, string title, string subtitle, string imagePath, bool isSpecialSection, IReadOnlyList<GalleryItem> items)
        {
            UniqueId = uniqueId;
            Title = title;
            Subtitle = subtitle;
            ImagePath = GalleryAssetUri.Normalize(imagePath);
            IsSpecialSection = isSpecialSection;
            Items = items ?? Array.Empty<GalleryItem>();
        }

        public string UniqueId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string ImagePath { get; }
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
