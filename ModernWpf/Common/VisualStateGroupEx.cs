using System.Windows;

namespace ModernWpf
{
    public class VisualStateGroupEx : VisualStateGroup
    {
        public VisualStateGroupEx()
        {
            CurrentStateChanged += OnCurrentStateChanged;
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

        private void OnCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
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
