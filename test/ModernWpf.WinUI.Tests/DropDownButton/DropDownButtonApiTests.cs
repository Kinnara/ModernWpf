using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.DropDownButton;

[TestClass]
public class DropDownButtonApiTests
{
    [TestMethod]
    public void VerifyDropDownButtonPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "Flyout content"
                }
            };
            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options",
                Flyout = flyout,
                CornerRadius = new CornerRadius(4),
                UseSystemFocusVisuals = true,
                FocusVisualMargin = new Thickness(2)
            };

            Assert.AreEqual("Options", button.Content);
            Assert.AreSame(flyout, button.Flyout);
            Assert.AreEqual(new CornerRadius(4), button.CornerRadius);
            Assert.IsTrue(button.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(2), button.FocusVisualMargin);

            button.Flyout = null;

            Assert.IsNull(button.Flyout);
        });
    }

    [TestMethod]
    public void VerifyDropDownButtonTemplateAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options"
            };

            using var host = new TestWindowHost(button, width: 320, height: 160);
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, button.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Center, button.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, button.VerticalContentAlignment);
            Assert.AreEqual(new Thickness(-3), button.FocusVisualMargin);
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondary"));
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondaryPointerOver"));
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondaryPressed"));

            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button);

            Assert.IsNotNull(chevron);
            Assert.AreEqual("ChevronIcon", chevron!.Name);
            Assert.IsNotNull(chevron.Foreground);
        });
    }
}
