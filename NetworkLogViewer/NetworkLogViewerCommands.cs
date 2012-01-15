using System.Windows.Input;

namespace NetworkLogViewer
{
    /// <summary>
    /// Defines commands to a packet viewer application.
    /// </summary>
    public class NetworkLogViewerCommands
    {
        /// <summary>
        /// Initializes members of the
        /// <see cref="Kamilla.PacketViewer.WPF.PacketViewerCommands"/> container.
        /// </summary>
        static NetworkLogViewerCommands()
        {
            OpenConsole = new RoutedUICommand("Open Console", "Open Console", typeof(NetworkLogViewerCommands));
            CloseFile = new RoutedUICommand("Close File", "Close File", typeof(NetworkLogViewerCommands));
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.Input.RoutedUICommand"/> that opens the console window.
        /// </summary>
        public static RoutedUICommand OpenConsole { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Windows.Input.RoutedUICommand"/> that closes the current file.
        /// </summary>
        public static RoutedUICommand CloseFile { get; private set; }
    }
}
