using System;
using System.Windows;
using System.Windows.Input;
using Kamilla.WPF;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static ConsoleWindow s_console;

        internal static void InitializeConsole(MainWindow wnd)
        {
            if (s_console == null)
            {
                s_console = new ConsoleWindow();
                s_console.Style = wnd.Style;
                wnd.Implementation.StyleChanged += (o, e) => s_console.Style = wnd.Style;
            }
            else
                throw new InvalidOperationException("Console is already initialized.");
        }

        /// <summary>
        /// Gets the <see cref="Kamilla.PacketViewer.WPF.ConsoleWindow"/> instance that serves as the console.
        /// </summary>
        internal static ConsoleWindow ConsoleWindow
        {
            get
            {
                if (s_console == null)
                    throw new InvalidOperationException();

                return s_console;
            }
        }

        void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
            {
                if (e.ClickCount == 2 && WindowBehavior.GetHasTitleBarMaximizeButton(window))
                {
                    if (window.WindowState == WindowState.Normal)
                        window.WindowState = WindowState.Maximized;
                    else
                        window.WindowState = WindowState.Normal;
                }
                else
                    window.DragMove();
            }
        }

        void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
                window.WindowState = WindowState.Minimized;
        }

        void Maximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
                window.WindowState = WindowState.Maximized;
        }

        void Normalize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
                window.WindowState = WindowState.Normal;
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
                window.Close();
        }
    }
}
