using System;
using System.Windows.Input;

namespace Kamilla.Network.Viewing.Plugins
{
    /// <summary>
    /// Represents a command to a
    /// <see cref="Kamilla.Network.Viewing.Plugins.INetworkLogViewerPlugin"/>.
    /// </summary>
    public class PluginCommand
    {
        /// <summary>
        /// Initializes a new instance of
        /// <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> class
        /// with the specified command information.
        /// </summary>
        /// <param name="plugin">
        /// The plugin to which the current command belongs.
        /// </param>
        /// <param name="title">
        /// The localized title string of the current command.
        /// </param>
        /// <param name="gesture">
        /// The key gesture that can be used to call the current command. This value can be null.
        /// </param>
        /// <param name="callback">
        /// The delegate that is called when the command is executed.
        /// </param>
        public PluginCommand(INetworkLogViewerPlugin plugin, string title, KeyGesture gesture, Action callback)
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");

            if (title == null)
                throw new ArgumentNullException("title");

            if (callback == null)
                throw new ArgumentNullException("callback");

            title = title.Trim();
            if (title == string.Empty)
                throw new ArgumentException("title must not be empty.", "title");

            this.Plugin = plugin;
            this.Title = title;
            this.Gesture = gesture;
            this.Callback = callback;
        }

        /// <summary>
        /// Gets the plugin to which the current command belongs.
        /// </summary>
        public INetworkLogViewerPlugin Plugin { get; private set; }

        /// <summary>
        /// Gets the localized title string of the current command.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the key gesture that can be used to call the current command. This value can be null. 
        /// </summary>
        public KeyGesture Gesture { get; private set; }

        /// <summary>
        /// Gets the delegate that is called when the command is executed.
        /// </summary>
        public Action Callback { get; private set; }
    }
}
