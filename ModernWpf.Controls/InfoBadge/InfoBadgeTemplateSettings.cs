using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class InfoBadgeTemplateSettings : DependencyObject
    {
        internal InfoBadgeTemplateSettings()
        {
        }

        #region IconElement

        private static readonly DependencyPropertyKey IconElementPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IconElement),
                typeof(IconElement),
                typeof(InfoBadgeTemplateSettings),
                null);

        public static readonly DependencyProperty IconElementProperty = IconElementPropertyKey.DependencyProperty;

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementPropertyKey, value);
        }

        #endregion

        #region InfoBadgeCornerRadius

        private static readonly DependencyPropertyKey InfoBadgeCornerRadiusPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(InfoBadgeCornerRadius),
                typeof(CornerRadius),
                typeof(InfoBadgeTemplateSettings),
                null);

        public static readonly DependencyProperty InfoBadgeCornerRadiusProperty = InfoBadgeCornerRadiusPropertyKey.DependencyProperty;

        public CornerRadius InfoBadgeCornerRadius
        {
            get => (CornerRadius)GetValue(InfoBadgeCornerRadiusProperty);
            internal set => SetValue(InfoBadgeCornerRadiusPropertyKey, value);
        }

        #endregion
    }
}
