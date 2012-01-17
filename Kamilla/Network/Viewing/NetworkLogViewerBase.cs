using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kamilla.Network.Logging;
using Kamilla.Network.Protocols;
using Kamilla.Network.Parsing;

namespace Kamilla.Network.Viewing
{
    /// <summary>
    /// Serves as a base for <see cref="Kamilla.Network.Logging.NetworkLog"/> viewers.
    /// </summary>
    public abstract class NetworkLogViewerBase
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected internal abstract void InternalNotifyParsingDone(PacketParser parser);

        #region Events
        /// <summary>
        /// Occurs when <see href="Kamilla.Network.Viewing.INetworkLogViewer.Style"/> property changes.
        /// </summary>
        public abstract event EventHandler StyleChanged;

        /// <summary>
        /// Occurs when the <see href="Kamilla.Network.Viewing.INetworkLogViewer.CurrentProtocol"/>
        /// property changes.
        /// </summary>
        public abstract event ProtocolChangedEventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see href="Kamilla.Network.Viewing.INetworkLogViewer.CurrentLog"/>
        /// property changes.
        /// </summary>
        public abstract event NetworkLogChangedEventHandler NetworkLogChanged;

        /// <summary>
        /// Occurs when data of a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is queried.
        /// </summary>
        public abstract event ViewerItemEventHandler ItemQueried;

        /// <summary>
        /// Occurs when a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is added.
        /// </summary>
        public abstract event ViewerItemEventHandler ItemAdded;

        /// <summary>
        /// Occurs when interpreting of contents of a
        /// <see cref="Kamilla.Network.Viewing.ViewerItem"/> is finished.
        /// </summary>
        public abstract event ViewerItemEventHandler ItemParsingDone;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the currently viewed <see cref="Kamilla.Network.Logging.NetworkLog"/>. This value can be null.
        /// </summary>
        public abstract NetworkLog CurrentLog { get; }

        /// <summary>
        /// Gets the currently used <see cref="Kamilla.Network.Protocols.Protocol"/>. This value can be null.
        /// </summary>
        public abstract Protocol CurrentProtocol { get; }

        /// <summary>
        /// Gets the collection of items currently loaded.
        /// </summary>
        public abstract IEnumerable<ViewerItem> Items { get; }

        /// <summary>
        /// Retrieves an object that contains style information. This value can be null.
        /// </summary>
        public abstract object Style { get; }

        /// <summary>
        /// Gets the handle of the viewer window. This value can be <see href="System.IntPtr.Zero"/>.
        /// </summary>
        public abstract IntPtr WindowHandle { get; }
        #endregion
    }
}
