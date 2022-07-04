// Ported from https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/TransitioningContentControl.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// A ContentControl that animates content as it loads and unloads.
    /// </summary>
    [TemplatePart(Name = PreviousContentPresentationSitePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = CurrentContentPresentationSitePartName, Type = typeof(ContentPresenter))]
    public class TransitioningContentControl : ContentControl
    {
        internal const string PresentationGroup = "PresentationStates";
        internal const string HiddenState = "Hidden";
        internal const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";
        internal const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";

        private ContentPresenter currentContentPresentationSite;
        private ContentPresenter previousContentPresentationSite;
        private bool allowIsTransitioningPropertyWrite;
        private Storyboard currentTransition;

        public event RoutedEventHandler TransitionCompleted;

        public const TransitionType DefaultTransitionState = TransitionType.Default;

        public static readonly DependencyProperty IsTransitioningProperty
            = DependencyProperty.Register(
                nameof(IsTransitioning),
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(false, OnIsTransitioningPropertyChanged));

        /// <summary>
        /// Gets whether if the content is transitioning.
        /// </summary>
        public bool IsTransitioning
        {
            get => (bool)GetValue(IsTransitioningProperty);
            private set
            {
                allowIsTransitioningPropertyWrite = true;
                try
                {
                    SetValue(IsTransitioningProperty, value);
                }
                finally
                {
                    allowIsTransitioningPropertyWrite = false;
                }
            }
        }

        public static readonly DependencyProperty TransitionProperty
            = DependencyProperty.Register(
                nameof(Transition),
                typeof(TransitionType),
                typeof(TransitioningContentControl),
                new FrameworkPropertyMetadata(TransitionType.Default, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits, OnTransitionPropertyChanged));

        /// <summary>
        /// Gets or sets the transition type.
        /// </summary>
        public TransitionType Transition
        {
            get => (TransitionType)GetValue(TransitionProperty);
            set => SetValue(TransitionProperty, value);
        }

        public static readonly DependencyProperty RestartTransitionOnContentChangeProperty
            = DependencyProperty.Register(
                nameof(RestartTransitionOnContentChange),
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(false, OnRestartTransitionOnContentChangePropertyChanged));

        /// <summary>
        /// Gets or sets whether if the transition should restart after the content change.
        /// </summary>
        public bool RestartTransitionOnContentChange
        {
            get => (bool)GetValue(RestartTransitionOnContentChangeProperty);
            set => SetValue(RestartTransitionOnContentChangeProperty, value);
        }

        public static readonly DependencyProperty CustomVisualStatesProperty
            = DependencyProperty.Register(
                nameof(CustomVisualStates),
                typeof(ObservableCollection<VisualState>),
                typeof(TransitioningContentControl),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets customized visual states to use as transition.
        /// </summary>
        public ObservableCollection<VisualState> CustomVisualStates
        {
            get => (ObservableCollection<VisualState>)GetValue(CustomVisualStatesProperty);
            set => SetValue(CustomVisualStatesProperty, value);
        }

        public static readonly DependencyProperty CustomVisualStatesNameProperty
            = DependencyProperty.Register(
                nameof(CustomVisualStatesName),
                typeof(string),
                typeof(TransitioningContentControl),
                new PropertyMetadata("CustomTransition"));

        /// <summary>
        /// Gets or sets the name of a custom transition visual state.
        /// </summary>
        public string CustomVisualStatesName
        {
            get => (string)GetValue(CustomVisualStatesNameProperty);
            set => SetValue(CustomVisualStatesNameProperty, value);
        }

        internal Storyboard CurrentTransition
        {
            get => currentTransition;
            set
            {
                // decouple event
                if (currentTransition != null)
                {
                    currentTransition.Completed -= OnTransitionCompleted;
                }

                currentTransition = value;

                if (currentTransition != null)
                {
                    currentTransition.Completed += OnTransitionCompleted;
                }
            }
        }

        private static void OnIsTransitioningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl)d;

            if (!source.allowIsTransitioningPropertyWrite)
            {
                source.IsTransitioning = (bool)e.OldValue;
                throw new InvalidOperationException();
            }
        }

        private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl)d;
            var oldTransition = (TransitionType)e.OldValue;
            var newTransition = (TransitionType)e.NewValue;

            if (source.IsTransitioning)
            {
                source.AbortTransition();
            }

            // find new transition
            var newStoryboard = source.GetStoryboard(newTransition);

            // unable to find the transition.
            if (newStoryboard is null)
            {
                // could be during initialization of xaml that presentationgroups was not yet defined
                if (VisualStates.TryGetVisualStateGroup(source, PresentationGroup) is null)
                {
                    // will delay check
                    source.CurrentTransition = null;
                }
                else
                {
                    // revert to old value
                    source.SetValue(TransitionProperty, oldTransition);

                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Temporary removed exception message", newTransition));
                }
            }
            else
            {
                source.CurrentTransition = newStoryboard;
            }
        }

        private static void OnRestartTransitionOnContentChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitioningContentControl)d).OnRestartTransitionOnContentChangeChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue)
        {
        }

        public TransitioningContentControl()
        {
            CustomVisualStates = new ObservableCollection<VisualState>();
            DefaultStyleKey = typeof(TransitioningContentControl);
        }

        public override void OnApplyTemplate()
        {
            if (IsTransitioning)
            {
                AbortTransition();
            }

            if (CustomVisualStates != null && CustomVisualStates.Any())
            {
                var presentationGroup = VisualStates.TryGetVisualStateGroup(this, PresentationGroup);
                if (presentationGroup != null)
                {
                    foreach (var state in CustomVisualStates)
                    {
                        presentationGroup.States.Add(state);
                    }
                }
            }

            base.OnApplyTemplate();

            previousContentPresentationSite = GetTemplateChild(PreviousContentPresentationSitePartName) as ContentPresenter;
            currentContentPresentationSite = GetTemplateChild(CurrentContentPresentationSitePartName) as ContentPresenter;

            // hookup currenttransition
            var transition = GetStoryboard(Transition);
            CurrentTransition = transition;
            if (transition is null)
            {
                var invalidTransition = Transition;
                // revert to default
                Transition = DefaultTransitionState;

                throw new ArgumentException($"'{invalidTransition}' transition could not be found!");
            }

            VisualStateManager.GoToState(this, HiddenState, false);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (oldContent != newContent)
            {
                StartTransition(oldContent, newContent);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newContent", Justification = "Should be used in the future.")]
        private void StartTransition(object oldContent, object newContent)
        {
            // both presenters must be available, otherwise a transition is useless.
            if (currentContentPresentationSite != null && previousContentPresentationSite != null)
            {
                if (RestartTransitionOnContentChange
                    && CurrentTransition is not null)
                {
                    CurrentTransition.Completed -= OnTransitionCompleted;
                }

                currentContentPresentationSite.SetCurrentValue(ContentPresenter.ContentProperty, newContent);
                previousContentPresentationSite.SetCurrentValue(ContentPresenter.ContentProperty, oldContent);

                // and start a new transition
                if (!IsTransitioning || RestartTransitionOnContentChange)
                {
                    if (RestartTransitionOnContentChange
                        && CurrentTransition is not null)
                    {
                        CurrentTransition.Completed += OnTransitionCompleted;
                    }

                    IsTransitioning = true;
                    VisualStateManager.GoToState(this, HiddenState, false);
                    VisualStateManager.GoToState(this, GetTransitionName(Transition), true);
                }
            }
        }

        /// <summary>
        /// Reload the current transition if the content is the same.
        /// </summary>
        public void ReloadTransition()
        {
            // both presenters must be available, otherwise a transition is useless.
            if (currentContentPresentationSite != null && previousContentPresentationSite != null)
            {
                if (RestartTransitionOnContentChange
                    && CurrentTransition is not null)
                {
                    CurrentTransition.Completed -= OnTransitionCompleted;
                }

                if (!IsTransitioning || RestartTransitionOnContentChange)
                {
                    if (RestartTransitionOnContentChange
                        && CurrentTransition is not null)
                    {
                        CurrentTransition.Completed += OnTransitionCompleted;
                    }

                    IsTransitioning = true;
                    VisualStateManager.GoToState(this, HiddenState, false);
                    VisualStateManager.GoToState(this, GetTransitionName(Transition), true);
                }
            }
        }

        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            AbortTransition();
            var clockGroup = sender as ClockGroup;
            if (clockGroup is null || clockGroup.CurrentState == ClockState.Stopped)
            {
                TransitionCompleted?.Invoke(this, new RoutedEventArgs());
            }
        }

        public void AbortTransition()
        {
            // go to normal state and release our hold on the old content.
            VisualStateManager.GoToState(this, HiddenState, false);
            IsTransitioning = false;
            previousContentPresentationSite?.SetCurrentValue(ContentPresenter.ContentProperty, null);
        }

        private Storyboard GetStoryboard(TransitionType newTransition)
        {
            var presentationGroup = VisualStates.TryGetVisualStateGroup(this, PresentationGroup);
            if (presentationGroup != null)
            {
                var transitionName = GetTransitionName(newTransition);
                return presentationGroup.States
                                        .OfType<VisualState>()
                                        .Where(state => state.Name == transitionName)
                                        .Select(state => state.Storyboard)
                                        .FirstOrDefault();
            }

            return null;
        }

        private string GetTransitionName(TransitionType transition)
        {
            return transition switch
            {
                TransitionType.Default => "DefaultTransition",
                TransitionType.Normal => "Normal",
                TransitionType.Up => "UpTransition",
                TransitionType.Down => "DownTransition",
                TransitionType.Right => "RightTransition",
                TransitionType.RightReplace => "RightReplaceTransition",
                TransitionType.Left => "LeftTransition",
                TransitionType.LeftReplace => "LeftReplaceTransition",
                TransitionType.Custom => CustomVisualStatesName,
                _ => "DefaultTransition"
            };
        }
    }
}
