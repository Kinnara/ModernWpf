using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfLayoutTests
    {
        [TestMethod]
        public void WrapPanelArrangesChildrenLikeWinUIGalleryWrapGrid()
        {
            WpfTestHost.Run(() =>
            {
                var panel = new WrapPanel
                {
                    Width = 250,
                    Height = 250
                };

                for (var i = 0; i < 4; i++)
                {
                    panel.Children.Add(new Button
                    {
                        Width = 120,
                        Height = 80
                    });
                }

                Layout(panel, 250, 250);

                AssertRectEqual(new Rect(0, 0, 120, 80), LayoutInformation.GetLayoutSlot((FrameworkElement)panel.Children[0]));
                AssertRectEqual(new Rect(120, 0, 120, 80), LayoutInformation.GetLayoutSlot((FrameworkElement)panel.Children[1]));
                AssertRectEqual(new Rect(0, 80, 120, 80), LayoutInformation.GetLayoutSlot((FrameworkElement)panel.Children[2]));
                AssertRectEqual(new Rect(120, 80, 120, 80), LayoutInformation.GetLayoutSlot((FrameworkElement)panel.Children[3]));
            });
        }

        [TestMethod]
        public void DispatcherLayoutReactsToAlignmentChange()
        {
            WpfTestHost.Run(() =>
            {
                var grid = new Grid
                {
                    Width = 200,
                    Height = 100
                };
                var border = new Border
                {
                    Child = new Rectangle
                    {
                        Width = 100,
                        Height = 20,
                        Fill = Brushes.Red
                    }
                };
                grid.Children.Add(border);

                Layout(grid, 200, 100);
                Assert.AreEqual(200, border.ActualWidth, 0.1);

                border.HorizontalAlignment = HorizontalAlignment.Left;
                Layout(grid, 200, 100);
                Assert.AreEqual(100, border.ActualWidth, 0.1);
            });
        }

        private static void Layout(FrameworkElement element, double width, double height)
        {
            element.Measure(new Size(width, height));
            element.Arrange(new Rect(0, 0, width, height));
            element.UpdateLayout();
        }

        private static void AssertRectEqual(Rect expected, Rect actual)
        {
            Assert.AreEqual(expected.X, actual.X, 0.1, "X");
            Assert.AreEqual(expected.Y, actual.Y, 0.1, "Y");
            Assert.AreEqual(expected.Width, actual.Width, 0.1, "Width");
            Assert.AreEqual(expected.Height, actual.Height, 0.1, "Height");
        }
    }
}
