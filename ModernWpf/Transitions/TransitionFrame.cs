// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Enables navigation transitions for pages.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [TemplatePart(Name = FirstTemplatePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SecondTemplatePartName, Type = typeof(ContentPresenter))]
    public class TransitionFrame : Frame
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

        /// <summary>
        /// A single shared instance for setting BitmapCache on a visual.
        /// </summary>
        internal static readonly CacheMode BitmapCacheMode = new BitmapCache();
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
        /// Indicates whether a navigation is forward.
        /// </summary>
        private bool _isForwardNavigation;

        /// <summary>
        /// Determines whether to set the new content to the first or second
        /// <see cref="T:System.Windows.Controls.ContentPresenter"/>.
        /// </summary>
        private bool _useFirstAsNew;

        /// <summary>
        /// A value indicating whether the old transition has completed and the
        /// new transition can begin.
        /// </summary>
        private bool _readyToTransitionToNewContent;

        /// <summary>
        /// A value indicating whether the new content has been loaded and the
        /// new transition can begin.
        /// </summary>
        private bool _contentReady;

        /// <summary>
        /// A value indicating whether the exit transition is currently being performed.
        /// </summary>
        private bool _performingExitTransition;

        /// <summary>
        /// A value indicating whether the navigation is cancelled.
        /// </summary>
        private bool _navigationStopped;

        /// <summary>
        /// The transition to use to move in new content once the old transition
        /// is complete and ready for movement.
        /// </summary>
        private ITransition _storedNewTransition;

        /// <summary>
        /// The stored NavigationIn transition instance to use once the old
        /// transition is complete and ready for movement.
        /// </summary>
        private NavigationInTransition _storedNavigationInTransition;

        /// <summary>
        /// The transition to use to complete the old transition.
        /// </summary>
        private ITransition _storedOldTransition;

        /// <summary>
        /// The stored NavigationOut transition instance.
        /// </summary>
        private NavigationOutTransition _storedNavigationOutTransition;

        static TransitionFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionFrame),
                new FrameworkPropertyMetadata(typeof(TransitionFrame)));
        }

        /// <summary>
        /// Initialzies a new instance of the TransitionFrame class.
        /// </summary>
        public TransitionFrame()
            : base()
        {
            InheritanceBehavior = InheritanceBehavior.Default;
            TransitionsEnabled = SystemParameters.ClientAreaAnimation && RenderCapability.Tier > 0;

            Navigating += OnNavigating;
            NavigationStopped += OnNavigationStopped;
        }

        #region DefaultNavigationInTransition

        public static readonly DependencyProperty DefaultNavigationInTransitionProperty =
            DependencyProperty.Register(
                nameof(DefaultNavigationInTransition),
                typeof(NavigationInTransition),
                typeof(TransitionFrame),
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
                typeof(TransitionFrame),
                null);

        public NavigationOutTransition DefaultNavigationOutTransition
        {
            get => (NavigationOutTransition)GetValue(DefaultNavigationOutTransitionProperty);
            set => SetValue(DefaultNavigationOutTransitionProperty, value);
        }

        #endregion

        #region TransitionsEnabled

        public static readonly DependencyProperty TransitionsEnabledProperty =
            DependencyProperty.Register(
                nameof(TransitionsEnabled),
                typeof(bool),
                typeof(TransitionFrame),
                new PropertyMetadata(true));

        public bool TransitionsEnabled
        {
            get => (bool)GetValue(TransitionsEnabledProperty);
            set => SetValue(TransitionsEnabledProperty, value);
        }

        #endregion

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
        /// Handles the Navigating event of the frame, the immediate way to
        /// begin a transition out before the new page has loaded or had its
        /// layout pass.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // If the current application is not the origin
            // and destination of the navigation, ignore it.
            // e.g. do not play a transition when the 
            // application gets deactivated because the shell
            // will animate the frame out automatically.
            if (!e.IsNavigationInitiator)
            {
                //return;
            }

            _isForwardNavigation = e.NavigationMode != NavigationMode.Back;

            var oldElement = Content as UIElement;
            if (oldElement == null)
            {
                return;
            }

            EnsureLastTransitionIsComplete();

            FlipPresenters();

            TransitionElement oldTransitionElement = null;
            NavigationOutTransition navigationOutTransition = null;
            ITransition oldTransition = null;

            if (TransitionsEnabled)
            {
                if (Helper.HasDefaultValue(oldElement, TransitionService.NavigationOutTransitionProperty))
                {
                    oldElement.SetCurrentValue(TransitionService.NavigationOutTransitionProperty, DefaultNavigationOutTransition);
                }

                navigationOutTransition = TransitionService.GetNavigationOutTransition(oldElement);

                if (navigationOutTransition != null)
                {
                    oldTransitionElement = _isForwardNavigation ? navigationOutTransition.Forward : navigationOutTransition.Backward;
                }
                if (oldTransitionElement != null)
                {
                    oldTransition = oldTransitionElement.GetTransition(oldElement, _oldContentPresenter.RenderSize);
                }
            }

            if (oldTransition != null)
            {
                EnsureStoppedTransition(oldTransition);

                _storedNavigationOutTransition = navigationOutTransition;
                _storedOldTransition = oldTransition;
                oldTransition.Completed += OnExitTransitionCompleted;

                _performingExitTransition = true;

                PerformTransition(navigationOutTransition, _oldContentPresenter, oldTransition);

                PrepareContentPresenterForCompositor(_oldContentPresenter);
            }
            else
            {
                _readyToTransitionToNewContent = true;
            }
        }

        /// <summary>
        /// Handles the NavigationStopped event of the frame. Set a value indicating 
        /// that the navigation is cancelled.
        /// </summary>
        private void OnNavigationStopped(object sender, NavigationEventArgs e)
        {
            _navigationStopped = true;
        }

        /// <summary>
        /// Stops the last navigation transition if it's active and a new navigation occurs.
        /// </summary>
        private void EnsureLastTransitionIsComplete()
        {
            _readyToTransitionToNewContent = false;
            _contentReady = false;

            if (_performingExitTransition)
            {
                Debug.Assert(_storedOldTransition != null && _storedNavigationOutTransition != null);

                // If the app calls GoBack on NavigatedTo, we want the old content to be null
                // because you can't have the same content in two spots on the visual tree.
                if (_oldContentPresenter != null)
                {
                    _oldContentPresenter.Content = null;
                }

                if (_storedOldTransition != null)
                {
                    _storedOldTransition.Stop();
                }

                _storedNavigationOutTransition = null;
                _storedOldTransition = null;

                if (_storedNewTransition != null)
                {
                    _storedNewTransition.Stop();

                    _storedNewTransition = null;
                    _storedNavigationInTransition = null;
                }

                _performingExitTransition = false;
            }
        }

        /// <summary>
        /// Handles the completion of the exit transition, automatically 
        /// continuing to bring in the new element's transition as well if it is
        /// ready.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnExitTransitionCompleted(object sender, EventArgs e)
        {
            _readyToTransitionToNewContent = true;
            _performingExitTransition = false;

            if (_navigationStopped)
            {
                // Restore the old content presenter's interactivity if the navigation is cancelled.
                CompleteTransition(_storedNavigationOutTransition, _oldContentPresenter, _storedOldTransition);
                _navigationStopped = false;
            }
            else
            {
                CompleteTransition(_storedNavigationOutTransition, /*_oldContentPresenter*/ null, _storedOldTransition);
            }
            
            _storedNavigationOutTransition = null;
            _storedOldTransition = null;

            if (_contentReady)
            {
                ITransition newTransition = _storedNewTransition;
                NavigationInTransition navigationInTransition = _storedNavigationInTransition;

                _storedNewTransition = null;
                _storedNavigationInTransition = null;

                TransitionNewContent(newTransition, navigationInTransition);
            }
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

            _readyToTransitionToNewContent = true;

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

            _contentReady = true;

            UIElement oldElement = oldContent as UIElement;
            UIElement newElement = newContent as UIElement;

            // Require the appropriate template parts plus a new element to
            // transition to.
            if (_firstContentPresenter == null || _secondContentPresenter == null || newElement == null)
            {
                return;
            }

            NavigationInTransition navigationInTransition = null;
            ITransition newTransition = null;

            if (oldElement != null && newElement != null && TransitionsEnabled)
            {
                if (Helper.HasDefaultValue(newElement, TransitionService.NavigationInTransitionProperty))
                {
                    newElement.SetCurrentValue(TransitionService.NavigationInTransitionProperty, DefaultNavigationInTransition);
                }

                navigationInTransition = TransitionService.GetNavigationInTransition(newElement);
                TransitionElement newTransitionElement = null;
                if (navigationInTransition != null)
                {
                    newTransitionElement = _isForwardNavigation ? navigationInTransition.Forward : navigationInTransition.Backward;
                }
                if (newTransitionElement != null)
                {
                    newElement.UpdateLayout();

                    Size containerSize = _newContentPresenter.RenderSize;
                    if (containerSize == new Size())
                    {
                        containerSize = _oldContentPresenter.RenderSize;
                    }
                    newTransition = newTransitionElement.GetTransition(newElement, containerSize);
                    PrepareContentPresenterForCompositor(_newContentPresenter);
                }
            }

            _newContentPresenter.Opacity = 0;
            _newContentPresenter.Visibility = Visibility.Visible;
            _newContentPresenter.Content = newElement;

            _oldContentPresenter.Opacity = 1;
            _oldContentPresenter.Visibility = Visibility.Visible;
            _oldContentPresenter.Content = oldElement;

            if (_readyToTransitionToNewContent)
            {
                TransitionNewContent(newTransition, navigationInTransition);
            }
            else
            {
                _storedNewTransition = newTransition;
                _storedNavigationInTransition = navigationInTransition;
            }
        }

        /// <summary>
        /// Transitions the new <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        /// <param name="newTransition">The <see cref="T:ModernWpf.Controls.ITransition"/> 
        /// for the new <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="navigationInTransition">The <see cref="T:ModernWpf.Controls.NavigationInTransition"/> 
        /// for the new <see cref="T:System.Windows.UIElement"/>.</param>
        private void TransitionNewContent(ITransition newTransition, NavigationInTransition navigationInTransition)
        {
            if (_oldContentPresenter != null)
            {
                _oldContentPresenter.Visibility = Visibility.Collapsed;
                _oldContentPresenter.Content = null;
            }

            if (null == newTransition)
            {
                RestoreContentPresenterInteractivity(_newContentPresenter);
                return;
            }

            EnsureStoppedTransition(newTransition);
            newTransition.Completed += delegate
            {
                CompleteTransition(navigationInTransition, _newContentPresenter, newTransition);
            };

            _readyToTransitionToNewContent = false;
            _storedNavigationInTransition = null;
            _storedNewTransition = null;

            PerformTransition(navigationInTransition, _newContentPresenter, newTransition);
        }

        /// <summary>
        /// This checks to make sure that, if the transition not be in the clock
        /// state of Stopped, that is will be stopped.
        /// </summary>
        /// <param name="transition">The transition instance.</param>
        private static void EnsureStoppedTransition(ITransition transition)
        {
            if (transition != null)
            {
                transition.Stop();
            }
        }

        /// <summary>
        /// Performs a transition when given the appropriate components,
        /// includes calling the appropriate start event and ensuring opacity
        /// on the content presenter.
        /// </summary>
        /// <param name="navigationTransition">The navigation transition.</param>
        /// <param name="presenter">The content presenter.</param>
        /// <param name="transition">The transition instance.</param>
        private static void PerformTransition(NavigationTransition navigationTransition, ContentPresenter presenter, ITransition transition)
        {
            if (navigationTransition != null)
            {
                navigationTransition.OnBeginTransition();
            }
            if (presenter != null && presenter.Opacity != 1)
            {
                presenter.Opacity = 1;
            }
            if (transition != null)
            {
                transition.Begin();
            }
        }

        /// <summary>
        /// Completes a transition operation by stopping it, restoring 
        /// interactivity, and then firing the OnEndTransition event.
        /// </summary>
        /// <param name="navigationTransition">The navigation transition.</param>
        /// <param name="presenter">The content presenter.</param>
        /// <param name="transition">The transition instance.</param>
        private static void CompleteTransition(NavigationTransition navigationTransition, ContentPresenter presenter, ITransition transition)
        {
            if (transition != null)
            {
                transition.Stop();
            }

            RestoreContentPresenterInteractivity(presenter);

            if (navigationTransition != null)
            {
                navigationTransition.OnEndTransition();
            }
        }

        /// <summary>
        /// Updates the content presenter for off-thread compositing for the
        /// transition animation. Also disables interactivity on it to prevent
        /// accidental touches.
        /// </summary>
        /// <param name="presenter">The content presenter instance.</param>
        /// <param name="applyBitmapCache">A value indicating whether to apply
        /// a bitmap cache.</param>
        private static void PrepareContentPresenterForCompositor(ContentPresenter presenter, bool applyBitmapCache = true)
        {
            if (presenter != null)
            {
                if (applyBitmapCache)
                {
                    presenter.CacheMode = BitmapCacheMode;
                }
                presenter.IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// Restores the interactivity for the presenter post-animation, also
        /// removes the BitmapCache value.
        /// </summary>
        /// <param name="presenter">The content presenter instance.</param>
        private static void RestoreContentPresenterInteractivity(ContentPresenter presenter)
        {
            if (presenter != null)
            {
                presenter.CacheMode = null;

                if (presenter.Opacity != 1)
                {
                    presenter.Opacity = 1;
                }

                presenter.IsHitTestVisible = true;
            }
        }
    }
}
