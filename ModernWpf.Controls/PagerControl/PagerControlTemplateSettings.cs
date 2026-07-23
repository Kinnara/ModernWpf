using System.Collections.Generic;
using System.Windows;

namespace ModernWpf.Controls
{
    public class PagerControlTemplateSettings : DependencyObject
    {
        internal PagerControlTemplateSettings(IList<object> pages, IList<object> numberPanelItems)
        {
            Pages = pages;
            NumberPanelItems = numberPanelItems;
        }

        public IList<object> Pages { get; }

        public IList<object> NumberPanelItems { get; }
    }
}
