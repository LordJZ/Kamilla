using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
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
                wnd.Implementation.StyleChanged += (o, e) => wnd.ThreadSafe(_ => s_console.Style = _.Style);
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
    }
}
