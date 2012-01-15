using System;
using System.Collections.Generic;
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
        /// Retrieves an object that contains style information. This value can be null.
        /// </summary>
        object Style { get; }

        /// <summary>
        /// Gets the currently viewed <see cref="Kamilla.Network.Logging.NetworkLog"/>. This value can be null.
        /// </summary>
        NetworkLog CurrentLog { get; }

        /// <summary>
        /// Gets the currently used <see cref="Kamilla.Network.Protocols.Protocol"/>. This value can be null.
        /// </summary>
        Protocol CurrentProtocol { get; }

        /// <summary>
        /// Gets the collection of items currently loaded.
        /// </summary>
        IEnumerable<ViewerItem> Items { get; }

        /// <summary>
        /// Occurs when <see href="Kamilla.Network.Viewing.INetworkLogViewer.Style"/> property changes.
        /// </summary>
        event EventHandler StyleChanged;

        /// <summary>
        /// Occurs when the <see href="Kamilla.Network.Viewing.INetworkLogViewer.CurrentProtocol"/>
        /// property changes.
        /// </summary>
        event ProtocolChangedEventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see href="Kamilla.Network.Viewing.INetworkLogViewer.CurrentLog"/>
        /// property changes.
        /// </summary>
        event NetworkLogChangedEventHandler NetworkLogChanged;

        /// <summary>
        /// Occurs when data of a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is queried.
        /// </summary>
        event ViewerItemEventHandler ItemQueried;

        /// <summary>
        /// Occurs when a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is added.
        /// </summary>
        event ViewerItemEventHandler ItemAdded;
    }
}
