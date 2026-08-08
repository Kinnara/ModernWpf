using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class WindowBackdropSourceAuditTests
{
    [TestMethod]
    public void OfficialWpfAndWindowsBackdropContractIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "window-backdrop-wpf-source-audit.md");
        var implementation = Read(repoRoot, "ModernWpf", "Window", "WindowBackdrop.cs");
        var kind = Read(repoRoot, "ModernWpf", "Window", "WindowBackdropKind.cs");
        var osVersion = Read(repoRoot, "ModernWpf", "Helpers", "OSVersionHelper.cs");
        var publicApi = Read(repoRoot, "ModernWpf", "PublicAPI.Unshipped.txt");
        var tests = Read(
            repoRoot,
            "test",
            "ModernWpf.WinUI.Tests",
            "CommonStyles",
            "WindowBackdropTests.cs");
        var galleryFactory = Read(
            repoRoot,
            "ModernWpf.Gallery",
            "Pages",
            "WindowingSampleFactory.cs");
        var catalog = Read(
            repoRoot,
            "ModernWpf.Gallery",
            "Samples",
            "Data",
            "ControlInfoData.json");
        var visualHarness = Read(
            repoRoot,
            "tools",
            "visual-checks",
            "Run-GalleryVisualChecks.ps1");
        var interactionHarness = Read(
            repoRoot,
            "tools",
            "visual-checks",
            "Record-GalleryControlInteractions.ps1");

        StringAssert.Contains(audit, "7f005faa89e79b0b1fa1cb2c21283bab7916c092");
        StringAssert.Contains(audit, "d28ae573fc2ef5af2e03a768ca1017ad0f647ffb");
        StringAssert.Contains(audit, "3204ce020ec702a1d1fd77ee31e09e1602b660f0");
        StringAssert.Contains(audit, "aedc66a4e76a260a2158528fa51f9cc87ae58d99");
        StringAssert.Contains(audit, "8497ff393dbb568b73f7d1976bcf874fe8dda11a");
        StringAssert.Contains(audit, "DWMWA_SYSTEMBACKDROP_TYPE = 38");
        StringAssert.Contains(audit, "DWMSBT_MAINWINDOW = 2");
        StringAssert.Contains(audit, "DWMSBT_TRANSIENTWINDOW = 3");
        StringAssert.Contains(audit, "Windows 11 build 22621");
        StringAssert.Contains(audit, "real OS High Contrast");
        StringAssert.Contains(audit, "final clean tip");

        StringAssert.Contains(kind, "Mica = 1");
        StringAssert.Contains(kind, "DesktopAcrylic = 2");
        StringAssert.Contains(implementation, "public static readonly DependencyProperty KindProperty");
        StringAssert.Contains(implementation, "RegisterAttachedReadOnly(");
        StringAssert.Contains(implementation, "public static WindowBackdropKind GetEffectiveKind(Window window)");
        StringAssert.Contains(implementation, "!platform.IsHighContrast");
        StringAssert.Contains(implementation, "!_window.AllowsTransparency");
        StringAssert.Contains(implementation, "PrepareCompositionSurface(handle)");
        StringAssert.Contains(implementation, "compositionTarget.BackgroundColor = Colors.Transparent;");
        StringAssert.Contains(implementation, "windowChrome.GlassFrameThickness != WindowChrome.GlassFrameCompleteThickness");
        StringAssert.Contains(implementation, "if (windowChrome == null && !_ownsExtendedFrame)");
        StringAssert.Contains(implementation, "platform.TryExtendFrame(handle, true)");
        StringAssert.Contains(implementation, "platform.TryExtendFrame(handle, false)");
        StringAssert.Contains(implementation, "private const int DwmwaSystemBackdropType = 38;");
        StringAssert.Contains(implementation, "private const int DwmSystemBackdropMainWindow = 2;");
        StringAssert.Contains(implementation, "private const int DwmSystemBackdropTransientWindow = 3;");
        StringAssert.Contains(implementation, "DwmExtendFrameIntoClientArea(");
        StringAssert.Contains(implementation, "ResolveFallbackBrush()");
        StringAssert.Contains(implementation, "nameof(SystemParameters.HighContrast)");
        StringAssert.Contains(implementation, "CaptureExternalBackgroundChange()");
        StringAssert.Contains(implementation, "if (!_isListeningForSystemParameters)");
        StringAssert.Contains(implementation, "if (_isListeningForSystemParameters)");
        StringAssert.Contains(osVersion, "new Version(10, 0, 22621)");

        StringAssert.Contains(publicApi, "ModernWpf.Controls.WindowBackdrop");
        StringAssert.Contains(publicApi, "ModernWpf.Controls.WindowBackdropKind.DesktopAcrylic = 2");
        StringAssert.Contains(publicApi, "~static ModernWpf.Controls.WindowBackdrop.GetEffectiveKind(System.Windows.Window window)");
        Assert.IsFalse(publicApi.Contains("MicaAlt", StringComparison.Ordinal));

        StringAssert.Contains(tests, "NativeBackdropMakesTheWindowTransparentAndRestoresItsBackground");
        StringAssert.Contains(tests, "UnsupportedCompositionHighContrastAndDwmFailureUseFallbackBrush");
        StringAssert.Contains(tests, "FramePreparationFailureAndAllowsTransparencyUseFallback");
        StringAssert.Contains(tests, "ExistingWindowChromeMustExposeTheCompleteGlassFrame");
        StringAssert.Contains(tests, "ExternalBackgroundChangeIsPreservedWhenBackdropIsRemoved");

        StringAssert.Contains(galleryFactory, "case \"SystemBackdrop\"");
        StringAssert.Contains(galleryFactory, "Open Mica window");
        StringAssert.Contains(galleryFactory, "Open Desktop Acrylic window");
        StringAssert.Contains(galleryFactory, "GalleryAutomation.SampleElementId(\"SystemBackdrop\", kind + \"Window\")");
        StringAssert.Contains(galleryFactory, "Mux.WindowBackdrop.GetEffectiveKind(window)");
        StringAssert.Contains(catalog, "\"UniqueId\": \"SystemBackdrop\"");
        StringAssert.Contains(visualHarness, "SystemBackdrop = 1");
        StringAssert.Contains(visualHarness, "GallerySample_SystemBackdrop_Root");
        StringAssert.Contains(interactionHarness, "GallerySample_SystemBackdrop_MicaButton");
    }

    private static string Read(string repoRoot, params string[] parts)
    {
        var path = repoRoot;
        foreach (var part in parts)
        {
            path = Path.Combine(path, part);
        }

        return File.ReadAllText(path);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
