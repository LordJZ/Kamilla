using System;
using System.Collections.Generic;
using Kamilla.Network.Logging;
using Kamilla.Network.Protocols;
using Kamilla.Network.Viewing.Plugins;

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
        /// <c>item</c> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <c>item</c> is invalid.
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
        /// Flags the specified <see cref="Kamilla.Network.Viewing.ViewerItem"/> for parsing.
        /// </summary>
        /// <param name="item">
        /// The <see cref="Kamilla.Network.Viewing.ViewerItem"/> that should be flagged for parsing.
        /// </param>
        public abstract void EnqueueParsing(ViewerItem item);

        /// <summary>
        /// Registers a <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/>.
        /// </summary>
        /// <param name="command">
        /// The <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> that should be registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <c>command</c> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The provided <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> is already
        /// registered with the current <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
        /// </exception>
        public abstract void RegisterPluginCommand(PluginCommand command);

        /// <summary>
        /// Unregisters a <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/>.
        /// </summary>
        /// <param name="command">
        /// The <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> that should be unregistered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <c>command</c> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The provided <see cref="Kamilla.Network.Viewing.Plugins.PluginCommand"/> is not
        /// registered with the current <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase"/>.
        /// </exception>
        public abstract void UnregisterPluginCommand(PluginCommand command);

        #region Events
        /// <summary>
        /// Occurs when the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase.CurrentProtocol"/>
        /// property changes.
        /// 
        /// Handlers of this event should be called from any suiting thread.
        /// </summary>
        public abstract event EventHandler ProtocolChanged;

        /// <summary>
        /// Occurs when the <see cref="Kamilla.Network.Viewing.NetworkLogViewerBase.CurrentLog"/>
        /// property changes.
        /// 
        /// Handlers of this event should be called from any suiting thread.
        /// </summary>
        public abstract event EventHandler NetworkLogChanged;

        /// <summary>
        /// Occurs when data of a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is queried.
        /// 
        /// Handlers of this event MUST be called from the UI thread if one exists.
        /// </summary>
        public abstract event ViewerItemEventHandler ItemQueried;

        /// <summary>
        /// Occurs when a <see cref="Kamilla.Network.Viewing.ViewerItem"/> is added.
        /// 
        /// Handlers of this event should be called from any suiting thread.
        /// </summary>
        public abstract event ViewerItemEventHandler ItemAdded;

        /// <summary>
        /// Occurs when interpreting of contents of a
        /// <see cref="Kamilla.Network.Viewing.ViewerItem"/> is finished.
        /// 
        /// Handlers of this event should be called from any suiting thread.
        /// </summary>
        public event ViewerItemEventHandler ItemParsingDone;
        #endregion

        #region Properties
        /// <summary>
        /// Retrieves the object that is responsible for the user interface. This value can be null.
        /// </summary>
        public abstract object InterfaceObject { get; }

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
        public abstract IList<ViewerItem> Items { get; }

        /// <summary>
        /// Gets the handle of the viewer window. This value can be <see cref="System.IntPtr.Zero"/>.
        /// </summary>
        public abstract IntPtr WindowHandle { get; }

        /// <summary>
        /// Gets the <see cref="Kamilla.Network.Viewing.ViewerItem"/> that is currently selected.
        /// This value can be null.
        /// </summary>
        public abstract ViewerItem SelectedItem { get; }
        #endregion
    }
}
