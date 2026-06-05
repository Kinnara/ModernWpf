using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ModernWpf.Gallery.Pages.WpfGallery.SystemPages
{
    /// <summary>
    /// Interaction logic for MessageBoxPage.xaml
    /// </summary>
    public partial class MessageBoxPage : Page
    {
        public MessageBoxPageViewModel ViewModel { get; }

        public MessageBoxPage(MessageBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        private void ShowDefaultMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ShowOwnedMessageBox("This is a simple message box!");
            ViewModel.DefaultMessageResult = $"Result: {result}";
        }

        private void ShowCustomTitleButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ShowOwnedMessageBox("This is a detailed description of what happened or what action is needed.", "Custom Title");
            ViewModel.CustomTitleResult = $"Result: {result}";
        }

        private void ShowButtonFromComboBox_Click(object sender, RoutedEventArgs e)
        {
            var buttonType = GetMessageBoxButton(ViewModel.SelectedButtonIndex);
            var buttonName = GetMessageBoxButtonName(ViewModel.SelectedButtonIndex);
            var result = ShowOwnedMessageBox($"This MessageBox has {buttonName} button(s).", $"{buttonName} Button(s)", buttonType);
            ViewModel.DifferentButtonsResult = $"Result: {result}";
        }

        private void ShowImageFromComboBox_Click(object sender, RoutedEventArgs e)
        {
            var imageType = GetMessageBoxImage(ViewModel.SelectedImageIndex);
            var imageName = GetMessageBoxImageName(ViewModel.SelectedImageIndex);
            var result = ShowOwnedMessageBox($"This MessageBox displays the {imageName} icon.", $"{imageName} Icon", MessageBoxButton.OK, imageType);
            ViewModel.DifferentImagesResult = $"Result: {result}";
        }

        // 6. Common Messages (Information, Error, Warning)
        private void ShowCommonInformation_Click(object sender, RoutedEventArgs e)
        {
            var result = ShowOwnedMessageBox("The operation completed successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            ViewModel.CommonMessagesResult = $"Type: Information | Result: {result}";
        }

        private void ShowCommonError_Click(object sender, RoutedEventArgs e)
        {
            var result = ShowOwnedMessageBox("An error occurred! The operation could not be completed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ViewModel.CommonMessagesResult = $"Type: Error | Result: {result}";
        }

        private void ShowCommonWarning_Click(object sender, RoutedEventArgs e)
        {
            var result = ShowOwnedMessageBox("This action cannot be undone! Do you want to continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            ViewModel.CommonMessagesResult = $"Type: Warning | Result: {result}";
        }

        // 7. Custom Default Button
        private void ShowCustomDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ShowOwnedMessageBox("Do you want to save changes? Press Enter to select the default 'No' button.", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
            ViewModel.CustomDefaultResult = $"User selected: {result}";
        }

        private MessageBoxResult ShowOwnedMessageBox(string messageBoxText)
        {
            var owner = GetOwnerWindow();
            using (new MessageBoxCenteringScope(owner))
            {
                return MessageBox.Show(owner, messageBoxText);
            }
        }

        private MessageBoxResult ShowOwnedMessageBox(string messageBoxText, string caption)
        {
            var owner = GetOwnerWindow();
            using (new MessageBoxCenteringScope(owner))
            {
                return MessageBox.Show(owner, messageBoxText, caption);
            }
        }

        private MessageBoxResult ShowOwnedMessageBox(string messageBoxText, string caption, MessageBoxButton button)
        {
            var owner = GetOwnerWindow();
            using (new MessageBoxCenteringScope(owner))
            {
                return MessageBox.Show(owner, messageBoxText, caption, button);
            }
        }

        private MessageBoxResult ShowOwnedMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            var owner = GetOwnerWindow();
            using (new MessageBoxCenteringScope(owner))
            {
                return MessageBox.Show(owner, messageBoxText, caption, button, icon);
            }
        }

        private MessageBoxResult ShowOwnedMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            var owner = GetOwnerWindow();
            using (new MessageBoxCenteringScope(owner))
            {
                return MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult);
            }
        }

        private Window GetOwnerWindow()
        {
            var owner = Window.GetWindow(this) ?? Application.Current.MainWindow;
            owner?.Activate();
            return owner;
        }

        private sealed class MessageBoxCenteringScope : IDisposable
        {
            private const int WH_CBT = 5;
            private const int HCBT_ACTIVATE = 5;
            private const int MONITOR_DEFAULTTONEAREST = 2;
            private const uint SWP_NOSIZE = 0x0001;
            private const uint SWP_NOZORDER = 0x0004;
            private const uint SWP_NOACTIVATE = 0x0010;

            private readonly IntPtr _ownerHandle;
            private readonly HookProc _hookProc;
            private IntPtr _hook;

            public MessageBoxCenteringScope(Window owner)
            {
                if (owner == null)
                {
                    return;
                }

                owner.Activate();
                _ownerHandle = new WindowInteropHelper(owner).Handle;
                if (_ownerHandle == IntPtr.Zero)
                {
                    return;
                }

                _hookProc = HookCallback;
                _hook = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());
            }

            public void Dispose()
            {
                if (_hook != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hook);
                    _hook = IntPtr.Zero;
                }
            }

            private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
            {
                var hook = _hook;
                if (code == HCBT_ACTIVATE)
                {
                    CenterDialog(wParam);
                    Dispose();
                }

                return CallNextHookEx(hook, code, wParam, lParam);
            }

            private void CenterDialog(IntPtr dialogHandle)
            {
                if (dialogHandle == IntPtr.Zero ||
                    !GetWindowRect(_ownerHandle, out var ownerRect) ||
                    !GetWindowRect(dialogHandle, out var dialogRect))
                {
                    return;
                }

                var dialogWidth = dialogRect.Right - dialogRect.Left;
                var dialogHeight = dialogRect.Bottom - dialogRect.Top;
                if (dialogWidth <= 0 || dialogHeight <= 0)
                {
                    return;
                }

                var left = ownerRect.Left + ((ownerRect.Right - ownerRect.Left - dialogWidth) / 2);
                var top = ownerRect.Top + ((ownerRect.Bottom - ownerRect.Top - dialogHeight) / 2);
                var workArea = GetOwnerMonitorWorkArea();
                left = Math.Max(workArea.Left, Math.Min(left, workArea.Right - dialogWidth));
                top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - dialogHeight));

                SetWindowPos(dialogHandle, IntPtr.Zero, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }

            private NativeRect GetOwnerMonitorWorkArea()
            {
                var monitorHandle = MonitorFromWindow(_ownerHandle, MONITOR_DEFAULTTONEAREST);
                var monitorInfo = new MonitorInfo
                {
                    Size = Marshal.SizeOf<MonitorInfo>()
                };

                if (monitorHandle != IntPtr.Zero && GetMonitorInfo(monitorHandle, ref monitorInfo))
                {
                    return monitorInfo.WorkArea;
                }

                GetWindowRect(_ownerHandle, out var ownerRect);
                return ownerRect;
            }

            private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

            [StructLayout(LayoutKind.Sequential)]
            private struct NativeRect
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MonitorInfo
            {
                public int Size;
                public NativeRect Monitor;
                public NativeRect WorkArea;
                public int Flags;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hmod, int dwThreadId);

            [DllImport("user32.dll")]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll")]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll")]
            private static extern int GetCurrentThreadId();

            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

            [DllImport("user32.dll")]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

            [DllImport("user32.dll")]
            private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

            [DllImport("user32.dll")]
            private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);
        }

        private static MessageBoxButton GetMessageBoxButton(int index)
        {
            switch (index)
            {
                case 1:
                    return MessageBoxButton.OKCancel;
#if NET10_0_OR_GREATER
                case 2:
                    return MessageBoxButton.AbortRetryIgnore;
#endif
                case 3:
                    return MessageBoxButton.YesNoCancel;
                case 4:
                    return MessageBoxButton.YesNo;
#if NET10_0_OR_GREATER
                case 5:
                    return MessageBoxButton.RetryCancel;
                case 6:
                    return MessageBoxButton.CancelTryContinue;
#endif
                default:
                    return MessageBoxButton.OK;
            }
        }

        private static string GetMessageBoxButtonName(int index)
        {
            switch (index)
            {
                case 1:
                    return "OK/Cancel";
                case 2:
                    return "Abort/Retry/Ignore";
                case 3:
                    return "Yes/No/Cancel";
                case 4:
                    return "Yes/No";
                case 5:
                    return "Retry/Cancel";
                case 6:
                    return "Cancel/Try/Continue";
                default:
                    return "OK";
            }
        }

        private static MessageBoxImage GetMessageBoxImage(int index)
        {
            switch (index)
            {
                case 1:
                    return MessageBoxImage.Error;
                case 2:
                    return MessageBoxImage.Question;
                case 3:
                    return MessageBoxImage.Warning;
                case 4:
                    return MessageBoxImage.Information;
                default:
                    return MessageBoxImage.None;
            }
        }

        private static string GetMessageBoxImageName(int index)
        {
            switch (index)
            {
                case 1:
                    return "Error";
                case 2:
                    return "Question";
                case 3:
                    return "Warning";
                case 4:
                    return "Information";
                default:
                    return "None";
            }
        }
    }
}
