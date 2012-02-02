using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Kamilla;
using Kamilla.WPF;

namespace NetworkLogViewer
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    partial class ConsoleWindow : Window
    {
        readonly MainWindow m_window;
        readonly int m_maxConsoleEntries;

        /// <summary>
        /// Initializes a new instance of <see cref="Kamilla.PacketViewer.WPF.ConsoleWindow"/> class.
        /// </summary>
        public ConsoleWindow(MainWindow window)
        {
            InitializeComponent();

            m_maxConsoleEntries = Configuration.GetValue("Max Console Entries", 512);

            m_window = window;
            this.Style = window.Style;
            window.StyleChanged += (o, e) => this.Style = m_window.Style;
            window.Activated += new EventHandler(MainWindow_Activated);

            ConsoleWriter.ConsoleWrite += new ConsoleWriteEventHandler(ConsoleWriter_ConsoleWrite);

            Console.WriteLine("Console Window Initialized.");
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            this.Owner = m_window;

            m_window.Activated -= new EventHandler(MainWindow_Activated);
        }

        void ConsoleWriter_ConsoleWrite(object sender, ConsoleWriteEventArgs args)
        {
            this.ThreadSafeBegin(_ =>
            {
                var text = args.Message;
                Brush brush = null;
                if (text.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
                    brush = Brushes.Red;
                else if (text.StartsWith("warning", StringComparison.InvariantCultureIgnoreCase))
                    brush = Brushes.Orange;
                else if (text.StartsWith("debug", StringComparison.InvariantCultureIgnoreCase))
                    brush = Brushes.Gray;

                ui_tbMain.BeginChange();
                var inlines = ui_para.Inlines;

                var tokens = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tok in tokens)
                {
                    var run = new Run(tok.Trim());
                    if (brush != null)
                        run.Foreground = brush;

                    inlines.Add(run);
                    inlines.Add(new LineBreak());
                }

                while (inlines.Count > m_maxConsoleEntries)
                    inlines.Remove(inlines.FirstInline);

                ui_tbMain.EndChange();
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Closing Console...");
            ConsoleWriter.Close();
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
