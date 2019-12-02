// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Mirrors the
    /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>
    /// interface to control an
    /// <see cref="T:ModernWpf.Controls.ITransition"/>
    /// for a
    /// <see cref="T:System.Windows.UIElement"/>.
    /// Saves and restores the
    /// <see cref="P:System.Windows.UIElement.CacheMode"/>
    /// and
    /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
    /// values for the
    /// <see cref="T:System.Windows.UIElement"/>.
    /// </summary>
    public class Transition : ITransition
    {
        /// <summary>
        /// The original
        /// <see cref="P:System.Windows.UIElement.CacheMode"/>
        /// of the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private CacheMode _cacheMode;

        /// <summary>
        /// The <see cref="T:System.Windows.UIElement"/>
        /// that the transition will be applied to.
        /// </summary>
        private UIElement _element;

        /// <summary>
        /// The original
        /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
        /// of the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private bool _isHitTestVisible;

        /// <summary>
        /// The
        /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>
        /// for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private Storyboard _storyboard;

        private ClockState _currentState = ClockState.Stopped;

        /// <summary>
        /// The property that identifies the
        /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>
        /// for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        protected internal Storyboard Storyboard
        {
            get { return _storyboard; }
            set
            {
                if (value != _storyboard)
                {
                    if (_storyboard != null)
                    {
                        _storyboard.CurrentStateInvalidated -= OnCurrentStateInvalidated;
                        _storyboard.Completed -= OnCompleted;
                    }

                    _storyboard = value;
                    _currentState = ClockState.Stopped;

                    if (_storyboard != null)
                    {
                        _storyboard.CurrentStateInvalidated += OnCurrentStateInvalidated;
                        _storyboard.Completed += OnCompleted;
                    }
                }
            }
        }

        private FrameworkElement ElementAsFE => _element as FrameworkElement;

        /// <summary>
        /// Mirrors <see cref="E:System.Windows.Media.Animation.Storyboard.Completed"/>.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Constructs a
        /// <see cref="T:ModernWpf.Controls.Transition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// and a
        /// <see cref="T:System.Windows.Media.Animation.Storyboard"/>.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="storyboard">The <see cref="T:System.Windows.Media.Animation.Storyboard"/>.</param>
        public Transition(UIElement element, Storyboard storyboard)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (storyboard == null)
            {
                throw new ArgumentNullException("storyboard");
            }
            _element = element;
            Storyboard = storyboard;
            Storyboard.Completed += OnCompletedRestore;
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Begin"/>.
        /// </summary>
        public virtual void Begin()
        {
            Save();
            Storyboard.Begin(ElementAsFE, true);
        }

        /// <summary>
        /// Restores the settings for the transition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCompletedRestore(object sender, EventArgs e)
        {
            Restore();
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.GetCurrentState"/>.
        /// </summary>
        public ClockState GetCurrentState()
        {
            return Storyboard.GetCurrentState(ElementAsFE);
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.GetCurrentTime"/>.
        /// </summary>
        public TimeSpan? GetCurrentTime()
        {
            return Storyboard.GetCurrentTime(ElementAsFE);
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Pause"/>.
        /// </summary>
        public void Pause()
        {
            Storyboard.Pause(ElementAsFE);
        }

        /// <summary>
        /// Restores the saved
        /// <see cref="P:System.Windows.UIElement.CacheMode"/>
        /// and
        /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
        /// values for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private void Restore()
        {
            if (!(_cacheMode is BitmapCache))
            {
                _element.CacheMode = _cacheMode;
            }
            if (_isHitTestVisible)
            {
                _element.IsHitTestVisible = _isHitTestVisible;
            }
            else
            {
                // This is resolving a bug where the new page cannot be used.
                // This may regress some scenarios for unsupported uses of the
                // transitions.
                _element.IsHitTestVisible = true;
            }
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Resume"/>.
        /// </summary>
        public void Resume()
        {
            Storyboard.Resume(ElementAsFE);
        }

        /// <summary>
        /// Saves the
        /// <see cref="P:System.Windows.UIElement.CacheMode"/>
        /// and
        /// <see cref="P:System.Windows.UIElement.IsHitTestVisible"/>
        /// values for the
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        private void Save()
        {
            _cacheMode = _element.CacheMode;
            if (!(_cacheMode is BitmapCache))
            {
                _element.CacheMode = TransitionFrame.BitmapCacheMode;
            }
            _isHitTestVisible = _element.IsHitTestVisible;
            if (_isHitTestVisible)
            {
                _element.IsHitTestVisible = false;
            }
        }

        private void OnCurrentStateInvalidated(object sender, EventArgs e)
        {
            var clock = (Clock)sender;
            _currentState = clock.CurrentState;
        }

        /// <summary>
        /// Mirrors <see cref="E:System.Windows.Media.Animation.Storyboard.Completed"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCompleted(object sender, EventArgs e)
        {
            Completed?.Invoke(this, e);
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Seek"/>.
        /// </summary>
        /// <param name="offset">The time offset.</param>
        public void Seek(TimeSpan offset)
        {
            Storyboard.Seek(ElementAsFE, offset, TimeSeekOrigin.BeginTime);
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.SeekAlignedToLastTick"/>.
        /// </summary>
        /// <param name="offset">The time offset.</param>
        public void SeekAlignedToLastTick(TimeSpan offset)
        {
            Storyboard.SeekAlignedToLastTick(ElementAsFE, offset, TimeSeekOrigin.BeginTime);
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.SkipToFill"/>.
        /// </summary>
        public void SkipToFill()
        {
            Storyboard.SkipToFill(ElementAsFE);
        }

        /// <summary>
        /// Mirrors <see cref="M:System.Windows.Media.Animation.Storyboard.Stop"/>.
        /// </summary>
        public void Stop()
        {
            if (_currentState != ClockState.Stopped)
            {
                Storyboard.Stop(ElementAsFE);
            }

            Restore();
        }
    }
}
