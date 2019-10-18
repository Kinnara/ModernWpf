using ModernWpf.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.Controls
{
    public class TransitionFrameEx : TransitionFrame
    {
        public TransitionFrameEx()
        {
            TransitionsEnabled = SystemParameters.ClientAreaAnimation && RenderCapability.Tier > 0;
        }

        #region DefaultNavigationInTransition

        public static readonly DependencyProperty DefaultNavigationInTransitionProperty =
            DependencyProperty.Register(
                nameof(DefaultNavigationInTransition),
                typeof(NavigationInTransition),
                typeof(TransitionFrameEx),
                null);

        public NavigationInTransition DefaultNavigationInTransition
        {
            get => (NavigationInTransition)GetValue(DefaultNavigationInTransitionProperty);
            set => SetValue(DefaultNavigationInTransitionProperty, value);
        }

        #endregion

        #region DefaultNavigationOutTransition

        public static readonly DependencyProperty DefaultNavigationOutTransitionProperty =
            DependencyProperty.Register(
                nameof(DefaultNavigationOutTransition),
                typeof(NavigationOutTransition),
                typeof(TransitionFrameEx),
                null);

        public NavigationOutTransition DefaultNavigationOutTransition
        {
            get => (NavigationOutTransition)GetValue(DefaultNavigationOutTransitionProperty);
            set => SetValue(DefaultNavigationOutTransitionProperty, value);
        }

        #endregion

        protected override void OnNavigating(NavigatingCancelEventArgs e)
        {
            var oldElement = Content as UIElement;
            if (oldElement == null)
            {
                return;
            }

            if (HasDefaultValue(oldElement, TransitionService.NavigationOutTransitionProperty))
            {
                oldElement.SetCurrentValue(TransitionService.NavigationOutTransitionProperty, DefaultNavigationOutTransition);
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            UIElement newElement = newContent as UIElement;

            if (newElement != null)
            {
                if (HasDefaultValue(newElement, TransitionService.NavigationInTransitionProperty))
                {
                    newElement.SetCurrentValue(TransitionService.NavigationInTransitionProperty, DefaultNavigationInTransition);
                }
                else
                {
                    var valueSource = DependencyPropertyHelper.GetValueSource(newElement, TransitionService.NavigationInTransitionProperty);
                    var value = TransitionService.GetNavigationInTransition(newElement);
                }
            }

            base.OnContentChanged(oldContent, newContent);
        }

        private static bool HasDefaultValue(DependencyObject d, DependencyProperty dp)
        {
            return DependencyPropertyHelper.GetValueSource(d, dp).BaseValueSource == BaseValueSource.Default;
        }
    }
}
