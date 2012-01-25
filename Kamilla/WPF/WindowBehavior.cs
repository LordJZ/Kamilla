using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Kamilla.WPF
{
    public static class WindowBehavior
    {
        static readonly Type s_ownerType = typeof(WindowBehavior);
        static readonly RoutedEventHandler s_loadedHandler = new RoutedEventHandler(InternalInitializeStyleFlags);

        #region Close Button
        public static readonly DependencyProperty HasTitleBarCloseButtonProperty =
            DependencyProperty.RegisterAttached("HasTitleBarCloseButton", typeof(bool), s_ownerType,
                new FrameworkPropertyMetadata(true,
                    new PropertyChangedCallback((o, e) =>
                        HasTitleBarButtonChangedCallback(o, e, Win32.WindowStyles.WS_SYSMENU))));

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetHasTitleBarCloseButton(Window obj)
        {
            return (bool)obj.GetValue(HasTitleBarCloseButtonProperty);
        }
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetHasTitleBarCloseButton(Window obj, bool value)
        {
            obj.SetValue(HasTitleBarCloseButtonProperty, value);
        }
        #endregion

        #region Minimize Button
        public static readonly DependencyProperty HasTitleBarMinimizeButtonProperty =
            DependencyProperty.RegisterAttached("HasTitleBarMinimizeButton", typeof(bool), s_ownerType,
                new FrameworkPropertyMetadata(true,
                    new PropertyChangedCallback((o, e) =>
                        HasTitleBarButtonChangedCallback(o, e, Win32.WindowStyles.WS_MINIMIZEBOX))));

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetHasTitleBarMinimizeButton(Window obj)
        {
            return (bool)obj.GetValue(HasTitleBarMinimizeButtonProperty);
        }
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetHasTitleBarMinimizeButton(Window obj, bool value)
        {
            obj.SetValue(HasTitleBarMinimizeButtonProperty, value);
        }
        #endregion

        #region Maximize Button
        public static readonly DependencyProperty HasTitleBarMaximizeButtonProperty =
            DependencyProperty.RegisterAttached("HasTitleBarMaximizeButton", typeof(bool), s_ownerType,
                new FrameworkPropertyMetadata(true,
                    new PropertyChangedCallback((o, e) =>
                        HasTitleBarButtonChangedCallback(o, e, Win32.WindowStyles.WS_MAXIMIZEBOX))));

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetHasTitleBarMaximizeButton(Window obj)
        {
            return (bool)obj.GetValue(HasTitleBarMaximizeButtonProperty);
        }
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetHasTitleBarMaximizeButton(Window obj, bool value)
        {
            obj.SetValue(HasTitleBarMaximizeButtonProperty, value);
        }
        #endregion


        static void HasTitleBarButtonChangedCallback(DependencyObject obj,
            DependencyPropertyChangedEventArgs e, Win32.WindowStyles button)
        {
            var window = obj as Window;
            if (window == null)
                return;

            if (!window.IsLoaded)
                window.Loaded += s_loadedHandler;
            else
                InternalSetStyleFlag(window, button, (bool)e.NewValue);
        }

        static void InternalSetStyleFlag(Window window, Win32.WindowStyles button, bool value)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            uint style = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE);

            if (value)
                style |= (uint)button;
            else
                style &= ~(uint)button;

            Win32.SetWindowLong(hwnd, Win32.GWL_STYLE, style);
        }

        static void InternalInitializeStyleFlags(object sender, RoutedEventArgs e)
        {
            var window = sender as Window;
            if (window == null)
                return;

            var hwnd = new WindowInteropHelper(window).Handle;
            uint style = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE);

            style &= ~(uint)Win32.WindowStyles.WS_TITLEBARBUTTONS;

            if (GetHasTitleBarCloseButton(window))
                style |= (uint)Win32.WindowStyles.WS_SYSMENU;

            if (GetHasTitleBarMinimizeButton(window))
                style |= (uint)Win32.WindowStyles.WS_MINIMIZEBOX;

            if (GetHasTitleBarMaximizeButton(window))
                style |= (uint)Win32.WindowStyles.WS_MAXIMIZEBOX;

            Win32.SetWindowLong(hwnd, Win32.GWL_STYLE, style);
            window.Loaded -= s_loadedHandler;
        }
    }
}
