using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Kamilla
{
    public static class ConsoleWriter
    {
        class WriterImpl : TextWriter
        {
            FileStream m_stream;
            StreamWriter m_writer;
            readonly bool m_debug;

            public WriterImpl()
            {
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

#if DEBUG
            m_debug = true;
#else
                m_debug = Configuration.GetValue("Debug", false);
#endif
            }

            #region Overrides

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override void WriteLine(string value)
            {
                InternalWrite(value + Environment.NewLine);
            }

            public override void WriteLine(string format, params object[] arg)
            {
                InternalWrite(string.Format(format, arg) + Environment.NewLine);
            }

            public override void WriteLine()
            {
                InternalWrite(Environment.NewLine);
            }

            public override void Write(string value)
            {
                InternalWrite(value);
            }

            public override void Write(string format, params object[] arg)
            {
                InternalWrite(string.Format(format, arg));
            }

            public override void Close()
            {
                base.Close();

                if (m_writer != null)
                {
                    m_writer.Close();
                    m_writer = null;
                }

                if (m_stream != null)
                {
                    m_stream.Close();
                    m_stream = null;
                }
            } 

            #endregion

            void InternalWrite(string text)
            {
                if (m_debug && text.StartsWith("debug", StringComparison.InvariantCultureIgnoreCase))
                    return;

                OnConsoleWrite(text);

                if (m_writer != null)
                    m_writer.WriteLine(string.Concat("[", DateTime.Now.ToString("HH:mm:ss.fff"), "] ", text));
            }
        }

        static WriterImpl m_impl;

        public static void Initialize()
        {
            if (m_impl != null)
                throw new InvalidOperationException("ConsoleWriter is already initialized.");

            m_impl = new WriterImpl();
            Console.SetOut(m_impl);
            Console.SetError(m_impl);
        }

        public static void Close()
        {
            m_impl.Close();
        }

        static void OnConsoleWrite(string message)
        {
            if (ConsoleWrite != null)
                ConsoleWrite(m_impl, new ConsoleWriteEventArgs(message));
        }

        public static event ConsoleWriteEventHandler ConsoleWrite;
    }
}
