using System;
using System.IO;
using System.Text;

namespace ModernWpf.Gallery.Testing
{
    internal static class GalleryDiagnostics
    {
        private static readonly object Gate = new object();

        public static bool IsEnabled { get; private set; }
        public static string ArtifactDirectory { get; private set; }
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
                ArtifactDirectory = options.ArtifactDirectory;
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
                ArtifactDirectory = null;
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
    }
}
