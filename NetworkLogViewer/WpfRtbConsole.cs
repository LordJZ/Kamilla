using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Kamilla;
using Kamilla.WPF;

namespace NetworkLogViewer
{
    class WpfRtbConsole : TextWriter
    {
        private FileStream m_stream;
        private StreamWriter m_writer;

        readonly Paragraph m_para;
        readonly RichTextBox m_output;
        int m_maxConsoleEntries;

        public WpfRtbConsole(RichTextBox output)
        {
            m_maxConsoleEntries = Configuration.GetValue("Max Console Entries", 512);

            try
            {
                m_stream = new FileStream(Assembly.GetEntryAssembly().GetName().Name + ".log", FileMode.Create);
                m_writer = new StreamWriter(m_stream, this.Encoding);
                m_writer.AutoFlush = true;
            }
            catch
            {
                m_stream = null;
                m_writer = null;
            }

            m_output = output;
            Paragraph para = null;
            output.ThreadSafe(rtb =>
            {
                rtb.Document.Blocks.Add(para = new Paragraph());
            });
            m_para = para;
        }

        // Must Implement Methods

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void WriteLine(string value)
        {
            _Write(value + Environment.NewLine);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            _Write(String.Format(format, arg) + Environment.NewLine);
        }

        public override void WriteLine()
        {
            _Write(Environment.NewLine);
        }

        public override void Write(string value)
        {
            _Write(value);
        }

        public override void Write(string format, params object[] arg)
        {
            _Write(String.Format(format, arg));
        }

        // Generic Write Method

        bool LogDebugOutput
#if DEBUG
            = true;
#else
            = Configuration.GetValue("Logging - Log Debug Output", false);
#endif

        private void _Write(string text)
        {
            bool debug = false;
            if (text.StartsWith("debug", StringComparison.InvariantCultureIgnoreCase))
            {
                if (LogDebugOutput)
                    debug = true;
                else
                    return;
            }

            if (m_output != null /*&& !OutputBox.IsDisposed*/)
            {
                m_output.ThreadSafe(rtb =>
                {
                    Brush brush = null;
                    if (text.StartsWith("error", StringComparison.InvariantCultureIgnoreCase))
                        brush = Brushes.Red;
                    else if (text.StartsWith("warning", StringComparison.InvariantCultureIgnoreCase))
                        brush = Brushes.Orange;
                    else if (debug)
                        brush = Brushes.Gray;

                    m_output.BeginChange();

                    var tokens = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tok in tokens)
                    {
                        var run = new Run(tok.Trim());

                        m_para.Inlines.Add(run);
                        m_para.Inlines.Add(new LineBreak());
                    }

                    while (m_para.Inlines.Count > m_maxConsoleEntries)
                        m_para.Inlines.Remove(m_para.Inlines.FirstInline);

                    m_output.EndChange();
                    m_output.ScrollToEnd();
                });
            }

            if (m_writer != null)
                m_writer.WriteLine(string.Concat("[", DateTime.Now.ToString("HH:mm:ss.fff"), "] ", text).Trim());
        }
    }
}
