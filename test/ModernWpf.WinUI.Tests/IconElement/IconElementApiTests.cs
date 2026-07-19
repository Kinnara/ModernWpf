using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.IconElements;

[TestClass]
public class IconElementApiTests
{
    [TestMethod]
    public void SymbolIconUsesCurrentWinUIGlyphRemapping()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new SymbolIcon();
            using var host = new TestWindowHost(icon, width: 80, height: 80);

            var textBlock = VisualTreeTestHelper.FindDescendant<TextBlock>(icon);
            Assert.IsNotNull(textBlock);
            Assert.AreEqual("\uE899", textBlock!.Text, "The default Emoji enum value should render the current WinUI glyph.");

            AssertGlyph(Symbol.Accept, "\uE8FB");
            AssertGlyph(Symbol.Account, "\uE910");
            AssertGlyph(Symbol.List, "\uEA37");
            AssertGlyph(Symbol.StopSlideShow, "\uE620");
            AssertGlyph(Symbol.Target, "\uF5F0");
            AssertGlyph(Symbol.GlobalNavigationButton, "\uE700");
            AssertGlyph(Symbol.Share, "\uE72D");

            void AssertGlyph(Symbol symbol, string expected)
            {
                icon.Symbol = symbol;
                host.UpdateLayout();
                Assert.AreEqual(expected, textBlock.Text, $"Unexpected current WinUI glyph mapping for {symbol}.");
            }
        });
    }

    [TestMethod]
    public void FontIconDefaultsAndInheritedTextStyleMatchWinUI()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new FontIcon { Glyph = "\uE790" };
            var root = new Grid();
            TextElement.SetFontWeight(root, FontWeights.Bold);
            TextElement.SetFontStyle(root, FontStyles.Italic);
            root.Children.Add(icon);

            using var host = new TestWindowHost(root, width: 120, height: 80);
            var textBlock = VisualTreeTestHelper.FindDescendant<TextBlock>(icon);

            Assert.AreEqual("Segoe Fluent Icons,Segoe MDL2 Assets", icon.FontFamily.Source);
            Assert.AreEqual(20.0, icon.FontSize);
            Assert.AreEqual(FontWeights.Bold, icon.FontWeight);
            Assert.AreEqual(FontStyles.Italic, icon.FontStyle);
            Assert.IsTrue(icon.IsTextScaleFactorEnabled);
            Assert.IsFalse(icon.MirroredWhenRightToLeft);
            Assert.IsNotNull(textBlock);
            Assert.AreEqual(FontWeights.Bold, textBlock!.FontWeight);
            Assert.AreEqual(FontStyles.Italic, textBlock.FontStyle);
        });
    }

    [TestMethod]
    public void PathIconKeepsSourceDefaultPathStretch()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new PathIcon
            {
                Width = 64,
                Height = 64,
                Data = Geometry.Parse("M 0,0 L 16,0 16,16 0,16 Z")
            };

            using var host = new TestWindowHost(icon, width: 100, height: 100);
            var path = VisualTreeTestHelper.FindDescendant<Path>(icon);

            Assert.IsNotNull(path);
            Assert.AreEqual(Stretch.None, path!.Stretch);
            Assert.AreEqual(HorizontalAlignment.Stretch, path.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, path.VerticalAlignment);
        });
    }

    [TestMethod]
    public void IconElementStringConverterCreatesMappedSymbolIcon()
    {
        WpfTestHost.Run(() =>
        {
            var converter = TypeDescriptor.GetConverter(typeof(ModernWpf.Controls.IconElement));
            var icon = converter.ConvertFromInvariantString("Accept") as SymbolIcon;

            Assert.IsNotNull(icon);
            Assert.AreEqual(Symbol.Accept, icon!.Symbol);

            using var host = new TestWindowHost(icon, width: 80, height: 80);
            var textBlock = VisualTreeTestHelper.FindDescendant<TextBlock>(icon);
            Assert.IsNotNull(textBlock);
            Assert.AreEqual("\uE8FB", textBlock!.Text);
        });
    }

    [TestMethod]
    public void IconElementsHaveNoStandaloneAutomationPeer()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new SymbolIcon(Symbol.Accept);
            using var host = new TestWindowHost(icon, width: 80, height: 80);

            Assert.IsNull(FrameworkElementAutomationPeer.CreatePeerForElement(icon));
        });
    }
}
