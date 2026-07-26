using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = ContentPresenterTemplatePartName, Type = typeof(ContentPresenterEx))]
    [TemplatePart(Name = PreviousContentPresenterTemplatePartName, Type = typeof(ContentPresenterEx))]
    public class ContentControlEx : ContentControl
    {
        static ContentControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(typeof(ContentControlEx)));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(VerticalAlignment.Top));
            ContentProperty.OverrideMetadata(
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(OnContentPropertyChanged));
        }

        #region BackgroundSizing

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(ModernWpf.Controls.BackgroundSizing.InnerBorderEdge));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        #endregion

        #region BackgroundTransition

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(ContentControlEx),
                new PropertyMetadata(null));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        #endregion

        #region CharacterSpacing

        public static readonly DependencyProperty CharacterSpacingProperty =
            ControlHelper.CharacterSpacingProperty.AddOwner(
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

        public int CharacterSpacing
        {
            get => (int)GetValue(CharacterSpacingProperty);
            set => SetValue(CharacterSpacingProperty, value);
        }

        #endregion

        #region ContentTransitions

        public static readonly DependencyProperty ContentTransitionsProperty =
            DependencyProperty.Register(
                nameof(ContentTransitions),
                typeof(TransitionCollection),
                typeof(ContentControlEx),
                new PropertyMetadata(null, OnContentTransitionsPropertyChanged));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            System.Windows.Controls.Border.CornerRadiusProperty.AddOwner(typeof(ContentControlEx));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsTextScaleFactorEnabled

        public static readonly DependencyProperty IsTextScaleFactorEnabledProperty =
            ControlHelper.IsTextScaleFactorEnabledProperty.AddOwner(
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

        public bool IsTextScaleFactorEnabled
        {
            get => (bool)GetValue(IsTextScaleFactorEnabledProperty);
            set => SetValue(IsTextScaleFactorEnabledProperty, value);
        }

        #endregion

        #region RecognizesAccessKey

        public static readonly DependencyProperty RecognizesAccessKeyProperty =
            DependencyProperty.Register(
                nameof(RecognizesAccessKey),
                typeof(bool),
                typeof(ContentControlEx),
                new FrameworkPropertyMetadata(false));

        public bool RecognizesAccessKey
        {
            get => (bool)GetValue(RecognizesAccessKeyProperty);
            set => SetValue(RecognizesAccessKeyProperty, value);
        }

        #endregion

        public UIElement ContentTemplateRoot => GetContentTemplateRoot();

        public override void OnApplyTemplate()
        {
            StopTransition();

            base.OnApplyTemplate();

            _contentPresenter = GetTemplateChild(ContentPresenterTemplatePartName) as ContentPresenter;
            _previousContentPresenter = GetTemplateChild(PreviousContentPresenterTemplatePartName) as ContentPresenter;

            if (_previousContentPresenter != null)
            {
                _previousContentPresenter.Content = null;
                _previousContentPresenter.Visibility = Visibility.Collapsed;
            }
        }

        private static void OnContentPropertyChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs args)
        {
            ((ContentControlEx)sender).StartContentTransition(args.OldValue, args.NewValue);
        }

        private void StartContentTransition(object oldContent, object newContent)
        {
            StopTransition();

            if (_contentPresenter == null ||
                _previousContentPresenter == null ||
                ReferenceEquals(oldContent, newContent) ||
                !Helper.IsAnimationsEnabled)
            {
                return;
            }

            var transitionInfo = GetNavigationTransitionInfo();
            if (transitionInfo == null)
            {
                return;
            }

            _exitAnimation = oldContent == null
                ? null
                : transitionInfo.GetExitAnimation(_previousContentPresenter, false);
            _enterAnimation = newContent == null
                ? null
                : transitionInfo.GetEnterAnimation(_contentPresenter, false);

            if (_exitAnimation == null && _enterAnimation == null)
            {
                return;
            }

            _previousContentPresenter.Content = oldContent;
            _previousContentPresenter.Opacity = 1;
            _previousContentPresenter.Visibility =
                oldContent == null ? Visibility.Collapsed : Visibility.Visible;
            _previousContentPresenter.IsHitTestVisible = false;

            _contentPresenter.Opacity = 0;
            _contentPresenter.Visibility = Visibility.Visible;
            _contentPresenter.IsHitTestVisible = false;

            BeginTransition();
        }

        private UIElement GetContentTemplateRoot()
        {
            if (_contentPresenter == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(_contentPresenter);
            for (int i = 0; i < childCount; i++)
            {
                if (VisualTreeHelper.GetChild(_contentPresenter, i) is UIElement child)
                {
                    return child;
                }
            }

            return null;
        }

        private static void OnContentTransitionsPropertyChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs args)
        {
            ((ContentControlEx)sender).StopTransition();
        }

        private NavigationTransitionInfo GetNavigationTransitionInfo()
        {
            var transition = ContentTransitions?
                .OfType<NavigationThemeTransition>()
                .LastOrDefault();

            if (transition == null)
            {
                return null;
            }

            return transition.DefaultNavigationTransitionInfo ??
                   new EntranceNavigationTransitionInfo();
        }

        private void BeginTransition()
        {
            if (_exitAnimation != null)
            {
                _exitAnimation.Completed += OnExitAnimationCompleted;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Completed += OnEnterAnimationCompleted;
            }

            _asyncBeginTransition = Dispatcher.BeginInvoke(() =>
            {
                _asyncBeginTransition = null;

                if (_exitAnimation != null)
                {
                    _exitAnimation.Begin();
                }
                else
                {
                    BeginEnterAnimation();
                }
            }, DispatcherPriority.ApplicationIdle);
        }

        private void BeginEnterAnimation()
        {
            ClearPreviousContent();

            if (_contentPresenter != null)
            {
                _contentPresenter.Opacity = 1;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Begin();
            }
            else
            {
                StopTransition();
            }
        }

        private void OnExitAnimationCompleted(object sender, EventArgs e)
        {
            StopExitAnimation();

            if (_enterAnimation != null)
            {
                BeginEnterAnimation();
            }
            else
            {
                StopTransition();
            }
        }

        private void OnEnterAnimationCompleted(object sender, EventArgs e)
        {
            StopEnterAnimation();
            StopTransition();
        }

        private void StopTransition()
        {
            if (_asyncBeginTransition != null)
            {
                _asyncBeginTransition.Abort();
                _asyncBeginTransition = null;
            }

            StopExitAnimation();
            StopEnterAnimation();
            ClearPreviousContent();

            if (_contentPresenter != null)
            {
                _contentPresenter.Visibility = Visibility.Visible;
                _contentPresenter.ClearValue(OpacityProperty);
                _contentPresenter.ClearValue(IsHitTestVisibleProperty);
            }
        }

        private void StopExitAnimation()
        {
            if (_exitAnimation != null)
            {
                _exitAnimation.Completed -= OnExitAnimationCompleted;
                _exitAnimation.Stop();
                _exitAnimation = null;
            }
        }

        private void StopEnterAnimation()
        {
            if (_enterAnimation != null)
            {
                _enterAnimation.Completed -= OnEnterAnimationCompleted;
                _enterAnimation.Stop();
                _enterAnimation = null;
            }
        }

        private void ClearPreviousContent()
        {
            if (_previousContentPresenter != null)
            {
                _previousContentPresenter.Content = null;
                _previousContentPresenter.Visibility = Visibility.Collapsed;
                _previousContentPresenter.ClearValue(OpacityProperty);
                _previousContentPresenter.ClearValue(IsHitTestVisibleProperty);
            }
        }

        private const string ContentPresenterTemplatePartName = "PART_ContentPresenter";
        private const string PreviousContentPresenterTemplatePartName = "PART_PreviousContentPresenter";

        private ContentPresenter _contentPresenter;
        private ContentPresenter _previousContentPresenter;
        private NavigationAnimation _exitAnimation;
        private NavigationAnimation _enterAnimation;
        private DispatcherOperation _asyncBeginTransition;
    }
}
