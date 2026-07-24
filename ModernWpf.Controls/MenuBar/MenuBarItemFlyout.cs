namespace ModernWpf.Controls
{
    public class MenuBarItemFlyout : MenuFlyout
    {
        public MenuBarItemFlyout()
        {
            // Keep stock WPF MenuItem objects on the shared MenuFlyout
            // presenter metrics. In particular, do not replace WinUI's
            // 14-DIP content font with the smaller system-menu font: doing so
            // shrinks every MenuBar flyout label while ordinary MenuFlyout
            // items remain pixel-aligned.
            // Stock WPF MenuItem rows retain a fractional aggregate height;
            // one scoped bottom DIP rounds the four-row presenter to WinUI's
            // 134-pixel surface at 96 DPI.
            Presenter.Padding = new System.Windows.Thickness(0, 2, 0, 3);
            // The stock MenuItem template otherwise starts its glyph run three
            // pixels left of WinUI. Keep the total horizontal inset unchanged
            // so popup width and submenu measurement are unaffected.
            Presenter.Resources["MenuItemSubmenuContentMargin"] = new System.Windows.Thickness(10, 4, 4, 5);
        }
    }
}
