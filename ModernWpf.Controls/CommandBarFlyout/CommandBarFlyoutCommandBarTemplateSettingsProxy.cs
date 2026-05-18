using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public partial class CommandBarFlyoutCommandBarTemplateSettingsProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new CommandBarFlyoutCommandBarTemplateSettingsProxy();
        }
    }
}
