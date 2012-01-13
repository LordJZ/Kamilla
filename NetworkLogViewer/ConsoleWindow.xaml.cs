using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    partial class ConsoleWindow : Window
    {
        internal bool m_closing;

        WpfRtbConsole m_console;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.PacketViewer.WPF.ConsoleWindow"/> class.
        /// </summary>
        public ConsoleWindow()
        {
            InitializeComponent();

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !m_closing;
            if (!m_closing)
                this.Hide();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Opacity = 0.3;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1.0;
        }
    }
}
