// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public sealed class AutoSuggestBoxHelper
    {
        const string c_popupName = "SuggestionsPopup";
        const string c_popupBorderName = "SuggestionsContainer";
        const string c_textBoxName = "TextBox";
        const string c_textBoxBorderName = "BorderElement";
        const string c_controlCornerRadiusKey = "ControlCornerRadius";
        const string c_overlayCornerRadiusKey = "OverlayCornerRadius";

        internal AutoSuggestBoxHelper()
        {
        }

        public static readonly DependencyProperty KeepInteriorCornersSquareProperty =
            DependencyProperty.RegisterAttached(
                "KeepInteriorCornersSquare",
                typeof(bool),
                typeof(AutoSuggestBoxHelper),
                new PropertyMetadata(false, OnKeepInteriorCornersSquareChanged));

        public static bool GetKeepInteriorCornersSquare(AutoSuggestBox autoSuggestBox)
        {
            return (bool)autoSuggestBox.GetValue(KeepInteriorCornersSquareProperty);
        }

        public static void SetKeepInteriorCornersSquare(AutoSuggestBox autoSuggestBox, bool value)
        {
            autoSuggestBox.SetValue(KeepInteriorCornersSquareProperty, value);
        }

        private static readonly DependencyProperty AutoSuggestEventRevokersProperty =
            DependencyProperty.RegisterAttached(
                "AutoSuggestEventRevokers",
                typeof(object),
                typeof(AutoSuggestBoxHelper));

        private static void OnKeepInteriorCornersSquareChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is AutoSuggestBox autoSuggestBox)
            {
                bool shouldMonitorAutoSuggestEvents = (bool)args.NewValue;
                if (shouldMonitorAutoSuggestEvents)
                {
                    var revokers = new AutoSuggestEventRevokers();

                    revokers.m_autoSuggestBoxLoadedRevoker = new RoutedEventHandlerRevoker(autoSuggestBox, FrameworkElement.LoadedEvent, new RoutedEventHandler(OnAutoSuggestBoxLoaded));
                    autoSuggestBox.SetValue(AutoSuggestEventRevokersProperty, revokers);
                }
                else
                {
                    if (autoSuggestBox.GetValue(AutoSuggestEventRevokersProperty) is AutoSuggestEventRevokers revokers)
                    {
                        if (revokers.m_autoSuggestBoxLoadedRevoker != null)
                        {
                            revokers.m_autoSuggestBoxLoadedRevoker.Revoke();
                            revokers.m_autoSuggestBoxLoadedRevoker = null;
                        }

                        if (revokers.m_popupOpenedRevoker != null)
                        {
                            revokers.m_popupOpenedRevoker.Revoke();
                            revokers.m_popupOpenedRevoker = null;
                        }

                        if (revokers.m_popupClosedRevoker != null)
                        {
                            revokers.m_popupClosedRevoker.Revoke();
                            revokers.m_popupClosedRevoker = null;
                        }
                    }

                    autoSuggestBox.SetValue(AutoSuggestEventRevokersProperty, null);
                }
            }
        }

        private static void OnAutoSuggestBoxLoaded(object sender, object args)
        {
            var autoSuggestBox = (AutoSuggestBox)sender;
            var revokers = (AutoSuggestEventRevokers)autoSuggestBox.GetValue(AutoSuggestEventRevokersProperty);

            if (revokers.m_popupOpenedRevoker == null || revokers.m_popupClosedRevoker == null)
            {
                if (GetTemplateChild<Popup>(c_popupName, autoSuggestBox) is Popup popup)
                {
                    var autoSuggestBoxWeakRef = new WeakReference<AutoSuggestBox>(autoSuggestBox);

                    revokers.m_popupOpenedRevoker = new PopupOpenedRevoker(popup,
                        delegate
                        {
                            if (autoSuggestBoxWeakRef.TryGetTarget(out var asb))
                            {
                                UpdateCornerRadius(asb, /*IsDropDownOpen=*/true);
                            }
                        });

                    revokers.m_popupClosedRevoker = new PopupClosedRevoker(popup,
                        delegate
                        {
                            if (autoSuggestBoxWeakRef.TryGetTarget(out var asb))
                            {
                                UpdateCornerRadius(asb, /*IsDropDownOpen=*/false);
                            }
                        });
                }
            }
        }

        private static void UpdateCornerRadius(AutoSuggestBox autoSuggestBox, bool isPopupOpen)
        {
            var textBoxRadius = autoSuggestBox.CornerRadius;
            var popupRadius = (CornerRadius)ResourceLookup(autoSuggestBox, c_overlayCornerRadiusKey);

            if (isPopupOpen)
            {
                bool isOpenDown = IsPopupOpenDown(autoSuggestBox);
                var cornerRadiusConverter = new CornerRadiusFilterConverter();

                var popupRadiusFilter = isOpenDown ? CornerRadiusFilterKind.Bottom : CornerRadiusFilterKind.Top;
                popupRadius = cornerRadiusConverter.Convert(popupRadius, popupRadiusFilter);

                var textBoxRadiusFilter = isOpenDown ? CornerRadiusFilterKind.Top : CornerRadiusFilterKind.Bottom;
                textBoxRadius = cornerRadiusConverter.Convert(textBoxRadius, textBoxRadiusFilter);
            }

            if (GetTemplateChild<Border>(c_popupBorderName, autoSuggestBox) is Border popupBorder)
            {
                popupBorder.CornerRadius = popupRadius;
            }

            if (GetTemplateChild<TextBox>(c_textBoxName, autoSuggestBox) is TextBox textBox)
            {
                ControlHelper.SetCornerRadius(textBox, textBoxRadius);
            }
        }

        private static bool IsPopupOpenDown(AutoSuggestBox autoSuggestBox)
        {
            double verticalOffset = 0;
            if (GetTemplateChild<Border>(c_popupBorderName, autoSuggestBox) is Border popupBorder)
            {
                if (GetTemplateChild<TextBox>(c_textBoxName, autoSuggestBox) is TextBox textBox)
                {
                    var popupTop = popupBorder.TranslatePoint(new Point(0, 0), textBox);
                    verticalOffset = popupTop.Y;
                }
            }
            return verticalOffset >= 0;
        }

        private static object ResourceLookup(Control control, object key)
        {
            return control.Resources.Contains(key) ? control.Resources[key] : Application.Current.TryFindResource(key);
        }

        private static T GetTemplateChild<T>(string childName, Control control) where T : DependencyObject
        {
            return control.Template?.FindName(childName, control) as T;
        }
    }

    internal class AutoSuggestEventRevokers
    {
        public RoutedEventHandlerRevoker m_autoSuggestBoxLoadedRevoker;
        public PopupOpenedRevoker m_popupOpenedRevoker;
        public PopupClosedRevoker m_popupClosedRevoker;
    }

    internal class PopupOpenedRevoker
    {
        public PopupOpenedRevoker(Popup source, EventHandler handler)
        {
            m_source = new WeakReference<Popup>(source);
            m_handler = new WeakReference<EventHandler>(handler);

            source.Opened += handler;
        }

        private readonly WeakReference<Popup> m_source;
        private readonly WeakReference<EventHandler> m_handler;

        public void Revoke()
        {
            if (m_source.TryGetTarget(out var source) &&
                m_handler.TryGetTarget(out var handler))
            {
                source.Opened -= handler;
            }
        }
    }

    internal class PopupClosedRevoker
    {
        public PopupClosedRevoker(Popup source, EventHandler handler)
        {
            m_source = new WeakReference<Popup>(source);
            m_handler = new WeakReference<EventHandler>(handler);

            source.Closed += handler;
        }

        private readonly WeakReference<Popup> m_source;
        private readonly WeakReference<EventHandler> m_handler;

        public void Revoke()
        {
            if (m_source.TryGetTarget(out var source) &&
                m_handler.TryGetTarget(out var handler))
            {
                source.Closed -= handler;
            }
        }
    }
}
