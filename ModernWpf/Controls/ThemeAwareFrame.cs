using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;

namespace ModernWpf.Controls
{
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ThemeAwareFrame : System.Windows.Controls.Frame
    {
        static ThemeAwareFrame()
        {
            NavigationUIVisibilityProperty.OverrideMetadata(typeof(ThemeAwareFrame), new FrameworkPropertyMetadata(NavigationUIVisibility.Hidden));
            IsTabStopProperty.OverrideMetadata(typeof(ThemeAwareFrame), new FrameworkPropertyMetadata(false));
            FocusableProperty.OverrideMetadata(typeof(ThemeAwareFrame), new FrameworkPropertyMetadata(false));
            FocusVisualStyleProperty.OverrideMetadata(typeof(ThemeAwareFrame), new FrameworkPropertyMetadata(null));
        }

        public ThemeAwareFrame()
        {
            InheritanceBehavior = InheritanceBehavior.Default;
        }
    }
}
