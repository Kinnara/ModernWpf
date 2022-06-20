using ModernWpf.Controls.Primitives;
using ModernWpf.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents the playback controls for a media player element.
    /// </summary>
    public partial class MediaTransportControls : Control
    {
        private DispatcherTimer _timer;

        private FrameworkElement ControlPanelGrid;

        private ButtonBase PlayPauseButtonOnLeft;
        private ButtonBase AudioMuteButton;
        private ButtonBase VolumeMuteButton;
        private ButtonBase StopButton;
        private ButtonBase SkipBackwardButton;
        private ButtonBase PreviousTrackButton;
        private ButtonBase RewindButton;
        private ButtonBase PlayPauseButton;
        private ButtonBase FastForwardButton;
        private ButtonBase NextTrackButton;
        private ButtonBase SkipForwardButton;
        private ButtonBase PlaybackRateButton;
        private ButtonBase RepeatButton;
        private ButtonBase ZoomButton;
        private ButtonBase CompactOverlayButton;
        private ButtonBase FullWindowButton;

        // Visual States
        private const string ControlPanelFadeInStateName = "ControlPanelFadeIn";
        private const string ControlPanelFadeOutStateName = "ControlPanelFadeOut";
        private const string NormalModeStateName = "NormalMode";
        private const string CompactModeStateName = "CompactMode";
        private const string PlayStateStateName = "PlayState";
        private const string PauseStateStateName = "PauseState";
        private const string VolumeStateStateName = "VolumeState";
        private const string MuteStateStateName = "MuteState";
        private const string NonFullWindowStateName = "NonFullWindowState";
        private const string FullWindowStateName = "FullWindowState";
        private const string RepeatOneStateName = "RepeatOneState";
        private const string RepeatAllStateName = "RepeatAllState";

        static MediaTransportControls()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaTransportControls), new FrameworkPropertyMetadata(typeof(MediaTransportControls)));
        }

        public MediaTransportControls()
        {
            TemplateSettings = new MediaTransportControlsTemplateSettings();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_timer.IsEnabled && !(ControlPanelGrid != null && ControlPanelGrid.IsMouseOver))
            {
                _timer.Stop();
                Hide();
            }
        }

        private void UpdateTimer()
        {
            Show();
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        public override void OnApplyTemplate()
        {
            if (ShowAndHideAutomatically)
            {
                _timer = new DispatcherTimer();
                MouseMove += (sender, e) => UpdateTimer();
                TouchMove += (sender, e) => UpdateTimer();
                StylusMove += (sender, e) => UpdateTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(3000);
                _timer.Tick += timer_Tick;
            }

            base.OnApplyTemplate();

            if (PlayPauseButtonOnLeft != null)
            {
                PlayPauseButtonOnLeft.Click -= PlayPause_Click;
            }
            if (AudioMuteButton != null)
            {
                AudioMuteButton.Click -= Mute_Click;
            }
            if (PlayPauseButton != null)
            {
                PlayPauseButton.Click -= PlayPause_Click;
            }

            ControlPanelGrid = (FrameworkElement)GetTemplateChild(nameof(ControlPanelGrid));

            PlayPauseButtonOnLeft = (ButtonBase)GetTemplateChild(nameof(PlayPauseButtonOnLeft));
            AudioMuteButton = (ButtonBase)GetTemplateChild(nameof(AudioMuteButton));
            VolumeMuteButton = (ButtonBase)GetTemplateChild(nameof(VolumeMuteButton));
            StopButton = (ButtonBase)GetTemplateChild(nameof(StopButton));
            SkipBackwardButton = (ButtonBase)GetTemplateChild(nameof(SkipBackwardButton));
            PlayPauseButton = (ButtonBase)GetTemplateChild(nameof(PlayPauseButton));
            SkipForwardButton = (ButtonBase)GetTemplateChild(nameof(SkipForwardButton));
            PlaybackRateButton = (ButtonBase)GetTemplateChild(nameof(PlaybackRateButton));
            RepeatButton = (ButtonBase)GetTemplateChild(nameof(RepeatButton));
            ZoomButton = (ButtonBase)GetTemplateChild(nameof(ZoomButton));
            CompactOverlayButton = (ButtonBase)GetTemplateChild(nameof(CompactOverlayButton));
            FullWindowButton = (ButtonBase)GetTemplateChild(nameof(FullWindowButton));

            if (PlayPauseButtonOnLeft != null)
            {
                PlayPauseButtonOnLeft.Click += PlayPause_Click;
            }
            if (AudioMuteButton != null)
            {
                AudioMuteButton.Click += Mute_Click;
            }
            if (StopButton != null)
            {
                StopButton.Click += Stop_Click;
            }
            if (SkipBackwardButton != null)
            {
                SkipBackwardButton.Click += SkipBackward_Click;
            }
            if (PlayPauseButton != null)
            {
                PlayPauseButton.Click += PlayPause_Click;
            }
            if (SkipForwardButton != null)
            {
                SkipForwardButton.Click += SkipForward_Click;
            }
            if (PlaybackRateButton != null)
            {
                PlaybackRateButton.Click += PlaybackRate_Click;
            }
            if (ZoomButton != null)
            {
                ZoomButton.Click += Zoom_Click;
            }
            if (ZoomButton != null)
            {
                CompactOverlayButton.Click += CompactOverlay_Click;
            }

            VisualStateManager.GoToState(this, ControlPanelFadeInStateName, false);
        }

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                mediaElement.IsMuted = !mediaElement.IsMuted;
                UpdateState(true);
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                mediaElement.Stop();
            }
        }

        private void SkipBackward_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(10);
                mediaElement.StartTimer();
                UpdateState(true);
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                if (mediaElement.CurrentState != MediaState.Play)
                {
                    if (mediaElement.LeftTime == 0)
                    {
                        mediaElement.Position = TimeSpan.FromMilliseconds(0);
                    }
                    mediaElement.Play();
                }
                else
                {
                    mediaElement.Pause();
                }
            }
        }

        private void SkipForward_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(30);
                UpdateState(true);
            }
        }

        private void PlaybackRate_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Play();
            }
        }

        private void Zoom_Click(object sender, RoutedEventArgs e)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                switch(mediaElement.Stretch)
                {
                    case Stretch.None:
                        mediaElement.Stretch = Stretch.Fill;
                        break;
                    case Stretch.Fill:
                        mediaElement.Stretch = Stretch.Uniform;
                        break;
                    case Stretch.Uniform:
                        mediaElement.Stretch = Stretch.UniformToFill;
                        break;
                    case Stretch.UniformToFill:
                    default:
                        mediaElement.Stretch = Stretch.None;
                        break;
                }
            }
        }

        private void CompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            IsCompact = !IsCompact;
        }

        private void UpdateState(bool useTransitions = false)
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                string state = mediaElement.CurrentState != MediaState.Play ? PauseStateStateName : PlayStateStateName;
                VisualStateManager.GoToState(this, state, useTransitions);
                state = mediaElement.IsMuted ? MuteStateStateName : VolumeStateStateName;
                VisualStateManager.GoToState(this, state, useTransitions);
                state = IsCompact ? CompactModeStateName : NormalModeStateName;
                VisualStateManager.GoToState(this, state, useTransitions);
            }
        }

        private void UpdateTarget()
        {
            var mediaElement = Target;
            if (mediaElement != null)
            {
                var templateSettings = TemplateSettings;
                HasTarget = true;
                templateSettings.AcrylicBrush = new AcrylicBrushExtension { Target = Target, NoiseOpacity = 0.01 }.CreatAcrylicBrush();

                var isOpeningBinding = new MultiBinding
                {
                    Converter = new OrConverter()
                };

                isOpeningBinding.Bindings.Add(new Binding
                {
                    Source = mediaElement,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(mediaElement.IsOpening))
                });

                isOpeningBinding.Bindings.Add(new Binding
                {
                    Source = mediaElement,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(mediaElement.IsBuffering))
                });

                SetBinding(IsBufferingProperty, isOpeningBinding);
                SetBinding(IsOpeningProperty, new Binding
                {
                    Source = mediaElement,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(mediaElement.IsOpening))
                });

                mediaElement.MediaPlay += (sender, e) => UpdateState(true);
                mediaElement.MediaPause += (sender, e) => UpdateState(true);
                mediaElement.MediaEnded += (sender, e) => UpdateState(true);
                mediaElement.MediaFailed += (sender, e) => UpdateState(true);
                mediaElement.MediaOpened += (sender, e) => UpdateState(true);

                UpdateState(false);
            }
        }

        /// <summary>
        /// Hides the transport controls if they're shown.
        /// </summary>
        public void Hide() => VisualStateManager.GoToState(this, ControlPanelFadeOutStateName, true);

        /// <summary>
        /// Shows the tranport controls if they're hidden.
        /// </summary>
        public void Show() => VisualStateManager.GoToState(this, ControlPanelFadeInStateName, true);
    }
}
