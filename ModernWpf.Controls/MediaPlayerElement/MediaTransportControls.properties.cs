using System.Windows;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class MediaTransportControls
    {
        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false, OnIsCompactPropertyChanged));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private static void OnIsCompactPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaTransportControls)d).UpdateState(true);
            ((MediaTransportControls)d).UpdateLayouts();
        }

        #endregion

        #region IsCompactOverlayButtonVisible

        public static readonly DependencyProperty IsCompactOverlayButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsCompactOverlayButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsCompactOverlayButtonVisible
        {
            get => (bool)GetValue(IsCompactOverlayButtonVisibleProperty);
            set => SetValue(IsCompactOverlayButtonVisibleProperty, value);
        }

        #endregion

        #region IsCompactOverlayEnabled

        public static readonly DependencyProperty IsCompactOverlayEnabledProperty =
            DependencyProperty.Register(
                nameof(IsCompactOverlayEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsCompactOverlayEnabled
        {
            get => (bool)GetValue(IsCompactOverlayEnabledProperty);
            set => SetValue(IsCompactOverlayEnabledProperty, value);
        }

        #endregion

        #region IsFastForwardButtonVisible

        public static readonly DependencyProperty IsFastForwardButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsFastForwardButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsFastForwardButtonVisible
        {
            get => (bool)GetValue(IsFastForwardButtonVisibleProperty);
            set => SetValue(IsFastForwardButtonVisibleProperty, value);
        }

        #endregion

        #region IsFastForwardEnabled

        public static readonly DependencyProperty IsFastForwardEnabledProperty =
            DependencyProperty.Register(
                nameof(IsFastForwardEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsFastForwardEnabled
        {
            get => (bool)GetValue(IsFastForwardEnabledProperty);
            set => SetValue(IsFastForwardEnabledProperty, value);
        }

        #endregion

        #region IsFastRewindButtonVisible

        public static readonly DependencyProperty IsFastRewindButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsFastRewindButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsFastRewindButtonVisible
        {
            get => (bool)GetValue(IsFastRewindButtonVisibleProperty);
            set => SetValue(IsFastRewindButtonVisibleProperty, value);
        }

        #endregion

        #region IsFastRewindEnabled

        public static readonly DependencyProperty IsFastRewindEnabledProperty =
            DependencyProperty.Register(
                nameof(IsFastRewindEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsFastRewindEnabled
        {
            get => (bool)GetValue(IsFastRewindEnabledProperty);
            set => SetValue(IsFastRewindEnabledProperty, value);
        }

        #endregion

        #region IsFullWindowButtonVisible

        public static readonly DependencyProperty IsFullWindowButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsFullWindowButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsFullWindowButtonVisible
        {
            get => (bool)GetValue(IsFullWindowButtonVisibleProperty);
            set => SetValue(IsFullWindowButtonVisibleProperty, value);
        }

        #endregion

        #region IsFullWindowEnabled

        public static readonly DependencyProperty IsFullWindowEnabledProperty =
            DependencyProperty.Register(
                nameof(IsFullWindowEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsFullWindowEnabled
        {
            get => (bool)GetValue(IsFullWindowEnabledProperty);
            set => SetValue(IsFullWindowEnabledProperty, value);
        }

        #endregion

        #region IsNextTrackButtonVisible

        public static readonly DependencyProperty IsNextTrackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsNextTrackButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsNextTrackButtonVisible
        {
            get => (bool)GetValue(IsNextTrackButtonVisibleProperty);
            set => SetValue(IsNextTrackButtonVisibleProperty, value);
        }

        #endregion

        #region IsPlaybackRateButtonVisible

        public static readonly DependencyProperty IsPlaybackRateButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPlaybackRateButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsPlaybackRateButtonVisible
        {
            get => (bool)GetValue(IsPlaybackRateButtonVisibleProperty);
            set => SetValue(IsPlaybackRateButtonVisibleProperty, value);
        }

        #endregion

        #region IsPlaybackRateEnabled

        public static readonly DependencyProperty IsPlaybackRateEnabledProperty =
            DependencyProperty.Register(
                nameof(IsPlaybackRateEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsPlaybackRateEnabled
        {
            get => (bool)GetValue(IsPlaybackRateEnabledProperty);
            set => SetValue(IsPlaybackRateEnabledProperty, value);
        }

        #endregion

        #region IsPreviousTrackButtonVisible

        public static readonly DependencyProperty IsPreviousTrackButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsPreviousTrackButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsPreviousTrackButtonVisible
        {
            get => (bool)GetValue(IsPreviousTrackButtonVisibleProperty);
            set => SetValue(IsPreviousTrackButtonVisibleProperty, value);
        }

        #endregion

        #region IsRepeatButtonVisible

        public static readonly DependencyProperty IsRepeatButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsRepeatButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsRepeatButtonVisible
        {
            get => (bool)GetValue(IsRepeatButtonVisibleProperty);
            set => SetValue(IsRepeatButtonVisibleProperty, value);
        }

        #endregion

        #region IsRepeatEnabled

        public static readonly DependencyProperty IsRepeatEnabledProperty =
            DependencyProperty.Register(
                nameof(IsRepeatEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsRepeatEnabled
        {
            get => (bool)GetValue(IsRepeatEnabledProperty);
            set => SetValue(IsRepeatEnabledProperty, value);
        }

        #endregion

        #region IsSeekBarVisible

        public static readonly DependencyProperty IsSeekBarVisibleProperty =
            DependencyProperty.Register(
                nameof(IsSeekBarVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsSeekBarVisible
        {
            get => (bool)GetValue(IsSeekBarVisibleProperty);
            set => SetValue(IsSeekBarVisibleProperty, value);
        }

        #endregion

        #region IsSeekEnabled

        public static readonly DependencyProperty IsSeekEnabledProperty =
            DependencyProperty.Register(
                nameof(IsSeekEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsSeekEnabled
        {
            get => (bool)GetValue(IsSeekEnabledProperty);
            set => SetValue(IsSeekEnabledProperty, value);
        }

        #endregion

        #region IsSkipBackwardButtonVisible

        public static readonly DependencyProperty IsSkipBackwardButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsSkipBackwardButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsSkipBackwardButtonVisible
        {
            get => (bool)GetValue(IsSkipBackwardButtonVisibleProperty);
            set => SetValue(IsSkipBackwardButtonVisibleProperty, value);
        }

        #endregion

        #region IsSkipBackwardEnabled

        public static readonly DependencyProperty IsSkipBackwardEnabledProperty =
            DependencyProperty.Register(
                nameof(IsSkipBackwardEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsSkipBackwardEnabled
        {
            get => (bool)GetValue(IsSkipBackwardEnabledProperty);
            set => SetValue(IsSkipBackwardEnabledProperty, value);
        }

        #endregion

        #region IsSkipForwardButtonVisible

        public static readonly DependencyProperty IsSkipForwardButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsSkipForwardButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsSkipForwardButtonVisible
        {
            get => (bool)GetValue(IsSkipForwardButtonVisibleProperty);
            set => SetValue(IsSkipForwardButtonVisibleProperty, value);
        }

        #endregion

        #region IsSkipForwardEnabled

        public static readonly DependencyProperty IsSkipForwardEnabledProperty =
            DependencyProperty.Register(
                nameof(IsSkipForwardEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsSkipForwardEnabled
        {
            get => (bool)GetValue(IsSkipForwardEnabledProperty);
            set => SetValue(IsSkipForwardEnabledProperty, value);
        }

        #endregion

        #region IsStopButtonVisible

        public static readonly DependencyProperty IsStopButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsStopButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsStopButtonVisible
        {
            get => (bool)GetValue(IsStopButtonVisibleProperty);
            set => SetValue(IsStopButtonVisibleProperty, value);
        }

        #endregion

        #region IsStopEnabled

        public static readonly DependencyProperty IsStopEnabledProperty =
            DependencyProperty.Register(
                nameof(IsStopEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool IsStopEnabled
        {
            get => (bool)GetValue(IsStopEnabledProperty);
            set => SetValue(IsStopEnabledProperty, value);
        }

        #endregion

        #region IsVolumeButtonVisible

        public static readonly DependencyProperty IsVolumeButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsVolumeButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsVolumeButtonVisible
        {
            get => (bool)GetValue(IsVolumeButtonVisibleProperty);
            set => SetValue(IsVolumeButtonVisibleProperty, value);
        }

        #endregion

        #region IsVolumeEnabled

        public static readonly DependencyProperty IsVolumeEnabledProperty =
            DependencyProperty.Register(
                nameof(IsVolumeEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsVolumeEnabled
        {
            get => (bool)GetValue(IsVolumeEnabledProperty);
            set => SetValue(IsVolumeEnabledProperty, value);
        }

        #endregion

        #region IsZoomButtonVisible

        public static readonly DependencyProperty IsZoomButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsZoomButtonVisible),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsZoomButtonVisible
        {
            get => (bool)GetValue(IsZoomButtonVisibleProperty);
            set => SetValue(IsZoomButtonVisibleProperty, value);
        }

        #endregion

        #region IsZoomEnabled

        public static readonly DependencyProperty IsZoomEnabledProperty =
            DependencyProperty.Register(
                nameof(IsZoomEnabled),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsZoomEnabled
        {
            get => (bool)GetValue(IsZoomEnabledProperty);
            set => SetValue(IsZoomEnabledProperty, value);
        }

        #endregion

        #region ShowAndHideAutomatically

        public static readonly DependencyProperty ShowAndHideAutomaticallyProperty =
            DependencyProperty.Register(
                nameof(ShowAndHideAutomatically),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool ShowAndHideAutomatically
        {
            get => (bool)GetValue(ShowAndHideAutomaticallyProperty);
            set => SetValue(ShowAndHideAutomaticallyProperty, value);
        }

        #endregion

        #region Target

        /// <summary>
        /// Identifies the <see cref="Target"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(MediaElementEx),
                typeof(MediaTransportControls),
                new PropertyMetadata(OnTargetPropertyChanged));

        public MediaElementEx Target
        {
            get => (MediaElementEx)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaTransportControls)d).UpdateTarget();
        }

        #endregion

        #region HasTarget

        public static readonly DependencyProperty HasTargetProperty =
            DependencyProperty.Register(
                nameof(HasTarget),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(false));

        public bool HasTarget
        {
            get => (bool)GetValue(HasTargetProperty);
            private set => SetValue(HasTargetProperty, value);
        }

        #endregion

        #region UseAcrylic

        public static readonly DependencyProperty UseAcrylicProperty =
            DependencyProperty.Register(
                nameof(UseAcrylic),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool UseAcrylic
        {
            get => (bool)GetValue(UseAcrylicProperty);
            set => SetValue(UseAcrylicProperty, value);
        }

        #endregion

        #region IsOpening

        public static readonly DependencyProperty IsOpeningProperty =
            DependencyProperty.Register(
                nameof(IsOpening),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsOpening
        {
            get => (bool)GetValue(IsOpeningProperty);
            private set => SetValue(IsOpeningProperty, value);
        }

        #endregion

        #region IsBuffering

        public static readonly DependencyProperty IsBufferingProperty =
            DependencyProperty.Register(
                nameof(IsBuffering),
                typeof(bool),
                typeof(MediaTransportControls),
                new PropertyMetadata(true));

        public bool IsBuffering
        {
            get => (bool)GetValue(IsBufferingProperty);
            private set => SetValue(IsBufferingProperty, value);
        }

        #endregion

        #region UseSystemFocusVisuals

        /// <summary>
        /// Identifies the UseSystemFocusVisuals dependency property.
        /// </summary>
        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(MediaTransportControls));

        /// <summary>
        /// Gets or sets a value that indicates whether the control uses focus visuals that
        /// are drawn by the system or those defined in the control template.
        /// </summary>
        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        /// <summary>
        /// Identifies the FocusVisualMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(MediaTransportControls));

        /// <summary>
        /// Gets or sets the outer margin of the focus visual for a FrameworkElement.
        /// </summary>
        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(MediaTransportControls));

        /// <summary>
        /// Gets or sets the radius for the corners of the control's border.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(MediaTransportControlsTemplateSettings),
                typeof(MediaTransportControls),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public MediaTransportControlsTemplateSettings TemplateSettings
        {
            get => (MediaTransportControlsTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsPropertyKey, value);
        }

        #endregion
    }
}
