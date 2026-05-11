using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace ModernWpf.Controls
{
    public class PipsPagerTemplateSettings : DependencyObject
    {
        internal PipsPagerTemplateSettings()
        {
        }

        public IList<int> PipsPagerItems { get; } = new ObservableCollection<int>();
    }
}
