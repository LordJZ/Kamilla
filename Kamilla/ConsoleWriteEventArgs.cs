using System;

namespace Kamilla
{
    public delegate void ConsoleWriteEventHandler(object sender, ConsoleWriteEventArgs args);

    public sealed class ConsoleWriteEventArgs : EventArgs
    {
        public ConsoleWriteEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}
