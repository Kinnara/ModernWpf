using System.Windows;

namespace ModernWpf.Media.Animation
{
    public enum SlideNavigationTransitionEffect
    {
        FromBottom = 0,
        FromLeft = 1,
        FromRight = 2
    }

    public sealed class SlideNavigationTransitionInfo : NavigationTransitionInfo, ISlideNavigationTransitionInfo2
    {
        public SlideNavigationTransitionInfo()
        {
        }

        #region Effect

        public static readonly DependencyProperty EffectProperty =
            DependencyProperty.Register(
                nameof(Effect),
                typeof(SlideNavigationTransitionEffect),
                typeof(SlideNavigationTransitionInfo),
                null);

        public SlideNavigationTransitionEffect Effect
        {
            get => (SlideNavigationTransitionEffect)GetValue(EffectProperty);
            set => SetValue(EffectProperty, value);
        }

        #endregion
    }

    internal interface ISlideNavigationTransitionInfo2
    {
        SlideNavigationTransitionEffect Effect { get; set; }
    }
}
