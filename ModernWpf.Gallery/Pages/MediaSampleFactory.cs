using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class MediaSampleFactory
    {
        private const string PersonPictureProfileImageResourcePath =
            "Assets/SampleMedia/shoulder-tap-static-payload.png";

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

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "PersonPicture":
                    return CreatePersonPictureSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "PersonPicture":
                    var exampleContent = CreatePersonPictureExampleContent(
                        assignRootAutomationId: true,
                        out var optionsContent);
                    return new[]
                    {
                        new GalleryExample(
                            "Select different looks for the person picture.",
                            exampleContent,
                            PersonPictureBasicXaml,
                            PersonPictureBasicCSharp,
                            optionsContent)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreatePersonPictureSample()
        {
            var content = CreatePersonPictureExampleContent(
                assignRootAutomationId: true,
                out var optionsContent);
            var layout = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            layout.Children.Add(content);
            layout.Children.Add(new Border
            {
                Margin = new Thickness(24, 0, 0, 0),
                Child = optionsContent
            });
            return layout;
        }

        private static GallerySamplePanel CreatePersonPictureExampleContent(
            bool assignRootAutomationId,
            out UIElement optionsContent)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("PersonPicture"));
            }

            var personPicture = new Mux.PersonPicture
            {
                Name = "personPicture",
                Height = 300,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(personPicture, GalleryAutomation.SampleElementId("PersonPicture", "PersonPicture"));

            var profileImageRadio = new RadioButton
            {
                Name = "ProfileImageRadio",
                Content = "Profile Image",
                IsChecked = true
            };
            var displayNameRadio = new RadioButton
            {
                Name = "DisplayNameRadio",
                Content = "Display Name"
            };
            var initialsRadio = new RadioButton
            {
                Name = "InitialsRadio",
                Content = "Initials"
            };

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
                Children =
                {
                    profileType
                }
            };

            root.Children.Add(personPicture);
            optionsContent = options;
            ApplyPersonPictureSelection(personPicture, profileImageRadio, displayNameRadio, initialsRadio);
            return root;
        }

        private static void ApplyPersonPictureSelection(
            Mux.PersonPicture personPicture,
            RadioButton profileImageRadio,
            RadioButton displayNameRadio,
            RadioButton initialsRadio)
        {
            if (profileImageRadio.IsChecked == true)
            {
                personPicture.ProfilePicture = CreateBitmap(ResourceUri(PersonPictureProfileImageResourcePath));
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

        private static BitmapImage CreateBitmap(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            if (bitmap.CanFreeze)
            {
                bitmap.Freeze();
            }
            return bitmap;
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/ModernWpf.Gallery;component/" + path;
        }
    }
}
