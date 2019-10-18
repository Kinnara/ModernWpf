using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf
{
    internal static class DesignMode
    {
        private static readonly Lazy<bool> _designModeEnabled =
            new Lazy<bool>(() => DesignerProperties.GetIsInDesignMode(new DependencyObject()));

        public static bool DesignModeEnabled => _designModeEnabled.Value;
    }
}
