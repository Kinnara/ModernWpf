using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace WinUIThemeShadowCapture;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(_ => new CaptureApp(args));
    }
}

public sealed class CaptureApp : Application
{
    private readonly string[] _args;
    private Window? _window;

    public CaptureApp(string[] args)
    {
        _args = args;
        UnhandledException += OnUnhandledException;
    }

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        Console.Error.WriteLine(args.Exception);
        args.Handled = true;
        Environment.ExitCode = 1;
        Environment.Exit(Environment.ExitCode);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new Window();
        _window.Activate();
        _ = RunAsync();
    }

    private async Task RunAsync()
    {
        try
        {
            var options = CaptureOptions.Parse(_args);
            var targets = LoadTargets(options.ManifestPath, options.ReferenceDirectory);
            var capturedCount = 0;

            foreach (var target in targets)
            {
                if (options.Targets.Count > 0 &&
                    !options.Targets.Contains(target.ReferenceFileBase) &&
                    !options.Targets.Contains(target.Name))
                {
                    continue;
                }

                await CaptureTargetAsync(target, options);
                capturedCount++;
                Console.WriteLine($"Wrote {target.ReferenceFileBase}.png ({FormatCaptureMode(options.CaptureMode)})");
            }

            if (capturedCount == 0)
            {
                throw new InvalidOperationException("No ThemeShadow reference capture targets matched the requested filter.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            Environment.Exit(Environment.ExitCode);
        }
    }

    private Task CaptureTargetAsync(CaptureTarget target, CaptureOptions options)
    {
        if (_window is null)
        {
            throw new InvalidOperationException("The WinUI window has not been created.");
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var enqueued = _window.DispatcherQueue.TryEnqueue(() =>
        {
            CaptureVisual visual;
            try
            {
                visual = CreateTargetVisual(target, options.CaptureMode);
                _window.Content = visual.Canvas;
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                return;
            }

            var timer = _window.DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(options.DelayMilliseconds);
            timer.Tick += async (_, _) =>
            {
                timer.Stop();

                try
                {
                    var targetElement = visual.TargetElement;
                    if (visual.Prepare is not null)
                    {
                        targetElement = visual.Prepare();
                        visual.Canvas.UpdateLayout();
                    }

                    var bitmap = new RenderTargetBitmap();
                    await bitmap.RenderAsync(visual.Canvas, target.Width, target.Height);
                    var pixels = await bitmap.GetPixelsAsync();
                    await WritePngAsync(
                        Path.Combine(options.ReferenceDirectory, target.ReferenceFileBase + ".png"),
                        target.Width,
                        target.Height,
                        pixels.ToArray());
                    WriteCaptureMetadata(
                        Path.Combine(options.ReferenceDirectory, target.ReferenceFileBase + ".capture.txt"),
                        target,
                        options.CaptureMode,
                        targetElement);
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };
            timer.Start();
        });

        if (!enqueued)
        {
            throw new InvalidOperationException("Could not enqueue ThemeShadow capture on the WinUI dispatcher.");
        }

        return tcs.Task;
    }

    private static CaptureVisual CreateTargetVisual(CaptureTarget target, ReferenceCaptureMode captureMode)
    {
        var canvas = new Canvas
        {
            Width = target.Width,
            Height = target.Height,
            Background = new SolidColorBrush(Colors.White),
            RequestedTheme = ElementTheme.Light
        };

        var targetElement = captureMode switch
        {
            ReferenceCaptureMode.SourceGeometry => CreateSourceGeometryCaster(target),
            ReferenceCaptureMode.ActualControl => CreateActualControlTarget(target),
            _ => throw new ArgumentOutOfRangeException(nameof(captureMode), captureMode, null)
        };

        canvas.Children.Add(targetElement.Element);
        return new CaptureVisual(canvas, targetElement.Description, targetElement.Prepare);
    }

    private static CaptureTargetElement CreateSourceGeometryCaster(CaptureTarget target)
    {
        var caster = new Border
        {
            Width = target.IgnoredBounds.Width,
            Height = target.IgnoredBounds.Height,
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            CornerRadius = new CornerRadius(target.CornerRadius),
            Shadow = new ThemeShadow(),
            Translation = new Vector3(0, 0, target.Depth)
        };

        Canvas.SetLeft(caster, target.IgnoredBounds.X);
        Canvas.SetTop(caster, target.IgnoredBounds.Y);
        return new CaptureTargetElement(caster, "Microsoft.UI.Xaml.Controls.Border source-geometry caster");
    }

    private static CaptureTargetElement CreateActualControlTarget(CaptureTarget target)
    {
        return target.ReferenceFileBase switch
        {
            "FlyoutPresenter-shadow-only" => CreateActualFlyoutPresenterTarget(target),
            "CommandBar-overflow-popup-shadow-only" => CreateActualCommandBarOverflowTarget(target),
            "MenuFlyoutPresenter-shadow-only" => CreateActualMenuFlyoutPresenterTarget(target),
            "ContentDialog-background-shadow-shadow-only" => CreateActualContentDialogBackgroundTarget(target),
            _ => throw new NotSupportedException(
                $"Actual-control capture is not implemented for {target.ReferenceFileBase}. " +
                "Use --capture-mode source-geometry for the current all-target reference path.")
        };
    }

    private static CaptureTargetElement CreateActualFlyoutPresenterTarget(CaptureTarget target)
    {
        var presenter = new FlyoutPresenter
        {
            Content = new Border
            {
                Width = target.IgnoredBounds.Width,
                Height = target.IgnoredBounds.Height,
                Background = new SolidColorBrush(Colors.White)
            },
            Background = new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(),
            Padding = new Thickness(),
            CornerRadius = new CornerRadius(target.CornerRadius),
            IsDefaultShadowEnabled = true,
            MinWidth = 0,
            MinHeight = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        Canvas.SetLeft(presenter, target.IgnoredBounds.X);
        Canvas.SetTop(presenter, target.IgnoredBounds.Y);
        return new CaptureTargetElement(presenter, "Microsoft.UI.Xaml.Controls.FlyoutPresenter actual control");
    }

    private static CaptureTargetElement CreateActualCommandBarOverflowTarget(CaptureTarget target)
    {
        var commandBar = new CommandBar
        {
            Width = 220,
            IsOpen = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Share" });

        return CreateActualTemplatePartTarget(
            target,
            commandBar,
            "SecondaryItemsControlShadowWrapper",
            "Microsoft.UI.Xaml.Controls.CommandBar actual SecondaryItemsControlShadowWrapper template part");
    }

    private static CaptureTargetElement CreateActualMenuFlyoutPresenterTarget(CaptureTarget target)
    {
        var presenter = new MenuFlyoutPresenter
        {
            Width = target.IgnoredBounds.Width,
            Height = target.IgnoredBounds.Height,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(),
            Background = new SolidColorBrush(Colors.Transparent),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(),
            CornerRadius = new CornerRadius(target.CornerRadius),
            IsDefaultShadowEnabled = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        presenter.Items.Add(new MenuFlyoutItem { Text = "Copy" });

        Canvas.SetLeft(presenter, target.IgnoredBounds.X);
        Canvas.SetTop(presenter, target.IgnoredBounds.Y);
        return new CaptureTargetElement(presenter, "Microsoft.UI.Xaml.Controls.MenuFlyoutPresenter actual control");
    }

    private static CaptureTargetElement CreateActualContentDialogBackgroundTarget(CaptureTarget target)
    {
        var contentDialog = new ContentDialog
        {
            Title = "Dialog",
            Content = "Dialog content",
            PrimaryButtonText = "OK",
            Width = 240,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        return CreateActualTemplatePartTarget(
            target,
            contentDialog,
            "BackgroundElement",
            "Microsoft.UI.Xaml.Controls.ContentDialog actual BackgroundElement template part",
            isChildlessTarget: true);
    }

    private static CaptureTargetElement CreateActualTemplatePartTarget(
        CaptureTarget target,
        FrameworkElement owner,
        string partName,
        string description,
        bool isChildlessTarget = false)
    {
        PrepareOwner(owner, target);
        return new CaptureTargetElement(
            owner,
            $"{owner.GetType().FullName} actual control host for {partName}",
            () =>
            {
                owner.UpdateLayout();
                var part = FindNamedDescendant(owner, partName)
                    ?? throw new InvalidOperationException($"Could not find WinUI template part '{partName}' in {owner.GetType().FullName}.");

                return ExtractPartIntoManifestCanvas(target, owner, part, description, isChildlessTarget);
            });
    }

    private static void PrepareOwner(FrameworkElement owner, CaptureTarget target)
    {
        owner.RequestedTheme = ElementTheme.Light;
        owner.MinWidth = 0;
        owner.MinHeight = 0;
        owner.MaxWidth = double.PositiveInfinity;
        owner.MaxHeight = double.PositiveInfinity;
        owner.Margin = new Thickness();
        owner.HorizontalAlignment = HorizontalAlignment.Left;
        owner.VerticalAlignment = VerticalAlignment.Top;
        Canvas.SetLeft(owner, 0);
        Canvas.SetTop(owner, 0);

        if (double.IsNaN(owner.Width))
        {
            owner.Width = Math.Max(target.Width, target.IgnoredBounds.X + target.IgnoredBounds.Width);
        }

        if (double.IsNaN(owner.Height))
        {
            owner.Height = Math.Max(target.Height, target.IgnoredBounds.Y + target.IgnoredBounds.Height);
        }
    }

    private static string ExtractPartIntoManifestCanvas(
        CaptureTarget target,
        FrameworkElement owner,
        FrameworkElement part,
        string description,
        bool isChildlessTarget)
    {
        DetachFromParent(part);
        owner.Visibility = Visibility.Collapsed;

        NormalizeExtractedPart(part, target, isChildlessTarget);
        var canvas = owner.Parent as Canvas
            ?? throw new InvalidOperationException("The WinUI actual-control host is not parented to the capture canvas.");

        canvas.Children.Clear();
        canvas.Children.Add(part);
        return description;
    }

    private static void NormalizeExtractedPart(FrameworkElement element, CaptureTarget target, bool isChildlessTarget)
    {
        element.MinWidth = 0;
        element.MinHeight = 0;
        element.MaxWidth = double.PositiveInfinity;
        element.MaxHeight = double.PositiveInfinity;
        element.Margin = new Thickness();
        if (element is Control control)
        {
            control.Padding = new Thickness();
        }
        element.HorizontalAlignment = HorizontalAlignment.Left;
        element.VerticalAlignment = VerticalAlignment.Top;
        element.Opacity = 1;
        element.RenderTransform = null;

        if (isChildlessTarget)
        {
            element.Width = target.IgnoredBounds.Width;
            element.Height = target.IgnoredBounds.Height;
        }
        else
        {
            element.Width = Math.Max(element.ActualWidth, target.IgnoredBounds.Width);
            element.Height = Math.Max(element.ActualHeight, target.IgnoredBounds.Height);
        }

        Canvas.SetLeft(element, target.IgnoredBounds.X);
        Canvas.SetTop(element, target.IgnoredBounds.Y);
    }

    private static FrameworkElement? FindNamedDescendant(DependencyObject root, string name)
    {
        if (root is FrameworkElement frameworkElement && frameworkElement.Name == name)
        {
            return frameworkElement;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var result = FindNamedDescendant(VisualTreeHelper.GetChild(root, i), name);
            if (result is not null)
            {
                return result;
            }
        }

        if (root is Popup popup && popup.Child is not null)
        {
            return FindNamedDescendant(popup.Child, name);
        }

        return null;
    }

    private static void DetachFromParent(FrameworkElement element)
    {
        if (element.Parent is Panel panel)
        {
            panel.Children.Remove(element);
            return;
        }

        if (element.Parent is Border border)
        {
            border.Child = null;
            return;
        }

        if (element.Parent is ContentControl contentControl && ReferenceEquals(contentControl.Content, element))
        {
            contentControl.Content = null;
            return;
        }

        if (element.Parent is ContentPresenter contentPresenter && ReferenceEquals(contentPresenter.Content, element))
        {
            contentPresenter.Content = null;
        }
    }

    private static async Task WritePngAsync(string path, int width, int height, byte[] pixels)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
        using var randomAccessStream = stream.AsRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, randomAccessStream);
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)width,
            (uint)height,
            96,
            96,
            pixels);
        await encoder.FlushAsync();
        await randomAccessStream.FlushAsync();
    }

    private static void WriteCaptureMetadata(string path, CaptureTarget target, ReferenceCaptureMode captureMode, string targetElement)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllLines(
            path,
            new[]
            {
                $"Name={target.Name}",
                $"ReferenceFileBase={target.ReferenceFileBase}",
                $"CaptureMode={FormatCaptureMode(captureMode)}",
                $"TargetElement={targetElement}",
                "RenderPath=WinUI RenderTargetBitmap",
                $"Canvas={target.Width}x{target.Height}",
                $"IgnoredBounds={target.IgnoredBounds.X},{target.IgnoredBounds.Y},{target.IgnoredBounds.Width},{target.IgnoredBounds.Height}",
                $"RequestedDepth={target.Depth.ToString(CultureInfo.InvariantCulture)}",
                $"CornerRadius={target.CornerRadius.ToString(CultureInfo.InvariantCulture)}"
            });
    }

    private static IReadOnlyList<CaptureTarget> LoadTargets(string manifestPath, string referenceDirectory)
    {
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("ThemeShadow reference capture manifest was not found.", manifestPath);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var targets = new List<CaptureTarget>();

        foreach (var element in document.RootElement.GetProperty("targets").EnumerateArray())
        {
            var name = element.GetProperty("name").GetString() ?? string.Empty;
            var fileBase = element.GetProperty("referenceFileBase").GetString() ?? string.Empty;
            var canvasSize = element.GetProperty("canvasSize");
            var width = canvasSize.GetProperty("width").GetInt32();
            var height = canvasSize.GetProperty("height").GetInt32();
            var mask = ReadMask(Path.Combine(referenceDirectory, fileBase + ".mask.txt"), name, width, height);
            targets.Add(new CaptureTarget(
                name,
                fileBase,
                width,
                height,
                mask,
                GetDepth(fileBase),
                GetCornerRadius(fileBase)));
        }

        return targets;
    }

    private static Int32Rect ReadMask(string path, string expectedName, int expectedWidth, int expectedHeight)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("ThemeShadow mask sidecar is required before WinUI reference capture. Run Prepare-ThemeShadowReferenceCaptures.ps1 first.", path);
        }

        var values = File.ReadLines(path)
            .Select(line => line.Split(new[] { '=' }, 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);

        if (!values.TryGetValue("Name", out var actualName) || actualName != expectedName)
        {
            throw new InvalidOperationException($"{path} should have Name={expectedName}.");
        }

        if (!values.TryGetValue("Size", out var actualSize) || actualSize != $"{expectedWidth}x{expectedHeight}")
        {
            throw new InvalidOperationException($"{path} should have Size={expectedWidth}x{expectedHeight}.");
        }

        if (!values.TryGetValue("IgnoredBounds", out var boundsText))
        {
            throw new InvalidOperationException($"{path} is missing IgnoredBounds.");
        }

        var parts = boundsText.Split(',');
        if (parts.Length != 4 ||
            !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) ||
            !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) ||
            !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
        {
            throw new InvalidOperationException($"{path} has invalid IgnoredBounds={boundsText}.");
        }

        return new Int32Rect(x, y, width, height);
    }

    private static float GetDepth(string fileBase)
    {
        return fileBase switch
        {
            "NumberBox-compact-popup-shadow-only" => 16,
            "ContentDialog-background-shadow-shadow-only" => 128,
            "NavigationView-pane-overlay-shadow-shadow-only" => 16,
            _ => 32
        };
    }

    private static double GetCornerRadius(string fileBase)
    {
        return fileBase switch
        {
            "NavigationView-pane-overlay-shadow-shadow-only" => 0,
            _ => 4
        };
    }

    private static string FormatCaptureMode(ReferenceCaptureMode captureMode)
    {
        return captureMode switch
        {
            ReferenceCaptureMode.SourceGeometry => "source-geometry",
            ReferenceCaptureMode.ActualControl => "actual-control",
            _ => throw new ArgumentOutOfRangeException(nameof(captureMode), captureMode, null)
        };
    }
}

public sealed record CaptureTarget(
    string Name,
    string ReferenceFileBase,
    int Width,
    int Height,
    Int32Rect IgnoredBounds,
    float Depth,
    double CornerRadius);

public sealed record Int32Rect(int X, int Y, int Width, int Height);

public sealed record CaptureVisual(Canvas Canvas, string TargetElement, Func<string>? Prepare = null);

public sealed record CaptureTargetElement(UIElement Element, string Description, Func<string>? Prepare = null);

public enum ReferenceCaptureMode
{
    SourceGeometry,
    ActualControl
}

public sealed class CaptureOptions
{
    public string ManifestPath { get; private set; } = string.Empty;

    public string ReferenceDirectory { get; private set; } = string.Empty;

    public int DelayMilliseconds { get; private set; } = 250;

    public ReferenceCaptureMode CaptureMode { get; private set; } = ReferenceCaptureMode.SourceGeometry;

    public HashSet<string> Targets { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static CaptureOptions Parse(string[] args)
    {
        var repoRoot = FindRepoRoot();
        var options = new CaptureOptions
        {
            ManifestPath = Path.Combine(repoRoot, "docs", "theme-shadow-reference-captures.json")
        };

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--manifest":
                    options.ManifestPath = RequireValue(args, ref i);
                    break;
                case "--reference-dir":
                    options.ReferenceDirectory = RequireValue(args, ref i);
                    break;
                case "--delay-ms":
                    options.DelayMilliseconds = RequireIntValue(args, ref i);
                    break;
                case "--capture-mode":
                    options.CaptureMode = ParseCaptureMode(RequireValue(args, ref i));
                    break;
                case "--target":
                    options.Targets.Add(RequireValue(args, ref i));
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[i]}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.ReferenceDirectory))
        {
            throw new ArgumentException("--reference-dir is required.");
        }

        options.ManifestPath = Path.GetFullPath(options.ManifestPath);
        options.ReferenceDirectory = Path.GetFullPath(options.ReferenceDirectory);
        Directory.CreateDirectory(options.ReferenceDirectory);
        return options;
    }

    private static string RequireValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{args[index]} requires a value.");
        }

        index++;
        return args[index];
    }

    private static int RequireIntValue(string[] args, ref int index)
    {
        var value = RequireValue(args, ref index);
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result < 0)
        {
            throw new ArgumentException($"Expected a non-negative integer, actual '{value}'.");
        }

        return result;
    }

    private static ReferenceCaptureMode ParseCaptureMode(string value)
    {
        return value switch
        {
            "source-geometry" => ReferenceCaptureMode.SourceGeometry,
            "actual-control" => ReferenceCaptureMode.ActualControl,
            _ => throw new ArgumentException($"Unknown capture mode '{value}'. Expected source-geometry or actual-control.")
        };
    }

    private static string FindRepoRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            var candidate = Path.Combine(directory, "docs", "theme-shadow-reference-captures.json");
            if (File.Exists(candidate))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName ?? string.Empty;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", ".."));
    }
}
