using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class SplitViewPaneRoot : Border
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SplitViewPaneAutomationPeer(this);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class SplitViewLightDismissLayer : FrameworkElement
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(SplitViewLightDismissLayer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SplitViewLightDismissAutomationPeer(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(Fill, null, new Rect(RenderSize));
        }
    }

    internal sealed class SplitViewPaneAutomationPeer : FrameworkElementAutomationPeer, IWindowProvider
    {
        public SplitViewPaneAutomationPeer(SplitViewPaneRoot owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Window && IsWindowContextEnabled
                ? this
                : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return "SplitViewPane";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Window;
        }

        protected override string GetAutomationIdCore()
        {
            var automationId = base.GetAutomationIdCore();
            return string.IsNullOrEmpty(automationId) ? ((SplitViewPaneRoot)Owner).Name : automationId;
        }

        protected override bool IsControlElementCore()
        {
            return true;
        }

        protected override bool IsContentElementCore()
        {
            return false;
        }

        public bool Maximizable => false;

        public bool Minimizable => false;

        public bool IsModal => true;

        public bool IsTopmost => true;

        public WindowInteractionState InteractionState => WindowInteractionState.Running;

        public WindowVisualState VisualState => WindowVisualState.Normal;

        public void Close()
        {
        }

        public void SetVisualState(WindowVisualState state)
        {
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            return true;
        }

        private bool IsWindowContextEnabled => GetSplitView()?.IsLightDismissEnabledForAutomation == true;

        private SplitView GetSplitView()
        {
            return ((SplitViewPaneRoot)Owner).TemplatedParent as SplitView;
        }
    }

    internal sealed class SplitViewLightDismissAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
    {
        public SplitViewLightDismissAutomationPeer(SplitViewLightDismissLayer owner)
            : base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return patternInterface == PatternInterface.Invoke && IsLightDismissEnabled
                ? this
                : base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return "SplitViewLightDismiss";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        protected override string GetNameCore()
        {
            return "Close";
        }

        protected override string GetAutomationIdCore()
        {
            return "LightDismiss";
        }

        protected override bool IsControlElementCore()
        {
            return true;
        }

        protected override bool IsContentElementCore()
        {
            return true;
        }

        void IInvokeProvider.Invoke()
        {
            if (IsLightDismissEnabled)
            {
                GetSplitView().InvokeLightDismissForAutomation();
            }
        }

        private bool IsLightDismissEnabled => GetSplitView()?.IsLightDismissEnabledForAutomation == true;

        private SplitView GetSplitView()
        {
            return ((SplitViewLightDismissLayer)Owner).TemplatedParent as SplitView;
        }
    }
}
