using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    public partial class AppBarElementContainer : ContentControl, ICommandBarElement
    {
        static AppBarElementContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarElementContainer),
                new FrameworkPropertyMetadata(typeof(AppBarElementContainer)));
        }

        public AppBarElementContainer()
        {
        }

    }
}
