using System;
using System.ComponentModel;
using System.Windows;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    partial class ConsoleWindow : Window
    {
        WpfRtbConsole m_console;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.PacketViewer.WPF.ConsoleWindow"/> class.
        /// </summary>
        public ConsoleWindow(MainWindow window)
        {
            //this.Owner = window;
            InitializeComponent();

            this.Style = window.Style;
            window.Implementation.StyleChanged += (o, e) => this.Style = window.Style;

            m_console = new WpfRtbConsole(tbMain);
            Console.SetOut(m_console);
            Console.SetError(m_console);

            Console.WriteLine("Console Initialized.");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Closing Console...");
            m_console.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Opacity = 0.5;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1.0;
        }
    }
}
