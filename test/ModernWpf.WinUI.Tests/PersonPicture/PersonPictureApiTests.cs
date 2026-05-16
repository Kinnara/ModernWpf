using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.PersonPicture;

[TestClass]
public class PersonPictureApiTests
{
    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var personPicture = new ModernWpf.Controls.PersonPicture();
            Assert.IsNotNull(personPicture);

            Assert.AreEqual(string.Empty, personPicture.BadgeGlyph);
            Assert.AreEqual(0, personPicture.BadgeNumber);
            Assert.IsFalse(personPicture.IsGroup);
            Assert.IsNull(personPicture.ProfilePicture);
            Assert.AreEqual(string.Empty, personPicture.DisplayName);
            Assert.AreEqual(string.Empty, personPicture.Initials);

            personPicture.BadgeGlyph = "\uE765";
            Assert.AreEqual("\uE765", personPicture.BadgeGlyph);

            personPicture.BadgeNumber = 10;
            Assert.AreEqual(10, personPicture.BadgeNumber);

            personPicture.IsGroup = true;
            Assert.IsTrue(personPicture.IsGroup);

            personPicture.DisplayName = "Some Name";
            Assert.AreEqual("Some Name", personPicture.DisplayName);

            personPicture.Initials = "MS";
            Assert.AreEqual("MS", personPicture.Initials);

            var imageSource = new DrawingImage(
                new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            personPicture.ProfilePicture = imageSource;
            Assert.IsNotNull(personPicture.ProfilePicture);
        });
    }

    [TestMethod]
    public void VerifyAutomationName()
    {
        WpfTestHost.Run(() =>
        {
            var personPicture = new ModernWpf.Controls.PersonPicture();
            Assert.IsNotNull(personPicture);

            personPicture.Initials = "AB";
            Assert.AreEqual("AB", AutomationProperties.GetName(personPicture));

            personPicture.DisplayName = "Jane Smith";
            Assert.AreEqual("Jane Smith", AutomationProperties.GetName(personPicture));

            personPicture.DisplayName = "John Doe";
            Assert.AreEqual("John Doe", AutomationProperties.GetName(personPicture));

            personPicture.IsGroup = true;
            Assert.AreEqual("Group", AutomationProperties.GetName(personPicture));

            personPicture.IsGroup = false;
            personPicture.BadgeGlyph = "\uE765";
            Assert.AreEqual("John Doe, icon", AutomationProperties.GetName(personPicture));

            personPicture.BadgeText = "Skype";
            Assert.AreEqual("John Doe, Skype", AutomationProperties.GetName(personPicture));

            personPicture.BadgeText = string.Empty;
            personPicture.BadgeNumber = 5;
            Assert.AreEqual("John Doe, 5 items", AutomationProperties.GetName(personPicture));

            personPicture.BadgeText = "direct reports";
            Assert.AreEqual("John Doe, 5 direct reports", AutomationProperties.GetName(personPicture));
        });
    }

    [TestMethod]
    public void VerifySmallWidthAndHeightDoNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            var sizeChanged = false;
            var personPicture = new ModernWpf.Controls.PersonPicture();
            using var host = new TestWindowHost(personPicture);

            personPicture.SizeChanged += (_, _) => sizeChanged = true;
            personPicture.Width = 0.4;
            personPicture.Height = 0.4;
            host.UpdateLayout();

            Assert.IsTrue(sizeChanged);
        });
    }

    [TestMethod]
    public void VerifyVSMStatesForPhotosAndInitials()
    {
        WpfTestHost.Run(() =>
        {
            var personPicture = new ModernWpf.Controls.PersonPicture();
            using var host = new TestWindowHost(personPicture);

            var initialsTextBlock = FindNamedDescendant<TextBlock>(personPicture, "InitialsTextBlock");
            var placeholderIcon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(personPicture, "PlaceholderIcon");

            personPicture.IsGroup = true;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, initialsTextBlock.Visibility);
            Assert.AreEqual(Visibility.Visible, placeholderIcon.Visibility);
            Assert.AreSame(placeholderIcon.FindResource("People"), placeholderIcon.Data);

            personPicture.IsGroup = false;
            personPicture.Initials = "JS";
            host.UpdateLayout();
            Assert.AreEqual("Segoe UI", initialsTextBlock.FontFamily.Source);
            Assert.AreEqual("JS", initialsTextBlock.Text);

            personPicture.Initials = string.Empty;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, initialsTextBlock.Visibility);
            Assert.AreEqual(Visibility.Visible, placeholderIcon.Visibility);
            Assert.AreSame(placeholderIcon.FindResource("Contact"), placeholderIcon.Data);

            personPicture.FontFamily = new FontFamily("Segoe UI Emoji");
            personPicture.Initials = "\U0001F44D";
            host.UpdateLayout();
            Assert.AreEqual("Segoe UI Emoji", initialsTextBlock.FontFamily.Source);
            Assert.AreEqual("\U0001F44D", initialsTextBlock.Text);

            personPicture.IsGroup = true;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, initialsTextBlock.Visibility);
            Assert.AreEqual(Visibility.Visible, placeholderIcon.Visibility);
            Assert.AreSame(placeholderIcon.FindResource("People"), placeholderIcon.Data);
        });
    }

    [TestMethod]
    public void VerifyRenderedInitialsAndBadgeNumber()
    {
        WpfTestHost.Run(() =>
        {
            var personPicture = new ModernWpf.Controls.PersonPicture();
            using var host = new TestWindowHost(personPicture);

            var initialsTextBlock = FindNamedDescendant<TextBlock>(personPicture, "InitialsTextBlock");
            var badgeNumberTextBlock = FindNamedDescendant<TextBlock>(personPicture, "BadgeNumberTextBlock");
            var badgeGrid = FindNamedDescendant<Grid>(personPicture, "BadgeGrid");
            var badgingEllipse = FindNamedDescendant<Ellipse>(personPicture, "BadgingEllipse");

            Assert.AreEqual(Visibility.Collapsed, badgeGrid.Visibility);

            personPicture.Initials = "AS";
            host.UpdateLayout();
            Assert.AreEqual("AS", initialsTextBlock.Text);

            personPicture.Initials = string.Empty;
            personPicture.DisplayName = "Some Name";
            host.UpdateLayout();
            Assert.AreEqual("SN", initialsTextBlock.Text);

            personPicture.DisplayName = "Another Name (OSG)";
            host.UpdateLayout();
            Assert.AreEqual("AN", initialsTextBlock.Text);

            personPicture.BadgeNumber = 1;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, badgeGrid.Visibility);
            Assert.AreEqual("1", badgeNumberTextBlock.Text);

            personPicture.BadgeNumber = 125;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, badgeGrid.Visibility);
            Assert.AreEqual("99+", badgeNumberTextBlock.Text);

            personPicture.BadgeNumber = 0;
            personPicture.BadgeImageSource = new DrawingImage(
                new GeometryDrawing(Brushes.Blue, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, badgeGrid.Visibility);
            Assert.AreEqual((double)personPicture.TryFindResource("PersonPictureEllipseBadgeImageSourceStrokeOpacity"), badgingEllipse.Opacity);

            personPicture.BadgeImageSource = null;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, badgeGrid.Visibility);
        });
    }

    [TestMethod]
    public void VerifyInitialsDisplayNameAndImagePriority()
    {
        WpfTestHost.Run(() =>
        {
            var personPicture = new ModernWpf.Controls.PersonPicture
            {
                Initials = "AL",
                DisplayName = "Some Name"
            };

            using var host = new TestWindowHost(personPicture);

            var initialsTextBlock = FindNamedDescendant<TextBlock>(personPicture, "InitialsTextBlock");
            var placeholderIcon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(personPicture, "PlaceholderIcon");
            var personPictureEllipse = FindNamedDescendant<Ellipse>(personPicture, "PersonPictureEllipse");

            Assert.AreEqual("AL", initialsTextBlock.Text);

            personPicture.Initials = string.Empty;
            host.UpdateLayout();
            Assert.AreEqual("SN", initialsTextBlock.Text);

            var imageSource = new DrawingImage(
                new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            personPicture.ProfilePicture = imageSource;
            host.UpdateLayout();

            var imageBrush = personPictureEllipse.Fill as ImageBrush;
            Assert.IsNotNull(imageBrush);
            Assert.AreSame(imageSource, imageBrush!.ImageSource);

            personPicture.ProfilePicture = null;
            personPicture.DisplayName = string.Empty;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, initialsTextBlock.Visibility);
            Assert.AreEqual(Visibility.Visible, placeholderIcon.Visibility);
            Assert.AreSame(placeholderIcon.FindResource("Contact"), placeholderIcon.Data);
            Assert.IsNull(personPictureEllipse.Fill);
        });
    }

    [TestMethod]
    public void VerifyPersonPictureVisualTree()
    {
        WpfTestHost.Run(() =>
        {
            var personPicture = new ModernWpf.Controls.PersonPicture
            {
                Initials = "LC",
                Width = 100,
                Height = 100
            };

            using var host = new TestWindowHost(personPicture);

            Assert.IsNotNull(FindNamedDescendant<Grid>(personPicture, "RootGrid"));
            Assert.IsNotNull(FindNamedDescendant<TextBlock>(personPicture, "InitialsTextBlock"));
            Assert.IsNotNull(FindNamedDescendant<FrameworkElement>(personPicture, "PersonPictureEllipse"));
            Assert.IsNotNull(FindNamedDescendant<Grid>(personPicture, "BadgeGrid"));
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2PersonPictureResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var personPicture = new ModernWpf.Controls.PersonPicture
            {
                Initials = "MW"
            };

            using var host = new TestWindowHost(personPicture);
            host.UpdateLayout();

            Assert.AreEqual(96.0, personPicture.Width);
            Assert.AreEqual(96.0, personPicture.Height);
            AssertBrushEquals((Brush)personPicture.TryFindResource("PersonPictureForegroundThemeBrush"), personPicture.Foreground);
            Assert.AreEqual(
                ((FontFamily)personPicture.TryFindResource("ContentControlThemeFontFamily")).Source,
                personPicture.FontFamily.Source);
            Assert.AreEqual(FontWeights.SemiBold, personPicture.FontWeight);
            Assert.IsFalse(personPicture.IsTabStop);

            var initialsTextBlock = FindNamedDescendant<TextBlock>(personPicture, "InitialsTextBlock");
            Assert.AreEqual(40.0, initialsTextBlock.FontSize, 0.5);
            Assert.AreEqual(personPicture.FontFamily.Source, initialsTextBlock.FontFamily.Source);
            AssertBrushEquals(personPicture.Foreground, initialsTextBlock.Foreground);
            Assert.AreEqual(personPicture.FontWeight, initialsTextBlock.FontWeight);

            var personPictureEllipse = FindNamedDescendant<Ellipse>(personPicture, "PersonPictureEllipse");
            Assert.AreEqual(FlowDirection.LeftToRight, personPictureEllipse.FlowDirection);

            var badgeGrid = FindNamedDescendant<Grid>(personPicture, "BadgeGrid");
            Assert.AreEqual(Visibility.Collapsed, badgeGrid.Visibility);
            Assert.AreEqual(VerticalAlignment.Top, badgeGrid.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Right, badgeGrid.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0, -4, -4, 0), badgeGrid.Margin);

            var badgingBackgroundEllipse = FindNamedDescendant<Ellipse>(personPicture, "BadgingBackgroundEllipse");
            Assert.AreEqual((double)personPicture.TryFindResource("PersonPictureEllipseBadgeStrokeOpacity"), badgingBackgroundEllipse.Opacity);
            AssertBrushEquals((Brush)personPicture.TryFindResource("PersonPictureEllipseBadgeFillThemeBrush"), badgingBackgroundEllipse.Fill);
            AssertBrushEquals((Brush)personPicture.TryFindResource("PersonPictureEllipseBadgeStrokeThemeBrush"), badgingBackgroundEllipse.Stroke);
            Assert.AreEqual((double)personPicture.TryFindResource("PersonPictureEllipseBadgeStrokeThickness"), badgingBackgroundEllipse.StrokeThickness);

            var badgingEllipse = FindNamedDescendant<Ellipse>(personPicture, "BadgingEllipse");
            Assert.AreEqual(0.0, badgingEllipse.Opacity);
            Assert.AreEqual(FlowDirection.LeftToRight, badgingEllipse.FlowDirection);

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "PersonPictureForegroundThemeBrush", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "PersonPictureEllipseBadgeForegroundThemeBrush", "TextOnAccentFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "PersonPictureEllipseBadgeFillThemeBrush", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "PersonPictureEllipseBadgeStrokeThemeBrush", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "PersonPictureEllipseFillThemeBrush", "ControlAltFillColorQuarternaryBrush");
                AssertThemeResourceReference(themeName, "PersonPictureEllipseFillStrokeBrush", "CardStrokeColorDefaultBrush");
                AssertCommonFinalResourceValues(themeName);
            }

            AssertThemeResourceReference("HighContrast", "PersonPictureForegroundThemeBrush", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "PersonPictureEllipseBadgeForegroundThemeBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "PersonPictureEllipseBadgeFillThemeBrush", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "PersonPictureEllipseBadgeStrokeThemeBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "PersonPictureEllipseFillThemeBrush", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "PersonPictureEllipseFillStrokeBrush", "CardStrokeColorDefaultBrush");
            AssertCommonFinalResourceValues("HighContrast");
        });
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .Single(element => element.Name == name);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertCommonFinalResourceValues(string themeName)
    {
        AssertThemeResourceValue(themeName, "PersonPictureEllipseBadgeStrokeOpacity", 1.0);
        AssertThemeResourceValue(themeName, "PersonPictureEllipseBadgeImageSourceStrokeOpacity", 1.0);
        AssertThemeResourceValue(themeName, "PersonPictureEllipseStrokeThickness", 1.0);
        AssertThemeResourceValue(themeName, "PersonPictureEllipseBadgeStrokeThickness", 2.0);
        AssertThemeResourceValue(themeName, "PersonPictureBadgeGridMargin", new Thickness(0, -4, -4, 0));
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
    }
}
