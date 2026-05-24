using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFGallery.Models;
using WPFGallery.Navigation;
using WPFGallery.ViewModels;
using WPFGallery.ViewModels.Layout;
using WPFGallery.ViewModels.Samples;
using WPFGallery.Views;

namespace OfficialWpfGalleryDirectHost;

internal static class Program
{
    // Keep source-matching random samples stable when comparing ModernWpf to this separate reference process.
    private const int ProductsVisualTestSeed = 12043;
    private const int BasicListViewVisualTestSeed = 22043;
    private const int GridViewVisualTestSeed = 22044;
    private const int UsersVisualTestSeed = 32043;

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
                DispatcherPriority.ApplicationIdle,
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
            "ColorText" => CreateColorPage("Text"),
            "ColorFill" => CreateColorPage("Fill"),
            "ColorStroke" => CreateColorPage("Stroke"),
            "ColorBackground" => CreateColorPage("Background"),
            "ColorSignal" => CreateColorPage("Signal"),
            "ColorHighContrast" => CreateColorPage("HighContrast"),
            "Typography" => new TypographyPage(new TypographyPageViewModel()),
            "Spacing" => new SpacingPage(new SpacingPageViewModel()),
            "Geometry" => new GeometryPage(new GeometryPageViewModel()),
            "Iconography" => new IconsPage(new IconsPageViewModel()),
            "Samples" => new SamplesPage(new SamplesPageViewModel(new NullNavigationService())),
            "UserDashboard" => CreateUserDashboardPage(),
            "BasicInput" => new BasicInputPage(new BasicInputPageViewModel(new NullNavigationService())),
            "Button" => new ButtonPage(new ButtonPageViewModel()),
            "CheckBox" => new CheckBoxPage(new CheckBoxPageViewModel()),
            "ComboBox" => new ComboBoxPage(new ComboBoxPageViewModel()),
            "RadioButton" => new RadioButtonPage(new RadioButtonPageViewModel()),
            "Slider" => new SliderPage(new SliderPageViewModel()),
            "Collections" => new CollectionsPage(new CollectionsPageViewModel(new NullNavigationService())),
            "DateAndCalendar" => new DateAndTimePage(new DateAndTimePageViewModel(new NullNavigationService())),
            "Calendar" => new CalendarPage(new CalendarPageViewModel()),
            "DatePicker" => new DatePickerPage(new DatePickerPageViewModel()),
            "DataGrid" => CreateDataGridPage(),
            "ListBox" => new ListBoxPage(new ListBoxPageViewModel()),
            "ListView" => CreateListViewPage(),
            "TreeView" => new TreeViewPage(new TreeViewPageViewModel()),
            "Layout" => new LayoutPage(new LayoutPageViewModel(new NullNavigationService())),
            "Media" => CreateMediaPage(),
            "Navigation" => new NavigationPage(new NavigationPageViewModel(new NullNavigationService())),
            "Menu" => new MenuPage(new MenuPageViewModel()),
            "TabControl" => new TabControlPage(new TabControlPageViewModel()),
            "Frame" => new FramePage(new FramePageViewModel()),
            "NavigationWindow" => new NavigationWindowPage(new NavigationWindowPageViewModel()),
            "Expander" => new ExpanderPage(new ExpanderPageViewModel()),
            "Grid" => new GridPage(new GridPageViewModel()),
            "ResizeGrip" => new ResizeGripPage(new ResizeGripPageViewModel()),
            "GridSplitter" => new GridSplitterPage(new GridSplitterPageViewModel()),
            "GroupBox" => new GroupBoxPage(new GroupBoxPageViewModel()),
            "StackPanel" => new StackPanelPage(new StackPanelPageViewModel()),
            "StatusAndInfo" => new StatusAndInfoPage(new StatusAndInfoPageViewModel(new NullNavigationService())),
            "ProgressBar" => new ProgressBarPage(new ProgressBarPageViewModel()),
            "ToolTip" => new ToolTipPage(new ToolTipPageViewModel()),
            "Text" => new TextPage(new TextPageViewModel(new NullNavigationService())),
            "Label" => new LabelPage(new LabelPageViewModel()),
            "TextBox" => new TextBoxPage(new TextBoxPageViewModel()),
            "TextBlock" => new TextBlockPage(new TextBlockPageViewModel()),
            "RichTextEdit" => new RichTextEditPage(new RichTextEditPageViewModel()),
            "PasswordBox" => new PasswordBoxPage(new PasswordBoxPageViewModel()),
            "Hyperlink" => new HyperlinkPage(new HyperlinkPageViewModel()),
            "Border" => new BorderPage(new BorderPageViewModel()),
            "System" => new SystemPage(new SystemPageViewModel(new NullNavigationService())),
            "FileAndFolderDialogs" => new FileAndFolderDialogsPage(new FileAndFolderDialogsPageViewModel()),
            "MessageBox" => new MessageBoxPage(new MessageBoxPageViewModel()),
            "Clipboard" => new ClipboardPage(new ClipboardPageViewModel()),
            "Canvas" => new CanvasPage(new CanvasPageViewModel()),
            "Image" => new ImagePage(new ImagePageViewModel()),
            _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Unsupported direct reference page.")
        };
    }

    private static ColorsPage CreateColorPage(string subpage)
    {
        var page = new ColorsPage(new ColorsPageViewModel());
        page.Loaded += (_, _) =>
        {
            page.Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => SelectColorSubpage(page, subpage)));
        };
        return page;
    }

    private static void SelectColorSubpage(Page page, string subpage)
    {
        if (FindColorSelector(page) is not { } selector)
        {
            return;
        }

        foreach (var item in selector.Items)
        {
            if (string.Equals(item as string, subpage, StringComparison.OrdinalIgnoreCase))
            {
                selector.SelectedItem = item;
                return;
            }
        }
    }

    private static ComboBox? FindColorSelector(DependencyObject root)
    {
        if (root is ComboBox comboBox &&
            (comboBox.Name == "PageSelector" ||
                AutomationProperties.GetName(comboBox) == "Page Selector"))
        {
            return comboBox;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < childCount; i++)
        {
            var result = FindColorSelector(VisualTreeHelper.GetChild(root, i));
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static DataGridPage CreateDataGridPage()
    {
        var viewModel = new DataGridPageViewModel
        {
            ProductsCollection = GenerateProducts(ProductsVisualTestSeed)
        };

        return new DataGridPage(viewModel);
    }

    private static ListViewPage CreateListViewPage()
    {
        var viewModel = new ListViewPageViewModel
        {
            BasicListViewItems = GeneratePersons(BasicListViewVisualTestSeed),
            GridViewItems = GeneratePersons(GridViewVisualTestSeed)
        };

        return new ListViewPage(viewModel);
    }

    private static UserDashboardPage CreateUserDashboardPage()
    {
        var viewModel = new UserDashboardPageViewModel
        {
            Users = GenerateUsers(UsersVisualTestSeed)
        };

        return new UserDashboardPage(viewModel);
    }

    private static ObservableCollection<Product> GenerateProducts(int seed)
    {
        var random = new StableSampleRandom(seed);
        var products = new ObservableCollection<Product>();
        var adjectives = new[] { "Red", "Blueberry" };
        var names = new[] { "Marmalade", "Dumplings", "Soup" };

        for (var i = 0; i < 50; i++)
        {
            products.Add(new Product
            {
                ProductId = i,
                ProductCode = i,
                ProductName = adjectives[random.Next(0, adjectives.Length)] + " " + names[random.Next(0, names.Length)],
                UnitPrice = Math.Round(random.NextDouble() * 20.0, 3),
                UnitsInStock = random.Next(0, 100)
            });
        }

        return products;
    }

    private static ObservableCollection<Person> GeneratePersons(int seed)
    {
        var random = new StableSampleRandom(seed);
        var persons = new ObservableCollection<Person>();
        var names = new[]
        {
            "John",
            "Winston",
            "Adrianna",
            "Spencer",
            "Phoebe",
            "Lucas",
            "Carl",
            "Marissa",
            "Brandon",
            "Antoine",
            "Arielle",
            "Arielle",
            "Jamie",
            "Alexander"
        };
        var surnames = new[]
        {
            "Doe",
            "Tapia",
            "Cisneros",
            "Lynch",
            "Munoz",
            "Marsh",
            "Hudson",
            "Bartlett",
            "Gregory",
            "Banks",
            "Hood",
            "Fry",
            "Carroll"
        };
        var companies = new[]
        {
            "Luminary Nexus",
            "CrestWave Dynamics",
            "Horizon Ventures",
            "Sapphire Pulse Technologies",
            "EmberLight Industries",
            "StellarEdge Ventrues",
            "Elysium Crest Holdings"
        };

        for (var i = 0; i < 50; i++)
        {
            persons.Add(new Person(
                names[random.Next(0, names.Length)],
                surnames[random.Next(0, surnames.Length)],
                companies[random.Next(0, companies.Length)]));
        }

        return persons;
    }

    private static ObservableCollection<User> GenerateUsers(int seed)
    {
        var random = new StableSampleRandom(seed);
        var users = new ObservableCollection<User>();

        var startDate = new DateTime(2020, 1, 1);
        var endDate = DateTime.Now.Date;
        var range = (endDate - startDate).Days;

        var imageids = new[]
        {
            "64", "65", "91", "103", "177", "334", "338", "342", "349", "366", "367", "373",
            "375", "378", "399", "447", "453", "473", "469", "505"
        };

        var names = new[]
        {
            "John",
            "Winston",
            "Adrianna",
            "Spencer",
            "Phoebe",
            "Lucas",
            "Carl",
            "Marissa",
            "Brandon",
            "Antoine",
            "Arielle"
        };

        var surnames = new[]
        {
            "Doe",
            "Tapia",
            "Cisneros",
            "Lynch",
            "Munoz",
            "Marsh",
            "Hudson",
            "Bartlett",
            "Gregory",
            "Banks",
            "Hood",
            "Fry",
            "Carroll"
        };

        var companies = new[]
        {
            "Luminary Nexus",
            "CrestWave Dynamics",
            "Horizon Ventures",
            "Sapphire Pulse Technologies",
            "EmberLight Industries",
            "StellarEdge Ventrues"
        };

        var addresses = new[]
        {
            "Room 1450, 9819 Rutledge Parkway, Saint Louis, Missouri, United States",
            "18th Floor, 3631 Manitowish Point, Mobile, Alabama, United States",
            "Apt 1145, Kansas, United States",
            "PO Box 54647, 252 Derek Way, Flushing, New York, United States",
            "Apt 687, 47182 Superior Avenue, Kansas City, Missouri, ",
            "20th Floor, 5524 Badeau Pass, Glendale, Arizona, United States",
            "Room 1121, 9 Kipling Terrace, Winston Salem, North Carolina, United States",
            "16th Floor, Odessa, Texas, United States",
            "Suite 82, 44 Shasta Terrace, Las Cruces, United States",
            "Room 1930, 45779 Anhalt Junction, Detroit, Michigan, United States",
            "PO Box 54206, 14 Waubesa Street, Greenville, South Carolina, United States",
            "1st Floor, 78 Barby Park, South Dakota, United States",
            "Room 1426, 7394 Welch Alley, Huntsville, Alabama, United States",
            "20th Floor, 11 Eastwood Road, El Paso, Texas, United States",
            "Suite 92, 9 Hermina Point, Bakersfield, United States",
            string.Empty
        };

        for (var i = 0; i < 20; i++)
        {
            var randomDays = random.Next(range + 1);
            users.Add(new User(
                imageids[random.Next(0, imageids.Length)],
                names[random.Next(0, names.Length)],
                surnames[random.Next(0, surnames.Length)],
                companies[random.Next(0, companies.Length)],
                addresses[random.Next(0, addresses.Length)],
                random.Next(21, 63),
                startDate.AddDays(randomDays),
                random.Next(2) == 1));
        }

        return users;
    }

    private static Page CreateMediaPage()
    {
        var viewModel = new MediaPageViewModel(new NullNavigationService())
        {
            NavigationCards = new List<ControlInfoDataItem>
            {
                new()
                {
                    UniqueId = "Canvas",
                    Title = "Canvas",
                    PageName = "CanvasPage",
                    ImagePath = "Assets/ControlImages/Canvas.png",
                    Description = "A layout panel that positions child elements by explicit coordinates."
                },
                new()
                {
                    UniqueId = "Image",
                    Title = "Image",
                    PageName = "ImagePage",
                    ImagePath = "Assets/ControlImages/Image.png",
                    Description = "A control that displays image content."
                }
            }
        };

        return new MediaPage(viewModel);
    }

    private sealed class StableSampleRandom
    {
        private uint _state;

        public StableSampleRandom(int seed)
        {
            _state = unchecked((uint)seed);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue));
            }

            return minValue + (int)(NextUInt32() % (uint)(maxValue - minValue));
        }

        public int Next(int maxValue)
        {
            if (maxValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue));
            }

            return (int)(NextUInt32() % (uint)maxValue);
        }

        public double NextDouble()
        {
            return NextUInt32() / ((double)uint.MaxValue + 1.0);
        }

        private uint NextUInt32()
        {
            _state = unchecked((_state * 1664525u) + 1013904223u);
            return _state;
        }
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
