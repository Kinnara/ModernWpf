using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfTestHostTests
    {
        [TestMethod]
        public void RunClosesWindowsBeforeTheNextInvocation()
        {
            WpfTestHost.Run(() =>
            {
                var window = new Window
                {
                    Content = new Button()
                };

                window.Show();
                Assert.IsTrue(window.IsVisible);
            });

            WpfTestHost.Run(() =>
            {
                Assert.AreEqual(0, Application.Current.Windows.Count);
            });
        }

        [TestMethod]
        public void RunUsesDeterministic96Dpi()
        {
            WpfTestHost.Run(() =>
            {
                var window = new Window
                {
                    Content = new Button()
                };

                window.Show();
                WpfTestHost.DoEvents();

                var dpi = VisualTreeHelper.GetDpi(window);
                Assert.AreEqual(1d, dpi.DpiScaleX, 0.001d);
                Assert.AreEqual(1d, dpi.DpiScaleY, 0.001d);
            });
        }
    }
}
