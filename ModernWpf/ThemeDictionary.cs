using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf
{
    public class ThemeDictionary : ResourceDictionary, ISupportInitialize
    {
        public ThemeDictionary()
        {
        }

        public string Key { get; set; }

        void ISupportInitialize.EndInit()
        {
            EndInit();

            if (string.IsNullOrEmpty(Key))
            {
                throw new InvalidOperationException(nameof(Key) + " must be set.");
            }
        }
    }
}
