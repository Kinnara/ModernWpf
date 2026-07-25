using System;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Media.Animation;
using ModernWpf.Navigation;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Navigation;

[TestClass]
public class FrameNavigationCacheTests
{
    [TestMethod]
    public void NavigationCachePropertiesMatchWinUIDefaultsAndValidation()
    {
        WpfTestHost.Run(() =>
        {
            var frame = new ModernWpf.Controls.Frame();
            var page = new ModernWpf.Controls.Page();

            Assert.AreEqual(10, frame.CacheSize);
            Assert.AreEqual(NavigationCacheMode.Disabled, page.NavigationCacheMode);
            Assert.IsNotNull(ModernWpf.Controls.Frame.CacheSizeProperty);
            Assert.IsNotNull(ModernWpf.Controls.Page.NavigationCacheModeProperty);
            Assert.ThrowsExactly<ArgumentException>(() => frame.CacheSize = -1);
            Assert.ThrowsExactly<ArgumentException>(
                () => page.NavigationCacheMode = (NavigationCacheMode)3);

            var parsedPage = (ModernWpf.Controls.Page)XamlReader.Parse(
                "<controls:Page " +
                "xmlns:controls=\"clr-namespace:ModernWpf.Controls;assembly=ModernWpf\" " +
                "NavigationCacheMode=\"Required\" />");
            var parsedFrame = (ModernWpf.Controls.Frame)XamlReader.Parse(
                "<controls:Frame " +
                "xmlns:controls=\"clr-namespace:ModernWpf.Controls;assembly=ModernWpf\" " +
                "CacheSize=\"2\" />");

            Assert.AreEqual(NavigationCacheMode.Required, parsedPage.NavigationCacheMode);
            Assert.AreEqual(2, parsedFrame.CacheSize);
        });
    }

    [TestMethod]
    public void DisabledNavigationCacheModeCreatesNewPageInstances()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            DisabledCachePage.InstanceCount = 0;
            var frame = new ModernWpf.Controls.Frame();
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            Assert.IsTrue(frame.Navigate(typeof(DisabledCachePage), new object()));
            host.UpdateLayout();
            var firstInstance = frame.Content;

            Assert.IsTrue(frame.Navigate(typeof(ModernWpf.Controls.Page)));
            host.UpdateLayout();
            Assert.IsTrue(frame.Navigate(typeof(DisabledCachePage)));
            host.UpdateLayout();

            Assert.AreNotSame(firstInstance, frame.Content);
            Assert.AreEqual(2, DisabledCachePage.InstanceCount);
        });
    }

    [TestMethod]
    public void RequiredNavigationCacheModeReusesPageInstancesRegardlessOfCacheSize()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            RequiredCachePage.InstanceCount = 0;
            var frame = new ModernWpf.Controls.Frame
            {
                CacheSize = 0
            };
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            Assert.IsTrue(
                frame.Navigate(
                    typeof(RequiredCachePage),
                    new object(),
                    new SuppressNavigationTransitionInfo()));
            host.UpdateLayout();
            var firstInstance = frame.Content;

            Assert.IsTrue(frame.Navigate(typeof(ModernWpf.Controls.Page)));
            host.UpdateLayout();
            frame.SourcePageType = typeof(RequiredCachePage);
            host.UpdateLayout();

            Assert.AreSame(firstInstance, frame.Content);
            Assert.AreEqual(typeof(RequiredCachePage), frame.CurrentSourcePageType);
            Assert.AreEqual(1, RequiredCachePage.InstanceCount);
        });
    }

    [TestMethod]
    public void EnabledNavigationCacheModeUsesLeastRecentlyUsedCacheSize()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            EnabledCachePageA.InstanceCount = 0;
            EnabledCachePageB.InstanceCount = 0;
            EnabledCachePageC.InstanceCount = 0;
            var frame = new ModernWpf.Controls.Frame
            {
                CacheSize = 2
            };
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            Assert.IsTrue(frame.Navigate(typeof(EnabledCachePageA)));
            host.UpdateLayout();
            var firstA = frame.Content;

            Assert.IsTrue(frame.Navigate(typeof(EnabledCachePageB)));
            host.UpdateLayout();
            var firstB = frame.Content;

            Assert.IsTrue(frame.Navigate(typeof(EnabledCachePageA)));
            host.UpdateLayout();
            Assert.AreSame(firstA, frame.Content);

            Assert.IsTrue(frame.Navigate(typeof(EnabledCachePageC)));
            host.UpdateLayout();
            Assert.IsTrue(frame.Navigate(typeof(EnabledCachePageA)));
            host.UpdateLayout();
            Assert.AreSame(firstA, frame.Content);

            Assert.IsTrue(frame.Navigate(typeof(EnabledCachePageB)));
            host.UpdateLayout();
            Assert.AreNotSame(firstB, frame.Content);
            Assert.AreEqual(1, EnabledCachePageA.InstanceCount);
            Assert.AreEqual(2, EnabledCachePageB.InstanceCount);
            Assert.AreEqual(1, EnabledCachePageC.InstanceCount);
        });
    }

    [TestMethod]
    public void EnabledNavigationCacheModeDoesNotCacheWhenCacheSizeIsZero()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            ZeroSizeCachePage.InstanceCount = 0;
            var frame = new ModernWpf.Controls.Frame
            {
                CacheSize = 0
            };
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            Assert.IsTrue(frame.Navigate(typeof(ZeroSizeCachePage)));
            host.UpdateLayout();
            var firstInstance = frame.Content;

            Assert.IsTrue(frame.Navigate(typeof(ModernWpf.Controls.Page)));
            host.UpdateLayout();
            Assert.IsTrue(frame.Navigate(typeof(ZeroSizeCachePage)));
            host.UpdateLayout();

            Assert.AreNotSame(firstInstance, frame.Content);
            Assert.AreEqual(2, ZeroSizeCachePage.InstanceCount);
        });
    }

    [TestMethod]
    public void ReducingCacheSizeEvictsLeastRecentlyUsedEnabledPage()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            ResizableCachePageA.InstanceCount = 0;
            ResizableCachePageB.InstanceCount = 0;
            var frame = new ModernWpf.Controls.Frame
            {
                CacheSize = 2
            };
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            Assert.IsTrue(frame.Navigate(typeof(ResizableCachePageA)));
            host.UpdateLayout();
            var firstA = frame.Content;
            Assert.IsTrue(frame.Navigate(typeof(ResizableCachePageB)));
            host.UpdateLayout();
            var firstB = frame.Content;

            frame.CacheSize = 1;

            Assert.IsTrue(frame.Navigate(typeof(ResizableCachePageB)));
            host.UpdateLayout();
            Assert.AreSame(firstB, frame.Content);
            Assert.IsTrue(frame.Navigate(typeof(ResizableCachePageA)));
            host.UpdateLayout();
            Assert.AreNotSame(firstA, frame.Content);
            Assert.AreEqual(2, ResizableCachePageA.InstanceCount);
            Assert.AreEqual(1, ResizableCachePageB.InstanceCount);
        });
    }

    [TestMethod]
    public void ChangingNavigationCacheModeToDisabledFlushesPage()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            FlushableCachePage.InstanceCount = 0;
            var frame = new ModernWpf.Controls.Frame();
            using var host = new TestWindowHost(frame, width: 320, height: 240);

            Assert.IsTrue(frame.Navigate(typeof(FlushableCachePage)));
            host.UpdateLayout();
            var firstInstance = (FlushableCachePage)frame.Content;

            firstInstance.NavigationCacheMode = NavigationCacheMode.Disabled;
            Assert.IsTrue(frame.Navigate(typeof(ModernWpf.Controls.Page)));
            host.UpdateLayout();
            Assert.IsTrue(frame.Navigate(typeof(FlushableCachePage)));
            host.UpdateLayout();

            Assert.AreNotSame(firstInstance, frame.Content);
            Assert.AreEqual(2, FlushableCachePage.InstanceCount);
        });
    }

    public sealed class RequiredCachePage : CountingCachePage<RequiredCachePage>
    {
        public RequiredCachePage()
            : base(NavigationCacheMode.Required)
        {
        }
    }

    public sealed class DisabledCachePage : CountingCachePage<DisabledCachePage>
    {
        public DisabledCachePage()
            : base(NavigationCacheMode.Disabled)
        {
        }
    }

    public sealed class EnabledCachePageA : CountingCachePage<EnabledCachePageA>
    {
        public EnabledCachePageA()
            : base(NavigationCacheMode.Enabled)
        {
        }
    }

    public sealed class EnabledCachePageB : CountingCachePage<EnabledCachePageB>
    {
        public EnabledCachePageB()
            : base(NavigationCacheMode.Enabled)
        {
        }
    }

    public sealed class EnabledCachePageC : CountingCachePage<EnabledCachePageC>
    {
        public EnabledCachePageC()
            : base(NavigationCacheMode.Enabled)
        {
        }
    }

    public sealed class ZeroSizeCachePage : CountingCachePage<ZeroSizeCachePage>
    {
        public ZeroSizeCachePage()
            : base(NavigationCacheMode.Enabled)
        {
        }
    }

    public sealed class ResizableCachePageA : CountingCachePage<ResizableCachePageA>
    {
        public ResizableCachePageA()
            : base(NavigationCacheMode.Enabled)
        {
        }
    }

    public sealed class ResizableCachePageB : CountingCachePage<ResizableCachePageB>
    {
        public ResizableCachePageB()
            : base(NavigationCacheMode.Enabled)
        {
        }
    }

    public sealed class FlushableCachePage : CountingCachePage<FlushableCachePage>
    {
        public FlushableCachePage()
            : base(NavigationCacheMode.Required)
        {
        }
    }

    public abstract class CountingCachePage<TPage> : ModernWpf.Controls.Page
        where TPage : CountingCachePage<TPage>
    {
        protected CountingCachePage(NavigationCacheMode navigationCacheMode)
        {
            InstanceCount++;
            NavigationCacheMode = navigationCacheMode;
        }

        public static int InstanceCount { get; set; }
    }
}
