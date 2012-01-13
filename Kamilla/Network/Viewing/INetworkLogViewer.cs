using System;
using Kamilla.Network.Logging;
using Kamilla.Network.Protocols;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Represents an abstract <see cref="Kamilla.Network.Logging.NetworkLog"/> viewer.
    /// </summary>
    public interface INetworkLogViewer
    {
        /// <summary>
        /// Gets the handle of the viewer window. This value can be <see href="System.IntPtr.Zero"/>.
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Gets the currently viewed <see cref="Kamilla.Network.Logging.NetworkLog"/>. This value can be null.
        /// </summary>
        NetworkLog CurrentLog { get; }

        /// <summary>
        /// Gets the currently used <see cref="Kamilla.Network.Protocols.Protocol"/>. This value can be null.
        /// </summary>
        Protocol CurrentProtocol { get; }

        /// <summary>
        /// Occurs when the <see href="Kamilla.Network.Viewing.CurrentProtocol"/> property changes.
        /// </summary>
        event ProtocolChangedEventHandler ProtocolChanged;
    }
}
