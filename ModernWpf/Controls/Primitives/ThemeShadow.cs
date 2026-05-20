using System;
using System.Windows.Markup;

namespace ModernWpf.Controls.Primitives
{
    public sealed class ThemeShadow
    {
    }

    [MarkupExtensionReturnType(typeof(ThemeShadow))]
    public sealed class ThemeShadowExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ThemeShadow();
        }
    }
}
