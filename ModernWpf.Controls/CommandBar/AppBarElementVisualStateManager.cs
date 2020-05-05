using System.Windows;

namespace ModernWpf.Controls
{
    internal class AppBarElementVisualStateManager : VisualStateManager
    {
        internal bool CanChangeCommonState { get; set; }

        protected override bool GoToStateCore(
            FrameworkElement control,
            FrameworkElement stateGroupsRoot,
            string stateName,
            VisualStateGroup group,
            VisualState state,
            bool useTransitions)
        {
            if (state != null && (group.Name != "CommonStates" || CanChangeCommonState))
            {
                return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
            }

            return false;
        }
    }
}
