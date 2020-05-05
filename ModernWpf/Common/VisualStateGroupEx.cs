using System;
using System.ComponentModel;
using System.Windows;

namespace ModernWpf
{
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class VisualStateGroupEx : VisualStateGroup
    {
        public VisualStateGroupEx()
        {
            CurrentStateChanging += OnCurrentStateChanging;
        }

        #region CurrentStateName

        private static readonly DependencyPropertyKey CurrentStateNamePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurrentStateName),
                typeof(string),
                typeof(VisualStateGroupEx),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CurrentStateNameProperty =
            CurrentStateNamePropertyKey.DependencyProperty;

        public string CurrentStateName
        {
            get => (string)GetValue(CurrentStateNameProperty);
            private set => SetValue(CurrentStateNamePropertyKey, value);
        }

        #endregion

        private void OnCurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState != null)
            {
                CurrentStateName = e.NewState.Name;
            }
            else
            {
                ClearValue(CurrentStateNamePropertyKey);
            }
        }
    }
}
