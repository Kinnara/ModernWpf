#if DEBUG
using System.Diagnostics;
using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public class DebugVisualStateManager : VisualStateManager
    {
        protected override bool GoToStateCore(
            FrameworkElement control,
            FrameworkElement stateGroupsRoot,
            string stateName,
            VisualStateGroup group,
            VisualState state,
            bool useTransitions)
        {
            if (state == null)
            {
                return false;
            }

            Debug.WriteLine($"stateName = {stateName}, useTransitions = {useTransitions}");

            return base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
        }
    }
}
#endif
