using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class MediaPlayerElement : Control
    {
        static MediaPlayerElement()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaPlayerElement), new FrameworkPropertyMetadata(typeof(MediaPlayerElement)));
        }

        #region AutoPlay

        /// <summary>
        /// Identifies the AutoPlay dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register(
                nameof(AutoPlay),
                typeof(bool),
                typeof(MediaPlayerElement),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether media will begin playback automatically when the <see cref="Source"/> property is set.
        /// </summary>
        /// <value><paramref name="true"/> if playback is automatic; otherwise, <paramref name="false"/>. The default is <paramref name="false"/>.</value>
        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }

        #endregion

        #region AreTransportControlsEnabled

        /// <summary>
        /// Identifies the AreTransportControlsEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty AreTransportControlsEnabledProperty =
            DependencyProperty.Register(
                nameof(AreTransportControlsEnabled),
                typeof(bool),
                typeof(MediaPlayerElement),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that determines whether the standard transport controls are enabled.
        /// </summary>
        /// <value><paramref name="true"/> if the standard transport controls are enabled; otherwise, <paramref name="false"/>. The default is <paramref name="false"/>.</value>
        public bool AreTransportControlsEnabled
        {
            get => (bool)GetValue(AreTransportControlsEnabledProperty);
            set => SetValue(AreTransportControlsEnabledProperty, value);
        }

        #endregion

        #region MediaPlayer

        /// <summary>
        /// Identifies the MediaPlayer dependency property.
        /// </summary>
        public static readonly DependencyProperty MediaPlayerProperty =
            DependencyProperty.Register(
                nameof(MediaPlayer),
                typeof(MediaElementEx),
                typeof(MediaPlayerElement),
                new PropertyMetadata(OnMediaPlayerPropertyChanged));

        /// <summary>
        /// Gets or sets the MediaPlayer instance used to render media.
        /// </summary>
        public MediaElementEx MediaPlayer
        {
            get => (MediaElementEx)GetValue(MediaPlayerProperty);
            set => SetValue(MediaPlayerProperty, value);
        }

        private static void OnMediaPlayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaPlayerElement)d).UpdateMediaPlayer();
        }

        #endregion

        #region PosterSource

        /// <summary>
        /// Identifies the PosterSource dependency property.
        /// </summary>
        public static readonly DependencyProperty PosterSourceProperty =
            DependencyProperty.Register(
                nameof(PosterSource),
                typeof(ImageSource),
                typeof(MediaPlayerElement),
                null);

        /// <summary>
        /// Gets or sets the image source that is used for a placeholder image during <see cref="MediaPlayerElement"/> loading transition states.
        /// </summary>
        /// <value>An image source for a transition ImageBrush that is applied to the MediaPlayerElement content area.</value>
        public ImageSource PosterSource
        {
            get => (ImageSource)GetValue(PosterSourceProperty);
            set => SetValue(PosterSourceProperty, value);
        }

        #endregion

        #region Source

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(Uri),
                typeof(MediaPlayerElement),
                null);

        /// <summary>
        /// Gets or sets a media source on the <see cref="MediaElementEx"/>.
        /// </summary>
        public Uri Source
        {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        #endregion

        #region Stretch

        /// <summary>
        /// Identifies the Stretch dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                nameof(Stretch),
                typeof(Stretch),
                typeof(MediaPlayerElement),
                new PropertyMetadata(Stretch.Uniform));

        /// <summary>
        /// Gets or sets a value that describes how an <see cref="MediaPlayerElement"/> should be stretched to fill the destination rectangle.
        /// </summary>
        /// <value>A value of the <see cref="System.Windows.Media.Stretch"/> enumeration that specifies how the source visual media is rendered. The default value is <see cref="Stretch.Uniform"/>.</value>
        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        #endregion

        #region TransportControls

        /// <summary>
        /// Identifies the TransportControls dependency property.
        /// </summary>
        public static readonly DependencyProperty TransportControlsProperty =
            DependencyProperty.Register(
                nameof(TransportControls),
                typeof(MediaTransportControls),
                typeof(MediaPlayerElement),
                new PropertyMetadata(OnTransportControlsPropertyChanged));

        /// <summary>
        /// Gets or sets the transport controls for the media.
        /// </summary>
        /// <value>The transport controls for the media.</value>
        public MediaTransportControls TransportControls
        {
            get => (MediaTransportControls)GetValue(TransportControlsProperty);
            set => SetValue(TransportControlsProperty, value);
        }

        private static void OnTransportControlsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaPlayerElement)d).UpdateTransportControls();
        }

        #endregion

        #region UseAcrylic

        public static readonly DependencyProperty UseAcrylicProperty =
            DependencyProperty.Register(
                nameof(UseAcrylic),
                typeof(bool),
                typeof(MediaPlayerElement),
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
                typeof(MediaPlayerElement),
                new PropertyMetadata(true));

        public bool IsOpening
        {
            get => (bool)GetValue(IsOpeningProperty);
            private set => SetValue(IsOpeningProperty, value);
        }

        #endregion

        private void UpdateMediaPlayer()
        {
            var mediaPlayer = MediaPlayer;
            if (mediaPlayer != null)
            {
                SetBinding(IsOpeningProperty, new Binding
                {
                    Source = MediaPlayer,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(mediaPlayer.IsOpening))
                });

                if (mediaPlayer.Source == null)
                {
                    mediaPlayer.SetBinding(MediaElement.SourceProperty, new Binding
                    {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(nameof(Source))
                    });
                }
                mediaPlayer.SetBinding(MediaElement.StretchProperty, new Binding
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(Stretch))
                });
                mediaPlayer.SetBinding(MediaElementEx.AutoPlayProperty, new Binding
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(AutoPlay))
                });
            }
        }

        private void UpdateTransportControls()
        {
            var transportControls = TransportControls;
            if (transportControls != null)
            {
                if (transportControls.Target == null)
                {
                    transportControls.SetBinding(MediaTransportControls.TargetProperty, new Binding
                    {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(nameof(MediaPlayer))
                    });
                }
                transportControls.SetBinding(MediaTransportControls.UseAcrylicProperty, new Binding
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(nameof(UseAcrylic))
                });
            }
        }
    }
}
