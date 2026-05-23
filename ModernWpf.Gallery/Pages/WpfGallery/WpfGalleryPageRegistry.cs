using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ModernWpf.Gallery.Pages
{
    internal static class WpfGalleryPageRegistry
    {
        private static readonly IReadOnlyDictionary<string, Func<UIElement>> DirectPageFactories =
            new Dictionary<string, Func<UIElement>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Border", () => new WpfGallery.Layout.BorderPage(new WpfGallery.Layout.BorderPageViewModel()) },
                { "Button", () => new WpfGallery.BasicInput.ButtonPage(new WpfGallery.BasicInput.ButtonPageViewModel()) },
                { "Canvas", () => new WpfGallery.Media.CanvasPage(new WpfGallery.Media.CanvasPageViewModel()) },
                { "Calendar", () => new WpfGallery.DateAndTime.CalendarPage(new WpfGallery.DateAndTime.CalendarPageViewModel()) },
                { "CheckBox", () => new WpfGallery.BasicInput.CheckBoxPage(new WpfGallery.BasicInput.CheckBoxPageViewModel()) },
                { "Clipboard", () => new WpfGallery.SystemPages.ClipboardPage() },
                { "Color", () => new WpfGallery.DesignGuidance.ColorPage() },
                { "ComboBox", () => new WpfGallery.BasicInput.ComboBoxPage(new WpfGallery.BasicInput.ComboBoxPageViewModel()) },
                { "DataGrid", () => new WpfGallery.Collections.DataGridPage() },
                { "DatePicker", () => new WpfGallery.DateAndTime.DatePickerPage(new WpfGallery.DateAndTime.DatePickerPageViewModel()) },
                { "Expander", () => new WpfGallery.Layout.ExpanderPage(new WpfGallery.Layout.ExpanderPageViewModel()) },
                { "FileAndFolderDialogs", () => new WpfGallery.SystemPages.FileAndFolderDialogsPage() },
                { "Frame", () => new WpfGallery.Navigation.FramePage() },
                { "Grid", () => new WpfGallery.Layout.GridPage(new WpfGallery.Layout.GridPageViewModel()) },
                { "GridSplitter", () => new WpfGallery.Layout.GridSplitterPage(new WpfGallery.Layout.GridSplitterPageViewModel()) },
                { "Geometry", () => new WpfGallery.DesignGuidance.GeometryPage() },
                { "GroupBox", () => new WpfGallery.Layout.GroupBoxPage(new WpfGallery.Layout.GroupBoxPageViewModel()) },
                { "Hyperlink", () => new WpfGallery.Text.HyperlinkPage() },
                { "Iconography", () => new WpfGallery.DesignGuidance.IconographyPage() },
                { "Image", () => new WpfGallery.Media.ImagePage(new WpfGallery.Media.ImagePageViewModel()) },
                { "Label", () => new WpfGallery.Text.LabelPage() },
                { "ListBox", () => new WpfGallery.Collections.ListBoxPage() },
                { "ListView", () => new WpfGallery.Collections.ListViewPage() },
                { "Menu", () => new WpfGallery.Navigation.MenuPage() },
                { "MessageBox", () => new WpfGallery.SystemPages.MessageBoxPage() },
                { "NavigationWindow", () => new WpfGallery.Navigation.NavigationWindowPage() },
                { "PasswordBox", () => new WpfGallery.Text.PasswordBoxPage() },
                { "ProgressBar", () => new WpfGallery.StatusAndInfo.ProgressBarPage(new WpfGallery.StatusAndInfo.ProgressBarPageViewModel()) },
                { "RadioButton", () => new WpfGallery.BasicInput.RadioButtonPage(new WpfGallery.BasicInput.RadioButtonPageViewModel()) },
                { "ResizeGrip", () => new WpfGallery.Layout.ResizeGripPage(new WpfGallery.Layout.ResizeGripPageViewModel()) },
                { "RichTextEdit", () => new WpfGallery.Text.RichTextEditPage() },
                { "Slider", () => new WpfGallery.BasicInput.SliderPage(new WpfGallery.BasicInput.SliderPageViewModel()) },
                { "Spacing", () => new WpfGallery.DesignGuidance.SpacingPage() },
                { "StackPanel", () => new WpfGallery.Layout.StackPanelPage(new WpfGallery.Layout.StackPanelPageViewModel()) },
                { "TabControl", () => new WpfGallery.Navigation.TabControlPage() },
                { "TextBlock", () => new WpfGallery.Text.TextBlockPage() },
                { "TextBox", () => new WpfGallery.Text.TextBoxPage() },
                { "ToolTip", () => new WpfGallery.StatusAndInfo.ToolTipPage(new WpfGallery.StatusAndInfo.ToolTipPageViewModel()) },
                { "TreeView", () => new WpfGallery.Collections.TreeViewPage() },
                { "Typography", () => new WpfGallery.DesignGuidance.TypographyPage() },
                { "UserDashboard", () => new WpfGallery.Samples.UserDashboardPage() }
            };

        public static IReadOnlyList<string> DirectPageIds
        {
            get { return DirectPageFactories.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray(); }
        }

        public static bool HasDirectPageContent(string uniqueId)
        {
            return uniqueId != null && DirectPageFactories.ContainsKey(uniqueId);
        }

        public static UIElement CreatePageContent(string uniqueId)
        {
            if (uniqueId == null)
            {
                return null;
            }

            return DirectPageFactories.TryGetValue(uniqueId, out var factory) ? factory() : null;
        }
    }
}
