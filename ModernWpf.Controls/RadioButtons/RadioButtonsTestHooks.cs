namespace ModernWpf.Controls
{
    internal class RadioButtonsTestHooks
    {
        public static RadioButtonsTestHooks EnsureGlobalTestHooks()
        {
            if (s_testHooks == null)
            {
                s_testHooks = new RadioButtonsTestHooks();
            }
            return s_testHooks;
        }

        public static void SetTestHooksEnabled(RadioButtons radioButtons, bool enabled)
        {
            if (radioButtons != null)
            {
                radioButtons.SetTestHooksEnabled(enabled);
            }
        }

        public static void NotifyLayoutChanged(RadioButtons sender)
        {
            var hooks = EnsureGlobalTestHooks();
            LayoutChanged?.Invoke(sender, null);
        }

        public static int GetRows(RadioButtons radioButtons)
        {
            if (radioButtons != null)
            {
                return radioButtons.GetRows();
            }
            return -1;
        }

        public static int GetColumns(RadioButtons radioButtons)
        {
            if (radioButtons != null)
            {
                return radioButtons.GetColumns();
            }
            return -1;
        }

        public static int GetLargerColumns(RadioButtons radioButtons)
        {
            if (radioButtons != null)
            {
                return radioButtons.GetLargerColumns();
            }
            return -1;
        }

        public static void SetKeyboardModifiersOverride(System.Windows.Input.ModifierKeys? modifiers)
        {
            s_keyboardModifiersOverride = modifiers;
        }

        internal static System.Windows.Input.ModifierKeys GetKeyboardModifiers(System.Windows.Input.KeyboardDevice keyboardDevice)
        {
            return s_keyboardModifiersOverride ?? keyboardDevice.Modifiers;
        }

        public static TypedEventHandler<RadioButtons, object> LayoutChanged;

        static RadioButtonsTestHooks s_testHooks;

        static System.Windows.Input.ModifierKeys? s_keyboardModifiersOverride;
    }
}
