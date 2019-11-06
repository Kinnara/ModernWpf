using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class VisualStateGroupListener : FrameworkElement
    {
        private VisualStateGroup _group;

        static VisualStateGroupListener()
        {
            VisibilityProperty.OverrideMetadata(typeof(VisualStateGroupListener), new FrameworkPropertyMetadata(Visibility.Collapsed));
        }

        public VisualStateGroupListener()
        {
        }

        #region GroupName

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(
                nameof(GroupName),
                typeof(string),
                typeof(VisualStateGroupListener),
                new PropertyMetadata(OnGroupNameChanged));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VisualStateGroupListener)d).OnGroupNameChanged(e);
        }

        private void OnGroupNameChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_group != null)
            {
                _group.CurrentStateChanged -= OnCurrentStateChanged;
                _group = null;
            }

            string groupName = (string)e.NewValue;
            if (!string.IsNullOrEmpty(groupName))
            {
                if (TemplatedParent is Control parentControl)
                {
                    var templateRoot = parentControl.GetTemplateRoot();
                    if (templateRoot != null)
                    {
                        if (templateRoot.FindName(groupName) is VisualStateGroup group)
                        {
                            _group = group;
                            _group.CurrentStateChanged += OnCurrentStateChanged;
                        }
                    }
                }

                AssertGroupIsNotNull();
            }

            UpdateCurrentStateName(_group?.CurrentState);
        }

        #endregion

        #region CurrentStateName

        private static readonly DependencyPropertyKey CurrentStateNamePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurrentStateName),
                typeof(string),
                typeof(VisualStateGroupListener),
                null);

        public static readonly DependencyProperty CurrentStateNameProperty =
            CurrentStateNamePropertyKey.DependencyProperty;

        public string CurrentStateName
        {
            get => (string)GetValue(CurrentStateNameProperty);
            private set => SetValue(CurrentStateNamePropertyKey, value);
        }

        #endregion

        private void UpdateCurrentStateName(VisualState currentState)
        {
            if (currentState != null)
            {
                CurrentStateName = currentState.Name;
            }
            else
            {
                ClearValue(CurrentStateNamePropertyKey);
            }
        }

        private void OnCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateCurrentStateName(e.NewState);
        }

        [Conditional("DEBUG")]
        private void AssertGroupIsNotNull()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Debug.Assert(_group != null);
            }
        }
    }
}
