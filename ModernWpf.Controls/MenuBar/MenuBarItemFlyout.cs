namespace ModernWpf.Controls
{
    public class MenuBarItemFlyout : MenuFlyout
    {
        public MenuBarItemFlyout()
        {
            // MenuBar hosts stock WPF MenuItem objects. The WPF system-menu
            // typography and content inset reproduce WinUI's 14-DIP glyph
            // metrics more closely for this adapter while preserving four
            // 32-DIP rows. Keep it scoped here so ordinary MenuFlyout items
            // retain their current source typography and exact geometry.
            Presenter.FontFamily = System.Windows.SystemFonts.MenuFontFamily;
            Presenter.FontSize = System.Windows.SystemFonts.MenuFontSize;
            // The stock MenuItem line box contributes a fractional DIP at
            // 96 DPI, so the asymmetric bottom inset keeps the popup HWND at
            // the source 134-pixel height after device rounding.
            Presenter.Padding = new System.Windows.Thickness(0, 2, 0, 1);
            Presenter.Resources["MenuItemSubmenuContentMargin"] = new System.Windows.Thickness(8, 6, 8, 6);
        }
    }
}
