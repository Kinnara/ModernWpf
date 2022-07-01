using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    public class PipsPagerTemplateSettings : DependencyObject
    {
        internal PipsPagerTemplateSettings()
        {
        }

        #region Dispatcher

        private static readonly DependencyPropertyKey DispatcherPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Dispatcher),
                typeof(DispatcherObject),
                typeof(PipsPagerTemplateSettings),
                null);

        public static readonly DependencyProperty DispatcherProperty = DispatcherPropertyKey.DependencyProperty;

        public DispatcherObject Dispatcher
        {
            get => (DispatcherObject)GetValue(DispatcherProperty);
            internal set => SetValue(DispatcherPropertyKey, value);
        }

        #endregion

        #region PipsPagerItems

        internal static readonly DependencyPropertyKey PipsPagerItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PipsPagerItems),
                typeof(ObservableCollection<int>),
                typeof(PipsPagerTemplateSettings),
                null);

        public static readonly DependencyProperty PipsPagerItemsProperty = PipsPagerItemsPropertyKey.DependencyProperty;

        public ObservableCollection<int> PipsPagerItems
        {
            get => (ObservableCollection<int>)GetValue(PipsPagerItemsProperty);
            internal set => SetValue(PipsPagerItemsPropertyKey, value);
        }

        #endregion
    }
}
