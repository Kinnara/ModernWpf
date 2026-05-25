using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class WindowingSampleFactory
    {
        private const string CreateMultipleWindowsCSharp =
@"// Ensure you close the child window before closing the parent window to avoid application crash.
var childWindow = new Window()
{
    ExtendsContentIntoTitleBar = true,
    SystemBackdrop = new MicaBackdrop(),
    Content = new Page()
    {
        Content = new TextBlock()
        {
            Text = ""New child window!"",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        },
        // Get the theme from the parent.
        RequestedTheme = this.ActualTheme,
    }
};

childWindow.AppWindow.ResizeClient(new SizeInt32(500, 500));
childWindow.Activate();";

        private const string AppWindowSample1Xaml =
@"<Window ...>
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <StackPanel HorizontalAlignment=""Center""
                VerticalAlignment=""Center""
                Spacing=""8"">
        <Button x:Name=""Hide""
                Content=""Hide""
                Click=""Hide_Click""
                Width=""200"" />
        <Button x:Name=""Show""
                Click=""Show_Click""
                Width=""200"" >
            <TextBlock Text=""Hide and show the window after 3 seconds"" TextWrapping=""WrapWholeWords"" TextAlignment=""Center""/>
        </Button>
        <Button x:Name=""Close""
                Click=""Close_Click""
                Width=""200""
                Margin=""0,16,0,0"">
            <StackPanel Orientation=""Horizontal"" VerticalAlignment=""Center"">
                <SymbolIcon Symbol=""Cancel"" />
                <TextBlock Text=""Close"" />
            </StackPanel>
        </Button>
    </StackPanel>
</Window>";

        private const string AppWindowSample1CSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace YourNamespace;

public sealed partial class SampleWindow1 : Window
{
    public SampleWindow1()
    {
        this.InitializeComponent();

        // Set the window title
        AppWindow.Title = ""$(WindowTitle)"";

        // Set the window size (including borders)
        AppWindow.Resize(new Windows.Graphics.SizeInt32($(Width), $(Height)));

        // Set the window position on screen
        AppWindow.Move(new Windows.Graphics.PointInt32($(X), $(Y)));

        // Set the taskbar icon (displayed in the taskbar)
        AppWindow.SetTaskbarIcon(""Assets/Tiles/GalleryIcon.ico"");

        // Set the title bar icon (displayed in the window's title bar)
        AppWindow.SetTitleBarIcon(""Assets/Tiles/GalleryIcon.ico"");

        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
    }

    private void Show_Click(object sender, RoutedEventArgs e)
    {
        AppWindow.Hide();
        Task.Delay(3000).ContinueWith(t => AppWindow.Show());
    }

    private void Hide_Click(object sender, RoutedEventArgs e)
    {
        AppWindow.Hide();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}";

        private const string AppWindowSample2Xaml =
@"<Window ...>
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <StackPanel HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Spacing=""8"">
        <TextBlock Text=""This is a centred sample window"" Style=""{ThemeResource TitleTextBlockStyle}"" TextAlignment=""Center"" />
    </StackPanel>
</Window>";

        private const string AppWindowSample2CSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace YourNamesapace;

public sealed partial class SampleWindow2 : Window
{
    public SampleWindow2()
    {
        this.InitializeComponent();
        AppWindow.SetIcon(""Assets/Tiles/GalleryIcon.ico"");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        // Center the window on the screen.
        CenterWindow();
    }

    // Centers the given AppWindow on the screen based on the available display area.
    private void CenterWindow()
    {
        var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
        if (area == null) return;
        AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
    }
}";

        private const string AppWindowSample3Xaml =
@"<Window ...>
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <StackPanel HorizontalAlignment=""Center""
                VerticalAlignment=""Center""
                Spacing=""10"">
        <Button x:Name=""MaximizeBtn""
                Content=""Maximize""
                Click=""MaximizeBtn_Click""
                Width=""200"" />
        <Button x:Name=""MinimizeBtn""
                Content=""Minimize""
                Click=""MinimizeBtn_Click""
                Width=""200"" />
        <Button x:Name=""RestoreBtn""
                Click=""RestoreBtn_Click""
                Width=""200"" >
            <TextBlock Text=""Minimize and restore the window after 3 seconds"" TextWrapping=""WrapWholeWords"" TextAlignment=""Center""/>
        </Button>
        <Button x:Name=""CloseBtn""
                Click=""CloseBtn_Click""
                Width=""200""
                Margin=""0,16,0,0"">
            <StackPanel Orientation=""Horizontal""
                        VerticalAlignment=""Center"">
                <SymbolIcon Symbol=""Cancel"" />
                <TextBlock Text=""Close"" />
            </StackPanel>
        </Button>
    </StackPanel>
</Window>";

        private const string AppWindowSample3CSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace YourNamespace;

public sealed partial class SampleWindow3 : Window
{
    public SampleWindow3()
    {
        this.InitializeComponent();

        AppWindow.SetIcon(""Assets/Tiles/GalleryIcon.ico"");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
        OverlappedPresenter presenter = OverlappedPresenter.Create();

        presenter.IsAlwaysOnTop = $(IsAlwaysOnTop);
        presenter.IsMaximizable = $(IsMaximizable);
        presenter.IsMinimizable = $(IsMinimizable);
        presenter.IsResizable = $(IsResizable);
        presenter.SetBorderAndTitleBar($(HasBorder), $(HasTitleBar));

        AppWindow.SetPresenter(presenter);
    }
}";

        private const string AppWindowSample4CSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace WinUIGallery.Samples.SamplePages;

public sealed partial class SampleWindow4 : Window
{
    public SampleWindow4(int MinWidth, int MinHeight, int MaxWidth, int MaxHeight)
    {
        this.InitializeComponent();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 500));
        AppWindow.SetIcon(""Assets/Tiles/GalleryIcon.ico"");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        OverlappedPresenter presenter = OverlappedPresenter.Create();
        presenter.PreferredMinimumWidth = MinWidth;
        presenter.PreferredMinimumHeight = MinHeight;
        presenter.PreferredMaximumWidth = MaxWidth;
        presenter.PreferredMaximumHeight = MaxHeight;
        presenter.IsMaximizable = false;

        AppWindow.SetPresenter(presenter);
    }
}";

        private const string AppWindowSample5Xaml =
@"<Window ...>
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <StackPanel HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Spacing=""8"">
        <TextBlock Text=""Modal Window"" Style=""{ThemeResource TitleTextBlockStyle}"" TextAlignment=""Center"" />
        <TextBlock Text=""This is a modal window created using AppWindow with OverlappedPresenter."" Style=""{ThemeResource BodyTextBlockStyle}"" TextAlignment=""Center"" TextWrapping=""Wrap"" />
        <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Center"" Spacing=""8"">
            <Button Content=""OK"" Width=""80"" Click=""OKButton_Click"" />
            <Button Content=""Cancel"" Width=""80"" Click=""CancelButton_Click"" />
        </StackPanel>
    </StackPanel>
</Window>";

        private const string AppWindowSample5CSharp =
@"using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace YourNamespace;

public sealed partial class ModalWindow : Window
{
    public ModalWindow()
    {
        this.InitializeComponent();

        OverlappedPresenter presenter = OverlappedPresenter.CreateForDialog();

        // Set this modal window's owner (the main application window).
        SetWindowOwner(owner: App.StartupWindow);

        // Make the window modal (blocks interaction with the owner window until closed).
        presenter.IsModal = true;

        // Apply the presenter settings to the AppWindow.
        AppWindow.SetPresenter(presenter);

        // Show the modal window.
        AppWindow.Show();
    }
}";

        private const string AppWindowSample6Xaml =
@"<Window ...>
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <StackPanel HorizontalAlignment=""Center""
                VerticalAlignment=""Center""
                Spacing=""8"">
        <TextBlock Text=""This window is running in Fullscreen mode""
                   Style=""{ThemeResource TitleTextBlockStyle}""
                   TextAlignment=""Center"" />
        <Button x:Name=""Close""
                Click=""Close_Click""
                Width=""200""
                HorizontalAlignment=""Center"">
            <StackPanel Orientation=""Horizontal""
                        VerticalAlignment=""Center"">
                <SymbolIcon Symbol=""Cancel"" />
                <TextBlock Text=""Close"" />
            </StackPanel>
        </Button>
    </StackPanel>
</Window>";

        private const string AppWindowSample6CSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace YourNamespace;

public sealed partial class SampleWindow6 : Window
{
    public SampleWindow6()
    {
        this.InitializeComponent();
        AppWindow.SetIcon(""Assets/Tiles/GalleryIcon.ico"");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        // Set the window to Full-Screen mode
        AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}";

        private const string AppWindowSample7Xaml =
@"<Window ...>
    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <StackPanel HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Spacing=""8"">
        <TextBlock Text=""This window is set to CompactOverlay (Picture-in-Picture) mode."" TextAlignment=""Center"" TextWrapping=""Wrap"" />
    </StackPanel>
</Window>";

        private const string AppWindowSample7CSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace YourNamespace;

public sealed partial class SampleWindow7 : Window
{
    public SampleWindow7(string InitialSize)
    {
        this.InitializeComponent();

        AppWindow.SetIcon(""Assets/Tiles/GalleryIcon.ico"");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        // Creates a CompactOverlay (Picture-in-Picture) presenter
        CompactOverlayPresenter presenter = CompactOverlayPresenter.Create();

        // Sets the initial size of the CompactOverlay window
        presenter.InitialSize = CompactOverlaySize.$(InitialSize);

        // Applies the CompactOverlay presenter to the window
        AppWindow.SetPresenter(presenter);
    }
}";

        private const string AppWindowTitleBarColorsCSharp =
@"using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;

public sealed partial class AppWindowTitleBarWindow : Window
{
    public AppWindowTitleBarWindow()
    {
        InitializeComponent();

        AppWindow.TitleBar.BackgroundColor = ColorHelper.FromArgb($(BackgroundColor));
        AppWindow.TitleBar.ForegroundColor = ColorHelper.FromArgb($(ForegroundColor));
        AppWindow.TitleBar.ButtonBackgroundColor = ColorHelper.FromArgb($(ButtonBackgroundColor));
        AppWindow.TitleBar.ButtonForegroundColor = ColorHelper.FromArgb($(ButtonForegroundColor));
        AppWindow.TitleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb($(ButtonHoverBackgroundColor));
        AppWindow.TitleBar.ButtonHoverForegroundColor = ColorHelper.FromArgb($(ButtonHoverForegroundColor));
        AppWindow.TitleBar.InactiveBackgroundColor = ColorHelper.FromArgb($(InactiveBackgroundColor));
        AppWindow.TitleBar.InactiveForegroundColor = ColorHelper.FromArgb($(InactiveForegroundColor));
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb($(ButtonInactiveBackgroundColor));
        AppWindow.TitleBar.ButtonInactiveForegroundColor = ColorHelper.FromArgb($(ButtonInactiveForegroundColor));
        AppWindow.TitleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb($(ButtonPressedBackgroundColor));
        AppWindow.TitleBar.ButtonPressedForegroundColor = ColorHelper.FromArgb($(ButtonPressedForegroundColor));
    }
}";

        private const string AppWindowTitleBarExtendCSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

public sealed partial class AppWindowTitleBarExtendWindow : Window
{
    public AppWindowTitleBarExtendWindow()
    {
        InitializeComponent();
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = $(ExtendsContentIntoTitleBar);
        if (AppWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            AppWindow.TitleBar.HeightOption = TitleBarHeightOption.$(TitleBarHeightOption);
        }
    }
}";

        private const string AppWindowTitleBarThemeCSharp =
@"using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

public sealed partial class AppWindowTitleBarThemeHeightWindow : Window
{
    public AppWindowTitleBarThemeHeightWindow()
    {
        InitializeComponent();
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.$(PreferredTheme);
    }
}";

        private const string TitleBarConfigurationXaml =
@"<TitleBar
    Title=""$(Title)""
    Subtitle=""$(Subtitle)""
    IsBackButtonVisible=""$(BackButtonVisibility)""
    IsPaneToggleButtonVisible=""$(PaneToggleVisibility)"">
    <TitleBar.IconSource>
        <ImageIconSource ImageSource=""/Assets/Tiles/GalleryIcon.ico"" />
    </TitleBar.IconSource>
    <TitleBar.Content>
        <AutoSuggestBox
            Width=""360""
            VerticalAlignment=""Center""
            PlaceholderText=""Search..""
            QueryIcon=""Find"" />
    </TitleBar.Content>
    <TitleBar.RightHeader>
        <PersonPicture
            Width=""30""
            Height=""30""
            Initials=""JD"" />
    </TitleBar.RightHeader>
</TitleBar>";

        private const string TitleBarEndToEndXaml =
@"<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <!--  TitleBar  -->
        <RowDefinition Height=""*"" />
        <!--  NavigationView  -->
    </Grid.RowDefinitions>

    <TitleBar
        x:Name=""titleBar""
        BackRequested=""TitleBar_BackRequested""
        IsBackButtonVisible=""{x:Bind navFrame.CanGoBack, Mode=OneWay}""
        IsPaneToggleButtonVisible=""True""
        PaneToggleRequested=""TitleBar_PaneToggleRequested"" />

    <NavigationView
        x:Name=""navView""
        Grid.Row=""1""
        IsBackButtonVisible=""Collapsed""
        IsPaneToggleButtonVisible=""False"">
        <NavigationView.MenuItems... />
        <Frame x:Name=""navFrame"" />
    </NavigationView>
</Grid>";

        private const string TitleBarEndToEndCSharp =
@"this.ExtendsContentIntoTitleBar = true; // Extend the content into the title bar and hide the default titlebar
this.SetTitleBar(titleBar); // Set the custom title bar";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppWindow":
                    return CreateAppWindowSample();
                case "AppWindowTitleBar":
                    return CreateAppWindowTitleBarSample();
                case "CreateMultipleWindows":
                    return CreateMultipleWindowsSample();
                case "TitleBar":
                    return CreateTitleBarSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets = null)
        {
            switch (uniqueId)
            {
                case "AppWindow":
                    return CreateAppWindowExamples(sampleSnippets);
                case "AppWindowTitleBar":
                    return CreateAppWindowTitleBarExamples();
                case "CreateMultipleWindows":
                    return new[]
                    {
                        new GalleryExample(
                            "Create single threaded Multiple Top level Windows(MTW).",
                            CreateMultipleWindowsExampleContent(assignRootAutomationId: true),
                            null,
                            CreateMultipleWindowsCSharp)
                    };
                case "TitleBar":
                    return CreateTitleBarExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppWindowTitleBar":
                    return CreateAppWindowTitleBarIntroContent();
                case "TitleBar":
                    return CreateTitleBarIntroContent();
                default:
                    return null;
            }
        }

        private static UIElement CreateAppWindowSample()
        {
            return CreateAppWindowCustomizeExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateAppWindowExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Creating and customizing an AppWindow from a Window instance",
                    CreateAppWindowCustomizeExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample1_xaml.txt", AppWindowSample1Xaml),
                    AppWindowSample1CSharp),
                new GalleryExample(
                    "Centering AppWindow on the screen using the available display area",
                    CreateAppWindowCenteredExampleContent(),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample2_xaml.txt", AppWindowSample2Xaml),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample2_cs.txt", AppWindowSample2CSharp)),
                new GalleryExample(
                    "AppWindow with OverlapedPresenter",
                    CreateAppWindowOverlappedPresenterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample3_xaml.txt", AppWindowSample3Xaml),
                    AppWindowSample3CSharp),
                new GalleryExample(
                    "Setting the minimum and maximum width / height on an AppWindow using OverlappedPresenter",
                    CreateAppWindowMinMaxExampleContent(),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample3_xaml.txt", AppWindowSample3Xaml),
                    AppWindowSample4CSharp),
                new GalleryExample(
                    "Modal window with OverlappedPresenter using AppWindow",
                    CreateAppWindowModalExampleContent(),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample5_xaml.txt", AppWindowSample5Xaml),
                    FindSampleCodeText(sampleSnippets, "AppWindowSample5_cs.txt", AppWindowSample5CSharp)),
                new GalleryExample(
                    "AppWindow with FullScreenPresenter",
                    CreateAppWindowFullScreenExampleContent(),
                    AppWindowSample6Xaml,
                    AppWindowSample6CSharp),
                new GalleryExample(
                    "AppWindow with CompactOverlayPresenter",
                    CreateAppWindowCompactOverlayExampleContent(),
                    AppWindowSample7Xaml,
                    AppWindowSample7CSharp)
            };
        }

        private static GallerySamplePanel CreateAppWindowCustomizeExampleContent(bool assignRootAutomationId)
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId);

            var button = CreateAppWindowButton("ShowSampleWindow1Button", "Show window");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppWindow", "ShowSampleWindow1Button"));

            var windowTitle = CreateAppWindowTextBox("WindowTitle", "Window title", "This is a title", "Enter window title");
            var windowWidth = CreateAppWindowNumberBox("WindowWidth", "Width", 200, 1000, 800);
            var windowHeight = CreateAppWindowNumberBox("WindowHeight", "Height", 200, 700, 500);
            var xPoint = CreateAppWindowNumberBox("XPoint", "X", 0, 800, 50);
            var yPoint = CreateAppWindowNumberBox("YPoint", "Y", 0, 300, 50);

            button.Click += delegate
            {
                var sampleWindow = CreateModernWindow(
                    (FrameworkElement)button,
                    string.IsNullOrEmpty(windowTitle.Text) ? "SampleWindow1" : windowTitle.Text,
                    CoerceWindowSize(windowWidth.Value, 800),
                    CoerceWindowSize(windowHeight.Value, 500));
                sampleWindow.Left = xPoint.Value;
                sampleWindow.Top = yPoint.Value;
                sampleWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                sampleWindow.Content = CreateAppWindowCommandBody();
                sampleWindow.Show();
            };

            var options = CreateAppWindowOptionsPanel();
            options.Children.Add(CreateAppWindowSectionLabel("Window title"));
            options.Children.Add(windowTitle);
            options.Children.Add(CreateAppWindowSectionLabel("Window size"));
            options.Children.Add(CreateAppWindowNumberGrid(windowWidth, windowHeight));
            options.Children.Add(CreateAppWindowSectionLabel("Window postion"));
            options.Children.Add(CreateAppWindowNumberGrid(xPoint, yPoint));

            root.Children.Add(CreateAppWindowExampleLayout(WrapInStack(button), options));
            return root;
        }

        private static GallerySamplePanel CreateAppWindowCenteredExampleContent()
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId: false);
            var button = CreateAppWindowButton("ShowSampleWindow2Button", "Show centered sample window");
            button.Click += delegate
            {
                var sampleWindow = CreateModernWindow((FrameworkElement)button, "SampleWindow2", 520, 320);
                sampleWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                sampleWindow.Content = CreateCenteredWindowBody("This is a centred sample window");
                sampleWindow.Show();
            };

            root.Children.Add(button);
            return root;
        }

        private static GallerySamplePanel CreateAppWindowOverlappedPresenterExampleContent()
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId: false);
            var button = CreateAppWindowButton("ShowSampleWindow3Button", "Show window");
            var isAlwaysOnTop = CreateAppWindowToggleSwitch("IsAlwaysOnTop", "IsAlwaysOnTop", false);
            var isMaximizable = CreateAppWindowToggleSwitch("IsMaximizable", "IsMaximizable", true);
            var isMinimizable = CreateAppWindowToggleSwitch("IsMinimizable", "IsMinimizable", true);
            var isResizable = CreateAppWindowToggleSwitch("IsResizable", "IsResizable", true);
            var hasBorder = CreateAppWindowToggleSwitch("HasBorder", "HasBorder", true);
            var hasTitleBar = CreateAppWindowToggleSwitch("HasTitleBar", "HasTitleBar", true);

            hasBorder.Toggled += delegate
            {
                if (!hasBorder.IsOn)
                {
                    hasTitleBar.IsOn = false;
                }
            };
            hasTitleBar.Toggled += delegate
            {
                if (hasTitleBar.IsOn)
                {
                    hasBorder.IsOn = true;
                }
            };
            button.Click += delegate
            {
                var sampleWindow = CreateModernWindow((FrameworkElement)button, "SampleWindow3", 620, 420);
                sampleWindow.Topmost = isAlwaysOnTop.IsOn;
                sampleWindow.ResizeMode = isResizable.IsOn ? ResizeMode.CanResize : ResizeMode.NoResize;
                if (!hasTitleBar.IsOn)
                {
                    WindowHelper.SetUseModernWindowStyle(sampleWindow, false);
                    sampleWindow.WindowStyle = WindowStyle.None;
                }
                sampleWindow.Content = CreateAppWindowPresenterCommandBody();
                sampleWindow.Show();
            };

            var example = CreateAppWindowStack();
            example.Children.Add(new TextBlock
            {
                Text = "OverlappedPresenter is the default presenter for AppWindow, providing a standard resizable window with system buttons. It is used for typical app windows and can be customized to control resizing and button visibility.",
                TextWrapping = TextWrapping.Wrap
            });
            example.Children.Add(new Mux.InfoBar
            {
                Title = "Warning",
                IsClosable = false,
                IsOpen = true,
                Severity = Mux.InfoBarSeverity.Warning,
                Message = "For an AppWindow with OverlappedPresenter, if the title bar is enabled, the window must have a border."
            });
            example.Children.Add(button);

            var options = CreateAppWindowOptionsPanel();
            options.Children.Add(isAlwaysOnTop);
            options.Children.Add(isMaximizable);
            options.Children.Add(isMinimizable);
            options.Children.Add(isResizable);
            options.Children.Add(hasBorder);
            options.Children.Add(hasTitleBar);

            root.Children.Add(CreateAppWindowExampleLayout(example, options));
            return root;
        }

        private static GallerySamplePanel CreateAppWindowMinMaxExampleContent()
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId: false);
            var minWidth = CreateAppWindowNumberBox("MinWidthBox", "PreferredMinimumWidth", 0, double.PositiveInfinity, 400);
            var minHeight = CreateAppWindowNumberBox("MinHeightBox", "PreferredMinimumHeight", 0, double.PositiveInfinity, 400);
            var maxWidth = CreateAppWindowNumberBox("MaxWidthBox", "PreferredMaximumWidth", 0, double.PositiveInfinity, 1000);
            var maxHeight = CreateAppWindowNumberBox("MaxHeightBox", "PreferredMaximumHeight", 0, double.PositiveInfinity, 1000);

            var button = CreateAppWindowButton("ShowSampleWindow4Button", "Show window");
            button.Click += delegate
            {
                var sampleWindow = CreateModernWindow((FrameworkElement)button, "SampleWindow4", 800, 500);
                sampleWindow.MinWidth = CoerceWindowSize(minWidth.Value, 400);
                sampleWindow.MinHeight = CoerceWindowSize(minHeight.Value, 400);
                sampleWindow.MaxWidth = CoerceWindowSize(maxWidth.Value, 1000);
                sampleWindow.MaxHeight = CoerceWindowSize(maxHeight.Value, 1000);
                sampleWindow.ResizeMode = ResizeMode.CanResize;
                sampleWindow.Content = CreateCenteredWindowBody("Minimum and maximum dimensions are applied to this WPF window.");
                sampleWindow.Show();
            };

            var example = CreateAppWindowStack();
            example.Children.Add(new TextBlock
            {
                Text = "The minimum and maximum width and height can be set on an AppWindow. When setting the maximum width or height, it's recommended to disable the window maximization.",
                TextWrapping = TextWrapping.Wrap
            });
            example.Children.Add(button);

            var options = CreateAppWindowOptionsPanel();
            options.Children.Add(minWidth);
            options.Children.Add(minHeight);
            options.Children.Add(maxWidth);
            options.Children.Add(maxHeight);

            root.Children.Add(CreateAppWindowExampleLayout(example, options));
            return root;
        }

        private static GallerySamplePanel CreateAppWindowModalExampleContent()
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId: false);
            var button = CreateAppWindowButton("ShowSampleWindow5Button", "Show modal window");
            button.Click += delegate
            {
                var sampleWindow = CreateModernWindow((FrameworkElement)button, "Modal Window", 400, 300);
                sampleWindow.Content = CreateModalWindowBody();
                sampleWindow.ShowDialog();
            };

            var example = CreateAppWindowStack();
            example.Children.Add(new TextBlock
            {
                Text = "A modal window is a separate window that blocks interaction with its owner window until it is closed, often used for critical actions like confirmations, authentication, or settings. Unlike a ContentDialog, which is a lightweight pop-up within the same window, a modal window is a fully independent window, making it suitable for multi-window applications or scenarios requiring more flexibility in layout and behavior.",
                TextWrapping = TextWrapping.Wrap
            });
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateAppWindowFullScreenExampleContent()
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId: false);
            var button = CreateAppWindowButton("ShowSampleWindow6Button", "Show window (Fullscreen mode)");
            button.Click += delegate
            {
                var sampleWindow = CreateModernWindow((FrameworkElement)button, "SampleWindow6", 900, 600);
                sampleWindow.WindowState = WindowState.Maximized;
                sampleWindow.Content = CreateCenteredWindowBody("This window is running in Fullscreen mode");
                sampleWindow.Show();
            };

            var example = CreateAppWindowStack();
            example.Children.Add(new TextBlock
            {
                Text = "The FullScreenPresenter makes an AppWindow cover the entire screen, removing the title bar and system UI to create an immersive experience. To ensure usability, an exit mechanism, such as handling the Escape key or close button, should be included, and fullscreen mode should be used in scenarios like media playback or focused tasks.",
                TextWrapping = TextWrapping.Wrap
            });
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateAppWindowCompactOverlayExampleContent()
        {
            var root = CreateAppWindowExampleRoot(assignRootAutomationId: false);
            var button = CreateAppWindowButton("ShowSampleWindow7Button", "Show window (Picture-in-Picture mode)");
            var initialSize = new ComboBox
            {
                Name = "InitialSize",
                Width = 150,
                ItemsSource = new[] { "Small", "Medium", "Large" },
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(initialSize, "InitialSize");
            var description = new TextBlock
            {
                Name = "InitialSizeDescription",
                Width = 250,
                Text = "Small: Window size is approximately 5% of the display's work area.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            };
            initialSize.SelectionChanged += delegate
            {
                var size = initialSize.SelectedItem as string ?? "Small";
                var percentage = size == "Medium" ? "15%" : size == "Large" ? "25%" : "5%";
                description.Text = size + ": Window size is approximately " + percentage + " of the display's work area.";
            };
            button.Click += delegate
            {
                var dimensions = GetCompactOverlayDimensions(initialSize.SelectedItem as string);
                var sampleWindow = CreateModernWindow((FrameworkElement)button, "SampleWindow7", dimensions.Width, dimensions.Height);
                sampleWindow.Topmost = true;
                sampleWindow.ResizeMode = ResizeMode.NoResize;
                sampleWindow.Content = CreateCenteredWindowBody("This window is set to CompactOverlay (Picture-in-Picture) mode.");
                sampleWindow.Show();
            };

            var example = CreateAppWindowStack();
            example.Children.Add(new TextBlock
            {
                Text = "CompactOverlayPresenter (Picture-in-Picture mode) keeps an AppWindow always on top while using minimal screen space. To ensure a good user experience, the window should have a small yet functional size (e.g., for media players or floating tools).",
                TextWrapping = TextWrapping.Wrap
            });
            example.Children.Add(button);

            var options = CreateAppWindowOptionsPanel();
            options.Children.Add(initialSize);
            options.Children.Add(description);

            root.Children.Add(CreateAppWindowExampleLayout(example, options));
            return root;
        }

        private static UIElement CreateAppWindowTitleBarSample()
        {
            return CreateAppWindowTitleBarColorExampleContent(assignRootAutomationId: true);
        }

        private static TextBlock CreateAppWindowTitleBarIntroContent()
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            textBlock.Inlines.Add(new Run("For the default title bar and basic scenarios, use the "));

            var hyperlink = new Hyperlink(new Run("TitleBar"));
            hyperlink.Click += OnTitleBarHyperlinkClick;
            textBlock.Inlines.Add(hyperlink);

            textBlock.Inlines.Add(new Run(" control."));
            return textBlock;
        }

        private static IReadOnlyList<GalleryExample> CreateAppWindowTitleBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "AppWindowTitleBar color customization",
                    CreateAppWindowTitleBarColorExampleContent(assignRootAutomationId: true),
                    null,
                    AppWindowTitleBarColorsCSharp),
                new GalleryExample(
                    "Extending content into the AppWindowTitleBar area",
                    CreateAppWindowTitleBarExtendExampleContent(),
                    null,
                    AppWindowTitleBarExtendCSharp),
                new GalleryExample(
                    "AppWindowTitleBar preferred theme and height options",
                    CreateAppWindowTitleBarThemeExampleContent(),
                    null,
                    AppWindowTitleBarThemeCSharp)
            };
        }

        private static GallerySamplePanel CreateAppWindowTitleBarColorExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("AppWindowTitleBar"));
            }

            var background = CreateTitleBarColorSelector("Background", "BackgroundColor", "#FFF2F6FA");
            var foreground = CreateTitleBarColorSelector("Foreground", "ForegroundColor", "#FF1E2933");
            var buttonBackground = CreateTitleBarColorSelector("ButtonBackground", "ButtonBackgroundColor", "#FF3B82F6");
            var buttonForeground = CreateTitleBarColorSelector("ButtonForeground", "ButtonForegroundColor", "#FFFFFFFF");
            var buttonHoverBackground = CreateTitleBarColorSelector("ButtonHoverBackground", "ButtonHoverBackgroundColor", "#FF2563EB");
            var buttonHoverForeground = CreateTitleBarColorSelector("ButtonHoverForeground", "ButtonHoverForegroundColor", "#FFFFFFFF");
            var inactiveBackground = CreateTitleBarColorSelector("InactiveBackground", "InactiveBackgroundColor", "#FFE5EAF0");
            var inactiveForeground = CreateTitleBarColorSelector("InactiveForeground", "InactiveForegroundColor", "#FF6B7280");
            var buttonInactiveBackground = CreateTitleBarColorSelector("ButtonInactiveBackground", "ButtonInactiveBackgroundColor", "#FFCBD5E1");
            var buttonInactiveForeground = CreateTitleBarColorSelector("ButtonInactiveForeground", "ButtonInactiveForegroundColor", "#FF475569");
            var buttonPressedBackground = CreateTitleBarColorSelector("ButtonPressedBackground", "ButtonPressedBackgroundColor", "#FF1D4ED8");
            var buttonPressedForeground = CreateTitleBarColorSelector("ButtonPressedForeground", "ButtonPressedForegroundColor", "#FFFFFFFF");

            Window sampleWindow = null;
            var showWindowButton = new Button
            {
                Name = "ShowWindowButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(showWindowButton, "Show window");
            GalleryAutomation.WithAutomationId(showWindowButton, GalleryAutomation.SampleElementId("AppWindowTitleBar", "ShowWindowButton"));
            showWindowButton.Click += delegate
            {
                showWindowButton.IsEnabled = false;
                sampleWindow = CreateModernWindow((FrameworkElement)showWindowButton, "AppWindowTitleBarWindow", 620, 380);
                ApplyAppWindowTitleBarColorSettings(
                    sampleWindow,
                    background,
                    foreground,
                    buttonBackground,
                    buttonForeground,
                    buttonHoverBackground,
                    buttonHoverForeground,
                    inactiveBackground,
                    inactiveForeground,
                    buttonInactiveBackground,
                    buttonInactiveForeground,
                    buttonPressedBackground,
                    buttonPressedForeground);
                sampleWindow.Content = CreateWindowBody(
                    "AppWindowTitleBar color customization",
                    "This WPF window maps WinUI AppWindowTitleBar colors to ModernWpf title bar attached properties.");
                sampleWindow.Closed += delegate
                {
                    showWindowButton.IsEnabled = true;
                    sampleWindow = null;
                };
                sampleWindow.Show();
            };

            root.Children.Add(showWindowButton);

            var options = new Grid
            {
                Margin = new Thickness(0, 16, 0, 0)
            };
            options.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            options.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            options.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var normalStates = new StackPanel();
            AddTitleBarColorOption(normalStates, "BackgroundColor", background);
            AddTitleBarColorOption(normalStates, "ForegroundColor", foreground);
            AddTitleBarColorOption(normalStates, "ButtonBackgroundColor", buttonBackground);
            AddTitleBarColorOption(normalStates, "ButtonForegroundColor", buttonForeground);
            AddTitleBarColorOption(normalStates, "ButtonHoverBackgroundColor", buttonHoverBackground);
            AddTitleBarColorOption(normalStates, "ButtonHoverForegroundColor", buttonHoverForeground);
            Grid.SetColumn(normalStates, 0);

            var separator = new Border
            {
                Width = 1,
                Margin = new Thickness(16, 0, 16, 0)
            };
            separator.SetResourceReference(Border.BackgroundProperty, "DividerStrokeColorDefaultBrush");
            Grid.SetColumn(separator, 1);

            var inactiveStates = new StackPanel();
            AddTitleBarColorOption(inactiveStates, "InactiveBackgroundColor", inactiveBackground);
            AddTitleBarColorOption(inactiveStates, "InactiveForegroundColor", inactiveForeground);
            AddTitleBarColorOption(inactiveStates, "ButtonInactiveBackgroundColor", buttonInactiveBackground);
            AddTitleBarColorOption(inactiveStates, "ButtonInactiveForegroundColor", buttonInactiveForeground);
            AddTitleBarColorOption(inactiveStates, "ButtonPressedBackgroundColor", buttonPressedBackground);
            AddTitleBarColorOption(inactiveStates, "ButtonPressedForegroundColor", buttonPressedForeground);
            Grid.SetColumn(inactiveStates, 2);

            options.Children.Add(normalStates);
            options.Children.Add(separator);
            options.Children.Add(inactiveStates);
            root.Children.Add(options);

            return root;
        }

        private static GallerySamplePanel CreateAppWindowTitleBarExtendExampleContent()
        {
            var root = new GallerySamplePanel();
            Window extendWindow = null;

            var showExtendButton = new Button
            {
                Name = "ShowExtendButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(showExtendButton, "Show window");

            var extendContentCheckBox = new CheckBox
            {
                Name = "ExtendContentCheckBox",
                Margin = new Thickness(0, 0, 0, 12),
                Content = "Extend content into title bar",
                IsChecked = true
            };
            var heightComboBox = new ComboBox
            {
                Name = "HeightComboBox",
                Width = 200,
                ItemsSource = new[] { "Standard", "Tall" },
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(heightComboBox, "TitleBarHeightOption");

            showExtendButton.Click += delegate
            {
                showExtendButton.IsEnabled = false;
                extendWindow = CreateModernWindow((FrameworkElement)showExtendButton, "AppWindowTitleBarExtendWindow", 620, 380);
                Mux.TitleBar.SetExtendViewIntoTitleBar(extendWindow, extendContentCheckBox.IsChecked == true);
                extendWindow.Content = CreateWindowBody(
                    "Extending content into the AppWindowTitleBar area",
                    "ModernWpf maps this to TitleBar.ExtendViewIntoTitleBar; the selected height option is represented in the sample source.");
                extendWindow.Closed += delegate
                {
                    showExtendButton.IsEnabled = true;
                    extendWindow = null;
                };
                extendWindow.Show();
            };

            extendContentCheckBox.Checked += delegate
            {
                if (extendWindow != null)
                {
                    Mux.TitleBar.SetExtendViewIntoTitleBar(extendWindow, true);
                }
            };
            extendContentCheckBox.Unchecked += delegate
            {
                if (extendWindow != null)
                {
                    Mux.TitleBar.SetExtendViewIntoTitleBar(extendWindow, false);
                }
            };

            var options = new StackPanel
            {
                Margin = new Thickness(0, 16, 0, 0)
            };
            options.Children.Add(extendContentCheckBox);
            options.Children.Add(heightComboBox);

            root.Children.Add(showExtendButton);
            root.Children.Add(options);
            return root;
        }

        private static GallerySamplePanel CreateAppWindowTitleBarThemeExampleContent()
        {
            var root = new GallerySamplePanel();
            Window themeWindow = null;

            var showThemeHeightButton = new Button
            {
                Name = "ShowThemeHeightButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(showThemeHeightButton, "Show window");

            var themeComboBox = new ComboBox
            {
                Name = "ThemeComboBox",
                Width = 200,
                ItemsSource = new[] { "UseDefaultAppMode", "Light", "Dark" },
                SelectedIndex = 1,
                Margin = new Thickness(0, 16, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(themeComboBox, "TitleBarTheme");

            showThemeHeightButton.Click += delegate
            {
                showThemeHeightButton.IsEnabled = false;
                themeWindow = CreateModernWindow((FrameworkElement)showThemeHeightButton, "AppWindowTitleBarThemeHeightWindow", 620, 380);
                ThemeManager.SetRequestedTheme(themeWindow, GetElementTheme(themeComboBox.SelectedItem as string));
                themeWindow.Content = CreateWindowBody(
                    "AppWindowTitleBar preferred theme",
                    "ModernWpf maps the title bar theme selection to the WPF window requested theme.");
                themeWindow.Closed += delegate
                {
                    showThemeHeightButton.IsEnabled = true;
                    themeWindow = null;
                };
                themeWindow.Show();
            };
            themeComboBox.SelectionChanged += delegate
            {
                if (themeWindow != null)
                {
                    ThemeManager.SetRequestedTheme(themeWindow, GetElementTheme(themeComboBox.SelectedItem as string));
                }
            };

            root.Children.Add(showThemeHeightButton);
            root.Children.Add(themeComboBox);
            return root;
        }

        private static UIElement CreateMultipleWindowsSample()
        {
            return CreateMultipleWindowsExampleContent(assignRootAutomationId: true);
        }

        private static GallerySamplePanel CreateMultipleWindowsExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CreateMultipleWindows"));
            }

            var button = new Button
            {
                Name = "Control1",
                Content = "Create new Window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, "Create new Window");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("CreateMultipleWindows", "Control1"));
            button.Click += delegate
            {
                var childWindow = CreateModernWindow((FrameworkElement)button, "New child window!", 500, 500);
                childWindow.Content = new Page
                {
                    Content = new TextBlock
                    {
                        Text = "New child window!",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                childWindow.Show();
            };

            root.Children.Add(button);
            return root;
        }

        private static UIElement CreateTitleBarSample()
        {
            return CreateTitleBarConfigurationExampleContent(assignRootAutomationId: true);
        }

        private static TextBlock CreateTitleBarIntroContent()
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = "Use the TitleBar control and ModernWpf title bar attached properties for WPF title bar customization."
            };
            return textBlock;
        }

        private static IReadOnlyList<GalleryExample> CreateTitleBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "TitleBar configuration",
                    CreateTitleBarConfigurationExampleContent(assignRootAutomationId: true),
                    TitleBarConfigurationXaml,
                    null),
                new GalleryExample(
                    "End to end TitleBar sample",
                    CreateTitleBarEndToEndExampleContent(),
                    TitleBarEndToEndXaml,
                    TitleBarEndToEndCSharp)
            };
        }

        private static GallerySamplePanel CreateTitleBarConfigurationExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("TitleBar"));
            }

            var titleText = new TextBlock
            {
                Name = "TitleText",
                Text = "WinUI Gallery",
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            var subtitleText = new TextBlock
            {
                Name = "SubtitleText",
                Text = "Preview",
                FontSize = 12,
                Opacity = 0.72,
                VerticalAlignment = VerticalAlignment.Top
            };

            var backButton = CreateTitleBarPreviewButton("BackButton", Mux.Symbol.Back);
            var paneButton = CreateTitleBarPreviewButton("PaneToggleButton", Mux.Symbol.OpenPane);

            var titleBarControl = new ContentControl
            {
                Name = "TitleBarControl",
                Width = 470,
                Height = 48,
                HorizontalAlignment = HorizontalAlignment.Left,
                ClipToBounds = true,
                Focusable = false
            };
            AutomationProperties.SetName(titleBarControl, "TitleBarControl");
            GalleryAutomation.WithAutomationId(titleBarControl, GalleryAutomation.SampleElementId("TitleBar", "TitleBarControl"));
            var titleBarRoot = new Grid();
            var titleBarBackground = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4)
            };
            titleBarBackground.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            titleBarBackground.SetResourceReference(Border.BorderBrushProperty, "SurfaceStrokeColorDefaultBrush");
            titleBarRoot.Children.Add(titleBarBackground);

            var titleBarGrid = new Grid();
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(backButton, 0);
            Grid.SetColumn(paneButton, 1);
            titleBarGrid.Children.Add(backButton);
            titleBarGrid.Children.Add(paneButton);

            var icon = new Image
            {
                Name = "TitleBarIcon",
                Width = 20,
                Height = 20,
                Margin = new Thickness(16, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Source = CreateBitmap(ResourceUri("Assets/Tiles/GalleryIcon.ico"))
            };
            Grid.SetColumn(icon, 2);
            titleBarGrid.Children.Add(icon);

            var titleStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStack.Children.Add(titleText);
            titleStack.Children.Add(subtitleText);
            Grid.SetColumn(titleStack, 3);
            titleBarGrid.Children.Add(titleStack);

            var searchBox = new Mux.AutoSuggestBox
            {
                Name = "TitleBarSearchBox",
                Width = 186,
                VerticalAlignment = VerticalAlignment.Center,
                PlaceholderText = "Search..",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find),
                Margin = new Thickness(16, 0, 16, 0)
            };
            Grid.SetColumn(searchBox, 4);
            titleBarGrid.Children.Add(searchBox);

            var personPicture = new Mux.PersonPicture
            {
                Name = "TitleBarRightHeader",
                Width = 30,
                Height = 30,
                Initials = "JD",
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(personPicture, 5);
            titleBarGrid.Children.Add(personPicture);
            titleBarRoot.Children.Add(titleBarGrid);
            titleBarControl.Content = titleBarRoot;

            var titleBox = new TextBox
            {
                Name = "TitleBox",
                Text = "WinUI Gallery"
            };
            ControlHelper.SetHeader(titleBox, "Title");

            var subtitleBox = new TextBox
            {
                Name = "SubtitleBox",
                Text = "Preview",
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(subtitleBox, "Subtitle");

            var backButtonToggle = new Mux.ToggleSwitch
            {
                Name = "BackButtonToggle",
                Header = "IsBackButtonVisible",
                IsOn = false,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var paneToggle = new Mux.ToggleSwitch
            {
                Name = "PaneToggle",
                Header = "IsPaneToggleButtonVisible",
                IsOn = false,
                Margin = new Thickness(0, 12, 0, 0)
            };

            Action updatePreview = delegate
            {
                titleText.Text = titleBox.Text;
                subtitleText.Text = subtitleBox.Text;
                backButton.Visibility = backButtonToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
                paneButton.Visibility = paneToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
            };
            titleBox.TextChanged += delegate { updatePreview(); };
            subtitleBox.TextChanged += delegate { updatePreview(); };
            backButtonToggle.Toggled += delegate { updatePreview(); };
            paneToggle.Toggled += delegate { updatePreview(); };

            var options = new StackPanel
            {
                Width = 240,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(titleBox);
            options.Children.Add(subtitleBox);
            options.Children.Add(backButtonToggle);
            options.Children.Add(paneToggle);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(titleBarControl, 0);
            Grid.SetColumn(options, 1);
            layout.Children.Add(titleBarControl);
            layout.Children.Add(options);
            root.Children.Add(layout);
            updatePreview();
            return root;
        }

        private static GallerySamplePanel CreateTitleBarEndToEndExampleContent()
        {
            var root = new GallerySamplePanel();
            var stack = new StackPanel
            {
                MaxWidth = 560,
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Click the button below to see an end to end sample of a TitleBar in an new window, binding some of its properties to the NavigationView and navigation frame.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            var showWindowButton = new Button
            {
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            showWindowButton.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            showWindowButton.Click += delegate
            {
                var window = CreateModernWindow((FrameworkElement)showWindowButton, "TitleBarWindow", 760, 520);
                Mux.TitleBar.SetExtendViewIntoTitleBar(window, true);
                Mux.TitleBar.SetIsBackButtonVisible(window, true);
                Mux.TitleBar.SetIsBackEnabled(window, false);
                window.Content = CreateTitleBarWindowBody();
                window.Show();
            };
            stack.Children.Add(showWindowButton);
            root.Children.Add(stack);
            return root;
        }

        private static Button CreateTitleBarPreviewButton(string name, Mux.Symbol symbol)
        {
            return new Button
            {
                Name = name,
                Width = 40,
                Height = 40,
                Padding = new Thickness(0),
                Margin = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed,
                Content = new Mux.SymbolIcon(symbol)
            };
        }

        private static FrameworkElement CreateTitleBarWindowBody()
        {
            var frame = new Frame
            {
                Name = "navFrame",
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Content = new Border
                {
                    Padding = new Thickness(32),
                    Child = new TextBlock
                    {
                        Text = "Sample page content",
                        FontSize = 24,
                        FontWeight = FontWeights.SemiBold
                    }
                }
            };

            var navigationView = new Mux.NavigationView
            {
                Name = "navView",
                IsBackButtonVisible = Mux.NavigationViewBackButtonVisible.Collapsed,
                IsPaneToggleButtonVisible = false,
                IsTitleBarAutoPaddingEnabled = false,
                Content = frame
            };
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Home",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Home)
            });
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Documents",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Document)
            });

            return navigationView;
        }

        private static GallerySamplePanel CreateAppWindowExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("AppWindow"));
            }

            return root;
        }

        private static Grid CreateAppWindowExampleLayout(UIElement example, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(example, 0);
            layout.Children.Add(example);

            if (options != null)
            {
                Grid.SetColumn(options, 1);
                layout.Children.Add(options);
            }

            return layout;
        }

        private static StackPanel CreateAppWindowStack()
        {
            return new StackPanel
            {
                Orientation = Orientation.Vertical
            };
        }

        private static StackPanel CreateAppWindowOptionsPanel()
        {
            return new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(24, 0, 0, 0),
                Width = 272
            };
        }

        private static StackPanel WrapInStack(UIElement child)
        {
            var stack = CreateAppWindowStack();
            stack.Children.Add(child);
            return stack;
        }

        private static Button CreateAppWindowButton(string name, string content)
        {
            var button = new Button
            {
                Name = name,
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, content);
            return button;
        }

        private static TextBlock CreateAppWindowSectionLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        private static TextBox CreateAppWindowTextBox(string name, string header, string text, string placeholder)
        {
            var textBox = new TextBox
            {
                Name = name,
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 16)
            };
            ControlHelper.SetHeader(textBox, header);
            ControlHelper.SetPlaceholderText(textBox, placeholder);
            return textBox;
        }

        private static Mux.NumberBox CreateAppWindowNumberBox(string name, string header, double minimum, double maximum, double value)
        {
            return new Mux.NumberBox
            {
                Name = name,
                Header = header,
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                SmallChange = 10,
                LargeChange = 100,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline,
                Width = 120,
                Margin = new Thickness(0, 0, 8, 16)
            };
        }

        private static Grid CreateAppWindowNumberGrid(Mux.NumberBox first, Mux.NumberBox second)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(first, 0);
            Grid.SetColumn(second, 1);
            grid.Children.Add(first);
            grid.Children.Add(second);
            return grid;
        }

        private static Mux.ToggleSwitch CreateAppWindowToggleSwitch(string name, string header, bool isOn)
        {
            return new Mux.ToggleSwitch
            {
                Name = name,
                Header = header,
                IsOn = isOn,
                OnContent = "true",
                OffContent = "false",
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        private static FrameworkElement CreateAppWindowCommandBody()
        {
            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(CreateWindowCommandButton("Hide", "Hide"));
            stack.Children.Add(CreateWindowCommandButton("Show", "Hide and show the window after 3 seconds"));
            stack.Children.Add(CreateWindowCommandButton("Close", "Close"));
            return stack;
        }

        private static FrameworkElement CreateAppWindowPresenterCommandBody()
        {
            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(CreateWindowCommandButton("MaximizeBtn", "Maximize"));
            stack.Children.Add(CreateWindowCommandButton("MinimizeBtn", "Minimize"));
            stack.Children.Add(CreateWindowCommandButton("RestoreBtn", "Minimize and restore the window after 3 seconds"));
            stack.Children.Add(CreateWindowCommandButton("CloseBtn", "Close"));
            return stack;
        }

        private static Button CreateWindowCommandButton(string name, string text)
        {
            var button = new Button
            {
                Name = name,
                Content = text,
                Width = 200,
                Margin = new Thickness(0, 0, 0, name == "Close" || name == "CloseBtn" ? 0 : 8)
            };
            button.Click += delegate
            {
                if (name == "Close" || name == "CloseBtn")
                {
                    var window = Window.GetWindow(button);
                    if (window != null)
                    {
                        window.Close();
                    }
                }
            };
            return button;
        }

        private static FrameworkElement CreateCenteredWindowBody(string text)
        {
            return new Border
            {
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(32)
                }
            };
        }

        private static FrameworkElement CreateModalWindowBody()
        {
            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                Text = "Modal Window",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = "This is a modal window created using AppWindow with OverlappedPresenter.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 12),
                MaxWidth = 320
            });

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttons.Children.Add(CreateDialogCloseButton("OK"));
            buttons.Children.Add(CreateDialogCloseButton("Cancel"));
            stack.Children.Add(buttons);
            return stack;
        }

        private static Button CreateDialogCloseButton(string content)
        {
            var button = new Button
            {
                Content = content,
                Width = 80,
                Margin = new Thickness(0, 0, content == "OK" ? 8 : 0, 0)
            };
            button.Click += delegate
            {
                var window = Window.GetWindow(button);
                if (window != null)
                {
                    window.Close();
                }
            };
            return button;
        }

        private static Size GetCompactOverlayDimensions(string size)
        {
            switch (size)
            {
                case "Large":
                    return new Size(480, 270);
                case "Medium":
                    return new Size(360, 220);
                default:
                    return new Size(280, 180);
            }
        }

        private static double CoerceWindowSize(double value, double fallback)
        {
            return double.IsNaN(value) || double.IsInfinity(value) ? fallback : value;
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string fileName, string fallback)
        {
            if (snippets != null)
            {
                foreach (var snippet in snippets)
                {
                    if (string.Equals(snippet.Title, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return snippet.Text;
                    }
                }
            }

            return fallback;
        }

        private static Button CreateTitleBarColorSelector(string name, string automationName, string color)
        {
            var swatch = new Border
            {
                Width = 30,
                Height = 18,
                Background = CreateBrush(color),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };
            swatch.SetResourceReference(Border.BorderBrushProperty, "ControlStrokeColorDefaultBrush");

            var button = new Button
            {
                Name = name,
                Width = 48,
                Height = 32,
                Padding = new Thickness(6),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = swatch,
                Tag = color,
                ToolTip = automationName
            };
            AutomationProperties.SetName(button, automationName);
            return button;
        }

        private static void AddTitleBarColorOption(StackPanel stackPanel, string label, Button selector)
        {
            stackPanel.Children.Add(new TextBlock
            {
                Text = label,
                Margin = stackPanel.Children.Count == 0 ? new Thickness(0) : new Thickness(0, 8, 0, 0)
            });
            stackPanel.Children.Add(selector);
        }

        private static void ApplyAppWindowTitleBarColorSettings(
            Window window,
            Button background,
            Button foreground,
            Button buttonBackground,
            Button buttonForeground,
            Button buttonHoverBackground,
            Button buttonHoverForeground,
            Button inactiveBackground,
            Button inactiveForeground,
            Button buttonInactiveBackground,
            Button buttonInactiveForeground,
            Button buttonPressedBackground,
            Button buttonPressedForeground)
        {
            Mux.TitleBar.SetBackground(window, GetTitleBarColorBrush(background));
            Mux.TitleBar.SetForeground(window, GetTitleBarColorBrush(foreground));
            Mux.TitleBar.SetInactiveBackground(window, GetTitleBarColorBrush(inactiveBackground));
            Mux.TitleBar.SetInactiveForeground(window, GetTitleBarColorBrush(inactiveForeground));
            Mux.TitleBar.SetButtonStyle(
                window,
                CreateTitleBarButtonStyle(
                    GetTitleBarColorBrush(buttonBackground),
                    GetTitleBarColorBrush(buttonForeground),
                    GetTitleBarColorBrush(buttonHoverBackground),
                    GetTitleBarColorBrush(buttonHoverForeground),
                    GetTitleBarColorBrush(buttonInactiveBackground),
                    GetTitleBarColorBrush(buttonInactiveForeground),
                    GetTitleBarColorBrush(buttonPressedBackground),
                    GetTitleBarColorBrush(buttonPressedForeground)));
        }

        private static Style CreateTitleBarButtonStyle(
            Brush background,
            Brush foreground,
            Brush hoverBackground,
            Brush hoverForeground,
            Brush inactiveBackground,
            Brush inactiveForeground,
            Brush pressedBackground,
            Brush pressedForeground)
        {
            var style = new Style(typeof(TitleBarButton));
            style.Setters.Add(new Setter(Control.BackgroundProperty, background));
            style.Setters.Add(new Setter(Control.ForegroundProperty, foreground));
            style.Setters.Add(new Setter(TitleBarButton.HoverBackgroundProperty, hoverBackground));
            style.Setters.Add(new Setter(TitleBarButton.HoverForegroundProperty, hoverForeground));
            style.Setters.Add(new Setter(TitleBarButton.InactiveBackgroundProperty, inactiveBackground));
            style.Setters.Add(new Setter(TitleBarButton.InactiveForegroundProperty, inactiveForeground));
            style.Setters.Add(new Setter(TitleBarButton.PressedBackgroundProperty, pressedBackground));
            style.Setters.Add(new Setter(TitleBarButton.PressedForegroundProperty, pressedForeground));
            return style;
        }

        private static Brush GetTitleBarColorBrush(Button selector)
        {
            return CreateBrush((string)selector.Tag);
        }

        private static ElementTheme GetElementTheme(string titleBarTheme)
        {
            switch (titleBarTheme)
            {
                case "Light":
                    return ElementTheme.Light;
                case "Dark":
                    return ElementTheme.Dark;
                default:
                    return ElementTheme.Default;
            }
        }

        private static void OnTitleBarHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var page = FindLogicalAncestor<ItemPage>(sender as DependencyObject);
            var target = GalleryCatalog.FindItem("TitleBar");
            if (page != null && target != null)
            {
                page.ItemRequested?.Invoke(target);
                e.Handled = true;
            }
        }

        private static void OnAppWindowTitleBarHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var page = FindLogicalAncestor<ItemPage>(sender as DependencyObject);
            var target = GalleryCatalog.FindItem("AppWindowTitleBar");
            if (page != null && target != null)
            {
                page.ItemRequested?.Invoke(target);
                e.Handled = true;
            }
        }

        private static T FindLogicalAncestor<T>(DependencyObject current)
            where T : class
        {
            while (current != null)
            {
                var match = current as T;
                if (match != null)
                {
                    return match;
                }

                current = LogicalTreeHelper.GetParent(current);
            }

            return null;
        }

        private static Window CreateModernWindow(FrameworkElement ownerElement, string title, double width, double height)
        {
            var window = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                MinWidth = 360,
                MinHeight = 240,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = CreateBrush("#F9F9F9")
            };
            var owner = Window.GetWindow(ownerElement);
            if (owner != null)
            {
                window.Owner = owner;
            }
            ThemeManager.SetIsThemeAware(window, true);
            WindowHelper.SetUseModernWindowStyle(window, true);
            Mux.TitleBar.SetIsIconVisible(window, true);
            return window;
        }

        private static FrameworkElement CreateWindowBody(string title, string body)
        {
            var close = CreateButton("Close");
            var grid = new Grid
            {
                Margin = new Thickness(32)
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 28,
                FontWeight = FontWeights.SemiBold
            });
            stack.Children.Add(new TextBlock
            {
                Text = body,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0),
                Opacity = 0.72
            });
            Grid.SetRow(stack, 0);

            close.HorizontalAlignment = HorizontalAlignment.Right;
            close.Margin = new Thickness(0);
            close.Click += delegate
            {
                var window = Window.GetWindow(close);
                if (window != null)
                {
                    window.Close();
                }
            };
            Grid.SetRow(close, 1);

            grid.Children.Add(stack);
            grid.Children.Add(close);
            return grid;
        }

        private static Border CreateWindowPreview(string title, string subtitle, Brush titleBarBrush, Brush titleBrush)
        {
            var titleText = new TextBlock
            {
                Name = "PreviewTitle",
                Text = title,
                Foreground = titleBrush,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };
            var subtitleText = new TextBlock
            {
                Name = "PreviewSubtitle",
                Text = subtitle,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(22, 18, 22, 0)
            };
            var icon = new Rectangle
            {
                Name = "Icon",
                Width = 14,
                Height = 14,
                Fill = titleBrush,
                RadiusX = 3,
                RadiusY = 3,
                Margin = new Thickness(14, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var titleBar = new Grid
            {
                Name = "PreviewChrome",
                Height = 38,
                Background = titleBarBrush
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(icon, 0);
            Grid.SetColumn(titleText, 1);
            titleBar.Children.Add(icon);
            titleBar.Children.Add(titleText);
            titleBar.Children.Add(CreateCaptionButtons(titleBrush));

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(titleBar, 0);
            Grid.SetRow(subtitleText, 1);
            content.Children.Add(titleBar);
            content.Children.Add(subtitleText);

            return new Border
            {
                Width = 520,
                Height = 250,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#C8C8C8"),
                Background = Brushes.White,
                Child = content
            };
        }

        private static Border CreateInteractiveTitleBarPreview()
        {
            var titleBar = new Grid
            {
                Height = 44,
                Background = CreateBrush("#F9F9F9")
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var back = new Button
            {
                Name = "BackButton",
                Content = "<",
                Width = 44,
                Height = 32,
                Margin = new Thickness(6, 6, 0, 6)
            };
            var icon = new Rectangle
            {
                Name = "Icon",
                Width = 16,
                Height = 16,
                Fill = CreateBrush("#0078D4"),
                RadiusX = 4,
                RadiusY = 4,
                Margin = new Thickness(10, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var title = new TextBlock
            {
                Text = "ModernWpf Gallery",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 16, 0)
            };
            var search = new TextBox
            {
                Text = "Interactive content",
                Width = 180,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 6, 16, 6)
            };
            var buttons = CreateCaptionButtons(CreateBrush("#202020"));

            Grid.SetColumn(back, 0);
            Grid.SetColumn(icon, 1);
            Grid.SetColumn(title, 2);
            Grid.SetColumn(search, 3);
            Grid.SetColumn(buttons, 4);
            titleBar.Children.Add(back);
            titleBar.Children.Add(icon);
            titleBar.Children.Add(title);
            titleBar.Children.Add(search);
            titleBar.Children.Add(buttons);

            var body = new Border
            {
                Padding = new Thickness(20),
                Child = new TextBlock
                {
                    Text = "The preview represents a ModernWpf title bar with drag region controls and optional interactive content.",
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.72
                }
            };
            var root = new Grid
            {
                Background = Brushes.White
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(titleBar, 0);
            Grid.SetRow(body, 1);
            root.Children.Add(titleBar);
            root.Children.Add(body);

            return new Border
            {
                Width = 560,
                Height = 190,
                BorderBrush = CreateBrush("#C8C8C8"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = root
            };
        }

        private static StackPanel CreateCaptionButtons(Brush foreground)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(panel, 2);
            panel.Children.Add(CreateCaptionGlyph("_", foreground));
            panel.Children.Add(CreateCaptionGlyph("[]", foreground));
            panel.Children.Add(CreateCaptionGlyph("X", foreground));
            return panel;
        }

        private static TextBlock CreateCaptionGlyph(string text, Brush foreground)
        {
            return new TextBlock
            {
                Text = text,
                Width = 44,
                Height = 38,
                Foreground = foreground,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 9, 0, 0)
            };
        }

        private static ComboBox CreatePaletteCombo(string header, int selectedIndex)
        {
            var combo = new ComboBox
            {
                Width = 180,
                ItemsSource = new[]
                {
                    "Blue",
                    "Purple",
                    "Neutral",
                    "Light",
                    "White",
                    "Black"
                },
                SelectedIndex = selectedIndex,
                Margin = new Thickness(0, 0, 12, 0)
            };
            ControlHelper.SetHeader(combo, header);
            return combo;
        }

        private static Brush GetPaletteBrush(ComboBox combo)
        {
            switch (combo.SelectedItem as string)
            {
                case "Purple":
                    return CreateBrush("#5C2D91");
                case "Neutral":
                    return CreateBrush("#605E5C");
                case "Light":
                    return CreateBrush("#F3F3F3");
                case "White":
                    return Brushes.White;
                case "Black":
                    return CreateBrush("#202020");
                default:
                    return CreateBrush("#0078D4");
            }
        }

        private static void ApplyPreviewChrome(Border preview, Brush background, Brush foreground, bool iconVisible)
        {
            var chrome = FindNamedElement<Panel>(preview, "PreviewChrome");
            if (chrome != null)
            {
                chrome.Background = background;
            }
            var title = FindNamedElement<TextBlock>(preview, "PreviewTitle");
            if (title != null)
            {
                title.Foreground = foreground;
            }
            var icon = FindNamedElement<Rectangle>(preview, "Icon");
            if (icon != null)
            {
                icon.Fill = foreground;
                icon.Visibility = iconVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void SetPreviewText(Border preview, string title, string subtitle)
        {
            var titleBlock = FindNamedElement<TextBlock>(preview, "PreviewTitle");
            var subtitleBlock = FindNamedElement<TextBlock>(preview, "PreviewSubtitle");
            if (titleBlock != null)
            {
                titleBlock.Text = title;
            }
            if (subtitleBlock != null)
            {
                subtitleBlock.Text = subtitle;
            }
        }

        private static void SetNamedElementVisibility(Border root, string name, bool isVisible)
        {
            var element = FindNamedElement<UIElement>(root, name);
            if (element != null)
            {
                element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void SetNamedElementOpacity(Border root, string name, double opacity)
        {
            var element = FindNamedElement<UIElement>(root, name);
            if (element != null)
            {
                element.Opacity = opacity;
            }
        }

        private static T FindNamedElement<T>(DependencyObject root, string name)
            where T : UIElement
        {
            if (root == null)
            {
                return null;
            }

            var frameworkElement = root as FrameworkElement;
            var typedElement = frameworkElement as T;
            if (frameworkElement != null && frameworkElement.Name == name && typedElement != null)
            {
                return typedElement;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var match = FindNamedElement<T>(child, name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Size GetWindowDimensions(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 1:
                    return new Size(820, 460);
                case 2:
                    return new Size(380, 280);
                default:
                    return new Size(640, 420);
            }
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }

        private static Uri ResourceUri(string relativePath)
        {
            return new Uri("pack://application:,,,/ModernWpf.Gallery;component/" + relativePath, UriKind.Absolute);
        }

        private static BitmapImage CreateBitmap(Uri uri)
        {
            return new BitmapImage(uri);
        }
    }
}
