using System;
using System.Windows;
using System.Windows.Threading;

namespace MUXControlsTestApp.Utilities
{
    public static class Extensions
    {
        private static readonly Action NoOpCallback = delegate { };

        public static void UpdateLayoutAndWaitForIdleDispatcher(this UIElement element)
        {
            element.UpdateLayout();
            element.Dispatcher.Invoke(NoOpCallback, DispatcherPriority.ApplicationIdle);
        }
    }
}
