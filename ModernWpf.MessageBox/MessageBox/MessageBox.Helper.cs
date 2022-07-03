using ModernWpf.Controls;
using ModernWpf.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModernWpf.Controls
{
    public partial class MessageBox
    {
        public static bool EnableLocalization { get; set; } = true;

        #region Sync

        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText) =>
            Show(messageBoxText, null);

        /// <summary>
        /// Displays a message box that has a message and title bar caption; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption) =>
            Show(GetActiveWindow(), messageBoxText, caption);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText) =>
            Show(owner, messageBoxText, null);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and button; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button) =>
            Show(GetActiveWindow(), messageBoxText, caption, button);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message and title bar caption; and it returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption) =>
            Show(owner, messageBoxText, caption, MessageBoxButton.OK);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)=>
            Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon) =>
            Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, string icon) =>
            Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, IconSource icon) =>
            Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, and button; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
            Show(owner, messageBoxText, caption, button, (IconSource)null);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)=>
            Show(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon, MessageBoxResult defaultResult) =>
            Show(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, string icon, MessageBoxResult defaultResult) =>
            Show(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>Use an overload of the Show method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, IconSource icon, MessageBoxResult defaultResult) =>
            Show(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)=>
            Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon) =>
            Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, string icon) =>
            Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, IconSource icon) =>
            Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) =>
            Show(owner, messageBoxText, caption, button, icon.ToSymbol(), defaultResult);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon, MessageBoxResult defaultResult) =>
            Show(owner, messageBoxText, caption, button, icon.ToGlyph(), defaultResult);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, string icon, MessageBoxResult defaultResult) =>
            Show(owner, messageBoxText, caption, button, new FontIconSource { Glyph = icon, FontSize = 30 }, defaultResult);

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, IconSource icon, MessageBoxResult defaultResult)
        {
            if (owner is null)
            {
                owner = GetActiveWindow();
            }

            Controls.MessageBox window = new Controls.MessageBox
            {
                Owner = owner,
                IconSource = icon,
                Result = defaultResult,
                Content = messageBoxText,
                MessageBoxButtons = button,
                Caption = caption ?? string.Empty,
                WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };

            return window.ShowDialog();
        }

        #endregion Sync

        #region Async

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText) =>
            ShowAsync(messageBoxText, null);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message and title bar caption; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption) =>
            ShowAsync(GetActiveWindow(), messageBoxText, caption);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText) =>
            ShowAsync(owner, messageBoxText, null);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, and button; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button) =>
            ShowAsync(GetActiveWindow(), messageBoxText, caption, button);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message and title bar caption; and it returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption) =>
            ShowAsync(owner, messageBoxText, caption, MessageBoxButton.OK);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon) =>
            ShowAsync(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon) =>
            ShowAsync(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, string icon) =>
            ShowAsync(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, IconSource icon) =>
            ShowAsync(messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, and button; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button) =>
            ShowAsync(owner, messageBoxText, caption, button, (IconSource)null);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) =>
            ShowAsync(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon, MessageBoxResult defaultResult) =>
            ShowAsync(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, string icon, MessageBoxResult defaultResult) =>
            ShowAsync(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box that has a message, title bar caption, button, and icon; and that accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>Use an overload of the ShowAsync method, which enables you to specify an owner window. Otherwise, the message box is owned by the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, IconSource icon, MessageBoxResult defaultResult) =>
            ShowAsync(GetActiveWindow(), messageBoxText, caption, button, icon, defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon) =>
            ShowAsync(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon) =>
            ShowAsync(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, string icon) =>
            ShowAsync(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and it also returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, IconSource icon) =>
            ShowAsync(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) =>
            ShowAsync(owner, messageBoxText, caption, button, icon.ToSymbol(), defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="SymbolGlyph"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, SymbolGlyph icon, MessageBoxResult defaultResult) =>
            ShowAsync(owner, messageBoxText, caption, button, icon.ToGlyph(), defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="string"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, string icon, MessageBoxResult defaultResult) =>
            ShowAsync(owner, messageBoxText, caption, button, new FontIconSource { Glyph = icon, FontSize = 30 }, defaultResult);

        /// <summary>
        /// Begins an asynchronous operation to displays a message box in front of the specified window. The message box displays a message, title bar caption, button, and icon; and accepts a default message box result and returns a result.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="IconSource"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <returns>An asynchronous operation showing the message box. When complete, returns a <see cref="MessageBoxResult"/>.</returns>
        /// <remarks>By default, the message box appears in front of the window that is currently active.</remarks>
        public static Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, IconSource icon, MessageBoxResult defaultResult)
        {
            TaskCompletionSource<MessageBoxResult> taskSource = new TaskCompletionSource<MessageBoxResult>(
#if !NET452
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
            );

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBoxResult result = Show(owner, messageBoxText, caption, button, icon, defaultResult);
                taskSource.TrySetResult(result);
            });

            return taskSource.Task;
        }

        #endregion Async

        private static Window GetActiveWindow() =>
            Application.Current.Windows.Cast<Window>()
                .FirstOrDefault(window => window.IsActive && window.ShowActivated);
    }
}
