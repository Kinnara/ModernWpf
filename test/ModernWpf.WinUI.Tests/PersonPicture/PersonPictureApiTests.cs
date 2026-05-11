using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .Single(element => element.Name == name);
    }
}
