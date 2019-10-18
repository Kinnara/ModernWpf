using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace ModernWpf
{
    internal static class MenuDropAlignmentHelper
    {
        private static readonly FieldInfo _menuDropAlignmentField;

        static MenuDropAlignmentHelper()
        {
            try
            {
                _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            }
            catch (Exception)
            {
            }

            Debug.Assert(_menuDropAlignmentField != null);
            if (_menuDropAlignmentField != null)
            {
                EnsureStandardPopupAlignment();
                SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            }
        }

        public static void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment)
            {
                try
                {
                    _menuDropAlignmentField.SetValue(null, false);
                }
                catch (Exception)
                {
                }

                Debug.Assert(!SystemParameters.MenuDropAlignment);
            }
        }

        private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.MenuDropAlignment))
            {
                EnsureStandardPopupAlignment();
            }
        }
    }
}
