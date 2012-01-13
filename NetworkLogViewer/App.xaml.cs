using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static ConsoleWindow s_console;

        internal static void InitializeConsole()
        {
            if (s_console == null)
                s_console = new ConsoleWindow();
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
                    InitializeConsole();

                return s_console;
            }
        }
    }
}
