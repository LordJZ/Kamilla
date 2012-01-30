using System.Windows.Input;

namespace NetworkLogViewer
{
    /// <summary>
    /// Defines commands to a packet viewer application.
    /// </summary>
    public class NetworkLogViewerCommands
    {
        static RoutedUICommand NewCommand(string name, params InputGesture[] gestures)
        {
            return new RoutedUICommand(name, name, typeof(NetworkLogViewerCommands),
                new InputGestureCollection(gestures));
        }

        /// <summary>
        /// Initializes members of the
        /// <see cref="Kamilla.PacketViewer.WPF.PacketViewerCommands"/> container.
        /// </summary>
        static NetworkLogViewerCommands()
        {
            OpenConsole = NewCommand("Open Console");
            CloseFile = NewCommand("Close File");
            GoToPacketN = NewCommand("Go To Packet", new KeyGesture(Key.G, ModifierKeys.Control));
            NextError = NewCommand("Go To Next Error", new KeyGesture(Key.F8));
            NextUndefinedParser = NewCommand("Go To Next Undefined Parser", new KeyGesture(Key.F9));
            NextUnknownOpcode = NewCommand("Go To Next Unknown Opcode", new KeyGesture(Key.F10));
            PrevError = NewCommand("Go To Prev Error", new KeyGesture(Key.F8, ModifierKeys.Shift));
            PrevUndefinedParser = NewCommand("Go To Prev Undefined Parser", new KeyGesture(Key.F9, ModifierKeys.Shift));
            PrevUnknownOpcode = NewCommand("Go To Prev Unknown Opcode", new KeyGesture(Key.F10, ModifierKeys.Shift));
            Search = NewCommand("Search", new KeyGesture(Key.F, ModifierKeys.Control));
            SearchUp = NewCommand("Search Up", new KeyGesture(Key.F3, ModifierKeys.Shift));
            SearchDown = NewCommand("Search Down", new KeyGesture(Key.F3));
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.Input.RoutedUICommand"/> that opens the console window.
        /// </summary>
        public static RoutedUICommand OpenConsole { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Windows.Input.RoutedUICommand"/> that closes the current file.
        /// </summary>
        public static RoutedUICommand CloseFile { get; private set; }

        public static RoutedUICommand Search { get; private set; }
        public static RoutedUICommand SearchUp { get; private set; }
        public static RoutedUICommand SearchDown { get; private set; }

        public static RoutedUICommand GoToPacketN { get; private set; }
        public static RoutedUICommand NextError { get; private set; }
        public static RoutedUICommand NextUndefinedParser { get; private set; }
        public static RoutedUICommand NextUnknownOpcode { get; private set; }
        public static RoutedUICommand PrevError { get; private set; }
        public static RoutedUICommand PrevUndefinedParser { get; private set; }
        public static RoutedUICommand PrevUnknownOpcode { get; private set; }
    }
}
