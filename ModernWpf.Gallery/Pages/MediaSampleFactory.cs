using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class MediaSampleFactory
    {
        private const string PersonPictureBasicXaml =
@"<PersonPicture x:Name=""personPicture"" Height=""300"" VerticalAlignment=""Top"" />

<RadioButtons SelectedIndex=""0"" Header=""Profile type"" SelectionChanged=""RadioButtons_SelectionChanged"">
    <RadioButton x:Name=""ProfileImageRadio"" Content=""Profile Image"" IsChecked=""True""/>
    <RadioButton x:Name=""DisplayNameRadio"" Content=""Display Name"" />
    <RadioButton x:Name=""InitialsRadio"" Content=""Initials"" />
</RadioButtons>

<PersonPicture $(ProfilePicture)$(DisplayName)$(Initials) />";

        private const string PersonPictureBasicCSharp =
@"private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (ProfileImageRadio.IsChecked == true)
    {
        personPicture.ProfilePicture = new BitmapImage(new Uri(""https://learn.microsoft.com/windows/uwp/contacts-and-calendar/images/shoulder-tap-static-payload.png""));
        personPicture.DisplayName = null;
        personPicture.Initials = null;
    }
    else if (DisplayNameRadio.IsChecked == true)
    {
        personPicture.ProfilePicture = null;
        personPicture.DisplayName = ""Jane Doe"";
        personPicture.Initials = null;
    }
    else if (InitialsRadio.IsChecked == true)
    {
        personPicture.ProfilePicture = null;
        personPicture.DisplayName = null;
        personPicture.Initials = ""SB"";
    }
}";

        private const string SoundToggleCSharp =
@"ElementSoundPlayer.State = ElementSoundPlayerState.Off;
ElementSoundPlayer.State = ElementSoundPlayerState.On;";

        private const string SoundSpatialAudioCSharp =
@"ElementSoundPlayer.State = ElementSoundPlayerState.On;
ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On";

        private const string SoundSpecificSystemSoundCSharp =
@"ElementSoundPlayer.State = ElementSoundPlayerState.On;

ElementSoundPlayer.Play(ElementSoundKind.Focus);
ElementSoundPlayer.Play(ElementSoundKind.Invoke);
ElementSoundPlayer.Play(ElementSoundKind.Show);
ElementSoundPlayer.Play(ElementSoundKind.Hide);
ElementSoundPlayer.Play(ElementSoundKind.MovePrevious);
ElementSoundPlayer.Play(ElementSoundKind.MoveNext);
ElementSoundPlayer.Play(ElementSoundKind.GoBack);";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AnimatedVisualPlayer":
                    return CreateAnimatedVisualPlayerSample();
                case "CaptureElementPreview":
                    return CreateCaptureElementPreviewSample();
                case "Image":
                    return CreateImageSample();
                case "MapControl":
                    return CreateMapControlSample();
                case "MediaPlayerElement":
                    return CreateMediaPlayerElementSample();
                case "PersonPicture":
                    return CreatePersonPictureSample();
                case "Sound":
                    return CreateSoundSample();
                case "WebView2":
                    return CreateWebView2Sample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "PersonPicture":
                    return new[]
                    {
                        new GalleryExample(
                            "Select different looks for the person picture.",
                            CreatePersonPictureExampleContent(assignRootAutomationId: true),
                            PersonPictureBasicXaml,
                            PersonPictureBasicCSharp)
                    };
                case "Sound":
                    var soundState = new SoundExampleState();
                    return new[]
                    {
                        new GalleryExample(
                            "Toggling Sound",
                            CreateSoundToggleExampleContent(assignRootAutomationId: true, soundState),
                            null,
                            SoundToggleCSharp),
                        new GalleryExample(
                            "Toggling Spatial Audio",
                            CreateSoundSpatialAudioExampleContent(soundState),
                            null,
                            SoundSpatialAudioCSharp),
                        new GalleryExample(
                            "Play Specific System Sound",
                            CreateSoundSpecificSystemSoundExampleContent(),
                            null,
                            SoundSpecificSystemSoundCSharp)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateAnimatedVisualPlayerSample()
        {
            var panel = CreateSamplePanel("AnimatedVisualPlayer maps to a WPF vector animation player with play, pause, stop, and progress controls.");
            var progress = new Slider
            {
                Width = 320,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(progress, "Progress");

            var rotate = new RotateTransform(0);
            var scale = new ScaleTransform(1, 1);
            var visual = new Grid
            {
                Width = 180,
                Height = 180,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TransformGroup
                {
                    Children = new TransformCollection { scale, rotate }
                }
            };
            visual.Children.Add(new Ellipse
            {
                Width = 118,
                Height = 118,
                Fill = CreateBrush("#0078D4"),
                Opacity = 0.16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            visual.Children.Add(new Path
            {
                Data = Geometry.Parse("M 90,20 L 148,148 L 32,148 Z"),
                Fill = CreateBrush("#0078D4"),
                Stretch = Stretch.Fill,
                Margin = new Thickness(38)
            });

            Action updateVisual = delegate
            {
                rotate.Angle = progress.Value * 3.6;
                var scaleValue = 0.82 + (Math.Sin(progress.Value / 100 * Math.PI) * 0.28);
                scale.ScaleX = scaleValue;
                scale.ScaleY = scaleValue;
            };
            progress.ValueChanged += delegate { updateVisual(); };

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(32) };
            timer.Tick += delegate
            {
                progress.Value = progress.Value >= 100 ? 0 : progress.Value + 1;
            };

            var commands = CreateCommandRow();
            var play = CreateButton("Play");
            var pause = CreateButton("Pause");
            var stop = CreateButton("Stop");
            play.Click += delegate { timer.Start(); };
            pause.Click += delegate { timer.Stop(); };
            stop.Click += delegate
            {
                timer.Stop();
                progress.Value = 0;
            };
            commands.Children.Add(play);
            commands.Children.Add(pause);
            commands.Children.Add(stop);

            panel.Children.Add(visual);
            panel.Children.Add(commands);
            panel.Children.Add(progress);
            updateVisual();
            return panel;
        }

        private static UIElement CreateCaptureElementPreviewSample()
        {
            var panel = CreateSamplePanel("CaptureElement maps to a WPF camera-preview surface placeholder because WPF has no built-in camera preview control.");
            var previewText = new TextBlock
            {
                Text = "Preview stopped",
                Foreground = Brushes.White,
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var preview = new Border
            {
                Width = 420,
                Height = 230,
                Background = CreateBrush("#202020"),
                Child = new Grid
                {
                    Children =
                    {
                        new Rectangle
                        {
                            Fill = CreateBrush("#2A2A2A"),
                            RadiusX = 8,
                            RadiusY = 8,
                            Margin = new Thickness(16)
                        },
                        previewText
                    }
                }
            };
            var output = CreateOutput("Camera preview is off.");
            var commands = CreateCommandRow();
            var start = CreateButton("Start preview");
            var stop = CreateButton("Stop preview");
            start.Click += delegate
            {
                previewText.Text = "Live camera preview";
                output.Text = "Camera preview is running.";
            };
            stop.Click += delegate
            {
                previewText.Text = "Preview stopped";
                output.Text = "Camera preview is off.";
            };
            commands.Children.Add(start);
            commands.Children.Add(stop);

            panel.Children.Add(preview);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateImageSample()
        {
            var panel = CreateSamplePanel("Image displays raster content with controllable source and stretch behavior.");
            var image = new Image
            {
                Width = 420,
                Height = 240,
                Stretch = Stretch.Uniform,
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/cliff.jpg"))
            };
            var frame = new Border
            {
                Width = 440,
                Height = 260,
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = image
            };

            var source = new ComboBox
            {
                Width = 220,
                Margin = new Thickness(0, 12, 12, 0),
                ItemsSource = new[] { "cliff.jpg", "grapes.jpg", "rainier.jpg", "sunset.jpg" },
                SelectedIndex = 0
            };
            ControlHelper.SetHeader(source, "Source");
            source.SelectionChanged += delegate
            {
                image.Source = CreateBitmap(ResourceUri("Assets/SampleMedia/" + source.SelectedItem));
            };

            var stretch = new ComboBox
            {
                Width = 180,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { Stretch.Uniform, Stretch.UniformToFill, Stretch.Fill, Stretch.None },
                SelectedItem = image.Stretch
            };
            ControlHelper.SetHeader(stretch, "Stretch");
            stretch.SelectionChanged += delegate
            {
                if (stretch.SelectedItem is Stretch)
                {
                    image.Stretch = (Stretch)stretch.SelectedItem;
                }
            };

            var options = new StackPanel { Orientation = Orientation.Horizontal };
            options.Children.Add(source);
            options.Children.Add(stretch);
            panel.Children.Add(frame);
            panel.Children.Add(options);
            return panel;
        }

        private static UIElement CreateMapControlSample()
        {
            var panel = CreateSamplePanel("MapControl maps to a WPF map-image viewport with zoom and marker overlays.");
            var mapScale = new ScaleTransform(1, 1);
            var map = new Grid
            {
                Width = 600,
                Height = 360,
                LayoutTransform = mapScale
            };
            map.Children.Add(new Image
            {
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/MapExample.png")),
                Stretch = Stretch.UniformToFill
            });
            var marker = new Border
            {
                Width = 112,
                Height = 32,
                CornerRadius = new CornerRadius(16),
                Background = CreateBrush("#0078D4"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "Seattle",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            marker.RenderTransform = new TranslateTransform(80, -44);
            map.Children.Add(marker);

            var scrollViewer = new ScrollViewer
            {
                Width = 430,
                Height = 260,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = map
            };
            var zoom = new Slider
            {
                Width = 260,
                Minimum = 0.75,
                Maximum = 2,
                Value = 1,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(zoom, "Zoom");
            zoom.ValueChanged += delegate
            {
                mapScale.ScaleX = zoom.Value;
                mapScale.ScaleY = zoom.Value;
            };

            panel.Children.Add(scrollViewer);
            panel.Children.Add(zoom);
            return panel;
        }

        private static UIElement CreateMediaPlayerElementSample()
        {
            var panel = CreateSamplePanel("MediaPlayerElement maps to WPF MediaElement with manual transport controls.");
            var player = new MediaElement
            {
                Width = 420,
                Height = 240,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Manual,
                Stretch = Stretch.Uniform,
                Source = new Uri(ResourceUri("Assets/SampleMedia/fishes.wmv"), UriKind.Absolute)
            };
            var output = CreateOutput("Ready");
            var commands = CreateCommandRow();
            var play = CreateButton("Play");
            var pause = CreateButton("Pause");
            var stop = CreateButton("Stop");
            play.Click += delegate
            {
                player.Play();
                output.Text = "Playing fishes.wmv";
            };
            pause.Click += delegate
            {
                player.Pause();
                output.Text = "Paused";
            };
            stop.Click += delegate
            {
                player.Stop();
                output.Text = "Stopped";
            };
            commands.Children.Add(play);
            commands.Children.Add(pause);
            commands.Children.Add(stop);

            var volume = new Slider
            {
                Width = 220,
                Minimum = 0,
                Maximum = 1,
                Value = 0.5,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(volume, "Volume");
            volume.ValueChanged += delegate { player.Volume = volume.Value; };
            player.Volume = volume.Value;

            panel.Children.Add(player);
            panel.Children.Add(commands);
            panel.Children.Add(volume);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreatePersonPictureSample()
        {
            return CreatePersonPictureExampleContent(assignRootAutomationId: true);
        }

        private static GallerySamplePanel CreatePersonPictureExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("PersonPicture"));
            }

            var personPicture = new Mux.PersonPicture
            {
                Name = "personPicture",
                Width = 96,
                Height = 96,
                VerticalAlignment = VerticalAlignment.Top
            };
            GalleryAutomation.WithAutomationId(personPicture, GalleryAutomation.SampleElementId("PersonPicture", "PersonPicture"));

            var profileImageRadio = new RadioButton
            {
                Name = "ProfileImageRadio",
                Content = "Profile Image",
                IsChecked = true
            };
            AutomationProperties.SetAutomationId(profileImageRadio, "ProfileImageRadio");
            var displayNameRadio = new RadioButton
            {
                Name = "DisplayNameRadio",
                Content = "Display Name"
            };
            AutomationProperties.SetAutomationId(displayNameRadio, "DisplayNameRadio");
            var initialsRadio = new RadioButton
            {
                Name = "InitialsRadio",
                Content = "Initials"
            };
            AutomationProperties.SetAutomationId(initialsRadio, "InitialsRadio");

            var profileType = new Mux.RadioButtons
            {
                Header = "Profile type",
                SelectedIndex = 0
            };
            profileType.Items.Add(profileImageRadio);
            profileType.Items.Add(displayNameRadio);
            profileType.Items.Add(initialsRadio);
            profileType.SelectionChanged += delegate
            {
                ApplyPersonPictureSelection(personPicture, profileImageRadio, displayNameRadio, initialsRadio);
            };

            var options = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0),
                Children =
                {
                    profileType
                }
            };

            root.Children.Add(personPicture);
            root.Children.Add(options);
            ApplyPersonPictureSelection(personPicture, profileImageRadio, displayNameRadio, initialsRadio);
            return root;
        }

        private static UIElement CreateSoundSample()
        {
            return CreateSoundToggleExampleContent(assignRootAutomationId: true, new SoundExampleState());
        }

        private static GallerySamplePanel CreateSoundToggleExampleContent(bool assignRootAutomationId, SoundExampleState soundState)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("Sound"));
            }

            var soundToggle = new Mux.ToggleSwitch
            {
                Name = "soundToggle",
                Width = 115,
                MinWidth = 0,
                OffContent = "Sound Off",
                OnContent = "Sound On"
            };
            GalleryAutomation.WithAutomationId(soundToggle, GalleryAutomation.SampleElementId("Sound", "ToggleSwitch"));
            soundToggle.Toggled += delegate
            {
                soundState.IsSoundOn = soundToggle.IsOn;
                UpdateSoundSpatialAudioState(soundState);
            };
            panel.Children.Add(soundToggle);
            return panel;
        }

        private static StackPanel CreateSoundSpatialAudioExampleContent(SoundExampleState soundState)
        {
            var spatialAudioBox = new CheckBox
            {
                Name = "spatialAudioBox",
                Content = "Enable Spatial Audio",
                IsEnabled = false
            };
            AutomationProperties.SetAutomationId(spatialAudioBox, "spatialAudioBox");
            soundState.SpatialAudioBox = spatialAudioBox;
            UpdateSoundSpatialAudioState(soundState);

            return new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    spatialAudioBox,
                    new TextBlock
                    {
                        Margin = new Thickness(0, 5, 0, 0),
                        FontStyle = FontStyles.Italic,
                        Foreground = SystemColors.HotTrackBrush,
                        Text = "Can only enable spatial audio when sound is on!"
                    }
                }
            };
        }

        private static StackPanel CreateSoundSpecificSystemSoundExampleContent()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            panel.Children.Add(CreateSoundButton("Focus", "0", delegate { SystemSounds.Asterisk.Play(); }));
            panel.Children.Add(CreateSoundButton("Invoke", "1", delegate { SystemSounds.Beep.Play(); }));
            panel.Children.Add(CreateSoundButton("Show", "2", delegate { SystemSounds.Exclamation.Play(); }));
            panel.Children.Add(CreateSoundButton("Hide", "3", delegate { SystemSounds.Hand.Play(); }));
            panel.Children.Add(CreateSoundButton("MovePrevious", "4", delegate { SystemSounds.Question.Play(); }));
            panel.Children.Add(CreateSoundButton("MoveNext", "5", delegate { SystemSounds.Asterisk.Play(); }));
            panel.Children.Add(CreateSoundButton("GoBack", "6", delegate { SystemSounds.Beep.Play(); }));
            return panel;
        }

        private static UIElement CreateWebView2Sample()
        {
            var panel = CreateSamplePanel("WebView2 maps to WPF WebBrowser here, hosting local HTML content without network access.");
            var browser = new WebBrowser
            {
                Width = 520,
                Height = 300
            };
            browser.Loaded += delegate
            {
                browser.NavigateToString(
                    "<!doctype html><html><head><meta charset='utf-8'>" +
                    "<style>body{font:14px Segoe UI;margin:24px;color:#1f1f1f;}button{padding:8px 12px;}</style>" +
                    "</head><body><h2>Hosted HTML</h2><p>This content is rendered inside a WPF browser control.</p>" +
                    "<button onclick=\"document.getElementById('out').textContent='Button clicked'\">Run script</button>" +
                    "<p id='out'>Ready</p></body></html>");
            };
            panel.Children.Add(browser);
            return panel;
        }

        private static void ApplyPersonPictureSelection(
            Mux.PersonPicture personPicture,
            RadioButton profileImageRadio,
            RadioButton displayNameRadio,
            RadioButton initialsRadio)
        {
            if (profileImageRadio.IsChecked == true)
            {
                personPicture.ProfilePicture = CreateBitmap(ResourceUri("Assets/UserDashboard/64-100x100.jpg"));
                personPicture.DisplayName = null;
                personPicture.Initials = null;
            }
            else if (displayNameRadio.IsChecked == true)
            {
                personPicture.ProfilePicture = null;
                personPicture.DisplayName = "Jane Doe";
                personPicture.Initials = null;
            }
            else if (initialsRadio.IsChecked == true)
            {
                personPicture.ProfilePicture = null;
                personPicture.DisplayName = null;
                personPicture.Initials = "SB";
            }
        }

        private static Button CreateSoundButton(string text, string tag, Action play)
        {
            var button = CreateButton("\u25B6 " + text);
            button.Tag = tag;
            button.Margin = new Thickness(0, 0, 0, 5);
            AutomationProperties.SetName(button, text);
            AutomationProperties.SetAutomationId(button, text);
            button.Click += delegate
            {
                play();
            };
            return button;
        }

        private static void UpdateSoundSpatialAudioState(SoundExampleState soundState)
        {
            if (soundState.SpatialAudioBox == null)
            {
                return;
            }

            soundState.SpatialAudioBox.IsEnabled = soundState.IsSoundOn;
            if (!soundState.IsSoundOn)
            {
                soundState.SpatialAudioBox.IsChecked = false;
            }
        }

        private sealed class SoundExampleState
        {
            public CheckBox SpatialAudioBox { get; set; }

            public bool IsSoundOn { get; set; }
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static BitmapImage CreateBitmap(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/ModernWpf.Gallery;component/" + path;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}
