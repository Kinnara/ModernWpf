using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SamplesCommon
{
    [TemplatePart(Name = FirstTemplatePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SecondTemplatePartName, Type = typeof(ContentPresenter))]
    public class CrossFadeContentControl : ContentControl
    {
        #region Constants and Statics
        /// <summary>
        /// The new
        /// <see cref="T:System.Windows.Controls.ContentPresenter"/>
        /// template part name.
        /// </summary>
        private const string FirstTemplatePartName = "FirstContentPresenter";

        /// <summary>
        /// The old
        /// <see cref="T:System.Windows.Controls.ContentPresenter"/>
        /// template part name.
        /// </summary>
        private const string SecondTemplatePartName = "SecondContentPresenter";
        #endregion

        #region Template Parts
        /// <summary>
        /// The first <see cref="T:System.Windows.Controls.ContentPresenter"/>.
        /// </summary>
        private ContentPresenter _firstContentPresenter;

        /// <summary>
        /// The second <see cref="T:System.Windows.Controls.ContentPresenter"/>.
        /// </summary>
        private ContentPresenter _secondContentPresenter;

        /// <summary>
        /// The new <see cref="T:System.Windows.Controls.ContentPresenter"/>.
        /// </summary>
        private ContentPresenter _newContentPresenter;

        /// <summary>
        /// The old <see cref="T:System.Windows.Controls.ContentPresenter"/>.
        /// </summary>
        private ContentPresenter _oldContentPresenter;
        #endregion

        /// <summary>
        /// Determines whether to set the new content to the first or second
        /// <see cref="T:System.Windows.Controls.ContentPresenter"/>.
        /// </summary>
        private bool _useFirstAsNew;

        private DispatcherOperation _pendingTransition;
        private DoubleAnimation _fadeOutAnimation;

        static CrossFadeContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossFadeContentControl),
                new FrameworkPropertyMetadata(typeof(CrossFadeContentControl)));
        }

        /// <summary>
        /// Initialzies a new instance of the CrossFadeContentControl class.
        /// </summary>
        public CrossFadeContentControl()
            : base()
        {
        }

        #region IsCrossFadeEnabled

        public static readonly DependencyProperty IsCrossFadeEnabledProperty =
            DependencyProperty.Register(
                nameof(IsCrossFadeEnabled),
                typeof(bool),
                typeof(CrossFadeContentControl),
                new PropertyMetadata(true));

        public bool IsCrossFadeEnabled
        {
            get => (bool)GetValue(IsCrossFadeEnabledProperty);
            set => SetValue(IsCrossFadeEnabledProperty, value);
        }

        #endregion

        private bool Animates =>
            SystemParameters.ClientAreaAnimation &&
            RenderCapability.Tier > 0 &&
            IsCrossFadeEnabled;

        /// <summary>
        /// Flips the logical content presenters to prepare for the next visual
        /// transition.
        /// </summary>
        private void FlipPresenters()
        {
            _newContentPresenter = _useFirstAsNew ? _firstContentPresenter : _secondContentPresenter;
            _oldContentPresenter = _useFirstAsNew ? _secondContentPresenter : _firstContentPresenter;
            _useFirstAsNew = !_useFirstAsNew;
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application 
        /// code or internal processes (such as a rebuilding layout pass) call
        /// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// In simplest terms, this means the method is called just before a UI 
        /// element displays in an application.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _firstContentPresenter = GetTemplateChild(FirstTemplatePartName) as ContentPresenter;
            _secondContentPresenter = GetTemplateChild(SecondTemplatePartName) as ContentPresenter;
            _newContentPresenter = _secondContentPresenter;
            _oldContentPresenter = _firstContentPresenter;
            _useFirstAsNew = true;

            if (Content != null)
            {
                OnContentChanged(null, Content);
            }
        }

        /// <summary>
        /// Called when the value of the
        /// <see cref="P:System.Windows.Controls.ContentControl.Content"/>
        /// property changes.
        /// </summary>
        /// <param name="oldContent">The old <see cref="T:System.Object"/>.</param>
        /// <param name="newContent">The new <see cref="T:System.Object"/>.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (_pendingTransition != null)
            {
                _pendingTransition.Abort();
                _pendingTransition = null;
            }

            if (_fadeOutAnimation != null)
            {
                _fadeOutAnimation.Completed -= OnFadeOutAnimationCompleted;
                _fadeOutAnimation = null;
            }

            UIElement oldElement = oldContent as UIElement;
            UIElement newElement = newContent as UIElement;

            // Require the appropriate template parts plus a new element to
            // transition to.
            if (_firstContentPresenter == null || _secondContentPresenter == null || newElement == null)
            {
                return;
            }

            if (oldElement != null)
            {
                FlipPresenters();
            }

            bool useTransition = oldElement != null && Animates;

            _newContentPresenter.Opacity = useTransition ? 0 : 1;
            _newContentPresenter.Visibility = Visibility.Visible;
            _newContentPresenter.Content = newElement;

            _oldContentPresenter.Opacity = 1;
            _oldContentPresenter.Visibility = useTransition ? Visibility.Visible : Visibility.Collapsed;
            _oldContentPresenter.Content = useTransition ? oldElement : null;

            if (useTransition)
            {
                _pendingTransition = Dispatcher.BeginInvoke(() =>
                {
                    _newContentPresenter.Opacity = 1;

                    _fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300), FillBehavior.Stop);
                    _fadeOutAnimation.Completed += OnFadeOutAnimationCompleted;

                    var fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300), FillBehavior.Stop);

                    _oldContentPresenter.BeginAnimation(OpacityProperty, _fadeOutAnimation);
                    _newContentPresenter.BeginAnimation(OpacityProperty, fadeInAnimation);
                }, DispatcherPriority.ApplicationIdle);
            }
        }

        private void OnFadeOutAnimationCompleted(object sender, EventArgs e)
        {
            if (_oldContentPresenter != null)
            {
                _oldContentPresenter.Visibility = Visibility.Collapsed;
                _oldContentPresenter.Content = null;
            }

            _fadeOutAnimation = null;
        }
    }
}
