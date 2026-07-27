using System.Windows;
using System.Windows.Controls;
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
    }
}
