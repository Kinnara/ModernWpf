using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    internal partial class CommandBarFlyoutCommandBarTemplateSettingsProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new CommandBarFlyoutCommandBarTemplateSettingsProxy();
        }
    }
}
