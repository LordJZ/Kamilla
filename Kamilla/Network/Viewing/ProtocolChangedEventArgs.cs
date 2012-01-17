using System;
using Kamilla.Network.Protocols;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Handles <see href="Kamilla.Network.Viewing.INetworkLogViewer.ProtocolChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The current <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
    /// </param>
    /// <param name="e">
    /// An instance of <see cref="Kamilla.Network.Viewing.ProtocolChangedEventArgs"/> class
    /// that contains the event data.
    /// </param>
    public delegate void ProtocolChangedEventHandler(object sender, ProtocolChangedEventArgs e);

    /// <summary>
    /// Contains data related to
    /// <see href="Kamilla.Network.Viewing.INetworkLogViewer.ProtocolChanged"/> event.
    /// </summary>
    public sealed class ProtocolChangedEventArgs : EventArgs
    {
        Protocol m_old;
        Protocol m_new;

        /// <summary>
        /// Initializes a new instance of
        /// <see cref="Kamilla.Network.Viewing.ProtocolChangedEventArgs"/> with the specified
        /// new and old instances of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        /// <param name="oldProtocol">
        /// The old instance of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </param>
        /// <param name="newProtocol">
        /// The new instance of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </param>
        public ProtocolChangedEventArgs(Protocol oldProtocol, Protocol newProtocol)
        {
            m_old = oldProtocol;
            m_new = newProtocol;
        }

        /// <summary>
        /// Gets the old instance of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public Protocol OldProtocol { get { return m_old; } }

        /// <summary>
        /// Gets the new instance of <see cref="Kamilla.Network.Protocols.Protocol"/>.
        /// </summary>
        public Protocol NewProtocol { get { return m_new; } }
    }
}
