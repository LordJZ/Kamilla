using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Kamilla.WPF
{
    public class WindowBehavior
    {
        public enum TitleBarButtons : uint
        {
            None        = 0,
            Maximize    = 0x10000,
            Minimize    = 0x20000,
            Close       = 0x80000,
            All         = Close | Minimize | Maximize
        }

        private static readonly Type OwnerType = typeof(WindowBehavior);

        #region TitleBarButtons (attached property)
        public static readonly DependencyProperty TitleBarButtonsProperty =
            DependencyProperty.RegisterAttached("TitleBarButtons", typeof(TitleBarButtons), OwnerType,
            new FrameworkPropertyMetadata(TitleBarButtons.All, new PropertyChangedCallback(TitleBarButtonsChangedCallback)));

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static TitleBarButtons GetTitleBarButtons(Window obj)
        {
            return (TitleBarButtons)obj.GetValue(TitleBarButtonsProperty);
        }
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetTitleBarButtons(Window obj, TitleBarButtons value)
        {
            obj.SetValue(TitleBarButtonsProperty, value);
        }
        private static void TitleBarButtonsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;

            var buttons = (TitleBarButtons)e.NewValue;

            if (!window.IsLoaded)
                window.Loaded += LoadedDelegate;
            else
                SetTitleBarButtonsInternal(window, buttons);
        }
        private static readonly RoutedEventHandler LoadedDelegate = (sender, args) =>
        {
            if (!(sender is Window))
                return;

            var w = (Window)sender;
            SetTitleBarButtonsInternal(w, GetTitleBarButtons(w));
            w.Loaded -= LoadedDelegate;
        };

        private static void SetTitleBarButtonsInternal(Window w, TitleBarButtons buttons)
        {
            var hwnd = new WindowInteropHelper(w).Handle;
            uint value = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE);
            Win32.SetWindowLong(hwnd, Win32.GWL_STYLE, (value & ~(uint)TitleBarButtons.All) | (uint)buttons);
        }
        #endregion
    }
}
