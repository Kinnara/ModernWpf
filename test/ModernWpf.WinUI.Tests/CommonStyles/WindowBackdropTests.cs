using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class WindowBackdropTests
{
    [TestMethod]
    public void DefaultsAndValidationAreWpfShaped()
    {
        WpfTestHost.Run(() =>
        {
            var window = new Window();

            Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetKind(window));
            Assert.IsNull(WindowBackdrop.GetFallbackBrush(window));
            Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
            Assert.ThrowsExactly<ArgumentException>(
                () => WindowBackdrop.SetKind(window, (WindowBackdropKind)99));
            Assert.ThrowsExactly<ArgumentNullException>(() => WindowBackdrop.GetKind(null!));
            Assert.ThrowsExactly<ArgumentNullException>(
                () => WindowBackdrop.SetFallbackBrush(null!, Brushes.Black));

            var border = new Border();
            Assert.ThrowsExactly<InvalidOperationException>(
                () => border.SetValue(WindowBackdrop.KindProperty, WindowBackdropKind.Mica));
        });
    }

    [TestMethod]
    public void NativeBackdropMakesTheWindowTransparentAndRestoresItsBackground()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var platform = new FakeBackdropPlatform();
            WindowBackdrop.Platform = platform;
            var originalBackground = new SolidColorBrush(Color.FromRgb(12, 34, 56));
            var window = CreateWindow(originalBackground);

            try
            {
                WindowBackdrop.SetKind(window, WindowBackdropKind.Mica);

                Assert.AreEqual(WindowBackdropKind.Mica, WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(Brushes.Transparent, window.Background);
                CollectionAssert.AreEqual(
                    new[] { WindowBackdropKind.Mica },
                    platform.Requests.ToArray());

                WindowBackdrop.SetKind(window, WindowBackdropKind.DesktopAcrylic);

                Assert.AreEqual(
                    WindowBackdropKind.DesktopAcrylic,
                    WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(Brushes.Transparent, window.Background);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        WindowBackdropKind.Mica,
                        WindowBackdropKind.DesktopAcrylic
                    },
                    platform.Requests.ToArray());

                WindowBackdrop.SetKind(window, WindowBackdropKind.None);

                Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(originalBackground, window.Background);
                Assert.AreEqual(WindowBackdropKind.None, platform.Requests[^1]);
                CollectionAssert.AreEqual(new[] { true, false }, platform.FrameRequests.ToArray());
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
                WindowBackdrop.ResetPlatformForTests();
            }
        });
    }

    [TestMethod]
    public void UnsupportedCompositionHighContrastAndDwmFailureUseFallbackBrush()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var fallback = new SolidColorBrush(Color.FromRgb(90, 80, 70));

            AssertFallback(
                new FakeBackdropPlatform { IsSystemBackdropSupported = false },
                fallback);
            AssertFallback(
                new FakeBackdropPlatform { IsCompositionEnabled = false },
                fallback);
            AssertFallback(
                new FakeBackdropPlatform { IsHighContrast = true },
                fallback);
            AssertFallback(
                new FakeBackdropPlatform { SetSucceeds = false },
                fallback);

            WindowBackdrop.ResetPlatformForTests();
        });
    }

    [TestMethod]
    public void FramePreparationFailureAndAllowsTransparencyUseFallback()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var fallback = new SolidColorBrush(Color.FromRgb(90, 80, 70));
            AssertFallback(
                new FakeBackdropPlatform { ExtendFrameSucceeds = false },
                fallback);

            var platform = new FakeBackdropPlatform();
            WindowBackdrop.Platform = platform;
            var window = new Window
            {
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                Background = Brushes.CadetBlue
            };
            WindowBackdrop.SetFallbackBrush(window, fallback);

            try
            {
                WindowBackdrop.SetKind(window, WindowBackdropKind.Mica);

                Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(fallback, window.Background);
                Assert.AreEqual(0, platform.Requests.Count);
                Assert.AreEqual(0, platform.FrameRequests.Count);
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
                WindowBackdrop.ResetPlatformForTests();
            }
        });
    }

    [TestMethod]
    public void ExistingWindowChromeMustExposeTheCompleteGlassFrame()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var fallback = new SolidColorBrush(Color.FromRgb(45, 55, 65));

            var partialPlatform = new FakeBackdropPlatform();
            WindowBackdrop.Platform = partialPlatform;
            var partialWindow = CreateWindow(Brushes.CadetBlue);
            WindowChrome.SetWindowChrome(
                partialWindow,
                new WindowChrome { GlassFrameThickness = new Thickness(0) });
            WindowBackdrop.SetFallbackBrush(partialWindow, fallback);
            try
            {
                WindowBackdrop.SetKind(partialWindow, WindowBackdropKind.Mica);

                Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(partialWindow));
                Assert.AreSame(fallback, partialWindow.Background);
                Assert.AreEqual(0, partialPlatform.Requests.Count);
                Assert.AreEqual(0, partialPlatform.FrameRequests.Count);
            }
            finally
            {
                partialWindow.Close();
                WpfTestHost.DoEvents();
            }

            var completePlatform = new FakeBackdropPlatform();
            WindowBackdrop.Platform = completePlatform;
            var completeWindow = CreateWindow(Brushes.CadetBlue);
            WindowChrome.SetWindowChrome(
                completeWindow,
                new WindowChrome
                {
                    GlassFrameThickness = WindowChrome.GlassFrameCompleteThickness
                });
            try
            {
                WindowBackdrop.SetKind(completeWindow, WindowBackdropKind.DesktopAcrylic);

                Assert.AreEqual(
                    WindowBackdropKind.DesktopAcrylic,
                    WindowBackdrop.GetEffectiveKind(completeWindow));
                Assert.AreEqual(0, completePlatform.FrameRequests.Count);
            }
            finally
            {
                completeWindow.Close();
                WpfTestHost.DoEvents();
                WindowBackdrop.ResetPlatformForTests();
            }
        });
    }

    [TestMethod]
    public void ExternalBackgroundChangeIsPreservedWhenBackdropIsRemoved()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var platform = new FakeBackdropPlatform();
            WindowBackdrop.Platform = platform;
            var window = CreateWindow(Brushes.CadetBlue);
            var applicationBackground = new SolidColorBrush(Color.FromRgb(18, 28, 38));

            try
            {
                WindowBackdrop.SetKind(window, WindowBackdropKind.Mica);
                window.Background = applicationBackground;
                WindowBackdrop.SetKind(window, WindowBackdropKind.None);

                Assert.AreSame(applicationBackground, window.Background);
                Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
                WindowBackdrop.ResetPlatformForTests();
            }
        });
    }

    [TestMethod]
    public void RefreshTransitionsBetweenNativeAndFallbackStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var fallback = new SolidColorBrush(Color.FromRgb(30, 40, 50));
            var platform = new FakeBackdropPlatform();
            WindowBackdrop.Platform = platform;
            var window = CreateWindow(Brushes.CadetBlue);

            try
            {
                WindowBackdrop.SetFallbackBrush(window, fallback);
                WindowBackdrop.SetKind(window, WindowBackdropKind.Mica);
                Assert.AreEqual(WindowBackdropKind.Mica, WindowBackdrop.GetEffectiveKind(window));

                platform.IsHighContrast = true;
                WindowBackdrop.Refresh(window);

                Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(fallback, window.Background);
                Assert.AreEqual(WindowBackdropKind.None, platform.Requests[^1]);

                platform.IsHighContrast = false;
                WindowBackdrop.Refresh(window);

                Assert.AreEqual(WindowBackdropKind.Mica, WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(Brushes.Transparent, window.Background);
                Assert.AreEqual(WindowBackdropKind.Mica, platform.Requests[^1]);
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
                WindowBackdrop.ResetPlatformForTests();
            }
        });
    }

    [TestMethod]
    public void ResourceFallbackIsUsedWhenNoExplicitBrushIsSet()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var resourceBrush = new SolidColorBrush(Color.FromRgb(61, 71, 81));
            var platform = new FakeBackdropPlatform { IsSystemBackdropSupported = false };
            WindowBackdrop.Platform = platform;
            var window = CreateWindow(Brushes.Orange);
            window.Resources["WindowBackground"] = resourceBrush;

            try
            {
                WindowBackdrop.SetKind(window, WindowBackdropKind.DesktopAcrylic);

                Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
                Assert.AreSame(resourceBrush, window.Background);
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
                WindowBackdrop.ResetPlatformForTests();
            }
        });
    }

    private static void AssertFallback(FakeBackdropPlatform platform, Brush fallback)
    {
        WindowBackdrop.Platform = platform;
        var window = CreateWindow(Brushes.CadetBlue);

        try
        {
            WindowBackdrop.SetFallbackBrush(window, fallback);
            WindowBackdrop.SetKind(window, WindowBackdropKind.Mica);

            Assert.AreEqual(WindowBackdropKind.None, WindowBackdrop.GetEffectiveKind(window));
            Assert.AreSame(fallback, window.Background);
            if (platform.Requests.Count > 0)
            {
                Assert.AreEqual(WindowBackdropKind.None, platform.Requests[^1]);
            }
        }
        finally
        {
            window.Close();
            WpfTestHost.DoEvents();
        }
    }

    private static Window CreateWindow(Brush background)
    {
        var window = new Window
        {
            Width = 320,
            Height = 200,
            Left = -32000,
            Top = -32000,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Background = background,
            Content = new Border()
        };
        WindowChrome.SetWindowChrome(window, null);
        window.Show();
        WpfTestHost.DoEvents();
        return window;
    }

    private sealed class FakeBackdropPlatform : WindowBackdrop.IWindowBackdropPlatform
    {
        public bool IsSystemBackdropSupported { get; set; } = true;

        public bool IsCompositionEnabled { get; set; } = true;

        public bool IsHighContrast { get; set; }

        public bool SetSucceeds { get; set; } = true;

        public bool ExtendFrameSucceeds { get; set; } = true;

        public List<WindowBackdropKind> Requests { get; } = new();

        public List<bool> FrameRequests { get; } = new();

        public IntPtr GetWindowHandle(Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        public bool TrySetBackdrop(IntPtr windowHandle, WindowBackdropKind kind)
        {
            Assert.AreNotEqual(IntPtr.Zero, windowHandle);
            Requests.Add(kind);
            return SetSucceeds;
        }

        public bool TryExtendFrame(IntPtr windowHandle, bool enabled)
        {
            Assert.AreNotEqual(IntPtr.Zero, windowHandle);
            FrameRequests.Add(enabled);
            return ExtendFrameSucceeds;
        }
    }
}
