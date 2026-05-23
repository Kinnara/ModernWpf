using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFGallery.Navigation;
using WPFGallery.ViewModels;
using WPFGallery.ViewModels.Layout;
using WPFGallery.ViewModels.Samples;
using WPFGallery.Views;

namespace OfficialWpfGalleryDirectHost;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            Run(args);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last-error.txt"), ex.ToString());
            throw;
        }
    }

    private static void Run(string[] args)
    {
        var options = HostOptions.Parse(args);
        AppDomain.CurrentDomain.AssemblyResolve += (_, eventArgs) => ResolveOfficialAssembly(options.OfficialOutput, eventArgs);
        var officialAssembly = LoadOfficialAssembly(options.OfficialOutput);
        SetApplicationResourceAssembly(officialAssembly);
        var app = CreateOfficialApplication(officialAssembly);
        app.ShutdownMode = ShutdownMode.OnMainWindowClose;
        ApplyTheme(app, options.Theme);

        var page = CreatePage(options.Page);

        var frame = new Frame
        {
            Name = "RootContentFrame",
            Content = page,
            Width = Math.Max(1, options.Width - 312),
            Height = Math.Max(1, options.Height - 62),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
        };
        AutomationProperties.SetAutomationId(frame, "RootContentFrame");

        var contentHost = new Border
        {
            Width = frame.Width,
            Height = frame.Height,
            Child = frame
        };
        contentHost.SetResourceReference(Control.BackgroundProperty, "SolidBackgroundFillColorTertiaryBrush");

        var window = new Window
        {
            Title = $"Official WPF Gallery Direct Reference - {options.Page}",
            Width = options.Width,
            Height = options.Height,
            Content = contentHost
        };
        window.SetResourceReference(Control.BackgroundProperty, "ApplicationPageBackgroundThemeBrush");
        window.Loaded += (_, _) =>
        {
            ApplyTheme(app, options.Theme);
            window.Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => WriteVisualArtifact(frame, options.ArtifactDirectory)));
        };

        app.MainWindow = window;
        ApplyTheme(app, options.Theme);
        app.Run(window);
    }

    private static Assembly LoadOfficialAssembly(string officialOutput)
    {
        return Assembly.LoadFrom(Path.Combine(officialOutput, "WPFGallery.dll"));
    }

    private static void SetApplicationResourceAssembly(Assembly officialAssembly)
    {
        var resourceAssemblyField = typeof(Application).GetField(
            "_resourceAssembly",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Could not find Application.ResourceAssembly backing field.");

        resourceAssemblyField.SetValue(null, officialAssembly);
    }

    private static Application CreateOfficialApplication(Assembly officialAssembly)
    {
        var appType = officialAssembly.GetType("WPFGallery.App", throwOnError: true)
            ?? throw new InvalidOperationException("Could not load WPFGallery.App.");
        var app = (Application?)Activator.CreateInstance(appType)
            ?? throw new InvalidOperationException("Could not create WPFGallery.App.");
        appType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance)
            ?.Invoke(app, null);
        return app;
    }

    private static Assembly? ResolveOfficialAssembly(string officialOutput, ResolveEventArgs eventArgs)
    {
        var assemblyName = new AssemblyName(eventArgs.Name).Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return null;
        }

        var assemblyPath = Path.Combine(officialOutput, assemblyName + ".dll");
        return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
    }

    private static void ApplyTheme(Application app, string theme)
    {
        ForceFluentThemeDictionary(app, theme);

        switch (theme)
        {
            case "Light":
                app.ThemeMode = ThemeMode.Light;
                break;
            case "Dark":
                app.ThemeMode = ThemeMode.Dark;
                break;
            case "Default":
                app.ThemeMode = ThemeMode.System;
                break;
        }
    }

    private static void ForceFluentThemeDictionary(Application app, string theme)
    {
        var forcedDictionaries = app.Resources.MergedDictionaries
            .Where(dictionary => dictionary.Source?.OriginalString.Contains(
                "PresentationFramework.Fluent;component/Themes/Fluent.",
                StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        foreach (var dictionary in forcedDictionaries)
        {
            app.Resources.MergedDictionaries.Remove(dictionary);
        }

        var themeFileName = theme switch
        {
            "Light" => "Fluent.Light.xaml",
            "Dark" => "Fluent.Dark.xaml",
            _ => ""
        };

        if (!string.IsNullOrEmpty(themeFileName))
        {
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(
                    $"pack://application:,,,/PresentationFramework.Fluent;component/Themes/{themeFileName}",
                    UriKind.Absolute)
            });
        }
    }

    private static Page CreatePage(string page)
    {
        return page switch
        {
            "WhatsNew" => new WhatsNewPage(new WhatsNewPageViewModel(new NullNavigationService())),
            "AllControls" => new AllSamplesPage(new AllSamplesPageViewModel(new NullNavigationService())),
            "DesignGuidance" => new DesignGuidancePage(new DesignGuidancePageViewModel(new NullNavigationService())),
            "Color" => new ColorsPage(new ColorsPageViewModel()),
            "Typography" => new TypographyPage(new TypographyPageViewModel()),
            "Spacing" => new SpacingPage(new SpacingPageViewModel()),
            "Geometry" => new GeometryPage(new GeometryPageViewModel()),
            "Iconography" => new IconsPage(new IconsPageViewModel()),
            "UserDashboard" => new UserDashboardPage(new UserDashboardPageViewModel()),
            "TextBlock" => new TextBlockPage(new TextBlockPageViewModel()),
            "Border" => new BorderPage(new BorderPageViewModel()),
            "Canvas" => new CanvasPage(new CanvasPageViewModel()),
            "Image" => new ImagePage(new ImagePageViewModel()),
            _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Unsupported direct reference page.")
        };
    }

    private static void WriteVisualArtifact(FrameworkElement element, string artifactDirectory)
    {
        if (string.IsNullOrWhiteSpace(artifactDirectory))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(artifactDirectory);
            WriteElementPng(element, Path.Combine(artifactDirectory, "RootContentFrame.png"));
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "last-artifact-error.txt"), ex.ToString());
        }
    }

    private static void WriteElementPng(FrameworkElement element, string path)
    {
        element.UpdateLayout();
        var width = (int)Math.Ceiling(element.ActualWidth);
        var height = (int)Math.Ceiling(element.ActualHeight);
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var drawingVisual = new DrawingVisual();
        var visualBrush = new VisualBrush(element)
        {
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            Stretch = Stretch.None,
            Viewbox = new Rect(0, 0, width, height),
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0, 0, width, height),
            ViewportUnits = BrushMappingMode.Absolute
        };

        using (var drawingContext = drawingVisual.RenderOpen())
        {
            drawingContext.DrawRectangle(
                GetElementBackground(element),
                null,
                new Rect(0, 0, width, height));
            drawingContext.DrawRectangle(
                visualBrush,
                null,
                new Rect(0, 0, width, height));
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(drawingVisual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static Brush GetElementBackground(FrameworkElement element)
    {
        return element.TryFindResource("SolidBackgroundFillColorTertiaryBrush") as Brush
            ?? element.TryFindResource("LayerFillColorDefaultBrush") as Brush
            ?? element.TryFindResource("ApplicationPageBackgroundThemeBrush") as Brush
            ?? Brushes.White;
    }

    private sealed class NullNavigationService : INavigationService
    {
        public event EventHandler<NavigatingEventArgs>? Navigating;

        public void Navigate(Type type, bool adjustFocus = true)
        {
            Navigating?.Invoke(this, new NavigatingEventArgs(type));
        }

        public void NavigateTo(Type type)
        {
            Navigating?.Invoke(this, new NavigatingEventArgs(type));
        }

        public void SetFrame(Frame frame)
        {
        }

        public void NavigateBack()
        {
        }

        public void NavigateForward()
        {
        }

        public bool IsBackHistoryNonEmpty()
        {
            return false;
        }
    }

    private sealed record HostOptions(
        string Page,
        string Theme,
        string OfficialOutput,
        int Width,
        int Height,
        string ArtifactDirectory)
    {
        public static HostOptions Parse(string[] args)
        {
            var page = "";
            var theme = "Light";
            var officialOutput = @"D:\repos\WPF-Samples\Sample Applications\WPFGallery\bin\Debug\net10.0-windows";
            var width = 1180;
            var height = 820;
            var artifactDirectory = "";

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var value = i + 1 < args.Length ? args[i + 1] : "";
                switch (arg)
                {
                    case "--page":
                        page = value;
                        i++;
                        break;
                    case "--theme":
                        theme = value;
                        i++;
                        break;
                    case "--official-output":
                        officialOutput = value;
                        i++;
                        break;
                    case "--width":
                        width = int.Parse(value);
                        i++;
                        break;
                    case "--height":
                        height = int.Parse(value);
                        i++;
                        break;
                    case "--visual-artifact-dir":
                        artifactDirectory = value;
                        i++;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(page))
            {
                throw new ArgumentException("--page is required.");
            }

            return new HostOptions(page, theme, officialOutput, width, height, artifactDirectory);
        }
    }
}
