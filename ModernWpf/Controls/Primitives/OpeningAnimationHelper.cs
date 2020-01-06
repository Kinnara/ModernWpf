using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls.Primitives
{
    public static class OpeningAnimationHelper
    {
        #region Storyboard

        public static readonly DependencyProperty StoryboardProperty =
            DependencyProperty.RegisterAttached(
                "Storyboard",
                typeof(Storyboard),
                typeof(OpeningAnimationHelper),
                new PropertyMetadata(OnStoryboardChanged));

        public static Storyboard GetStoryboard(FrameworkElement element)
        {
            return (Storyboard)element.GetValue(StoryboardProperty);
        }

        public static void SetStoryboard(FrameworkElement element, Storyboard value)
        {
            element.SetValue(StoryboardProperty, value);
        }

        private static void OnStoryboardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            element.Loaded -= OnLoaded;
            if (e.NewValue is Storyboard)
            {
                element.Loaded += OnLoaded;
            }
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Helper.IsAnimationsEnabled)
            {
                var element = (FrameworkElement)sender;
                var storyboard = GetStoryboard(element);
                if (storyboard != null)
                {
                    storyboard.Begin();
                }
            }
        }
    }
}
