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
                Console.WriteLine($"Wrote {target.ReferenceFileBase}.png");
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
            Exit();
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
            var canvas = CreateTargetCanvas(target);
            _window.Content = canvas;

            var timer = _window.DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(options.DelayMilliseconds);
            timer.Tick += async (_, _) =>
            {
                timer.Stop();

                try
                {
                    var bitmap = new RenderTargetBitmap();
                    await bitmap.RenderAsync(canvas, target.Width, target.Height);
                    var pixels = await bitmap.GetPixelsAsync();
                    await WritePngAsync(
                        Path.Combine(options.ReferenceDirectory, target.ReferenceFileBase + ".png"),
                        target.Width,
                        target.Height,
                        pixels.ToArray());
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

    private static Canvas CreateTargetCanvas(CaptureTarget target)
    {
        var canvas = new Canvas
        {
            Width = target.Width,
            Height = target.Height,
            Background = new SolidColorBrush(Colors.White),
            RequestedTheme = ElementTheme.Light
        };

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
        canvas.Children.Add(caster);
        return canvas;
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

public sealed class CaptureOptions
{
    public string ManifestPath { get; private set; } = string.Empty;

    public string ReferenceDirectory { get; private set; } = string.Empty;

    public int DelayMilliseconds { get; private set; } = 250;

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
