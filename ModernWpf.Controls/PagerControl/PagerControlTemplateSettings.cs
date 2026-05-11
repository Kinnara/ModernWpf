using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class PagerControlTemplateSettings : DependencyObject
    {
        internal PagerControlTemplateSettings()
        {
            Pages = new ObservableCollection<object>();
            NumberPanelItems = new ObservableCollection<object>();
        }

        public IList<object> Pages { get; }

        public IList<object> NumberPanelItems { get; }
    }
}
