using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class ContentDialogAutomationPeer : FrameworkElementAutomationPeer, IWindowProvider
    {
        public ContentDialogAutomationPeer(ContentDialog owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Window ? this : base.GetPattern(patternInterface);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Window;
        }

        protected override string GetClassNameCore()
        {
            return nameof(ContentDialog);
        }

        protected override string GetNameCore()
        {
            var dialog = GetImpl();
            var explicitName = AutomationProperties.GetName(dialog);
            if (!string.IsNullOrWhiteSpace(explicitName))
            {
                return explicitName;
            }

            var title = GetPlainText(dialog.Title);
            return !string.IsNullOrWhiteSpace(title) ? title : GetPlainText(dialog.Content);
        }

        public bool Maximizable => false;

        public bool Minimizable => false;

        public bool IsModal => true;

        public bool IsTopmost => GetImpl().IsShowingForAutomation;

        public WindowInteractionState InteractionState => GetImpl().IsShowingForAutomation
            ? WindowInteractionState.ReadyForUserInteraction
            : WindowInteractionState.Closing;

        public WindowVisualState VisualState => WindowVisualState.Normal;

        public void Close()
        {
            GetImpl().Hide();
        }

        public void SetVisualState(WindowVisualState state)
        {
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            return true;
        }

        private static string GetPlainText(object value)
        {
            return value as string ?? value?.ToString() ?? string.Empty;
        }

        private ContentDialog GetImpl()
        {
            return (ContentDialog)Owner;
        }
    }
}
