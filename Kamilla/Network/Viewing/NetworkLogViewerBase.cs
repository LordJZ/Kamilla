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
        /// <summary>
        /// Fires the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase.ItemParsingDone"/> event
        /// with the specified <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// </summary>
        /// <param name="item">
        /// The <see cref="Kamilla.Network.Viewing.ViewerItem"/> whose parsing has finished.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// item is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// item is invalid.
        /// </exception>
        protected internal virtual void OnParsingDone(ViewerItem item)
        {
            if (item == null)
                throw new ArgumentNullException();

            if (item.Viewer != this)
                throw new ArgumentException();

            if (this.ItemParsingDone != null)
                this.ItemParsingDone(this, new ViewerItemEventArgs(item));
        }

        /// <summary>
        /// Repaints the specified <see cref="Kamilla.Network.Viewing.ViewerItem"/>.
        /// </summary>
        /// <param name="item">
        /// The <see cref="Kamilla.Network.Viewing.ViewerItem"/> that should be repainted.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// item is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The specified <see cref="Kamilla.Network.Viewing.ViewerItem"/> is invalid.
        /// </exception>
        public abstract void UpdateItem(ViewerItem item);

        #region Events
        /// <summary>
        /// Occurs when <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase.Style"/> property changes.
        /// </summary>
        public abstract event EventHandler StyleChanged;

        /// <summary>
        /// Occurs when the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase.CurrentProtocol"/>
        /// property changes.
        /// </summary>
        public abstract event ProtocolChangedEventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase.CurrentLog"/>
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
        public event ViewerItemEventHandler ItemParsingDone;
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
        /// Gets the handle of the viewer window. This value can be <see cref="System.IntPtr.Zero"/>.
        /// </summary>
        public abstract IntPtr WindowHandle { get; }
        #endregion
    }
}
