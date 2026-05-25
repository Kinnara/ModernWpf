using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class StylesSampleFactory
    {
        private const string ThemeShadowXaml =
@"<Grid>
    <Grid x:Name=""ShadowCastGrid""/>
    <Border x:Name=""ShadowRect"" Translation=""0,0,$(TranslationSlider)"" Loaded=""ShadowRect_Loaded"" Width=""200"" Height=""200"" CornerRadius=""{ThemeResource OverlayCornerRadius}"" Background=""{ThemeResource CardBackgroundFillColorDefaultBrush}""/>
        <Border.Shadow>
            <ThemeShadow x:Name=""shadow""/>
        </Border.Shadow>
    </Border>
</Grid>";

        private const string ThemeShadowCSharp =
@"private void ShadowRect_Loaded(object sender, RoutedEventArgs e)
{
    shadow.Receivers.Add(ShadowCastGrid);
}";

        private const string LineLineXaml =
@"<Line Stroke=""SteelBlue""
      X1=""$(Slider1)"" Y1=""$(Slider2)""
      X2=""$(Slider3)"" Y2=""$(Slider4)""
      StrokeThickness=""$(Slider5)""/>";

        private const string LinePolylineXaml =
@"<Polyline Stroke=""Black"" StrokeThickness=""$(Slider1)""
          Points=""10,100 60,40 200,40 250,100""/>";

        private const string LinePathXaml =
@"<!-- The first segment is a cubic Bezier curve that begins at Point #1 and ends at Point #4, which is drawn by using Point #2 and 3 as the two control points. This segment is indicated by the ""C"" command in the Data attribute string. -->
<!-- The second segment begins with an absolute horizontal line command ""H"", which specifies a line drawn from the preceding subpath endpoint (Point #4) to a new endpoint (Point #5). Because it's a horizontal line command, the value specified is an x-coordinate. -->

<Path Stroke=""DarkGoldenRod"" StrokeThickness=""$(Slider1)""
      Data=""M 10,100 C 100,25 300,250 400,75 H 200""/>";

        private const string LineGeometryGroupXaml =
@"<Path Stroke=""Black"" StrokeThickness=""4"" Fill=""#CCCCFF"">
    <Path.Data>
        <!-- Creates a composite shape from three geometries. -->
        <GeometryGroup FillRule=""EvenOdd"">
            <LineGeometry StartPoint=""10,10"" EndPoint=""50,30"" />
            <EllipseGeometry Center=""40,70"" RadiusX=""$(Slider1)"" RadiusY=""$(Slider2)"" />
            <RectangleGeometry Rect=""30,55 100 30"" />
        </GeometryGroup>
    </Path.Data>
</Path>";

        private const string ShapeEllipseXaml =
@"<Ellipse Fill=""SteelBlue"" Height=""$(Slider1)"" Width=""$(Slider2)"" StrokeThickness=""$(Slider3)"" Stroke=""Black""/>";

        private const string ShapeRectangleXaml =
@"<Rectangle Fill=""SteelBlue"" Height=""$(Slider1)"" Width=""$(Slider2)""
           Stroke=""Black"" StrokeThickness=""$(Slider3)""
           RadiusY=""$(Slider4)"" RadiusX=""$(Slider5)""/>";

        private const string ShapePolygonXaml =
@"<Polygon Fill=""SteelBlue"" Points=""10,100 60,40 200,40 250,100""
         StrokeThickness=""$(Slider1)"" Stroke=""Black""/>";

        private const string IconElementBitmapIconXaml =
@"<BitmapIcon x:Name=""SlicesIcon"" UriSource=""ms-appx:///Assets/SampleMedia/Slices.png"" Width=""50"" ShowAsMonochrome=""$(ShowAsMonochrome)""/>";

        private const string CompactSizingXaml =
@"<Page.Resources>
    <ResourceDictionary Source=""ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml"" />
</Page.Resources>";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Acrylic":
                    return CreateAcrylicSample();
                case "AnimatedIcon":
                    return CreateAnimatedIconSample();
                case "CompactSizing":
                    return CreateCompactSizingSample();
                case "IconElement":
                    return CreateIconElementSample();
                case "Line":
                    return CreateLineSample();
                case "Shape":
                    return CreateShapeSample();
                case "RadialGradientBrush":
                    return CreateRadialGradientBrushSample();
                case "SystemBackdrops":
                    return CreateSystemBackdropsSample();
                case "SystemBackdropElement":
                    return CreateSystemBackdropElementSample();
                case "ThemeShadow":
                    return CreateThemeShadowSample();
                default:
                    return null;
            }
        }

        public static object CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "CompactSizing":
                    return CreateCompactSizingIntroContent();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "CompactSizing":
                    return CreateCompactSizingExamples();
                case "IconElement":
                    return CreateIconElementExamples(sampleSnippets);
                case "Line":
                    return CreateLineExamples();
                case "Shape":
                    return CreateShapeExamples();
                case "RadialGradientBrush":
                    return CreateRadialGradientBrushExamples(sampleSnippets);
                case "SystemBackdrops":
                    return CreateSystemBackdropsExamples(sampleSnippets);
                case "SystemBackdropElement":
                    return CreateSystemBackdropElementExamples(sampleSnippets);
                case "ThemeShadow":
                    return CreateThemeShadowExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateAcrylicSample()
        {
            var panel = CreateSamplePanel("AcrylicBrush maps to layered WPF brushes: a backdrop image, tint, and translucent content layer.");
            var tint = CreateBrush("#CCF7F7F7");
            var overlay = new Border
            {
                Width = 300,
                Height = 180,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#66FFFFFF"),
                Background = tint,
                Padding = new Thickness(18),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "Acrylic layer",
                    FontSize = 24,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            var root = new Grid
            {
                Width = 430,
                Height = 260
            };
            root.Children.Add(new Image
            {
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/rainier.jpg")),
                Stretch = Stretch.UniformToFill
            });
            root.Children.Add(overlay);

            var opacity = new Slider
            {
                Width = 240,
                Minimum = 0.35,
                Maximum = 0.95,
                Value = tint.Opacity,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(opacity, "Tint opacity");
            opacity.ValueChanged += delegate { tint.Opacity = opacity.Value; };

            panel.Children.Add(root);
            panel.Children.Add(opacity);
            return panel;
        }

        private static UIElement CreateAnimatedIconSample()
        {
            var panel = CreateSamplePanel("AnimatedIcon maps to a ModernWpf FontIcon whose state changes over time.");
            var icon = new Mux.FontIcon
            {
                Glyph = "\xE768",
                FontSize = 54,
                Width = 96,
                Height = 96,
                Foreground = CreateBrush("#0078D4")
            };
            var frame = new Border
            {
                Width = 160,
                Height = 130,
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = icon
            };
            var states = new[] { "\xE768", "\xE895", "\xE72C", "\xE7C1" };
            var index = 0;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
            timer.Tick += delegate
            {
                index = (index + 1) % states.Length;
                icon.Glyph = states[index];
            };

            var commands = CreateCommandRow();
            var start = CreateButton("Start");
            var stop = CreateButton("Stop");
            start.Click += delegate { timer.Start(); };
            stop.Click += delegate { timer.Stop(); };
            commands.Children.Add(start);
            commands.Children.Add(stop);

            panel.Children.Add(frame);
            panel.Children.Add(commands);
            return panel;
        }

        private static UIElement CreateCompactSizingSample()
        {
            return CreateCompactSizingExampleContent(assignRootAutomationId: true);
        }

        private static object CreateCompactSizingIntroContent()
        {
            var stack = new StackPanel
            {
                Margin = new Thickness(0, 24, 0, 0)
            };
            stack.Children.Add(new TextBlock
            {
                Text = "Controls that support compact styling:",
                FontWeight = FontWeights.SemiBold
            });

            foreach (var control in new[]
            {
                "ListView",
                "TextBox",
                "PasswordBox",
                "AutoSuggestBox",
                "ComboBox",
                "DatePicker",
                "TimePicker",
                "TreeView",
                "NavigationView",
                "MenuBar"
            })
            {
                stack.Children.Add(new TextBlock { Text = "\u2022 " + control });
            }

            return stack;
        }

        private static IReadOnlyList<GalleryExample> CreateCompactSizingExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Compact Sizing for controls",
                    CreateCompactSizingExampleContent(assignRootAutomationId: true),
                    CompactSizingXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateCompactSizingExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CompactSizing"));
            }

            var state = new CompactSizingState();
            var contentFrame = new ContentControl
            {
                Name = "ContentFrame",
                Content = CreateCompactSizingForm(compact: false, state)
            };
            GalleryAutomation.WithAutomationId(contentFrame, GalleryAutomation.SampleElementId("CompactSizing", "ContentFrame"));

            var standardRadio = new RadioButton
            {
                Name = "StandardSizeRadioButton",
                Content = "Standard",
                GroupName = "ControlSize",
                IsChecked = true,
                Tag = "StandardSize"
            };
            var compactRadio = new RadioButton
            {
                Name = "CompactSizeRadioButton",
                Content = "Compact",
                GroupName = "ControlSize",
                Tag = "CompactSize"
            };

            standardRadio.Checked += delegate { contentFrame.Content = CreateCompactSizingForm(compact: false, state); };
            compactRadio.Checked += delegate { contentFrame.Content = CreateCompactSizingForm(compact: true, state); };

            var options = new Mux.RadioButtons
            {
                Name = "ControlSizeRadioButtons",
                Header = "Fluent Standard and Compact Sizing"
            };
            options.Items.Add(standardRadio);
            options.Items.Add(compactRadio);

            root.Children.Add(CreateExampleWithOptions(contentFrame, options));
            return root;
        }

        private static Grid CreateCompactSizingForm(bool compact, CompactSizingState state)
        {
            var spacing = compact ? 8 : 16;
            var height = compact ? 26 : 34;
            var padding = compact ? new Thickness(8, 3, 8, 3) : new Thickness(12, 6, 12, 6);

            var panel = new StackPanel
            {
                Name = "CompactPanel"
            };
            AddCompactSizingChild(panel, new TextBlock
            {
                Name = "HeaderBlock",
                FontSize = 18,
                Text = compact ? "Compact Size" : "Standard Size"
            }, spacing);

            var firstName = CreateCompactSizingTextBox("firstName", "First Name:", state.FirstName, height, padding);
            GalleryAutomation.WithAutomationId(firstName, GalleryAutomation.SampleElementId("CompactSizing", "FirstName"));
            firstName.TextChanged += delegate { state.FirstName = firstName.Text; };
            AddCompactSizingChild(panel, firstName, spacing);

            var lastName = CreateCompactSizingTextBox("lastName", "Last Name:", state.LastName, height, padding);
            lastName.TextChanged += delegate { state.LastName = lastName.Text; };
            AddCompactSizingChild(panel, lastName, spacing);

            var password = CreateCompactSizingPasswordBox("password", "Password:", state.Password, height, padding);
            password.PasswordChanged += delegate { state.Password = password.Password; };
            AddCompactSizingChild(panel, password, spacing);

            var confirmPassword = CreateCompactSizingPasswordBox("confirmPassword", "Confirm Password:", state.ConfirmPassword, height, padding);
            confirmPassword.PasswordChanged += delegate { state.ConfirmPassword = confirmPassword.Password; };
            AddCompactSizingChild(panel, confirmPassword, spacing);

            var chosenDate = new DatePicker
            {
                Name = "chosenDate",
                MinHeight = height,
                Padding = padding,
                SelectedDate = state.ChosenDate
            };
            ControlHelper.SetHeader(chosenDate, "Pick a date");
            chosenDate.SelectedDateChanged += delegate { state.ChosenDate = chosenDate.SelectedDate; };
            AddCompactSizingChild(panel, chosenDate, 0);

            var root = new Grid();
            root.Children.Add(panel);
            return root;
        }

        private static TextBox CreateCompactSizingTextBox(string name, string header, string text, double height, Thickness padding)
        {
            var textBox = new TextBox
            {
                Name = name,
                Text = text,
                MinHeight = height,
                Padding = padding
            };
            ControlHelper.SetHeader(textBox, header);
            return textBox;
        }

        private static PasswordBox CreateCompactSizingPasswordBox(string name, string header, string password, double height, Thickness padding)
        {
            var passwordBox = new PasswordBox
            {
                Name = name,
                MinHeight = height,
                Padding = padding,
                Password = password
            };
            ControlHelper.SetHeader(passwordBox, header);
            return passwordBox;
        }

        private static void AddCompactSizingChild(StackPanel panel, FrameworkElement child, double bottomMargin)
        {
            child.Margin = new Thickness(0, 0, 0, bottomMargin);
            panel.Children.Add(child);
        }

        private sealed class CompactSizingState
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
            public DateTime? ChosenDate { get; set; }
        }

        private static UIElement CreateIconElementSample()
        {
            return CreateBitmapIconExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateIconElementExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A BitmapIcon with a multicolor bitmap image",
                    CreateBitmapIconExampleContent(assignRootAutomationId: true),
                    IconElementBitmapIconXaml,
                    null),
                new GalleryExample(
                    "A FontIcon using a glyph from a specific font family in a button",
                    CreateFontIconExampleContent(),
                    FindSampleCodeText(sampleSnippets, "FontIconSample1_xaml.txt", System.IO.Path.Combine("Icons", "FontIconSample1_xaml.txt")),
                    null),
                new GalleryExample(
                    "A ImageIcon using a bitmap image in a button",
                    CreateImageIconBitmapExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ImageIconSample1_xaml.txt", System.IO.Path.Combine("Icons", "ImageIconSample1_xaml.txt")),
                    null),
                new GalleryExample(
                    "A ImageIcon using a SVG image in a button",
                    CreateImageIconSvgExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ImageIconSample2_xaml.txt", System.IO.Path.Combine("Icons", "ImageIconSample2_xaml.txt")),
                    null),
                new GalleryExample(
                    "A PathIcon in a button",
                    CreatePathIconExampleContent(),
                    FindSampleCodeText(sampleSnippets, "PathIconSample1_xaml.txt", System.IO.Path.Combine("Icons", "PathIconSample1_xaml.txt")),
                    null),
                new GalleryExample(
                    "A SymbolIcon in a button",
                    CreateSymbolIconExampleContent(),
                    FindSampleCodeText(sampleSnippets, "SymbolIconSample_1_xaml.txt", System.IO.Path.Combine("Icons", "SymbolIconSample_1_xaml.txt")),
                    null)
            };
        }

        private static GallerySamplePanel CreateBitmapIconExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("IconElement"));
            }

            var slicesIcon = new Mux.BitmapIcon
            {
                Name = "SlicesIcon",
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Left,
                ShowAsMonochrome = false,
                UriSource = new Uri(ResourceUri("Assets/SampleMedia/Slices.png"), UriKind.Absolute)
            };
            GalleryAutomation.WithAutomationId(slicesIcon, GalleryAutomation.SampleElementId("IconElement", "SlicesIcon"));

            var example = CreateIconElementStack("The ShowAsMonochrome property (true by default) will result in a solid block of the foreground color if the property is set to true and the icon is more than one color. This behavior can be ignored by setting the ShowAsMonochrome property to false.");
            example.Children.Add(slicesIcon);

            var monochromeButton = new CheckBox
            {
                Name = "MonochromeButton",
                Content = "Monochrome",
                IsChecked = false
            };
            monochromeButton.Checked += delegate { slicesIcon.ShowAsMonochrome = true; };
            monochromeButton.Unchecked += delegate { slicesIcon.ShowAsMonochrome = false; };

            root.Children.Add(CreateExampleWithOptions(example, monochromeButton));
            return root;
        }

        private static GallerySamplePanel CreateFontIconExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "ExampleButton1",
                Content = new Mux.FontIcon
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = "\uE790"
                }
            };
            AutomationProperties.SetName(button, "ExampleButton1");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "ExampleButton1"));

            var example = CreateIconElementStack("Use FontIcon as the icon for a control if you want to specify a Glyph value from a FontFamily. Windows 10 uses the Segoe MDL2 Assets FontFamily and that is what this example is showing.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateImageIconBitmapExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "ImageExample1",
                Width = 100,
                Content = new Mux.ImageIcon
                {
                    Source = CreateBitmap(ResourceUri("Assets/SampleMedia/Slices.png"))
                }
            };
            AutomationProperties.SetName(button, "ImageExample1");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "ImageExample1"));

            var example = CreateIconElementStack("To use an ImageIcon as the icon for a control, you can set image that has a file format supported by the Image class. The two examples here show a PNG and SVG image as the icon.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateImageIconSvgExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "ImageExample2",
                Content = new Mux.ImageIcon
                {
                    Width = 50,
                    Source = CreateCameraPanoramaDrawing()
                }
            };
            AutomationProperties.SetName(button, "ImageExample2");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "ImageExample2"));
            root.Children.Add(button);
            return root;
        }

        private static GallerySamplePanel CreatePathIconExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "Example1Button",
                Content = new Mux.PathIcon
                {
                    Data = Geometry.Parse("F1 M 16,12 20,2L 20,16 1,16"),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            AutomationProperties.SetName(button, "Example1Button");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "Example1Button"));

            var example = CreateIconElementStack("To use a PathIcon as the icon for a control, you specify the path data of the image you are trying to display. The path data draws a series of connected lines and curves.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateSymbolIconExampleContent()
        {
            var root = new GallerySamplePanel();
            var content = new StackPanel();
            content.Children.Add(new Mux.SymbolIcon(Mux.Symbol.Accept));
            content.Children.Add(new TextBlock { Text = "Accept" });

            var button = new Button
            {
                Name = "AcceptButton",
                Content = content
            };
            AutomationProperties.SetName(button, "AcceptButton");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "AcceptButton"));

            var example = CreateIconElementStack("To use a SymbolIcon as the icon for a control, you specify the enum value for the glyph you would like to display. SymbolIcon's enum is based off of icons from the Segoe MDL2 font used by Windows 10.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static UIElement CreateLineSample()
        {
            return CreateLineExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateLineExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Line",
                    CreateLineExampleContent(assignRootAutomationId: true),
                    LineLineXaml,
                    null),
                new GalleryExample(
                    "Polyline",
                    CreatePolylineExampleContent(),
                    LinePolylineXaml,
                    null),
                new GalleryExample(
                    "Path",
                    CreatePathExampleContent(),
                    LinePathXaml,
                    null),
                new GalleryExample(
                    "GeometryGroup",
                    CreateGeometryGroupExampleContent(),
                    LineGeometryGroupXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateLineExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("Line"));
            }

            var canvas = new Canvas
            {
                Width = 100,
                Height = 200
            };
            var line = new Line
            {
                Name = "LineElement",
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 5,
                X1 = 0,
                Y1 = 0,
                X2 = 200,
                Y2 = 0
            };
            Canvas.SetTop(line, 50);
            GalleryAutomation.WithAutomationId(line, GalleryAutomation.SampleElementId("Line", "Line"));
            canvas.Children.Add(line);

            var lineSlider1 = CreateLineSlider("lineSlider1", "Start point X", 0, 100);
            var lineSlider2 = CreateLineSlider("lineSlider2", "Start point Y", 0, 100);
            var lineSlider3 = CreateLineSlider("lineSlider3", "End point X", 200, 300);
            var lineSlider4 = CreateLineSlider("lineSlider4", "End point Y", 0, 100);
            var lineSlider5 = CreateLineSlider("lineSlider5", "Stroke Thickness", 5, 10);

            lineSlider1.ValueChanged += delegate { line.X1 = lineSlider1.Value; };
            lineSlider2.ValueChanged += delegate { line.Y1 = lineSlider2.Value; };
            lineSlider3.ValueChanged += delegate { line.X2 = lineSlider3.Value; };
            lineSlider4.ValueChanged += delegate { line.Y2 = lineSlider4.Value; };
            lineSlider5.ValueChanged += delegate { line.StrokeThickness = lineSlider5.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(lineSlider1);
            options.Children.Add(lineSlider2);
            options.Children.Add(lineSlider3);
            options.Children.Add(lineSlider4);
            options.Children.Add(lineSlider5);

            root.Children.Add(CreateExampleWithOptions(canvas, options));
            return root;
        }

        private static GallerySamplePanel CreatePolylineExampleContent()
        {
            var root = new GallerySamplePanel();
            var canvas = new Canvas
            {
                Width = 320,
                Height = 170
            };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "Draws a series of connected straight lines.",
                Margin = new Thickness(0, 0, 0, 10)
            });
            var polyline = new Polyline
            {
                Name = "PolylineElement",
                Points = new PointCollection { new Point(10, 100), new Point(60, 40), new Point(200, 40), new Point(250, 100) },
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            stack.Children.Add(polyline);
            canvas.Children.Add(stack);

            var pointLabels = new[]
            {
                AddLinePointLabel(canvas, "Point #1: (10,100)", 0, 140),
                AddLinePointLabel(canvas, "Point #2: (60,40)", 50, 40),
                AddLinePointLabel(canvas, "Point #3: (200,40)", 200, 40),
                AddLinePointLabel(canvas, "Point #4: (250,100)", 240, 140)
            };
            SetVisibility(pointLabels, Visibility.Collapsed);

            var toggleSwitch = new Mux.ToggleSwitch
            {
                Name = "ToggleSwitch2",
                Header = "Show points",
                IsOn = false
            };
            toggleSwitch.Toggled += delegate
            {
                SetVisibility(pointLabels, toggleSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed);
            };

            var slider = CreateLineSlider("polyLineSlider1", "Stroke Thickness", 2, 10);
            slider.ValueChanged += delegate { polyline.StrokeThickness = slider.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(toggleSwitch);
            options.Children.Add(slider);

            root.Children.Add(CreateExampleWithOptions(canvas, options));
            return root;
        }

        private static GallerySamplePanel CreatePathExampleContent()
        {
            var root = new GallerySamplePanel();
            var canvas = new Canvas
            {
                Width = 320,
                Height = 200
            };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "Draws a series of connected lines and curves."
            });
            var path = new Path
            {
                Name = "PathElement",
                Data = Geometry.Parse("M 10,100 C 100,25 300,250 400,75 H 200"),
                Stroke = Brushes.DarkGoldenrod,
                StrokeThickness = 2
            };
            stack.Children.Add(path);
            canvas.Children.Add(stack);

            var pointLabels = new[]
            {
                AddLinePointLabel(canvas, "Point #1: (10,100)", 0, 130),
                AddLinePointLabel(canvas, "Point #2: (100,25)", 40, 75),
                AddLinePointLabel(canvas, "Point #3: (300,250)", 280, 175),
                AddLinePointLabel(canvas, "Point #4: (400,75)", 360, 60),
                AddLinePointLabel(canvas, "Point #5: (200,75)", 170, 60)
            };
            SetVisibility(pointLabels, Visibility.Collapsed);

            var toggleSwitch = new Mux.ToggleSwitch
            {
                Name = "ToggleSwitch",
                Header = "Show points",
                IsOn = false
            };
            toggleSwitch.Toggled += delegate
            {
                SetVisibility(pointLabels, toggleSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed);
            };

            var slider = CreateLineSlider("pathSlider1", "Stroke Thickness", 2, 10);
            slider.ValueChanged += delegate { path.StrokeThickness = slider.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(toggleSwitch);
            options.Children.Add(slider);

            root.Children.Add(CreateExampleWithOptions(canvas, options));
            return root;
        }

        private static GallerySamplePanel CreateGeometryGroupExampleContent()
        {
            var root = new GallerySamplePanel();
            var canvas = new Canvas
            {
                Width = 100,
                Height = 170
            };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "Composite geometry objects can be created using a GeometryGroup.",
                Margin = new Thickness(0, 0, 0, 15)
            });

            var ellipseGeometry = new EllipseGeometry
            {
                Center = new Point(40, 70),
                RadiusX = 30,
                RadiusY = 30
            };
            var geometryGroup = new GeometryGroup { FillRule = FillRule.EvenOdd };
            geometryGroup.Children.Add(new LineGeometry(new Point(10, 10), new Point(50, 30)));
            geometryGroup.Children.Add(ellipseGeometry);
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(30, 55, 100, 30)));

            stack.Children.Add(new Path
            {
                Name = "GeometryGroupPath",
                Data = geometryGroup,
                Fill = CreateBrush("#CCCCFF"),
                Stroke = Brushes.Black,
                StrokeThickness = 4
            });
            canvas.Children.Add(stack);

            var radiusXSlider = CreateLineSlider("geogroupslider1", "RadiusX", 30, 40);
            var radiusYSlider = CreateLineSlider("geogroupslider2", "RadiusY", 30, 50);
            radiusXSlider.ValueChanged += delegate { ellipseGeometry.RadiusX = radiusXSlider.Value; };
            radiusYSlider.ValueChanged += delegate { ellipseGeometry.RadiusY = radiusYSlider.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(radiusXSlider);
            options.Children.Add(radiusYSlider);

            root.Children.Add(CreateExampleWithOptions(canvas, options));
            return root;
        }

        private static Grid CreateExampleWithOptions(UIElement example, FrameworkElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(example, 0);
            Grid.SetColumn(options, 1);
            options.Margin = new Thickness(24, 0, 0, 0);
            layout.Children.Add(example);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreateLineOptionsPanel()
        {
            return new StackPanel
            {
                Width = 220
            };
        }

        private static Slider CreateLineSlider(string name, string header, double minimum, double maximum)
        {
            var slider = new Slider
            {
                Name = name,
                Minimum = minimum,
                Maximum = maximum,
                Value = minimum,
                SmallChange = 1,
                TickFrequency = 0.5
            };
            ControlHelper.SetHeader(slider, header);
            return slider;
        }

        private static TextBlock AddLinePointLabel(Canvas canvas, string text, double left, double top)
        {
            var textBlock = new TextBlock
            {
                Text = text
            };
            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);
            Panel.SetZIndex(textBlock, 1);
            canvas.Children.Add(textBlock);
            return textBlock;
        }

        private static void SetVisibility(IEnumerable<UIElement> elements, Visibility visibility)
        {
            foreach (var element in elements)
            {
                element.Visibility = visibility;
            }
        }

        private static UIElement CreateShapeSample()
        {
            return CreateShapeEllipseExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateShapeExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Ellipse",
                    CreateShapeEllipseExampleContent(assignRootAutomationId: true),
                    ShapeEllipseXaml,
                    null),
                new GalleryExample(
                    "Rectangle",
                    CreateShapeRectangleExampleContent(),
                    ShapeRectangleXaml,
                    null),
                new GalleryExample(
                    "Polygon",
                    CreateShapePolygonExampleContent(),
                    ShapePolygonXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateShapeEllipseExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("Shape"));
            }

            var ellipse = new Ellipse
            {
                Name = "EllipseElement",
                Width = 100,
                Height = 100,
                Fill = Brushes.SteelBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            GalleryAutomation.WithAutomationId(ellipse, GalleryAutomation.SampleElementId("Shape", "Ellipse"));

            var heightSlider = CreateLineSlider("slider1", "Height", 100, 150);
            var widthSlider = CreateLineSlider("slider2", "Width", 100, 150);
            var strokeSlider = CreateLineSlider("slider3", "Stroke Thickness", 2, 10);
            heightSlider.ValueChanged += delegate { ellipse.Height = heightSlider.Value; };
            widthSlider.ValueChanged += delegate { ellipse.Width = widthSlider.Value; };
            strokeSlider.ValueChanged += delegate { ellipse.StrokeThickness = strokeSlider.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(heightSlider);
            options.Children.Add(widthSlider);
            options.Children.Add(strokeSlider);

            root.Children.Add(CreateExampleWithOptions(ellipse, options));
            return root;
        }

        private static GallerySamplePanel CreateShapeRectangleExampleContent()
        {
            var root = new GallerySamplePanel();
            var rectangle = new Rectangle
            {
                Name = "RectangleElement",
                Width = 100,
                Height = 100,
                Fill = Brushes.SteelBlue,
                RadiusX = 0,
                RadiusY = 0,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            var heightSlider = CreateLineSlider("recSlider1", "Height", 100, 150);
            var widthSlider = CreateLineSlider("recSlider2", "Width", 100, 150);
            var strokeSlider = CreateLineSlider("recSlider3", "Stroke Thickness", 2, 10);
            var radiusYSlider = CreateLineSlider("recSlider4", "Radius Y", 0, 100);
            var radiusXSlider = CreateLineSlider("recSlider5", "Radius X", 0, 100);
            heightSlider.ValueChanged += delegate { rectangle.Height = heightSlider.Value; };
            widthSlider.ValueChanged += delegate { rectangle.Width = widthSlider.Value; };
            strokeSlider.ValueChanged += delegate { rectangle.StrokeThickness = strokeSlider.Value; };
            radiusYSlider.ValueChanged += delegate { rectangle.RadiusY = radiusYSlider.Value; };
            radiusXSlider.ValueChanged += delegate { rectangle.RadiusX = radiusXSlider.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(heightSlider);
            options.Children.Add(widthSlider);
            options.Children.Add(strokeSlider);
            options.Children.Add(radiusYSlider);
            options.Children.Add(radiusXSlider);

            root.Children.Add(CreateExampleWithOptions(rectangle, options));
            return root;
        }

        private static GallerySamplePanel CreateShapePolygonExampleContent()
        {
            var root = new GallerySamplePanel();
            var canvas = new Canvas
            {
                Width = 320,
                Height = 200
            };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "A polygon is a connected series of lines that form a closed shape.",
                Margin = new Thickness(0, 0, 0, 15)
            });
            var polygon = new Polygon
            {
                Name = "PolygonElement",
                Fill = Brushes.SteelBlue,
                Points = new PointCollection { new Point(10, 100), new Point(60, 40), new Point(200, 40), new Point(250, 100) },
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            stack.Children.Add(polygon);
            canvas.Children.Add(stack);

            var pointLabels = new[]
            {
                AddLinePointLabel(canvas, "Point #1: (10,100)", 0, 150),
                AddLinePointLabel(canvas, "Point #2: (60,40)", 50, 40),
                AddLinePointLabel(canvas, "Point #3: (200,40)", 200, 40),
                AddLinePointLabel(canvas, "Point #4: (250,100)", 240, 150)
            };
            SetVisibility(pointLabels, Visibility.Collapsed);

            var toggleSwitch = new Mux.ToggleSwitch
            {
                Name = "ToggleSwitchPoly",
                Header = "Show points",
                IsOn = false
            };
            toggleSwitch.Toggled += delegate
            {
                SetVisibility(pointLabels, toggleSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed);
            };

            var slider = CreateLineSlider("polySlider1", "Stroke Thickness", 2, 10);
            slider.ValueChanged += delegate { polygon.StrokeThickness = slider.Value; };

            var options = CreateLineOptionsPanel();
            options.Children.Add(toggleSwitch);
            options.Children.Add(slider);

            root.Children.Add(CreateExampleWithOptions(canvas, options));
            return root;
        }

        private static UIElement CreateRadialGradientBrushSample()
        {
            return CreateRadialGradientBrushExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateRadialGradientBrushExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "RadialGradientBrush Sample",
                    CreateRadialGradientBrushExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "RadialGradientBrushSample_xaml.txt", System.IO.Path.Combine("Brushes", "RadialGradientBrushSample_xaml.txt")),
                    null)
            };
        }

        private static GallerySamplePanel CreateRadialGradientBrushExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("RadialGradientBrush"));
            }

            var brush = new RadialGradientBrush
            {
                Center = new Point(0.25, 0.25),
                GradientOrigin = new Point(0.5, 0.25),
                MappingMode = BrushMappingMode.RelativeToBoundingBox,
                RadiusX = 0.5,
                RadiusY = 0.5,
                SpreadMethod = GradientSpreadMethod.Pad
            };
            brush.GradientStops.Add(new GradientStop(Colors.Yellow, 0));
            brush.GradientStops.Add(new GradientStop(Colors.Blue, 1));

            var rect = new Rectangle
            {
                Name = "Rect",
                Width = 200,
                Height = 200,
                Fill = brush
            };
            GalleryAutomation.WithAutomationId(rect, GalleryAutomation.SampleElementId("RadialGradientBrush", "Rect"));

            var sample = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal
            };
            sample.Children.Add(rect);

            var mappingModeComboBox = new ComboBox
            {
                Name = "MappingModeComboBox"
            };
            ControlHelper.SetHeader(mappingModeComboBox, "MappingMode");
            mappingModeComboBox.Items.Add("RelativeToBoundingBox");
            mappingModeComboBox.Items.Add("Absolute");
            mappingModeComboBox.SelectedIndex = 0;

            var centerXSlider = CreateRadialGradientSlider("CenterXSlider", "Center.X");
            var centerYSlider = CreateRadialGradientSlider("CenterYSlider", "Center.Y");
            var radiusXSlider = CreateRadialGradientSlider("RadiusXSlider", "RadiusX");
            var radiusYSlider = CreateRadialGradientSlider("RadiusYSlider", "RadiusY");
            var originXSlider = CreateRadialGradientSlider("OriginXSlider", "GradientOrigin.X");
            var originYSlider = CreateRadialGradientSlider("OriginYSlider", "GradientOrigin.Y");

            var spreadMethodComboBox = new ComboBox
            {
                Name = "SpreadMethodComboBox",
                Margin = new Thickness(0, 10, 0, 0)
            };
            ControlHelper.SetHeader(spreadMethodComboBox, "SpreadMethod");
            spreadMethodComboBox.Items.Add("Pad");
            spreadMethodComboBox.Items.Add("Reflect");
            spreadMethodComboBox.Items.Add("Repeat");
            spreadMethodComboBox.SelectedIndex = 0;

            void UpdateBrushFromSliders()
            {
                brush.Center = new Point(centerXSlider.Value, centerYSlider.Value);
                brush.RadiusX = radiusXSlider.Value;
                brush.RadiusY = radiusYSlider.Value;
                brush.GradientOrigin = new Point(originXSlider.Value, originYSlider.Value);
            }

            void InitializeSliders()
            {
                if (brush.MappingMode == BrushMappingMode.Absolute)
                {
                    InitializeRadialGradientSlider(centerXSlider, 200, 100, 4, 10);
                    InitializeRadialGradientSlider(centerYSlider, 200, 100, 4, 10);
                    InitializeRadialGradientSlider(radiusXSlider, 200, 100, 4, 10);
                    InitializeRadialGradientSlider(radiusYSlider, 200, 100, 4, 10);
                    InitializeRadialGradientSlider(originXSlider, 200, 100, 4, 10);
                    InitializeRadialGradientSlider(originYSlider, 200, 100, 4, 10);
                }
                else
                {
                    InitializeRadialGradientSlider(centerXSlider, 1, 0.5, 0.02, 0.05);
                    InitializeRadialGradientSlider(centerYSlider, 1, 0.5, 0.02, 0.05);
                    InitializeRadialGradientSlider(radiusXSlider, 1, 0.5, 0.02, 0.05);
                    InitializeRadialGradientSlider(radiusYSlider, 1, 0.5, 0.02, 0.05);
                    InitializeRadialGradientSlider(originXSlider, 1, 0.5, 0.02, 0.05);
                    InitializeRadialGradientSlider(originYSlider, 1, 0.5, 0.02, 0.05);
                }

                UpdateBrushFromSliders();
            }

            centerXSlider.ValueChanged += delegate { UpdateBrushFromSliders(); };
            centerYSlider.ValueChanged += delegate { UpdateBrushFromSliders(); };
            radiusXSlider.ValueChanged += delegate { UpdateBrushFromSliders(); };
            radiusYSlider.ValueChanged += delegate { UpdateBrushFromSliders(); };
            originXSlider.ValueChanged += delegate { UpdateBrushFromSliders(); };
            originYSlider.ValueChanged += delegate { UpdateBrushFromSliders(); };
            mappingModeComboBox.SelectionChanged += delegate
            {
                var modeString = mappingModeComboBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(modeString))
                {
                    brush.MappingMode = (BrushMappingMode)Enum.Parse(typeof(BrushMappingMode), modeString);
                    InitializeSliders();
                }
            };
            spreadMethodComboBox.SelectionChanged += delegate
            {
                var methodString = spreadMethodComboBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(methodString))
                {
                    brush.SpreadMethod = (GradientSpreadMethod)Enum.Parse(typeof(GradientSpreadMethod), methodString);
                }
            };
            InitializeSliders();

            var options = new Grid();
            options.RowDefinitions.Add(new RowDefinition());
            options.RowDefinitions.Add(new RowDefinition());
            options.RowDefinitions.Add(new RowDefinition());
            options.RowDefinitions.Add(new RowDefinition());
            options.RowDefinitions.Add(new RowDefinition());
            options.ColumnDefinitions.Add(new ColumnDefinition());
            options.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumnSpan(mappingModeComboBox, 2);
            Grid.SetRow(centerXSlider, 1);
            Grid.SetRow(centerYSlider, 1);
            Grid.SetColumn(centerYSlider, 1);
            Grid.SetRow(radiusXSlider, 2);
            Grid.SetRow(radiusYSlider, 2);
            Grid.SetColumn(radiusYSlider, 1);
            Grid.SetRow(originXSlider, 3);
            Grid.SetRow(originYSlider, 3);
            Grid.SetColumn(originYSlider, 1);
            Grid.SetRow(spreadMethodComboBox, 4);
            Grid.SetColumnSpan(spreadMethodComboBox, 2);
            options.Children.Add(mappingModeComboBox);
            options.Children.Add(centerXSlider);
            options.Children.Add(centerYSlider);
            options.Children.Add(radiusXSlider);
            options.Children.Add(radiusYSlider);
            options.Children.Add(originXSlider);
            options.Children.Add(originYSlider);
            options.Children.Add(spreadMethodComboBox);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(sample, 0);
            Grid.SetColumn(options, 1);
            options.Margin = new Thickness(24, 0, 0, 0);
            layout.Children.Add(sample);
            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
        }

        private static Slider CreateRadialGradientSlider(string name, string header)
        {
            var slider = new Slider
            {
                Name = name,
                SmallChange = 0.05
            };
            ControlHelper.SetHeader(slider, header);
            return slider;
        }

        private static void InitializeRadialGradientSlider(Slider slider, double maximum, double value, double tickFrequency, double smallChange)
        {
            slider.Maximum = maximum;
            slider.TickFrequency = tickFrequency;
            slider.SmallChange = smallChange;
            slider.Value = value;
        }

        private static UIElement CreateSystemBackdropsSample()
        {
            return CreateSystemBackdropsBackdropTypesExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateSystemBackdropsExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Backdrop types",
                    CreateSystemBackdropsBackdropTypesExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleBackdropTypes_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleBackdropTypes_cs.txt")),
                new GalleryExample(
                    "MicaController",
                    CreateSystemBackdropsMicaControllerExampleContent(),
                    null,
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleMicaController.txt")),
                new GalleryExample(
                    "DesktopAcrylicController",
                    CreateSystemBackdropsDesktopAcrylicControllerExampleContent(),
                    null,
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleDesktopAcrylicController.txt"))
            };
        }

        private static GallerySamplePanel CreateSystemBackdropsBackdropTypesExampleContent(bool assignRootAutomationId)
        {
            var root = CreateSystemBackdropsExampleRoot(assignRootAutomationId);
            var stack = CreateSystemBackdropsStack();
            var text = CreateSystemBackdropsTextBlock();
            AddInlineText(text, "A window can use one of the following system backdrops:");
            AddLineBreak(text);
            AddInlineText(text, "1. ");
            AddInlineBold(text, "Mica");
            AddInlineText(text, " \u2014 An opaque material that samples the desktop wallpaper once to tint the window background. Best for main app windows.");
            AddLineBreak(text);
            AddInlineText(text, "2. ");
            AddInlineBold(text, "Mica Alt");
            AddInlineText(text, " \u2014 A variant of Mica with stronger tinting. Recommended for apps with a tabbed title bar.");
            AddLineBreak(text);
            AddInlineText(text, "3. ");
            AddInlineBold(text, "Desktop Acrylic (Base)");
            AddInlineText(text, " \u2014 A semi-transparent material that shows a blurred view of the content behind the window.");
            AddLineBreak(text);
            AddInlineText(text, "4. ");
            AddInlineBold(text, "Desktop Acrylic (Thin)");
            AddInlineText(text, " \u2014 A lighter variant of Desktop Acrylic with more transparency.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineBold(text, "Mica vs. Acrylic:");
            AddInlineText(text, " Mica is opaque and renders the desktop wallpaper within the window background. Desktop Acrylic is semi-transparent and reveals a blurred view of what is behind the window in real-time. Mica is more performant because it captures the wallpaper only once, while Acrylic updates continuously.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "There are three backdrop types in the API:");
            AddLineBreak(text);
            AddInlineText(text, "\u2022 ");
            AddInlineBold(text, "SystemBackdrop");
            AddInlineText(text, " \u2014 The base class of every backdrop type.");
            AddLineBreak(text);
            AddInlineText(text, "\u2022 ");
            AddInlineBold(text, "MicaBackdrop");
            AddInlineText(text, " \u2014 Applies the Mica material. Set the Kind property to switch between Base and Alt.");
            AddLineBreak(text);
            AddInlineText(text, "\u2022 ");
            AddInlineBold(text, "DesktopAcrylicBackdrop");
            AddInlineText(text, " \u2014 Applies the Desktop Acrylic material (Base type only).");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "All Mica variants require Windows 11 build 22000 or later. In-app acrylic (AcrylicBrush) is a separate XAML brush used within UI elements, not a window backdrop.");
            stack.Children.Add(text);

            var button = CreateSystemBackdropsShowWindowButton("SystemBackdrops", "ShowWindowButton", "Show window");
            button.Click += delegate
            {
                ShowSystemBackdropPreviewWindow(button, "Built-in system backdrops", "Mica", CreateBrush("#F3F3F3"));
            };
            stack.Children.Add(button);
            root.Children.Add(stack);
            return root;
        }

        private static GallerySamplePanel CreateSystemBackdropsMicaControllerExampleContent()
        {
            var root = CreateSystemBackdropsExampleRoot(assignRootAutomationId: false);
            var stack = CreateSystemBackdropsStack();
            var text = CreateSystemBackdropsTextBlock();
            AddInlineText(text, "MicaController provides a customizable way to apply the Mica material. You can modify: FallbackColor, Kind, LuminosityOpacity, TintColor, and TintOpacity.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "There are 2 kinds of Mica:");
            AddLineBreak(text);
            AddInlineText(text, "1. ");
            AddInlineBold(text, "Base");
            AddInlineText(text, " \u2014 The default, lighter appearance.");
            AddLineBreak(text);
            AddInlineText(text, "2. ");
            AddInlineBold(text, "Alt");
            AddInlineText(text, " \u2014 A darker appearance with stronger tinting of the desktop wallpaper.");
            stack.Children.Add(text);

            var button = CreateSystemBackdropsShowWindowButton("SystemBackdrops", "MicaControllerShowWindowButton", "Show window");
            button.Click += delegate
            {
                ShowSystemBackdropPreviewWindow(button, "MicaController", "Mica", CreateBrush("#F3F3F3"));
            };
            stack.Children.Add(button);
            root.Children.Add(stack);
            return root;
        }

        private static GallerySamplePanel CreateSystemBackdropsDesktopAcrylicControllerExampleContent()
        {
            var root = CreateSystemBackdropsExampleRoot(assignRootAutomationId: false);
            var stack = CreateSystemBackdropsStack();
            var text = CreateSystemBackdropsTextBlock();
            AddInlineText(text, "DesktopAcrylicController provides a customizable way to apply the Desktop Acrylic material. It supports the same customization properties as MicaController.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "There are 2 kinds of Desktop Acrylic:");
            AddLineBreak(text);
            AddInlineText(text, "1. ");
            AddInlineBold(text, "Base");
            AddInlineText(text, " \u2014 The default, darker appearance with less transparency.");
            AddLineBreak(text);
            AddInlineText(text, "2. ");
            AddInlineBold(text, "Thin");
            AddInlineText(text, " \u2014 A lighter appearance with more transparency.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "Note: DesktopAcrylicBackdrop always uses the Base kind. To use the Thin kind, you must use DesktopAcrylicController directly.");
            stack.Children.Add(text);

            var button = CreateSystemBackdropsShowWindowButton("SystemBackdrops", "DesktopAcrylicControllerShowWindowButton", "Show window");
            button.Click += delegate
            {
                ShowSystemBackdropPreviewWindow(button, "DesktopAcrylicController", "Desktop Acrylic", CreateBrush("#DDEEF6FF"));
            };
            stack.Children.Add(button);
            root.Children.Add(stack);
            return root;
        }

        private static UIElement CreateSystemBackdropElementSample()
        {
            return CreateSystemBackdropElementExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateSystemBackdropElementExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            var acrylicXaml = FindSampleCodeText(sampleSnippets, "SystemBackdropElementAcrylic_xaml.txt");
            var micaXaml = FindSampleCodeText(sampleSnippets, "SystemBackdropElementMica_xaml.txt");
            var micaAltXaml = FindSampleCodeText(sampleSnippets, "SystemBackdropElementMicaAlt_xaml.txt");
            return new[]
            {
                new GalleryExample(
                    "SystemBackdropElement Sample",
                    CreateSystemBackdropElementExampleContent(assignRootAutomationId: true),
                    acrylicXaml,
                    null,
                    new Thickness(10),
                    new[] { micaXaml, micaAltXaml })
            };
        }

        private static GallerySamplePanel CreateSystemBackdropElementExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("SystemBackdropElement"));
            }

            var dynamicBackdropHost = new Border
            {
                Name = "DynamicBackdropHost",
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush("#DDEEF6FF")
            };
            var button = new Button
            {
                Content = "Click Me",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("SystemBackdropElement", "Button"));

            var example = new Grid
            {
                Width = 300,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            example.Children.Add(dynamicBackdropHost);
            example.Children.Add(button);

            var backdropTypeComboBox = new ComboBox
            {
                Name = "BackdropTypeComboBox",
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 180
            };
            ControlHelper.SetHeader(backdropTypeComboBox, "Backdrop Type");
            backdropTypeComboBox.Items.Add(CreateComboBoxItem("Acrylic", "Acrylic"));
            backdropTypeComboBox.Items.Add(CreateComboBoxItem("Mica", "Mica"));
            backdropTypeComboBox.Items.Add(CreateComboBoxItem("Mica Alt", "MicaAlt"));

            var cornerRadiusSlider = new Slider
            {
                Name = "CornerRadiusSlider",
                Minimum = 0,
                Maximum = 50,
                TickFrequency = 1,
                Value = 8,
                Width = 220,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(cornerRadiusSlider, "Corner radius");

            backdropTypeComboBox.SelectionChanged += delegate
            {
                var selectedItem = backdropTypeComboBox.SelectedItem as ComboBoxItem;
                switch (selectedItem == null ? null : selectedItem.Tag as string)
                {
                    case "Mica":
                        dynamicBackdropHost.Background = CreateBrush("#F3F3F3");
                        break;
                    case "MicaAlt":
                        dynamicBackdropHost.Background = CreateBrush("#E9EEF5");
                        break;
                    default:
                        dynamicBackdropHost.Background = CreateBrush("#DDEEF6FF");
                        break;
                }
            };
            cornerRadiusSlider.ValueChanged += delegate
            {
                dynamicBackdropHost.CornerRadius = new CornerRadius(cornerRadiusSlider.Value);
            };

            var options = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(backdropTypeComboBox);
            options.Children.Add(cornerRadiusSlider);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(example, 0);
            Grid.SetColumn(options, 1);
            layout.Children.Add(example);
            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
        }

        private static UIElement CreateThemeShadowSample()
        {
            return CreateThemeShadowExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateThemeShadowExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "ThemeShadow applied to a Border",
                    CreateThemeShadowExampleContent(assignRootAutomationId: true),
                    ThemeShadowXaml,
                    ThemeShadowCSharp)
            };
        }

        private static GallerySamplePanel CreateThemeShadowExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ThemeShadow"));
            }

            var shadowCastGrid = new Grid
            {
                Name = "ShadowCastGrid"
            };

            var shadowRect = new Border
            {
                Name = "ShadowRect",
                Width = 200,
                Height = 200
            };
            shadowRect.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            shadowRect.SetResourceReference(Border.CornerRadiusProperty, "OverlayCornerRadius");
            GalleryAutomation.WithAutomationId(shadowRect, GalleryAutomation.SampleElementId("ThemeShadow", "ShadowRect"));

            var shadow = new ThemeShadowChrome
            {
                Name = "shadow",
                Depth = 32,
                TranslationZ = 32,
                Child = shadowRect,
                Margin = new Thickness(36),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            shadow.SetResourceReference(ThemeShadowChrome.CornerRadiusProperty, "OverlayCornerRadius");

            var exampleGrid = new Grid
            {
                Name = "Example3Grid",
                MinWidth = 272,
                MinHeight = 272
            };
            exampleGrid.Children.Add(shadowCastGrid);
            exampleGrid.Children.Add(shadow);

            var translationSlider = new Slider
            {
                Name = "TranslationSliderInApp",
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                Minimum = 0,
                Maximum = 64,
                SmallChange = 1,
                TickFrequency = 1,
                Value = 32
            };
            AutomationProperties.SetName(translationSlider, "shadow intensity");
            ControlHelper.SetHeader(translationSlider, "Z-translation");
            translationSlider.ValueChanged += delegate
            {
                shadow.Depth = translationSlider.Value;
                shadow.TranslationZ = translationSlider.Value;
            };

            var options = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(translationSlider);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(exampleGrid, 0);
            Grid.SetColumn(options, 1);
            layout.Children.Add(exampleGrid);
            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateSystemBackdropsExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("SystemBackdrops"));
            }

            return root;
        }

        private static StackPanel CreateSystemBackdropsStack()
        {
            return new StackPanel();
        }

        private static TextBlock CreateSystemBackdropsTextBlock()
        {
            return new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static Button CreateSystemBackdropsShowWindowButton(string controlId, string elementName, string content)
        {
            var button = new Button
            {
                Name = elementName,
                Content = content,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, content);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId(controlId, elementName));
            return button;
        }

        private static void AddInlineText(TextBlock textBlock, string text)
        {
            textBlock.Inlines.Add(new Run(text));
        }

        private static void AddInlineBold(TextBlock textBlock, string text)
        {
            textBlock.Inlines.Add(new Bold(new Run(text)));
        }

        private static void AddLineBreak(TextBlock textBlock)
        {
            textBlock.Inlines.Add(new LineBreak());
        }

        private static void ShowSystemBackdropPreviewWindow(FrameworkElement ownerElement, string title, string backdropName, Brush backdropBrush)
        {
            var window = new Window
            {
                Title = title,
                Width = 560,
                Height = 360,
                MinWidth = 420,
                MinHeight = 260,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = CreateBrush("#F9F9F9"),
                Content = CreateSystemBackdropPreviewWindowContent(title, backdropName, backdropBrush)
            };
            var owner = Window.GetWindow(ownerElement);
            if (owner != null)
            {
                window.Owner = owner;
            }

            ThemeManager.SetIsThemeAware(window, true);
            WindowHelper.SetUseModernWindowStyle(window, true);
            Mux.TitleBar.SetIsIconVisible(window, true);
            window.Show();
        }

        private static FrameworkElement CreateSystemBackdropPreviewWindowContent(string title, string backdropName, Brush backdropBrush)
        {
            var root = new Grid
            {
                Background = CreateBrush("#252525"),
                Margin = new Thickness(0)
            };
            root.Children.Add(CreateBackdropCard(
                backdropName,
                backdropBrush,
                title,
                "This WPF preview represents the WinUI system backdrop sample window."));
            return root;
        }

        private static Border CreateBackdropCard(string name, Brush brush, string title, string subtitle)
        {
            return new Border
            {
                Width = 430,
                Height = 260,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CornerRadius = new CornerRadius(8),
                Background = brush,
                Padding = new Thickness(20),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 24,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = subtitle,
                            Opacity = 0.72,
                            Margin = new Thickness(0, 8, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = name,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 18, 0, 0)
                        }
                    }
                }
            };
        }

        private static StackPanel CreateSizingColumn(string title, double height, Thickness padding, double spacing)
        {
            var column = new StackPanel
            {
                Width = 220,
                Margin = new Thickness(0, 0, 24, 0)
            };
            column.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            column.Children.Add(new Button
            {
                Content = "Button",
                MinHeight = height,
                Padding = padding,
                Margin = new Thickness(0, 0, 0, spacing),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            column.Children.Add(new TextBox
            {
                Text = "TextBox",
                MinHeight = height,
                Padding = padding,
                Margin = new Thickness(0, 0, 0, spacing)
            });
            column.Children.Add(new ComboBox
            {
                ItemsSource = new[] { "First", "Second", "Third" },
                SelectedIndex = 0,
                MinHeight = height,
                Padding = padding
            });
            return column;
        }

        private static StackPanel CreateIconElementStack(string description)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return stack;
        }

        private static DrawingImage CreateCameraPanoramaDrawing()
        {
            var drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(CreateBrush("#E6F4FF"), null, Geometry.Parse("M 0,0 L 50,0 L 50,32 L 0,32 Z")));
            drawing.Children.Add(new GeometryDrawing(CreateBrush("#107C10"), null, Geometry.Parse("M 0,23 L 12,13 L 21,21 L 31,10 L 50,25 L 50,32 L 0,32 Z")));
            drawing.Children.Add(new GeometryDrawing(CreateBrush("#FCE100"), null, new EllipseGeometry(new Point(37, 7), 4, 4)));
            drawing.Children.Add(new GeometryDrawing(null, new Pen(CreateBrush("#1F1F1F"), 2), Geometry.Parse("M 1,1 L 49,1 L 49,31 L 1,31 Z")));

            var image = new DrawingImage(drawing);
            image.Freeze();
            return image;
        }

        private static StackPanel CreateIconColumn(string label, UIElement icon)
        {
            var column = new StackPanel
            {
                Width = 116,
                Margin = new Thickness(0, 0, 16, 0)
            };
            var frame = new Border
            {
                Width = 80,
                Height = 70,
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = icon,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            column.Children.Add(frame);
            column.Children.Add(new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return column;
        }

        private static void AddShape(Canvas canvas, Shape shape, double left, double top)
        {
            Canvas.SetLeft(shape, left);
            Canvas.SetTop(shape, top);
            canvas.Children.Add(shape);
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
        }

        private static ComboBoxItem CreateComboBoxItem(string content, string tag)
        {
            return new ComboBoxItem
            {
                Content = content,
                Tag = tag
            };
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

        private static BitmapImage CreateBitmap(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/ModernWpf.Gallery;component/" + path;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string fileName)
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

            return null;
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string fileName, string fallbackRelativePath)
        {
            var text = FindSampleCodeText(snippets, fileName);
            if (text != null)
            {
                return text;
            }

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", fallbackRelativePath);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        }
    }
}
