using System.ComponentModel;
using System.Windows;

namespace ModernWpf.DesignTime
{
    public abstract class IntellisenseResourcesBase : ResourceDictionary, ISupportInitialize
    {
        protected IntellisenseResourcesBase()
        {
        }

        void ISupportInitialize.EndInit()
        {
            Clear();
            MergedDictionaries.Clear();
        }
    }
}
