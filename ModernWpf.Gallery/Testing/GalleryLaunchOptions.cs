using System;

namespace ModernWpf.Gallery.Testing
{
    internal sealed class GalleryLaunchOptions
    {
        private GalleryLaunchOptions()
        {
        }

        public bool VisualTestMode { get; private set; }
        public string ArtifactDirectory { get; private set; }
        public string InitialRoute { get; private set; }
        public string Theme { get; private set; }

        public static GalleryLaunchOptions Parse(string[] args)
        {
            var options = new GalleryLaunchOptions();

            if (args == null)
            {
                return options;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.IsNullOrWhiteSpace(arg))
                {
                    continue;
                }

                if (IsFlag(arg, "--visual-test"))
                {
                    options.VisualTestMode = true;
                    continue;
                }

                if (TryReadValue(args, ref i, arg, "--visual-artifact-dir", out var artifactDirectory))
                {
                    options.ArtifactDirectory = artifactDirectory;
                    continue;
                }

                if (TryReadValue(args, ref i, arg, "--route", out var route))
                {
                    options.InitialRoute = route;
                    continue;
                }

                if (TryReadValue(args, ref i, arg, "--theme", out var theme))
                {
                    options.Theme = theme;
                    continue;
                }

                if (!arg.StartsWith("-", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(options.InitialRoute))
                {
                    options.InitialRoute = arg;
                }
            }

            return options;
        }

        private static bool IsFlag(string arg, string flag)
        {
            return string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryReadValue(string[] args, ref int index, string arg, string optionName, out string value)
        {
            value = null;

            if (arg.StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                value = arg.Substring(optionName.Length + 1);
                return true;
            }

            if (!string.Equals(arg, optionName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (index + 1 < args.Length)
            {
                value = args[++index];
            }

            return true;
        }
    }
}
