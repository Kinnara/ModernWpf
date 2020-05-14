using ModernWpf.Media.Animation;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xaml;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Displays Page instances, supports navigation to new pages, and maintains a navigation
    /// history to support forward and backward navigation.
    /// </summary>
    [TemplatePart(Name = FirstContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SecondContentPresenterName, Type = typeof(ContentPresenter))]
    public class Frame : System.Windows.Controls.Frame
    {
        static Frame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(typeof(Frame)));
            NavigationUIVisibilityProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(NavigationUIVisibility.Hidden));
            IsTabStopProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(false));
            FocusableProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(false));
            FocusVisualStyleProperty.OverrideMetadata(typeof(Frame), new FrameworkPropertyMetadata(null));
        }

        /// <summary>
        /// Initialzies a new instance of the Frame class.
        /// </summary>
        public Frame() : base()
        {
            InheritanceBehavior = InheritanceBehavior.Default;
            JournalOwnership = JournalOwnership.OwnsJournal;
            SetCurrentValue(ContentTransitionsProperty, new TransitionCollection());

            SetFrame(NavigationService, this);

            Navigating += OnNavigating;
            Navigated += OnNavigated;
            NavigationStopped += OnNavigationStopped;
            NavigationFailed += OnNavigationFailed;
        }

        #region SourcePageType

        /// <summary>
        /// Identifies the SourcePageType dependency property.
        /// </summary>
        public static readonly DependencyProperty SourcePageTypeProperty =
            DependencyProperty.Register(
                nameof(SourcePageType),
                typeof(Type),
                typeof(Frame),
                new PropertyMetadata(OnSourcePageTypePropertyChanged));

        /// <summary>
        /// Gets or sets a type reference of the current content, or the content that should
        /// be navigated to.
        /// </summary>
        /// <returns>
        /// A type reference for the current content, or the content to navigate to.
        /// </returns>
        public Type SourcePageType
        {
            get => (Type)GetValue(SourcePageTypeProperty);
            set => SetValue(SourcePageTypeProperty, value);
        }

        private static void OnSourcePageTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Frame)sender).OnSourcePageTypePropertyChanged(args);
        }

        private void OnSourcePageTypePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!_ignoreSourcePageTypeChanged)
            {
                Navigate((Type)args.NewValue);
            }
        }

        #endregion

        #region CurrentSourcePageType

        private static readonly DependencyPropertyKey CurrentSourcePageTypePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurrentSourcePageType),
                typeof(Type),
                typeof(Frame),
                null);

        /// <summary>
        /// Identifies the CurrentSourcePageType dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentSourcePageTypeProperty =
            CurrentSourcePageTypePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a type reference for the content that is currently displayed.
        /// </summary>
        /// <returns>
        /// A type reference for the content that is currently displayed.
        /// </returns>
        public Type CurrentSourcePageType
        {
            get => (Type)GetValue(CurrentSourcePageTypeProperty);
            private set => SetValue(CurrentSourcePageTypePropertyKey, value);
        }

        #endregion

        #region BackStackDepth

        private static readonly DependencyPropertyKey BackStackDepthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(BackStackDepth),
                typeof(int),
                typeof(Frame),
                null);

        /// <summary>
        /// Identifies the BackStackDepth dependency property.
        /// </summary>
        public static readonly DependencyProperty BackStackDepthProperty =
            BackStackDepthPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the number of entries in the navigation back stack.
        /// </summary>
        /// <returns>The number of entries in the navigation back stack.</returns>
        public int BackStackDepth
        {
            get => (int)GetValue(BackStackDepthProperty);
            private set => SetValue(BackStackDepthPropertyKey, value);
        }

        private void UpdateBackStackDepth()
        {
            BackStackDepth = BackStack?.Cast<object>().Count() ?? 0;
        }

        #endregion

        #region ContentTransitions

        /// <summary>
        /// Identifies the ContentTransitions dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentTransitionsProperty =
            DependencyProperty.Register(
                nameof(ContentTransitions),
                typeof(TransitionCollection),
                typeof(Frame),
                new PropertyMetadata(OnContentTransitionsPropertyChanged));

        /// <summary>
        /// Gets or sets the collection of Transition style elements that apply to the content
        /// of a ContentControl.
        /// </summary>
        /// <returns>The strongly typed collection of Transition style elements.</returns>
        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        private static void OnContentTransitionsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Frame)sender).OnContentTransitionsPropertyChanged(args);
        }

        private void OnContentTransitionsPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DefaultNavigationTransitionInfo =
                ((TransitionCollection)args.NewValue)?
                .OfType<NavigationThemeTransition>()
                .LastOrDefault()?
                .DefaultNavigationTransitionInfo ?? new EntranceNavigationTransitionInfo();
        }

        #endregion

        #region Frame

        private static readonly AttachableMemberIdentifier FrameProperty =
            new AttachableMemberIdentifier(typeof(Frame), "Frame");

        internal static Frame GetFrame(NavigationService navigationService)
        {
            AttachablePropertyServices.TryGetProperty<Frame>(navigationService, FrameProperty, out var value);
            return value;
        }

        private static void SetFrame(NavigationService navigationService, Frame value)
        {
            AttachablePropertyServices.SetProperty(navigationService, FrameProperty, value);
        }

        #endregion

        #region NavigationTransitionInfo

        private static readonly DependencyProperty NavigationTransitionInfoProperty =
            DependencyProperty.RegisterAttached(
                "NavigationTransitionInfo",
                typeof(NavigationTransitionInfo),
                typeof(Frame));

        private static NavigationTransitionInfo GetNavigationTransitionInfo(JournalEntry entry)
        {
            return (NavigationTransitionInfo)entry.GetValue(NavigationTransitionInfoProperty);
        }

        private static void SetNavigationTransitionInfo(JournalEntry entry, NavigationTransitionInfo value)
        {
            entry.SetValue(NavigationTransitionInfoProperty, value);
        }

        #endregion

        private NavigationTransitionInfo DefaultNavigationTransitionInfo { get; set; }

        private JournalEntry BackEntry => BackStack?.OfType<JournalEntry>().FirstOrDefault();

        /// <summary>
        /// Causes the Frame to load content represented by the specified Page.
        /// </summary>
        /// <param name="sourcePageType">The page to navigate to, specified as a type reference to its partial class type.</param>
        /// <returns>true if navigation is not canceled; otherwise, false.</returns>
        public bool Navigate(Type sourcePageType)
        {
            return Navigate(Activator.CreateInstance(sourcePageType));
        }

        /// <summary>
        /// Causes the Frame to load content represented by the specified Page, also passing
        /// a parameter to be interpreted by the target of the navigation.
        /// </summary>
        /// <param name="sourcePageType">The page to navigate to, specified as a type reference to its partial class type.</param>
        /// <param name="parameter">The navigation parameter to pass to the target page.</param>
        /// <returns>true if navigation is not canceled; otherwise, false.</returns>
        public bool Navigate(Type sourcePageType, object parameter)
        {
            return Navigate(Activator.CreateInstance(sourcePageType), parameter);
        }

        /// <summary>
        /// Causes the Frame to load content represented by the specified Page -derived data
        /// type, also passing a parameter to be interpreted by the target of the navigation,
        /// and a value indicating the animated transition to use.
        /// </summary>
        /// <param name="sourcePageType">The page to navigate to, specified as a type reference to its partial class type.</param>
        /// <param name="parameter">The navigation parameter to pass to the target page.</param>
        /// <param name="infoOverride">Info about the animated transition.</param>
        /// <returns>true if navigation is not canceled; otherwise, false.</returns>
        public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            _transitionInfoOverride = infoOverride;
            return Navigate(Activator.CreateInstance(sourcePageType), parameter);
        }

        /// <summary>
        /// Navigates asynchronously to source content located at a uniform resource identifier
        /// (URI), and passes an object that contains data to be used for processing during
        /// navigation, and a value indicating the animated transition to use.
        /// </summary>
        /// <param name="source">A System.Uri object initialized with the URI for the desired content.</param>
        /// <param name="extraData">A System.Object that contains data to be used for processing during navigation.</param>
        /// <param name="infoOverride">Info about the animated transition.</param>
        /// <returns>true if navigation is not canceled; otherwise, false.</returns>
        public bool Navigate(Uri source, object extraData, NavigationTransitionInfo infoOverride)
        {
            _transitionInfoOverride = infoOverride;
            return Navigate(source, extraData);
        }

        /// <summary>
        /// Navigates to the most recent item in back navigation history, if a Frame manages
        /// its own navigation history, and specifies the animated transition to use.
        /// </summary>
        /// <param name="transitionInfoOverride">Info about the animated transition to use.</param>
        public void GoBack(NavigationTransitionInfo transitionInfoOverride)
        {
            _transitionInfoOverride = transitionInfoOverride;
            GoBack();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _oldContentPresenter = GetTemplateChild(FirstContentPresenterName) as ContentPresenter;
            _newContentPresenter = GetTemplateChild(SecondContentPresenterName) as ContentPresenter;

            if (Content != null)
            {
                OnContentChanged(null, Content);
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            StopTransition();

            if (oldContent is Page oldPage)
            {
                _oldPage = oldPage;
            }

            if (_oldContentPresenter is null || _newContentPresenter is null)
            {
                return;
            }

            bool transitioning = false;

            if (Helper.IsAnimationsEnabled &&
                oldContent is FrameworkElement oldElement &&
                newContent is FrameworkElement newElement)
            {
                NavigationTransitionInfo transitionInfo = _transitionInfoOverride ?? DefaultNavigationTransitionInfo;
                _exitAnimation = transitionInfo.GetExitAnimation(oldElement, _movingBackwards);
                _enterAnimation = transitionInfo.GetEnterAnimation(newElement, _movingBackwards);

                if (_exitAnimation != null || _enterAnimation != null)
                {
                    (_newContentPresenter, _oldContentPresenter) = (_oldContentPresenter, _newContentPresenter);

                    _newContentPresenter.Opacity = 0;
                    _newContentPresenter.Visibility = Visibility.Visible;
                    _newContentPresenter.IsHitTestVisible = false;
                    _newContentPresenter.Content = newElement;

                    _oldContentPresenter.Opacity = 1;
                    _oldContentPresenter.Visibility = Visibility.Visible;
                    _oldContentPresenter.IsHitTestVisible = false;
                    _oldContentPresenter.Content = oldElement;

                    BeginTransition();
                    transitioning = true;
                }
            }

            if (!transitioning)
            {
                _oldContentPresenter.Visibility = Visibility.Collapsed;
                _oldContentPresenter.Content = null;

                _newContentPresenter.Visibility = Visibility.Visible;
                _newContentPresenter.Content = Content;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == BackStackProperty)
            {
                OnBackStackPropertyChanged(e);
            }
        }

        private void OnBackStackPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged oldBackStack)
            {
                oldBackStack.CollectionChanged -= OnCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newBackStack)
            {
                newBackStack.CollectionChanged += OnCollectionChanged;
            }

            UpdateBackStackDepth();

            void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                UpdateBackStackDepth();
            }
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (Content is Page newPage)
            {
                newPage.InternalOnNavigatingFrom(e);

                if (e.Cancel)
                {
                    return;
                }
            }

            _movingBackwards = e.NavigationMode == NavigationMode.Back;

            if (_transitionInfoOverride == null)
            {
                if (_movingBackwards && BackEntry is { } backEntry)
                {
                    _transitionInfoOverride = GetNavigationTransitionInfo(backEntry);
                }
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (_transitionInfoOverride != null)
            {
                if (!_movingBackwards && BackEntry is { } backEntry)
                {
                    SetNavigationTransitionInfo(backEntry, _transitionInfoOverride);
                }
                _transitionInfoOverride = null;
            }

            try
            {
                _ignoreSourcePageTypeChanged = true;
                var pageType = e.Content?.GetType();
                SourcePageType = pageType;
                CurrentSourcePageType = pageType;
            }
            finally
            {
                _ignoreSourcePageTypeChanged = false;
            }

            if (_oldPage is { } oldPage)
            {
                _oldPage = null;
                oldPage.InternalOnNavigatedFrom(e);
            }

            if (e.Content is Page newPage)
            {
                newPage.InternalOnNavigatedTo(e);
            }
        }

        private void OnNavigationStopped(object sender, NavigationEventArgs e)
        {
            _transitionInfoOverride = null;
            _oldPage = null;
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            _transitionInfoOverride = null;
            _oldPage = null;
        }

        private void BeginTransition()
        {
            Debug.Assert(_exitAnimation != null || _enterAnimation != null);

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
                else if (_enterAnimation != null)
                {
                    BeginEnterAnimation();
                }
            }, DispatcherPriority.ApplicationIdle);
        }

        private void BeginEnterAnimation()
        {
            if (_oldContentPresenter != null)
            {
                _oldContentPresenter.Visibility = Visibility.Collapsed;
                _oldContentPresenter.Content = null;
            }

            if (_newContentPresenter != null)
            {
                _newContentPresenter.Opacity = 1;
            }

            _enterAnimation.Begin();
        }

        private void OnExitAnimationCompleted(object sender, EventArgs e)
        {
            if (_exitAnimation != null)
            {
                _exitAnimation.Stop();
                _exitAnimation = null;
            }

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
            if (_enterAnimation != null)
            {
                _enterAnimation.Stop();
                _enterAnimation = null;
            }

            StopTransition();
        }

        private void StopTransition()
        {
            if (_asyncBeginTransition != null)
            {
                _asyncBeginTransition.Abort();
                _asyncBeginTransition = null;
            }

            if (_exitAnimation != null)
            {
                _exitAnimation.Stop();
                _exitAnimation = null;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Stop();
                _enterAnimation = null;
            }

            if (_oldContentPresenter != null)
            {
                _oldContentPresenter.Content = null;
                _oldContentPresenter.ClearValue(OpacityProperty);
                _oldContentPresenter.ClearValue(IsHitTestVisibleProperty);
            }

            if (_newContentPresenter != null)
            {
                _newContentPresenter.ClearValue(OpacityProperty);
                _newContentPresenter.ClearValue(IsHitTestVisibleProperty);
            }
        }

        private const string FirstContentPresenterName = "FirstContentPresenter";
        private const string SecondContentPresenterName = "SecondContentPresenter";

        private ContentPresenter _oldContentPresenter;
        private ContentPresenter _newContentPresenter;

        private bool _movingBackwards;
        private bool _ignoreSourcePageTypeChanged;
        private Page _oldPage;
        private NavigationTransitionInfo _transitionInfoOverride;
        private NavigationAnimation _exitAnimation;
        private NavigationAnimation _enterAnimation;
        private DispatcherOperation _asyncBeginTransition;
    }
}
