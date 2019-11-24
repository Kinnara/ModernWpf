using System;

namespace MUXControlsTestApp.Utilities
{
    public class RunOnUIThread
    {
        public static void Execute(Action action)
        {
            action();
        }
    }
}
