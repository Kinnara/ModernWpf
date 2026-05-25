using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ModernWpf.Gallery.Models;
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

        private const string MediaPlayerElementTransportXaml =
@"<MediaPlayerElement Source=""/Assets/SampleMedia/ladybug.wmv""
                    MaxWidth=""400""
                    AutoPlay=""False""
                    AreTransportControlsEnabled=""True"" />";

        private const string MediaPlayerElementTransportCSharp =
@"private async void OpenFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
{
    var picker = new FileOpenPicker((sender as Button).XamlRoot.ContentIslandEnvironment.AppWindowId);
    var file = await picker.PickSingleFileAsync();
    if (file == null)
        return;

    var mediaSource = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(file.Path));
    Player1.Source = mediaSource;
}";

        private const string MediaPlayerElementAutoPlayXaml =
@"<MediaPlayerElement Source=""Assets/SampleMedia/fishes.wmv""
                    MaxWidth=""400""
                    AutoPlay=""True"" />";

        private const string MapControlXaml =
@"<MapControl x:Name=""map1"" MapServiceToken=""MapServiceToken"" Height=""600""/>";

        private const string MapControlCSharp =
@"
BasicGeoposition centerPosition = new BasicGeoposition { Latitude = 0, Longitude = 0 };
Geopoint centerPoint = new Geopoint(centerPosition);

map1.Center = centerPoint;
map1.ZoomLevel = 1;

var myLandmarks = new List<MapElement>();
BasicGeoposition position = new BasicGeoposition { Latitude = -30.034647, Longitude = -51.217659 };
Geopoint point = new Geopoint(position);

var icon = new MapIcon
{
    Location = point,
};

myLandmarks.Add(icon);

var LandmarksLayer = new MapElementsLayer
{
    MapElements = myLandmarks
};

map1.Layers.Add(LandmarksLayer);";

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

        public static IReadOnlyList<GalleryExample> CreateExamples(
            string uniqueId,
            IReadOnlyList<SampleSnippet> sampleSnippets = null)
        {
            switch (uniqueId)
            {
                case "MapControl":
                    return new[]
                    {
                        new GalleryExample(
                            "Showing a pin on the map",
                            CreateMapControlExampleContent(assignRootAutomationId: true),
                            MapControlXaml,
                            FindSampleCodeText(sampleSnippets, "MapControlSample_cs.txt") ?? MapControlCSharp)
                    };
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
                case "MediaPlayerElement":
                    return new[]
                    {
                        new GalleryExample(
                            "A MediaPlayerElement with transport controls.",
                            CreateMediaPlayerElementWithTransportControls(assignRootAutomationId: true),
                            MediaPlayerElementTransportXaml,
                            MediaPlayerElementTransportCSharp),
                        new GalleryExample(
                            "A MediaPlayerElement that autoplays the video.",
                            CreateMediaPlayerElementAutoPlayExampleContent(),
                            MediaPlayerElementAutoPlayXaml,
                            null)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "MapControl":
                    return CreateMapControlIntroContent();
                default:
                    return null;
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
            return CreateMapControlExampleContent(assignRootAutomationId: true);
        }

        private static StackPanel CreateMapControlIntroContent()
        {
            var root = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 24)
            };

            var instructions = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap
            };
            instructions.Inlines.Add(new Run("Follow instructions "));
            instructions.Inlines.Add(new Hyperlink(new Run("here"))
            {
                NavigateUri = new Uri("https://learn.microsoft.com/azure/azure-maps/how-to-manage-account-keys")
            });
            instructions.Inlines.Add(new Run(" to obtain your MapServiceToken."));

            root.Children.Add(instructions);
            root.Children.Add(new Image
            {
                Height = 320,
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/MapExample.png"))
            });

            return root;
        }

        private static GallerySamplePanel CreateMapControlExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("MapControl"));
            }

            var token = new PasswordBox
            {
                Name = "MapToken",
                MinWidth = 200
            };
            AutomationProperties.SetName(token, "Map service token");
            GalleryAutomation.WithAutomationId(token, GalleryAutomation.SampleElementId("MapControl", "MapToken"));
            ControlHelper.SetPlaceholderText(token, "Map service token");

            var map = CreateMapControlSurface();
            var setToken = new Button
            {
                Content = "Set token",
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(16, 6, 16, 6),
                VerticalAlignment = VerticalAlignment.Top
            };
            RoutedEventHandler setTokenHandler = delegate
            {
                map.Tag = token.Password;
            };
            setToken.Click += setTokenHandler;
            token.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    setTokenHandler(sender, e);
                }
            };

            var tokenRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12),
                Children =
                {
                    token,
                    setToken
                }
            };

            root.Children.Add(tokenRow);
            root.Children.Add(map);
            return root;
        }

        private static Grid CreateMapControlSurface()
        {
            var map = new Grid
            {
                Name = "map1",
                Height = 400,
                MinWidth = 400,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ClipToBounds = true,
                Background = Brushes.Black,
                Tag = "Center=0,0; ZoomLevel=1; Pin=-30.034647,-51.217659"
            };
            AutomationProperties.SetName(map, "Map");
            GalleryAutomation.WithAutomationId(map, GalleryAutomation.SampleElementId("MapControl", "MapControl"));
            return map;
        }

        private static UIElement CreateMediaPlayerElementSample()
        {
            return CreateMediaPlayerElementWithTransportControls(assignRootAutomationId: true);
        }

        private static GallerySamplePanel CreateMediaPlayerElementWithTransportControls(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("MediaPlayerElement"));
            }

            var player = CreateMediaPlayerSurface("Player1", "Assets/SampleMedia/ladybug.wmv", showTransportControls: true);
            GalleryAutomation.WithAutomationId(player, GalleryAutomation.SampleElementId("MediaPlayerElement", "MediaPlayerElement"));

            var openFileButton = CreateButton("Open a file");
            openFileButton.Name = "OpenFileButton";
            AutomationProperties.SetName(openFileButton, "Open file button");
            GalleryAutomation.WithAutomationId(openFileButton, GalleryAutomation.SampleElementId("MediaPlayerElement", "OpenFileButton"));
            openFileButton.Click += delegate
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Media files|*.wmv;*.mp4;*.avi;*.mp3;*.wav|All files|*.*"
                };
                if (dialog.ShowDialog() == true)
                {
                    player.Tag = dialog.FileName;
                }
            };

            var options = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0),
                Children =
                {
                    openFileButton
                }
            };

            root.Children.Add(player);
            root.Children.Add(options);
            return root;
        }

        private static GallerySamplePanel CreateMediaPlayerElementAutoPlayExampleContent()
        {
            var root = new GallerySamplePanel();
            var player = CreateMediaPlayerSurface("Player2", "Assets/SampleMedia/fishes.wmv", showTransportControls: false);
            GalleryAutomation.WithAutomationId(player, GalleryAutomation.SampleElementId("MediaPlayerElement", "AutoPlayMediaPlayerElement"));
            root.Children.Add(player);
            return root;
        }

        private static Grid CreateMediaPlayerSurface(string name, string sourcePath, bool showTransportControls)
        {
            var player = new Grid
            {
                Name = name,
                Width = 400,
                Height = 225,
                MaxWidth = 400,
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = sourcePath,
                ClipToBounds = true,
                Background = Brushes.Black
            };
            var posterPath = showTransportControls
                ? "Assets/SampleMedia/ladybug.poster.png"
                : "Assets/SampleMedia/fishes.poster.png";
            player.Children.Add(new Image
            {
                Source = CreateBitmap(ResourceUri(posterPath)),
                Width = 400,
                Height = 225,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            });

            return player;
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

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string title)
        {
            if (snippets == null)
            {
                return null;
            }

            foreach (var snippet in snippets)
            {
                if (string.Equals(snippet.Title, title, StringComparison.OrdinalIgnoreCase))
                {
                    return snippet.Text;
                }
            }

            return null;
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
