using System;
using Kamilla.Network.Logging;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Handles <see href="Kamilla.Network.Viewing.INetworkLogViewer.NetworkLogChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The current <see cref="Kamilla.Network.Viewing.INetworkLogViewer"/>.
    /// </param>
    /// <param name="e">
    /// An instance of <see cref="Kamilla.Network.Viewing.NetworkLogChangedEventArgs"/> class
    /// that contains the event data.
    /// </param>
    public delegate void NetworkLogChangedEventHandler(object sender, NetworkLogChangedEventArgs e);

    /// <summary>
    /// Contains data related to
    /// <see href="Kamilla.Network.Viewing.INetworkLogViewer.NetworkLogChanged"/> event.
    /// </summary>
    public sealed class NetworkLogChangedEventArgs : EventArgs
    {
        NetworkLog m_old;
        NetworkLog m_new;

        /// <summary>
        /// Initializes a new instance of
        /// <see cref="Kamilla.Network.Viewing.NetworkLogChangedEventArgs"/> with the specified
        /// new and old instances of <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        /// <param name="oldLog">
        /// The old instance of <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </param>
        /// <param name="newLog">
        /// The new instance of <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </param>
        public NetworkLogChangedEventArgs(NetworkLog oldLog, NetworkLog newLog)
        {
            m_old = oldLog;
            m_new = newLog;
        }

        /// <summary>
        /// Gets the old instance of <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public NetworkLog OldLog { get { return m_old; } }

        /// <summary>
        /// Gets the new instance of <see cref="Kamilla.Network.Logging.NetworkLog"/>.
        /// </summary>
        public NetworkLog NewLog { get { return m_new; } }
    }
}
