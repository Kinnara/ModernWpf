using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf.DesignTime
{
    public abstract class IntellisenseResourcesBase : ResourceDictionary, ISupportInitialize
    {
        protected IntellisenseResourcesBase()
        {
        }

        public new Uri Source
        {
            get => base.Source;
            set
            {
                if (DesignMode.DesignModeEnabled)
                {
                    Source = value;
                }
            }
        }

        void ISupportInitialize.EndInit()
        {
            if (Source != null)
            {
                Source = null;
            }
        }
    }
}
