using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ModernWpf.Gallery.Testing
{
    internal static class GalleryDiagnostics
    {
        private static readonly object Gate = new object();

        public static bool IsEnabled { get; private set; }
        public static bool OpenInteractions { get; private set; }
        public static string ArtifactDirectory { get; private set; }
        public static string Theme { get; private set; }
        public static string CurrentRoute { get; private set; } = "home";
        public static string ReadyState { get; private set; } = "Starting";
        public static string LastException { get; private set; } = string.Empty;

        public static void Configure(GalleryLaunchOptions options)
        {
            if (options == null)
            {
                return;
            }

            lock (Gate)
            {
                IsEnabled = options.VisualTestMode;
                OpenInteractions = options.OpenInteractions;
                ArtifactDirectory = options.ArtifactDirectory;
                Theme = options.Theme;
                CurrentRoute = "home";
                ReadyState = "Starting";
                LastException = string.Empty;
            }
        }

        public static void ResetForTests()
        {
            lock (Gate)
            {
                IsEnabled = false;
                OpenInteractions = false;
                ArtifactDirectory = null;
                Theme = null;
                CurrentRoute = "home";
                ReadyState = "Starting";
                LastException = string.Empty;
            }
        }

        public static void RecordRoute(string route)
        {
            lock (Gate)
            {
                CurrentRoute = string.IsNullOrWhiteSpace(route) ? "home" : route;
            }
        }

        public static void SetReadyState(string state)
        {
            lock (Gate)
            {
                ReadyState = string.IsNullOrWhiteSpace(state) ? "Unknown" : state;
            }
        }

        public static void RecordException(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            var text = FormatException(exception);
            lock (Gate)
            {
                LastException = text;
            }

            if (IsEnabled && !string.IsNullOrWhiteSpace(ArtifactDirectory))
            {
                TryAppendExceptionLog(text);
            }
        }

        public static void WriteVisualArtifacts(DependencyObject root)
        {
            if (!IsEnabled || string.IsNullOrWhiteSpace(ArtifactDirectory) || root == null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(ArtifactDirectory);
                WriteVisualArtifactsCore(root);
            }
            catch (Exception ex)
            {
                RecordException(ex);
            }
        }

        public static void PrepareInteractiveVisualState(DependencyObject root)
        {
            if (!IsEnabled || !OpenInteractions || root == null)
            {
                return;
            }

            try
            {
                var teachingTipButton = FindByAutomationId(root, "GallerySample_TeachingTip_ShowButton") as ButtonBase;
                if (teachingTipButton != null)
                {
                    teachingTipButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    root.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                }
            }
            catch (Exception ex)
            {
                RecordException(ex);
            }
        }

        private static string FormatException(Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append(exception.GetType().FullName);
            builder.Append(": ");
            builder.Append(exception.Message);

            if (exception.InnerException != null)
            {
                builder.Append(" | Inner: ");
                builder.Append(exception.InnerException.GetType().FullName);
                builder.Append(": ");
                builder.Append(exception.InnerException.Message);
            }

            return builder.ToString();
        }

        private static void TryAppendExceptionLog(string text)
        {
            try
            {
                Directory.CreateDirectory(ArtifactDirectory);
                File.AppendAllText(
                    Path.Combine(ArtifactDirectory, "modernwpf-gallery-exceptions.log"),
                    DateTimeOffset.Now.ToString("o") + " " + text + Environment.NewLine);
            }
            catch
            {
                // Diagnostics must not create new Gallery failures.
            }
        }

        private static void WriteVisualArtifactsCore(DependencyObject root)
        {
            var element = root as FrameworkElement;
            if (element != null)
            {
                var automationId = AutomationProperties.GetAutomationId(element);
                if (ShouldWriteVisualArtifact(automationId))
                {
                    WriteElementPng(element, Path.Combine(ArtifactDirectory, SanitizeFileName(automationId) + ".png"));
                }

                var popup = element as Popup;
                if (popup?.Child != null)
                {
                    WriteVisualArtifactsCore(popup.Child);
                }
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                WriteVisualArtifactsCore(VisualTreeHelper.GetChild(root, i));
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
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    new SolidColorBrush(GetArtifactBackgroundColor()),
                    null,
                    new Rect(0, 0, width, height));
                drawingContext.DrawRectangle(
                    new VisualBrush(element),
                    null,
                    new Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(path))
            {
                encoder.Save(stream);
            }
        }

        private static bool ShouldWriteVisualArtifact(string automationId)
        {
            return !string.IsNullOrEmpty(automationId) &&
                (automationId.StartsWith("GallerySample_", StringComparison.Ordinal) ||
                    string.Equals(automationId, "ContentRootGrid", StringComparison.Ordinal));
        }

        private static DependencyObject FindByAutomationId(DependencyObject root, string automationId)
        {
            var element = root as UIElement;
            if (element != null && AutomationProperties.GetAutomationId(element) == automationId)
            {
                return root;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindByAutomationId(VisualTreeHelper.GetChild(root, i), automationId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Color GetArtifactBackgroundColor()
        {
            return string.Equals(Theme, "Dark", StringComparison.OrdinalIgnoreCase)
                ? Color.FromRgb(0x20, 0x20, 0x20)
                : Color.FromRgb(0xF3, 0xF3, 0xF3);
        }

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                builder.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
            }

            return builder.ToString();
        }
    }
}
