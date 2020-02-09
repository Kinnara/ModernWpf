using System.ComponentModel;
using System.Windows;

namespace MUXControlsTestApp
{
    public static class DesignModeHelpers
    {
        private static bool s_inDesignModeCached;
        private static bool s_inDesignMode;
        private static bool GetIsInDesignMode()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return true;

            return false;
        }

        public static bool IsInDesignMode
        {
            get
            {
                if (!s_inDesignModeCached)
                {
                    s_inDesignMode = GetIsInDesignMode();
                    s_inDesignModeCached = true;
                }

                return s_inDesignMode;
            }
        }
    }
}
