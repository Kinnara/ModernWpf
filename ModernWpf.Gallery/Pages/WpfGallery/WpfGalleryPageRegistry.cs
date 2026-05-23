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
                { "Clipboard", () => new WpfGallery.SystemPages.ClipboardPage(new WpfGallery.SystemPages.ClipboardPageViewModel()) },
                { "Color", () => new WpfGallery.DesignGuidance.ColorPage(new WpfGallery.DesignGuidance.ColorsPageViewModel()) },
                { "ComboBox", () => new WpfGallery.BasicInput.ComboBoxPage(new WpfGallery.BasicInput.ComboBoxPageViewModel()) },
                { "DataGrid", () => new WpfGallery.Collections.DataGridPage(new WpfGallery.Collections.DataGridPageViewModel()) },
                { "DatePicker", () => new WpfGallery.DateAndTime.DatePickerPage(new WpfGallery.DateAndTime.DatePickerPageViewModel()) },
                { "Expander", () => new WpfGallery.Layout.ExpanderPage(new WpfGallery.Layout.ExpanderPageViewModel()) },
                { "FileAndFolderDialogs", () => new WpfGallery.SystemPages.FileAndFolderDialogsPage(new WpfGallery.SystemPages.FileAndFolderDialogsPageViewModel()) },
                { "Frame", () => new WpfGallery.Navigation.FramePage(new WpfGallery.Navigation.FramePageViewModel()) },
                { "Grid", () => new WpfGallery.Layout.GridPage(new WpfGallery.Layout.GridPageViewModel()) },
                { "GridSplitter", () => new WpfGallery.Layout.GridSplitterPage(new WpfGallery.Layout.GridSplitterPageViewModel()) },
                { "Geometry", () => new WpfGallery.DesignGuidance.GeometryPage(new WpfGallery.DesignGuidance.GeometryPageViewModel()) },
                { "GroupBox", () => new WpfGallery.Layout.GroupBoxPage(new WpfGallery.Layout.GroupBoxPageViewModel()) },
                { "Hyperlink", () => new WpfGallery.Text.HyperlinkPage(new WpfGallery.Text.HyperlinkPageViewModel()) },
                { "Iconography", () => new WpfGallery.DesignGuidance.IconographyPage(new WpfGallery.DesignGuidance.IconographyPageViewModel()) },
                { "Image", () => new WpfGallery.Media.ImagePage(new WpfGallery.Media.ImagePageViewModel()) },
                { "Label", () => new WpfGallery.Text.LabelPage(new WpfGallery.Text.LabelPageViewModel()) },
                { "ListBox", () => new WpfGallery.Collections.ListBoxPage(new WpfGallery.Collections.ListBoxPageViewModel()) },
                { "ListView", () => new WpfGallery.Collections.ListViewPage(new WpfGallery.Collections.ListViewPageViewModel()) },
                { "Menu", () => new WpfGallery.Navigation.MenuPage(new WpfGallery.Navigation.MenuPageViewModel()) },
                { "MessageBox", () => new WpfGallery.SystemPages.MessageBoxPage(new WpfGallery.SystemPages.MessageBoxPageViewModel()) },
                { "NavigationWindow", () => new WpfGallery.Navigation.NavigationWindowPage(new WpfGallery.Navigation.NavigationWindowPageViewModel()) },
                { "PasswordBox", () => new WpfGallery.Text.PasswordBoxPage(new WpfGallery.Text.PasswordBoxPageViewModel()) },
                { "ProgressBar", () => new WpfGallery.StatusAndInfo.ProgressBarPage(new WpfGallery.StatusAndInfo.ProgressBarPageViewModel()) },
                { "RadioButton", () => new WpfGallery.BasicInput.RadioButtonPage(new WpfGallery.BasicInput.RadioButtonPageViewModel()) },
                { "ResizeGrip", () => new WpfGallery.Layout.ResizeGripPage(new WpfGallery.Layout.ResizeGripPageViewModel()) },
                { "RichTextEdit", () => new WpfGallery.Text.RichTextEditPage(new WpfGallery.Text.RichTextEditPageViewModel()) },
                { "Slider", () => new WpfGallery.BasicInput.SliderPage(new WpfGallery.BasicInput.SliderPageViewModel()) },
                { "Spacing", () => new WpfGallery.DesignGuidance.SpacingPage(new WpfGallery.DesignGuidance.SpacingPageViewModel()) },
                { "StackPanel", () => new WpfGallery.Layout.StackPanelPage(new WpfGallery.Layout.StackPanelPageViewModel()) },
                { "TabControl", () => new WpfGallery.Navigation.TabControlPage(new WpfGallery.Navigation.TabControlPageViewModel()) },
                { "TextBlock", () => new WpfGallery.Text.TextBlockPage(new WpfGallery.Text.TextBlockPageViewModel()) },
                { "TextBox", () => new WpfGallery.Text.TextBoxPage(new WpfGallery.Text.TextBoxPageViewModel()) },
                { "ToolTip", () => new WpfGallery.StatusAndInfo.ToolTipPage(new WpfGallery.StatusAndInfo.ToolTipPageViewModel()) },
                { "TreeView", () => new WpfGallery.Collections.TreeViewPage(new WpfGallery.Collections.TreeViewPageViewModel()) },
                { "Typography", () => new WpfGallery.DesignGuidance.TypographyPage(new WpfGallery.DesignGuidance.TypographyPageViewModel()) },
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
