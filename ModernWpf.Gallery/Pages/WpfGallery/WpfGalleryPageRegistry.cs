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
                { "Border", () => new WpfGallery.Layout.BorderPage() },
                { "Button", () => new WpfGallery.BasicInput.ButtonPage() },
                { "Canvas", () => new WpfGallery.Media.CanvasPage() },
                { "Calendar", () => new WpfGallery.DateAndTime.CalendarPage() },
                { "CheckBox", () => new WpfGallery.BasicInput.CheckBoxPage() },
                { "Clipboard", () => new WpfGallery.SystemPages.ClipboardPage() },
                { "Color", () => new WpfGallery.DesignGuidance.ColorPage() },
                { "ComboBox", () => new WpfGallery.BasicInput.ComboBoxPage() },
                { "DataGrid", () => new WpfGallery.Collections.DataGridPage() },
                { "DatePicker", () => new WpfGallery.DateAndTime.DatePickerPage() },
                { "Expander", () => new WpfGallery.Layout.ExpanderPage() },
                { "FileAndFolderDialogs", () => new WpfGallery.SystemPages.FileAndFolderDialogsPage() },
                { "Frame", () => new WpfGallery.Navigation.FramePage() },
                { "Grid", () => new WpfGallery.Layout.GridPage() },
                { "GridSplitter", () => new WpfGallery.Layout.GridSplitterPage() },
                { "Geometry", () => new WpfGallery.DesignGuidance.GeometryPage() },
                { "GroupBox", () => new WpfGallery.Layout.GroupBoxPage() },
                { "Hyperlink", () => new WpfGallery.Text.HyperlinkPage() },
                { "Iconography", () => new WpfGallery.DesignGuidance.IconographyPage() },
                { "Image", () => new WpfGallery.Media.ImagePage() },
                { "Label", () => new WpfGallery.Text.LabelPage() },
                { "ListBox", () => new WpfGallery.Collections.ListBoxPage() },
                { "ListView", () => new WpfGallery.Collections.ListViewPage() },
                { "Menu", () => new WpfGallery.Navigation.MenuPage() },
                { "MessageBox", () => new WpfGallery.SystemPages.MessageBoxPage() },
                { "NavigationWindow", () => new WpfGallery.Navigation.NavigationWindowPage() },
                { "PasswordBox", () => new WpfGallery.Text.PasswordBoxPage() },
                { "ProgressBar", () => new WpfGallery.StatusAndInfo.ProgressBarPage(new WpfGallery.StatusAndInfo.ProgressBarPageViewModel()) },
                { "RadioButton", () => new WpfGallery.BasicInput.RadioButtonPage() },
                { "ResizeGrip", () => new WpfGallery.Layout.ResizeGripPage() },
                { "RichTextEdit", () => new WpfGallery.Text.RichTextEditPage() },
                { "Slider", () => new WpfGallery.BasicInput.SliderPage() },
                { "Spacing", () => new WpfGallery.DesignGuidance.SpacingPage() },
                { "StackPanel", () => new WpfGallery.Layout.StackPanelPage() },
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
