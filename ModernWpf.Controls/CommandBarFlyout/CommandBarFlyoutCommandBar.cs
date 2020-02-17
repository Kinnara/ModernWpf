using System;
using System.Windows;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = ToolBarName, Type = typeof(CommandBarFlyoutToolBar))]
    public class CommandBarFlyoutCommandBar : CommandBar
    {
        static CommandBarFlyoutCommandBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBarFlyoutCommandBar),
                new FrameworkPropertyMetadata(typeof(CommandBarFlyoutCommandBar)));
        }

        public CommandBarFlyoutCommandBar()
        {
            IsVisibleChanged += OnIsVisibleChanged;
        }

        internal WeakReference<CommandBarFlyout> OwningFlyout => m_owningFlyout;

        internal void SetOwningFlyout(
            CommandBarFlyout owningFlyout)
        {
            m_owningFlyout = new WeakReference<CommandBarFlyout>(owningFlyout);
        }

        internal bool HasOpenAnimation()
        {
            return m_toolBar != null ? m_toolBar.HasOpenAnimation() : false;
        }

        internal void PlayOpenAnimation()
        {
            m_toolBar?.PlayOpenAnimation();
        }

        internal bool HasCloseAnimation()
        {
            return m_toolBar != null ? m_toolBar.HasCloseAnimation() : false;
        }

        internal void PlayCloseAnimation(Action onCompleteFunc)
        {
            m_toolBar?.PlayCloseAnimation(onCompleteFunc);
        }

        internal void ClearShadow()
        {
            m_toolBar?.ClearShadow();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_toolBar = GetTemplateChild(ToolBarName) as CommandBarFlyoutToolBar;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                InputManager.Current.PostProcessInput += OnPostProcessInput;
            }
            else
            {
                InputManager.Current.PostProcessInput -= OnPostProcessInput;
            }
        }

        private void OnPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (e.StagingItem.Input.RoutedEvent == Mouse.MouseUpEvent)
            {
                e.StagingItem.Input.Handled = true;
            }
        }

        CommandBarFlyoutToolBar m_toolBar;
        WeakReference<CommandBarFlyout> m_owningFlyout;
    }
}
